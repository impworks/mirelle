using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace MirelleStdlib.Extenders
{
  public static class MathExtender
  {
    /// <summary>
    /// Process an array of values
    /// </summary>
    /// <typeparam name="TIn">Input array type</typeparam>
    /// <typeparam name="TOut">Output array type</typeparam>
    /// <param name="input">Array values</param>
    /// <param name="functor">Conversion function</param>
    /// <returns></returns>
    public static TOut[] MapArray<TIn, TOut>(TIn[] input, Func<TIn, TOut> functor)
    {
      var result = new TOut[input.Length];
      for (int idx = 0; idx < input.Length; idx++)
        result[idx] = functor(input[idx]);

      return result;
    }

    /************************************************************************/
    /* Trigonometric functions on doubles                                   */
    /************************************************************************/
    public static double[] Sin(double[] xs)
    {
      return MapArray(xs, x => Math.Sin(x));
    }

    public static double[] Sinh(double[] xs)
    {
      return MapArray(xs, x => Math.Sinh(x));
    }

    public static double[] Asin(double[] xs)
    {
      return MapArray(xs, x => Math.Asin(x));
    }

    public static double[] Cos(double[] xs)
    {
      return MapArray(xs, x => Math.Cos(x));
    }

    public static double[] Cosh(double[] xs)
    {
      return MapArray(xs, x => Math.Cosh(x));
    }

    public static double[] Acos(double[] xs)
    {
      return MapArray(xs, x => Math.Acos(x));
    }

    public static double[] Tan(double[] xs)
    {
      return MapArray(xs, x => Math.Tan(x));
    }

    public static double[] Tanh(double[] xs)
    {
      return MapArray(xs, x => Math.Tanh(x));
    }

    public static double[] Atan(double[] xs)
    {
      return MapArray(xs, x => Math.Atan(x));
    }

    public static double[] Sqrt(double[] xs)
    {
      return MapArray(xs, x => Math.Sqrt(x));
    }

    public static double[] Log(double[] xs)
    {
      return MapArray(xs, x => Math.Log(x));
    }

    public static double[] Log10(double[] xs)
    {
      return MapArray(xs, x => Math.Log10(x));
    }

    /************************************************************************/
    /* Trigonometric functions on complex                                   */
    /************************************************************************/
    public static Complex[] Sin(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Sin(x));
    }

    public static Complex[] Sinh(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Sinh(x));
    }

    public static Complex[] Asin(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Asin(x));
    }

    public static Complex[] Cos(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Cos(x));
    }

    public static Complex[] Cosh(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Cosh(x));
    }

    public static Complex[] Acos(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Acos(x));
    }

    public static Complex[] Tan(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Tan(x));
    }

    public static Complex[] Tanh(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Tanh(x));
    }

    public static Complex[] Atan(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Atan(x));
    }

    public static Complex[] Sqrt(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Sqrt(x));
    }

    public static Complex[] Log(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Log(x));
    }

    public static Complex[] Log10(Complex[] xs)
    {
      return MapArray(xs, x => Complex.Log10(x));
    }

    /************************************************************************/
    /* Minimum and maximum of two values                                    */
    /************************************************************************/
    public static int Min(int x, int y)
    {
      return (int)Math.Min(x, y);
    }

    public static int Max(int x, int y)
    {
      return (int)Math.Max(x, y);
    }

    public static int Limit(int value, int low, int high)
    {
      return (value < low ? low : (value > high ? high : value));
    }

    public static double Limit(double value, double low, double high)
    {
      return (value < low ? low : (value > high ? high : value));
    }

    /************************************************************************/
    /* Rounding                                                             */
    /************************************************************************/
    public static int Floor(double x)
    {
      return (int)Math.Floor(x);
    }

    public static int Ceil(double x)
    {
      return (int)Math.Ceiling(x);
    }

    public static int Round(double x)
    {
      return (int)Math.Round(x);
    }

    public static int Abs(int x)
    {
      return (int)Math.Abs(x);
    }

    /************************************************************************/
    /* Constants                                                            */
    /************************************************************************/
    public static double E()
    {
      return Math.E;
    }

    public static double Pi()
    {
      return Math.PI;
    }

    public static double GoldRatio()
    {
      return MathNet.Numerics.Constants.GoldenRatio;
    }

    /************************************************************************/
    /* Randomization                                                        */
    /************************************************************************/
    private static Random RNG = new System.Random();

    public static double Random()
    {
      return RNG.NextDouble();
    }

    public static double Random(double from, double to)
    {
      if(from > to)
      {
        var tmp = to;
        to = from;
        from = tmp;
      }

      return from + RNG.NextDouble() * (to - from);
    }

    public static int Random(Range rg)
    {
      return RNG.Next(rg.From(), rg.To());
    }
  }
}
