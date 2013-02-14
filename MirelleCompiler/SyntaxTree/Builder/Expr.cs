using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirelle.Lexer;

namespace Mirelle.SyntaxTree
{
  /// <summary>
  /// Static class for constructing Mirelle Syntax Tree
  /// programmatically, similar to compiled expressions.
  /// For use withing Emitter.Generate*
  /// 
  /// No error checking is performed, and expressions are
  /// not lexem-bound, therefore errors in generated code
  /// cannot be detected properly with the compiler
  /// </summary>
  public class Expr
  {
    /// <summary>
    /// Create an ArrayGetNode
    /// </summary>
    /// <param name="array">Array</param>
    /// <param name="index">Index</param>
    /// <returns></returns>
    public static ArrayGetNode ArrayGet(SyntaxTreeNode array, SyntaxTreeNode index)
    {
      var node = new ArrayGetNode();
      node.ExpressionPrefix = array;
      node.Index = index;
      return node;
    }

    /// <summary>
    /// Create an ArrayNode
    /// </summary>
    /// <param name="values">Array items</param>
    /// <returns></returns>
    public static ArrayNode Array(params SyntaxTreeNode[] values)
    {
      return new ArrayNode(values);
    }

    /// <summary>
    /// Create an ArraySetNode
    /// </summary>
    /// <param name="array">Array</param>
    /// <param name="index">Index</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static ArraySetNode ArraySet(SyntaxTreeNode array, SyntaxTreeNode index, SyntaxTreeNode value)
    {
      var node = new ArraySetNode();
      node.ExpressionPrefix = array;
      node.Index = index;
      node.Expression = value;
      return node;
    }

    /// <summary>
    /// Create an ArraySetNode of a getter
    /// </summary>
    /// <param name="node">Array getter node</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static ArraySetNode ArraySet(ArrayGetNode getter, SyntaxTreeNode value)
    {
      var node = ArraySet(
        getter.ExpressionPrefix,
        getter.Index,
        value
      );
      node.Lexem = getter.Lexem;

      return node;
    }

    /// <summary>
    /// Create an AsNode
    /// </summary>
    /// <param name="expr">Expression to cast</param>
    /// <param name="type">Desired type</param>
    /// <returns></returns>
    public static AsNode As(SyntaxTreeNode expr, string type)
    {
      return new AsNode(expr, type);
    }

    /// <summary>
    /// Create a BoolNode
    /// </summary>
    /// <param name="value">Value</param>
    /// <returns></returns>
    public static BoolNode Bool(bool value)
    {
      return new BoolNode(value);
    }

    /// <summary>
    /// Create a BreakNode
    /// </summary>
    /// <returns></returns>
    public static BreakNode Break()
    {
      return new BreakNode();
    }

    /// <summary>
    /// Create a CodeBlockNode
    /// </summary>
    /// <param name="stmts">Statements</param>
    /// <returns></returns>
    public static CodeBlockNode CodeBlock(params SyntaxTreeNode[] stmts)
    {
      var node = new CodeBlockNode();
      node.Statements.AddRange(stmts);

      return node;
    }

    /// <summary>
    /// Create a ComplexNode
    /// </summary>
    /// <param name="real">Real part</param>
    /// <param name="img">Imaginary part</param>
    /// <returns></returns>
    public static ComplexNode Complex(double real = 0, double img = 0)
    {
      return new ComplexNode(real, img);
    }

    /// <summary>
    /// Create a DictNode
    /// </summary>
    /// <param name="parts">Dictionary key => value tuples</param>
    /// <returns></returns>
    public static DictNode Dict(params Tuple<SyntaxTreeNode, SyntaxTreeNode>[] parts)
    {
      var node = new DictNode();
      node.Data.AddRange(parts);

      return node;
    }

    /// <summary>
    /// Create a dummy node that has a type but does not contain a value
    /// Is used for method parameter substitution (in Simulate Planner)
    /// </summary>
    /// <param name="type">Desired type</param>
    /// <returns></returns>
    public static DummyNode Dummy(string type)
    {
      return new DummyNode(type);
    }

