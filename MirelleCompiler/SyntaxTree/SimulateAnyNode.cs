using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class SimulateAnyNode: SyntaxTreeNode
  {
    /// <summary>
    /// Number of processors
    /// </summary>
    public SyntaxTreeNode Processors;

    /// <summary>
    /// Maximum queue length
    /// </summary>
    public SyntaxTreeNode MaxQueue;

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "sim_result";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var method = emitter.AssemblyImport(typeof(MirelleStdlib.Events.Simulation).GetMethod("Process", new[] { typeof(int), typeof(int) }));

      // processor count
      if(Processors == null)
        emitter.EmitLoadInt(1);
      else
      {
        if(Processors.GetExpressionType(emitter) != "int")
          Error(Resources.errSimulateProcessorsInt);

        Processors.Compile(emitter);
      }

      // queue length
      if(MaxQueue == null)
        emitter.EmitLoadInt(0);
      else
      {
        if(MaxQueue.GetExpressionType(emitter) != "int")
          Error(Resources.errSimulateQueueInt);

        MaxQueue.Compile(emitter);
      }

      emitter.EmitCall(method);
    }
  }
}
