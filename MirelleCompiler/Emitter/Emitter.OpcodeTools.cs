using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Mirelle.SyntaxTree;
using Mirelle.Utils;

namespace Mirelle.Emitter
{
  public partial class Emitter
  {
    /// <summary>
    /// Return a method's IL processor
    /// </summary>
    /// <param name="method">Optional method</param>
    /// <returns></returns>
    public ILProcessor GetMethodProcessor()
    {
      return ((MethodDefinition)CurrentMethod.Method).Body.GetILProcessor();
    }


    /// <summary>
    /// Load from a local variable
    /// </summary>
    /// <param name="id">VariableD</param>
    public void EmitLoadVariable(ScopeVariable var)
    {
      GetMethodProcessor().Emit(OpCodes.Ldloc, var.Var);
    }


    /// <summary>
    /// Save to a local variable
    /// </summary>
    /// <param name="id">Variable</param>
    public void EmitSaveVariable(ScopeVariable var)
    {
      GetMethodProcessor().Emit(OpCodes.Stloc, var.Var);
    }


    /// <summary>
    /// Load a variable address
    /// </summary>
    /// <param name="id">Variable</param>
    public void EmitLoadVariableAddress(ScopeVariable var)
    {
      GetMethodProcessor().Emit(OpCodes.Ldloca, var.Var);
    }


    /// <summary>
    /// Load a field
    /// </summary>
    /// <param name="field">Field node</param>
    public void EmitLoadField(FieldNode field)
    {
      var mp = GetMethodProcessor();
      if (field.Static)
        mp.Emit(OpCodes.Ldsfld, field.Field);
      else
        mp.Emit(OpCodes.Ldfld, field.Field);
    }

    /// <summary>
    /// Load a field by reference
    /// </summary>
    /// <param name="field">Field reference</param>
    /// <param name="isStatic">Static?</param>
    public void EmitLoadField(FieldReference field, bool isStatic = false)
    {
      var mp = GetMethodProcessor();
      if (isStatic)
        mp.Emit(OpCodes.Ldsfld, field);
      else
        mp.Emit(OpCodes.Ldfld, field);
    }


    /// <summary>
    /// Save a field
    /// </summary>
    /// <param name="field">Field node</param>
    public void EmitSaveField(FieldNode field)
    {
      var mp = GetMethodProcessor();
      if (field.Static)
        mp.Emit(OpCodes.Stsfld, field.Field);
      else
        mp.Emit(OpCodes.Stfld, field.Field);
    }

    /// <summary>
    /// Save a field by reference
    /// </summary>
    /// <param name="field">Field reference</param>
    /// <param name="isStatic">Static?</param>
    public void EmitSaveField(FieldReference field, bool isStatic = false)
    {
      var mp = GetMethodProcessor();
      if (isStatic)
        mp.Emit(OpCodes.Stsfld, field);
      else
        mp.Emit(OpCodes.Stfld, field);
    }


    /// <summary>
    /// Load a parameter
    /// </summary>
    /// <param name="id">Parameter identifier</param>
    public void EmitLoadParameter(int id)
    {
      // account for 'this' being the 0th argument
      if (!CurrentMethod.Static) id++;

      GetMethodProcessor().Emit(OpCodes.Ldarg, id);
    }


    /// <summary>
    /// Save to a parameter
    /// </summary>
    /// <param name="id">Parameter identifier</param>
    public void EmitSaveParameter(int id)
    {
      // account for 'this' being the 0th argument
      if (!CurrentMethod.Static) id++;

      GetMethodProcessor().Emit(OpCodes.Starg, id);
    }


    /// <summary>
    /// Load an index from an array
    /// </summary>
    /// <param name="type">Data type</param>
    public void EmitLoadIndex(string type)
    {
      var mp = GetMethodProcessor();
      switch(type)
      {
        case "bool": mp.Emit(OpCodes.Ldelem_I1); return;
        case "int": mp.Emit(OpCodes.Ldelem_I4); return;
        case "float": mp.Emit(OpCodes.Ldelem_R8); return;
        default: mp.Emit(OpCodes.Ldelem_Ref); return;
      }
    }