    /// <summary>
    /// Create an EmitNode
    /// </summary>
    /// <param name="action">Emitter action</param>
    /// <param name="step">Event timing step</param>
    /// <param name="distr">Event timing distribution</param>
    /// <param name="limit">Event count limit</param>
    /// <param name="cond">Finishing condition</param>
    /// <returns></returns>
    public static EmitNode Emit(SyntaxTreeNode action, SyntaxTreeNode step = null,
                                SyntaxTreeNode distr = null, SyntaxTreeNode limit = null,
                                SyntaxTreeNode cond = null)
    {
      var node = new EmitNode();
      node.Action = action;
      node.Step = step;
      node.Distribution = distr;
      node.Limit = limit;
      node.Condition = cond;

      return node;
    }

    /// <summary>
    /// Create an ExchangeNode
    /// </summary>
    /// <param name="left">Left-hand side variable</param>
    /// <param name="right">Right-hand side variable</param>
    /// <returns></returns>
    public static ExchangeNode Exchange(IdentifierNode left, IdentifierNode right)
    {
      var node = new ExchangeNode();
      node.Left = left;
      node.Right = right;

      return node;
    }

    /// <summary>
    /// Create an ExitNode
    /// </summary>
    /// <returns></returns>
    public static ExitNode Exit()
    {
      return new ExitNode();
    }

    /// <summary>
    /// Create a FloatNode
    /// </summary>
    /// <param name="value">Float value</param>
    /// <returns></returns>
    public static FloatNode Float(double value)
    {
      return new FloatNode(value);
    }

    /// <summary>
    /// Create a FloatRangeNode
    /// </summary>
    /// <param name="from">Starting value</param>
    /// <param name="to">Ending value</param>
    /// <param name="step">Step</param>
    /// <returns></returns>
    public static FloatRangeNode FloatRange(SyntaxTreeNode from, SyntaxTreeNode to, SyntaxTreeNode step)
    {
      var node = new FloatRangeNode();
      node.From = from;
      node.To = to;
      node.Step = step;

      return node;
    }

    /// <summary>
    /// Create a ForNode
    /// </summary>
    /// <param name="value">Variable name</param>
    /// <param name="iter">Iterable expression</param>
    /// <param name="body">Loop body</param>
    /// <returns></returns>
    public static ForNode For(string value, SyntaxTreeNode iter, SyntaxTreeNode body)
    {
      var node = new ForNode();
      node.Item = new Lexem(LexemType.Identifier, value);
      node.Iterable = iter;
      if (body is CodeBlockNode)
        node.Body = body;
      else
        node.Body = CodeBlock(body);

      return node;
    }

    /// <summary>
    /// Create a ForNode
    /// </summary>
    /// <param name="key">Key name</param>
    /// <param name="value">Variable name</param>
    /// <param name="iter">Iterable expression</param>
    /// <param name="body">Loop body</param>
    /// <returns></returns>
    public static ForNode For(string key, string value, SyntaxTreeNode iter, SyntaxTreeNode body)
    {
      var node = new ForNode();
      node.Key = new Lexem(LexemType.Identifier, key);
      node.Item = new Lexem(LexemType.Identifier, value);
      node.Iterable = iter;
      if (body is CodeBlockNode)
        node.Body = body;
      else
        node.Body = CodeBlock(body);

      return node;
    }

    /// <summary>
    /// Create an IdentifierGetNode of a variable or local field
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="atmark">Atmark prefix</param>
    /// <returns></returns>
    public static IdentifierGetNode IdentifierGet(string name, bool atmark = false)
    {
      return new IdentifierGetNode(name, atmark);
    }

    /// <summary>
    /// Create an IdentifierGetNode of a type
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="type">Type prefix</param>
    /// <returns></returns>
    public static IdentifierGetNode IdentifierGet(string name, string type)
    {
      var node = new IdentifierGetNode(name);
      node.TypePrefix = new Lexem(LexemType.Identifier, type);

      return node;
    }

