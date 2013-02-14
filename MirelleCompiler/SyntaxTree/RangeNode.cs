using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class RangeNode: SyntaxTreeNode
  {
    /// <summary>
    /// Range starting value
    /// </summary>
    public SyntaxTreeNode From;

    /// <summary>
    /// Range ending value
    /// </summary>
    public SyntaxTreeNode To;

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "range";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var fromType = From.GetExpressionType(emitter);
      var toType = To.GetExpressionType(emitter);

      if (fromType != "int" || toType != "int")
        Error(Resources.errRangeLimitsIntExpected);

      From.Compile(emitter);
      To.Compile(emitter);

      emitter.EmitNewObj(emitter.FindMethod("range", ".ctor", "int", "int"));
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return new[] { From, To };
    }
  }
}
