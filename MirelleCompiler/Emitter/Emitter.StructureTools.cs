using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MN = MathNet.Numerics.LinearAlgebra.Double;
using MND = MathNet.Numerics.Distributions;

using Mirelle.SyntaxTree;

using Mono.Cecil;

namespace Mirelle.Emitter
{
  public partial class Emitter
  {
    /// <summary>
    /// Create underlying entities for methods and fields
    /// </summary>
    public void Prepare()
    {
      foreach(var curr in RootNode.Types)
      {
        var currType = RootNode.Types[curr];

        // only registered types should be prepared, built-in are already defined
        if(currType.BuiltIn) continue;

        GenerateToString(currType);

        if (!currType.Methods.Contains(".ctor"))
          GenerateDefaultCtor(currType);

        if (currType.AutoConstruct)
          GenerateAutoCtor(currType);

        if (currType.Enum)
          GenerateEnumToArray(currType);
        else
          GenerateAutoComparator(currType);

        // prepare fields
        foreach (var currField in currType.Fields)
        {
          try
          {
            PrepareField(currType.Fields[currField]);
          }
          catch(CompilerException ex)
          {
            ex.AffixToLexem(currType.Fields[currField].Lexem);
            throw;
          }
        }

        // prepare methods
        foreach (var currMethodName in currType.Methods)
        {
          foreach (var currMethod in currType.Methods[currMethodName])
          {
            try
            {
              PrepareMethod(currMethod);
            }
            catch(CompilerException ex)
            {
              ex.AffixToLexem(currMethod.Lexem);
              throw;
            }
          }
        }
      }
    }

    /// <summary>
    /// Create underlying FieldReference in assembly
    /// </summary>
    /// <param name="field">Field node</param>
    public void PrepareField(FieldNode field)
    {
      // calculate required attributes
      var attribs = FieldAttributes.Public;
      if (field.Static)
        attribs |= FieldAttributes.Static;

      // create field in assembly
      var nativeField = new FieldDefinition(field.Name, attribs, ResolveType(field.Type));
      field.Field = nativeField;
      ((TypeDefinition)field.Owner.Type).Fields.Add(nativeField);
    }

    /// <summary>
    /// Create underlying MethodReference in assembly
    /// </summary>
    /// <param name="method">Method to prepare</param>
    public void PrepareMethod(MethodNode method)
    {
      // check if method should be virtual and set it's parent to virtual too
      if (method.Name != ".ctor")
      {
        var shadowedMethod = FindShadowedMethod(method);
        if (shadowedMethod != null)
        {
          method.Virtual = shadowedMethod.Virtual = true;

          if (shadowedMethod.Method != null && !shadowedMethod.BuiltIn)
            (shadowedMethod.Method as MethodDefinition).IsVirtual = true;
        }
      }

      // calculate required attributes
      var attribs = MethodAttributes.HideBySig;
      if (method.Static)
        attribs |= MethodAttributes.Static;
      if (method.Virtual)
        attribs |= MethodAttributes.Virtual;
      if (method.Name == ".ctor" || method.Name == ".cctor")
        attribs |= MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

      if (method.Name == ".cctor")
        attribs |= MethodAttributes.Private;
      else
        attribs |= MethodAttributes.Public;

      // create method in assembly
      method.Type.CompiledType = ResolveType(method.Type);
      var nativeMethod = new MethodDefinition(method.Name, attribs, ResolveType(method.Type));
      method.Method = nativeMethod;
      method.Scope = new Utils.Scope(nativeMethod);
      ((TypeDefinition)method.Owner.Type).Methods.Add(nativeMethod);

      // add parameter info in assembly
      if(method.Parameters != null)
        foreach (var curr in method.Parameters)
          method.Method.Parameters.Add(new ParameterDefinition(curr, ParameterAttributes.None, ResolveType(method.Parameters[curr].Type)));
    }


