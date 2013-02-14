using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace MirelleStdlib.Extenders
{
  public class ComplexExtender
  {
    /// <summary>
    /// Convert complex to bool
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool ToBool(Complex value)
    {
      return value.Real == 0 && value.Imaginary == 0 ? false : true;
    }

    /// <summary>
    /// Convert complex to string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString(Complex value)
    {
      if (value.Imaginary >= 0)
        return String.Format("{0}+{1}i", value.Real, value.Imaginary);
      else
        return String.Format("{0}-{1}i", value.Real, Math.Abs(value.Imaginary));
    }

    /// <summary>
    /// Get the real part of a complex number
    /// </summary>
    /// <param name="value">Complex value</param>
    /// <returns></returns>
    public static double Real(Complex value)
    {
      return value.Real;
    }

    /// <summary>
    /// Get the imaginary part of a complex number
    /// </summary>
    /// <param name="value">Complex value</param>
    /// <returns></returns>
    public static double Imaginary(Complex value)
    {
      return value.Imaginary;
    }

    /// <summary>
    /// Merge a series of real and imaginary values into a series of complex values
    /// </summary>
    /// <param name="reals">Array of real parts</param>
    /// <param name="imgs">Array of imaginary parts</param>
    /// <returns></returns>
    public static Complex[] Merge(double[] reals, double[] imgs)
    {
      var length = Math.Min(reals.Length, imgs.Length);
      var arr = new Complex[length];

      for (int idx = 0; idx < length; idx++)
        arr[idx] = new Complex(reals[idx], imgs[idx]);

      return arr;
    }
  }
}
