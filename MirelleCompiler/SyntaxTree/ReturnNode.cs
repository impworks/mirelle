using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class ReturnNode: SyntaxTreeNode
  {
    public SyntaxTreeNode Expression;

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (Expression == null) return "void";

      if(ExpressionType == "")
        ExpressionType = Expression.GetExpressionType(emitter);

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var methodType = emitter.CurrentMethod.Type.Signature;
      var exprType = "void";
      if (Expression != null)
      {
        exprType = GetExpressionType(emitter);
        Expression.Compile(emitter);
      }

      if(!emitter.TypeIsParent(methodType, exprType))
        Error(String.Format(Resources.errReturnTypeMismatch, emitter.CurrentMethod.Name, emitter.CurrentMethod.Type.Signature, exprType));
      
      emitter.EmitReturn();
    }
  }
}
