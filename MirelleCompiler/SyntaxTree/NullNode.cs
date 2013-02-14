using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class NullNode: SyntaxTreeNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "null";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      emitter.EmitLoadNull();
    }
  }
}