    /// <summary>
    /// Create an automatic constructor to set all fields
    /// </summary>
    /// <param name="type">Type node</param>
    private void GenerateAutoCtor(TypeNode type)
    {
      var node = new MethodNode(".ctor", new SignatureNode("void"), false);

      var parameters = new HashList<ParameterNode>();
      int idx = 0;
      foreach(var curr in type.Fields)
      {
        var param = new ParameterNode("_" + curr, type.Fields[curr].Type, idx);
        parameters.Add("_" + curr, param);
        idx++;

        var assignNode = new IdentifierSetNode(curr, true);
        assignNode.Expression = new IdentifierGetNode("_" + curr);
        node.Body.Statements.Add(assignNode);
      }

      node.SetParameters(parameters);
      node.Owner = type;
      AddMethod(type, node);
    }

    /// <summary>
    /// Create an automatic ToString() method
    /// </summary>
    /// <param name="type">Type node</param>
    private void GenerateToString(TypeNode type)
    {
      var node = new MethodNode("ToString", new SignatureNode("string"), false);
      node.Virtual = true;

      node.Body.Statements.Add(Expr.Return(Expr.String(type.Name)));

      node.Owner = type;
      AddMethod(type, node);
    }


    /// <summary>
    /// Create a default constructor without parameters
    /// </summary>
    /// <param name="type">Type node</param>
    private void GenerateDefaultCtor(TypeNode type)
    {
      var node = new MethodNode(".ctor", new SignatureNode("void"), false);
      node.Body.Statements.Add(Expr.Return());

      node.SetParameters();
      node.Owner = type;
      AddMethod(type, node);
    }

    /// <summary>
    /// Create an automatic .Equal() method
    /// </summary>
    /// <param name="type">Type node</param>
    private void GenerateAutoComparator(TypeNode type)
    {
      var node = new MethodNode("equal", new SignatureNode("bool"), false);

      // define parameter
      var parameters = new HashList<ParameterNode>();
      var param = new ParameterNode("_obj", new SignatureNode(type.Name), 0);
      parameters.Add(param.Name, param);

      // generate series of comparisons
      foreach(var curr in type.Fields)
      {
        //
        // if(@<name> != _obj.<name>) return false;
        //
        node.Body.Statements.Add(
          Expr.If(
            Expr.Compare(
              Expr.IdentifierGet(curr, true),
              Lexer.LexemType.NotEqual,
              Expr.IdentifierGet(curr, Expr.IdentifierGet("_obj"))
            ),
            Expr.Return(
              Expr.Bool(true)
            )
          )
        );
      }

      // return true
      node.Body.Statements.Add(
        Expr.Return(
          Expr.Bool(true)
        )
      );

      node.SetParameters(parameters);
      node.Owner = type;
      AddMethod(type, node);
    }

    /// <summary>
    /// Create a to_a static method for the enum.
    /// </summary>
    /// <param name="owner">Owner type</param>
    private void GenerateEnumToArray(TypeNode owner)
    {
      // exclude .ctor, to_s, to_a and ToString
      var count = owner.Methods.Count - 4;
      var to_a = FindMethod(owner.Name, "to_a");

      
      // return arr
      to_a.Body = Expr.CodeBlock(

        // var arr = new [null as <enum>] * count
        Expr.Var(
          "names",
          Expr.Multiply(
            Expr.Array(
              Expr.String()
            ),
            Expr.Int(count)
          )
        ),

        // previous content
        to_a.Body,

        Expr.Var(
          "arr",
          Expr.Multiply(
            Expr.Array(
              Expr.As(
                Expr.Null(),
                owner.Name
              )
            ),
            Expr.Int(count)
          )
        ),

        // for idx in 0..(count-1) do
        Expr.For(
          "idx",
          Expr.Range(
            Expr.Int(0),
            Expr.Int(count-1)
          ),
          // arr[idx] = new <enum> idx, names[idx]
          Expr.ArraySet(
            Expr.IdentifierGet("arr"),
            Expr.IdentifierGet("idx"),
            Expr.New(
              owner.Name,
              Expr.IdentifierGet("idx"),
              Expr.ArrayGet(
                Expr.IdentifierGet("names"),
                Expr.IdentifierGet("idx")
              )
            )
          )
        ),

        // return arr
        Expr.Return(
          Expr.IdentifierGet("arr")
        )
      );
    }

