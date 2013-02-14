using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// A single block within a transmittable symbol
  /// </summary>
  public class Block
  {
    /// <summary>
    /// Block ID within the symbol
    /// </summary>
    private int BlockID = 0;

    /// <summary>
    /// Block was used
    /// </summary>
    public bool Used = false;

    /// <summary>
    /// Data was transfered
    /// </summary>
    public bool Transmitted = false;

    /// <summary>
    /// List of flows which data has been used in the block
    /// </summary>
    public Flow Flow;

    /// <summary>
    /// Modulation type
    /// </summary>
    public Modulation Modulation = Modulation.Unknown();

    /// <summary>
    /// Group of data transmitted within the block
    /// </summary>
    public FlattenedQueue<double> Data;

    /// <summary>
    /// Misc data tag
    /// </summary>
    public string Tag;

    public Block(int id)
    {
      BlockID = id;
    }

    /// <summary>
    /// Mark the block as used and request N blocks from flow queue
    /// </summary>
    /// <param name="flow">Flow to use</param>
    public void Use(Flow flow)
    {
      Use(flow, Modulation.Bpsk());
    }

    /// <summary>
    /// Mark the block as used, update the modulation type
    /// and request N blocks from flow queue
    /// </summary>
    /// <param name="flow"></param>
    /// <param name="modulation"></param>
    public void Use(Flow flow, Modulation modulation)
    {
      if (Used)
        throw new Exception("The block is already used!");

      Modulation = modulation;
      Used = true;
      Flow = flow;

      if (flow == null)
        throw new Exception("No flow!");

      if (flow.Data == null)
        throw new Exception("no data!");

      Data = flow.Data.GetSlice(Size());
    }

    /// <summary>
    /// Return the quality indicator for the block
    /// </summary>
    /// <returns></returns>
    public double Quality()
    {
      return FlowSimulation.ChannelQuality[BlockID];
    }

    /// <summary>
    /// Return the block size in PDUs according to modulation;
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
      return FlowSimulation.BlockSize * Modulation.Multiplier();
    }

    /// <summary>
    /// Return the amout of data put into the block
    /// </summary>
    /// <returns></returns>
    public int DataSize()
    {
      if (Data == null)
        return 0;
      else
        return Data.Size();
    }
  }
}
