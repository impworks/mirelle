using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MN = MathNet.Numerics.LinearAlgebra.Double;

namespace Mirelle.SyntaxTree
{
  public class MatrixSetNode : MatrixIndexNode
  {
    /// <summary>
    /// Expression to assign
    /// </summary>
    public SyntaxTreeNode Expression;

    public MatrixSetNode()
    {
    }

    public MatrixSetNode(MatrixGetNode node)
    {
      ExpressionPrefix = node.ExpressionPrefix;
      Index1 = node.Index1;
      Index2 = node.Index2;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // ensure this is a matrix
      if (ExpressionPrefix.GetExpressionType(emitter) != "matrix")
        Error(Resources.errIndexingNotAMatrix);

      // ensure indexes are integers
      if (Index1.GetExpressionType(emitter) != "int")
        Error(Resources.errIntIndexExpected, Index1.Lexem);

      if (Index2.GetExpressionType(emitter) != "int")
        Error(Resources.errIntIndexExpected, Index2.Lexem);

      // ensure assigned value is either int or float
      var exprType = Expression.GetExpressionType(emitter);
      if (!exprType.IsAnyOf("int", "float"))
        Error(Resources.errMatrixItemTypeMismatch);

      ExpressionPrefix.Compile(emitter);

      Index1.Compile(emitter);
      Index2.Compile(emitter);

      Expression.Compile(emitter);
      if(exprType != "float")
        emitter.EmitConvertToFloat();

      var method = emitter.AssemblyImport(typeof(MN.DenseMatrix).GetMethod("At", new[] { typeof(int), typeof(int), typeof(double) }));
      emitter.EmitCall(method);
    }
  }
}
