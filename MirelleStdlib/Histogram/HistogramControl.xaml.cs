using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Numerics;
using System.Globalization;
using System.Threading;

namespace MirelleStdlib.Histogram
{
  public enum HistogramState
  {
    Ready,
    DataUpdate,
    Redraw
  }

  /// <summary>
  /// Interaction logic for Histogram.xaml
  /// </summary>
  public partial class HistogramControl : UserControl
  {
    /// <summary>
    /// Data to display
    /// </summary>
    private List<KeyValuePair<string, double>> Data;

    /// <summary>
    /// Maximum column height in the data
    /// </summary>
    private double Max = 0;

    /// <summary>
    /// Locker variable disabling redraw during data update
    /// </summary>
    private HistogramState State = HistogramState.Ready;

    /// <summary>
    /// Padding
    /// </summary>
    public int Pad = 16;

    /// <summary>
    /// Pen for drawing most lines and text
    /// </summary>
    public Pen Pen;

    /// <summary>
    /// Pen for drawing hint lines
    /// </summary>
    public Pen HintPen;

    /// <summary>
    /// Brush for drawing graph background
    /// </summary>
    public Brush TransparentBrush;

    /// <summary>
    /// Brush for drawing text
    /// </summary>
    public Brush TextBrush;

    /// <summary>
    /// Brush for drawing columns
    /// </summary>
    public Brush ColumnBrush;

    /// <summary>
    /// Font for texts
    /// </summary>
    public Typeface TextFont;

    /// <summary>
    /// Culture info
    /// </summary>
    public CultureInfo Culture;

