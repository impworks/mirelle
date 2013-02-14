using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Mirelle.Lexer
{
  public class Lexer
  {
    /// <summary>
    /// Parse the file into a list of lexems
    /// </summary>
    /// <param name="src">Source as string</param>
    /// <param name="file">File name to mark lexems</param>
    /// <returns></returns>
    public List<Lexem> Parse(string src, string file)
    {
      var lexems = new List<Lexem>();
      int position = 0;
      int line = 1;
      int offset = 1;

      while (position < src.Length)
      {
        // skip spaces and tabs
        while (position < src.Length && (src[position] == ' ' || src[position] == '\t')) { position++; offset++; }

        // skip single line comments
        if (src.SafeSubstring(position, 2) == "//")
        {
          position += 2; offset += 2;
          while (position < src.Length && src[position] != '\n') { position++; offset++; }
        }

        // skip multiline comments
        if(src.SafeSubstring(position, 2) == "/*")
        {
          position += 2; offset += 2;
          while (position < src.Length && src.SafeSubstring(position, 2) != "*/") { position++; offset++; }
          position += 2; offset += 2;
        }

        if(position >= src.Length) break;

        // extract static lexem
        var lexem = ExtractStaticLexem(src, position) ?? ExtractDynamicLexem(src, position);

        if (lexem == null)
          throw new CompilerException(Resources.errUnknownLexem, line, offset, position, file, 1);

        // affix lexem to current position and save it
        lexem.Line = line;
        lexem.Offset = offset;
        lexem.File = file;
        lexem.TotalOffset = position;
        lexems.Add(lexem);

        // lexem validation
        if (DetectOverflow(lexem.Data, lexem.Type))
          throw new CompilerException(Resources.errConstantOverflow, lexem);

        // update position
        position += lexem.Length;
        if (lexem.Type == LexemType.NewLine)
        {
          line++;
          offset = 1;
        }
        else
          offset += lexem.Length;
      }

      lexems.Add(new Lexem(LexemType.EOF, line, offset, position, file));

      return lexems;
    }

    /// <summary>
    /// Try to find a static lexem at current position
    /// </summary>
    /// <param name="str">String</param>
    /// <param name="offset">Offset inside string</param>
    /// <returns></returns>
    private Lexem ExtractStaticLexem(string str, int offset)
    {
      foreach (var curr in StaticLexems)
      {
        // looks like we got a complete match
        if (str.SafeSubstring(offset, curr.Signature.Length) == curr.Signature)
        {
          // if this was only a part of the identifier - false alarm
          if (curr.IsIdentifier && IsPartOfIdentifier(str, offset + curr.Signature.Length))
            continue;

          var lexem = new Lexem(curr.Type);
          lexem.Length = curr.Signature.Length;
          return lexem;
        }
      }

      return null;
    }

    /// <summary>
    /// Try to find a dynamic lexem at current position
    /// </summary>
    /// <param name="str">String</param>
    /// <param name="offset">Offset inside string</param>
    /// <returns></returns>
    private Lexem ExtractDynamicLexem(string str, int offset)
    {
      var piece = str.Substring(offset, str.Length - offset);
      foreach (var curr in DynamicLexems)
      {
        // perform a regex match
        var match = curr.Signature.Match(piece);
        if (match.Success)
        {
          var lexem = new Lexem(curr.Type, match.Value);

          // special case of string regexp
          if (curr.Type == LexemType.StringLiteral)
            lexem.Data = lexem.Data.SafeSubstring(1, lexem.Data.Length - 2);
          lexem.Length = match.Value.Length;

          return lexem;
        }
      }

      return null;
    }

    /// <summary>
    /// Check if the symbol is possibly a part of an identifier
    /// </summary>
    /// <param name="str">String to search in</param>
    /// <param name="pos">Offset inside string</param>
    /// <returns></returns>
    private bool IsPartOfIdentifier(string str, int pos)
    {
      // check if the first char of the remaining string matches [a-zA-Z0-9_']
      if (pos >= str.Length) return false;

      char item = str[pos];
      if((item >= 'a' && item <= 'z') || (item >= 'A' && item <= 'Z') || (item >= '0' && item <= '9'))
        return true;

      if (item == '_' || item == '\'')
        return true;

      return false;
    }

    /// <summary>
    /// Parse an int or return 0
    /// </summary>
    /// <param name="str">String representation</param>
    /// <returns></returns>
    public int RetrieveInt(string str, bool safe = true)
    {
      try
      {
        // hexademical
        if (str.SafeSubstring(0, 2) == "0x")
          return Convert.ToInt32(str.Substring(2), 16);
        else if (str.SafeSubstring(0, 2) == "0b")
          return Convert.ToInt32(str.Substring(2), 2);
        else
          return int.Parse(str, NumberStyles.AllowExponent);
      }
      catch
      {
        if(!safe) throw;
        return 0;
      }
    }

    /// <summary>
    /// Parse a float or return 0.0
    /// </summary>
    /// <param name="str">String representation</param>
    /// <returns></returns>
    public double RetrieveFloat(string str, bool safe = true)
    {
      try
      {
        var result = double.Parse(str);
        return result;
      }
      catch
      {
        if (!safe) throw;
        return 0;
      }
    }

    /// <summary>
    /// Return the imaginary part of a complex number
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public double RetrieveComplex(string str, bool safe = true)
    {
      try
      {
        // trim off the "i" or "j" suffix
        return double.Parse(str.Substring(0, str.Length - 1));
      }
      catch
      {
        if(!safe) throw;
        return 0;
      }
    }

    /// <summary>
    /// Detect constant value overflow in lexems
    /// </summary>
    /// <param name="value">Constant value</param>
    /// <param name="type">Constant type</param>
    /// <returns></returns>
    public bool DetectOverflow(string value, LexemType type)
    {
      try
      {
        if(type == LexemType.IntLiteral)
          RetrieveInt(value, false);

        if (type == LexemType.FloatLiteral)
          RetrieveFloat(value, false);

        if (type == LexemType.ComplexLiteral)
          RetrieveComplex(value, false);

        return false;
      }
      catch
      {
        return true;
      }
    }

    /// <summary>
    /// Lexem list
    /// </summary>
    private static StaticLexemDefinition[] StaticLexems = new[]
      {
        new StaticLexemDefinition("type", LexemType.Type, true),
        new StaticLexemDefinition("var", LexemType.Var, true),
        new StaticLexemDefinition("if", LexemType.If, true),
        new StaticLexemDefinition("else", LexemType.Else, true),
        new StaticLexemDefinition("for", LexemType.For, true),
        new StaticLexemDefinition("while", LexemType.While, true),
        new StaticLexemDefinition("in", LexemType.In, true),
        new StaticLexemDefinition("do", LexemType.Do, true),
        new StaticLexemDefinition("return", LexemType.Return, true),
        new StaticLexemDefinition("include", LexemType.Include, true),
        new StaticLexemDefinition("autoconstruct", LexemType.Autoconstruct, true),
        new StaticLexemDefinition("break", LexemType.Break, true),
        new StaticLexemDefinition("redo", LexemType.Redo, true),
        new StaticLexemDefinition("emit", LexemType.Emit, true),
        new StaticLexemDefinition("void", LexemType.Void, true),
        new StaticLexemDefinition("static", LexemType.Static, true),
        new StaticLexemDefinition("new", LexemType.New, true),
        new StaticLexemDefinition("true", LexemType.TrueLiteral, true),
        new StaticLexemDefinition("false", LexemType.FalseLiteral, true),
        new StaticLexemDefinition("use", LexemType.Use, true),
        new StaticLexemDefinition("println", LexemType.PrintLine, true),
        new StaticLexemDefinition("print", LexemType.Print, true),
        new StaticLexemDefinition("as", LexemType.As, true),
        new StaticLexemDefinition("null", LexemType.Null, true),
        new StaticLexemDefinition("exit", LexemType.Exit, true),
        new StaticLexemDefinition("with", LexemType.With, true),
        new StaticLexemDefinition("every", LexemType.Every, true),
        new StaticLexemDefinition("limit", LexemType.Limit, true),
        new StaticLexemDefinition("until", LexemType.Until, true),
        new StaticLexemDefinition("enum", LexemType.Enum, true),
        new StaticLexemDefinition("simulate", LexemType.Simulate, true),
        new StaticLexemDefinition("once", LexemType.Once, true),

        new StaticLexemDefinition("==", LexemType.Equal),
        new StaticLexemDefinition("=>", LexemType.Arrow),
        new StaticLexemDefinition("=", LexemType.Assign),

        new StaticLexemDefinition("!=", LexemType.NotEqual),
        new StaticLexemDefinition("!", LexemType.Not),

        new StaticLexemDefinition(">=", LexemType.GreaterEqual),
        new StaticLexemDefinition(">>=", LexemType.AssignShiftRight),
        new StaticLexemDefinition(">>", LexemType.BinaryShiftRight),
        new StaticLexemDefinition(">", LexemType.Greater),

        new StaticLexemDefinition("<=>", LexemType.Exchange),
        new StaticLexemDefinition("<<=", LexemType.AssignShiftLeft),
        new StaticLexemDefinition("<=", LexemType.LessEqual),
        new StaticLexemDefinition("<<", LexemType.BinaryShiftLeft),
        new StaticLexemDefinition("<", LexemType.Less),

        new StaticLexemDefinition("++", LexemType.Inc),
        new StaticLexemDefinition("+=", LexemType.AssignAdd),
        new StaticLexemDefinition("+", LexemType.Add),

        new StaticLexemDefinition("--", LexemType.Dec),
        new StaticLexemDefinition("-=", LexemType.AssignSubtract),
        new StaticLexemDefinition("->", LexemType.Arrow),
        new StaticLexemDefinition("-", LexemType.Subtract),

        new StaticLexemDefinition("**=", LexemType.AssignPower),
        new StaticLexemDefinition("**", LexemType.Power),
        new StaticLexemDefinition("*=", LexemType.AssignMultiply),
        new StaticLexemDefinition("*", LexemType.Multiply),

        new StaticLexemDefinition("/=", LexemType.AssignDivide),
        new StaticLexemDefinition("/", LexemType.Divide),

        new StaticLexemDefinition("%=", LexemType.AssignRemainder),
        new StaticLexemDefinition("%", LexemType.Remainder),

        new StaticLexemDefinition("&&", LexemType.And),
        new StaticLexemDefinition("&", LexemType.BinaryAnd),

        new StaticLexemDefinition("||", LexemType.Or),
        new StaticLexemDefinition("|", LexemType.BinaryOr),

        new StaticLexemDefinition("^", LexemType.BinaryXor),

        new StaticLexemDefinition("[[", LexemType.DoubleSquareOpen),
        new StaticLexemDefinition("[", LexemType.SquareOpen),

        new StaticLexemDefinition("]]", LexemType.DoubleSquareClose),
        new StaticLexemDefinition("]", LexemType.SquareClose),

        new StaticLexemDefinition("..", LexemType.DoubleDot),
        new StaticLexemDefinition(".", LexemType.Dot),

        new StaticLexemDefinition("~", LexemType.Tilde),
        new StaticLexemDefinition(":", LexemType.Colon),
        new StaticLexemDefinition(";", LexemType.Semicolon),
        new StaticLexemDefinition(",", LexemType.Comma),
        new StaticLexemDefinition("\r\n", LexemType.NewLine),
        new StaticLexemDefinition("\n", LexemType.NewLine),
        new StaticLexemDefinition("(", LexemType.ParenOpen),
        new StaticLexemDefinition(")", LexemType.ParenClose),
        new StaticLexemDefinition("{", LexemType.CurlyOpen),
        new StaticLexemDefinition("}", LexemType.CurlyClose),
      };

    private static DynamicLexemDefinition[] DynamicLexems = new[]
      {
        new DynamicLexemDefinition("^[@a-zA-Z_'][a-zA-Z0-9_']*", LexemType.Identifier),

        new DynamicLexemDefinition("^[0-9]+(\\.[0-9]+)?[ij]", LexemType.ComplexLiteral),

        new DynamicLexemDefinition("^[0-9]+\\.[0-9]+", LexemType.FloatLiteral),

        new DynamicLexemDefinition("^0x[0-9a-fA-F]+", LexemType.IntLiteral),
        new DynamicLexemDefinition("^0b[01]+", LexemType.IntLiteral),
        new DynamicLexemDefinition("^[0-9]+", LexemType.IntLiteral),

        new DynamicLexemDefinition("^\"[^\"]*\"", LexemType.StringLiteral)
      };
  }
}

