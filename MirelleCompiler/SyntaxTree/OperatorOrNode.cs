using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class OperatorOrNode : BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "bool";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // check types
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);
      if (leftType != "bool" || rightType != "bool")
        Error(String.Format(Resources.errOperatorTypesMismatch, "||", leftType, rightType));

      // create labels
      var labelElse = emitter.CreateLabel();
      var labelEnd = emitter.CreateLabel();

      Left.Compile(emitter);
      emitter.EmitBranchFalse(labelElse);
      emitter.EmitLoadBool(true);
      emitter.EmitBranch(labelEnd);

      emitter.PlaceLabel(labelElse);
      Right.Compile(emitter);
      emitter.PlaceLabel(labelEnd);
    }
  }
}
