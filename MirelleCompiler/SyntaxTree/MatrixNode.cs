using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MN = MathNet.Numerics.LinearAlgebra.Double;
using Mono.Cecil;

namespace Mirelle.SyntaxTree
{
  public class MatrixNode: SyntaxTreeNode
  {
    /// <summary>
    /// Items in the matrix
    /// </summary>
    public List<List<SyntaxTreeNode>> MatrixItems = new List<List<SyntaxTreeNode>>();

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "matrix";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // retrieve matrix dimensions
      var height = MatrixItems.Count;
      var width = MatrixItems[0].Count;

      // retrieve info about matrices
      var matrixType = typeof(MN.DenseMatrix);
      var matrixCtor = emitter.AssemblyImport(matrixType.GetConstructor(new[] { typeof(int), typeof(int) }));
      var matrixSet = emitter.AssemblyImport(matrixType.GetMethod("At", new[] { typeof(int), typeof(int), typeof(double) }));

      var tmpVar = emitter.CurrentMethod.Scope.Introduce("matrix", emitter.AssemblyImport(matrixType));

      // create matrix
      emitter.EmitLoadInt(height);
      emitter.EmitLoadInt(width);
      emitter.EmitNewObj(matrixCtor);
      emitter.EmitSaveVariable(tmpVar);

      // set items
      for(var idx1 = 0; idx1 < MatrixItems.Count; idx1++)
      {
        // ensure all lines have the same number of items
        if (idx1 > 0 && MatrixItems[0].Count != MatrixItems[idx1].Count)
          Error(String.Format(Resources.errMatrixLineLengthMismatch, MatrixItems[0].Count, idx1+1, MatrixItems[idx1].Count));

        for(var idx2 = 0; idx2 < MatrixItems[idx1].Count; idx2++)
        {
          var item = MatrixItems[idx1][idx2];
          var itemType = item.GetExpressionType(emitter);
          if (!itemType.IsAnyOf("int", "float"))
            Error(Resources.errMatrixItemTypeMismatch, item.Lexem);

          emitter.EmitLoadVariable(tmpVar);

          emitter.EmitLoadInt(idx1);
          emitter.EmitLoadInt(idx2);
          item.Compile(emitter);

          if (itemType != "float")
            emitter.EmitConvertToFloat();

          emitter.EmitCall(matrixSet);
        }
      }

      emitter.EmitLoadVariable(tmpVar);
    }
  }
}
