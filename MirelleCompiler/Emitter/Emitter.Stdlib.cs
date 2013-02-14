using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SN = System.Numerics;
using MN = MathNet.Numerics;
using MNM = MathNet.Numerics.LinearAlgebra.Double;

using MirelleStdlib;
using MirelleStdlib.Extenders;
using MirelleStdlib.Chart;
using MirelleStdlib.Events;
using MirelleStdlib.Histogram;
using MirelleStdlib.Wireless;

namespace Mirelle.Emitter
{
  public partial class Emitter
  {
    /// <summary>
    /// Import standard types into library
    /// </summary>
    public void RegisterStdLib()
    {
      // types
      ImportType(typeof(bool), "bool");
      ImportType(typeof(int), "int");
      ImportType(typeof(double), "float");
      ImportType(typeof(string), "string");
      ImportType(typeof(Range), "range");
      ImportType(typeof(Dict), "dict");
      ImportType(typeof(SN.Complex), "complex");
      ImportType(typeof(MN.LinearAlgebra.Double.DenseMatrix), "matrix");

      ImportType(typeof(IO), "io");
      ImportType(typeof(File), "file");
      ImportType(typeof(NetSocket), "socket");

      ImportType(typeof(ArrayExtender), "array");
      ImportType(typeof(MathExtender), "math");
      ImportType(typeof(Fourier), "fourier");
      ImportType(typeof(Timer), "timer");

      ImportType(typeof(Chart), "chart");
      ImportType(typeof(Series), "series");
      ImportType(typeof(Colors), "colors");
      ImportType(typeof(Histogram), "histogram");

      ImportType(typeof(MN.Distributions.IContinuousDistribution), "distr");
      ImportType(typeof(EventEmitter), ".emitter");
      ImportType(typeof(Simulation), "sim");
      ImportType(typeof(SimulationResult), "sim_result");

      ImportType(typeof(Flow), "flow");
      ImportType(typeof(Block), "block");
      ImportType(typeof(Symbol), "symbol");
      ImportEnum(typeof(Modulation), "modulation");
      ImportEnum(typeof(FlowType), "flow_type");
      ImportType(typeof(FlowSimulation), "flow_sim");
      ImportType(typeof(FlowSimulationResult), "flow_sim_result");
      ImportType(typeof(Planner), ".planner");

      // bool methods
      ImportMethod(typeof(BoolExtender), "ToInt", "bool", "to_i", "int", false, "bool");
      ImportMethod(typeof(BoolExtender), "ToFloat", "bool", "to_f", "float", false, "bool");
      ImportMethod(typeof(BoolExtender), "ToString", "bool", "to_s", "string", false, "bool");

      // int methods
      ImportMethod(typeof(IntExtender), "ToBool", "int", "to_b", "int", false, "int");
      ImportMethod(typeof(IntExtender), "ToFloat", "int", "to_f", "float", false, "int");
      ImportMethod(typeof(IntExtender), "ToString", "int", "to_s", "string", false, "int");
      ImportMethod(typeof(IntExtender), "Char", "int", "char", "string", false, "int");

      // float methods
      ImportMethod(typeof(FloatExtender), "ToBool", "float", "to_b", "bool", false, "float");
      ImportMethod(typeof(FloatExtender), "ToInt", "float", "to_f", "float", false, "float");
      ImportMethod(typeof(FloatExtender), "ToString", "float", "to_s", "string", false, "float");

      // string methods
      ImportMethod(typeof(StringExtender), "ToBool", "string", "to_b", "bool", false, "string");
      ImportMethod(typeof(StringExtender), "ToInt", "string", "to_i", "int", false, "string");
      ImportMethod(typeof(StringExtender), "ToFloat", "string", "to_f", "float", false, "string");
      ImportMethod(typeof(StringExtender), "IsInt", "string", "is_i", "bool", false, "string");
      ImportMethod(typeof(StringExtender), "IsFloat", "string", "is_f", "bool", false, "string");
      ImportMethod(typeof(string), "Concat", "string", "concat", "string", true, "string", "string");
      ImportMethod(typeof(string), "Contains", "string", "has", "bool", false, "string");
      ImportMethod(typeof(string), "StartsWith", "string", "starts_with", "bool", false, "string");
      ImportMethod(typeof(string), "EndsWith", "string", "ends_with", "bool", false, "string");
      ImportMethod(typeof(string), "Trim", "string", "trim", "string", false);
      ImportMethod(typeof(string), "Substring", "string", "substr", "string", false, "int");
      ImportMethod(typeof(string), "Substring", "string", "substr", "string", false, "int", "int");
      ImportMethod(typeof(string), "IndexOf", "string", "find", "int", false, "string");
      ImportMethod(typeof(string), "IndexOf", "string", "find", "int", false, "string", "int");
      ImportMethod(typeof(string), "LastIndexOf", "string", "find_last", "int", false, "string");
      ImportMethod(typeof(string), "LastIndexOf", "string", "find_last", "int", false, "string", "int");
      ImportMethod(typeof(string), "Replace", "string", "replace", "string", false, "string", "string");
      ImportMethod(typeof(string), "Insert", "string", "insert", "string", false, "int", "string");
      ImportMethod(typeof(StringExtender), "Reverse", "string", "reverse", "string", false, "string");
      ImportMethod(typeof(StringExtender), "Size", "string", "size", "int", false, "string");
      ImportMethod(typeof(StringExtender), "Split", "string", "split", "string[]", false, "string", "string");
      ImportMethod(typeof(StringExtender), "Split", "string", "split", "string[]", false, "string", "string", "int");
      ImportMethod(typeof(StringExtender), "Join", "string", "join", "string", false, "string", "string[]");
      ImportMethod(typeof(StringExtender), "Repeat", "string", "repeat", "string", false, "string", "int");
      ImportMethod(typeof(StringExtender), "Ord", "string", "ord", "int", false, "string");
      ImportMethod(typeof(StringExtender), "CharAt", "string", "at", "string", false, "string", "int");
      ImportMethod(typeof(StringExtender), "NewLine", "string", "NL", "string", true);
      ImportMethod(typeof(StringExtender), "EndLine", "string", "ENDL", "string", true);
      ImportMethod(typeof(StringExtender), "Tab", "string", "TAB", "string", true);
      ImportMethod(typeof(StringExtender), "Quote", "string", "QUOTE", "string", true);

      // range
      ImportCtor(typeof(Range), "range", "int", "int");
      ImportMethod(typeof(Range), "Next", "range", "next", "bool");
      ImportMethod(typeof(Range), "Reset", "range", "reset", "void");
      ImportMethod(typeof(Range), "Current", "range", "current", "int");
      ImportMethod(typeof(Range), "Has", "range", "has", "bool", false, "float");
      ImportMethod(typeof(Range), "Size", "range", "size", "int");
      ImportMethod(typeof(Range), "From", "range", "from", "int");
      ImportMethod(typeof(Range), "To", "range", "to", "int");
      ImportMethod(typeof(Range), "ToArray", "range", "to_a", "int[]");
      ImportMethod(typeof(Range), "ToString", "range", "to_s", "string");

      // dict
      ImportMethod(typeof(Dict), "Size", "dict", "size", "int");
      ImportMethod(typeof(Dict), "Set", "dict", "set", "void", false, "string", "string");
      ImportMethod(typeof(Dict), "Get", "dict", "get", "string", false, "string");
      ImportMethod(typeof(Dict), "Unset", "dict", "unset", "void", false, "string");
      ImportMethod(typeof(Dict), "Has", "dict", "has", "bool", false, "string");
      ImportMethod(typeof(Dict), "HasValue", "dict", "has_value", "bool", false, "string");
      ImportMethod(typeof(Dict), "Keys", "dict", "keys", "string[]");
      ImportMethod(typeof(Dict), "Values", "dict", "values", "string[]");
      ImportMethod(typeof(Dict), "Next", "dict", "next", "bool");
      ImportMethod(typeof(Dict), "Reset", "dict", "reset", "void");
      ImportMethod(typeof(Dict), "Current", "dict", "current", "string[]");

      // complex
      ImportCtor(typeof(SN.Complex), "complex", "float", "float");
      ImportMethod(typeof(ComplexExtender), "ToBool", "complex", "to_b", "bool", false, "complex");
      ImportMethod(typeof(ComplexExtender), "ToString", "complex", "to_s", "string", false, "complex");
      ImportMethod(typeof(ComplexExtender), "Real", "complex", "real", "float", false, "complex");
      ImportMethod(typeof(ComplexExtender), "Imaginary", "complex", "img", "float", false, "complex");
      ImportMethod(typeof(ComplexExtender), "Merge", "complex", "merge", "complex[]", true, "float[]", "float[]");

      // matrix
      ImportCtor(typeof(MNM.DenseMatrix), "matrix", "int");
      ImportCtor(typeof(MNM.DenseMatrix), "matrix", "int", "int");
      ImportCtor(typeof(MNM.DenseMatrix), "matrix", "int", "int", "float");
      ImportMethod(typeof(MatrixExtender), "Identity", "matrix", "identity", "matrix", true, "int");
      ImportMethod(typeof(MatrixExtender), "Invert", "matrix", "invert", "matrix", false, "matrix");
      ImportMethod(typeof(MNM.DenseMatrix), "Transpose", "matrix", "trans", "matrix");
      ImportMethod(typeof(MNM.DenseMatrix), "Determinant", "matrix", "det", "float");
      ImportMethod(typeof(MNM.DenseMatrix), "Rank", "matrix", "rank", "int");
      ImportMethod(typeof(MNM.DenseMatrix), "SetColumn", "matrix", "set_col", "void", false, "int", "float[]");
      ImportMethod(typeof(MNM.DenseMatrix), "SetRow", "matrix", "set_row", "void", false, "int", "float[]");
      ImportMethod(typeof(MNM.DenseMatrix), "SubMatrix", "matrix", "piece", "matrix", false, "int", "int", "int", "int");
      ImportMethod(typeof(MNM.DenseMatrix), "ConjugateTranspose", "matrix", "conj", "matrix");
      ImportMethod(typeof(MNM.DenseMatrix), "FrobeniusNorm", "matrix", "frob", "float");
      ImportMethod(typeof(MNM.DenseMatrix), "L1Norm", "matrix", "l1", "float");
      ImportMethod(typeof(MNM.DenseMatrix), "L2Norm", "matrix", "l2", "float");
      ImportMethod(typeof(MatrixExtender), "Row", "matrix", "row", "float[]", false, "matrix", "int");
      ImportMethod(typeof(MatrixExtender), "Column", "matrix", "col", "float[]", false, "matrix", "int");
      ImportMethod(typeof(MatrixExtender), "Flatten", "matrix", "flatten", "float[]", false, "matrix");
      ImportMethod(typeof(MatrixExtender), "Inflate", "matrix", "inflate", "matrix", true, "float[]", "int");
      ImportMethod(typeof(MatrixExtender), "FactorLU", "matrix", "lu", "matrix[]", false, "matrix");
      ImportMethod(typeof(MatrixExtender), "FactorQR", "matrix", "qr", "matrix[]", false, "matrix");
      ImportMethod(typeof(MatrixExtender), "FactorCholessky", "matrix", "chol", "matrix", false, "matrix");
      ImportMethod(typeof(MatrixExtender), "Pascal", "matrix", "pascal", "matrix", true, "int");

      // array
      ImportMethod(typeof(ArrayExtender), "Sort", "array", "sort", "void", true, "int[]");
      ImportMethod(typeof(ArrayExtender), "Sort", "array", "sort", "void", true, "int[]", "bool");
      ImportMethod(typeof(ArrayExtender), "Sort", "array", "sort", "void", true, "float[]");
      ImportMethod(typeof(ArrayExtender), "Sort", "array", "sort", "void", true, "float[]", "bool");
      ImportMethod(typeof(ArrayExtender), "Sort", "array", "sort", "void", true, "string[]");
      ImportMethod(typeof(ArrayExtender), "Sort", "array", "sort", "void", true, "string[]", "bool");
      ImportMethod(typeof(ArrayExtender), "Min", "array", "min", "int", true, "int[]");
      ImportMethod(typeof(ArrayExtender), "Min", "array", "min", "float", true, "float[]");
      ImportMethod(typeof(ArrayExtender), "Min", "array", "min", "string", true, "string[]");
      ImportMethod(typeof(ArrayExtender), "Max", "array", "max", "int", true, "int[]");
      ImportMethod(typeof(ArrayExtender), "Max", "array", "max", "float", true, "float[]");
      ImportMethod(typeof(ArrayExtender), "Max", "array", "max", "string", true, "string[]");

      // file
      ImportCtor(typeof(File), "file", "string");
      ImportMethod(typeof(File), "Delete", "file", "delete", "void");
      ImportMethod(typeof(File), "Exists", "file", "exists", "bool");
      ImportMethod(typeof(File), "Name", "file", "name", "string");
      ImportMethod(typeof(File), "FullName", "file", "full_name", "string");
      ImportMethod(typeof(File), "Extension", "file", "ext", "string");
      ImportMethod(typeof(File), "DirName", "file", "dir", "string");
      ImportMethod(typeof(File), "Size", "file", "size", "int");
      ImportMethod(typeof(File), "Open", "file", "open", "void");
      ImportMethod(typeof(File), "Close", "file", "close", "void");
      ImportMethod(typeof(File), "Create", "file", "create", "void");
      ImportMethod(typeof(File), "Read", "file", "read", "string", false, "int");
      ImportMethod(typeof(File), "ReadAll", "file", "read_all", "string");
      ImportMethod(typeof(File), "Write", "file", "write", "void", false, "string");
      ImportMethod(typeof(File), "LoadBytes", "file", "load_bytes", "int[]");
      ImportMethod(typeof(File), "SaveBytes", "file", "save_bytes", "void", false, "int[]");
      ImportMethod(typeof(File), "LoadBools", "file", "load_bools", "bool[]");
      ImportMethod(typeof(File), "SaveBools", "file", "save", "void", false, "bool[]");
      ImportMethod(typeof(File), "LoadInts", "file", "load_ints", "int[]");
      ImportMethod(typeof(File), "SaveInts", "file", "save", "void", false, "int[]");
      ImportMethod(typeof(File), "LoadFloats", "file", "load_floats", "float[]");
      ImportMethod(typeof(File), "SaveFloats", "file", "save", "void", false, "float[]");
      ImportMethod(typeof(File), "LoadStrings", "file", "load_strings", "string[]");
      ImportMethod(typeof(File), "SaveStrings", "file", "save", "void", false, "string[]");
      ImportMethod(typeof(File), "LoadMatrix", "file", "load_matrix", "matrix");
      ImportMethod(typeof(File), "SaveMatrix", "file", "save", "void", false, "matrix");
      ImportMethod(typeof(File), "LoadDict", "file", "load_dict", "dict");
      ImportMethod(typeof(File), "SaveDict", "file", "save", "void", false, "dict");

      // math
      ImportMethod(typeof(Math), "Sin", "math", "sin", "float", true, "float");
      ImportMethod(typeof(Math), "Sinh", "math", "sinh", "float", true, "float");
      ImportMethod(typeof(Math), "Asin", "math", "asin", "float", true, "float");
      ImportMethod(typeof(SN.Complex), "Sin", "math", "sin", "complex", true, "complex");
      ImportMethod(typeof(SN.Complex), "Sinh", "math", "sinh", "complex", true, "complex");
      ImportMethod(typeof(SN.Complex), "Asin", "math", "asin", "complex", true, "complex");
      ImportMethod(typeof(Math), "Cos", "math", "cos", "float", true, "float");
      ImportMethod(typeof(Math), "Cosh", "math", "cosh", "float", true, "float");
      ImportMethod(typeof(Math), "Acos", "math", "acos", "float", true, "float");
      ImportMethod(typeof(SN.Complex), "Cos", "math", "cos", "complex", true, "complex");
      ImportMethod(typeof(SN.Complex), "Cosh", "math", "cosh", "complex", true, "complex");
      ImportMethod(typeof(SN.Complex), "Acos", "math", "acos", "complex", true, "complex");
      ImportMethod(typeof(Math), "Tan", "math", "tan", "float", true, "float");
      ImportMethod(typeof(Math), "Tanh", "math", "tanh", "float", true, "float");
      ImportMethod(typeof(Math), "Atan", "math", "atan", "float", true, "float");
      ImportMethod(typeof(SN.Complex), "Tan", "math", "tan", "complex", true, "complex");
      ImportMethod(typeof(SN.Complex), "Tanh", "math", "tanh", "complex", true, "complex");
      ImportMethod(typeof(SN.Complex), "Atan", "math", "atan", "complex", true, "complex");
      ImportMethod(typeof(Math), "Sqrt", "math", "sqrt", "float", true, "float");
      ImportMethod(typeof(SN.Complex), "Sqrt", "math", "sqrt", "complex", true, "complex");
      ImportMethod(typeof(Math), "Log", "math", "log", "float", true, "float");
      ImportMethod(typeof(SN.Complex), "Log", "math", "log", "complex", true, "complex");
      ImportMethod(typeof(Math), "Log10", "math", "log10", "float", true, "float");
      ImportMethod(typeof(SN.Complex), "Log10", "math", "log10", "complex", true, "complex");

      ImportMethod(typeof(MathExtender), "Sin", "math", "sin", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Sinh", "math", "sinh", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Asin", "math", "asin", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Sin", "math", "sin", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Sinh", "math", "sinh", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Asin", "math", "asin", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Cos", "math", "cos", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Cosh", "math", "cosh", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Acos", "math", "acos", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Cos", "math", "cos", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Cosh", "math", "cosh", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Acos", "math", "acos", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Tan", "math", "tan", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Tanh", "math", "tanh", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Atan", "math", "atan", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Tan", "math", "tan", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Tanh", "math", "tanh", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Atan", "math", "atan", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Sqrt", "math", "sqrt", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Sqrt", "math", "sqrt", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Log", "math", "log", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Log", "math", "log", "complex[]", true, "complex[]");
      ImportMethod(typeof(MathExtender), "Log10", "math", "log10", "float[]", true, "float[]");
      ImportMethod(typeof(MathExtender), "Log10", "math", "log10", "complex[]", true, "complex[]");

      ImportMethod(typeof(Math), "Min", "math", "min", "float", true, "float", "float");
      ImportMethod(typeof(Math), "Max", "math", "max", "float", true, "float", "float");
      ImportMethod(typeof(MathExtender), "Min", "math", "min", "int", true, "int", "int");
      ImportMethod(typeof(MathExtender), "Max", "math", "max", "int", true, "int", "int");
      ImportMethod(typeof(MathExtender), "Limit", "math", "limit", "int", true, "int", "int", "int");
      ImportMethod(typeof(MathExtender), "Limit", "math", "limit", "float", true, "float", "float", "float");

      ImportMethod(typeof(MathExtender), "Ceil", "math", "ceil", "int", true, "float");
      ImportMethod(typeof(MathExtender), "Floor", "math", "floor", "int", true, "float");
      ImportMethod(typeof(MathExtender), "Round", "math", "round", "int", true, "float");
      ImportMethod(typeof(MathExtender), "Abs", "math", "abs", "int", true, "int");
      ImportMethod(typeof(Math), "Abs", "math", "abs", "float", true, "float");
      ImportMethod(typeof(Math), "Exp", "math", "exp", "float", true, "float");
      ImportMethod(typeof(Math), "Pow", "math", "pow", "float", true, "float", "float");

      ImportMethod(typeof(MathExtender), "E", "math", "E", "float", true);
      ImportMethod(typeof(MathExtender), "Pi", "math", "PI", "float", true);
      ImportMethod(typeof(MathExtender), "GoldRatio", "math", "gold", "float", true);

      ImportMethod(typeof(MathExtender), "Random", "math", "rand", "float", true);
      ImportMethod(typeof(MathExtender), "Random", "math", "rand", "float", true, "float", "float");
      ImportMethod(typeof(MathExtender), "Random", "math", "rand", "int", true, "range");

      ImportMethod(typeof(MN.SpecialFunctions), "Erf", "math", "erf", "float", true, "float");
      ImportMethod(typeof(MN.SpecialFunctions), "Erfc", "math", "erfc", "float", true, "float");
      ImportMethod(typeof(MN.SpecialFunctions), "ErfInv", "math", "ierf", "float", true, "float");
      ImportMethod(typeof(MN.SpecialFunctions), "ErfcInv", "math", "ierfc", "float", true, "float");
      ImportMethod(typeof(MN.SpecialFunctions), "Beta", "math", "beta", "float", true, "float", "float");
      ImportMethod(typeof(MN.SpecialFunctions), "Gamma", "math", "gamma", "float", true, "float");
      ImportMethod(typeof(MN.SpecialFunctions), "Factorial", "math", "fact", "float", true, "int");

      // IO
      ImportMethod(typeof(IO), "Read", "io", "read", "string", true);
      ImportMethod(typeof(IO), "Read", "io", "read", "string", true, "string");
      ImportMethod(typeof(IO), "Wait", "io", "wait", "void", true);
      ImportMethod(typeof(System.Threading.Thread), "Sleep", "io", "wait", "void", true, "int");
      ImportField(typeof(IO), "Args", "io", "args", "string[]", true);
      ImportField(typeof(MirelleStdlib.Printer), "LineSeparator", "io", "line_separator", "string", true);
      ImportField(typeof(MirelleStdlib.Printer), "ArgumentSeparator", "io", "arg_separator", "string", true);

      // socket
      ImportCtor(typeof(NetSocket), "socket");
      ImportMethod(typeof(NetSocket), "Connect", "socket", "connect", "void", false, "string", "int");
      ImportMethod(typeof(NetSocket), "Bind", "socket", "bind", "void", false, "int");
      ImportMethod(typeof(NetSocket), "Listen", "socket", "listen", "void");
      ImportMethod(typeof(NetSocket), "CanRead", "socket", "can_read", "bool");
      ImportMethod(typeof(NetSocket), "CanWrite", "socket", "can_write", "bool");
      ImportMethod(typeof(NetSocket), "ReadBytes", "socket", "read_bytes", "int[]");
      ImportMethod(typeof(NetSocket), "Read", "socket", "read", "string");
      ImportMethod(typeof(NetSocket), "WriteBytes", "socket", "write_bytes", "void", false, "int[]");
      ImportMethod(typeof(NetSocket), "Write", "socket", "write", "void", false, "string");
      ImportMethod(typeof(NetSocket), "Close", "socket", "close", "void");
      ImportMethod(typeof(NetSocket), "Refresh", "socket", "refresh", "void");

      // fourier
      ImportMethod(typeof(Fourier), "FFT", "fourier", "fft", "complex[]", true, "complex[]");
      ImportMethod(typeof(Fourier), "IFFT", "fourier", "ifft", "complex[]", true, "complex[]");
      
      // timer
      ImportMethod(typeof(Timer), "Tic", "timer", "tic", "int", true);
      ImportMethod(typeof(Timer), "Toc", "timer", "toc", "float", true);
      ImportMethod(typeof(Timer), "Toc", "timer", "toc", "float", true, "int");

      // chart
      ImportCtor(typeof(Chart), "chart");
      ImportCtor(typeof(Chart), "chart", "string");
      ImportMethod(typeof(Chart), "NewSeries", "chart", "new_series", "series", false);
      ImportMethod(typeof(Chart), "NewSeries", "chart", "new_series", "series", false, "string");
      ImportMethod(typeof(Chart), "NewSeries", "chart", "new_series", "series", false, "string", "float");
      ImportMethod(typeof(Chart), "NewSeries", "chart", "new_series", "series", false, "string", "float", "int");

      // series
      ImportMethod(typeof(Series), "Plot", "series", "plot", "void", false, "float", "float");
      ImportMethod(typeof(Series), "Plot", "series", "plot", "void", false, "float[]", "float[]");
      ImportMethod(typeof(Series), "Plot", "series", "plot", "void", false, "complex");
      ImportMethod(typeof(Series), "Plot", "series", "plot", "void", false, "complex[]");

      // colors
      ImportMethod(typeof(Colors), "Red", "colors", "red", "int", true);
      ImportMethod(typeof(Colors), "Orange", "colors", "orange", "int", true);
      ImportMethod(typeof(Colors), "Yellow", "colors", "yellow", "int", true);
      ImportMethod(typeof(Colors), "Green", "colors", "green", "int", true);
      ImportMethod(typeof(Colors), "Blue", "colors", "blue", "int", true);
      ImportMethod(typeof(Colors), "Violet", "colors", "violet", "int", true);
      ImportMethod(typeof(Colors), "Random", "colors", "rand", "int", true);
      ImportMethod(typeof(Colors), "RGB", "colors", "rgb", "int", true, "int", "int", "int");
      ImportMethod(typeof(Colors), "Hex", "colors", "hex", "int", true, "string");

      // histogram
      ImportCtor(typeof(Histogram), "histogram");
      ImportCtor(typeof(Histogram), "histogram", "string");
      ImportField(typeof(Histogram), "Visible", "histogram", "visible", "bool");
      ImportMethod(typeof(Histogram), "Plot", "histogram", "plot", "void", false, "complex[]");
      ImportMethod(typeof(Histogram), "Plot", "histogram", "plot", "void", false, "float[]");
      ImportMethod(typeof(Histogram), "Plot", "histogram", "plot", "void", false, "float[]", "float[]");
      ImportMethod(typeof(Histogram), "Plot", "histogram", "plot", "void", false, "string[]", "float[]");

      // distr
      ImportMethod(typeof(DistributionExtender), "Mean", "distr", "mean", "float", false, "distr");
      ImportMethod(typeof(DistributionExtender), "Entropy", "distr", "entropy", "float", false, "distr");
      ImportMethod(typeof(DistributionExtender), "Deviation", "distr", "deviation", "float", false, "distr");
      ImportMethod(typeof(DistributionExtender), "Variance", "distr", "variance", "float", false, "distr");
      ImportMethod(typeof(MN.Distributions.IContinuousDistribution), "Sample", "distr", "sample", "float");
      ImportMethod(typeof(DistributionExtender), "Samples", "distr", "samples", "float[]", false, "distr", "int");
      ImportMethod(typeof(DistributionExtender), "Normal", "distr", "normal", "distr", true);
      ImportMethod(typeof(MN.Distributions.Normal), "WithMeanStdDev", "distr", "normal", "distr", true, "float", "float");
      ImportMethod(typeof(MN.Distributions.Normal), "WithMeanVariance", "distr", "normal_variance", "distr", true, "float", "float");
      ImportMethod(typeof(DistributionExtender), "Exponential", "distr", "exp", "distr", true, "float");
      ImportMethod(typeof(DistributionExtender), "Rayleigh", "distr", "rayleigh", "distr", true);
      ImportMethod(typeof(DistributionExtender), "Rayleigh", "distr", "rayleigh", "distr", true, "float");
      ImportMethod(typeof(DistributionExtender), "Erlang", "distr", "erlang", "distr", true, "int", "float");
      ImportMethod(typeof(DistributionExtender), "Uniform", "distr", "uniform", "distr", true);
      ImportMethod(typeof(DistributionExtender), "Uniform", "distr", "uniform", "distr", true, "float", "float");
      ImportMethod(typeof(DistributionExtender), "Uniform", "distr", "uniform", "distr", true, "range");

      // .emitter
      ImportCtor(typeof(EventEmitter), ".emitter");
      ImportField(typeof(EventEmitter), "Step", ".emitter", "step", "float");
      ImportField(typeof(EventEmitter), "Limit", ".emitter", "limit", "float");
      ImportField(typeof(EventEmitter), "Distribution", ".emitter", "distr", "distr");
      ImportMethod(typeof(EventEmitter), "Emit", ".emitter", "emit", "void");

      // sim
      ImportMethod(typeof(Simulation), "AddEventTime", "sim", "takes", "void", true, "float");
      ImportMethod(typeof(Simulation), "GetTime", "sim", "time", "float", true);
      ImportMethod(typeof(Simulation), "Stop", "sim", "stop", "float", true);

      // sim_result
      ImportField(typeof(SimulationResult), "EmitterCount", "sim_result", "emitters", "int");
      ImportField(typeof(SimulationResult), "ProcessorCount", "sim_result", "processors", "int");
      ImportField(typeof(SimulationResult), "EventCount", "sim_result", "events", "int");
      ImportField(typeof(SimulationResult), "DiscardedEventCount", "sim_result", "discarded", "int");
      ImportField(typeof(SimulationResult), "Time", "sim_result", "time", "float");
      ImportField(typeof(SimulationResult), "MaxQueue", "sim_result", "queue_max", "int");
      ImportField(typeof(SimulationResult), "AvgWait", "sim_result", "wait_avg", "float");
      ImportField(typeof(SimulationResult), "MaxWait", "sim_result", "wait_max", "float");
      ImportField(typeof(SimulationResult), "ProcessorGraphPoints", "sim_result", "processors_graph", "complex[]");
      ImportField(typeof(SimulationResult), "QueueGraphPoints", "sim_result", "queue_graph", "complex[][]");
      ImportMethod(typeof(SimulationResult), "Show", "sim_result", "show", "void");

      // flow
      ImportCtor(typeof(Flow), "flow", "flow_type", "int", "int");
      ImportCtor(typeof(Flow), "flow", "flow_type", "int", "int", "int");
      ImportField(typeof(Flow), "FlowType", "flow", "flow_type", "flow_type");
      ImportField(typeof(Flow), "QoS", "flow", "qos", "int");
      ImportField(typeof(Flow), "Speed", "flow", "speed", "int");
      ImportField(typeof(Flow), "PacketSize", "flow", "packet_size", "int");
      ImportMethod(typeof(Flow), "QueueSize", "flow", "queue_size", "int");
      ImportMethod(typeof(Flow), "AvgWait", "flow", "wait_avg", "float");
      ImportMethod(typeof(Flow), "WaitGraph", "flow", "wait", "complex[]");

      // flow_type
      ImportEnumValue(typeof(FlowType), "Http", "flow_type", "http");
      ImportEnumValue(typeof(FlowType), "Ftp", "flow_type", "ftp");
      ImportEnumValue(typeof(FlowType), "Video", "flow_type", "video");
      ImportEnumValue(typeof(FlowType), "Voip", "flow_type", "voip");
      ImportEnumValue(typeof(FlowType), "Unknown", "flow_type", "unknown");
      ImportMethod(typeof(FlowType), "ToString", "flow_type", "to_s", "string", false);
      ImportMethod(typeof(FlowType), "ToArray", "flow_type", "to_a", "flow_type[]", false);

      // modulation
      ImportEnumValue(typeof(Modulation), "Bpsk", "modulation", "bpsk");
      ImportEnumValue(typeof(Modulation), "Qpsk", "modulation", "qpsk");
      ImportEnumValue(typeof(Modulation), "Qam16", "modulation", "qam16");
      ImportEnumValue(typeof(Modulation), "Qam64", "modulation", "qam64");
      ImportEnumValue(typeof(Modulation), "Qam256", "modulation", "qam256");
      ImportMethod(typeof(Modulation), "ToString", "modulation", "to_s", "string", false);
      ImportMethod(typeof(Modulation), "ToArray", "modulation", "to_a", "modulation[]", false);

      // block
      ImportField(typeof(Block), "Used", "block", "used", "bool");
      ImportField(typeof(Block), "Transmitted", "block", "ok", "bool");
      ImportField(typeof(Block), "Modulation", "block", "modulation", "modulation");
      ImportField(typeof(Block), "Flow", "block", "flow", "flow");
      ImportField(typeof(Block), "Tag", "block", "tag", "string");
      ImportMethod(typeof(Block), "Use", "block", "fill", "void", false, "flow");
      ImportMethod(typeof(Block), "Use", "block", "fill", "void", false, "flow", "modulation");
      ImportMethod(typeof(Block), "Quality", "block", "quality", "float");
      ImportMethod(typeof(Block), "Size", "block", "size", "int");
      ImportMethod(typeof(Block), "DataSize", "block", "data_size", "int");

      // symbol
      ImportCtor(typeof(Symbol), "symbol");
      ImportField(typeof(Symbol), "Blocks", "symbol", "blocks", "block[]");

      // .planner
      ImportCtor(typeof(Planner), ".planner");
      ImportMethod(typeof(Planner), "Action", ".planner", "Action", "symbol", false, "flow[]", "symbol");

      // flow_sim
      ImportField(typeof(FlowSimulation), "Speed", "flow_sim", "speed", "int", true);
      ImportField(typeof(FlowSimulation), "BlocksPerSymbol", "flow_sim", "block_per_symbol", "int", true);
      ImportField(typeof(FlowSimulation), "BlockSize", "flow_sim", "block_size", "int", true);
      ImportField(typeof(FlowSimulation), "MaxTime", "flow_sim", "time_max", "float", true);
      ImportField(typeof(FlowSimulation), "Scale", "flow_sim", "scale", "int", true);
      ImportField(typeof(FlowSimulation), "MaxQueueSize", "flow_sim", "queue_max", "int", true);
      ImportField(typeof(FlowSimulation), "Flows", "flow_sim", "flows", "flow[]", true);
      ImportField(typeof(FlowSimulation), "ChannelQuality", "flow_sim", "channel", "float[]", true);
      ImportMethod(typeof(FlowSimulation), "AddFlow", "flow_sim", "add", "void", true, "flow");
      ImportMethod(typeof(FlowSimulation), "SetChannel", "flow_sim", "set_channel", "void", true, "dict");
      ImportMethod(typeof(FlowSimulation), "PickWithPriority", "flow_sim", "pick_flow", "flow", true, "flow[]");

      // flow_sim_result
      ImportField(typeof(FlowSimulationResult), "Total", "flow_sim_result", "total", "int");
      ImportField(typeof(FlowSimulationResult), "FailedBlocks", "flow_sim_result", "failed", "int");
      ImportField(typeof(FlowSimulationResult), "Discarded", "flow_sim_result", "discarded", "int");
      ImportField(typeof(FlowSimulationResult), "AvgWait", "flow_sim_result", "wait_avg", "float");
      ImportField(typeof(FlowSimulationResult), "MaxWait", "flow_sim_result", "wait_max", "float");
      ImportField(typeof(FlowSimulationResult), "AvgSpeed", "flow_sim_result", "speed_avg", "float");
      ImportField(typeof(FlowSimulationResult), "Flows", "flow_sim_result", "flows", "flow[]");
    }
  }
}
