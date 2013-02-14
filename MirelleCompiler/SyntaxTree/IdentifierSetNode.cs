using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class IdentifierSetNode : IdentifierNode
  {
    /// <summary>
    /// Expression to assign
    /// </summary>
    public SyntaxTreeNode Expression;

    /// <summary>
    /// Identifier kind (static / dynamic, field / method, variable, etc)
    /// </summary>
    public IdentifierKind Kind = IdentifierKind.Unresolved;

    /// <summary>
    /// The type of the identifier (to check whether assigned expression type matches)
    /// </summary>
    private string IdentifierType = "";

    public IdentifierSetNode(string name, bool atmark = false)
    {
      Name = name;
      AtmarkPrefix = atmark;
    }

    public IdentifierSetNode(IdentifierGetNode node)
    {
      Name = node.Name;
      TypePrefix = node.TypePrefix;
      ExpressionPrefix = node.ExpressionPrefix;
      AtmarkPrefix = node.AtmarkPrefix;
    }

    private void Resolve(Emitter.Emitter emitter)
    {
      // already resolved?
      if (Kind != IdentifierKind.Unresolved) return;

      // atmark?
      if (AtmarkPrefix)
      {
        OwnerType = emitter.CurrentType.Name;

        var field = emitter.FindField(emitter.CurrentType.Name, Name);
        if (field != null)
          Kind = field.Static ? IdentifierKind.StaticField : IdentifierKind.Field;
        else
          Error(String.Format(Resources.errFieldNotFound, Name, emitter.CurrentType.Name));

        // additional check: dynamic field from static method
        if (emitter.CurrentMethod.Static && !field.Static)
          Error(String.Format(Resources.errDynamicFromStatic, field.Name));

        IdentifierType = field.Type.Signature;
      }

      // other prefix?
      else if (TypePrefix != null || ExpressionPrefix != null)
      {
        OwnerType = (TypePrefix != null ? TypePrefix.Data : ExpressionPrefix.GetExpressionType(emitter));

        // check class existence
        var type = emitter.FindType(OwnerType);
        if (type == null)
          Error(String.Format(Resources.errTypeNotFound, OwnerType), TypePrefix);

        // field ?
        var field = emitter.FindField(OwnerType, Name);
        Kind = (TypePrefix != null ? IdentifierKind.StaticField : IdentifierKind.Field);
        IdentifierType = field.Type.Signature;
      }

      else
      {
        // local variable
        if (emitter.CurrentMethod.Scope.Exists(Name))
        {
          Kind = IdentifierKind.Variable;
          IdentifierType = emitter.CurrentMethod.Scope.Find(Name).Type;
        }

        // parameter
        else if (emitter.CurrentMethod.Parameters.Contains(Name))
        {
          Kind = IdentifierKind.Parameter;
          IdentifierType = emitter.CurrentMethod.Parameters[Name].Type.Signature;
        }

        // no luck at all
        else
          Error(String.Format(Resources.errIdentifierUnresolved, Name));
      }

    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      try
      {
        Resolve(emitter);

        if (!emitter.TypeIsParent(IdentifierType, Expression.GetExpressionType(emitter)))
          Error(String.Format(Resources.errAssignTypeMismatch, Expression.GetExpressionType(emitter), IdentifierType));
      }
      catch(CompilerException ex)
      {
        ex.AffixToLexem(Lexem);
        throw;
      }

      switch (Kind)
      {

        case IdentifierKind.StaticField:  Expression.Compile(emitter);
                                          emitter.EmitSaveField(emitter.FindField(OwnerType, Name)); break;

        case IdentifierKind.Field:        if (ExpressionPrefix != null)
                                            ExpressionPrefix.Compile(emitter);
                                          else
                                            emitter.EmitLoadThis();
                                          Expression.Compile(emitter);
                                          emitter.EmitSaveField(emitter.FindField(OwnerType, Name)); break;

        case IdentifierKind.Variable:     Expression.Compile(emitter);
                                          emitter.EmitSaveVariable(emitter.CurrentMethod.Scope.Find(Name)); break;

        case IdentifierKind.Parameter:    Expression.Compile(emitter);
                                          emitter.EmitSaveParameter(emitter.CurrentMethod.Parameters[Name].Id); break;
      }
    }
  }
}
