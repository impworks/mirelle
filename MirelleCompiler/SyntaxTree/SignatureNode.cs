using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mirelle.SyntaxTree
{
  public class SignatureNode: SyntaxTreeNode
  {
    /// <summary>
    /// Signature in string form
    /// </summary>
    public string Signature;

    /// <summary>
    /// Assembly-level type reference
    /// </summary>
    public TypeReference CompiledType;

    public SignatureNode(string signature)
    {
      Signature = signature;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // nothing to compile actualy
    }
  }
}
