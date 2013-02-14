using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mirelle.Lexer;

namespace Mirelle.SyntaxTree
{
  /// <summary>
  /// Generic interface for all syntax tree nodes
  /// </summary>
  public abstract class SyntaxTreeNode
  {
    /// <summary>
    /// Lexem that the current node is bound to
    /// </summary>
    public Lexem Lexem;

    /// <summary>
    /// Expression type
    /// </summary>
    protected string ExpressionType = "";

    /// <summary>
    /// Emit the code of the node into current method
    /// </summary>
    /// <param name="emitter">Emitter</param>
    public abstract void Compile(Emitter.Emitter emitter);

    /// <summary>
    /// Calculate expression type
    /// </summary>
    /// <param name="emitter">Emitter</param>
    /// <returns></returns>
    public virtual string GetExpressionType(Emitter.Emitter emitter)
    {
      return ExpressionType;
    }

    /// <summary>
    /// Throw an error message bound to current lexem
    /// </summary>
    /// <param name="msg">Error message</param>
    public void Error(string msg)
    {
      throw new CompilerException(msg, Lexem);
    }

    /// <summary>
    /// Throw an error message bound to some other lexem
    /// </summary>
    /// <param name="msg">Error message</param>
    /// <param name="lexem">Lexem</param>
    public void Error(string msg, Lexem lexem)
    {
      throw new CompilerException(msg, lexem);
    }

    /// <summary>
    /// Return children for closure detector traversing
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<SyntaxTreeNode> Children()
    {
      return null;
    }

    /// <summary>
    /// Return a local variable name if the current node represents one
    /// </summary>
    /// <returns></returns>
    public virtual string ClosuredName(Emitter.Emitter emitter)
    {
      return null;
    }
  }
}
