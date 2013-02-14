using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib
{
  public class Range
  {
    /// <summary>
    /// Starting value
    /// </summary>
    private int _From;

    /// <summary>
    /// Ending value
    /// </summary>
    private int _To;

    /// <summary>
    /// Current value
    /// </summary>
    private int _Current;

    /// <summary>
    /// Iteration direction (1 or -1)
    /// </summary>
    private int Direction;

    /// <summary>
    /// Flag indicating the iteration has started
    /// </summary>
    private bool InRange = false;

    public Range(int from, int to)
    {
      _From = from;
      _Current = from - 1;
      _To = to;
      Direction = _From < _To ? 1 : -1;
    }

    /// <summary>
    /// Reset iteration to the pre-started value
    /// </summary>
    public void Reset()
    {
      _Current = _From - 1;
      InRange = false;
    }

    /// <summary>
    /// Get current value of the range
    /// </summary>
    /// <returns></returns>
    public int Current()
    {
      if (InRange)
        return _Current;
      else
        throw new InvalidOperationException();
    }

    /// <summary>
    /// Step to next value
    /// </summary>
    /// <returns></returns>
    public bool Next()
    {
      InRange = true;
      _Current += Direction;
      return Direction == 1 ? _Current <= _To : _Current >= _To;
    }

    /// <summary>
    /// Check if the range contains the value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Has(double value)
    {
      if(Direction == 1)
        return value >= _From && value <= _To;
      else
        return value >= _To && value <= _From;
    }

    /// <summary>
    /// Get the number of values in the range
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
      return Math.Abs(_From - _To + 1);
    }

    /// <summary>
    /// Get string representation
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _From.ToString() + ".." + _To.ToString();
    }

    /// <summary>
    /// Get all items as an array
    /// </summary>
    /// <returns></returns>
    public int[] ToArray()
    {
      return ArrayHelper.CreateRangedArray(_From, _To);
    }

    /// <summary>
    /// Get starting value
    /// </summary>
    /// <returns></returns>
    public int From()
    {
      return _From;
    }

    /// <summary>
    /// Get ending value
    /// </summary>
    /// <returns></returns>
    public int To()
    {
      return _To;
    }
  }
}
