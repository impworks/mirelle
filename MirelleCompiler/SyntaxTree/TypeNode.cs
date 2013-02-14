using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;

namespace Mirelle.SyntaxTree
{
  public class TypeNode: SyntaxTreeNode
  {
    /// <summary>
    /// Type name
    /// </summary>
    public string Name;

    /// <summary>
    /// Type parent name
    /// </summary>
    public string Parent;

    /// <summary>
    /// Flag indicating the type is an enum
    /// </summary>
    public bool Enum = false;

    /// <summary>
    /// Flag indicating the type should not be prepared
    /// </summary>
    public bool BuiltIn = false;

    /// <summary>
    /// Flag indicating the type should be autoconstructed
    /// </summary>
    public bool AutoConstruct = false;

    /// <summary>
    /// Native type reference
    /// </summary>
    public TypeReference Type;

    /// <summary>
    /// Accessible fields
    /// </summary>
    public HashList<FieldNode> Fields = new HashList<FieldNode>();

    /// <summary>
    /// Accessible methods
    /// </summary>
    public HashList<List<MethodNode>> Methods = new HashList<List<MethodNode>>();

    public TypeNode(string name, string parent = "", bool builtIn = false, TypeReference type = null)
    {
      Name = name;
      Type = type;
      Parent = parent;
      BuiltIn = builtIn;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // if class is imported, it has nothing to compile
      if (!BuiltIn)
      {
        // set current type
        emitter.CurrentType = this;
        emitter.CurrentMethod = null;

        // compile fields
        foreach (var curr in Fields)
          Fields[curr].Compile(emitter);

        // compile methods
        foreach (var curr in Methods)
          foreach(var currMethod in Methods[curr])
            currMethod.Compile(emitter);

        // unset current type
        emitter.CurrentType = null;
        emitter.CurrentMethod = emitter.RootNode.GlobalMethod;
      }
    }
  }
}
