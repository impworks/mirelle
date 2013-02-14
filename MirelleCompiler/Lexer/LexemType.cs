using System;
using System.Collections.Generic;
using System.Text;

namespace Mirelle.Lexer
{
  public enum LexemType
  {
    Unknown,
    EOF,

    // Keywords
    Type,
    Var,
    If,
    Else,
    For,
    While,
    In,
    Do,
    Return,
    Include,
    Autoconstruct,
    Break,
    Redo,
    Emit,
    Static,
    New,
    Void,
    Use,
    Print,
    PrintLine,
    As,
    Null,
    Exit,
    With,
    Every,
    Limit,
    Until,
    Enum,
    Simulate,
    Once,

    // Operators
    Add,
    Subtract,
    Multiply,
    Divide,
    Remainder,
    Power,
    Inc,
    Dec,
    And,
    Or,
    Not,
    BinaryAnd,
    BinaryOr,
    BinaryXor,
    BinaryShiftLeft,
    BinaryShiftRight,

    // Comparison
    Equal,
    NotEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,

    // Assignment
    Assign,
    AssignAdd,
    AssignSubtract,
    AssignMultiply,
    AssignDivide,
    AssignRemainder,
    AssignPower,
    AssignShiftLeft,
    AssignShiftRight,
    Exchange,

    // Special operators
    DoubleDot,
    Dot,
    Colon,
    Semicolon,
    NewLine,
    Arrow,
    Comma,
    Tilde,

    // Braces
    ParenOpen,
    ParenClose,
    SquareOpen,
    SquareClose,
    DoubleSquareOpen,
    DoubleSquareClose,
    CurlyOpen,
    CurlyClose,

    // Literals
    TrueLiteral,
    FalseLiteral,
    IntLiteral,
    FloatLiteral,
    StringLiteral,
    ComplexLiteral,

    Identifier
  }
}
