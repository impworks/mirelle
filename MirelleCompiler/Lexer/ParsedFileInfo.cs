using System;
using System.Collections.Generic;
using System.Text;

using Mirelle.Lexer;

namespace Mirelle.Lexer
{
  public class ParsedFileInfo
  {
    /// <summary>
    /// File name
    /// </summary>
    public string Name;

    /// <summary>
    /// List of lexems
    /// </summary>
    public List<Lexem> Lexems;

    /// <summary>
    /// Current lexem
    /// </summary>
    public int LexemId = 0;

    public ParsedFileInfo(string name)
    {
      Name = name;
      Lexems = new List<Lexem>();
    }

    public Lexem GetCurrentLexem()
    {
      return Lexems[LexemId];
    }
  }
}
