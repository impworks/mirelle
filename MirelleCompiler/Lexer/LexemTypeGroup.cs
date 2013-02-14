using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.Lexer
{
  public static class LexemTypeGroup
  {
    public static LexemType[] Literal = new []
    {
      LexemType.IntLiteral,
      LexemType.FloatLiteral,
      LexemType.StringLiteral,
      LexemType.ComplexLiteral,
      LexemType.TrueLiteral,
      LexemType.FalseLiteral
    };

    public static LexemType[] Parameter = new[]
    {
      LexemType.IntLiteral,
      LexemType.FloatLiteral,
      LexemType.StringLiteral,
      LexemType.ComplexLiteral,
      LexemType.TrueLiteral,
      LexemType.FalseLiteral,
      LexemType.CurlyOpen,

      LexemType.Identifier,
      LexemType.New
    };

    public static LexemType[] Logical = new[]
    {
      LexemType.And,
      LexemType.Or,
      LexemType.NewLine
    };

    public static LexemType[] Sign2 = new[]
    {
      LexemType.Equal,
      LexemType.NotEqual,
      LexemType.Less,
      LexemType.Greater,
      LexemType.LessEqual,
      LexemType.GreaterEqual,
      LexemType.In,
      LexemType.NewLine
    };

    public static LexemType[] Sign3 = new[]
    {
      LexemType.Add,
      LexemType.Subtract,
      LexemType.NewLine
    };

    public static LexemType[] Sign4 = new[]
    {
      LexemType.Multiply,
      LexemType.Divide,
      LexemType.Remainder,
      LexemType.NewLine
    };

    public static LexemType[] Binary = new[]
    {
      LexemType.BinaryAnd,
      LexemType.BinaryOr,
      LexemType.BinaryXor,
      LexemType.BinaryShiftLeft,
      LexemType.BinaryShiftRight,
      LexemType.NewLine
    };

    public static LexemType[] Assignment = new[]
    {
      LexemType.Assign,
      LexemType.AssignAdd,
      LexemType.AssignSubtract,
      LexemType.AssignMultiply,
      LexemType.AssignDivide,
      LexemType.AssignRemainder,
      LexemType.AssignPower,
      LexemType.AssignShiftLeft,
      LexemType.AssignShiftRight,
      LexemType.Exchange
    };

    public static LexemType[] Print = new[]
    {
      LexemType.Print,
      LexemType.PrintLine
    };
  }
}
