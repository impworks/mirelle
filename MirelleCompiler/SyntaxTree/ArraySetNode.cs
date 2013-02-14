using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class ArraySetNode : ArrayIndexNode
  {
    /// <summary>
    /// Expression to assign
    /// </summary>
    public SyntaxTreeNode Expression;

    public ArraySetNode(SyntaxTreeNode index = null, SyntaxTreeNode expr = null)
    {
      Index = index;
      Expression = expr;
    }

    public ArraySetNode(ArrayGetNode node)
    {
      ExpressionPrefix = node.ExpressionPrefix;
      Index = node.Index;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var type = ExpressionPrefix.GetExpressionType(emitter);
      if (type == "dict")
        CompileDict(emitter);
      else
        CompileArray(emitter);
    }

    public void CompileDict(Emitter.Emitter emitter)
    {
      // ensure index is a string
      if (Index.GetExpressionType(emitter) != "string")
        Error(Resources.errStringExpected, Index.Lexem);

      if(Expression.GetExpressionType(emitter) != "string")
        Error(Resources.errStringExpected, Expression.Lexem);

      ExpressionPrefix.Compile(emitter);
      Index.Compile(emitter);
      Expression.Compile(emitter);

      var method = emitter.FindMethod("dict", "set", "string", "string");
      emitter.EmitCall(method);
    }

    public void CompileArray(Emitter.Emitter emitter)
    {
      // make sure it's an array
      var type = emitter.GetArrayItemType(ExpressionPrefix.GetExpressionType(emitter));
      if (type == "")
        Error(Resources.errIndexingNotAnArray);

      // make sure expression type matches array type
      var exprType = Expression.GetExpressionType(emitter);
      if (!emitter.TypeIsParent(type, exprType))
        Error(String.Format(Resources.errAssignTypeMismatch, exprType, type));

      ExpressionPrefix.Compile(emitter);
      Index.Compile(emitter);

      if (type == "complex")
      {
        // special case of valuetypes
        var typeRef = emitter.ResolveType(type);
        emitter.EmitLoadIndexAddress(typeRef);
        Expression.Compile(emitter);
        emitter.EmitSaveObject(typeRef);
      }
      else
      {
        Expression.Compile(emitter);
        emitter.EmitSaveIndex(type);
      }
    }
  }
}
