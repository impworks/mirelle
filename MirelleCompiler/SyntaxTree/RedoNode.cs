using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class RedoNode : SyntaxTreeNode
  {
    public override void Compile(Emitter.Emitter emitter)
    {
      if (emitter.CurrentLoop == null)
        Error(Resources.errBreakRedoOutsideLoop);

      emitter.EmitBranch(emitter.CurrentLoop.BodyStart);
    }
  }
}
