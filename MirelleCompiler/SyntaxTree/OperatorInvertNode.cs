using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SN = System.Numerics;

namespace Mirelle.SyntaxTree
{
  public class OperatorInvertNode: SyntaxTreeNode
  {
    public SyntaxTreeNode Expression;

    public OperatorInvertNode(SyntaxTreeNode expr)
    {
      Expression = expr;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return Expression.GetExpressionType(emitter);
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var type = Expression.GetExpressionType(emitter);
      if (type.IsAnyOf("int", "float", "complex"))
      {
        if(type == "complex")
        {
          // simple case: the complex is a constant
          if(Expression is ComplexNode)
          {
            (Expression as ComplexNode).Imaginary *= -1;
            Expression.Compile(emitter);
          }
          // complex case: complex is an expression result
          else
          {
            Expression.Compile(emitter);

            // tmp = ...
            var tmpVar = emitter.CurrentMethod.Scope.Introduce("complex", emitter.FindType("complex").Type);
            emitter.EmitSaveVariable(tmpVar);

            // tmp.real
            emitter.EmitLoadVariableAddress(tmpVar);
            emitter.EmitCall(emitter.AssemblyImport(typeof(SN.Complex).GetMethod("get_Real")));

            // tmp.imaginary * -1
            emitter.EmitLoadVariableAddress(tmpVar);
            emitter.EmitCall(emitter.AssemblyImport(typeof(SN.Complex).GetMethod("get_Imaginary")));
            emitter.EmitLoadFloat(-1);
            emitter.EmitMul();

            // new complex
            emitter.EmitNewObj(emitter.FindMethod("complex", ".ctor", "float", "float"));
          }
        }
        else
        {
          // multiply by -1
          Expression.Compile(emitter);
          if (type == "int")
            emitter.EmitLoadInt(-1);
          else
            emitter.EmitLoadFloat(-1);
          emitter.EmitMul();
        }
      }
      else
        Error(String.Format(Resources.errInvertType, type));
    }
  }
}
