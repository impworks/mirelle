using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class IntNode: SyntaxTreeNode, IConstant
  {
    /// <summary>
    /// The integer value
    /// </summary>
    public int Value = 0;

    public IntNode(int value)
    {
      Value = value;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "int";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      emitter.EmitLoadInt(Value);
    }
  }
}
