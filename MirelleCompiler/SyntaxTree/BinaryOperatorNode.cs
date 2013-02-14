using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  abstract public class BinaryOperatorNode: SyntaxTreeNode
  {
    /// <summary>
    /// Left hand side expression
    /// </summary>
    public SyntaxTreeNode Left;

    /// <summary>
    /// Right hand side expression
    /// </summary>
    public SyntaxTreeNode Right;

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return new[] { Left, Right };
    }
  }
}
