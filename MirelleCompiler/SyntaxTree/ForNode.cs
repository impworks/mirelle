using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirelle.Lexer;

namespace Mirelle.SyntaxTree
{
  public class ForNode: LoopNode
  {
    /// <summary>
    /// Local key variable name
    /// </summary>
    public Lexem Key;

    /// <summary>
    /// Local variable name
    /// </summary>
    public Lexem Item;

    /// <summary>
    /// Iterable expression
    /// </summary>
    public SyntaxTreeNode Iterable;

    /// <summary>
    /// Create iteration code for an array
    /// </summary>
    /// <param name="emitter"></param>
    public void CompileArray(Emitter.Emitter emitter)
    {
      var iterType = Iterable.GetExpressionType(emitter);

      // make local variables only visible inside the scope
      emitter.CurrentMethod.Scope.EnterSubScope();
      var idxVar = emitter.CurrentMethod.Scope.Introduce("int", emitter.ResolveType("int"), Key == null ? null : Key.Data);
      var arrVar = emitter.CurrentMethod.Scope.Introduce(iterType, emitter.ResolveType(iterType));
      var currType = emitter.GetArrayItemType(iterType);
      var currVar = emitter.CurrentMethod.Scope.Introduce(currType, emitter.ResolveType(currType), Item.Data);

      // prelude: arr = ..., idx = -1
      Iterable.Compile(emitter);
      emitter.EmitSaveVariable(arrVar);
      emitter.EmitLoadInt(-1);
      emitter.EmitSaveVariable(idxVar);

      emitter.PlaceLabel(BodyStart);

      // increment
      emitter.EmitLoadVariable(idxVar);
      emitter.EmitLoadInt(1);
      emitter.EmitAdd();
      emitter.EmitSaveVariable(idxVar);

      // loop exit condition
      emitter.EmitLoadVariable(idxVar);
      emitter.EmitLoadVariable(arrVar);
      emitter.EmitLoadArraySize();
      emitter.EmitCompareLess();
      emitter.EmitBranchFalse(BodyEnd);

      // variable: curr = array[idx]
      emitter.EmitLoadVariable(arrVar);
      emitter.EmitLoadVariable(idxVar);
      emitter.EmitLoadIndex(currType);
      emitter.EmitSaveVariable(currVar);

      // body
      var preCurrLoop = emitter.CurrentLoop;
      emitter.CurrentLoop = this;
      Body.Compile(emitter);
      emitter.CurrentLoop = preCurrLoop;

      emitter.EmitBranch(BodyStart);

      emitter.PlaceLabel(BodyEnd);
      emitter.CurrentMethod.Scope.LeaveSubScope();
    }

    /// <summary>
    /// Create iteration code for a range
    /// </summary>
    /// <param name="emitter"></param>
    public void CompileRange(Emitter.Emitter emitter)
    {
      // make local variables only visible inside the scope
      emitter.CurrentMethod.Scope.EnterSubScope();
      var idxVar = emitter.CurrentMethod.Scope.Introduce("int", emitter.ResolveType("int"), Key == null ? null : Key.Data);
      var rangeVar = emitter.CurrentMethod.Scope.Introduce("range", emitter.ResolveType("range"));
      var currVar = emitter.CurrentMethod.Scope.Introduce("int", emitter.ResolveType("int"), Item.Data);

      // preface: range = ..., range.reset
      Iterable.Compile(emitter);
      emitter.EmitSaveVariable(rangeVar);
      emitter.EmitLoadVariable(rangeVar);
      emitter.EmitCall(emitter.FindMethod("range", "reset"));

      // set key if exists
      if(Key != null)
      {
        emitter.EmitLoadInt(-1);
        emitter.EmitSaveVariable(idxVar);
      }

      // range.next == false ? exit
      emitter.PlaceLabel(BodyStart);
      emitter.EmitLoadVariable(rangeVar);
      emitter.EmitCall(emitter.FindMethod("range", "next"));
      emitter.EmitLoadBool(false);
      emitter.EmitCompareEqual();
      emitter.EmitBranchTrue(BodyEnd);

      // curr = range.current
      emitter.EmitLoadVariable(rangeVar);
      emitter.EmitCall(emitter.FindMethod("range", "current"));
      emitter.EmitSaveVariable(currVar);

      // increment key if exists
      if(Key != null)
      {
        emitter.EmitLoadVariable(idxVar);
        emitter.EmitLoadInt(1);
        emitter.EmitAdd();
        emitter.EmitSaveVariable(idxVar);
      }

      // body
      var preCurrLoop = emitter.CurrentLoop;
      emitter.CurrentLoop = this;
      Body.Compile(emitter);
      emitter.CurrentLoop = preCurrLoop;

      emitter.EmitBranch(BodyStart);

      emitter.PlaceLabel(BodyEnd);
      emitter.CurrentMethod.Scope.LeaveSubScope();
    }

