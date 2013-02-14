using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MND = MathNet.Numerics.Distributions;

namespace MirelleStdlib.Extenders
{
  public class DistributionExtender
  {
    /// <summary>
    /// Return the mean value of a distribution
    /// </summary>
    /// <param name="distr"></param>
    /// <returns></returns>
    public static double Mean(MND.IContinuousDistribution distr)
    {
      return distr.Mean;
    }

    /// <summary>
    /// Return the variance of a distribution
    /// </summary>
    /// <param name="distr"></param>
    /// <returns></returns>
    public static double Variance(MND.IContinuousDistribution distr)
    {
      return distr.Variance;
    }

    /// <summary>
    /// Return the standard deviation of a distribution
    /// </summary>
    /// <param name="distr"></param>
    /// <returns></returns>
    public static double Deviation(MND.IContinuousDistribution distr)
    {
      return distr.StdDev;
    }

    /// <summary>
    /// Return the entropy of a distribution
    /// </summary>
    /// <param name="distr"></param>
    /// <returns></returns>
    public static double Entropy(MND.IContinuousDistribution distr)
    {
      return distr.Entropy;
    }

    /// <summary>
    /// Return an array of samples of the deviation
    /// </summary>
    /// <param name="distr"></param>
    /// <returns></returns>
    public static double[] Samples(MND.IContinuousDistribution distr, int count)
    {
      if (count < 0) return null;
      var result = new double[count];
      for (int idx = 0; idx < count; idx++)
        result[idx] = distr.Sample();

      return result;
    }

    /// <summary>
    /// Create a default normal distribution
    /// </summary>
    /// <returns></returns>
    public static MND.IContinuousDistribution Normal()
    {
      return new MND.Normal();
    }

    /// <summary>
    /// Create a rayleigh distribution
    /// </summary>
    /// <returns></returns>
    public static MND.IContinuousDistribution Rayleigh()
    {
      return new MND.Rayleigh(1);
    }

    /// <summary>
    /// Create a rayleigh distribution with a scale
    /// </summary>
    /// <param name="scale">Scale value</param>
    /// <returns></returns>
    public static MND.IContinuousDistribution Rayleigh(double scale)
    {
      return new MND.Rayleigh(scale);
    }

    /// <summary>
    /// Create an exponential distribution
    /// </summary>
    /// <param name="lambda">Lambda value</param>
    /// <returns></returns>
    public static MND.IContinuousDistribution Exponential(double lambda)
    {
      return new MND.Exponential(lambda);
    }

    /// <summary>
    /// Create a new Erlang distribution
    /// </summary>
    /// <param name="shape">Shape id</param>
    /// <param name="scale">Scale value</param>
    /// <returns></returns>
    public static MND.IContinuousDistribution Erlang(int shape, double scale)
    {
      return new MND.Erlang(shape, scale);
    }

    /// <summary>
    /// Create a continuous uniform distribution
    /// </summary>
    /// <returns></returns>
    public static MND.IContinuousDistribution Uniform()
    {
      return new MND.ContinuousUniform();
    }

    /// <summary>
    /// Create a continuous uniform distribution with bounds
    /// </summary>
    /// <param name="from">Lower bound</param>
    /// <param name="to">Upper bound</param>
    /// <returns></returns>
    public static MND.IContinuousDistribution Uniform(double from, double to)
    {
      return new MND.ContinuousUniform(from, to);
    }

    /// <summary>
    /// Create a continuous uniform distribution with bounds given by a range
    /// </summary>
    /// <param name="rg">Range</param>
    /// <returns></returns>
    public static MND.IContinuousDistribution Uniform(Range rg)
    {
      return new MND.ContinuousUniform(rg.From(), rg.To());
    }
  }
}
