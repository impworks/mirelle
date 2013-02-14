using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;

namespace MirelleStdlib.Chart
{
  public class Colors
  {
    public static int Red()
    {
      return System.Drawing.Color.Red.ToArgb();
    }

    public static int Orange()
    {
      return System.Drawing.Color.Orange.ToArgb();
    }

    public static int Yellow()
    {
      return System.Drawing.Color.Yellow.ToArgb();
    }

    public static int Green()
    {
      return System.Drawing.Color.Green.ToArgb();
    }

    public static int Blue()
    {
      return System.Drawing.Color.Blue.ToArgb();
    }

    public static int Violet()
    {
      return System.Drawing.Color.Violet.ToArgb();
    }

    /// <summary>
    /// Get a new color out of RGB values
    /// </summary>
    /// <param name="R">Red</param>
    /// <param name="G">Green</param>
    /// <param name="B">Blue</param>
    /// <returns></returns>
    public static int RGB(int R, int G, int B)
    {
      return 255 << 24 | R << 16 | G << 8 | B;
    }

    /// <summary>
    /// Generate a random color
    /// </summary>
    /// <returns></returns>
    public static int Random()
    {
      var color = ColorHelper.CreateRandomHsbColor();
      return RGB(color.R, color.G, color.B);
    }

    /// <summary>
    /// Get a color by it's string representation
    /// </summary>
    /// <param name="color">Color</param>
    /// <returns></returns>
    public static int Hex(string color)
    {
      var col = (Color)ColorConverter.ConvertFromString(color);
      return RGB(col.R, col.G, col.B);
    }

    /// <summary>
    /// Convert an integer color to System.Windows.Media.Color
    /// </summary>
    /// <param name="color">Color as integer</param>
    /// <returns></returns>
    public static Color FromInt(int color)
    {
      var R = (byte) ((color >> 16) & 0xFF);
      var G = (byte) ((color >> 8) & 0xFF);
      var B = (byte) (color & 0xFF);
      return Color.FromRgb(R, G, B);
    }
  }
}
