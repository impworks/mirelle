using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class FloatRangeNode: SyntaxTreeNode
  {
    /// <summary>
    /// Starting expression
    /// </summary>
    public SyntaxTreeNode From;

    /// <summary>
    /// Ending expression
    /// </summary>
    public SyntaxTreeNode To;

    /// <summary>
    /// Step
    /// </summary>
    public SyntaxTreeNode Step;

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "float[]";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var numTypes = new[] { "int", "float" };

      var fromType = From.GetExpressionType(emitter);
      var toType = To.GetExpressionType(emitter);
      var stepType = Step.GetExpressionType(emitter);

      // validate parameters
      if (!fromType.IsAnyOf(numTypes) || !toType.IsAnyOf(numTypes) || !stepType.IsAnyOf(numTypes))
        Error(Resources.errIntFloatExpected);

      // from, to, step
      From.Compile(emitter);
      if (fromType != "float")
        emitter.EmitConvertToFloat();

      To.Compile(emitter);
      if (toType != "float")
        emitter.EmitConvertToFloat();

      Step.Compile(emitter);
      if (stepType != "float")
        emitter.EmitConvertToFloat();

      // invoke method
      var method = typeof(MirelleStdlib.ArrayHelper).GetMethod("CreateRangedArray", new[] { typeof(double), typeof(double), typeof(double) });
      emitter.EmitCall(emitter.AssemblyImport(method));
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return new[] { From, To, Step };
    }
  }
}
