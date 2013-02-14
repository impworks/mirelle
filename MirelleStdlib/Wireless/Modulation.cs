using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// Modulation type
  /// </summary>
  public enum ModulationType
  {
    Unknown = 0,
    Bpsk,
    Qpsk,
    Qam16,
    Qam64,
    Qam256
  }

  /// <summary>
  /// A class emulating Mirelle-style enums
  /// </summary>
  public class Modulation : IMirelleEnum
  {
    /// <summary>
    /// The numerical value
    /// </summary>
    public int Value;

    /// <summary>
    /// The caption
    /// </summary>
    public string Caption;

    /// <summary>
    /// Modulation type
    /// </summary>
    public ModulationType Type;

    public Modulation(int value, string caption)
    {
      Value = value;
      Caption = caption;
      Type = (ModulationType)value;
    }

    /// <summary>
    /// to_s converter
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "modulation:" + Caption;
    }

    /// <summary>
    /// to_a converter
    /// </summary>
    /// <returns></returns>
    public static Modulation[] ToArray()
    {
      var names = new[]
      {
        "unknown",
        "bpsk",
        "qpsk",
        "qam16",
        "qam64",
        "qam256"
      };

      var arr = new Modulation[names.Length];
      for (int idx = 0; idx < names.Length; idx++)
        arr[idx] = new Modulation(idx, names[idx]);

      return arr;
    }

    /// <summary>
    /// unknown
    /// </summary>
    /// <returns></returns>
    public static Modulation Unknown()
    {
      return new Modulation(0, "unknown");
    }

    /// <summary>
    /// BPSC
    /// </summary>
    /// <returns></returns>
    public static Modulation Bpsk()
    {
      return new Modulation(1, "bpsk");
    }

    /// <summary>
    /// QPSC
    /// </summary>
    /// <returns></returns>
    public static Modulation Qpsk()
    {
      return new Modulation(2, "qpsk");
    }

    /// <summary>
    /// 16 QAM
    /// </summary>
    /// <returns></returns>
    public static Modulation Qam16()
    {
      return new Modulation(3, "qam16");
    }

    /// <summary>
    /// 64 QAM
    /// </summary>
    /// <returns></returns>
    public static Modulation Qam64()
    {
      return new Modulation(4, "qam64");
    }

    /// <summary>
    /// 256 QAM
    /// </summary>
    /// <returns></returns>
    public static Modulation Qam256()
    {
      return new Modulation(5, "qam256");
    }

    /// <summary>
    /// Return the block size multiplier for a modulation type
    /// </summary>
    /// <returns></returns>
    public int Multiplier()
    {
      switch(Type)
      {
        case ModulationType.Bpsk: return 1;
        case ModulationType.Qpsk: return 2;
        case ModulationType.Qam16: return 4;
        case ModulationType.Qam64: return 6;
        case ModulationType.Qam256: return 8;
        default: throw new Exception("Modulation type not set!");
      }
    }

    /// <summary>
    /// Return transmission probab
    /// </summary>
    /// <param name="snr">Signal-to-noise relation</param>
    /// <returns></returns>
    public double Probability(double snr)
    {
      switch (Type)
      {
        case ModulationType.Bpsk: return BpskProbability(snr);
        case ModulationType.Qpsk: return QpskProbability(snr);
        case ModulationType.Qam16: return QamProbability(16, snr);
        case ModulationType.Qam64: return QamProbability(64, snr);
        case ModulationType.Qam256: return QamProbability(256, snr);
        default: throw new ArithmeticException();
      }
    }

    /// <summary>
    /// Probability function for BPSK modulation
    /// </summary>
    /// <param name="snr">Signal-to-noise relation</param>
    /// <returns></returns>
    private double BpskProbability(double snr)
    {
      return SpecialFunctions.Erfc(Math.Sqrt(snr)) / 2;
    }

    /// <summary>
    /// Probability function for QPSK modulation
    /// </summary>
    /// <param name="snr">Signal-to-noise relation</param>
    /// <returns></returns>
    private double QpskProbability(double snr)
    {
      var tmp = Math.Sqrt(snr / 2);
      return SpecialFunctions.Erfc(tmp) - (Math.Pow(SpecialFunctions.Erfc(tmp), 2) / 4);
    }

    /// <summary>
    /// Probability function for arbitrary QAM
    /// 
    /// http://www.tlc.polito.it/garello/trasmissione/RG12%20mQAM.pdf
    /// </summary>
    /// <param name="snr">Signal-to-noise relation</param>
    /// <returns></returns>
    private double QamProbability(int m, double snr)
    {
      var sqr_m = Math.Sqrt(m);
      var k = sqr_m;

      var fract1 = (sqr_m - 1) / (sqr_m * k);
      var fract2 = (3 * k * snr) / (2 * (m - 1));

      return 2 * fract1 * SpecialFunctions.Erfc(Math.Sqrt(fract2));
    }
  }
}
