using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class IdentifierGetNode : IdentifierNode, IAssignable
  {
    /// <summary>
    /// The identifier kind (static / dynamic, field / method / variable ) 
    /// </summary>
    public IdentifierKind Kind = IdentifierKind.Unresolved;

    public IdentifierGetNode(string name, bool atmark = false)
    {
      Name = name;
      AtmarkPrefix = atmark;
    }

    public IdentifierGetNode(string name, string typePrefix)
    {
      Name = name;
      TypePrefix = new Lexer.Lexem(Lexer.LexemType.Identifier, typePrefix);
    }

    public IdentifierGetNode(string name, SyntaxTreeNode expPrefix)
    {
      Name = name;
      ExpressionPrefix = expPrefix;
    }

    /// <summary>
    /// Resolve the identifier meaning
    /// </summary>
    public void Resolve(Emitter.Emitter emitter)
    {
      // already resolved?
      if (Kind != IdentifierKind.Unresolved) return;

      // atmark?
      if (AtmarkPrefix)
      {
        if (emitter.CurrentType == null)
          Error(Resources.errFieldOutsideType);

        OwnerType = emitter.CurrentType.Name;

        var field = emitter.FindField(emitter.CurrentType.Name, Name);
        if (field != null)
          Kind = field.Static ? IdentifierKind.StaticField : IdentifierKind.Field;
        else
          Error(String.Format(Resources.errFieldNotFound, Name, emitter.CurrentType.Name));

        // additional check: dynamic field from static method
        if (emitter.CurrentMethod.Static && !field.Static)
          Error(String.Format(Resources.errDynamicFromStatic, field.Name));
      }

      // other prefix?
      else if (TypePrefix != null || ExpressionPrefix != null)
      {
        OwnerType = (TypePrefix != null ? TypePrefix.Data : ExpressionPrefix.GetExpressionType(emitter));

        if (OwnerType == "null")
          Error(Resources.errNullAccessor);

        if(OwnerType.EndsWith("[]") && Name == "size")
        {
          Kind = IdentifierKind.SizeProperty;
          return;
        }

        // check class existence
        emitter.FindType(OwnerType);

        // field ?
        try
        {
          emitter.FindField(OwnerType, Name);
          Kind = (TypePrefix != null ? IdentifierKind.StaticField : IdentifierKind.Field);
          return;
        }
        catch { }

        // method ?!
        MethodNode method = null;
        try
        {
          method = emitter.FindMethod(OwnerType, Name);
        }
        catch
        {
          Error(String.Format(Resources.errTypeIdentifierUnresolved, OwnerType, Name));
        }

        if (ExpressionPrefix == null && !method.Static)
          Error(String.Format(Resources.errNonStaticMethod, Name));

        Kind = (TypePrefix != null ? IdentifierKind.StaticMethod : IdentifierKind.Method);
      }

      else
      {
        MethodNode method = null;

        // local variable
        if (emitter.CurrentMethod.Scope.Exists(Name))
        {
          Kind = IdentifierKind.Variable;
          return;
        }

        // parameter
        if(emitter.CurrentMethod.Parameters.Contains(Name))
        {
          Kind = IdentifierKind.Parameter;
          return;
        }
        
        // search for a method
        try
        {
          method = emitter.FindMethod(emitter.CurrentType != null ? emitter.CurrentType.Name : "", true, Name);
        }
        catch
        {
          Error(String.Format(Resources.errIdentifierUnresolved, Name));
        }

        OwnerType = method.Owner.Name;
        if(method.Static)
          Kind = IdentifierKind.StaticMethod;
        else
        {
          // additional check for invoking a dynamic method from static context
          if (emitter.CurrentMethod == null || emitter.CurrentMethod.Static)
            Error(String.Format(Resources.errDynamicFromStatic, Name));

          Kind = IdentifierKind.Method;
        }
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
      catch (CompilerException ex)
      {
        ex.AffixToLexem(Lexem);
        throw;
      }

      switch(Kind)
      {
        case IdentifierKind.StaticField:
        case IdentifierKind.Field: ExpressionType = emitter.FindField(OwnerType, Name).Type.Signature; break;
        case IdentifierKind.StaticMethod:
        case IdentifierKind.Method: ExpressionType = emitter.FindMethod(OwnerType, Name).Type.Signature; break;
        case IdentifierKind.Variable: ExpressionType = emitter.CurrentMethod.Scope.Find(Name).Type; break;
        case IdentifierKind.Parameter: ExpressionType = emitter.CurrentMethod.Parameters[Name].Type.Signature; break;
        case IdentifierKind.SizeProperty: ExpressionType = "int"; break;
      }

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

      switch (Kind)
      {

        case IdentifierKind.StaticField:    emitter.EmitLoadField(emitter.FindField(OwnerType, Name)); break;

        case IdentifierKind.Field:          if(ExpressionPrefix != null)
                                              ExpressionPrefix.Compile(emitter);
                                            else
                                              emitter.EmitLoadThis();
                                            emitter.EmitLoadField(emitter.FindField(OwnerType, Name)); break;

        case IdentifierKind.StaticMethod:   emitter.EmitCall(emitter.FindMethod(OwnerType, Name)); break;

        case IdentifierKind.Method:         if (ExpressionPrefix != null)
                                              ExpressionPrefix.Compile(emitter);
                                            else
                                              emitter.EmitLoadThis();
                                            emitter.EmitCall(emitter.FindMethod(OwnerType, Name)); break;

        case IdentifierKind.Variable:       emitter.EmitLoadVariable(emitter.CurrentMethod.Scope.Find(Name)); break;

        case IdentifierKind.Parameter:      emitter.EmitLoadParameter(emitter.CurrentMethod.Parameters[Name].Id); break;

        case IdentifierKind.SizeProperty:   ExpressionPrefix.Compile(emitter);
                                            emitter.EmitLoadArraySize(); break;
      }
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return new[] { ExpressionPrefix };
    }

    public override string ClosuredName(Emitter.Emitter emitter)
    {
      Resolve(emitter);
      switch(Kind)
      {
        case IdentifierKind.Field: if (AtmarkPrefix) Error(Resources.errClosuredMember); break;
        case IdentifierKind.Method: if (ExpressionPrefix == null) Error(Resources.errClosuredMember); break;
        case IdentifierKind.Variable: return Name;
      }

      return null;
    }
  }
}
