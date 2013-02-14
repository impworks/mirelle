using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  /**
   * Basic class to store multiple statements in a row
   */
  public class CodeBlockNode: SyntaxTreeNode
  {
    /// <summary>
    /// Flag indicating that all paths within the code block re
    /// </summary>
    public bool AllPathsReturn = false;

    /// <summary>
    /// List of statements within the block
    /// </summary>
    public List<SyntaxTreeNode> Statements = new List<SyntaxTreeNode>();

    public override void Compile(Emitter.Emitter emitter)
    {
      // check for variable definitions being the only expressions in a block
      if (Statements.Count == 1 && (Statements[0] is VarDeclarationNode || Statements[0] is VarSplatNode))
        Error(Resources.errVariableDefinitionOnly, Statements[0].Lexem);

      emitter.CurrentMethod.Scope.EnterSubScope();

      foreach (var curr in Statements)
      {
        curr.Compile(emitter);

        // eliminate dead code
        if (curr is ReturnNode || (curr is IfNode && (curr as IfNode).AllPathsReturn))
        {
          AllPathsReturn = true;
          break;
        }

        // remove clutter from stack
        if (!curr.GetExpressionType(emitter).IsAnyOf("", "void"))
          emitter.EmitPop();
      }

      emitter.CurrentMethod.Scope.LeaveSubScope();
    }
  }
}
