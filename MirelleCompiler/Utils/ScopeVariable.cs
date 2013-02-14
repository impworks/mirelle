using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil.Cil;

namespace Mirelle.Utils
{
  public class ScopeVariable
  {
    public string Name;
    public string Type;
    public VariableDefinition Var;

    public ScopeVariable(string name, string type, VariableDefinition variable)
    {
      Name = name;
      Type = type;

      Var = variable;
    }
  }
}