    /// <summary>
    /// Create a type node
    /// </summary>
    /// <param name="name">Type name</param>
    /// <returns></returns>
    public TypeNode CreateType(string name)
    {
      // check if type already exists
      if (RootNode.Types.Contains(name))
        throw new CompilerException(String.Format(Resources.errTypeRedefinition, name));

      // add type to the assembly
      var attribs = TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass;
      var nativeType = new TypeDefinition("MirelleCompiled", name, attribs, Assembly.MainModule.TypeSystem.Object);
      nativeType.Interfaces.Add(MirelleTypeInterface);
      Assembly.MainModule.Types.Add(nativeType);

      var type = new TypeNode(name, "", false, nativeType);
      RootNode.Types.Add(name, type);
      return type;
    }

    /// <summary>
    /// Create a type node
    /// </summary>
    /// <param name="name">Type name</param>
    /// <param name="parent">Parent name</param>
    /// <returns></returns>
    public TypeNode CreateType(string name, string parent)
    {
      // check if type already exists
      if (RootNode.Types.Contains(name))
        throw new CompilerException(String.Format(Resources.errTypeRedefinition, name));

      // check if type's parent does not exist
      if (!RootNode.Types.Contains(parent))
        throw new CompilerException(String.Format(Resources.errTypeParentNotFound, name, parent));

      // check if the parent type has a default constructor
      try
      {
        FindMethod(parent, ".ctor");
        throw new CompilerException(String.Format(Resources.errTypeParentDefaultCtor, parent));
      }
      catch { }

      // add type to the assembly and Mirelle registry
      var attribs = TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass;
      var nativeType = new TypeDefinition("MirelleCompiled", name, attribs, RootNode.Types[parent].Type);
      Assembly.MainModule.Types.Add(nativeType);
      var type = new TypeNode(name, parent, false, nativeType);
      RootNode.Types.Add(name, type);
      return type;
    }

    /// <summary>
    /// Create a field in a type
    /// </summary>
    /// <param name="owner">Owner type node</param>
    /// <param name="name">Field name</param>
    /// <param name="type">Field type signature</param>
    /// <param name="isStatic">Static flag</param>
    /// <returns></returns>
    public FieldNode CreateField(TypeNode owner, string name, SignatureNode type, bool isStatic = false)
    {
      // cut the atmark
      if(name[0] == '@')
        name = name.SafeSubstring(1, name.Length - 1);

      // cannot add fields to built-in types
      if (owner.BuiltIn)
        throw new CompilerException(String.Format(Resources.errExtendBuiltInType, owner.Name));

      // check if such a field has already been registered
      if (owner.Fields.Contains(name))
        throw new CompilerException(String.Format(Resources.errFieldRedefinition, name, owner.Name));

      // create field in Mirelle registry
      var field = new FieldNode(name, type, isStatic);
      owner.Fields.Add(name, field);
      field.Owner = owner;
      return field;
    }

    /// <summary>
    /// Create a method in a type
    /// </summary>
    /// <param name="owner">Owner type node</param>
    /// <param name="name">Method name</param>
    /// <param name="type">Method return value type signature</param>
    /// <param name="parameters">Method parameters list</param>
    /// <param name="isStatic">Static flag</param>
    /// <returns></returns>
    public MethodNode CreateMethod(TypeNode owner, string name, SignatureNode type, HashList<ParameterNode> parameters = null, bool isStatic = false, bool prepare = false)
    {
      // cannot add methods to built-in types
      if (owner.BuiltIn)
        throw new CompilerException(String.Format(Resources.errExtendBuiltInType, owner.Name));

      // create method in assembly and in Mirelle Registry
      var method = new MethodNode(name, type, isStatic);

      method.SetParameters(parameters);

      // check if an identical method hasn't already been declared
      if (owner.Methods.Contains(method.Name))
      {
        if(owner.Methods[method.Name].Exists(curr => curr.Signature == method.Signature))
          throw new CompilerException(String.Format(Resources.errMethodRedefinition, method.Name, owner.Name));
      }

      // register method in the parent type
      AddMethod(owner, method);
      method.Owner = owner;

      if (prepare)
        PrepareMethod(method);

      return method;
    }

