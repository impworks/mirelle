using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class ArrayGetNode : ArrayIndexNode, IAssignable
  {
    public ArrayGetNode(SyntaxTreeNode index = null)
    {
      Index = index;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType == "")
      {
        var type = ExpressionPrefix.GetExpressionType(emitter);
        if (type == "dict")
          ExpressionType = "string";
        else
          ExpressionType = emitter.GetArrayItemType(type);
      }

      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var type = ExpressionPrefix.GetExpressionType(emitter);
      if (type == "dict")
        CompileDict(emitter);
      else
        CompileArray(emitter);
    }

    /// <summary>
    /// Compile a dictionary indexer
    /// </summary>
    /// <param name="emitter"></param>
    private void CompileDict(Emitter.Emitter emitter)
    {
      // ensure index is a string
      if (Index.GetExpressionType(emitter) != "string")
        Error(Resources.errStringExpected, Index.Lexem);

      ExpressionPrefix.Compile(emitter);
      Index.Compile(emitter);

      var method = emitter.FindMethod("dict", "get", "string");
      emitter.EmitCall(method);
    }

    /// <summary>
    /// Compile an array indexer
    /// </summary>
    /// <param name="emitter"></param>
    private void CompileArray(Emitter.Emitter emitter)
    {
      // ensure this is an array
      var type = emitter.GetArrayItemType(ExpressionPrefix.GetExpressionType(emitter));
      if (type == "")
        Error(Resources.errIndexingNotAnArray);

      // ensure index is integer
      if (Index.GetExpressionType(emitter) != "int")
        Error(Resources.errIntIndexExpected, Index.Lexem);

      ExpressionPrefix.Compile(emitter);
      Index.Compile(emitter);

      if (type == "complex")
      {
        // special case of valuetypes
        var typeRef = emitter.ResolveType(type);
        emitter.EmitLoadIndexAddress(typeRef);
        emitter.EmitLoadObject(typeRef);
      }
      else
        emitter.EmitLoadIndex(type);
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return new[] { ExpressionPrefix, Index };
    }
  }
}
