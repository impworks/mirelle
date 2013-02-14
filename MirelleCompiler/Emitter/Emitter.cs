using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mirelle.SyntaxTree;


namespace Mirelle.Emitter
{
  public partial class Emitter
  {
    /// <summary>
    /// Reference to compiler
    /// </summary>
    public Compiler Compiler;

    /// <summary>
    /// Assembly to create and output
    /// </summary>
    private AssemblyDefinition Assembly;

    /// <summary>
    /// The magic method to put all global instructions to
    /// </summary>
    private MethodDefinition GlobalBody;
    /// <summary>
    /// The type to store magic method
    /// </summary>
    private TypeDefinition GlobalType;

    /// <summary>
    /// The marker interface for all Mirelle-defined types
    /// </summary>
    private TypeReference MirelleTypeInterface;

    /// <summary>
    /// The marker interface for all Mirelle-defined enums
    /// </summary>
    private TypeReference MirelleEnumInterface;

    /// <summary>
    /// Type currently being compiled
    /// </summary>
    public TypeNode CurrentType;
    /// <summary>
    /// Method currently being compiled
    /// </summary>
    public MethodNode CurrentMethod;
    /// <summary>
    /// Loop currently being compiled
    /// </summary>
    public LoopNode CurrentLoop;

    /// <summary>
    /// The root node for all types, methods and all other stuff
    /// </summary>
    public RootNode RootNode = new RootNode();

    /// <summary>
    /// List of types declared as visible by the 'use' keyword
    /// </summary>
    public List<TypeNode> VisibleTypes = new List<TypeNode>();

    public Emitter(Compiler compiler)
    {
      Compiler = compiler;

      // create assembly object
      var name = new AssemblyNameDefinition("MirelleCompiled", new Version(1, 0, 0, 0));
      Assembly = AssemblyDefinition.CreateAssembly(name, "MirelleCompiled", ModuleKind.Console);

      var attr = typeof(STAThreadAttribute).GetConstructor(new Type[] { } );

      // register global method
      GlobalBody = new MethodDefinition("main", MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig, Assembly.MainModule.TypeSystem.Void);
      GlobalBody.CustomAttributes.Add(new CustomAttribute(AssemblyImport(attr)));
      RootNode.GlobalMethod = new MethodNode("main", new SignatureNode("void"), true, false, GlobalBody);
      RootNode.GlobalMethod.Scope = new Utils.Scope(GlobalBody);

      // register global type
      GlobalType = new TypeDefinition("MirelleCompiled", ".program", TypeAttributes.AutoClass | TypeAttributes.Public | TypeAttributes.SpecialName | TypeAttributes.BeforeFieldInit, Assembly.MainModule.TypeSystem.Object);
      Assembly.MainModule.Types.Add(GlobalType);
      GlobalType.Methods.Add(GlobalBody);
      Assembly.EntryPoint = GlobalBody;

      // register marker interfaces
      MirelleTypeInterface = AssemblyImport(typeof(MirelleStdlib.IMirelleType));
      MirelleEnumInterface = AssemblyImport(typeof(MirelleStdlib.IMirelleEnum));
    }

    /// <summary>
    /// Output assembly to disk
    /// </summary>
    /// <param name="name">Filename to save as</param>
    public void SaveAssembly(string name)
    {
      Assembly.Write(name);
    }
  }
}