    /// <summary>
    /// Load an array item's address onto the stack
    /// </summary>
    /// <param name="type"></param>
    public void EmitLoadIndexAddress(TypeReference type)
    {
      GetMethodProcessor().Emit(OpCodes.Ldelema, type);
    }


    /// <summary>
    /// Save to an index of an array
    /// </summary>
    /// <param name="type"></param>
    public void EmitSaveIndex(string type)
    {
      var mp = GetMethodProcessor();
      switch (type)
      {
        case "bool": mp.Emit(OpCodes.Stelem_I1); return;
        case "int": mp.Emit(OpCodes.Stelem_I4); return;
        case "float": mp.Emit(OpCodes.Stelem_R8); return;
        default: mp.Emit(OpCodes.Stelem_Ref); return;
      }
    }


    /// <summary>
    /// Load the 'this' pointer
    /// </summary>
    public void EmitLoadThis()
    {
      if (CurrentMethod.Static)
        throw new CompilerException(Resources.errThisInStaticContext);

      GetMethodProcessor().Emit(OpCodes.Ldarg, 0);
    }


    /// <summary>
    /// Load an integer
    /// </summary>
    /// <param name="value"></param>
    /// <param name="method"></param>
    public void EmitLoadInt(int value)
    {
      var mp = GetMethodProcessor();
      switch(value)
      {
        case 0: mp.Emit(OpCodes.Ldc_I4_0); return;
        case 1: mp.Emit(OpCodes.Ldc_I4_1); return;
        case 2: mp.Emit(OpCodes.Ldc_I4_2); return;
        case 3: mp.Emit(OpCodes.Ldc_I4_3); return;
        case 4: mp.Emit(OpCodes.Ldc_I4_4); return;
        case 5: mp.Emit(OpCodes.Ldc_I4_5); return;
        case 6: mp.Emit(OpCodes.Ldc_I4_6); return;
        case 7: mp.Emit(OpCodes.Ldc_I4_7); return;
        case -1: mp.Emit(OpCodes.Ldc_I4_M1); return;
      }

      if(value > -127 && value < 127)
        mp.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
      else
        mp.Emit(OpCodes.Ldc_I4, value);
    }


    /// <summary>
    /// Load a booleam
    /// </summary>
    /// <param name="value"></param>
    /// <param name="method"></param>
    public void EmitLoadBool(bool value)
    {
      GetMethodProcessor().Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
    }


    /// <summary>
    /// Load a float
    /// </summary>
    /// <param name="value"></param>
    /// <param name="method"></param>
    public void EmitLoadFloat(double value)
    {
      GetMethodProcessor().Emit(OpCodes.Ldc_R8, value);
    }


    /// <summary>
    /// Load a string
    /// </summary>
    /// <param name="value"></param>
    /// <param name="method"></param>
    public void EmitLoadString(string value)
    {
      GetMethodProcessor().Emit(OpCodes.Ldstr, value);
    }


    /// <summary>
    /// Load a null value
    /// </summary>
    public void EmitLoadNull()
    {
      GetMethodProcessor().Emit(OpCodes.Ldnull);
    }

    
    /// <summary>
    /// Load a default value for the type
    /// </summary>
    /// <param name="type">Type name</param>
    public void EmitLoadDefaultValue(string type)
    {
      switch(type)
      {
        case "bool":        EmitLoadBool(false); break;
        case "int":         EmitLoadInt(0); break;
        case "float":       EmitLoadFloat(0); break;
        case "string":      EmitLoadString(""); break;

        case "complex":     EmitLoadFloat(0);
                            EmitLoadFloat(0);
                            EmitNewObj(FindMethod("complex", ".ctor", "float", "float")); break;

        default:            EmitLoadNull(); break;
      }
    }


    /// <summary>
    /// Load a value-type object from the address
    /// </summary>
    /// <param name="type"></param>
    public void EmitLoadObject(TypeReference type)
    {
      GetMethodProcessor().Emit(OpCodes.Ldobj, type);
    }


    /// <summary>
    /// Save the object by the address
    /// </summary>
    /// <param name="type"></param>
    public void EmitSaveObject(TypeReference type)
    {
      GetMethodProcessor().Emit(OpCodes.Stobj, type);
    }


