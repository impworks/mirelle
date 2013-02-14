using System;
using System.Collections.Generic;
using System.Text;

namespace Mirelle.Lexer
{
  public class Lexem
  {
    /// <summary>
    /// Type of the lexem
    /// </summary>
    public LexemType Type;

    /// <summary>
    /// Lexem length in source code
    /// </summary>
    public int Length;

    /// <summary>
    /// The line at which the lexem resides in the file
    /// </summary>
    public int Line;

    /// <summary>
    /// The offset at which the lexem resided in the file
    /// </summary>
    public int TotalOffset;

    /// <summary>
    /// The offset at which the lexem resides in the line
    /// </summary>
    public int Offset;

    /// <summary>
    /// For dynamic lexems, the data it stores (e.g. identifier name)
    /// </summary>
    public string Data = "";

    /// <summary>
    /// The file the lexem was found in
    /// </summary>
    public string File;

    public Lexem(LexemType type, string data = "")
    {
      Type = type;
      Data = data;
    }

    public Lexem(LexemType type, int line, int offset, int total, string file, string data = "")
    {
      Type = type;
      Line = line;
      Offset = offset;
      TotalOffset = total;
      File = file;
      Data = data;
    }
  }
}
