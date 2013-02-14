using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib
{
  public class Dict
  {
    /// <summary>
    /// Dictionary enumerator
    /// </summary>
    private IEnumerator<KeyValuePair<string, string>> Enumerator = null;

    /// <summary>
    /// Dictionary
    /// </summary>
    private Dictionary<string, string> Data = new Dictionary<string, string>();

    /// <summary>
    /// Get a value
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public string Get(string key)
    {
      if (Data.ContainsKey(key))
        return Data[key];
      else
        return null;
    }

    /// <summary>
    /// Set a value
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public void Set(string key, string value)
    {
      if (Data.ContainsKey(key))
        Data[key] = value;
      else
        Data.Add(key, value);
    }

    /// <summary>
    /// Remove a record from the dictionary
    /// </summary>
    /// <param name="key"></param>
    public void Unset(string key)
    {
      if(Data.ContainsKey(key))
        Data.Remove(key);
    }

    /// <summary>
    /// Get the number of items in the dictionary
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
      return Data.Count;
    }

    /// <summary>
    /// Check if the dictionary contains a value
    /// </summary>
    /// <param name="value">Value</param>
    /// <returns></returns>
    public bool HasValue(string value)
    {
      return Data.ContainsValue(value);
    }

    /// <summary>
    /// Check if the dictionary contains a key
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public bool Has(string key)
    {
      return Data.ContainsKey(key);
    }

    /// <summary>
    /// Return all keys as an array of strings
    /// </summary>
    /// <returns></returns>
    public string[] Keys()
    {
      return Data.Keys.ToArray();
    }

    /// <summary>
    /// Return all values as an array of strings
    /// </summary>
    /// <returns></returns>
    public string[] Values()
    {
      return Data.Values.ToArray();
    }

    /// <summary>
    /// Reset the value
    /// </summary>
    public void Reset()
    {
      Enumerator = (Data as IEnumerable<KeyValuePair<string, string>>).GetEnumerator();
    }

    /// <summary>
    /// Return the new key and value
    /// </summary>
    /// <returns></returns>
    public string[] Current()
    {
      if(Enumerator == null)
        throw new InvalidOperationException();

      var pair = Enumerator.Current;
      return new[] { pair.Key, pair.Value };
    }

    /// <summary>
    /// Shift to the next value
    /// </summary>
    /// <returns></returns>
    public bool Next()
    {
      if (Enumerator == null)
        throw new InvalidOperationException();

      return Enumerator.MoveNext();
    }

    /// <summary>
    /// Add two dicts
    /// </summary>
    /// <param name="dict">Other dict</param>
    /// <returns></returns>
    public Dict Add(Dict dict)
    {
      var result = new Dict();

      foreach (var curr in Data)
        result.Set(curr.Key, curr.Value);

      foreach (var curr in dict.Data)
        result.Set(curr.Key, curr.Value);

      return result;
    }

    /// <summary>
    /// Subtract one dict from another
    /// </summary>
    /// <param name="dict">Other dict</param>
    /// <returns></returns>
    public Dict Subtract(Dict dict)
    {
      var result = new Dict();

      foreach (var curr in Data)
        result.Set(curr.Key, curr.Value);

      foreach (var curr in dict.Data)
        result.Unset(curr.Key);

      return result;
    }
  }
}
