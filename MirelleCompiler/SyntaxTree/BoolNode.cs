using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class BoolNode : SyntaxTreeNode, IConstant
  {
    /// <summary>
    /// The boolean value
    /// </summary>
    public bool Value = false;

    public BoolNode(bool value)
    {
      Value = value;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "bool";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      emitter.EmitLoadBool(Value);
    }
  }
}
