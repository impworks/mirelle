using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class NewNode: SyntaxTreeNode
  {
    /// <summary>
    /// Type name
    /// </summary>
    public string Name;

    /// <summary>
    /// Constructor parameters
    /// </summary>
    public List<SyntaxTreeNode> Parameters = new List<SyntaxTreeNode>();

    /// <summary>
    /// Array of parameter types
    /// </summary>
    public string[] Signature = null;

    private string[] GetSignature(Emitter.Emitter emitter)
    {
      if (Signature != null) return Signature;

      Signature = new string[Parameters.Count];
      var idx = 0;
      foreach (var curr in Parameters)
      {
        Signature[idx] = curr.GetExpressionType(emitter);
        idx++;
      }

      return Signature;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return Name;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // check if constructor exists
      TypeNode type = null;
      try
      {
        type = emitter.FindType(Name);
      }
      catch
      {
        Error(String.Format(Resources.errTypeNotFound, Name));
      }

      // check if type is an enum?
      if (type.Enum)
      {
        // the constructor in an enum is pseudo-private
        // so that it can only be called from another method
        // of the same enum type
        if(emitter.CurrentMethod == null || emitter.CurrentMethod.Owner != type)
          Error(String.Format(Resources.errConstructEnum, Name));
      }

      MethodNode method = null;
      var signature = GetSignature(emitter);
      try
      {
        method = emitter.FindMethod(Name, ".ctor", signature);
      }
      catch
      {
        Error(String.Format(Resources.errConstructorNotFound, Name, signature.Join(" ")));
      }

      // parameters
      for (int idx = 0; idx < Parameters.Count; idx++)
      {
        Parameters[idx].Compile(emitter);
        emitter.EmitUpcastBasicType(Parameters[idx].GetExpressionType(emitter), method.Parameters[idx].Type.Signature);
      }

      emitter.EmitNewObj(method);
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return Parameters;
    }
  }
}
