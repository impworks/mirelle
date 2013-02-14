using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MN = MathNet.Numerics.LinearAlgebra.Double;

namespace MirelleStdlib
{
  public class Printer
  {
    /// <summary>
    /// Value to be printed between arguments
    /// </summary>
    static public string ArgumentSeparator = "";

    /// <summary>
    /// Value to be printed after each "println" statement
    /// </summary>
    static public string LineSeparator = "\n";

    static public void Print(IEnumerable<dynamic> a, bool printLine)
    {
      // output items with a possible separator
      var first = true;
      foreach(dynamic curr in a)
      {
        if (!first && ArgumentSeparator != "" && ArgumentSeparator != null)
          Console.Write(ArgumentSeparator);
        else
          first = false;

        PrintAny(curr);
      }

      // print newline separator
      if (printLine)
        Console.Write(LineSeparator);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="printLine"></param>
    static public void Print(dynamic a, bool printLine)
    {
      PrintAny(a);

      // print newline separator
      if (printLine)
        Console.Write(LineSeparator);
    }

    /// <summary>
    /// Print out any object
    /// </summary>
    /// <param name="a">Object</param>
    static public void PrintAny(dynamic a)
    {
      // null given
      if (a == null)
        Console.Write("null");

      // special formatting for a float
      else if (a is double)
        Console.Write("{0:f7}", a);

      // special formatting for a matrix
      else if(a is MN.DenseMatrix)
        PrintMatrix(a as MN.DenseMatrix);

      // special formatting for a dict
      else if(a is Dict)
        PrintDict(a as Dict);

      // wrap array in [ ... ]
      else if (a.GetType().IsArray)
      {
        Console.Write("[ ");
        foreach (dynamic curr in a)
        {
          PrintAny(curr);
          Console.Write(" ");
        }
        Console.Write("]");
      }

      // any other type must have a to_s() or ToString() method
      else
      {
        if (a.GetType().GetMethod("to_s") != null)
          Console.Write(a.to_s());
        else
          Console.Write(a.ToString());
      }
    }

    /// <summary>
    /// Print a matrix
    /// </summary>
    /// <param name="matrix">Matrix</param>
    static private void PrintMatrix(MN.DenseMatrix matrix)
    {
      var padding = 2;
      var rows = matrix.RowCount;
      var cols = matrix.ColumnCount;

      // detect maximum length of each column
      var lengths = new int[cols];
      for(var idx1 = 0; idx1 < cols; idx1++)
      {
        var maxLength = 0;
        for(var idx2 = 0; idx2 < rows; idx2++)
          maxLength = Math.Max(maxLength, matrix[idx2, idx1].ToString().Length);

        lengths[idx1] = maxLength + padding;
      }

      // print matrix
      Console.WriteLine("[[");

      for(var idx1 = 0; idx1 < rows; idx1++)
      {
        for(var idx2 = 0; idx2 < cols; idx2++)
          Console.Write("{0," + lengths[idx2].ToString() + "}", matrix[idx1, idx2]);

        Console.WriteLine();
      }

      Console.WriteLine("]]");
    }

    /// <summary>
    /// Print a dict
    /// </summary>
    /// <param name="dict">Dict to print</param>
    private static void PrintDict(Dict dict)
    {
      var keys = dict.Keys();
      var maxLength = keys.Max(str => str.Length);

      Console.WriteLine("{");

      foreach(var curr in keys)
        Console.WriteLine("  {0,-" +  maxLength.ToString() + "} => {1}", curr, dict.Get(curr));

      Console.WriteLine("}");
    }
  }
}