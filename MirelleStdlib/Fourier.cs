using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using MN = MathNet.Numerics.IntegralTransforms;

namespace MirelleStdlib
{
  public class Fourier
  {
    /// <summary>
    /// Perform a FFT using Bluestein's algorithm
    /// </summary>
    /// <param name="input">Array of complex numbers</param>
    /// <returns></returns>
    public Complex[] FFT(Complex[] input)
    {
      Complex[] output = new Complex[input.Length];
      Array.Copy(input, output, input.Length);
      var transform = new MN.Algorithms.DiscreteFourierTransform();
      transform.BluesteinForward(output, MN.FourierOptions.Matlab);
      return output;
    }

    /// <summary>
    /// Perform a IFFT using Bluestein's algorithm
    /// </summary>
    /// <param name="input">Array of complex numbers</param>
    /// <returns></returns>
    public Complex[] IFFT(Complex[] input)
    {
      Complex[] output = new Complex[input.Length];
      Array.Copy(input, output, input.Length);
      var transform = new MN.Algorithms.DiscreteFourierTransform();
      transform.BluesteinInverse(output, MN.FourierOptions.Matlab);
      return output;
    }
  }
}
