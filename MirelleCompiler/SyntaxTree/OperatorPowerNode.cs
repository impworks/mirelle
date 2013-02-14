using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class OperatorPowerNode : BinaryOperatorNode
  {
    public bool IsInt;

    public void Resolve(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);
      var supportedTypes = new[] { "int", "float" };

      if(!leftType.IsAnyOf(supportedTypes) || !rightType.IsAnyOf(supportedTypes))
        Error(String.Format(Resources.errOperatorTypesMismatch, "**", leftType, rightType));

      IsInt = (leftType == "int" && rightType == "int");
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      Resolve(emitter);
      return IsInt ? "int" : "float";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      Resolve(emitter);

      // operands
      Left.Compile(emitter);
      emitter.EmitConvertToFloat();
      Right.Compile(emitter);
      emitter.EmitConvertToFloat();

      // magic method
      var method = emitter.AssemblyImport(typeof(Math).GetMethod("Pow", new[] { typeof(double), typeof(double) }));
      emitter.EmitCall(method);

      // convert back
      if (IsInt)
        emitter.EmitConvertToInt();
      else
        emitter.EmitConvertToFloat();
    }
  }
}
