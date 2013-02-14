using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// A logical pack of blocks to be sent via the network simultaneously
  /// </summary>
  public class Symbol
  {
    /// <summary>
    /// Number of blocks in the symbol
    /// </summary>
    public Block[] Blocks;

    public Symbol()
    {
      Blocks = new Block[FlowSimulation.BlocksPerSymbol];
      for (int idx = 0; idx < FlowSimulation.BlocksPerSymbol; idx++)
        Blocks[idx] = new Block(idx);
    }
  }
}
