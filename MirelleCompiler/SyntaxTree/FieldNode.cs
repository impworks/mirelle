using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Mirelle.SyntaxTree
{
  public class FieldNode: SyntaxTreeNode
  {
    /// <summary>
    /// Field name
    /// </summary>
    public string Name;

    /// <summary>
    /// Field type signature
    /// </summary>
    public SignatureNode Type;

    /// <summary>
    /// Static flag
    /// </summary>
    public bool Static;

    /// <summary>
    /// Private flag
    /// </summary>
    public bool Private = false;

    /// <summary>
    /// Owner type node
    /// </summary>
    public TypeNode Owner;

    /// <summary>
    /// Underlying assembly-level field
    /// </summary>
    public FieldReference Field;

    /// <summary>
    /// Initializer expression
    /// </summary>
    public SyntaxTreeNode Expression = null;

    public FieldNode(string name, SignatureNode type, bool isStatic = false, FieldReference field = null)
    {
      Name = name;
      Field = field;
      Type = type;
      Static = isStatic;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return Type.Signature;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      if(Expression != null)
      {
        try
        {
          emitter.CurrentMethod = emitter.CurrentType.Methods[(Static ? ".cctor" : ".init")][0];
        }
        catch
        {
          emitter.CurrentMethod = Static ? emitter.CreateStaticCtor(emitter.CurrentType, true) : emitter.CreateInitializer(emitter.CurrentType, true);
        }

        if (!Static)
          emitter.EmitLoadThis();

        Expression.Compile(emitter);
        emitter.EmitSaveField(this);

        emitter.CurrentMethod = null;
      }
    }
  }
}
