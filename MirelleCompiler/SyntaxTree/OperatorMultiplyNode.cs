using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SN = System.Numerics;
using MN = MathNet.Numerics.LinearAlgebra.Generic;

namespace Mirelle.SyntaxTree
{
  public class OperatorMultiplyNode : BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType != "")
        return ExpressionType;

      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      // mathematic operations
      var supportedTypes = new[] { "int", "float", "complex" };
      var matrixTypes = new[] { "int", "float", "matrix" };

      if ("matrix".IsAnyOf(leftType, rightType) && leftType.IsAnyOf(matrixTypes) && rightType.IsAnyOf(matrixTypes))
        ExpressionType = "matrix";

      else if(leftType.EndsWith("[]") && rightType == "int")
        ExpressionType = leftType;

      else if (leftType == "string" && rightType == "int")
        ExpressionType = "string";
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
        Error(String.Format(Resources.errOperatorTypesMismatch, "*", leftType, rightType));

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      var type = GetExpressionType(emitter);

      // repeat a string
      if (type == "string")
      {
        Left.Compile(emitter);
        Right.Compile(emitter);
        emitter.EmitCall(emitter.FindMethod("string", "repeat", "int"));
      }

      // multiply matrices
      else if(type == "matrix")
      {
        var matrixType = typeof(MN.Matrix<double>);

        // matrix by matrix
        if(leftType == rightType)
        {
          Left.Compile(emitter);
          Right.Compile(emitter);

          var method = emitter.AssemblyImport(matrixType.GetMethod("Multiply", new[] { matrixType }));
          emitter.EmitCall(method);
        }
        else
        {
          // matrix should be the first in stack
          if(leftType == "matrix")
          {
            Left.Compile(emitter);
            Right.Compile(emitter);
            if (rightType != "float")
              emitter.EmitConvertToFloat();
          }
          else
          {
            Right.Compile(emitter);
            Left.Compile(emitter);
            if (leftType != "float")
              emitter.EmitConvertToFloat();
          }

          var method = emitter.AssemblyImport(matrixType.GetMethod("Multiply", new[] { typeof(double) }));
          emitter.EmitCall(method);
        }
      }

      // multiply complex numbers
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

        emitter.EmitCall(emitter.AssemblyImport(typeof(SN.Complex).GetMethod("op_Multiply", new[] { typeof(SN.Complex), typeof(SN.Complex) })));
      }

      // repeat array
      else if (leftType.EndsWith("[]"))
      {
        Left.Compile(emitter);
        Right.Compile(emitter);
        var method = emitter.AssemblyImport(typeof(MirelleStdlib.ArrayHelper).GetMethod("RepeatArray", new[] { typeof(object), typeof(int) }));
        emitter.EmitCall(method);
      }

      // multiply floating point numbers or integers
      else if (type.IsAnyOf("int", "float"))
      {
        Left.Compile(emitter);
        emitter.EmitUpcastBasicType(leftType, type);
        Right.Compile(emitter);
        emitter.EmitUpcastBasicType(rightType, type);
        emitter.EmitMul();
      }
    }
  }
}