    /// <summary>
    /// Create iteration code for a dictionary
    /// </summary>
    /// <param name="emitter"></param>
    public void CompileDict(Emitter.Emitter emitter)
    {
      // check if key defined
      if (Key == null)
        Error(Resources.errDictIterKeyRequired);

      // make local variables only visible inside the scope
      emitter.CurrentMethod.Scope.EnterSubScope();
      var dictVar = emitter.CurrentMethod.Scope.Introduce("dict", emitter.ResolveType("dict"));
      var currVar = emitter.CurrentMethod.Scope.Introduce("string[]", emitter.ResolveType("string[]"));
      var keyVar = emitter.CurrentMethod.Scope.Introduce("string", emitter.ResolveType("string"), Key.Data);
      var itemVar = emitter.CurrentMethod.Scope.Introduce("string", emitter.ResolveType("string"), Item.Data);

      // preface: dictVar = ...;
      Iterable.Compile(emitter);
      emitter.EmitSaveVariable(dictVar);
      emitter.EmitLoadVariable(dictVar);
      emitter.EmitCall(emitter.FindMethod("dict", "reset"));

      // dict.next == false ? exit
      emitter.PlaceLabel(BodyStart);
      emitter.EmitLoadVariable(dictVar);
      emitter.EmitCall(emitter.FindMethod("dict", "next"));
      emitter.EmitLoadBool(false);
      emitter.EmitCompareEqual();
      emitter.EmitBranchTrue(BodyEnd);

      // curr = dict.current
      emitter.EmitLoadVariable(dictVar);
      emitter.EmitCall(emitter.FindMethod("dict", "current"));
      emitter.EmitSaveVariable(currVar);

      // (key, value) = curr
      emitter.EmitLoadVariable(currVar);
      emitter.EmitLoadInt(0);
      emitter.EmitLoadIndex("string");
      emitter.EmitSaveVariable(keyVar);
      emitter.EmitLoadVariable(currVar);
      emitter.EmitLoadInt(1);
      emitter.EmitLoadIndex("string");
      emitter.EmitSaveVariable(itemVar);

      // body
      var preCurrLoop = emitter.CurrentLoop;
      emitter.CurrentLoop = this;
      Body.Compile(emitter);
      emitter.CurrentLoop = preCurrLoop;

      emitter.EmitBranch(BodyStart);

      emitter.PlaceLabel(BodyEnd);
      emitter.CurrentMethod.Scope.LeaveSubScope();
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      // check variable
      if (emitter.CurrentMethod.Scope.Exists(Item.Data))
        Error(String.Format(Resources.errVariableRedefinition, Item.Data), Item);

      // check key if exists
      if (Key != null && emitter.CurrentMethod.Scope.Exists(Key.Data))
        Error(String.Format(Resources.errVariableRedefinition, Key.Data), Key);

      // define labels
      BodyStart = emitter.CreateLabel();
      BodyEnd = emitter.CreateLabel();

      var iterType = Iterable.GetExpressionType(emitter);
      if (iterType == "range")
        CompileRange(emitter);
      else if (iterType == "dict")
        CompileDict(emitter);
      else if (iterType.EndsWith("[]"))
        CompileArray(emitter);
      else
        Error(Resources.errForIterableExpected);
    }
  }
}