    /// <summary>
    /// Create an IdentifierGetNode of an expression
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="expr">Expression prefix</param>
    /// <returns></returns>
    public static IdentifierGetNode IdentifierGet(string name, SyntaxTreeNode expr)
    {
      var node = new IdentifierGetNode(name);
      node.ExpressionPrefix = expr;

      return node;
    }

    /// <summary>
    /// Create an IdentifierInvokeNode of a local / visible method
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="parameters">Parameters</param>
    /// <returns></returns>
    public static IdentifierInvokeNode IdentifierInvoke(string name, params SyntaxTreeNode[] parameters)
    {
      var node = new IdentifierInvokeNode(name);
      node.Parameters.AddRange(parameters);

      return node;
    }

    /// <summary>
    /// Create an IdentifierInvokeNode of a type method
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="type">Type name</param>
    /// <param name="parameters">Parameters</param>
    /// <returns></returns>
    public static IdentifierInvokeNode IdentifierInvoke(string name, string type, params SyntaxTreeNode[] parameters)
    {
      var node = new IdentifierInvokeNode(name);
      node.TypePrefix = new Lexem(LexemType.Identifier, type);
      node.Parameters.AddRange(parameters);

      return node;
    }

    /// <summary>
    /// Create an IdentifierInvokeNode of a expression method
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="expr">Expression</param>
    /// <param name="parameters">Parameters</param>
    /// <returns></returns>
    public static IdentifierInvokeNode IdentifierInvoke(string name, SyntaxTreeNode expr, params SyntaxTreeNode[] parameters)
    {
      var node = new IdentifierInvokeNode(name);
      node.ExpressionPrefix = expr;
      node.Parameters.AddRange(parameters);

      return node;
    }

    /// <summary>
    /// Create an IdentifierInvokeNode from a getter node
    /// </summary>
    /// <param name="getter">IdentifierGetNode</param>
    /// <param name="parameters">Desired parameters</param>
    /// <returns></returns>
    public static IdentifierInvokeNode IdentifierInvoke(IdentifierGetNode getter, params SyntaxTreeNode[] parameters)
    {
      var node = new IdentifierInvokeNode(getter.Name);
      node.AtmarkPrefix = getter.AtmarkPrefix;
      node.ExpressionPrefix = getter.ExpressionPrefix;
      node.TypePrefix = getter.TypePrefix;
      node.Parameters.AddRange(parameters);

      return node;
    }

    /// <summary>
    /// Create an IdentifierSetNode of a variable or local field
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static IdentifierSetNode IdentifierSet(string name, SyntaxTreeNode value)
    {
      var node = new IdentifierSetNode(name);
      node.Expression = value;

      return node;
    }

    /// <summary>
    /// Create an IdentifierSetNode of a variable or local field
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="atmark">Atmark prefix</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static IdentifierSetNode IdentifierSet(string name, bool atmark, SyntaxTreeNode value)
    {
      var node = new IdentifierSetNode(name, atmark);
      node.Expression = value;

      return node;
    }

    /// <summary>
    /// Create an IdentifierSetNode of a type
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="type">Type prefix</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static IdentifierSetNode IdentifierSet(string name, string type, SyntaxTreeNode value)
    {
      var node = new IdentifierSetNode(name);
      node.TypePrefix = new Lexem(LexemType.Identifier, type);
      node.Expression = value;

      return node;
    }

    /// <summary>
    /// Create an IdentifierSetNode of an expression
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <param name="expr">Expression prefix</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static IdentifierSetNode IdentifierSet(string name, SyntaxTreeNode expr, SyntaxTreeNode value)
    {
      var node = new IdentifierSetNode(name);
      node.ExpressionPrefix = expr;
      node.Expression = value;

      return node;
    }

