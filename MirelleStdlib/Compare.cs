using System;
using System.Collections;
using System.Linq;
using System.Text;
using MN = MathNet.Numerics.LinearAlgebra.Double;

namespace MirelleStdlib
{
  public class Compare
  {
    static public bool Equal(dynamic a, dynamic b)
    {
      // null cases
      if (a == null && b == null) return true;
      if (a == null ^ b == null) return false;

      // deep compare arrays
      if (a.GetType().IsArray)
      {
        if (a.Length != b.Length) return false;

        IEnumerator iter = b.GetEnumerator();
        iter.MoveNext();
        foreach (dynamic curr in a)
        {
          if (!Equal(curr, iter.Current))
            return false;

          iter.MoveNext();
        }

        return true;
      }
      else if (a is MN.DenseMatrix)
        return MatrixEqual(a as MN.DenseMatrix, b as MN.DenseMatrix);
      else if (a is IMirelleEnum)
        return a.value == b.value;
      else if (a is IMirelleType)
      {
        if (a.GetType().GetMethod("equal", new Type[] { b.GetType() }) != null)
          return a.equal(b);
        else
          return b.equal(a);
      }
      else if (a.GetType() != b.GetType())
        return false;
      else
        return a == b;
    }

    static public bool MatrixEqual(MN.DenseMatrix a, MN.DenseMatrix b)
    {
      if (a.RowCount != b.RowCount || a.ColumnCount != b.ColumnCount)
        return false;

      for (int idx1 = 0; idx1 < a.RowCount; idx1++)
        for (int idx2 = 0; idx2 < a.ColumnCount; idx2++)
          if (a[idx1, idx2] != b[idx1, idx2])
            return false;

      return true;
    }
  }
}
