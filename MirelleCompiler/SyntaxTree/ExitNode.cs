using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mirelle.SyntaxTree
{
  public class ExitNode: SyntaxTreeNode
  {
    public override void Compile(Emitter.Emitter emitter)
    {
      var method = typeof(Environment).GetMethod("Exit", new[] { typeof(int) });
      emitter.EmitLoadInt(0);
      emitter.EmitCall(emitter.AssemblyImport(method));
    }
  }
}
