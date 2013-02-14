using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mirelle.Lexer;

namespace Mirelle.SyntaxTree
{
  public class VarSplatNode: SyntaxTreeNode
  {
    /// <summary>
    /// List of variable names
    /// </summary>
    public List<Lexem> Names = new List<Lexem>();

    /// <summary>
    /// Expression to splat
    /// </summary>
    public SyntaxTreeNode Expression;

    public override void Compile(Emitter.Emitter emitter)
    {
      // make sure there is an array
      var exprType = Expression.GetExpressionType(emitter);
      if (!exprType.Contains("[]"))
        Error(Resources.errSplatArrayExpected);

      // define required variables
      var type = emitter.GetArrayItemType(exprType);
      var typeRef = emitter.ResolveType(type);
      var idx = 0;
      var tmpVar = emitter.CurrentMethod.Scope.Introduce(exprType, emitter.ResolveType(exprType));

      // compile array
      Expression.Compile(emitter);
      emitter.EmitSaveVariable(tmpVar);

      foreach(var curr in Names)
      {
        // check for variable redefinition
        if (emitter.CurrentMethod.Scope.Exists(curr.Data))
          Error(String.Format(Resources.errVariableRedefinition, curr.Data), curr);

        var varDecl = emitter.CurrentMethod.Scope.Introduce(type, emitter.ResolveType(type), curr.Data);
        var elseLabel = emitter.CreateLabel();
        var endLabel = emitter.CreateLabel();

        // make sure the array is not a null
        emitter.EmitLoadNull();
        emitter.EmitLoadVariable(tmpVar);
        emitter.EmitCompareEqual();
        emitter.EmitBranchTrue(elseLabel);

        // make sure there are items in the array
        emitter.EmitLoadInt(idx);
        emitter.EmitLoadVariable(tmpVar);
        emitter.EmitLoadArraySize();
        emitter.EmitCompareLess();
        emitter.EmitBranchFalse(elseLabel);

        // retrieve the current value
        emitter.EmitLoadVariable(tmpVar);
        emitter.EmitLoadInt(idx);
        if(type == "complex")
        {
          emitter.EmitLoadIndexAddress(typeRef);
          emitter.EmitLoadObject(typeRef);
        }
        else
          emitter.EmitLoadIndex(type);
        emitter.EmitBranch(endLabel);

        // or create a default 
        emitter.PlaceLabel(elseLabel);
        emitter.EmitLoadDefaultValue(type);

        // assign the variable
        emitter.PlaceLabel(endLabel);
        emitter.EmitSaveVariable(varDecl);

        idx++;
      }
    }
  }
}
