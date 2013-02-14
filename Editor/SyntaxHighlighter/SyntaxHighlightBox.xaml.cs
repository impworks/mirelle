using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Collections.Generic;
using System.Threading;

namespace SyntaxHighlightBox
{
  public partial class SyntaxHighlightBox : TextBox
  {

    // --------------------------------------------------------------------
    // Attributes
    // --------------------------------------------------------------------

    public double LineHeight
    {
      get { return lineHeight; }
      set
      {
        if (value != lineHeight)
        {
          lineHeight = value;
          blockHeight = MaxLineCountInBlock * value;
          TextBlock.SetLineStackingStrategy(this, LineStackingStrategy.BlockLineHeight);
          TextBlock.SetLineHeight(this, lineHeight);
        }
      }
    }

    public int MaxLineCountInBlock
    {
      get { return maxLineCountInBlock; }
      set
      {
        maxLineCountInBlock = value > 0 ? value : 0;
        blockHeight = value * LineHeight;
      }
    }

    public IHighlighter CurrentHighlighter { get; set; }

    public Mirelle.CompilerException Error { get; set; }

    private DrawingControl renderCanvas;
    private DrawingControl lineNumbersCanvas;
    private ScrollViewer scrollViewer;
    private double lineHeight;
    private int totalLineCount;
    private List<InnerTextBlock> blocks;
    private double blockHeight;
    private int maxLineCountInBlock;

    // --------------------------------------------------------------------
    // Ctor and event handlers
    // --------------------------------------------------------------------

    public SyntaxHighlightBox()
    {
      InitializeComponent();

      MaxLineCountInBlock = 100;
      LineHeight = FontSize * 1.3;
      totalLineCount = 1;
      blocks = new List<InnerTextBlock>();

      Loaded += (s, e) =>
      {
        this.ApplyTemplate();

        renderCanvas = (DrawingControl)Template.FindName("PART_RenderCanvas", this);
        lineNumbersCanvas = (DrawingControl)Template.FindName("PART_LineNumbersCanvas", this);
        scrollViewer = (ScrollViewer)Template.FindName("PART_ContentHost", this);

        lineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", totalLineCount)) + 5;

        scrollViewer.ScrollChanged += OnScrollChanged;

        InvalidateBlocks(0);
        InvalidateVisual();
      };

      SizeChanged += (s, e) =>
      {
        if (e.HeightChanged == false)
          return;
        UpdateBlocks();
        InvalidateVisual();
      };

      TextChanged += (s, e) =>
      {
        Error = null;
        UpdateTotalLineCount();
        InvalidateBlocks(e.Changes.First().Offset);
        InvalidateVisual();
      };

      DataObject.AddPastingHandler(this, SmartPasteHandler);
    }

    public void MarkError(Mirelle.CompilerException ex)
    {
      Error = ex;
      Redraw();
    }

    /// <summary>
    /// Enable smart identing features
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
      e.Handled = true;

      var vertOff = VerticalOffset;

      if (e.Key == Key.Tab)
        SmartTabHandler(e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift));
      else if (e.Key == Key.Enter)
        SmartEnterHandler();
      else
        e.Handled = false;

      ScrollToVerticalOffset(vertOff);

