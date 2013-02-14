using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle.SyntaxTree
{
  public class ComplexNode: SyntaxTreeNode
  {
    /// <summary>
    /// The real part
    /// </summary>
    public double Real;

    /// <summary>
    /// The imaginary part
    /// </summary>
    public double Imaginary;

    public ComplexNode(double real = 0, double imaginary = 0)
    {
      Real = real;
      Imaginary = imaginary;
    }

    public override string GetExpressionType(Emitter.Emitter emitter)
    {
      return "complex";
    }

    public override void Compile(Emitter.Emitter emitter)
    {
      emitter.EmitLoadFloat(Real);
      emitter.EmitLoadFloat(Imaginary);
      emitter.EmitNewObj(emitter.FindMethod("complex", ".ctor", "float", "float"));
    }
  }
}
