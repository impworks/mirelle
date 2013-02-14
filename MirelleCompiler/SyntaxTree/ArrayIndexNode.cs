using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  abstract public class ArrayIndexNode : IdentifierNode
  {
    /// <summary>
    /// Index of the array item
    /// </summary>
    public SyntaxTreeNode Index;
  }
}
