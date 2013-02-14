using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class ParameterNode: SyntaxTreeNode
  {
    /// <summary>
    /// Parameter node
    /// </summary>
    public string Name;

    /// <summary>
    /// Parameter type
    /// </summary>
    public SignatureNode Type;

    /// <summary>
    /// Parameter number
    /// </summary>
    public int Id;

    public ParameterNode(string name, SignatureNode type, int id)
    {
      Name = name;
      Type = type;
      Id = id;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return Type.Signature;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
    }
  }
}