    /// <summary>
    /// Call a method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="method"></param>
    public void EmitCall(MethodNode node)
    {
      GetMethodProcessor().Emit((node.Virtual ? OpCodes.Callvirt : OpCodes.Call), node.Method);
    }


    /// <summary>
    /// Call a virtual method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="method"></param>
    public void EmitCallVirtual(MethodNode node)
    {
      GetMethodProcessor().Emit(OpCodes.Callvirt, node.Method);
    }


    /// <summary>
    /// Call a method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="method"></param>
    public void EmitCall(MethodReference callee)
    {
      GetMethodProcessor().Emit(OpCodes.Call, callee);
    }


    /// <summary>
    /// Return from an expression
    /// </summary>
    public void EmitReturn()
    {
      GetMethodProcessor().Emit(OpCodes.Ret);
    }


    /// <summary>
    /// Pop an item out of stack
    /// </summary>
    public void EmitPop()
    {
      GetMethodProcessor().Emit(OpCodes.Pop);
    }


    /// <summary>
    /// Create a new array of type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="method"></param>
    public void EmitNewArray(TypeReference type)
    {
      GetMethodProcessor().Emit(OpCodes.Newarr, type);
    }


    /// <summary>
    /// Sum two items on the stack
    /// </summary>
    /// <param name="method"></param>
    public void EmitAdd()
    {
      GetMethodProcessor().Emit(OpCodes.Add);
    }


    /// <summary>
    /// Multiply two items on the stack
    /// </summary>
    /// <param name="method"></param>
    public void EmitMul()
    {
      GetMethodProcessor().Emit(OpCodes.Mul);
    }


    /// <summary>
    /// Subtract two items on the stack
    /// </summary>
    /// <param name="method"></param>
    public void EmitSub()
    {
      GetMethodProcessor().Emit(OpCodes.Sub);
    }


    /// <summary>
    /// Divide two items on the stack
    /// </summary>
    /// <param name="method"></param>
    public void EmitDiv()
    {
      GetMethodProcessor().Emit(OpCodes.Div);
    }


    /// <summary>
    /// Divide two items on the stack and return remainder
    /// </summary>
    /// <param name="method"></param>
    public void EmitRem()
    {
      GetMethodProcessor().Emit(OpCodes.Rem);
    }

    /// <summary>
    /// Convert top item of the stack to int
    /// </summary>
    public void EmitConvertToBool()
    {
      GetMethodProcessor().Emit(OpCodes.Conv_I1);
    }

    /// <summary>
    /// Convert top item of the stack to int
    /// </summary>
    public void EmitConvertToInt()
    {
      GetMethodProcessor().Emit(OpCodes.Conv_I4);
    }

    /// <summary>
    /// Convert top item of the stack to float
    /// </summary>
    /// <param name="method"></param>
    public void EmitConvertToFloat()
    {
      GetMethodProcessor().Emit(OpCodes.Conv_R8);
    }

    /// <summary>
    /// Upcast a basic numeric type
    /// </summary>
    /// <param name="from">Source type</param>
    /// <param name="to">Destination type</param>
    public void EmitUpcastBasicType(string from, string to)
    {
      // @TODO: complex, long
      if (from == "int" && to == "float")
        EmitConvertToFloat();

      if(from.IsAnyOf("int", "float") && to == "complex")
      {
        if (from == "int")
          EmitConvertToFloat();

        EmitLoadFloat(0);
        EmitNewObj(FindMethod("complex", ".ctor", "float", "float"));
      }
    }

    /// <summary>
    /// Create a new object using the constructor
    /// </summary>
    /// <param name="ctor">Constructor method node</param>
    /// <param name="method"></param>
    public void EmitNewObj(MethodNode ctor)
    {
      GetMethodProcessor().Emit(OpCodes.Newobj, ctor.Method);
    }

    /// <summary>
    /// Create a new object using the constructor
    /// </summary>
    /// <param name="ctor">Constructor method reference</param>
    /// <param name="method"></param>
    public void EmitNewObj(MethodReference ctor)
    {
      GetMethodProcessor().Emit(OpCodes.Newobj, ctor);
    }

