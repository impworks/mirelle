using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class FloatNode : SyntaxTreeNode, IConstant
  {
    /// <summary>
    /// The floating point value
    /// </summary>
    public double Value = 0;

    public FloatNode(double value)
    {
      Value = value;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "float";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      emitter.EmitLoadFloat(Value);
    }
  }
}
