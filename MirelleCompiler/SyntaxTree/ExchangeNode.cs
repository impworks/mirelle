using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class ExchangeNode: SyntaxTreeNode
  {
    /// <summary>
    /// Left hand side expression
    /// </summary>
    public IdentifierNode Left;

    /// <summary>
    /// Right hand side expression
    /// </summary>
    public IdentifierNode Right;

    /// <summary>
    /// Return an assignable node from 
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public IdentifierNode GetAssignableNode(IdentifierNode getNode, IdentifierNode setNode)
    {
      if (getNode is IdentifierGetNode)
        return Expr.IdentifierSet(getNode as IdentifierGetNode, setNode);

      if (getNode is ArrayGetNode)
        return Expr.ArraySet(getNode as ArrayGetNode, setNode);

      if (getNode is MatrixGetNode)
        return Expr.MatrixSet(getNode as MatrixGetNode, setNode);

      throw new NotImplementedException();
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // check if there's an expression prefix and back it up
      if (Left.ExpressionPrefix != null)
      {
        // save to tmp variable
        var tmpVarName = emitter.CurrentMethod.Scope.CreateNewName();
        var tmpVar = Expr.Var(tmpVarName, Left.ExpressionPrefix);
        tmpVar.Compile(emitter);

        Left.ExpressionPrefix = Expr.IdentifierGet(tmpVarName);
      }

      if (Right.ExpressionPrefix != null)
      {
        // save to tmp variable
        var tmpVarName = emitter.CurrentMethod.Scope.CreateNewName();
        var tmpVar = Expr.Var(tmpVarName, Right.ExpressionPrefix);
        tmpVar.Compile(emitter);

        Right.ExpressionPrefix = Expr.IdentifierGet(tmpVarName);
      }

      // create temp variable
      var exchgVarName = emitter.CurrentMethod.Scope.CreateNewName();
      var exchgNode = new IdentifierGetNode(exchgVarName);

      // create three assignable nodes
      var assignNodes = new SyntaxTreeNode[3];
      assignNodes[0] = Expr.Var(exchgVarName, Left);
      assignNodes[1] = GetAssignableNode(Left, Right);
      assignNodes[2] = GetAssignableNode(Right, exchgNode);

      // affix them to the lexem and compile
      foreach(var curr in assignNodes)
      {
        curr.Lexem = Lexem;
        curr.Compile(emitter);
      }
    }
  }
}
