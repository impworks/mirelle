using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class DummyNode: SyntaxTreeNode
  {
    public DummyNode(string type)
    {
      ExpressionType = type;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // nothing to compile, the node is a dummy substitute
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return ExpressionType;
    }
  }
}
