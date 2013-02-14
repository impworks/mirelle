using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MN = MathNet.Numerics.LinearAlgebra.Double;

namespace Mirelle.SyntaxTree
{
  public class MatrixGetNode : MatrixIndexNode, IAssignable
  {
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "float";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // ensure this is a matrix
      if (ExpressionPrefix.GetExpressionType(emitter) != "matrix")
        Error(Resources.errIndexingNotAMatrix);

      // ensure indexes are integers
      if (Index1.GetExpressionType(emitter) != "int")
        Error(Resources.errIntIndexExpected, Index1.Lexem);

      if(Index2.GetExpressionType(emitter) != "int")
        Error(Resources.errIntIndexExpected, Index2.Lexem);

      ExpressionPrefix.Compile(emitter);
      Index1.Compile(emitter);
      Index2.Compile(emitter);

      var method = emitter.AssemblyImport(typeof(MN.DenseMatrix).GetMethod("At", new[] { typeof(int), typeof(int) }));
      emitter.EmitCall(method);
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return new[] { ExpressionPrefix };
    }
  }
}
