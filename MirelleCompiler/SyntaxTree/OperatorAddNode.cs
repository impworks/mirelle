using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MirelleStdlib;
using SN = System.Numerics;
using MN = MathNet.Numerics.LinearAlgebra.Generic;

namespace Mirelle.SyntaxTree
{
  public class OperatorAddNode: BinaryOperatorNode
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType != "")
        return ExpressionType;

      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      var supportedTypes = new[] { "int", "float", "complex" };

      // concat strings
      if (leftType == "string" && rightType == "string")
        ExpressionType = "string";

      // add matrices
      else if (leftType == "matrix" && rightType == "matrix")
        ExpressionType = "matrix";

      // add dicts
      else if (leftType == "dict" && rightType == "dict")
        ExpressionType = "dict";

      // add arrays
      else if(leftType == rightType && leftType.Contains("[]"))
        ExpressionType = leftType;

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
        Error(String.Format(Resources.errOperatorTypesMismatch, "+", leftType, rightType));

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      var type = GetExpressionType(emitter);

      // concat strings
      if (type == "string")
      {
        Left.Compile(emitter);
        Right.Compile(emitter);
        emitter.EmitCall(emitter.FindMethod("string", "concat", "string", "string"));
      }

      // add matrices
      else if(type == "matrix")
      {
        Left.Compile(emitter);
        Right.Compile(emitter);

        var matrixType = typeof(MN.Matrix<double>);
        var method = emitter.AssemblyImport(matrixType.GetMethod("Add", new [] { matrixType } ));
        emitter.EmitCall(method);
      }

      // add dicts
      else if (type == "dict")
      {
        Left.Compile(emitter);
        Right.Compile(emitter);

        var dictType = typeof(Dict);
        var method = emitter.AssemblyImport(dictType.GetMethod("Add", new[] { dictType }));
        emitter.EmitCall(method);
      }

      // add complex numbers
      else if(type == "complex")
      {
        Left.Compile(emitter);
        if(leftType != "complex")
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

        emitter.EmitCall(emitter.AssemblyImport(typeof(SN.Complex).GetMethod("op_Addition", new[] { typeof(SN.Complex), typeof(SN.Complex) })));
      }

      // add floating point numbers or integers
      else if(type.IsAnyOf("int", "float"))
      {
        Left.Compile(emitter);
        emitter.EmitUpcastBasicType(leftType, type);
        Right.Compile(emitter);
        emitter.EmitUpcastBasicType(rightType, type);
        emitter.EmitAdd();
      }

      // array addition
      else if(type.Contains("[]"))
      {
        Left.Compile(emitter);
        Right.Compile(emitter);

        var method = emitter.AssemblyImport(typeof(ArrayHelper).GetMethod("AddArrays", new[] { typeof(object), typeof(object) } ));
        emitter.EmitCall(method);
      }
    }
  }
}
