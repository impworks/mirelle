using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;

using Mirelle.Lexer;
using Mirelle.Parser;
using Mirelle.Emitter;
using Mirelle.SyntaxTree;

namespace Mirelle
{
  public class Compiler
  {
    public Lexer.Lexer Lexer;
    public Parser.Parser Parser;
    public Emitter.Emitter Emitter;

    public List<string> ProcessedFiles = new List<string>();
    public Stack<ParsedFileInfo> CurrentFiles = new Stack<ParsedFileInfo>();

    /// <summary>
    /// Initialize the compiler
    /// </summary>
    private void Init()
    {
      Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

      Lexer = new Lexer.Lexer();
      Parser = new Parser.Parser(this);
      Emitter = new Emitter.Emitter(this);

      Emitter.RegisterStdLib();
      Emitter.EmitInitialize();

      // reset specific class-bound properties
      SimulatePlannerNode.Exists = false;
      EmitNode.EmitterCount = 0;
    }

    /// <summary>
    /// Process a file
    /// </summary>
    /// <param name="file">File name</param>
    /// <param name="options">Compilation options</param>
    public void Process(string file, CompilerOptions options = CompilerOptions.None)
    {
      // clear compilation info if compilation
      if (options != CompilerOptions.ContinueCompilation)
        Init();

      ParseFile(file);
      Emitter.Prepare();
      Emitter.RootNode.Compile(Emitter);
    }

    /// <summary>
    /// Save assembly to disk
    /// </summary>
    /// <param name="file">file name to save assembly as</param>
    public void SaveAssembly(string file)
    {
       Emitter.SaveAssembly(file);
    }

    /// <summary>
    /// Compile a file
    /// </summary>
    /// <param name="file">File path</param>
    public void ParseFile(string file)
    {
      // check if file actually exists
      var fi = new FileInfo(file);
      if (!fi.Exists)
        throw new CompilerException(String.Format(Resources.errFileNotFound, file), 0, 0, 0, file);

      // register file in both current and total file lists
      ProcessedFiles.Add(fi.FullName);
      CurrentFiles.Push(new ParsedFileInfo(file));

      // parse file into a list of lexems
      using (StreamReader sr = File.OpenText(file))
        CurrentFiles.Peek().Lexems = Lexer.Parse(sr.ReadToEnd(), file);

      try
      {
        // invoke compilation root procedure
        Parser.ParseMain();
      }
      catch (CompilerException ex)
      {
        // make sure the exception is affixed to lexem
        ex.AffixToLexem(CurrentFiles.Peek().GetCurrentLexem());
        throw;
      }

      // remove file info from stack
      CurrentFiles.Pop();
    }

    /// <summary>
    /// Throw an exception based on a lexem
    /// </summary>
    /// <param name="msg">Error message</param>
    /// <param name="lexem">Erroneous lexem</param>
    private void Error(string msg, Lexem lexem)
    {
      throw new CompilerException(msg, lexem);
    }
  }
}
