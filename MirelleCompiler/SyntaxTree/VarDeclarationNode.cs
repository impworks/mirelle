using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class VarDeclarationNode : SyntaxTreeNode
  {
    /// <summary>
    /// Variable name
    /// </summary>
    public string Name;

    /// <summary>
    /// Expression to assign
    /// </summary>
    public SyntaxTreeNode Expression;

    public VarDeclarationNode(string name = "")
    {
      Name = name;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // check variable for redefinition
      if (emitter.CurrentMethod.Scope.Exists(Name))
        Error(String.Format(Resources.errVariableRedefinition, Name));

      // get variable type
      var type = Expression.GetExpressionType(emitter);
      if (type == "null")
        Error(Resources.errVariableDefinedNull);
      if (type.IsAnyOf("", "void"))
        Error(Resources.errVoidExpression);

      // compile expression
      Expression.Compile(emitter);

      // declare variable in the current scope
      var varDecl = emitter.CurrentMethod.Scope.Introduce(type, emitter.ResolveType(type), Name);
      // emit save opcode
      emitter.EmitSaveVariable(varDecl);
    }
  }
}