    /// <summary>
    /// Construct the histogram
    /// </summary>
    public HistogramControl()
    {
      InitializeComponent();

      // init pens, brushes and fonts
      Pen = new Pen(Brushes.Black, 1);
      HintPen = new Pen(Brushes.LightGray, 1);
      TransparentBrush = Brushes.Transparent;
      TextBrush = Brushes.Black;
      ColumnBrush = new LinearGradientBrush(Color.FromRgb(70, 140, 210), Color.FromRgb(130, 190, 210), 45);
      TextFont = new Typeface("Verdana");

      Culture = System.Globalization.CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Set data to display
    /// </summary>
    /// <param name="data">Array of complex numbers</param>
    public void SetData(IEnumerable<Complex> data)
    {
      WaitReady();

      Data = new List<KeyValuePair<string, double>>();

      Max = 0;
      foreach (var curr in data)
      {
        if (curr.Imaginary > Max) Max = curr.Imaginary;
        Data.Add(new KeyValuePair<string, double>(curr.Real.ToString("0.###"), curr.Imaginary));
      }


      ForceRedraw();
    }

    /// <summary>
    /// Set data to display
    /// </summary>
    /// <param name="xs">Array of column identifiers</param>
    /// <param name="ys">Array of values</param>
    public void SetData(double[] xs, double[] ys)
    {
      WaitReady();

      Data = new List<KeyValuePair<string, double>>();
      var length = Math.Min(xs.Length, ys.Length);

      Max = 0;
      for (int idx = 0; idx < length; idx++)
      {
        if (ys[idx] > Max) Max = ys[idx];
        Data.Add(new KeyValuePair<string, double>(xs[idx].ToString("0.###"), ys[idx]));
      }

      ForceRedraw();
    }

    /// <summary>
    /// Set data to display (column identifiers = array indices)
    /// </summary>
    /// <param name="values"></param>
    public void SetData(IEnumerable<double> values)
    {
      WaitReady();

      Data = new List<KeyValuePair<string, double>>();
      var idx = 0;
      foreach (var curr in values)
      {
        if (curr > Max) Max = curr;
        Data.Add(new KeyValuePair<string, double>(idx.ToString("0.###"), curr));
        idx++;
      }

      ForceRedraw();
    }


    public void SetData(string[] captions, double[] values)
    {
      WaitReady();

      Data = new List<KeyValuePair<string, double>>();
      var length = Math.Min(captions.Length, values.Length);

      Max = 0;
      for (int idx = 0; idx < length; idx++)
      {
        if (values[idx] > Max) Max = values[idx];
        Data.Add(new KeyValuePair<string, double>(captions[idx], values[idx]));
      }

      ForceRedraw();
    }

    /// <summary>
    /// Overridden method that is invoked when a redraw is required
    /// </summary>
    /// <param name="dc"></param>
    protected override void OnRender(DrawingContext dc)
    {
      if (State != HistogramState.Ready) return;

      State = HistogramState.Redraw;

      if(Data == null || Data.Count == 0)
        RenderEmpty(dc);
      else
      {
        var box = CalculateBox();
        RenderBox(dc, box);
        RenderHints(dc, box);
        RenderColumns(dc, box);
      }

      State = HistogramState.Ready;
    }

    /// <summary>
    /// Render an empty graph
    /// </summary>
    private void RenderEmpty(DrawingContext dc)
    {
      // draw surrounding box
      var box = new Rect(Pad, Pad, SafeSize(ActualWidth - Pad * 2), SafeSize(ActualHeight - Pad * 2));
      RenderBox(dc, box);

      // draw no-data info
      var text = FormatText("No data");
      if(box.Width > text.Width && box.Height > text.Height)
      {
        var textPoint = new Point(ActualWidth / 2 - text.Width / 2, ActualHeight / 2 - text.Height / 2);
        dc.DrawText(text, textPoint);
      }
    }

    /// <summary>
    /// Calculate the surrounding box rectangle for the graph
    /// </summary>
    /// <returns></returns>
    private Rect CalculateBox()
    {
      // calculate left offset for hints
      var hintCount = GetHintCount();

      // find longest hint
      var maxHintWidth = 0.0;
      for (int idx = 0; idx <= hintCount; idx++)
      {
        var currHint = Max / hintCount * idx;
        var format = GetHintFormat(currHint);

        var hint = FormatText(currHint.ToString(format));
        if (hint.Width > maxHintWidth)
          maxHintWidth = hint.Width;
      }

      // draw bounding box
      var boxPoint = new Point(Pad + maxHintWidth, Pad);
      var boxSize = new Size(SafeSize(ActualWidth - Pad * 2 - maxHintWidth), SafeSize(ActualHeight - Pad * 3));

      return new Rect(boxPoint, boxSize);
    }

    /// <summary>
    /// Render the surrounding box around the graph
    /// </summary>
    /// <param name="dc">Drawing context</param>
    /// <param name="box">Surrounding box</param>
    private void RenderBox(DrawingContext dc, Rect box)
    {
      dc.DrawRectangle(TransparentBrush, Pen, box);
    }

    /// <summary>
    /// Render the value hints at left hand side and hint lines
    /// </summary>
    /// <param name="dc">Drawing context</param>
    /// <param name="box">Surrounding box</param>
    private void RenderHints(DrawingContext dc, Rect box)
    {
      var hintCount = GetHintCount();

      // value hints
      for (int idx = hintCount; idx >= 0; idx--)
      {
        // draw text
        var currHint = Max / hintCount * idx;
        var format = GetHintFormat(currHint);
        var hint = FormatText(currHint.ToString(format));
        var hintPoint = new Point(box.X - hint.Width - (Pad / 2), Pad + box.Height - ((box.Height - Pad) / hintCount) * idx - (Pad / 2));
        dc.DrawText(hint, hintPoint);

        // draw line
        if (idx > 0)
        {
          var lineStart = new Point(box.X, hintPoint.Y + (Pad / 2));
          var lineEnd = new Point(box.X + box.Width, hintPoint.Y + (Pad / 2));
          dc.DrawLine(HintPen, lineStart, lineEnd);
        }
      }
    }

    /// <summary>
    /// Render the columns and their captions
    /// </summary>
    /// <param name="dc">Drawing context</param>
    /// <param name="box">Surrounding box</param>
    private void RenderColumns(DrawingContext dc, Rect box)
    {
      // columns
      var columnCount = Data.Count;
      var columnPad = box.Width / (columnCount * 5 + 1);
      var columnSpace = columnPad * 5;
      var columnWidth = columnPad * 4;

      for (var idx = 0; idx < columnCount; idx++)
      {
        // draw column
        var curr = Data[idx];
        var columnHeight = (int)((box.Height - Pad) / Max * curr.Value);
        if (columnHeight < 0) columnHeight = 0;
        var columnPoint = new Point((int)(box.X + columnPad + columnSpace * idx), (int)(Pad + box.Height - columnHeight));
        var columnSize = new Size((int)SafeSize(columnWidth), SafeSize(columnHeight));

        dc.DrawRectangle(ColumnBrush, Pen, new Rect(columnPoint, columnSize));

        // draw column text
        var caption = curr.Key;
        var columnText = FormatText(caption);
        if (columnText.Width > columnWidth && caption.Length > 2)
        {
          var extraPart = columnWidth / columnText.Width;
          var len = Math.Max((int)(caption.Length * extraPart), 1);
          caption = caption.Substring(0, len) + "...";
          columnText = FormatText(caption);
        }

        var textPoint = new Point(columnPoint.X + (columnWidth - columnText.Width) / 2, Pad * 1.2 + box.Height);
        dc.DrawText(columnText, textPoint);
      }
    }

    /// <summary>
    /// Generate test data for displaying in the histogram
    /// </summary>
    private void GenerateTestData()
    {
      Data = new List<KeyValuePair<string, double>>();
      Max = 0;
      for (int idx = 0; idx < 20; idx++)
      {
        var value = 2000 / (Math.Sqrt(2 * Math.PI * 5)) * Math.Exp(-(Math.Pow(idx - 10, 2) / (2 * 5)));
        if(value > Max) Max = value;
        Data.Add(new KeyValuePair<string, double>((idx * 10).ToString(), value));
      }
    }

    /// <summary>
    /// Return a proper format for displaying hints: casual or scientific
    /// </summary>
    /// <param name="hint">Current hint</param>
    /// <returns></returns>
    private string GetHintFormat(double hint)
    {
      if (hint > 0 && (Math.Log10(hint) > 3 || Math.Log10(1 / hint) > 3))
        return "E";

      return "0.###";
    }

    /// <summary>
    /// Calculate appropriate hint count
    /// </summary>
    /// <returns></returns>
    private int GetHintCount()
    {
      var hintCount = (int)((ActualHeight - Pad * 3) / (Pad * 2));
      if (hintCount > 10) hintCount = 10;

      return hintCount;
    }

    /// <summary>
    /// Format a string according to global rules
    /// </summary>
    /// <param name="str">String to be formatted</param>
    /// <returns></returns>
    private FormattedText FormatText(string str)
    {
      return new FormattedText(str, Culture, FlowDirection.LeftToRight, TextFont, 12, TextBrush);
    }

    /// <summary>
    /// Ensure size value is >= 0
    /// </summary>
    /// <param name="value">Value</param>
    /// <returns></returns>
    private double SafeSize(double value)
    {
      return value > 0 ? value : 0.0;
    }

    /// <summary>
    /// Force redraw after data has been updated
    /// </summary>
    private void ForceRedraw()
    {
      State = HistogramState.Ready;

      Dispatcher.Invoke((Action)
        delegate()
        {
          InvalidateVisual();
        }
      );
    }

    /// <summary>
    /// Wait for the component to be ready
    /// </summary>
    private void WaitReady()
    {
      while (State != HistogramState.Ready)
        Thread.Sleep(10);
    }
  }
}
