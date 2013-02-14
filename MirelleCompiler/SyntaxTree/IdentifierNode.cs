using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirelle.Lexer;

namespace Mirelle.SyntaxTree
{
  abstract public class IdentifierNode: SyntaxTreeNode
  {
    /// <summary>
    /// The type name prefix (if called like type:identifier)
    /// </summary>
    public Lexem TypePrefix;

    /// <summary>
    /// The expression prefix (if called like expr.identifier)
    /// </summary>
    public SyntaxTreeNode ExpressionPrefix;

    /// <summary>
    /// Atmark flag (if called like @identifier)
    /// </summary>
    public bool AtmarkPrefix;

    /// <summary>
    /// Identifier name
    /// </summary>
    public string Name;

    /// <summary>
    /// The type to which the identifier belongs (as a method or a field)
    /// </summary>
    public string OwnerType = "";

    public IdentifierNode()
    {

    }

    public IdentifierNode(string name, bool atmark = false)
    {
      Name = name;
      AtmarkPrefix = atmark;
    }

    public IdentifierNode(string name, string typePrefix)
    {
      Name = name;
      TypePrefix = new Lexer.Lexem(Lexer.LexemType.Identifier, typePrefix);
    }

    public IdentifierNode(string name, SyntaxTreeNode expPrefix)
    {
      Name = name;
      ExpressionPrefix = expPrefix;
    }

    public IdentifierNode(IdentifierNode node)
    {
      Name = node.Name;
      AtmarkPrefix = node.AtmarkPrefix;
      ExpressionPrefix = node.ExpressionPrefix;
      TypePrefix = node.TypePrefix;
    }
  }
}