      base.OnKeyDown(e);
    }

    public void Redraw()
    {
      InvalidateBlocks(0);
      InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      DrawBlocks();
      base.OnRender(drawingContext);
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.VerticalChange != 0)
        UpdateBlocks();
      InvalidateVisual();
    }

    // -----------------------------------------------------------
    // Smart text handling
    // -----------------------------------------------------------

    /// <summary>
    /// Replace tab with two spaces
    /// Or tabulate a whole block of code
    /// </summary>
    private void SmartTabHandler(bool shift)
    {
      if (SelectionLength == 0)
        InsertAtCaret("  ");
      else
      {
        // do not select last endline
        if (Text[SelectionStart + SelectionLength - 1] == '\n')
          SelectionLength--;

        // if the fragment starts at first line,
        // temporarily prepend a "\n" to be removed afterwards
        var augmented = false;
        var blockStart = Text.LastIndexOf("\n", SelectionStart);
        if (blockStart == -1)
        {
          blockStart = 0;
          augmented = true;
        }

        // retrieve fragment to be idented
        var blockSize = SelectionLength + (SelectionStart - blockStart);
        var src = (augmented ? "\n" : "") + Text.Substring(blockStart, blockSize);

        // calculate new selection and caret positions
        var start = SelectionStart;
        var length = SelectionLength;

        // ident or un-ident
        string result;
        if (shift)
        {
          result = src.Replace("\n  ", "\n");
          if (start > blockStart + 1) start -= 2;
          length += (result.Length - src.Length) + (start > blockStart + 1 ? 2 : 0);
        }
        else
        {
          result = src.Replace("\n", "\n  ");
          if (start > blockStart + 1) start += 2;
          length += (result.Length - src.Length) - (start > blockStart + 1 ? 2 : 0);
        }

        // replace text
        Text = Text.Substring(0, blockStart) + (augmented ? result.Substring(1) : result) + Text.Substring(blockStart + blockSize);

        // set selection and caret
        CaretIndex = SelectionStart = start;
        SelectionLength = length;
      }
    }

    /// <summary>
    /// Insert proper tabulation at the start of next line
    /// </summary>
    private void SmartEnterHandler()
    {
      var lineStart = Text.LastIndexOf("\n", CaretIndex > 0 ? CaretIndex - 1 : CaretIndex) + 1;
      var line = Text.Substring(lineStart, CaretIndex - lineStart);

      int ident = 0;
      while (ident < line.Length && line[ident] == ' ')
        ident++;

      InsertAtCaret("\n" + new String(' ', ident));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SmartPasteHandler(object sender, DataObjectPastingEventArgs e)
    {
      var vertOff = VerticalOffset;

      var isText = e.SourceDataObject.GetDataPresent(System.Windows.DataFormats.Text, true);
      if (!isText)
        return;

      var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
      text = text.Replace("\t", "  ");

      if (SelectionLength == 0)
        InsertAtCaret(text);
      else
      {
        var start = SelectionStart;
        var end = SelectionStart + SelectionLength;
        Text = Text.Substring(0, start) + text + Text.Substring(end);
        CaretIndex = start + text.Length;
      }

      e.CancelCommand();
      e.Handled = false;

      ScrollToVerticalOffset(vertOff);
    }

    /// <summary>
    /// Insert a string at caret position
    /// </summary>
    /// <param name="str"></param>
    private void InsertAtCaret(string str)
    {
      var ci = CaretIndex;
      Text = Text.Substring(0, ci) + str + Text.Substring(ci);
      CaretIndex = ci + str.Length;
    }

    // -----------------------------------------------------------
    // Updating & Block managing
    // -----------------------------------------------------------

    private void UpdateTotalLineCount()
    {
      totalLineCount = TextUtilities.GetLineCount(Text);
    }

    private void UpdateBlocks()
    {
      if (blocks.Count == 0)
        return;

      // While something is visible after last block...
      while (!blocks.Last().IsLast && blocks.Last().Position.Y + blockHeight - VerticalOffset < ActualHeight)
      {
        int firstLineIndex = blocks.Last().LineEndIndex + 1;
        int lastLineIndex = firstLineIndex + maxLineCountInBlock - 1;
        lastLineIndex = lastLineIndex <= totalLineCount - 1 ? lastLineIndex : totalLineCount - 1;

        int fisrCharIndex = blocks.Last().CharEndIndex + 1;
        int lastCharIndex = TextUtilities.GetLastCharIndexFromLineIndex(Text, lastLineIndex); // to be optimized (forward search)

        if (lastCharIndex <= fisrCharIndex)
        {
          blocks.Last().IsLast = true;
          return;
        }

        InnerTextBlock block = new InnerTextBlock(
          fisrCharIndex,
          lastCharIndex,
          blocks.Last().LineEndIndex + 1,
          lastLineIndex,
          LineHeight);
        block.RawText = block.GetSubString(Text);
        block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);
        blocks.Add(block);
        FormatBlock(block, blocks.Count > 1 ? blocks[blocks.Count - 2] : null);
      }
    }

    private void InvalidateBlocks(int changeOffset)
    {
      InnerTextBlock blockChanged = null;
      for (int i = 0; i < blocks.Count; i++)
      {
        if (blocks[i].CharStartIndex <= changeOffset && changeOffset <= blocks[i].CharEndIndex + 1)
        {
          blockChanged = blocks[i];
          break;
        }
      }

      if (blockChanged == null && changeOffset > 0)
        blockChanged = blocks.Last();

      int fvline = blockChanged != null ? blockChanged.LineStartIndex : 0;
      int lvline = GetIndexOfLastVisibleLine();
      int fvchar = blockChanged != null ? blockChanged.CharStartIndex : 0;
      int lvchar = TextUtilities.GetLastCharIndexFromLineIndex(Text, lvline);

      if (blockChanged != null)
        blocks.RemoveRange(blocks.IndexOf(blockChanged), blocks.Count - blocks.IndexOf(blockChanged));

      int localLineCount = 1;
      int charStart = fvchar;
      int lineStart = fvline;
      for (int i = fvchar; i < Text.Length; i++)
      {
        if (Text[i] == '\n')
        {
          localLineCount += 1;
        }
        if (i == Text.Length - 1)
        {
          string blockText = Text.Substring(charStart);
          InnerTextBlock block = new InnerTextBlock(
            charStart,
            i, lineStart,
            lineStart + TextUtilities.GetLineCount(blockText) - 1,
            LineHeight);
          block.RawText = block.GetSubString(Text);
          block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);
          block.IsLast = true;

          foreach (InnerTextBlock b in blocks)
            if (b.LineStartIndex == block.LineStartIndex)
              throw new Exception();

          blocks.Add(block);
          FormatBlock(block, blocks.Count > 1 ? blocks[blocks.Count - 2] : null);
          break;
        }
        if (localLineCount > maxLineCountInBlock)
        {
          InnerTextBlock block = new InnerTextBlock(
            charStart,
            i,
            lineStart,
            lineStart + maxLineCountInBlock - 1,
            LineHeight);
          block.RawText = block.GetSubString(Text);
          block.LineNumbers = GetFormattedLineNumbers(block.LineStartIndex, block.LineEndIndex);

          foreach (InnerTextBlock b in blocks)
            if (b.LineStartIndex == block.LineStartIndex)
              throw new Exception();

          blocks.Add(block);
          FormatBlock(block, blocks.Count > 1 ? blocks[blocks.Count - 2] : null);

          charStart = i + 1;
          lineStart += maxLineCountInBlock;
          localLineCount = 1;

          if (i > lvchar)
            break;
        }
      }
    }

    // -----------------------------------------------------------
    // Rendering
    // -----------------------------------------------------------

    private void DrawBlocks()
    {
      if (!IsLoaded || renderCanvas == null || lineNumbersCanvas == null)
        return;

      var dc = renderCanvas.GetContext();
      var dc2 = lineNumbersCanvas.GetContext();
      for (int i = 0; i < blocks.Count; i++)
      {
        InnerTextBlock block = blocks[i];
        Point blockPos = block.Position;
        double top = blockPos.Y - VerticalOffset;
        double bottom = top + blockHeight;
        if (top < ActualHeight && bottom > 0)
        {
          try
          {
            var blockPoint = new Point(2 - HorizontalOffset, block.Position.Y - VerticalOffset);

            // draw error
            if (Error != null)
            {
              var geom = block.FormattedText.BuildHighlightGeometry(blockPoint, Error.Position, Error.Length);
              dc.DrawGeometry(Brushes.Pink, new Pen(Brushes.Black, 0), geom);
            }

            // draw main text
            dc.DrawText(block.FormattedText, blockPoint);

            // draw line margins
            if (IsLineNumbersMarginVisible)
            {
              lineNumbersCanvas.Width = GetFormattedTextWidth(string.Format("{0:0000}", totalLineCount)) + 5;
              dc2.DrawText(block.LineNumbers, new Point(lineNumbersCanvas.ActualWidth, 1 + block.Position.Y - VerticalOffset));
            }
          }
          catch
          {
            // Don't know why this exception is raised sometimes.
            // Reproduce steps:
            // - Sets a valid syntax highlighter on the box.
            // - Copy a large chunk of code in the clipboard.
            // - Paste it using ctrl+v and keep these buttons pressed.
          }
        }
      }
      dc.Close();
      dc2.Close();
    }

    // -----------------------------------------------------------
    // Utilities
    // -----------------------------------------------------------

    /// <summary>
    /// Returns the index of the first visible text line.
    /// </summary>
    public int GetIndexOfFirstVisibleLine()
    {
      int guessedLine = (int)(VerticalOffset / lineHeight);
      return guessedLine > totalLineCount ? totalLineCount : guessedLine;
    }

    /// <summary>
    /// Returns the index of the last visible text line.
    /// </summary>
    public int GetIndexOfLastVisibleLine()
    {
      double height = VerticalOffset + ViewportHeight;
      int guessedLine = (int)(height / lineHeight);
      return guessedLine > totalLineCount - 1 ? totalLineCount - 1 : guessedLine;
    }

    /// <summary>
    /// Formats and Highlights the text of a block.
    /// </summary>
    private void FormatBlock(InnerTextBlock currentBlock, InnerTextBlock previousBlock)
    {
      currentBlock.FormattedText = GetFormattedText(currentBlock.RawText);
      if (CurrentHighlighter != null)
      {
        ThreadPool.QueueUserWorkItem(p =>
        {
          int previousCode = previousBlock != null ? previousBlock.Code : -1;
          currentBlock.Code = CurrentHighlighter.Highlight(currentBlock.FormattedText, previousCode);
        });
      }
    }

    /// <summary>
    /// Returns a formatted text object from the given string
    /// </summary>
    private FormattedText GetFormattedText(string text)
    {
      FormattedText ft = new FormattedText(
        text,
        System.Globalization.CultureInfo.InvariantCulture,
        FlowDirection.LeftToRight,
        new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
        FontSize,
        Brushes.Black);

      ft.Trimming = TextTrimming.None;
      ft.LineHeight = lineHeight;

      return ft;
    }

    /// <summary>
    /// Returns a string containing a list of numbers separated with newlines.
    /// </summary>
    private FormattedText GetFormattedLineNumbers(int firstIndex, int lastIndex)
    {
      string text = "";
      for (int i = firstIndex + 1; i <= lastIndex + 1; i++)
        text += i.ToString() + "\n";
      text = text.Trim();

      FormattedText ft = new FormattedText(
        text,
        System.Globalization.CultureInfo.InvariantCulture,
        FlowDirection.LeftToRight,
        new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
        FontSize,
        new SolidColorBrush(Color.FromRgb(0x21, 0xA1, 0xD8)));

      ft.Trimming = TextTrimming.None;
      ft.LineHeight = lineHeight;
      ft.TextAlignment = TextAlignment.Right;

      return ft;
    }

    /// <summary>
    /// Returns the width of a text once formatted.
    /// </summary>
    private double GetFormattedTextWidth(string text)
    {
      FormattedText ft = new FormattedText(
        text,
        System.Globalization.CultureInfo.InvariantCulture,
        FlowDirection.LeftToRight,
        new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
        FontSize,
        Brushes.Black);

      ft.Trimming = TextTrimming.None;
      ft.LineHeight = lineHeight;

      return ft.Width;
    }

    // -----------------------------------------------------------
    // Dependency Properties
    // -----------------------------------------------------------

    public static readonly DependencyProperty IsLineNumbersMarginVisibleProperty = DependencyProperty.Register(
      "IsLineNumbersMarginVisible", typeof(bool), typeof(SyntaxHighlightBox), new PropertyMetadata(true));

    public bool IsLineNumbersMarginVisible
    {
      get { return (bool)GetValue(IsLineNumbersMarginVisibleProperty); }
      set { SetValue(IsLineNumbersMarginVisibleProperty, value); }
    }

    // -----------------------------------------------------------
    // Classes
    // -----------------------------------------------------------

    private class InnerTextBlock
    {
      public string RawText { get; set; }
      public FormattedText FormattedText { get; set; }
      public FormattedText LineNumbers { get; set; }
      public int CharStartIndex { get; private set; }
      public int CharEndIndex { get; private set; }
      public int LineStartIndex { get; private set; }
      public int LineEndIndex { get; private set; }
      public Point Position { get { return new Point(0, LineStartIndex * lineHeight); } }
      public bool IsLast { get; set; }
      public int Code { get; set; }

      private double lineHeight;

      public InnerTextBlock(int charStart, int charEnd, int lineStart, int lineEnd, double lineHeight)
      {
        CharStartIndex = charStart;
        CharEndIndex = charEnd;
        LineStartIndex = lineStart;
        LineEndIndex = lineEnd;
        this.lineHeight = lineHeight;
        IsLast = false;

      }

      public string GetSubString(string text)
      {
        return text.Substring(CharStartIndex, CharEndIndex - CharStartIndex + 1);
      }

      public override string ToString()
      {
        return string.Format("L:{0}/{1} C:{2}/{3} {4}",
          LineStartIndex,
          LineEndIndex,
          CharStartIndex,
          CharEndIndex,
          FormattedText.Text);
      }
    }
  }
}
