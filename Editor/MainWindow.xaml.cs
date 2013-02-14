using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using AvalonDock;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace Editor
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      CreateEditorTab();
    }

    private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !DisableInterface;
    }

    private void DocumentCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
    {
      if (DisableInterface)
        e.CanExecute = false;
      else
        e.CanExecute = Documents.Count > 0;
    }

    private void NewCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      CreateEditorTab();
    }

    private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      OpenFile();
    }

    private void SaveCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      SaveFile();
    }

    private void CompileCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      Compile();
    }

    private void RunCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      Run();
    }

    private void InfoCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      new AboutWindow().ShowDialog();
    }

    private void LangRussianCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("Ru-ru", true);
      ReloadInterface();
    }

    private void LangEnglishCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
      ReloadInterface();
    }
  }
}