    /// <summary>
    /// Create an enumerable type in Mirelle
    /// </summary>
    /// <param name="name">Enum name</param>
    /// <returns></returns>
    public TypeNode CreateEnum(string name, params string[] items)
    {
      // mark type as enum
      var type = CreateType(name);
      (type.Type as TypeDefinition).Interfaces.Add(MirelleEnumInterface);
      type.Enum = true;

      // add field
      var valueField = CreateField(type, "value", new SignatureNode("int"));
      valueField.Private = true;
      var captionField = CreateField(type, "caption", new SignatureNode("string"));
      captionField.Private = true;

      // add constructor
      var ctor = CreateCtor(type, new HashList<ParameterNode>()
        {
          { "value", new ParameterNode("value", new SignatureNode("int"), 0) },
          { "caption", new ParameterNode("caption", new SignatureNode("string"), 1) },
        });

      ctor.Body = Expr.CodeBlock(
        Expr.IdentifierSet(
          "value",
          true,
          Expr.IdentifierGet("value")
        ),

        Expr.IdentifierSet(
          "caption",
          true,
          Expr.IdentifierGet("caption")
        ),

        Expr.Return()
      );

      // add to_s
      var to_s = CreateMethod(type, "to_s", new SignatureNode("string"));
      to_s.Body = Expr.CodeBlock(
        Expr.Return(
          Expr.Add(
            Expr.String(type.Name + ":"),
            Expr.IdentifierGet("caption", true)
          )
        )
      );

      // add to_a
      CreateMethod(type, "to_a", new SignatureNode(name + "[]"), null, true);

      // create items
      foreach (var curr in items)
        CreateEnumValue(type, curr);

      // return the created types
      return type;
    }


    /// <summary>
    /// Create a new value in the enumerable type
    /// </summary>
    /// <param name="owner">Owner type</param>
    /// <param name="name">Value name</param>
    public void CreateEnumValue(TypeNode owner, string name)
    {
      // get the identifier
      // .ctor and to_s  are not counted
      // while to_a will be added later
      var value = owner.Methods.Count - 3;

      // check for redefinition
      if(owner.Methods.Contains(name))
        throw new CompilerException(String.Format(Resources.errEnumRedefinition, name, owner.Name));

      // create the method
      var method = CreateMethod(owner, name, new SignatureNode(owner.Name), null, true);

      // create a new instance with a separate identifier
      method.Body = Expr.CodeBlock(
        Expr.Return(
          Expr.New(
            owner.Name,
            Expr.Int(value),
            Expr.String(name)
          )
        )
      );

      // save corresponding info to to_a
      var to_a = FindMethod(owner.Name, "to_a");
      to_a.Body.Statements.Add(
        Expr.ArraySet(
          Expr.IdentifierGet("names"),
          Expr.Int(value),
          Expr.String(name)
        )
      );
    }

    /// <summary>
    /// Create type instance constructor
    /// </summary>
    /// <param name="owner">Owner type node</param>
    /// <param name="parameters">List of parameters</param>
    /// <returns></returns>
    public MethodNode CreateCtor(TypeNode owner, HashList<ParameterNode> parameters = null, bool prepare = false)
    {
      return CreateMethod(owner, ".ctor", new SignatureNode("void"), parameters, false, prepare);
    }

