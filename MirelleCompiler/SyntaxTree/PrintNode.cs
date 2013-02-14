using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mirelle.SyntaxTree
{
  public class PrintNode: SyntaxTreeNode
  {
    /// <summary>
    /// Arguments to print
    /// </summary>
    public List<SyntaxTreeNode> Parameters = new List<SyntaxTreeNode>();

    /// <summary>
    /// Use a trailing endline character?
    /// </summary>
    public bool PrintLine = false;

    public override void Compile(Emitter.Emitter emitter)
    {
      var args = new[] { Parameters.Count == 1 ? typeof(object) : typeof(IEnumerable<dynamic>), typeof(bool) };
      var printMethod = emitter.AssemblyImport(typeof(MirelleStdlib.Printer).GetMethod("Print", args));

      if (Parameters.Count == 1)
      {
        var currType = Parameters[0].GetExpressionType(emitter);
        Parameters[0].Compile(emitter);
        if (currType.IsAnyOf("int", "bool", "float", "complex"))
          emitter.EmitBox(emitter.ResolveType(currType));
      }
      else
      {
        var objType = emitter.AssemblyImport(typeof(object));
        var arrType = new ArrayType(objType);

        var tmpVariable = emitter.CurrentMethod.Scope.Introduce("object[]", arrType);

        // load count & create
        emitter.EmitLoadInt(Parameters.Count);
        emitter.EmitNewArray(objType);
        emitter.EmitSaveVariable(tmpVariable);

        int idx = 0;
        foreach (var curr in Parameters)
        {
          var currType = curr.GetExpressionType(emitter);
          emitter.EmitLoadVariable(tmpVariable);
          emitter.EmitLoadInt(idx);
          curr.Compile(emitter);

          if (currType.IsAnyOf("int", "bool", "float", "complex"))
            emitter.EmitBox(emitter.ResolveType(currType));

          emitter.EmitSaveIndex("object");

          idx++;
        }

        // return the created array
        emitter.EmitLoadVariable(tmpVariable);
      }

      emitter.EmitLoadBool(PrintLine);
      emitter.EmitCall(printMethod);
    }
  }
}
