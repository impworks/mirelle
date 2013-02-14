using System;
using System.Collections.Generic;
using System.Text;

using Mirelle.Lexer;

namespace Mirelle
{
  [Serializable]
  public class CompilerException : System.Exception
  {
    public int Line {get; set;}
    public int Offset { get; set; }
    public int Position { get; set; }
    public string File { get; set; }
    public int Length { get; set; }
    public bool Affixed = false;

    /// <summary>
    /// Create a source-based exception
    /// </summary>
    /// <param name="msg">Message</param>
    /// <param name="line">Exception line</param>
    /// <param name="offset">Exception offset</param>
    /// <param name="file">Exception file</param>
    /// <param name="length">Length of the erroneous fragment</param>
    public CompilerException(string msg, int line = 0, int offset = 0, int position = 0, string file = "", int length = 1):base(msg)
    {
      Line = line;
      Offset = offset;
      Position = position;
      File = file;
      Length = length;

      if (file != "")
        Affixed = true;
    }

    /// <summary>
    /// Create a meta exception not based on source
    /// </summary>
    /// <param name="msg">Message</param>
    /// <param name="affix">Affixed</param>
    public CompilerException(string msg, bool affix):base(msg)
    {
      Affixed = true;
    }

    /// <summary>
    /// Create an exception based on a lexem
    /// </summary>
    /// <param name="msg">Message</param>
    /// <param name="lexem">Erroneous lexem</param>
    public CompilerException(string msg, Lexem lexem):base(msg)
    {
      if(lexem != null)
        AffixToLexem(lexem);
    }

    /// <summary>
    /// Set exception properties based on a lexem
    /// </summary>
    /// <param name="lexem">Lexem to affix to</param>
    public void AffixToLexem(Lexem lexem)
    {
      // already affixed
      if (Affixed) return;

      Line = lexem.Line;
      Offset = lexem.Offset;
      Position = lexem.TotalOffset;
      File = lexem.File;
      Length = lexem.Length;

      Affixed = true;
    }

    /// <summary>
    /// Return a nice formatted message
    /// </summary>
    /// <returns></returns>
    public string FormattedMessage()
    {
      return String.Format("Error: {0}\nFile: {1}, position: {2}:{3}", Message, File, Line, Offset);
    }
  }
}