    /// <summary>
    /// Create type static constructor
    /// </summary>
    /// <param name="owner">Owner type node</param>
    /// <returns></returns>
    public MethodNode CreateStaticCtor(TypeNode owner, bool prepare = false)
    {
      return CreateMethod(owner, ".cctor", new SignatureNode("void"), null, true, prepare);
    }

    /// <summary>
    /// Create field initializer
    /// </summary>
    /// <param name="parent">Owner type node</param>
    /// <returns></returns>
    public MethodNode CreateInitializer(TypeNode owner, bool prepare = false)
    {
      return CreateMethod(owner, ".init", new SignatureNode("void"), null, false, prepare);
    }

    /// <summary>
    /// Create a type that represents the planner
    /// </summary>
    public void CreatePlannerType()
    {
      var type = CreateType(".user_planner", ".planner");
      var action = CreateMethod(type,
                                "Action",
                                new SignatureNode("symbol"),
                                new HashList<ParameterNode>
                                {
                                  {
                                    "flows",
                                    new ParameterNode("flows", new SignatureNode("flow[]"), 1)
                                  },
                                  {
                                    "old_symbol",
                                    new ParameterNode("old_symbol", new SignatureNode("symbol"), 2)
                                  }
                                }
                              );
      action.BuiltIn = true;
    }

    /// <summary>
    /// Find a type
    /// </summary>
    /// <param name="type">Type name</param>
    /// <returns></returns>
    public TypeNode FindType(string type)
    {
      // do not throw exception if type is empty
      if (type == "" || type == "null")
        return null;

      if (RootNode.Types.Contains(type))
        return RootNode.Types[type];
      else
        throw new CompilerException(String.Format(Resources.errTypeNotFound, type));
    }

    /// <summary>
    /// Add a method to the type
    /// </summary>
    /// <param name="type">Type node</param>
    /// <param name="method">Method node</param>
    public void AddMethod(TypeNode type, MethodNode method)
    {
      if (type.Methods.Contains(method.Name))
        type.Methods[method.Name].Add(method);
      else
        type.Methods.Add(method.Name, new List<MethodNode> { method });
    }

    /// <summary>
    /// Find a method in a type or it's ancestors
    /// </summary>
    /// <param name="type">Type name to search in</param>
    /// <param name="signature">Method parameter signature, name and then types separated by spaces</param>
    /// <returns></returns>
    public MethodNode FindMethod(string type, string signature)
    {
      var space = new[] { ' ' };
      var sigParts = signature.Split(space, 2);
      string[] parameters = new string[0];
      if(sigParts.Length == 2)
        parameters = sigParts[1].Split(space);

      return FindMethod(type, sigParts[0], parameters);
    }
    
