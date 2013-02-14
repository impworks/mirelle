using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MN = MathNet.Numerics.LinearAlgebra.Double;

namespace MirelleStdlib
{
  public class File
  {
    private FileInfo FileInfo;
    private FileStream FileStream;

    public File(string path)
    {
      FileInfo = new FileInfo(path);
    }

    /// <summary>
    /// Remove the file
    /// </summary>
    public void Delete()
    {
      Close();
      FileInfo.Delete();
    }

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <returns></returns>
    public bool Exists()
    {
      return FileInfo.Exists;
    }

    /// <summary>
    /// Get directory name of the file
    /// </summary>
    /// <returns></returns>
    public string DirName()
    {
      return FileInfo.DirectoryName;
    }

    /// <summary>
    /// Get file name
    /// </summary>
    /// <returns></returns>
    public string Name()
    {
      return FileInfo.Name;
    }

    /// <summary>
    /// Get file full name
    /// </summary>
    /// <returns></returns>
    public string FullName()
    {
      return FileInfo.FullName;
    }

    /// <summary>
    /// Get file extension
    /// </summary>
    /// <returns></returns>
    public string Extension()
    {
      return FileInfo.Extension;
    }

    /// <summary>
    /// Get file size
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
      return (int)FileInfo.Length;
    }

    /// <summary>
    /// Open the file for reading and writing
    /// </summary>
    public void Open()
    {
      if (FileStream != null)
        FileStream.Close();
      FileStream = new FileStream(FileInfo.FullName, FileMode.OpenOrCreate);
    }

    /// <summary>
    /// Close the file
    /// </summary>
    public void Close()
    {
      if (FileStream != null)
        FileStream.Close();
      FileStream = null;
    }

    /// <summary>
    /// Create the file (or clear an existing file)
    /// </summary>
    public void Create()
    {
      if(FileStream != null)
        FileStream.Close();
      FileStream = new FileStream(FileInfo.FullName, FileMode.Create);
    }

    /// <summary>
    /// Read a portion of file and return a string in UTF8
    /// </summary>
    /// <param name="size">Chunk size</param>
    /// <returns></returns>
    public string Read(int size)
    {
      if (FileStream == null) Open();
      var buf = new byte[size];
      FileStream.Read(buf, 0, size);
      return System.Text.Encoding.UTF8.GetString(buf);
    }

    /// <summary>
    /// Read the file to the very end
    /// </summary>
    /// <returns></returns>
    public string ReadAll()
    {
      return Read((int)FileStream.Length);
    }

    /// <summary>
    /// Write a string to the file
    /// </summary>
    /// <param name="data">String to write</param>
    public void Write(string data)
    {
      if (FileStream == null) Open();
      var buf = System.Text.Encoding.UTF8.GetBytes(data);
      FileStream.Write(buf, 0, buf.Length);
    }

    /// <summary>
    /// Read the whole file as an array of bytes stored inside integers
    /// </summary>
    /// <returns></returns>
    public int[] LoadBytes()
    {
      if (FileStream == null) Open();

      var count = Size();
      var result = new int[count];
      var buf = new byte[1];

      FileStream.Seek(0, SeekOrigin.Begin);
      for (int idx = 0; idx < count; idx++)
      {
        FileStream.Read(buf, 0, 1);
        result[idx] = buf[0];
      }

      return result;
    }

    /// <summary>
    /// Save an array of bytes to file
    /// </summary>
    /// <param name="values"></param>
    public void SaveBytes(int[] values)
    {
      Create();
      var buf = new byte[1];
      foreach (var curr in values)
      {
        buf[0] = (byte)curr;
        FileStream.Write(buf, 0, buf.Length);
      }
      Close();
    }

    /// <summary>
    /// Read the whole file as an array of floats
    /// </summary>
    /// <returns></returns>
    public bool[] LoadBools()
    {
      if (FileStream == null) Open();

      var typeSize = sizeof(bool);
      var count = (int)Math.Floor((double)Size() / typeSize);
      var result = new bool[count];
      var buf = new byte[typeSize];

      FileStream.Seek(0, SeekOrigin.Begin);
      for (int idx = 0; idx < count; idx++)
      {
        FileStream.Read(buf, 0, typeSize);
        result[idx] = BitConverter.ToBoolean(buf, 0);
      }

      return result;
    }

    /// <summary>
    /// Save an array of booleans to file
    /// </summary>
    /// <param name="values"></param>
    public void SaveBools(bool[] values)
    {
      Create();
      foreach (var curr in values)
      {
        var buf = BitConverter.GetBytes(curr);
        FileStream.Write(buf, 0, buf.Length);
      }
      Close();
    }


    /// <summary>
    /// Read the whole file as an array of integers
    /// </summary>
    /// <returns></returns>
    public int[] LoadInts()
    {
      if (FileStream == null) Open();

      var typeSize = sizeof(int);
      var count = (int)Math.Floor((double)Size() / typeSize);
      var result = new int[count];
      var buf = new byte[typeSize];

      FileStream.Seek(0, SeekOrigin.Begin);
      for(int idx = 0; idx < count; idx++)
      {
        FileStream.Read(buf, 0, typeSize);
        result[idx] = BitConverter.ToInt32(buf, 0);
      }

      return result;
    }

