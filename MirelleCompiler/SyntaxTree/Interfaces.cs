using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  /// <summary>
  /// A marker interface for statements
  /// </summary>
  public interface IStatement { }

  /// <summary>
  /// A marker interface for expressions
  /// </summary>
  public interface IExpression { }

  /// <summary>
  /// A marker interface for expressions that can be a lvalue
  /// </summary>
  public interface IAssignable { }

  /// <summary>
  /// A marker interface for constant expressions
  /// </summary>
  public interface IConstant { }
}