    /// <summary>
    /// Find the most appropriate method in the type, it's ancestors or visible types
    /// </summary>
    /// <param name="type">Type name to search in</param>
    /// <param name="checkVisible">Flag allowing to look in visible types too</param>
    /// <param name="name">Method name</param>
    /// <param name="parameters">List of parameter types</param>
    /// <returns></returns>
    public MethodNode FindMethod(string type, bool checkVisible, string name, params string[] parameters)
    {
      // aggregate the list of types
      var methodList = new List<MethodNode>();
      var currTypeName = type;
      while(currTypeName != "")
      {
        var currType = FindType(currTypeName);
        if (currType.Methods.Contains(name))
          methodList.AddRange(currType.Methods[name]);
        currTypeName = currType.Parent;
      }

      // append the visible types if allowed
      if (checkVisible)
      {
        foreach(var currType in VisibleTypes)
        {
          if (!currType.Methods.Contains(name)) continue;

          foreach(var currMethod in currType.Methods[name])
          {
            // method constraint: only static methods can be found in visible classes
            if (currMethod.Static)
              methodList.Add(currMethod);
          }
        }
      }

      MethodNode bestCandidate = null;
      MethodNode ambigious = null;
      int bestDistance = -1;

      // loop through types
      foreach (var currMethod in methodList)
      {
        // method constraint: parameter count must match
        if(currMethod.Parameters.Count != parameters.Length) continue;

        // method constraint: method should not be hidden
        if(currMethod.Private && CurrentType != currMethod.Owner) continue;

        // method constraint: methods that share the same signature are overloads,
        // thus only the first one (being topmost) is accounted for, the rest are skipped
        // so that they don't create ambiguity
        if(bestCandidate != null
            && currMethod.Signature == bestCandidate.Signature
            && TypeIsParent(currMethod.Owner.Name, bestCandidate.Owner.Name)
          ) continue;

        // calculate total parameter distance
        var currDistance = TotalDistance(parameters, currMethod.Parameters);
        if(currDistance == -1) continue;

        // the method call is ambigious!
        if (bestDistance == currDistance)
          ambigious = currMethod;

        // a new best candidate is found!
        if(bestDistance == -1 || bestDistance > currDistance)
        {
          bestDistance = currDistance;
          bestCandidate = currMethod;
          ambigious = null;
        }
      }

      if (bestCandidate == null)
      {
        if(type != "")
          throw new CompilerException(String.Format(Resources.errMethodNotFoundInType, name, parameters.Join(" "), type));
        else
          throw new CompilerException(String.Format(Resources.errMethodNotFound, name, parameters.Join(" ")));
      }

      if (ambigious != null)
        throw new CompilerException(String.Format(Resources.errAmbigiousCall, name,
          bestCandidate.Owner.Name + ":" + bestCandidate.Signature,
          ambigious.Owner.Name + ":" + ambigious.Signature));

      return bestCandidate;
    }

    /// <summary>
    /// Find the most appropriate method in the type or it's ancestors
    /// </summary>
    /// <param name="type">Type name to search in</param>
    /// <param name="name">Method name</param>
    /// <param name="parameters">List of parameter types</param>
    /// <returns></returns>
    public MethodNode FindMethod(string type, string name, params string[] parameters)
    {
      return FindMethod(type, false, name, parameters);
    }

    /// <summary>
    /// Find out if there's a method that the current method overloads
    /// </summary>
    /// <param name="node">Method node</param>
    /// <returns></returns>
    public MethodNode FindShadowedMethod(MethodNode node)
    {
      var parentType = FindType(node.Owner.Parent);
      while(parentType != null)
      {
        if(parentType.Methods.Contains(node.Name))
        {
          var overloads = parentType.Methods[node.Name];
          var found = overloads.Find(curr => curr.Signature == node.Signature);
          if(found != null)
            return found;
        }

        parentType = FindType(parentType.Parent);
      }

      return null;
    }

    /// <summary>
    /// Check if a method with such a name exists in the type or it's ancestors
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool MethodNameExists(string type, string name)
    {
      var currType = FindType(type);
      while(currType != null)
      {
        if (currType.Methods.Contains(name)) return true;
        currType = FindType(currType.Parent);
      }

      return false;
    }

    /// <summary>
    /// Find a field in a type or it's ancestors
    /// </summary>
    /// <param name="type">Type name to search in</param>
    /// <param name="name">Field name</param>
    /// <returns></returns>
    public FieldNode FindField(string type, string name)
    {
      var currType = FindType(type);
      while (currType != null)
      {
        // search in the current type for the field
        if (currType.Fields.Contains(name))
        {
          var fld = currType.Fields[name];
          if (!fld.Private || CurrentType == fld.Owner)
            return fld;
        }

        // or try it's parent if no luck
        currType = FindType(currType.Parent);
      }

      // no luck at all
      throw new CompilerException(String.Format(Resources.errFieldNotFound, name, type));
    }

    /// <summary>
    /// Resolve a type in a SignatureNode
    /// </summary>
    /// <param name="node">Signature node</param>
    /// <returns></returns>
    public TypeReference ResolveType(SignatureNode node)
    {
      node.CompiledType = ResolveType(node.Signature);
      return node.CompiledType;
    }

