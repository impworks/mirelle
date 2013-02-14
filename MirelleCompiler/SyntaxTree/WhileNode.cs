using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Mirelle.SyntaxTree
{
  public class WhileNode: LoopNode
  {
    /// <summary>
    /// Condition of the loop
    /// </summary>
    public SyntaxTreeNode Condition;

    public override void Compile(Emitter.Emitter emitter)
    {
      // check if condition is boolean
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

      // create markers
      BodyStart = emitter.CreateLabel();
      BodyEnd = emitter.CreateLabel();

      emitter.PlaceLabel(BodyStart);

      // condition
      Condition.Compile(emitter);
      if (converter != null)
        emitter.EmitCall(converter);
      emitter.EmitBranchFalse(BodyEnd);

      // body
      var preCurrLoop = emitter.CurrentLoop;
      emitter.CurrentLoop = this;
      Body.Compile(emitter);
      emitter.CurrentLoop = preCurrLoop;

      // re-test condition
      emitter.EmitBranch(BodyStart);

      emitter.PlaceLabel(BodyEnd);
    }
  }
}
