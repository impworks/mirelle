using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Mirelle.Utils
{
  public class Scope
  {
    private Stack<HashList<ScopeVariable>> Data;
    public int MaxId;
    public MethodDefinition Method;

    public Scope(MethodDefinition method)
    {
      Data = new Stack<HashList<ScopeVariable>>();
      Data.Push(new HashList<ScopeVariable>());
      MaxId = 0;
      Method = method;
    }

    /// <summary>
    /// Adds a new subscope onto scope stack
    /// </summary>
    public void EnterSubScope()
    {
      Data.Push(new HashList<ScopeVariable>());
    }

    /// <summary>
    /// Remove current subscope from scope stack
    /// </summary>
    public void LeaveSubScope()
    {
      Data.Pop();
    }

    /// <summary>
    /// Check if any of the current subscopes contain the variable
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <returns></returns>
    public bool Exists(string name)
    {
      foreach (var curr in Data)
      {
        if (curr.Contains(name))
          return true;
      }

      return false;
    }

    /// <summary>
    /// Find a local variable in any of the subscopes
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <returns></returns>
    public ScopeVariable Find(string name)
    {
      foreach (var curr in Data)
      {
        if(curr.Contains(name))
          return curr[name];
      }

      return null;
    }

    /// <summary>
    /// Declare a local variable in the current subscope
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="typeName">Variable type as string</param>
    /// <param name="type">Variable type as TypeReference</param>
    /// <returns></returns>
    public ScopeVariable Introduce(string typeName, TypeReference type, string name = null)
    {
      if (name == null || name == "")
        name = CreateNewName();

      if (Exists(name))
        throw new CompilerException(String.Format(Resources.errVariableRedefinition, name));

      var actualVariable = new VariableDefinition(name, type);
      var local = new ScopeVariable(name, typeName, actualVariable);
      Method.Body.Variables.Add(actualVariable);
      Data.Peek().Add(name, local);

      return local;
    }

    /// <summary>
    /// Create a new unique name for a variable
    /// </summary>
    /// <returns></returns>
    public string CreateNewName()
    {
      var id = "$loc" + MaxId.ToString();
      MaxId++;
      return id;
    }
  }
}