    /// <summary>
    /// Resolve a type by it's signature
    /// </summary>
    /// <param name="signature">Signature in string form</param>
    /// <returns></returns>
    public TypeReference ResolveType(string signature)
    {
      // special case of "void"
      if (signature.SafeSubstring(0, 4) == "void")
      {
        if (signature.Length > 4)
          throw new CompilerException(Resources.errVoidCompoundType);
        else
          return Assembly.MainModule.TypeSystem.Void;
      }

      // find the base type in defined types
      string baseTypeName = signature.Contains("[") ? signature.Substring(0, signature.IndexOf('[')) : signature;
      if (!RootNode.Types.Contains(baseTypeName))
        throw new CompilerException(String.Format(Resources.errTypeNotFound, baseTypeName));

      TypeReference type = FindType(baseTypeName).Type;

      // wrap type into array if required
      int offset = baseTypeName.Length;
      while (offset < signature.Length)
      {
        // array
        if (signature.SafeSubstring(offset, 2) == "[]")
        {
          type = new ArrayType(type);
          offset += 2;
        }
        // signature incorrect
        else
          throw new CompilerException(String.Format(Resources.errIncorrectSignature, signature));
      }

      return type;
    }

    /// <summary>
    /// Unwrap array item type from array type
    /// </summary>
    /// <param name="type">Type signature</param>
    /// <returns></returns>
    public string GetArrayItemType(string type)
    {
      if (type.Contains("[]"))
        return type.Substring(0, type.Length - 2);
      else
        return "";
    }

    /// <summary>
    /// Resolve a basic type by it's name
    /// </summary>
    /// <param name="typeName">Type name</param>
    /// <returns></returns>
    private Type ResolveBasicType(string typeName)
    {
      // detect type
      var arrIndex = typeName.Contains("[]") ? (typeName.Length - typeName.IndexOf('[')) / 2 : 0;
      typeName = typeName.Replace("[]", "");
      switch (typeName)
      {
        case "void": typeName = "System.Void"; break;
        case "bool": typeName = "System.Boolean"; break;
        case "int": typeName = "System.Int32"; break;
        case "float": typeName = "System.Double"; break;
        case "string": typeName = "System.String"; break;
        case "complex": typeName = typeof(System.Numerics.Complex).AssemblyQualifiedName; break;
        case "matrix": typeName = typeof(MN.DenseMatrix).AssemblyQualifiedName; break;
        case "distr": typeName = typeof(MND.IContinuousDistribution).AssemblyQualifiedName; break;
        case "range": typeName = typeof(MirelleStdlib.Range).AssemblyQualifiedName; break;
        case "file": typeName = typeof(MirelleStdlib.File).AssemblyQualifiedName; break;
        case "dict": typeName = typeof(MirelleStdlib.Dict).AssemblyQualifiedName; break;
        case "chart": typeName = typeof(MirelleStdlib.Chart.Chart).AssemblyQualifiedName; break;
        case "series": typeName = typeof(MirelleStdlib.Chart.Series).AssemblyQualifiedName; break;
        case "sim_result": typeName = typeof(MirelleStdlib.Events.SimulationResult).AssemblyQualifiedName; break;
        case "flow": typeName = typeof(MirelleStdlib.Wireless.Flow).AssemblyQualifiedName; break;
        case "block": typeName = typeof(MirelleStdlib.Wireless.Block).AssemblyQualifiedName; break;
        case "symbol": typeName = typeof(MirelleStdlib.Wireless.Symbol).AssemblyQualifiedName; break;
        case "flow_type": typeName = typeof(MirelleStdlib.Wireless.FlowType).AssemblyQualifiedName; break;
        case "modulation": typeName = typeof(MirelleStdlib.Wireless.Modulation).AssemblyQualifiedName; break;
        case "flow_sim_result": typeName = typeof(MirelleStdlib.Wireless.FlowSimulationResult).AssemblyQualifiedName; break;
      }

      // wrap it into array
      var type = Type.GetType(typeName);
      while(arrIndex > 0)
      {
        type = type.MakeArrayType();
        arrIndex--;
      }

      return type;
    }

