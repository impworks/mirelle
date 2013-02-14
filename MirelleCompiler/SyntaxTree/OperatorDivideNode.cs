using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SN = System.Numerics;
using MN = MathNet.Numerics.LinearAlgebra.Generic;

namespace Mirelle.SyntaxTree
{
  public class OperatorDivideNode : BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType != "")
        return ExpressionType;

      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      var supportedTypes = new[] { "int", "float", "complex" };

      // matrix / numeric division
      if (leftType == "matrix" && rightType.IsAnyOf("int", "float"))
        ExpressionType = "matrix";

      // mathematic operations
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
        Error(String.Format(Resources.errOperatorTypesMismatch, "/", leftType, rightType));

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      var type = GetExpressionType(emitter);

      // divide matrix by a number
      if(type == "matrix")
      {
        Left.Compile(emitter);

        // division is broken, therefore multiply by an inverse value
        emitter.EmitLoadFloat(1);
        Right.Compile(emitter);

        if (rightType != "float")
          emitter.EmitConvertToFloat();

        emitter.EmitDiv();

        var matrixType = typeof(MN.Matrix<double>);
        var method = emitter.AssemblyImport(matrixType.GetMethod("Multiply", new[] { typeof(double) }));
        emitter.EmitCall(method);
      }

      // divide complex numbers
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

        emitter.EmitCall(emitter.AssemblyImport(typeof(SN.Complex).GetMethod("op_Division", new[] { typeof(SN.Complex), typeof(SN.Complex) })));
      }

      // divide floating point numbers or integers
      else if (type.IsAnyOf("int", "float"))
      {
        Left.Compile(emitter);
        emitter.EmitUpcastBasicType(leftType, type);
        Right.Compile(emitter);
        emitter.EmitUpcastBasicType(rightType, type);
        emitter.EmitDiv();
      }
    }
  }
}
