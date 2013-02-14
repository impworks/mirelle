using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirelle.Lexer;

namespace Mirelle.SyntaxTree
{
  public class OperatorCompareNode: BinaryOperatorNode
  {
    public LexemType ComparisonType;

    public OperatorCompareNode(LexemType type)
    {
      switch (type)
      {
        case LexemType.Equal:
        case LexemType.NotEqual:
        case LexemType.Less:
        case LexemType.LessEqual:
        case LexemType.Greater:
        case LexemType.GreaterEqual: ComparisonType = type; break;
        default: throw new NotImplementedException();
      }
    }
 
    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "bool";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var leftType = Left.GetExpressionType(emitter);
      var rightType = Right.GetExpressionType(emitter);

      // ensure objects can be compared or issue an exception
      CheckComparable(leftType, rightType, emitter);

      // compile arguments and ensure their types
      Left.Compile(emitter);
      emitter.EmitUpcastBasicType(leftType, rightType);
      Right.Compile(emitter);
      emitter.EmitUpcastBasicType(rightType, leftType);
      
      // emity comparison opcodes
      if (ComparisonType == LexemType.Equal || ComparisonType == LexemType.NotEqual)
        CompileEquality(emitter, leftType, rightType);
      else
        CompileRelation(emitter, leftType, rightType);
    }

    /// <summary>
    /// Ensure two values can be compared, or issue an exception
    /// </summary>
    /// <param name="leftType">Left-hand operand type</param>
    /// <param name="rightType">Right-hand operand type</param>
    private void CheckComparable(string leftType, string rightType, Emitter.Emitter emitter)
    {
      var ok = false;
      var byValueTypes = new[] { "bool", "int", "float", "complex", "long" };
      var numericTypes = new[] { "int", "float", "complex", "long" };

      // equality comparison
      if(ComparisonType == LexemType.Equal || ComparisonType == LexemType.NotEqual)
      {
        // null and ref-type
        if(leftType == "null" && !rightType.IsAnyOf(byValueTypes))
          ok = true;

        // ref-type and null
        else if(rightType == "null" && !leftType.IsAnyOf(byValueTypes))
          ok = true;

        // types are equal and meaningful 
        else if(leftType == rightType && leftType != "void" && leftType != "")
          ok = true;

        // numeric types
        else if(leftType.IsAnyOf(numericTypes) && rightType.IsAnyOf(numericTypes))
        {
          // @TODO: rewrite! a clumsy kludge!
          var zomg = leftType + rightType;
          if(!zomg.Contains("complex") || !zomg.Contains("long"))
            ok = true;
        }

        // a valid comparator exits
        else
        {
          try
          {
            emitter.FindMethod(leftType, "equal", rightType);
            ok = true;
          }
          catch
          {
            try
            {
              emitter.FindMethod(rightType, "equal", leftType);
              ok = true;
            }
            catch { }
          }
        }

        if(!ok)
          Error(String.Format(Resources.errIncomparableTypes, leftType, rightType));
      }

      // relation comparison
      else
      {
        // strings
        if(leftType == "string" && rightType == "string")
          ok = true;

        // numeric types
        else if (leftType.IsAnyOf(numericTypes) && rightType.IsAnyOf(numericTypes))
        {
          // complex cannot be related (math law!)
          if(leftType != "complex" && rightType != "complex")
            ok = true;
        }

        if(!ok)
        {
          if(leftType == rightType)
            Error(String.Format(Resources.errRelationIncomparableTypes, leftType));
          else
            Error(String.Format(Resources.errIncomparableTypes, leftType, rightType));
        }
      }
    }

    /// <summary>
    /// Compare two values for their equality
    /// </summary>
    /// <param name="emitter">Emitter link</param>
    /// <param name="leftType">Left-hand argument type</param>
    /// <param name="rightType">Left-hand argument type</param>
    private void CompileEquality(Emitter.Emitter emitter, string leftType, string rightType)
    {
      var basicTypes = new[] { "bool", "int", "float", "string" };
      if (leftType.IsAnyOf(basicTypes))
      {
        // compare strings
        if (leftType == "string")
        {
          var method = emitter.AssemblyImport(typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }));
          emitter.EmitCall(method);
          emitter.EmitLoadBool(false);
        }

        emitter.EmitCompareEqual();
      }

      // long, complex and others
      else
      {
        System.Reflection.MethodInfo method;
        if(leftType.IsAnyOf("long", "complex"))
        {
          var type = (leftType == "long" ? typeof(System.Numerics.BigInteger) : typeof(System.Numerics.Complex));
          method = type.GetMethod("op_Equality", new[] { type, type });
        }
        else
          method = typeof(MirelleStdlib.Compare).GetMethod("Equal", new[] { typeof(object), typeof(object) });

        emitter.EmitCall(emitter.AssemblyImport(method));
      }

      // invert ?
      if (ComparisonType == LexemType.NotEqual)
      {
        emitter.EmitLoadBool(false);
        emitter.EmitCompareEqual();
      }
    }

    /// <summary>
    /// Compare two values for their relative order: less and greater
    /// </summary>
    /// <param name="emitter">Emitter link</param>
    /// <param name="leftType">Left-hand argument type</param>
    /// <param name="rightType">Left-hand argument type</param>
    private void CompileRelation(Emitter.Emitter emitter, string leftType, string rightType)
    {
      if(leftType == "string")
      {
        var method = emitter.AssemblyImport(typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }));
        emitter.EmitCall(method);
        emitter.EmitLoadBool(false);
      }

      switch (ComparisonType)
      {
        case LexemType.Less: emitter.EmitCompareLess(); break;

        case LexemType.LessEqual: emitter.EmitCompareGreater();
          emitter.EmitLoadBool(false);
          emitter.EmitCompareEqual(); break;

        case LexemType.Greater: emitter.EmitCompareGreater(); break;

        case LexemType.GreaterEqual: emitter.EmitCompareLess();
          emitter.EmitLoadBool(false);
          emitter.EmitCompareEqual(); break;
      }
    }
  }
}
