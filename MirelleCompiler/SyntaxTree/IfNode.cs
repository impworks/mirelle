using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Mirelle.SyntaxTree
{
  public class IfNode: SyntaxTreeNode
  {
    /// <summary>
    /// Flag indicating both paths return a value
    /// </summary>
    public bool AllPathsReturn = false;

    /// <summary>
    /// Condition expression
    /// </summary>
    public SyntaxTreeNode Condition;

    /// <summary>
    /// Block of code to evaluate when the condition is true
    /// </summary>
    public SyntaxTreeNode TrueBlock;

    /// <summary>
    /// Block of code to evaluate when the condition is false
    /// </summary>
    public SyntaxTreeNode FalseBlock;

    /// <summary>
    /// Label to branch to when the expression is false
    /// </summary>
    public Instruction FalseBlockStart;

    /// <summary>
    /// Label to branch to after the true block has executed
    /// </summary>
    public Instruction FalseBlockEnd;

    public IfNode(SyntaxTreeNode condition = null, SyntaxTreeNode trueBlock = null, SyntaxTreeNode falseBlock = null)
    {
      Condition = condition;
      TrueBlock = trueBlock;
      FalseBlock = falseBlock;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // check if condition is boolean or can be converted
      var condType = Condition.GetExpressionType(emitter);
      MethodNode converter = null;
      if (condType != "bool")
      {
        try
        {
          converter = emitter.FindMethod(condType, "to_b");
        }
        catch
        {
          Error(Resources.errBoolExpected);
        }
      }

      FalseBlockStart = emitter.CreateLabel();

      // condition
      Condition.Compile(emitter);
      if (converter != null)
        emitter.EmitCall(converter);
      emitter.EmitBranchFalse(FalseBlockStart);

      // "true" body
      TrueBlock.Compile(emitter);

      // there is a false block?
      if(FalseBlock != null)
      {
        // create a jump
        FalseBlockEnd = emitter.CreateLabel();
        emitter.EmitBranch(FalseBlockEnd);
        emitter.PlaceLabel(FalseBlockStart);

        // "false" body
        FalseBlock.Compile(emitter);
        emitter.PlaceLabel(FalseBlockEnd);
      }
      else
      {
        // put the 'nop' after the condition body
        emitter.PlaceLabel(FalseBlockStart);
      }
    }
  }
}
