using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class StringNode : SyntaxTreeNode, IConstant
  {
    /// <summary>
    /// The string value
    /// </summary>
    public string Value;

    public StringNode(string value)
    {
      Value = value;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "string";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      emitter.EmitLoadString(Value);
    }
  }
}
