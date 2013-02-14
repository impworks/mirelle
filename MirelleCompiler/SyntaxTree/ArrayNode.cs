using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirelle.SyntaxTree
{
  public class ArrayNode: SyntaxTreeNode
  {
    /// <summary>
    /// Array items
    /// </summary>
    public List<SyntaxTreeNode> Values = new List<SyntaxTreeNode>();

    public ArrayNode(params SyntaxTreeNode[] items)
    {
      foreach (var curr in items)
        Values.Add(curr);
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType == "")
      {
        if (Values.Count > 0)
          ExpressionType = Values[0].GetExpressionType(emitter) + "[]";
      }

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var typeName = Values[0].GetExpressionType(emitter);
      if (typeName == "null")
        Error(Resources.errArrayItemNull, Values[0].Lexem);
      if (typeName.IsAnyOf("", "void"))
        Error(Resources.errVoidExpression, Values[0].Lexem);

      TypeReference type;
      try
      {
        type = emitter.ResolveType(typeName);
      }
      catch (CompilerException ex)
      {
        ex.AffixToLexem(Values[0].Lexem);
        throw;
      }
      var tmpVariable = emitter.CurrentMethod.Scope.Introduce(typeName, emitter.ResolveType(typeName + "[]"));

      // load count & create
      emitter.EmitLoadInt(Values.Count);
      emitter.EmitNewArray(type);
      emitter.EmitSaveVariable(tmpVariable);

      int idx = 0;
      foreach (var curr in Values)
      {
        var currType = curr.GetExpressionType(emitter);
        if (currType != typeName)
          Error(String.Format(Resources.errArrayTypeMismatch, currType, typeName), curr.Lexem);

        emitter.EmitLoadVariable(tmpVariable);
        emitter.EmitLoadInt(idx);
        curr.Compile(emitter);
        emitter.EmitSaveIndex(typeName);

        idx++;
      }

      // return the created array
      emitter.EmitLoadVariable(tmpVariable);
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return Values;
    }
  }
}
