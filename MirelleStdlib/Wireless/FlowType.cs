using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// A class emulating Mirelle-style enums
  /// </summary>
  public class FlowType: IMirelleEnum
  {
    /// <summary>
    /// The numerical value
    /// </summary>
    public int Value;

    /// <summary>
    /// The caption
    /// </summary>
    public string Caption;

    public FlowType(int value, string caption)
    {
      Value = value;
      Caption = caption;
    }

    /// <summary>
    /// to_s converter
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "flow_type:" + Caption;
    }

    /// <summary>
    /// to_a converter
    /// </summary>
    /// <returns></returns>
    public static FlowType[] ToArray()
    {
      var names = new[]
      {
        "unknown",
        "http",
        "ftp",
        "video",
        "voip"
      };

      var arr = new FlowType[names.Length];
      for (int idx = 0; idx < names.Length; idx++)
        arr[idx] = new FlowType(idx, names[idx]);

      return arr;
    }

    /// <summary>
    /// unknown
    /// </summary>
    /// <returns></returns>
    public static FlowType Unknown()
    {
      return new FlowType(0, "unknown");
    }

    /// <summary>
    /// http
    /// </summary>
    /// <returns></returns>
    public static FlowType Http()
    {
      return new FlowType(1, "http");
    }

    /// <summary>
    /// ftp
    /// </summary>
    /// <returns></returns>
    public static FlowType Ftp()
    {
      return new FlowType(2, "ftp");
    }

    /// <summary>
    /// video
    /// </summary>
    /// <returns></returns>
    public static FlowType Video()
    {
      return new FlowType(3, "video");
    }

    /// <summary>
    /// voip
    /// </summary>
    /// <returns></returns>
    public static FlowType Voip()
    {
      return new FlowType(4, "voip");
    }
  }
}
