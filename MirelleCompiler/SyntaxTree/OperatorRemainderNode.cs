using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class OperatorRemainderNode : BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType != "")
        return ExpressionType;

      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      // mathematic operations
      var supportedTypes = new[] { "int", "float" };
      if (leftType.IsAnyOf(supportedTypes) && rightType.IsAnyOf(supportedTypes))
      {
        if ("float".IsAnyOf(leftType, rightType))
          ExpressionType = "float";
        else
          ExpressionType = "int";
      }
      else
        Error(String.Format(Resources.errOperatorTypesMismatch, "%", leftType, rightType));

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      var type = GetExpressionType(emitter);

      Left.Compile(emitter);
      emitter.EmitUpcastBasicType(leftType, type);
      Right.Compile(emitter);
      emitter.EmitUpcastBasicType(rightType, type);
      emitter.EmitRem();
    }
  }
}
