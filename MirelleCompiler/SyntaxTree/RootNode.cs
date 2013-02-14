using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class RootNode: SyntaxTreeNode
  {
    /// <summary>
    /// The overall list of declared types
    /// </summary>
    public HashList<TypeNode> Types = new HashList<TypeNode>();

    /// <summary>
    /// The method in which all global code resides
    /// </summary>
    public MethodNode GlobalMethod;

    /// <summary>
    /// Import data from another RootNode
    /// </summary>
    /// <param name="otherNode">Node to be imported</param>
    public void Merge(RootNode otherNode)
    {
      foreach (var curr in otherNode.Types)
        Types.Add(curr, otherNode.Types[curr]);

      foreach (var curr in otherNode.GlobalMethod.Body.Statements)
        GlobalMethod.Body.Statements.Add(curr);
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      foreach (var curr in Types)
        Types[curr].Compile(emitter);

      GlobalMethod.Compile(emitter);
    }
  }
}
