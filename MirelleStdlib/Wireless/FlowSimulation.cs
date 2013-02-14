using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MirelleStdlib.Events;
using MirelleStdlib.Extenders;
using System.Numerics;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// The main class that sets up basic simulation class
  /// for performing a wireless-specific simulation
  /// </summary>
  public class FlowSimulation
  {
    /// <summary>
    /// Maximum time to simulate (in virtual time units)
    /// </summary>
    public static double MaxTime = 1000;

    /// <summary>
    /// Transfer speed of the environment
    /// </summary>
    public static int Speed = 256;

    /// <summary>
    /// Number of blocks in a single symbol
    /// </summary>
    public static int BlocksPerSymbol = 16;

    /// <summary>
    /// Block size (in KBPS)
    /// </summary>
    public static int BlockSize = 16;

    /// <summary>
    /// Maximum number of the queued data blocks
    /// </summary>
    public static int MaxQueueSize = 1000;

    /// <summary>
    /// Number of graph points to be averaged
    /// </summary>
    public static int Scale = 1;

    /// <summary>
    /// Array of floats for user to traverse
    /// </summary>
    public static Flow[] Flows = new Flow[0];

    /// <summary>
    /// Transmitter symbol
    /// </summary>
    public static Symbol OldSymbol;

    /// <summary>
    /// Total number of blocks the user attempted to transmit
    /// </summary>
    public static int StatTotalBlocks;

    /// <summary>
    /// Number of successfully transmitter blocks
    /// </summary>
    public static int StatTransmittedBlocks;

    /// <summary>
    /// Number of packets discarded due to queue overflow
    /// </summary>
    public static int StatDiscardedData;

    /// <summary>
    /// Maximal waiting time of a packet in queue
    /// </summary>
    public static double StatMaxWaitingTime;

    /// <summary>
    /// Total waiting time of a packet in the queue
    /// </summary>
    public static double StatTotalWaitingTime;

    /// <summary>
    /// The global planner object
    /// </summary>
    public static Planner Planner;

    /// <summary>
    /// Randomizer
    /// </summary>
    public static Random Rand = new Random();

    /// <summary>
    /// Channel quality
    /// </summary>
    public static double[] ChannelQuality;

    /// <summary>
    /// Process the simulation
    /// </summary>
    /// <param name="planner">Planner instance</param>
    /// <returns></returns>
    public static FlowSimulationResult Start(Planner planner)
    {
      ValidateParameters();
      Init();

      Planner = planner;

      Simulation.AllowTimespans = false;
      Simulation.MakeGraphs = false;
      Simulation.MakeStats = false;

      Simulation.RegisterEvent(new PlannerEvent());
      Simulation.Process(1, 0);

      return CreateSimulationResult();
    }

    /// <summary>
    /// Ensure all parameters are correct
    /// </summary>
    public static void ValidateParameters()
    {
      if (MaxTime <= 0)
        throw new Exception("flow_sim:max_time must be >= 0!");

      if (Speed <= 0)
        throw new Exception("flow_sim:speed must be >= 0!");

      if (BlocksPerSymbol <= 0)
        throw new Exception("flow_sim:blocks_per_symbol must be >= 0!");

      if (BlockSize <= 0)
        throw new Exception("flow_sim:block_size must be >= 0!");

      if (MaxQueueSize <= 0)
        throw new Exception("flow_sim:max_queue must be >= 0!");

      if (Flows.Length == 0)
        throw new Exception("At least one flow must exist in the simulation! Use flow_sim:add to add a flow.");

      if (ChannelQuality == null)
        throw new Exception("Channel quality not set!");
    }

    /// <summary>
    /// Initialize counters
    /// </summary>
    public static void Init()
    {
      // reset all statistics
      StatTotalBlocks = 0;
      StatTransmittedBlocks = 0;
      StatTotalWaitingTime = 0;
      StatDiscardedData = 0;
      StatMaxWaitingTime = 0;

      // clear all flow queues
      foreach (var curr in Flows)
        curr.Reset();
    }

    /// <summary>
    /// Create a simulation result snapshot
    /// </summary>
    /// <returns></returns>
    public static FlowSimulationResult CreateSimulationResult()
    {
      var result = new FlowSimulationResult();

      result.Total = StatTotalBlocks;
      result.Discarded = StatDiscardedData;
      result.FailedBlocks = StatTotalBlocks - StatTransmittedBlocks;
      result.AvgWait = ((double)StatTotalBlocks) / StatTotalWaitingTime;
      result.MaxWait = StatMaxWaitingTime;
      result.AvgSpeed = ((double)StatTransmittedBlocks) / Simulation.Time;
      result.Flows = Flows;

      return result;
    }

    /// <summary>
    /// Register a new flow in the system
    /// </summary>
    /// <param name="flow">Flow to add</param>
    public static void AddFlow(Flow flow)
    {
      Array.Resize(ref Flows, Flows.Length + 1);
      Flows[Flows.Length - 1] = flow;

      Simulation.RegisterEvent(new FlowEvent(flow));
    }

    /// <summary>
    /// Process the symbol, marking it's blocks as 'transfered' or not
    /// </summary>
    /// <param name="symbol"></param>
    public static Symbol ProcessSymbol(Symbol symbol)
    {
      var idx = 0;
      foreach(var block in symbol.Blocks)
      {
        // @TODO: REPLACE
        // FAUX RANDOMIZATION
        if (block.Used)
        {
          block.Transmitted = MathExtender.Random() > block.Modulation.Probability(ChannelQuality[idx]);

          // update statistics
          FlowSimulation.StatTotalBlocks++;
          if (block.Transmitted)
          {
            FlowSimulation.StatTransmittedBlocks++;
            block.Flow.ProcessTransmittedBlock(block);
          }

          block.Flow.Data.Offset = 0;
        }

        idx++;
      }

      return symbol;
    }

    /// <summary>
    /// Pick a random flow from the array according to their priorities
    /// </summary>
    /// <param name="flows">Flow array</param>
    /// <returns></returns>
    static public Flow PickWithPriority(Flow[] flows)
    {
      // check improbable version
      if (flows.Length == 0)
        return null;

      // pick a random flow according to it's priority
      var rnd = Extenders.MathExtender.Random();
      double delta = 1.0 / (double)flows.Sum(flow => flow.QoS);
      var probability = 0.0;

      foreach(var curr in flows)
      {
        probability += (double)curr.QoS * delta;
        if (rnd <= probability) return curr;
      }

      throw new ArithmeticException("Impossible!");
    }

    /// <summary>
    /// Set the channel properties
    /// </summary>
    /// <param name="channel"></param>
    static public void SetChannel(Dict channel)
    {
      // validate channel's properties
      var data = new Complex[BlocksPerSymbol];
      foreach(var curr in channel.Keys())
      {
        int index;
        double value;

        if (!int.TryParse(curr, out index))
          throw new Exception(String.Format("Channel quality indexes must be integers. '{0}' is not a valid integer!", curr));

        if (index >= data.Length)
          throw new Exception("Key exceeds array length");

        if (!double.TryParse(channel.Get(curr), out value))
          throw new Exception(String.Format("Channel quality values must be floats. '{0}' is not a valid float!", curr));

        data[index] = new Complex(value, 0);
      }

      // reverse-transform to get SNRs
      var fft = new MathNet.Numerics.IntegralTransforms.Algorithms.DiscreteFourierTransform();
      fft.BluesteinInverse(data, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
      ChannelQuality = data.Select(p => p.Magnitude).ToArray();
    }
  }
}