    /// <summary>
    /// Return the inheritance path length between two types
    /// </summary>
    /// <param name="parent">Parent name</param>
    /// <param name="child">Child name</param>
    /// <returns></returns>
    public int TypeDistance(string parent, string child)
    {
      // special case: numeric types
      // @TODO: add long and complex types when implemented
      if (parent.IsAnyOf("bool", "int", "float", "long", "complex"))
        return NumericTypeDistance(parent, child);

      // special case: voids
      if (child == "void" && parent == "void") return 0;

      // special case: null can be casted to any type, but you can't cast to null
      if (child == "null") return 0;
      if (parent == "null") return -1;

      // array types must match completely 
      if(child.Contains("[]"))
        return parent == child ? 0 : -1;

      var distance = 0;
      var currType = FindType(child);

      while(currType != null)
      {
        if(currType.Name == parent)
          return distance;

        distance++;
        currType = FindType(currType.Parent);
      }

      return -1;
    }

    /// <summary>
    /// Return the upcast path length between two numeric types
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <returns></returns>
    public int NumericTypeDistance(string parent, string child)
    {
      // possibly not the best algorithm, but since there's not going to be
      // any more types, this is the fastest-working version possible
      // might need remaking in case more numeric types are to be introduced, though it's unlikely
      if (parent == child) return 0;

      if (child == "int" && parent.IsAnyOf("long", "float")) return 1;
      if (child == "int" && parent == "complex") return 2;
      if (child == "float" && parent == "complex") return 1;

      return -1;
    }

    /// <summary>
    /// Calculate the total distance between a list of possible and actual types
    /// </summary>
    /// <param name="actualTypes">Actual list of parameter types at invokation</param>
    /// <param name="possibleTypes">List of types in the current method</param>
    /// <returns></returns>
    public int TotalDistance(string[] actualTypes, HashList<ParameterNode> possibleTypes)
    {
      var totalDistance = 0;
      var idx = 0;
      foreach(var currType in actualTypes)
      {
        var currDistance = TypeDistance(possibleTypes[idx].Type.Signature, currType);

        // types do not relate
        if (currDistance == -1)
          return -1;
        else
          totalDistance += currDistance;

        idx++;
      }

      return totalDistance;
    }

    /// <summary>
    /// Check if a type is a parent of another type
    /// </summary>
    /// <param name="parent">Parent type name</param>
    /// <param name="child">Child type name</param>
    /// <returns></returns>
    public bool TypeIsParent(string parent, string child)
    {
      return TypeDistance(parent, child) >= 0;
    }

    /// <summary>
    /// Check if type can possibly be casted from one to another
    /// </summary>
    /// <param name="from">Source type</param>
    /// <param name="to">Destination type</param>
    /// <returns></returns>
    public bool TypeCastable(string from, string to)
    {
      return TypeIsParent(from, to) || TypeIsParent(to, from);
    }

    /// <summary>
    /// Declare that a type's static methods are globally visible as functions
    /// </summary>
    /// <param name="typeName">Type name</param>
    public void UseType(string typeName)
    {
      // make sure the type exists
      var type = FindType(typeName);

      // check if it's not been included already
      if (VisibleTypes.Contains(type))
        throw new CompilerException(String.Format(Resources.errUseTypeDuplicated, typeName));

      VisibleTypes.Add(type);
    }

    /// <summary>
    /// Explicitly mark method as virtual
    /// </summary>
    /// <param name="method">Method to work on</param>
    public void MakeMethodVirtual(MethodNode method)
    {
      var nativeMethod = method.Method as MethodDefinition;
      if(nativeMethod != null)
        nativeMethod.Attributes |= MethodAttributes.Virtual;
      method.Virtual = true;
    }
  }
}
