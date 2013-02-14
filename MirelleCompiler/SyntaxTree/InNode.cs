using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class InNode: BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "bool";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      // an array of values
      if (rightType == leftType + "[]")
      {
        Right.Compile(emitter);
        Left.Compile(emitter);
        if (leftType.IsAnyOf("int", "bool", "float", "complex"))
          emitter.EmitBox(emitter.ResolveType(leftType));

        var method = typeof(MirelleStdlib.ArrayHelper).GetMethod("Has", new[] { typeof(object), typeof(object) });
        emitter.EmitCall(emitter.AssemblyImport(method));
      }

      // an object has a "has" method that accepts the lefthand expression
      else
      {
        try
        {
          Expr.IdentifierInvoke("has", Right, Left).Compile(emitter);
          return;
        }
        catch { }

        Error(String.Format(Resources.errOperatorTypesMismatch, "in", leftType, rightType));
      }
    }
  }
}
