using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// The result holder for a simulation
  /// </summary>
  public class FlowSimulationResult
  {
    /// <summary>
    /// Total number of blocks processed
    /// </summary>
    public int Total;

    /// <summary>
    /// Number of blocks which failed at transmission
    /// </summary>
    public int FailedBlocks;

    /// <summary>
    /// Number of discarded packets due to queue overflow
    /// </summary>
    public int Discarded;

    /// <summary>
    /// Average waiting time of a packet
    /// </summary>
    public double AvgWait;

    /// <summary>
    /// Maximum waiting time of a packet
    /// </summary>
    public double MaxWait;

    /// <summary>
    /// Average speed of data transfer
    /// </summary>
    public double AvgSpeed;

    /// <summary>
    /// The flows that have been processed during the simulation
    /// </summary>
    public Flow[] Flows;
  }
}
