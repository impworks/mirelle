using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  abstract public class MatrixIndexNode : IdentifierNode
  {
    /// <summary>
    /// Row index
    /// </summary>
    public SyntaxTreeNode Index1;

    /// <summary>
    /// Column index
    /// </summary>
    public SyntaxTreeNode Index2;
  }
}
