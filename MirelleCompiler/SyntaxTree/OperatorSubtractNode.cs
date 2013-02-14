using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MirelleStdlib;
using SN = System.Numerics;
using MN = MathNet.Numerics.LinearAlgebra.Generic;

namespace Mirelle.SyntaxTree
{
  public class OperatorSubtractNode : BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType != "")
        return ExpressionType;

      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      // mathematic operations
      var supportedTypes = new[] { "int", "float", "complex" };

      // subtract matrices
      if (leftType == "matrix" && rightType == "matrix")
        ExpressionType = "matrix";

      // subtract dicts
      else if (leftType == "dict" && rightType == "dict")
        ExpressionType = "dict";

      else if (leftType.IsAnyOf(supportedTypes) && rightType.IsAnyOf(supportedTypes))
      {
        if ("complex".IsAnyOf(leftType, rightType))
          ExpressionType = "complex";
        else if ("float".IsAnyOf(leftType, rightType))
          ExpressionType = "float";
        else
          ExpressionType = "int";
      }
      else
        Error(String.Format(Resources.errOperatorTypesMismatch, "-", leftType, rightType));

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      var type = GetExpressionType(emitter);

      // subtract matrices
      if(type == "matrix")
      {
        Left.Compile(emitter);
        Right.Compile(emitter);

        var matrixType = typeof(MN.Matrix<double>);
        var method = emitter.AssemblyImport(matrixType.GetMethod("Subtract", new [] { matrixType } ));
        emitter.EmitCall(method);
      }

      // subtract dicts
      else if (type == "dict")
      {
        Left.Compile(emitter);
        Right.Compile(emitter);

        var dictType = typeof(Dict);
        var method = emitter.AssemblyImport(dictType.GetMethod("Subtract", new[] { dictType }));
        emitter.EmitCall(method);
      }

      // subtract complex numbers
      else if (type == "complex")
      {
        Left.Compile(emitter);
        if (leftType != "complex")
        {
          emitter.EmitUpcastBasicType(leftType, "float");
          emitter.EmitLoadFloat(0);
          emitter.EmitNewObj(emitter.FindMethod("complex", ".ctor", "float", "float"));
        }

        Right.Compile(emitter);
        if (rightType != "complex")
        {
          emitter.EmitUpcastBasicType(rightType, "float");
          emitter.EmitLoadFloat(0);
          emitter.EmitNewObj(emitter.FindMethod("complex", ".ctor", "float", "float"));
        }

        emitter.EmitCall(emitter.AssemblyImport(typeof(SN.Complex).GetMethod("op_Subtraction", new[] { typeof(SN.Complex), typeof(SN.Complex) })));
      }
      // add floating point numbers or integers
      else if (type.IsAnyOf("int", "float"))
      {
        Left.Compile(emitter);
        emitter.EmitUpcastBasicType(leftType, type);
        Right.Compile(emitter);
        emitter.EmitUpcastBasicType(rightType, type);
        emitter.EmitSub();
      }
    }
  }
}
