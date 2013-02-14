using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class DictNode: SyntaxTreeNode
  {
    /// <summary>
    /// Data as a list of (key, value) tuples
    /// </summary>
    public List<Tuple<SyntaxTreeNode, SyntaxTreeNode>> Data = new List<Tuple<SyntaxTreeNode, SyntaxTreeNode>>();

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "dict";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // declare variables and methods
      var tmpVar = emitter.CurrentMethod.Scope.Introduce("dict", emitter.ResolveType("dict"));
      var ctor = emitter.AssemblyImport(typeof(MirelleStdlib.Dict).GetConstructor(new Type[] { }));
      var set = emitter.FindMethod("dict", "set", "string", "string");

      // var tmp = new dict
      emitter.EmitNewObj(ctor);
      emitter.EmitSaveVariable(tmpVar);

      // tmp[key] = value
      foreach(var curr in Data)
      {
        var keyType = curr.Item1.GetExpressionType(emitter);
        var valueType = curr.Item2.GetExpressionType(emitter);

        if (keyType != "string")
          Error(Resources.errDictItemTypeMismatch, curr.Item1.Lexem);

        if (valueType != "string")
          Error(Resources.errDictItemTypeMismatch, curr.Item2.Lexem);

        emitter.EmitLoadVariable(tmpVar);
        curr.Item1.Compile(emitter);
        curr.Item2.Compile(emitter);
        emitter.EmitCall(set);
      }

      emitter.EmitLoadVariable(tmpVar);
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      foreach(var curr in Data)
      {
        yield return curr.Item1;
        yield return curr.Item2;
      }
    }
  }
}
