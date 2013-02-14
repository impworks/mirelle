using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mirelle.Lexer;

namespace Mirelle.Parser
{
  public partial class Parser
  {
    private void SkipLexem(bool strict = false)
    {
      GetLexem(0, strict);

      var currFile = Compiler.CurrentFiles.Peek();

      if(!strict)
      {
        while (currFile.Lexems[currFile.LexemId].Type == LexemType.NewLine)
          currFile.LexemId++;
      }

      currFile.LexemId++;
    }

    /**
     * Look ahead for branching
     */
    private bool PeekLexem(int offset, params LexemType[] types)
    {
      // use 'strict' mode if the user requested a newline
      Lexem lexem = GetLexem(offset, Array.Exists(types, item => item == LexemType.NewLine));

      // check if the lexem is any of the given list
      LexemType type = lexem.Type;
      foreach (var curr in types)
        if (type == curr)
          return true;

      return false;
    }

    /**
     * Look ahead for branching: shorthand
     */
    private bool PeekLexem(params LexemType[] types)
    {
      return PeekLexem(0, types);
    }

    /**
     * Get a lexem
     */
    private Lexem GetLexem(int offset = 0, bool strict = false)
    {
      var currFile = Compiler.CurrentFiles.Peek();
      var lexemList = currFile.Lexems;
      var position = currFile.LexemId;

      // strict mode: use newlines are accounted for
      if (strict)
      {
        if (lexemList.Count < currFile.LexemId + offset) return new Lexem(LexemType.EOF);
        return lexemList[currFile.LexemId + offset];
      }

      // ----------------------------------------------
      // loose mode: account only for meaningful lexems
      // ----------------------------------------------

      // make sure out-of-bounds index doesn't fail everything
      // just return an EOF in this case and it will throw a compiler error up there
      try
      {
        // affix to first meaningful lexem found
        while (lexemList[position].Type == LexemType.NewLine) position++;

        // detect direction: forward or backwards
        int incr = offset > 0 ? 1 : -1;
        while (offset != 0)
        {
          // shift position one step
          position += incr;
          // offset counts backwards, therefore -=
          offset -= incr;
          // skip additional newlines after
          while (lexemList[position].Type == LexemType.NewLine) position += incr;
        }

        return lexemList[position];
      }
      catch
      {
        return new Lexem(LexemType.EOF);
      }
    }

    /// <summary>
    /// Return the current lexem id
    /// </summary>
    /// <returns></returns>
    public int GetLexemId()
    {
      return Compiler.CurrentFiles.Peek().LexemId;
    }

    /// <summary>
    /// Set the current lexem id
    /// </summary>
    /// <param name="id">New lexem id</param>
    public void SetLexemId(int id)
    {
      Compiler.CurrentFiles.Peek().LexemId = id;
    }

    /// <summary>
    /// Throw an exception
    /// </summary>
    /// <param name="msg">Message</param>
    private void Error(string msg)
    {
      var lexem = GetLexem();
      throw new CompilerException(msg, GetLexem());
    }

    /// <summary>
    /// Throw an exception
    /// </summary>
    /// <param name="msg">Message</param>
    /// <param name="lexem">Erroneous lexem</param>
    private void Error(string msg, Lexem lexem)
    {
      throw new CompilerException(msg, lexem);
    }

    /// <summary>
    /// List of special methods and their desired return types
    /// </summary>
    private Dictionary<string, string> ReservedMethods = new Dictionary<string, string>
    {
      {"construct", "void"},
      {"to_b", "bool"},
      {"to_i", "int"},
      {"to_f", "float"},
      {"to_s", "string"},
      {"equal", "bool"}
    };
  }
}
