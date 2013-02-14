using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MirelleStdlib.Events;
using Mono.Cecil;

namespace Mirelle.SyntaxTree
{
  public class EmitNode: SyntaxTreeNode
  {
    /// <summary>
    /// The total number of emitter types created
    /// </summary>
    public static int EmitterCount = 0;

    /// <summary>
    /// Action handler (emit ...)
    /// </summary>
    public SyntaxTreeNode Action;

    /// <summary>
    /// Condition handler (until ...)
    /// </summary>
    public SyntaxTreeNode Condition;

    /// <summary>
    /// Step value (every ...)
    /// </summary>
    public SyntaxTreeNode Step;

    /// <summary>
    /// Probability distribution (with ...)
    /// </summary>
    public SyntaxTreeNode Distribution;

    /// <summary>
    /// Event limit value (limit ...)
    /// </summary>
    public SyntaxTreeNode Limit;

    /// <summary>
    /// The number of the current emitter
    /// </summary>
    public string EmitterID;

    /// <summary>
    /// Closured names used in Action
    /// </summary>
    public Dictionary<string, string> ActionClosures = new Dictionary<string, string>();

    /// <summary>
    /// Closured names used in Condition
    /// </summary>
    public Dictionary<string, string> ConditionClosures = new Dictionary<string, string>();

    /// <summary>
    /// Total list of closured names
    /// </summary>
    public Dictionary<string, string> Closures;

    public EmitNode()
    {
      EmitterID = ".emitter" + EmitterCount.ToString();
      EmitterCount++;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var currType = emitter.CurrentType;
      var currMethod = emitter.CurrentMethod;
      emitter.CurrentType = emitter.FindType(EmitterID);

      // extract closured variables
      var detector = new Utils.ClosureDetector();
      ActionClosures = detector.Process(Action, emitter);
      if (Condition != null)
        ConditionClosures = detector.Process(Condition, emitter);
      MergeClosures();
      DeclareClosuredVariables(emitter);

      // generate closured code for emitter
      CompileBody(emitter);

      emitter.CurrentType = currType;
      emitter.CurrentMethod = currMethod;

      CompileInitiation(emitter);
    }

    /// <summary>
    /// Generate action and condition (possibly) for the currently defined emitter
    /// </summary>
    /// <param name="emitter"></param>
    private void CompileBody(Emitter.Emitter emitter)
    {
      // compile action
      emitter.CurrentMethod = emitter.FindMethod(EmitterID, "Action");
      emitter.MakeMethodVirtual(emitter.CurrentMethod);

      LoadClosuredVariables(ActionClosures, emitter);
      Action.Compile(emitter);
      if (!Action.GetExpressionType(emitter).IsAnyOf("void", ""))
        emitter.EmitPop();
      emitter.EmitReturn();

      // compile condition, if any
      if (Condition != null)
      {
        if (Condition.GetExpressionType(emitter) != "bool")
          Error(Resources.errEmitConditionExpected);

        emitter.CurrentMethod = emitter.FindMethod(EmitterID, "Condition");
        emitter.MakeMethodVirtual(emitter.CurrentMethod);
        LoadClosuredVariables(ConditionClosures, emitter);
        Condition.Compile(emitter);
        emitter.EmitReturn();
      }
    }

    /// <summary>
    /// Generate code to setup the currently defined emitter
    /// </summary>
    /// <param name="emitter"></param>
    private void CompileInitiation(Emitter.Emitter emitter)
    {
      var emitterType = emitter.FindType(EmitterID);
      var tmpVar = emitter.CurrentMethod.Scope.Introduce(EmitterID, emitterType.Type);

      // tmp = new emitterN()
      emitter.EmitNewObj(emitter.FindMethod(EmitterID, ".ctor"));
      emitter.EmitSaveVariable(tmpVar);

      // step
      if (Step != null)
      {
        // validate step
        var stepType = Step.GetExpressionType(emitter);
        if (!stepType.IsAnyOf("int", "float"))
          Error(Resources.errEmitStepExpected);

        // tmp.Step = step
        emitter.EmitLoadVariable(tmpVar);
        Step.Compile(emitter);
        if (stepType == "int")
          emitter.EmitConvertToFloat();
        emitter.EmitSaveField(emitter.FindField(EmitterID, "step"));
      }

      // distribution
      if (Distribution != null)
      {
        // validate distr
        if (Distribution.GetExpressionType(emitter) != "distr")
          Error(Resources.errEmitDistributionExpected);

        // tmp.Distr = distr
        emitter.EmitLoadVariable(tmpVar);
        Distribution.Compile(emitter);
        emitter.EmitSaveField(emitter.FindField(EmitterID, "distr"));
      }

      // limit
      if (Limit != null)
      {
        // validate distr
        if (Limit.GetExpressionType(emitter) != "int")
          Error(Resources.errEmitLimitExpected);

        // tmp.Distr = distr
        emitter.EmitLoadVariable(tmpVar);
        Limit.Compile(emitter);
        emitter.EmitSaveField(emitter.FindField(EmitterID, "limit"));
      }

      SaveClosuredVariables(emitter, tmpVar);

      // register emitter in the system
      emitter.EmitLoadVariable(tmpVar);
      var registerMethod = emitter.AssemblyImport(typeof(Simulation).GetMethod("RegisterEmitter", new[] { typeof(EventEmitter) }));
      emitter.EmitCall(registerMethod);
    }

    /// <summary>
    /// Emit code for loading local variables from fields
    /// </summary>
    /// <param name="list">List of closures</param>
    private void LoadClosuredVariables(Dictionary<string, string> list, Emitter.Emitter emitter)
    {
      foreach(var curr in list)
      {
        emitter.EmitLoadThis();

        // load field
        var currField = emitter.FindField(EmitterID, "_" + curr.Key);
        emitter.EmitLoadField(currField);

        // save into variable
        var currVar = emitter.CurrentMethod.Scope.Introduce(curr.Value, emitter.ResolveType(curr.Value), curr.Key);
        emitter.EmitSaveVariable(currVar);
      }
    }

    /// <summary>
    /// Emit code for saving local variables into fields
    /// </summary>
    private void SaveClosuredVariables(Emitter.Emitter emitter, Utils.ScopeVariable variable)
    {
      foreach(var curr in Closures)
      {
        emitter.EmitLoadVariable(variable);

        // load variable
        var currVar = emitter.CurrentMethod.Scope.Find(curr.Key);
        emitter.EmitLoadVariable(currVar);

        // save into field
        var currField = emitter.FindField(EmitterID, "_" + curr.Key);
        emitter.EmitSaveField(currField);
      }
    }

    /// <summary>
    /// Create fields in the emitter type to save local variables intos
    /// </summary>
    private void DeclareClosuredVariables(Emitter.Emitter emitter)
    {
      var emitterType = emitter.FindType(EmitterID);

      foreach(var curr in Closures)
      {
        var fld = emitter.CreateField(emitterType, "_" + curr.Key, new SignatureNode(curr.Value));
        emitter.PrepareField(fld);
      }
    }

    /// <summary>
    /// Merge action and condition closure lists into one,
    /// removing duplicates
    /// </summary>
    private void MergeClosures()
    {
      if (Condition == null)
        Closures = ActionClosures;
      else
      {
        // traverse condition closures and extract unique ones
        Closures = new Dictionary<string, string>(ActionClosures);
        foreach(var curr in ConditionClosures)
        {
          if (!Closures.ContainsKey(curr.Key))
            Closures.Add(curr.Key, curr.Value);
        }
      }
    }
  }
}