    /// <summary>
    /// Compare two basic types for equality
    /// </summary>
    /// <param name="method"></param>
    public void EmitCompareEqual()
    {
      GetMethodProcessor().Emit(OpCodes.Ceq);
    }

    /// <summary>
    /// Compare two basic types for <
    /// </summary>
    /// <param name="method"></param>
    public void EmitCompareLess()
    {
      GetMethodProcessor().Emit(OpCodes.Clt);
    }

    /// <summary>
    /// Compare two basic types for >
    /// </summary>
    /// <param name="method"></param>
    public void EmitCompareGreater()
    {
      GetMethodProcessor().Emit(OpCodes.Cgt);
    }

    /// <summary>
    /// Jump to a specific instruction in the code
    /// </summary>
    /// <param name="location">A nop instruction to jump to</param>
    /// <param name="method"></param>
    public void EmitBranch(Instruction location)
    {
      GetMethodProcessor().Emit(OpCodes.Br, location);
    }

    /// <summary>
    /// Jump to a specific instruction in the code if the result is 'true'
    /// </summary>
    /// <param name="location">A nop instruction to jump to</param>
    /// <param name="method"></param>
    public void EmitBranchTrue(Instruction location)
    {
      GetMethodProcessor().Emit(OpCodes.Brtrue, location);
    }

    /// <summary>
    /// Jump to a specific instruction in the code if the result is 'false'
    /// </summary>
    /// <param name="location">A nop instruction to jump to</param>
    /// <param name="method"></param>
    public void EmitBranchFalse(Instruction location)
    {
      GetMethodProcessor().Emit(OpCodes.Brfalse, location);
    }

    /// <summary>
    /// Box a value object to a reference object
    /// </summary>
    /// <param name="type">Type reference</param>
    public void EmitBox(TypeReference type)
    {
      GetMethodProcessor().Emit(OpCodes.Box, type);
    }

    /// <summary>
    /// Load array length as integer
    /// </summary>
    public void EmitLoadArraySize()
    {
      GetMethodProcessor().Emit(OpCodes.Ldlen);
    }

    /// <summary>
    /// Perform a binary AND on the arguments
    /// </summary>
    public void EmitAnd()
    {
      GetMethodProcessor().Emit(OpCodes.And);
    }

    /// <summary>
    /// Perform a binary OR on the arguments
    /// </summary>
    public void EmitOr()
    {
      GetMethodProcessor().Emit(OpCodes.Or);
    }

    /// <summary>
    /// Perform a binary XOR on the arguments
    /// </summary>
    public void EmitXor()
    {
      GetMethodProcessor().Emit(OpCodes.Xor);
    }

    /// <summary>
    /// Perform a binary left shift on the arguments
    /// </summary>
    public void EmitShiftLeft()
    {
      GetMethodProcessor().Emit(OpCodes.Shl);
    }

    /// <summary>
    /// Perform a binary right shift on the arguments
    /// </summary>
    public void EmitShiftRight()
    {
      GetMethodProcessor().Emit(OpCodes.Shr);
    }

    /// <summary>
    /// Cast to a type
    /// </summary>
    /// <param name="type"></param>
    public void EmitCast(TypeReference type)
    {
      GetMethodProcessor().Emit(OpCodes.Castclass, type);
    }

    /// <summary>
    /// Create a label to jump to
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public Instruction CreateLabel()
    {
      return GetMethodProcessor().Create(OpCodes.Nop);
    }

    /// <summary>
    /// Put the label into some place
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public void PlaceLabel(Instruction instruction)
    {
      GetMethodProcessor().Append(instruction);
    }

    /// <summary>
    /// Emit a code to load global arguments into io:args field
    /// </summary>
    public void EmitInitialize()
    {
      // save parameters of main() to io:args
      GlobalBody.Parameters.Add(new ParameterDefinition(AssemblyImport(typeof(string).MakeArrayType())));
      var ip = GlobalBody.Body.GetILProcessor();
      ip.Emit(OpCodes.Ldarg_0);
      ip.Emit(OpCodes.Stsfld, FindField("io", "args").Field);

      // set current culture and other options
      ip.Emit(OpCodes.Call, AssemblyImport(typeof(MirelleStdlib.Initializer).GetMethod("Initialize")));
    }
  }
}
