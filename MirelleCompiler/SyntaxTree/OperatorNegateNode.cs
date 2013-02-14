using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class OperatorNegateNode: SyntaxTreeNode
  {
    public SyntaxTreeNode Expression;

    public OperatorNegateNode(SyntaxTreeNode expr)
    {
      Expression = expr;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "bool";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      if (Expression.GetExpressionType(emitter) != "bool")
        Error(Resources.errBoolExpected);

      Expression.Compile(emitter);
      emitter.EmitLoadBool(false);
      emitter.EmitCompareEqual();
    }
  }
}