    /// <summary>
    /// Create an IdentifierSetNode of a getter
    /// </summary>
    /// <param name="getter">Identifier getter node</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static IdentifierSetNode IdentifierSet(IdentifierGetNode getter, SyntaxTreeNode value)
    {
      var node = new IdentifierSetNode(getter.Name);
      node.AtmarkPrefix = getter.AtmarkPrefix;
      node.ExpressionPrefix = getter.ExpressionPrefix;
      node.TypePrefix = getter.TypePrefix;
      node.Lexem = getter.Lexem;
      node.Expression = value;

      return node;
    }

    /// <summary>
    /// Create an IfNode
    /// </summary>
    /// <param name="cond">Condition</param>
    /// <param name="trueBlock">The block of code to execute if the condition is false</param>
    /// <param name="falseBlock">The block of code to execute otherwise</param>
    /// <returns></returns>
    public static IfNode If(SyntaxTreeNode cond, SyntaxTreeNode trueBlock, SyntaxTreeNode falseBlock = null)
    {
      if (!(trueBlock is CodeBlockNode))
        trueBlock = CodeBlock(trueBlock);

      if (falseBlock != null && !(falseBlock is CodeBlockNode))
        falseBlock = CodeBlock(falseBlock);

      return new IfNode(cond, trueBlock, falseBlock);
    }

    /// <summary>
    /// Create an InNode
    /// </summary>
    /// <param name="needle">Needle</param>
    /// <param name="haystack">Haystack</param>
    /// <returns></returns>
    public static InNode In(SyntaxTreeNode needle, SyntaxTreeNode haystack)
    {
      return BinaryOperator(new InNode(), needle, haystack);
    }

    /// <summary>
    /// Create an IntNode
    /// </summary>
    /// <param name="value">Value</param>
    /// <returns></returns>
    public static IntNode Int(int value)
    {
      return new IntNode(value);
    }

    /// <summary>
    /// Create a MatrixGetNode
    /// </summary>
    /// <param name="matrix">Matrix to index</param>
    /// <param name="idx1">Row index</param>
    /// <param name="idx2">Column index</param>
    /// <returns></returns>
    public static MatrixGetNode MatrixGet(SyntaxTreeNode matrix, SyntaxTreeNode idx1, SyntaxTreeNode idx2)
    {
      var node = new MatrixGetNode();
      node.ExpressionPrefix = matrix;
      node.Index1 = idx1;
      node.Index2 = idx2;

      return node;
    }

    /// <summary>
    /// Create a MatrixNode
    /// </summary>
    /// <param name="rows">Matrix rows</param>
    /// <returns></returns>
    public static MatrixNode Matrix(params List<SyntaxTreeNode>[] rows)
    {
      var node = new MatrixNode();
      node.MatrixItems.AddRange(rows);

      return node;
    }

    /// <summary>
    /// Create a row of matrix items
    /// </summary>
    /// <param name="items">Array of items in a matrix row</param>
    /// <returns></returns>
    public static List<SyntaxTreeNode> MatrixRow(params SyntaxTreeNode[] items)
    {
      return new List<SyntaxTreeNode>(items);
    }

    /// <summary>
    /// Create a MatrixSetNode
    /// </summary>
    /// <param name="matrix">Matrix to index</param>
    /// <param name="idx1">Row index</param>
    /// <param name="idx2">Column index</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static MatrixSetNode MatrixSet(SyntaxTreeNode matrix, SyntaxTreeNode idx1, SyntaxTreeNode idx2, SyntaxTreeNode value)
    {
      var node = new MatrixSetNode();
      node.ExpressionPrefix = matrix;
      node.Index1 = idx1;
      node.Index2 = idx2;
      node.Expression = value;

      return node;
    }

    /// <summary>
    /// Create a MatrixSetNode of a getter
    /// </summary>
    /// <param name="getter">Matrix getter node</param>
    /// <param name="value">Assigned value</param>
    /// <returns></returns>
    public static MatrixSetNode MatrixSet(MatrixGetNode getter, SyntaxTreeNode value)
    {
      var node = new MatrixSetNode();
      node.ExpressionPrefix = getter.ExpressionPrefix;
      node.Index1 = getter.Index1;
      node.Index2 = getter.Index2;
      node.Lexem = getter.Lexem;
      node.ExpressionPrefix = value;

      return node;
    }

