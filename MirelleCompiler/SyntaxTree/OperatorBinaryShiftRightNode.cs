using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class OperatorBinaryShiftRightNode : BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "int";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);
      if (leftType != "int" || rightType != "int")
        Error(String.Format(Resources.errOperatorTypesMismatch, ">>", leftType, rightType));

      Left.Compile(emitter);
      Right.Compile(emitter);
      emitter.EmitShiftRight();
    }
  }
}
