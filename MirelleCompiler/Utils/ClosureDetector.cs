using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirelle.SyntaxTree;

namespace Mirelle.Utils
{
  public class ClosureDetector
  {
    /// <summary>
    /// List of bound variables in an expression
    /// </summary>
    public Dictionary<string, string> Variables;

    /// <summary>
    /// Create the list of bound variables
    /// </summary>
    /// <param name="expr">Expression to parse</param>
    /// <param name="emitter">Emitter</param>
    /// <returns></returns>
    public Dictionary<string, string> Process(SyntaxTreeNode expr, Emitter.Emitter emitter)
    {
      Variables = new Dictionary<string, string>();
      ProcessR(expr, emitter);
      return Variables;
    }

    /// <summary>
    /// Recursively parse the expression and try to find local variables
    /// </summary>
    /// <param name="expr">Expression to parse</param>
    /// <param name="emitter">Emitter</param>
    private void ProcessR(SyntaxTreeNode expr, Emitter.Emitter emitter)
    {
      if (expr == null) return;

      // traverse all children
      var children = expr.Children();
      if (children != null)
      {
        foreach (var child in children)
          ProcessR(child, emitter);
      }

      // check current expression for being a local variable
      if(expr is IdentifierGetNode)
      {
        var name = expr.ClosuredName(emitter);
        if (name != null)
          Variables.Add(name, expr.GetExpressionType(emitter));
      }
    }
  }
}
