using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class ThisNode: SyntaxTreeNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return emitter.CurrentType.Name;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      emitter.EmitLoadThis();
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      Error(Resources.errClosuredThis);
      return null;
    }
  }
}
