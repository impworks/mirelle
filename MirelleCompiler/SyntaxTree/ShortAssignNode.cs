using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirelle.Lexer;

namespace Mirelle.SyntaxTree
{
  public class ShortAssignNode: SyntaxTreeNode
  {
    /// <summary>
    /// Left hand side expression (field, variable, array or matrix item)
    /// </summary>
    public IdentifierNode Lvalue;
    public SyntaxTreeNode Expression;

    public ShortAssignNode(IdentifierNode node, Lexer.Lexem lexem)
    {
      Lvalue = node;
      Lexem = lexem;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // check if there's an expression prefix and back it up
      if(Lvalue.ExpressionPrefix != null)
      {
        // save to tmp variable
        var tmpVarName = emitter.CurrentMethod.Scope.CreateNewName();
        var tmpVar = Expr.Var(tmpVarName, Lvalue.ExpressionPrefix);
        tmpVar.Compile(emitter);

        Lvalue.ExpressionPrefix = new IdentifierGetNode(tmpVarName);
      }

      // prepare operator node
      BinaryOperatorNode opNode;
      switch(Lexem.Type)
      {
        case LexemType.AssignAdd: opNode = new OperatorAddNode(); break;
        case LexemType.AssignSubtract: opNode = new OperatorSubtractNode(); break;
        case LexemType.AssignMultiply: opNode = new OperatorMultiplyNode(); break;
        case LexemType.AssignDivide: opNode = new OperatorDivideNode(); break;
        case LexemType.AssignRemainder: opNode = new OperatorRemainderNode(); break;
        case LexemType.AssignPower: opNode = new OperatorPowerNode(); break;
        case LexemType.AssignShiftLeft: opNode = new OperatorBinaryShiftLeftNode(); break;
        case LexemType.AssignShiftRight: opNode = new OperatorBinaryShiftRightNode(); break;
        default: throw new NotImplementedException();
      }
      opNode.Lexem = Lexem;
      opNode.Left = Lvalue;
      opNode.Right = Expression;

      // prepare assignment
      IdentifierNode saveNode;
      if (Lvalue is IdentifierGetNode)
        saveNode = Expr.IdentifierSet(Lvalue as IdentifierGetNode, opNode);
      else if (Lvalue is ArrayGetNode)
        saveNode = Expr.ArraySet(Lvalue as ArrayGetNode, opNode);
      else
       saveNode = Expr.MatrixSet(Lvalue as MatrixGetNode, opNode);

      saveNode.Lexem = Lexem;
      saveNode.Compile(emitter);
    }
  }
}
