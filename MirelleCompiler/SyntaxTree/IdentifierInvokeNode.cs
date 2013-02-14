using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class IdentifierInvokeNode: IdentifierNode
  {
    /// <summary>
    /// Arguments
    /// </summary>
    public List<SyntaxTreeNode> Parameters = new List<SyntaxTreeNode>();

    /// <summary>
    /// Static invocation?
    /// </summary>
    public bool Static = false;

    /// <summary>
    /// Array of argument types
    /// </summary>
    public string[] Signature = null;

    public IdentifierInvokeNode(string name)
    {
      Name = name;
    }

    private string[] GetSignature(Emitter.Emitter emitter)
    {
      if (Signature != null) return Signature;

      Signature = new string[Parameters.Count];
      var idx = 0;
      foreach(var curr in Parameters)
      {
        Signature[idx] = curr.GetExpressionType(emitter);
        idx++;
      }
      
      return Signature;
    }

    public void Resolve(Emitter.Emitter emitter)
    {
      // build signature
      var signature = GetSignature(emitter);
      MethodNode method;

      // type prefix ?
      if(TypePrefix != null)
      {
        OwnerType = TypePrefix.Data;
        Static = true;

        // check class existence
        emitter.FindType(OwnerType);
        method = emitter.FindMethod(TypePrefix.Data, Name, signature);

        if (!method.Static)
          Error(String.Format(Resources.errNonStaticMethod, Name));
      }

      // expression prefix ?
      else if(ExpressionPrefix != null)
      {
        OwnerType = ExpressionPrefix.GetExpressionType(emitter);
        if (OwnerType == "null")
          Error(Resources.errNullAccessor);

        emitter.FindMethod(OwnerType, Name, signature);
      }

      // local or visible method ?
      else
      {
        // try to find method in local type, if there's one
        var tmpOwner = emitter.CurrentType != null ? emitter.CurrentType.Name : "";
        method = emitter.FindMethod(tmpOwner, true, Name, signature);

        OwnerType = method.Owner.Name;
        Static = method.Static;
      }
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      if (ExpressionType != "")
        return ExpressionType;

      try
      {
        Resolve(emitter);
      }
      catch(CompilerException ex)
      {
        ex.AffixToLexem(Lexem);
        throw;
      }

      var method = emitter.FindMethod(OwnerType, Name, GetSignature(emitter));
      ExpressionType = method != null ? method.Type.Signature : "";
      return ExpressionType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      try
      {
        Resolve(emitter);
      }
      catch (CompilerException ex)
      {
        ex.AffixToLexem(Lexem);
        throw;
      }

      var method = emitter.FindMethod(OwnerType, Name, GetSignature(emitter));

      // load 'this'
      if (ExpressionPrefix != null)
        ExpressionPrefix.Compile(emitter);
      else if (!Static)
        emitter.EmitLoadThis();

      // load parameters
      for (int idx = 0; idx < Parameters.Count; idx++)
      {
        Parameters[idx].Compile(emitter);
        emitter.EmitUpcastBasicType(Parameters[idx].GetExpressionType(emitter), method.Parameters[idx].Type.Signature);
      }

      // invoke
      emitter.EmitCall(method);
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      if(ExpressionPrefix == null)
        return Parameters;

      var list = new List<SyntaxTreeNode>(Parameters);
      list.Add(ExpressionPrefix);
      return list;
    }
  }
}
