using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MN = MathNet.Numerics.LinearAlgebra.Double;

namespace MirelleStdlib.Extenders
{
  public class MatrixExtender
  {
    /// <summary>
    /// Create a square identity matrix
    /// </summary>
    /// <param name="size">Height and width</param>
    /// <returns></returns>
    public static MN.DenseMatrix Identity(int size)
    {
      var matrix = new MN.DenseMatrix(size);
      for (int idx = 0; idx < size; idx++)
        matrix[idx, idx] = 1;

      return matrix;
    }

    /// <summary>
    /// Invert a matrix if it can be inverted
    /// </summary>
    /// <param name="matrix">Matrix to invert</param>
    /// <returns></returns>
    public static MN.DenseMatrix Invert(MN.DenseMatrix matrix)
    {
      if (matrix.Determinant() == 0)
        return null;
      else
        return (MN.DenseMatrix)matrix.Inverse();
    }

    /// <summary>
    /// Return a row of the matrix as an array
    /// </summary>
    /// <param name="matrix">Matrix</param>
    /// <param name="id">Row id</param>
    /// <returns></returns>
    public static double[] Row(MN.DenseMatrix matrix, int id)
    {
      var row = new double[matrix.ColumnCount];
      for (int idx = 0; idx < matrix.ColumnCount; idx++)
        row[idx] = matrix[id, idx];

      return row;
    }

    /// <summary>
    /// Return a column of the matrix as an array
    /// </summary>
    /// <param name="matrix">Matrix</param>
    /// <param name="id">Column id</param>
    /// <returns></returns>
    public static double[] Column(MN.DenseMatrix matrix, int id)
    {
      var col = new double[matrix.RowCount];
      for (int idx = 0; idx < matrix.RowCount; idx++)
        col[idx] = matrix[idx, id];

      return col;
    }

    /// <summary>
    /// Flatten the matrix down to an array
    /// </summary>
    /// <param name="matrix">Matrix</param>
    /// <returns></returns>
    public static double[] Flatten(MN.DenseMatrix matrix)
    {
      var array = new double[matrix.RowCount * matrix.ColumnCount];

      var idx3 = 0;
      for (int idx1 = 0; idx1 < matrix.RowCount; idx1++)
      {
        for (int idx2 = 0; idx2 < matrix.ColumnCount; idx2++)
        {
          array[idx3] = matrix[idx1, idx2];
          idx3++;
        }
      }

      return array;
    }


    /// <summary>
    /// Inflate a matrix out of an array
    /// </summary>
    /// <param name="values">Array of flatten values</param>
    /// <param name="height">Desired matrix height</param>
    /// <returns></returns>
    public static MN.DenseMatrix Inflate(double[] values, int width)
    {
      var height = values.Length / width;
      var matrix = new MN.DenseMatrix(height, width);

      // fill the matrix
      var idx3 = 0;
      for (int idx1 = 0; idx1 < height; idx1++)
      {
        for (int idx2 = 0; idx2 < width; idx2++)
        {
          matrix[idx1, idx2] = values[idx3];
          idx3++;
        }
      }

      return matrix;
    }

    /// <summary>
    /// Perform a LU factorization of the matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static MN.DenseMatrix[] FactorLU(MN.DenseMatrix matrix)
    {
      try
      {
        var lu = new MN.Factorization.DenseLU(matrix);
        return new MN.DenseMatrix[] { lu.L as MN.DenseMatrix, lu.U as MN.DenseMatrix };
      }
      catch
      {
        return null;
      }
    }


    /// <summary>
    /// Perform a QR factorization of the matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static MN.DenseMatrix[] FactorQR(MN.DenseMatrix matrix)
    {
      try
      {
        var qr = new MN.Factorization.DenseQR(matrix);
        return new MN.DenseMatrix[] { qr.Q as MN.DenseMatrix, qr.R as MN.DenseMatrix };
      }
      catch
      {
        return null;
      }
    }


    /// <summary>
    /// Perform a SVD factorization of the matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static MN.DenseMatrix FactorCholessky(MN.DenseMatrix matrix)
    {
      try
      {
        var cholessky = new MN.Factorization.DenseCholesky(matrix);
        return cholessky.Factor as MN.DenseMatrix;
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Generate a matrix based on Pascal Triangle values
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static MN.DenseMatrix Pascal(int size)
    {
      if (size <= 0) return null;

      var matrix = new MN.DenseMatrix(size, size, 1);

      for (int idx1 = 1; idx1 < size; idx1++)
        for (int idx2 = 1; idx2 < size; idx2++)
          matrix[idx1, idx2] = matrix[idx1 - 1, idx2] + matrix[idx1, idx2 - 1];

      return matrix;
    }
  }
}
