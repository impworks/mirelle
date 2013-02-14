using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class AsNode: SyntaxTreeNode
  {
    /// <summary>
    /// Expression to cast
    /// </summary>
    public SyntaxTreeNode Expression;

    /// <summary>
    /// Desired type
    /// </summary>
    public string ToType;

    public AsNode(SyntaxTreeNode expr, string to)
    {
      Expression = expr;
      ToType = to;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return ToType;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var fromType = Expression.GetExpressionType(emitter);
      var toType = ToType;
      var simpleTypes = new[] { "bool", "int", "float", "long", "complex" };

      if (emitter.ResolveType(toType) == null)
        Error(String.Format(Resources.errTypeNotFound, toType));

      // compile the expression itself
      Expression.Compile(emitter);

      // idiotic case?
      if (fromType == toType) return;

      if(fromType == "null")
      {
        if(toType.IsAnyOf(simpleTypes))
          Error(String.Format(Resources.errInvalidNullCast, toType));

        // no actual casting is required
      }
      // cast simple types
      else if(fromType.IsAnyOf(simpleTypes))
      {
        if (toType.IsAnyOf(simpleTypes))
        {
          switch(toType)
          {
            case "bool": emitter.EmitConvertToBool(); break;
            case "int": emitter.EmitConvertToInt(); break;
            case "float": emitter.EmitConvertToFloat(); break;
            default: throw new NotImplementedException();
          }
        }
        else
          Error(String.Format(Resources.errInvalidCast, fromType, toType));
      }
      else
      {
        // complex type to simple type: search for a conversion method
        if(toType.IsAnyOf(simpleTypes))
        {
          // to_b, to_f, to_i
          string methodName = "to_" + toType[0];
          MethodNode converter = null;
          try
          {
            converter = emitter.FindMethod(fromType, methodName);
          }
          catch
          {
            Error(String.Format(Resources.errInvalidCast, fromType, toType));
          }

          // call the method
          emitter.EmitCall(converter);
        }
        else
        {
          if (fromType.Contains("[]") || toType.Contains("[]"))
            Error(Resources.errCastArray);

          if(!emitter.TypeCastable(fromType, toType))
            Error(String.Format(Resources.errInvalidCast, fromType, toType));

          var type = emitter.FindType(toType);
          emitter.EmitCast(type.Type);
        }
      }
    }

    public override IEnumerable<SyntaxTreeNode> Children()
    {
      return new[] { Expression };
    }
  }
}
