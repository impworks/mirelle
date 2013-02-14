using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Mirelle.SyntaxTree
{
  public abstract class LoopNode: SyntaxTreeNode
  {
    /// <summary>
    /// Label to branch to at the end of the loop
    /// </summary>
    public Instruction BodyStart;

    /// <summary>
    /// Label to branch to when the iteration has ended
    /// </summary>
    public Instruction BodyEnd;

    /// <summary>
    /// Body of the loop
    /// </summary>
    public SyntaxTreeNode Body;
  }
}
