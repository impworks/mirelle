using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MirelleStdlib.Wireless;

namespace Mirelle.SyntaxTree
{
  public class SimulatePlannerNode: SyntaxTreeNode
  {
    /// <summary>
    /// Check for duplication
    /// </summary>
    public static bool Exists = false;

    /// <summary>
    /// The planner reference
    /// </summary>
    public SyntaxTreeNode Action;

    /// <summary>
    /// List of closured names
    /// </summary>
    public Dictionary<string, string> Closures = new Dictionary<string, string>();

    /// <summary>
    /// Current flow_sim call ID
    /// </summary>
    public string PlannerID = ".user_planner";

    public SimulatePlannerNode()
    {
      if (Exists)
        Error(Resources.errSimulateMultiple);
      else
        Exists = true;
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      var currType = emitter.CurrentType;
      var currMethod = emitter.CurrentMethod;
      emitter.CurrentType = emitter.FindType(PlannerID);

      // convert the IdentifierGet to IdentifierInvoke with two faux arguments
      AppendActionParameters(emitter);

      // extract closured variables
      var detector = new Utils.ClosureDetector();
      Closures = detector.Process(Action, emitter);
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
      emitter.CurrentMethod = emitter.FindMethod(PlannerID, "Action", "flow[]", "symbol");
      emitter.MakeMethodVirtual(emitter.CurrentMethod);

      LoadClosuredVariables(Closures, emitter);

      emitter.EmitLoadParameter(0);
      emitter.EmitLoadParameter(1);
      Action.Compile(emitter);

      // append the arguments to the user
      emitter.EmitReturn();
    }

    /// <summary>
    /// Generate code to setup the currently defined emitter
    /// </summary>
    /// <param name="emitter"></param>
    private void CompileInitiation(Emitter.Emitter emitter)
    {
      var flowSimType = emitter.FindType(PlannerID);
      var tmpVar = emitter.CurrentMethod.Scope.Introduce(PlannerID, flowSimType.Type);

      // tmp = new flowsimN()
      emitter.EmitNewObj(emitter.FindMethod(PlannerID, ".ctor"));
      emitter.EmitSaveVariable(tmpVar);

      SaveClosuredVariables(emitter, tmpVar);

      // call FlowSimulation.Start(planner)
      emitter.EmitLoadVariable(tmpVar);
      var method = emitter.AssemblyImport(typeof(FlowSimulation).GetMethod("Start", new[] { typeof(Planner) }));
      emitter.EmitCall(method);
    }

    /// <summary>
    /// Emit code for loading local variables from fields
    /// </summary>
    /// <param name="list">List of closures</param>
    private void LoadClosuredVariables(Dictionary<string, string> list, Emitter.Emitter emitter)
    {
      foreach (var curr in list)
      {
        emitter.EmitLoadThis();

        // load field
        var currField = emitter.FindField(PlannerID, "_" + curr.Key);
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
      foreach (var curr in Closures)
      {
        emitter.EmitLoadVariable(variable);

        // load variable
        var currVar = emitter.CurrentMethod.Scope.Find(curr.Key);
        emitter.EmitLoadVariable(currVar);

        // save into field
        var currField = emitter.FindField(PlannerID, "_" + curr.Key);
        emitter.EmitSaveField(currField);
      }
    }

    /// <summary>
    /// Create fields in the emitter type to save local variables intos
    /// </summary>
    private void DeclareClosuredVariables(Emitter.Emitter emitter)
    {
      var flowSimType = emitter.FindType(PlannerID);

      foreach (var curr in Closures)
      {
        var fld = emitter.CreateField(flowSimType, "_" + curr.Key, new SignatureNode(curr.Value));
        emitter.PrepareField(fld);
      }
    }

    /// <summary>
    /// Convert the IdentifierGet node to an IdentifierInvoke node with two dummy parameters
    /// </summary>
    /// <param name="emitter"></param>
    public void AppendActionParameters(Emitter.Emitter emitter)
    {
      // make sure it's a proper getter
      var act = Action as IdentifierGetNode;
      if (act == null || act.AtmarkPrefix)
        Error(Resources.errPlannerExpression);

      var node = Expr.IdentifierInvoke(act, Expr.Dummy("flow[]"), Expr.Dummy("symbol"));
      string type = "";
      try
      {
        // checking if it compiles properly
        type = node.GetExpressionType(emitter);
      }
      catch
      {
        Error(Resources.errPlannerArgsMismatch);
      }

      if (type != "symbol")
        Error(Resources.errPlannerTypeMismatch);

      Action = node;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "flow_sim_result";
    }
  }
}
