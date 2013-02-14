using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;

namespace Mirelle.SyntaxTree
{
  public class MethodNode: SyntaxTreeNode
  {
    /// <summary>
    /// Method name
    /// </summary>
    public string Name;

    /// <summary>
    /// Method return type
    /// </summary>
    public SignatureNode Type;

    /// <summary>
    /// Flag indicating method is written in C# and has no body
    /// </summary>
    public bool BuiltIn;

    /// <summary>
    /// Flag indicating method does not have a "this" pointer
    /// </summary>
    public bool Static;

    /// <summary>
    /// Flag indicating method has overloads in parent or child classes
    /// </summary>
    public bool Virtual;

    /// <summary>
    /// Private flag
    /// </summary>
    public bool Private = false;

    /// <summary>
    /// Method signature in format "name paramtype1 paramtype2 ..."
    /// </summary>
    public string Signature;

    /// <summary>
    /// Scope of local variables
    /// </summary>
    public Utils.Scope Scope;

    /// <summary>
    /// A list of parameter nodes
    /// </summary>
    public HashList<ParameterNode> Parameters = new HashList<ParameterNode>();

    /// <summary>
    /// The method body
    /// </summary>
    public CodeBlockNode Body = new CodeBlockNode();

    /// <summary>
    /// Reference to method owner type
    /// </summary>
    public TypeNode Owner;

    /// <summary>
    /// Underlying method in the assembly
    /// </summary>
    public MethodReference Method;

    public MethodNode(string name, SignatureNode type, bool isStatic = false, bool builtIn = false, MethodReference method = null)
    {
      Name = name;
      Type = type;
      Static = isStatic;
      Method = method;
      BuiltIn = builtIn;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      if (!BuiltIn)
      {
        emitter.CurrentMethod = this;

        // special cases for constructors
        if(Name == ".ctor")
        {
          // invoke base constructor
          emitter.EmitLoadThis();
          if(emitter.CurrentType.Parent != "")
            emitter.EmitCall(emitter.FindMethod(emitter.CurrentType.Parent, ".ctor"));
          else
            emitter.EmitCall(emitter.AssemblyImport(typeof(object).GetConstructor(new Type[] { } )));

          // invoke initializer
          if (emitter.MethodNameExists(emitter.CurrentType.Name, ".init"))
          {
            emitter.EmitLoadThis();
            emitter.EmitCall(emitter.FindMethod(emitter.CurrentType.Name, ".init"));
          }
        }

        Body.Compile(emitter);
        if (!Body.AllPathsReturn)
        {
          if (Type.Signature == "void")
            emitter.EmitReturn();
          else
            Error(String.Format(Resources.errNotAllPathsReturn, Name));
        }

        emitter.CurrentMethod = null;
      }
    }

    /// <summary>
    /// Set the list of parameters
    /// </summary>
    /// <param name="parameters"></param>
    public void SetParameters(HashList<ParameterNode> parameters = null)
    {
      if (parameters != null)
      {
        Parameters = parameters;

        var sb = new StringBuilder(Name);
        if (parameters != null)
        {
          foreach (var curr in parameters)
            sb.Append(" " + parameters[curr].Type.Signature);
        }
        Signature = sb.ToString();
      }
      else
      {
        Parameters = new HashList<ParameterNode>();
        Signature = Name;
      }
    }

    /// <summary>
    /// Retrieve parameter types as a string array
    /// </summary>
    /// <returns></returns>
    public string[] GetParameterTypes()
    {
      var types = new string[Parameters.Count];
      for (int idx = 0; idx < Parameters.Count; idx++)
        types[idx] = Parameters[idx].Type.Signature;

      return types;
    }
  }
}