    /// <summary>
    /// Create a NewNode
    /// </summary>
    /// <param name="type">Type name</param>
    /// <param name="parameters">Parameters</param>
    /// <returns></returns>
    public static NewNode New(string type, params SyntaxTreeNode[] parameters)
    {
      var node = new NewNode();
      node.Name = type;
      node.Parameters.AddRange(parameters);

      return node;
    }

    /// <summary>
    /// Create a NullNode
    /// </summary>
    /// <returns></returns>
    public static NullNode Null()
    {
      return new NullNode();
    }

    /// <summary>
    /// Underlying assigner for BinaryOperatorNode
    /// </summary>
    /// <typeparam name="T">Actual operator type</typeparam>
    /// <param name="node">Operator node</param>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    private static T BinaryOperator<T>(T node, SyntaxTreeNode left, SyntaxTreeNode right) where T : BinaryOperatorNode
    {
      node.Left = left;
      node.Right = right;

      return node;
    }

    /// <summary>
    /// Create an AddNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorAddNode Add(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorAddNode(), left, right);
    }

    /// <summary>
    /// Create an AndNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorAndNode And(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorAndNode(), left, right);
    }

    /// <summary>
    /// Create an BinaryAndNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorBinaryAndNode BinaryAnd(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorBinaryAndNode(), left, right);
    }

    /// <summary>
    /// Create an BinaryOrNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorBinaryOrNode BinaryOr(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorBinaryOrNode(), left, right);
    }

    /// <summary>
    /// Create an BinaryShiftLeftNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorBinaryShiftLeftNode BinaryShiftLeft(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorBinaryShiftLeftNode(), left, right);
    }

    /// <summary>
    /// Create an BinaryShiftRightNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorBinaryShiftRightNode BinaryShiftRight(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorBinaryShiftRightNode(), left, right);
    }

    /// <summary>
    /// Create an BinaryXorNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorBinaryXorNode BinaryXor(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorBinaryXorNode(), left, right);
    }

    /// <summary>
    /// Create an BinaryShiftRightNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns></returns>
    public static OperatorCompareNode Compare(SyntaxTreeNode left, LexemType compare, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorCompareNode(compare), left, right);
    }

    /// <summary>
    /// Create a DivideNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorDivideNode Divide(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorDivideNode(), left, right);
    }

    /// <summary>
    /// Create a InvertNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorInvertNode Invert(SyntaxTreeNode expr)
    {
      return new OperatorInvertNode(expr);
    }

    /// <summary>
    /// Create a MultiplyNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorMultiplyNode Multiply(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorMultiplyNode(), left, right);
    }

    /// <summary>
    /// Create a NegateNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorNegateNode Negate(SyntaxTreeNode expr)
    {
      return new OperatorNegateNode(expr);
    }

    /// <summary>
    /// Create a OrNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorOrNode Or(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorOrNode(), left, right);
    }

    /// <summary>
    /// Create a PowerNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorPowerNode Power(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorPowerNode(), left, right);
    }

    /// <summary>
    /// Create a RemainderNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorRemainderNode Remainder(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorRemainderNode(), left, right);
    }

    /// <summary>
    /// Create a SimulateAnyNode
    /// </summary>
    /// <param name="processors">Number of processors</param>
    /// <param name="queue">Maximum queue length</param>
    /// <returns></returns>
    public static SimulateAnyNode SimulateAny(SyntaxTreeNode processors = null, SyntaxTreeNode queue = null)
    {
      var node = new SimulateAnyNode();
      node.Processors = processors;
      node.MaxQueue = queue;

      return node;
    }

    /// <summary>
    /// Create a SimulatePlannerNode
    /// </summary>
    /// <param name="action">Action</param>
    /// <returns></returns>
    public static SimulatePlannerNode SimulatePlanner(SyntaxTreeNode action)
    {
      var node = new SimulatePlannerNode();
      node.Action = action;

      return node;
    }

    /// <summary>
    /// Create a SubtractNode
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    public static OperatorSubtractNode Subtract(SyntaxTreeNode left, SyntaxTreeNode right)
    {
      return BinaryOperator(new OperatorSubtractNode(), left, right);
    }

    /// <summary>
    /// Create a PrintNode
    /// </summary>
    /// <param name="values">Values</param>
    /// <returns></returns>
    public static PrintNode Print(params SyntaxTreeNode[] values)
    {
      var node = new PrintNode();
      node.Parameters.AddRange(values);

      return node;
    }

    /// <summary>
    /// Create a PrintNode with PrintLine
    /// </summary>
    /// <param name="values">Values</param>
    /// <returns></returns>
    public static PrintNode PrintLn(params SyntaxTreeNode[] values)
    {
      var node = new PrintNode();
      node.PrintLine = true;
      node.Parameters.AddRange(values);

      return node;
    }

    /// <summary>
    /// Create a RangeNode
    /// </summary>
    /// <param name="from">Range start</param>
    /// <param name="to">Range end</param>
    /// <returns></returns>
    public static RangeNode Range(SyntaxTreeNode from, SyntaxTreeNode to)
    {
      var node = new RangeNode();
      node.From = from;
      node.To = to;

      return node;
    }

    /// <summary>
    /// Create a RedoNode
    /// </summary>
    /// <returns></returns>
    public static RedoNode Redo()
    {
      return new RedoNode();
    }

    /// <summary>
    /// Create a ReturnNode
    /// </summary>
    /// <param name="expr"></param>
    public static ReturnNode Return(SyntaxTreeNode expr = null)
    {
      var node = new ReturnNode();
      node.Expression = expr;

      return node;
    }

    /// <summary>
    /// Create a ShortAssignNode
    /// </summary>
    /// <param name="lvalue">Assignable identifier</param>
    /// <param name="type">Assignment type</param>
    /// <param name="expr">Expression to assign</param>
    /// <returns></returns>
    public static ShortAssignNode ShortAssign(IdentifierNode lvalue, LexemType type, SyntaxTreeNode expr)
    {
      var node = new ShortAssignNode(lvalue, new Lexem(type));
      node.Expression = expr;

      return node;
    }

    /// <summary>
    /// Create a StringNode
    /// </summary>
    /// <param name="str">String value</param>
    /// <returns></returns>
    public static StringNode String(string str = "")
    {
      return new StringNode(str);
    }

    /// <summary>
    /// Create a ThisNode
    /// </summary>
    /// <returns></returns>
    public static ThisNode This()
    {
      return new ThisNode();
    }

    /// <summary>
    /// Create a VarDeclarationNode
    /// </summary>
    /// <param name="name">Variable node</param>
    /// <param name="expr">Expression to assign</param>
    /// <returns></returns>
    public static VarDeclarationNode Var(string name, SyntaxTreeNode expr)
    {
      var node = new VarDeclarationNode(name);
      node.Expression = expr;

      return node;
    }

    /// <summary>
    /// Create a VarSplatNode
    /// </summary>
    /// <param name="expr">Expression to splat</param>
    /// <param name="names">Variable names</param>
    /// <returns></returns>
    public static VarSplatNode VarSplat(SyntaxTreeNode expr, params string[] names)
    {
      var node = new VarSplatNode();
      node.Expression = expr;
      foreach (var curr in names)
        node.Names.Add(new Lexem(LexemType.Identifier, curr));

      return node;
    }

    /// <summary>
    /// Create a WhileNode
    /// </summary>
    /// <param name="cond">Condition</param>
    /// <param name="body">Loop body</param>
    /// <returns></returns>
    public static WhileNode While(SyntaxTreeNode cond, SyntaxTreeNode body)
    {
      if (!(body is CodeBlockNode))
        body = CodeBlock(body);

      var node = new WhileNode();
      node.Condition = cond;
      node.Body = body;

      return node;
    }
  }
}
