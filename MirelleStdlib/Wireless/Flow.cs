using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using MirelleStdlib.Events;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// A data flow defined by user that generates data
  /// to be transmitted via wireless network
  /// and stores statistics
  /// </summary>
  public class Flow
  {
    /// <summary>
    /// Flow type
    /// </summary>
    public FlowType FlowType;

    /// <summary>
    /// Quality of service
    /// </summary>
    public int QoS;

    /// <summary>
    /// Desired speed
    /// </summary>
    public int Speed;

    /// <summary>
    /// Desired pack size
    /// </summary>
    public int PacketSize;

    /// <summary>
    /// Total wait of all packets
    /// </summary>
    public double StatLocalWait;

    /// <summary>
    /// Total number of packets transmitted
    /// </summary>
    public int StatLocalPackets;

    /// <summary>
    /// List of waiting times
    /// </summary>
    public List<Complex> StatWaitGraph;

    /// <summary>
    /// Queue of PDU data
    /// </summary>
    public FlattenedQueue<double> Data = new FlattenedQueue<double>();

    public Flow(FlowType type, int speed, int packsize)
    {
      FlowType = type;
      Speed = speed;
      PacketSize = packsize;

      Reset();
    }

    public Flow(FlowType type, int speed, int packsize, int qos):this(type, speed, packsize)
    {
      QoS = qos;
    }

    /// <summary>
    /// Remove data from the queue if necessary and update statistics on the fly
    /// </summary>
    /// <param name="block"></param>
    public void ProcessTransmittedBlock(Block block)
    {
      foreach(var curr in block.Data.Data)
      {
        var pos = Data.Data.FindIndex(p => p.Key == curr.Key);
        var item = Data.Data[pos];

        if (item.Value > curr.Value)
        {
          // only a part of the PDUs have been transferred
          Data.Data[pos] = new KeyValuePair<double, int>(item.Key, item.Value - curr.Value);
        }
        else
        {
          // the last PDU of the packet has been transferred
          Data.Data.RemoveAt(pos);

          // collect stats
          StatLocalWait += Simulation.Time - item.Key;
          StatLocalPackets++;

          // add a point to graph if current 
          if (StatLocalPackets == FlowSimulation.Scale)
          {
            // old value at current time to create immediate changes
            var avg = StatLocalWait / (double)StatLocalPackets;
            var pt1 = new Complex(StatWaitGraph.Count == 0 ? 0 : StatWaitGraph[StatWaitGraph.Count - 1].Real, avg);
            var pt2 = new Complex(Simulation.Time, avg);

            StatWaitGraph.Add(pt1);
            StatWaitGraph.Add(pt2);

            StatLocalWait = 0;
            StatLocalPackets = 0;
          }
        }
      }
    }

    /// <summary>
    /// Return the number of PDUs in the queue
    /// </summary>
    /// <returns></returns>
    public int QueueSize()
    {
      return Data.Size();
    }

    /// <summary>
    /// Reset flow data before simulation
    /// </summary>
    public void Reset()
    {
      StatLocalPackets = 0;
      StatLocalWait = 0;
      StatWaitGraph = new List<Complex>();

      Data.Data.Clear();
    }

    /// <summary>
    /// Get average waiting time
    /// </summary>
    /// <returns></returns>
    public double AvgWait()
    {
      return StatWaitGraph.Average(p => p.Imaginary);
    }

    /// <summary>
    /// Get waiting time graph
    /// </summary>
    /// <returns></returns>
    public Complex[] WaitGraph()
    {
      return StatWaitGraph.ToArray();
    }
  }
}
