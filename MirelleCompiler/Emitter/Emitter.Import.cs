using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SR = System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Mirelle.SyntaxTree;

namespace Mirelle.Emitter
{
  public partial class Emitter
  {
    /// <summary>
    /// Import a type from any assembly into Mirelle Registry
    /// </summary>
    /// <param name="name">Type name</param>
    /// <param name="actualType">Type reference</param>
    /// <param name="parent">Parent type name (must be already imported)</param>
    /// <returns></returns>
    public TypeNode ImportType(Type actualType, string name, string parent = "")
    {
      // type has already been defined?
      if (RootNode.Types.Contains(name))
        throw new CompilerException(String.Format(Resources.errTypeRedefinition, name));

      FindType(parent);

      // add type to the assembly and Mirelle registry
      var importedType = AssemblyImport(actualType);
      var type = new TypeNode(name, parent, true, importedType);
      RootNode.Types.Add(name, type);
      return type;
    }

    /// <summary>
    /// Import a type from any assembly into Mirelle registry and mark it as an enum
    /// </summary>
    /// <param name="actualType">Type reference</param>
    /// <param name="name">Type name</param>
    /// <returns></returns>
    public TypeNode ImportEnum(Type actualType, string name)
    {
      var type = ImportType(actualType, name);
      type.Enum = true;
      return type;
    }

    /// <summary>
    /// Import a method from any assembly into Mirelle registry
    /// </summary>
    /// <param name="baseType">Actual type</param>
    /// <param name="baseName">Method name within actual type</param>
    /// <param name="owner">Owner type name within Mirelle</param>
    /// <param name="name">Method name within mirelle</param>
    /// <param name="type">Type</param>
    /// <param name="isStatic">Static flag</param>
    /// <param name="parameters">Parameter list</param>
    /// <returns></returns>
    public MethodNode ImportMethod(Type baseType, string baseName, string owner, string name, string type, bool isStatic = false, params string[] parameters)
    {
      var ownerType = FindType(owner);
      if (ownerType == null)
        throw new CompilerException(String.Format(Resources.errTypeNotFound, owner));

      // find method in the base type and import it
      var types = new Type[parameters.Length];
      var idx = 0;
      foreach (var curr in parameters)
      {
        types[idx] = ResolveBasicType(curr);
        idx++;
      }

      MethodReference importedMethod;
      if(baseName == ".ctor")
        importedMethod = AssemblyImport(baseType.GetConstructor(types));
      else
        importedMethod = AssemblyImport(baseType.GetMethod(baseName, types));

      var typeNode = new SignatureNode(type);
      ResolveType(typeNode);
      var method = new MethodNode(name, typeNode, isStatic, true, importedMethod);

      // parameters
      var sb = new StringBuilder(name);
      idx = 0;
      foreach (var curr in parameters)
      {
        // declared as extension method ?
        // first parameter is the invoker itself, remove it from parameters list
        if (!importedMethod.HasThis && !isStatic && idx == 0)
        {
          idx++;
          continue;
        }

        var signNode = new SignatureNode(curr);
        sb.Append(" ");
        sb.Append(curr);
        var param = new ParameterNode("p" + idx.ToString(), signNode, idx);
        method.Parameters.Add(param.Name, param);
        idx++;
      }
      method.Signature = sb.ToString();

      // check if an identical method hasn't already been declared
      if (ownerType.Methods.Contains(method.Signature))
        throw new CompilerException(String.Format(Resources.errMethodRedefinition, method.Name, owner));

      // register method in the owner type
      AddMethod(ownerType, method);
      method.Owner = ownerType;
      return method;
    }

    /// <summary>
    /// Import a constructor
    /// </summary>
    /// <param name="baseType">Base type</param>
    /// <param name="owner">Owner type name within Mirelle</param>
    /// <param name="parameters">Parameter types</param>
    /// <returns></returns>
    public MethodNode ImportCtor(Type baseType, string owner, params string[] parameters)
    {
      return ImportMethod(baseType, ".ctor", owner, ".ctor", "void", false, parameters);
    }

    /// <summary>
    /// Import a field
    /// </summary>
    /// <param name="baseType">Actual type</param>
    /// <param name="baseName">Name of the field in actual type</param>
    /// <param name="owner">Owner type name within Mirelle</param>
    /// <param name="name">Field name within Mirelle</param>
    /// <param name="type">Field value type</param>
    /// <param name="isStatic">Static flag</param>
    /// <returns></returns>
    public FieldNode ImportField(Type baseType, string baseName, string owner, string name, string type, bool isStatic = false)
    {
      // check if type exists
      var ownerType = FindType(owner);
      if (ownerType == null)
        throw new CompilerException(String.Format(Resources.errTypeNotFound, owner));

      // check if field is not yet defined
      try
      {
        FindField(owner, name);
        throw new CompilerException(String.Format(Resources.errFieldRedefinition, name, owner));
      }
      catch { }

      var importedField = AssemblyImport(baseType.GetField(baseName));

      var typeNode = new SignatureNode(type);
      ResolveType(typeNode);

      var field = new FieldNode(name, typeNode, isStatic, importedField);
      ownerType.Fields.Add(name, field);

      return field;
    }

    /// <summary>
    /// Import a method as an enum value
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="baseName"></param>
    /// <param name="owner"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public MethodNode ImportEnumValue(Type baseType, string baseName, string owner, string name)
    {
      return ImportMethod(baseType, baseName, owner, name, owner, true);
    }


    /// <summary>
    /// Assembly type import routine
    /// </summary>
    /// <param name="type">Type declaration</param>
    /// <returns></returns>
    public TypeReference AssemblyImport(Type type)
    {
      return Assembly.MainModule.Import(type);
    }

    /// <summary>
    /// Assembly method import routine
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public MethodReference AssemblyImport(SR.MethodInfo method)
    {
      return Assembly.MainModule.Import(method);
    }

    /// <summary>
    /// Assembly constructor import routine
    /// </summary>
    /// <param name="ctor"></param>
    /// <returns></returns>
    public MethodReference AssemblyImport(SR.ConstructorInfo ctor)
    {
      return Assembly.MainModule.Import(ctor);
    }

    /// <summary>
    /// Assembly field import routine
    /// </summary>
    /// <param name="ctor"></param>
    /// <returns></returns>
    public FieldReference AssemblyImport(SR.FieldInfo field)
    {
      return Assembly.MainModule.Import(field);
    }
  }
}
