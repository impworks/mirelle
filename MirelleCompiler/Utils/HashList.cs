using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle
{
  public class HashList<T>: IEnumerable
  {
    public Dictionary<string, T> Data;
    public List<string> Keys;

    public HashList()
    {
      Data = new Dictionary<string, T>();
      Keys = new List<string>();
    }

    /// <summary>
    /// Add an item to the collection
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Object</param>
    public void Add(string key, T value)
    {
      Data.Add(key, value);
      Keys.Add(key);
    }

    /// <summary>
    /// Remove everything from the collection
    /// </summary>
    public void Clear()
    {
      Data.Clear();
      Keys.Clear();
    }

    /// <summary>
    /// Check if a key exists
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public bool Contains(string key)
    {
      return Data.ContainsKey(key);
    }

    /// <summary>
    /// Get an item by string key
    /// </summary>
    /// <param name="key">String key</param>
    /// <returns></returns>
    public T this [string key]
    {
      get
      {
        return Data[key];
      }

      set
      {
        Data[key] = value;
      }
    }

    /// <summary>
    /// Get an item by integer index
    /// </summary>
    /// <param name="id">Integer index</param>
    /// <returns></returns>
    public T this [int id]
    {
      get
      {
        return Data[Keys[id]];
      }

      set
      {
        Data[Keys[id]] = value;
      }
    }

    /// <summary>
    /// Proxied count
    /// </summary>
    public int Count
    {
      get { return Keys.Count; }
    }

    /// <summary>
    /// Proxied enumerator
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return Keys.GetEnumerator();
    }

    public IEnumerator<string> GetEnumerator()
    {
      return Keys.GetEnumerator();
    }
  }
}
