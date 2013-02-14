using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// A group of PDUs bound to their parent flow packets
  /// </summary>
  public class FlattenedQueue<T>
  {
    /// <summary>
    /// The list of items in the queue in format:
    /// key => number of items
    /// </summary>
    public List<KeyValuePair<T, int>> Data = new List<KeyValuePair<T, int>>();

    /// <summary>
    /// Offset in the queue at which new slice should start
    /// </summary>
    public int Offset = 0;

    /// <summary>
    /// Add a new flow packet to the PDU group
    /// </summary>
    /// <param name="time">Key</param>
    /// <param name="count">Number of items</param>
    public void Add(T key, int count)
    {
      Data.Add(new KeyValuePair<T, int>(key, count));
    }

    /// <summary>
    /// Get a part of the queue of given size
    /// and shift the inner Offset value further
    /// </summary>
    /// <param name="size">Number of items to get</param>
    /// <returns></returns>
    public FlattenedQueue<T> GetSlice(int size)
    {
      var result = new FlattenedQueue<T>();
      var idx = 0;
      var pos = 0;
      var blockOffset = 0;
      var sizeLeft = size;

      // check if queue empty
      var max = Size();
      if (Offset == max) return result;

      // rewind to offset
      while(true)
      {
        var item = Data[idx];
        if (pos + item.Value <= Offset)
        {
          pos += item.Value;
          idx++;
        }

        else
        {
          blockOffset = Offset - pos;
          break;
        }
      }

      // output current
      while(sizeLeft > 0 && idx < Data.Count)
      {
        var item = Data[idx];
        var currSize = Math.Min(item.Value - blockOffset, sizeLeft);

        result.Add(item.Key, currSize);

        idx++;
        sizeLeft -= currSize;
        blockOffset = 0;
      }

      // update offset for next operations
      Offset += size - sizeLeft;

      return result;
    }

    /// <summary>
    /// Return the size of the flat queue
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
      var result = 0;
      foreach (var curr in Data)
        result += curr.Value;

      return result;
    }

    /// <summary>
    /// Inspect
    /// </summary>
    public void Inspect()
    {
      Console.WriteLine("Items: {0}, Length: {1}", Data.Count, Size());
    }
  }
}
