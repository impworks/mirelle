using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public interface ISyntaxTreeNode
  {
    string Compile(Compiler compiler);
  }
}
