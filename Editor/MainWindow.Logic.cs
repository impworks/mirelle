using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using AvalonDock;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace Editor
{
  public delegate void TextDelegate(string str);

  public partial class MainWindow : Window
  {
    /// <summary>
    /// The dictionary of open documents
    /// </summary>
    private Dictionary<DocumentContent, DocumentInfo> Documents = new Dictionary<DocumentContent, DocumentInfo>();

    /// <summary>
    /// Maximum number for untitled documents
    /// </summary>
    private int UntitledCount = 1;

    /// <summary>
    /// Executable thread
    /// </summary>
    private Thread RunThread;

    /// <summary>
    /// Executable file
    /// </summary>
    private string RunFile = "";

    /// <summary>
    /// Interface disabler flag (for Run mode)
    /// </summary>
    private bool DisableInterface = false;

    /// <summary>
    /// Creates a new tab for the editor
    /// </summary>
    /// <param name="file">File path (possibly empty)</param>
    private void CreateEditorTab(string file = "")
    {
      // create new syntax highlight box
      var info = new DocumentInfo();
      var shbox = new SyntaxHighlightBox.SyntaxHighlightBox();
      shbox.CurrentHighlighter = SyntaxHighlightBox.HighlighterManager.Instance.Highlighters["Mirelle"];

      // create a new tab
      var newdoc = new DocumentContent();
      newdoc.Content = shbox;

      if (file == "")
      {
        // mark tab as 'untitled'
        newdoc.Title = Editor.Resources.Untitled + " " + UntitledCount.ToString();
        UntitledCount++;
      }
      else
      {
        // load file into tab
        try
        {
          var fi = new FileInfo(file);
          info.FullPath = file;
          newdoc.Title = info.Name = fi.Name;
          var sr = fi.OpenText();
          shbox.Text = sr.ReadToEnd().Replace("\t", "  ");
          shbox.Redraw();
          sr.Close();
        }
        catch
        {
          Error(String.Format(Editor.Resources.FileNotFound, file));
          return;
        }
      }

      newdoc.Closing += (s, e) =>
      {
        // check for interface being disabled
        if (DisableInterface)
        {
          e.Cancel = true;
          return;
        }

        // check if there's need to saves
        var docinfo = Documents[s as DocumentContent];
        if (info.Modified)
        {
          var result = MessageBox.Show(Editor.Resources.SourceModified, Editor.Resources.Caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
          if (result == MessageBoxResult.Cancel)
            e.Cancel = true;
        }

        if (!e.Cancel)
          Documents.Remove((DocumentContent)s);
      };

      shbox.TextChanged += (s, e) =>
      {
        var dc = docContent.SelectedItem as DocumentContent;
        if (!Documents[dc].Modified)
        {
          Documents[dc].Modified = true;
          dc.Title += " *";
        }
      };

      docContent.Items.Add(newdoc);
      Documents.Add(newdoc, info);

      newdoc.Activate();
    }

    /// <summary>
    /// Open an existing file
    /// </summary>
    private void OpenFile()
    {
      // create a new open file dialog and set it's properties
      var of = new OpenFileDialog();
      of.DefaultExt = "mr";
      of.CheckFileExists = true;
      of.Title = Editor.Resources.OpenCaption;
      of.Filter = Editor.Resources.MirelleSourceFiles + "|*.mr";
      of.Multiselect = false;

      // request file opening
      var result = of.ShowDialog();
      if (result.HasValue && result.Value)
      {
        CreateEditorTab(of.FileName);
      }
    }

    /// <summary>
    /// Save a file
    /// </summary>
    private void SaveFile()
    {
      // select current tabs and stuff
      var currTab = docContent.SelectedItem as DocumentContent;
      var currEditor = currTab.Content as SyntaxHighlightBox.SyntaxHighlightBox;
      var currInfo = Documents[currTab];

      // nothing to save?
      if (!currInfo.Modified) return;

      // request a file name if there's none assigned
      var cancel = false;
      if (currInfo.FullPath == "")
      {
        var sf = new SaveFileDialog();
        sf.DefaultExt = "mr";
        sf.Title = Editor.Resources.SaveCaption;
        sf.Filter = Editor.Resources.MirelleSourceFiles + "|*.mr";

        var result = sf.ShowDialog();
        if (result.HasValue && result.Value)
        {
          currInfo.FullPath = sf.FileName;
          currInfo.Name = sf.SafeFileName;
        }
        else
          cancel = true;
      }

      // user confirms save
      if (!cancel)
      {
        try
        {
          // save file
          var fs = new FileStream(currInfo.FullPath, FileMode.Create);
          var buf = System.Text.Encoding.UTF8.GetBytes(currEditor.Text);
          fs.Write(buf, 0, buf.Length);
          fs.Close();
        }
        catch
        {
          Error(String.Format(Editor.Resources.FileSaveFailed, currInfo.FullPath));
        }

        // update info
        currTab.Title = currInfo.Name;
        currInfo.Modified = false;
      }
    }

    /// <summary>
    /// Save file with a temporary name
    /// </summary>
    private string SaveTmp()
    {
      // select current tabs and stuff
      var currTab = docContent.SelectedItem as DocumentContent;
      var currEditor = currTab.Content as SyntaxHighlightBox.SyntaxHighlightBox;
      var currInfo = Documents[currTab];
      var fname = currTab.Title.Replace("*", "").Replace(" ", "") + ".mr";

      try
      {
        // save file
        var fs = new FileStream(fname, FileMode.Create);
        var buf = System.Text.Encoding.UTF8.GetBytes(currEditor.Text);
        fs.Write(buf, 0, buf.Length);
        fs.Close();

        return new FileInfo(fname).FullName;
      }
      catch
      {
        Error(String.Format(Editor.Resources.FileSaveFailed, currInfo.FullPath));
        return "";
      }
    }

    /// <summary>
    /// Compiles the source code in current file
    /// </summary>
    /// <returns>The file name of the assembly to execute</returns>
    private string Compile()
    {
      // select current tabs and stuff
      var currTab = docContent.SelectedItem as DocumentContent;
      var currEditor = currTab.Content as SyntaxHighlightBox.SyntaxHighlightBox;
      var currInfo = Documents[currTab];

      // the file has not been saved?
      // save it to a tmp folder
      var path = currInfo.FullPath;
      if (path == "")
        path = SaveTmp();
      else
        SaveFile();

      var filePath = path.Substring(0, path.LastIndexOf(".")+1) + "exe";

      // compile
      ShowCompileStart();
      var compiler = new Mirelle.Compiler();
      try
      {
        compiler.Process(path);

        // display info about successful compilation
        ShowCompileSuccess();

        compiler.SaveAssembly(filePath);
        return filePath;
      }
      catch (Mirelle.CompilerException ex)
      {
        ShowCompileError(ex);
        return "";
      }
      finally
      {
        // remove temp file
        if (currInfo.FullPath == "")
          new FileInfo(path).Delete();
      }
    }

    /// <summary>
    /// Display a message about compilation start
    /// </summary>
    private void ShowCompileStart()
    {
      Status.Content = Editor.Resources.Compiling;
      ErrorTable.Items.Clear();
    }

    /// <summary>
    /// Display message about successful compilation
    /// </summary>
    private void ShowCompileSuccess()
    {
      Status.Content = Editor.Resources.CompileSuccess;
    }

    /// <summary>
    /// Display message about compilation error
    /// </summary>
    /// <param name="ex">Compiler exception</param>
    private void ShowCompileError(Mirelle.CompilerException ex)
    {
      Status.Content = Editor.Resources.CompileFail;
      Errors.Activate();

      // select current tabs and stuff
      var currTab = docContent.SelectedItem as DocumentContent;
      var currEditor = currTab.Content as SyntaxHighlightBox.SyntaxHighlightBox;
      var currInfo = Documents[currTab];

      var path = currInfo.FullPath;
      if (path == "")
        path = new FileInfo(currTab.Title.Replace("*", "").Replace(" ", "") + ".mr").FullName;

      if (ex.File == path)
      {
        ex.File = currInfo.Name;
        currEditor.MarkError(ex);
      }

      ErrorTable.Items.Add(ex);
    }

    /// <summary>
    /// Compile and execute the source file
    /// </summary>
    private void Run()
    {
      RunFile = Compile();
      if (RunFile == "") return;

      CopyLibraries(RunFile);

      RunThread = new Thread(new ThreadStart(RunIntercepted));
      RunThread.Start();
    }

    /// <summary>
    /// Copy required libraries to the executable file folder
    /// </summary>
    /// <param name="file"></param>
    private void CopyLibraries(string file)
    {
      var dir = new FileInfo(file).Directory.FullName;
      var libs = new[]
      {
        "MirelleStdlib.dll",
        "DynamicDataDisplay.dll",
        "MathNet.Numerics.dll"
      };

      foreach (var lib in libs)
      {
        try
        {
          new FileInfo(lib).CopyTo(dir + "\\" + lib, true);
        }
        catch { }
      }
    }

    /// <summary>
    /// Run a program in console-intercepted mode
    /// </summary>
    /// <param name="file"></param>
    private void RunIntercepted()
    {
      DisableInterface = true;

      var process = new Process();
      process.EnableRaisingEvents = true;
      process.Exited += (s, e) =>
      {
        DisableInterface = false;
        new FileInfo(RunFile).Delete();
      };

      process.StartInfo.FileName = RunFile;
      process.Start();
    }

    /// <summary>
    /// Display an error message
    /// </summary>
    /// <param name="str">Error</param>
    private void Error(string str)
    {
      MessageBox.Show(str, Editor.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Reset string captions according to current locale
    /// </summary>
    private void ReloadInterface()
    {
      // title
      Title = Editor.Resources.Caption;

      // menu
      FileMenu.Header = Editor.Resources.File;
      NewMenu.Header = Editor.Resources.New;
      OpenMenu.Header = Editor.Resources.Open;
      SaveMenu.Header = Editor.Resources.Save;
      ExitMenu.Header = Editor.Resources.Exit;
      BuildMenu.Header = Editor.Resources.Build;
      CompileMenu.Header = Editor.Resources.Compile;
      RunMenu.Header = Editor.Resources.Run;
      InfoMenu.Header = Editor.Resources.Info;
      AboutMenu.Header = Editor.Resources.About;
      LanguagesMenu.Header = Editor.Resources.Languages;

      // toolbars
      NewButton.ToolTip = Editor.Resources.NewHint;
      OpenButton.ToolTip = Editor.Resources.OpenHint;
      SaveButton.ToolTip = Editor.Resources.SaveHint;
      CompileButton.ToolTip = Editor.Resources.CompileHint;
      RunButton.ToolTip = Editor.Resources.RunHint;

      // columns
      ErrorColumn.Header = Editor.Resources.Error;
      FileColumn.Header = Editor.Resources.File;
      LineColumn.Header = Editor.Resources.Line;
      OffsetColumn.Header = Editor.Resources.Offset;

      // various
      Errors.Title = Editor.Resources.ErrorConsole;
      ScratchPadTab.Title = Editor.Resources.ScratchPad;
      Status.Content = Editor.Resources.Welcome;
    }
  }
}