    /// <summary>
    /// Save an array of ints to file
    /// </summary>
    /// <param name="values"></param>
    public void SaveInts(int[] values)
    {
      Create();
      foreach (var curr in values)
      {
        var buf = BitConverter.GetBytes(curr);
        FileStream.Write(buf, 0, buf.Length);
      }
      Close();
    }

    /// <summary>
    /// Read the whole file as an array of floats
    /// </summary>
    /// <returns></returns>
    public double[] LoadFloats()
    {
      if (FileStream == null) Open();

      var typeSize = sizeof(double);
      var count = (int)Math.Floor((double)Size() / typeSize);
      var result = new double[count];
      var buf = new byte[typeSize];

      FileStream.Seek(0, SeekOrigin.Begin);
      for (int idx = 0; idx < count; idx++)
      {
        FileStream.Read(buf, 0, typeSize);
        result[idx] = BitConverter.ToDouble(buf, 0);
      }

      return result;
    }

    /// <summary>
    /// Save an array of floats to file
    /// </summary>
    /// <param name="values"></param>
    public void SaveFloats(double[] values)
    {
      Create();
      foreach (var curr in values)
      {
        var buf = BitConverter.GetBytes(curr);
        FileStream.Write(buf, 0, buf.Length);
      }
      Close();
    }

    /// <summary>
    /// Read the whole file as an array of strings
    /// </summary>
    /// <returns></returns>
    public string[] LoadStrings()
    {
      if (FileStream == null) Open();
      FileStream.Seek(0, SeekOrigin.Begin);
      return ReadAll().Split(new[] { "\r\n" }, StringSplitOptions.None);
    }

    /// <summary>
    /// Save an array of strings to file
    /// </summary>
    /// <param name="values"></param>
    public void SaveStrings(string[] values)
    {
      Create();
      var buf = System.Text.Encoding.UTF8.GetBytes(String.Join("\r\n", values));
      FileStream.Write(buf, 0, buf.Length);
      Close();
    }

    /// <summary>
    /// Read the whole file as a matrix
    /// </summary>
    /// <returns></returns>
    public MN.DenseMatrix LoadMatrix()
    {
      if (FileStream == null) Open();
      FileStream.Seek(0, SeekOrigin.Begin);

      // create buffers
      var lenBuf = new byte[sizeof(int)];
      var buf = new byte[sizeof(double)];
      var count = 0;

      // get matrix dimensions
      count = FileStream.Read(lenBuf, 0, sizeof(int));
      if(count != sizeof(int)) return null;
      var rows = BitConverter.ToInt32(lenBuf, 0);

      count = FileStream.Read(lenBuf, 0, sizeof(int));
      if(count != sizeof(int)) return null;
      var cols = BitConverter.ToInt32(lenBuf, 0);

      // load the matrix
      var matr = new MN.DenseMatrix(rows, cols);
      for(int idx = 0; idx < rows; idx++)
      {
        for(int idx2 = 0; idx2 < cols; idx2++)
        {
          count = FileStream.Read(buf, 0, sizeof(double));
          if (count != sizeof(double)) return null;

          matr[idx, idx2] = BitConverter.ToDouble(buf, 0);
        }
      }

      return matr;
    }

    /// <summary>
    /// Save the matrix to file
    /// </summary>
    /// <param name="matr">Matrix</param>
    public void SaveMatrix(MN.DenseMatrix matr)
    {
      Create();

      // write matrix dimensions
      var lenBuf = BitConverter.GetBytes(matr.RowCount);
      FileStream.Write(lenBuf, 0, lenBuf.Length);

      lenBuf = BitConverter.GetBytes(matr.ColumnCount);
      FileStream.Write(lenBuf, 0, lenBuf.Length);

      // write matrix
      for (int idx = 0; idx < matr.RowCount; idx++)
      {
        for (int idx2 = 0; idx2 < matr.ColumnCount; idx2++)
        {
          var buf = BitConverter.GetBytes(matr[idx, idx2]);
          FileStream.Write(buf, 0, buf.Length);
        }
      }

      Close();
    }

    /// <summary>
    /// Load a dict from file
    /// </summary>
    /// <returns></returns>
    public Dict LoadDict()
    {
      if (FileStream == null) Open();
      FileStream.Seek(0, SeekOrigin.Begin);
      var array = ReadAll().Split(new[] { "\r\n" }, StringSplitOptions.None);

      var dict = new Dict();
      for (int idx = 0; idx < array.Length / 2; idx++)
        dict.Set(array[idx * 2], array[idx * 2 + 1]);

      return dict;
    }

    /// <summary>
    /// Save the dict to file
    /// </summary>
    /// <param name="dict">Dict</param>
    public void SaveDict(Dict dict)
    {
      Create();

      var keys = dict.Keys();
      for(int idx = 0; idx < keys.Length; idx++)
      {
        var str = (idx == 0 ? "" : "\r\n") + keys[idx] + "\r\n" + dict.Get(keys[idx]);
        var buf = System.Text.Encoding.UTF8.GetBytes(str);
        FileStream.Write(buf, 0, buf.Length);
      }

      Close();
    }
  }
}
