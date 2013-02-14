using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public enum IdentifierKind
  {
    Variable,
    Field,
    Method,
    Parameter,

    StaticField,
    StaticMethod,

    SizeProperty,

    Unresolved
  }
}
