using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Mirelle.Lexer;
using Mirelle.SyntaxTree;

namespace Mirelle.Parser
{
  public partial class Parser
  {
    /// <summary>
    /// Reference to compiler
    /// </summary>
    public Compiler Compiler;

    /// <summary>
    /// Current depth of loop to check whether loop control statements are available
    /// </summary>
    public int InLoop = 0;

    public Parser(Compiler compiler)
    {
      Compiler = compiler;
    }


    /// <summary>
    /// main = { global_stmt } EOF;
    /// </summary>
    public void ParseMain()
    {
      Compiler.Emitter.CurrentType = null;
      Compiler.Emitter.CurrentMethod = Compiler.Emitter.RootNode.GlobalMethod;

      while (!PeekLexem(LexemType.EOF))
        ParseGlobalStmt();
    }


    /// <summary>
    /// global_stmt = type | include | emit | local_stmt ;
    /// </summary>
    private void ParseGlobalStmt()
    {
      // ? type
      var lexem = GetLexem();
      switch (lexem.Type)
      {
        case LexemType.Type: ParseType(); break;
        case LexemType.Enum: ParseEnum(); break;
        case LexemType.Include: ParseInclude(); break;
        case LexemType.Use: ParseUse(); break;
        default: Compiler.Emitter.RootNode.GlobalMethod.Body.Statements.Add(ParseLocalStmt()); break;
      }
    }


    /// <summary>
    /// type = "type", type_name, [ ":", identifier ], "{", { type_stmt, }, "}" ;
    /// </summary>
    /// 
    private void ParseType()
    {
      TypeNode type;

      // "type"
      SkipLexem();

      // <name>
      var typeName = GetLexem();
      if(typeName.Type != LexemType.Identifier) Error(Resources.errTypeNameExpected);
      if (typeName.Data[0] == '@') Error(Resources.errTypeNameExpected);
      SkipLexem();

      // ? ":" <parent>
      if(PeekLexem(LexemType.Colon))
      {
        SkipLexem();

        if (!PeekLexem(LexemType.Identifier)) Error(Resources.errTypeNameExpected);
        var parentName = GetLexem();

        var parentType = Compiler.Emitter.FindType(parentName.Data);
        if (parentType.BuiltIn)
          Error(String.Format(Resources.errInheritBuiltInType, parentName.Data));

        if (parentType.Enum)
          Error(String.Format(Resources.errInheritEnum, parentName.Data));

        SkipLexem();

        type = Compiler.Emitter.CreateType(typeName.Data, parentName.Data);
      }
      else
        type = Compiler.Emitter.CreateType(typeName.Data);

      type.Lexem = typeName;

      Compiler.Emitter.CurrentType = type;
      Compiler.Emitter.CurrentMethod = null;

      // "{"
      if (!PeekLexem(LexemType.CurlyOpen)) Error(Resources.errTypeCurlyBrace);
      SkipLexem();

      while (!PeekLexem(LexemType.CurlyClose, LexemType.EOF))
        ParseTypeStmt();

      // "}"
      if (!PeekLexem(LexemType.CurlyClose)) Error(Resources.errTypeCurlyBrace);
      SkipLexem();

      Compiler.Emitter.CurrentType = null;
      Compiler.Emitter.CurrentMethod = Compiler.Emitter.RootNode.GlobalMethod;
    }


    /// <summary>
    /// type_stmt = type_field | type_method | autoconstr ;
    /// </summary>
    private void ParseTypeStmt()
    {
      // save current LexemID for backtracking
      var lexemId = GetLexemId();

      if (PeekLexem(LexemType.Type))
        Error(Resources.errTypeNesting);
      // "autoconstruct
      else if (PeekLexem(LexemType.Autoconstruct))
        ParseAutoconstruct();
      else
      {
        // ? "static"
        if (PeekLexem(LexemType.Static)) SkipLexem();

        // signature
        ParseSignature();
        // ? "@"
        var lexem = GetLexem();
        if (lexem.Type == LexemType.Identifier)
        {
          SetLexemId(lexemId);
          if (lexem.Data[0] == '@')
            ParseTypeField();
          else
            ParseTypeMethod();
        }
        else
          Error(Resources.errIdentifierExpected);
      }
    }


    /// <summary>
    /// type_field = [ "static" ], "var", "@", field_name, [ "=", constant ],  NL ;
    /// </summary>
    private void ParseTypeField()
    {
      // ? "static"
      bool isStatic = false;
      if(PeekLexem(LexemType.Static))
      {
        SkipLexem();
        isStatic = true;
      }

      // signature
      var fieldType = ParseSignature();

      // <@field_name>
      if (!PeekLexem(LexemType.Identifier)) Error(Resources.errFieldNameExpected);
      var fieldName = GetLexem();

      if (fieldName.Data[0] != '@')
        Error(Resources.errFieldAtmarkExpected);
      if (fieldName.Data.Length == 1)
        Error(Resources.errFieldNameExpected);

      var field = Compiler.Emitter.CreateField(Compiler.Emitter.CurrentType, fieldName.Data, fieldType, isStatic);
      field.Lexem = fieldName;
      SkipLexem();

      // ? "="
      if(PeekLexem(LexemType.Assign))
      {
        SkipLexem();
        field.Expression = ParseExpr();
      }

      // NL
      if (!PeekLexem(LexemType.NewLine)) Error(Resources.errNewLineExpected);
      SkipLexem(true);
    }


    /// <summary>
    /// type_method = [ "static" ], signature, method_name, [ ":", signature, identifier, { ",", signature, identifier } ], "{", { local_stmt }, "}" ;
    /// </summary>
    private void ParseTypeMethod()
    {
      // ? "static"
      bool isStatic = false;
      if (PeekLexem(LexemType.Static))
      {
        SkipLexem();
        isStatic = true;
      }

      // signature
      var methodType = ParseSignature();

      // <method_name>
      if (!PeekLexem(LexemType.Identifier)) Error(Resources.errMethodNameExpected);
      var methodName = GetLexem();
      if (ReservedMethods.ContainsKey(methodName.Data) && ReservedMethods[methodName.Data] != methodType.Signature)
        Error(String.Format(Resources.errSpecialIncorrectReturnType, methodName.Data, ReservedMethods[methodName.Data]));

      // validate static constructor
      if (methodName.Data == "construct" && isStatic)
        Error(String.Format(Resources.errStaticConstructor, Compiler.Emitter.CurrentType.Name));

      SkipLexem();

      HashList<ParameterNode> parameters = null;
      // ? "(", params, ")"
      if(PeekLexem(LexemType.ParenOpen))
      {
        SkipLexem();

        parameters = new HashList<ParameterNode>();
        var idx = 0;

        while(!PeekLexem(LexemType.ParenClose, LexemType.EOF))
        {
          // separator
          if(idx > 0)
          {
            if (!PeekLexem(LexemType.Comma)) Error(Resources.errCommaExpected);
            SkipLexem();
          }

          // signature
          var paramType = ParseSignature();
          // <name>
          if (!PeekLexem(LexemType.Identifier)) Error(Resources.errParameterNameExpected);
          var paramName = GetLexem();
          if (parameters.Contains(paramName.Data)) Error(String.Format(Resources.errParameterNameDuplicated, paramName.Data));
          parameters.Add(paramName.Data, new ParameterNode(paramName.Data, paramType, idx));
          SkipLexem();
          idx++;
        }

        // ")"
        if (!PeekLexem(LexemType.ParenClose))
          Error(Resources.errParenExpected);
        SkipLexem();
      }

      MethodNode method;
      if (methodName.Data == "construct")
        method = Compiler.Emitter.CreateCtor(Compiler.Emitter.CurrentType, parameters);
      else
        method = Compiler.Emitter.CreateMethod(Compiler.Emitter.CurrentType, methodName.Data, methodType, parameters, isStatic);

      method.Lexem = methodName;
      Compiler.Emitter.CurrentMethod = method;

      // lookahead "{"
      if (!PeekLexem(LexemType.CurlyOpen)) Error(Resources.errMethodCurlyBrace);

      method.Body = ParseCodeBlock();

      Compiler.Emitter.CurrentMethod = null;
    }


    /// <summary>
    /// autoconstr = "autoconstruct", NL ;
    /// </summary>
    private void ParseAutoconstruct()
    {
      if (Compiler.Emitter.CurrentType.AutoConstruct)
        Error(Resources.errAutoconstructRepeated);

      Compiler.Emitter.CurrentType.AutoConstruct = true;
      SkipLexem();

      if (!PeekLexem(LexemType.NewLine)) Error(Resources.errNewLineExpected);
      SkipLexem(true);
    }


    /// <summary>
    /// enum = "enum", type_name, "{", { identifier, NL }, "}" ;
    /// </summary>
    public void ParseEnum()
    {
      // "enum"
      SkipLexem();

      // <name>
      var typeName = GetLexem();
      if(typeName.Type != LexemType.Identifier || typeName.Data[0] == '@') Error(Resources.errTypeNameExpected);
      SkipLexem();

      try
      {
        Compiler.Emitter.CurrentType = Compiler.Emitter.CreateEnum(typeName.Data);
      }
      catch(CompilerException ex)
      {
        ex.AffixToLexem(typeName);
        throw;
      }

      Compiler.Emitter.CurrentType.Lexem = typeName;

      // ? ":"
      if (PeekLexem(LexemType.Colon))
        Error(Resources.errEnumParent);

      // "{"
      if (!PeekLexem(LexemType.CurlyOpen))
        Error(Resources.errTypeCurlyBrace);
      SkipLexem();

      // { <identifier>, "NL" }
      while (!PeekLexem(LexemType.CurlyClose, LexemType.EOF))
      {
        var currValue = GetLexem();
        if (currValue.Type != LexemType.Identifier || currValue.Data[0] == '@') Error(Resources.errEnumValueNameExpected);
        SkipLexem();

        try
        {
          Compiler.Emitter.CreateEnumValue(Compiler.Emitter.CurrentType, currValue.Data);
        }
        catch(CompilerException ex)
        {
          ex.AffixToLexem(currValue);
          throw;
        }

        // ? "," | ";"
        if (PeekLexem(LexemType.Comma, LexemType.Semicolon)) SkipLexem();
      }

      // "}"
      if (!PeekLexem(LexemType.CurlyClose)) Error(Resources.errTypeCurlyBrace);
      SkipLexem();

      Compiler.Emitter.CurrentType = null;
    }


    /// <summary>
    /// include = "include", string, NL;
    /// </summary>
    public void ParseInclude()
    {
      // "include"
      SkipLexem();

      // <file>
      if (!PeekLexem(LexemType.StringLiteral)) Error(Resources.errIncludePathExpected);
      var fileName = GetLexem().Data;

      // check if file exists
      FileInfo fi = null;
      string localPath = new FileInfo(Compiler.CurrentFiles.Peek().Name).DirectoryName;
      try
      {
        fi = new FileInfo(localPath + "\\" + fileName);
        if (!fi.Exists) Error(Resources.errFileNotFound);

        // check file for having already been compiled
        if (Compiler.ProcessedFiles.Contains(fi.FullName))
          Error(String.Format(Resources.errFileAlreadyCompiled, fi.Name));
      }
      catch
      {
        Error(String.Format(Resources.errFileNotFound, fileName));
      }

      SkipLexem();

      // compile the file
      Compiler.ParseFile(fi.FullName);

      // NL
      if (!PeekLexem(LexemType.NewLine, LexemType.EOF)) Error(Resources.errNewLineExpected);
      SkipLexem(true);
    }


    /// <summary>
    /// use = "use" identifier NL;
    /// </summary>
    public void ParseUse()
    {
      // "use"
      SkipLexem();

      // <type>
      if (!PeekLexem(LexemType.Identifier)) Error(Resources.errTypeNameExpected);
      var typeName = GetLexem();

      try
      {
        Compiler.Emitter.UseType(typeName.Data);
      }
      catch(CompilerException ex)
      {
        ex.AffixToLexem(typeName);
        throw;
      }

      SkipLexem();

      // NL
      if (!PeekLexem(LexemType.NewLine, LexemType.EOF)) Error(Resources.errNewLineExpected);
      SkipLexem(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public SignatureNode ParseSignature()
    {
      var sb = new StringBuilder();
      var isVoid = false;

      // <typename>
      if (PeekLexem(LexemType.Void))
      {
        sb.Append("void");
        isVoid = true;
      }
      else if (PeekLexem(LexemType.Identifier))
        sb.Append(GetLexem().Data);
      else
        Error(Resources.errTypeNameExpected);

      SkipLexem();

      // { "[]" }
      while(PeekLexem(LexemType.SquareOpen) && PeekLexem(1, LexemType.SquareClose))
      {
        if (isVoid) Error(Resources.errVoidCompoundType);
        sb.Append("[]");
        SkipLexem(); SkipLexem();
      }

      return new SignatureNode(sb.ToString());
    }


    /// <summary>
    /// local_stmt = if | for | while | return | break | redo | var | generic_stmt ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseLocalStmt()
    {
      var lexem = GetLexem();
      switch(lexem.Type)
      {
        case LexemType.If: return ParseIf();
        case LexemType.For: return ParseFor();
        case LexemType.While: return ParseWhile();
        case LexemType.Return: return ParseReturn();
        case LexemType.Print:
        case LexemType.PrintLine: return ParsePrint();
        case LexemType.Exit: return ParseExit();
        case LexemType.Emit: return ParseEmit();

        case LexemType.Break:
        case LexemType.Redo: return ParseLoopStmt();

        case LexemType.Var: return ParseVarDeclaration();

        case LexemType.Type:
        case LexemType.Enum: Error(Resources.errTypeNesting); return null;

        case LexemType.Include:
        case LexemType.Use: Error(Resources.errGlobalStmtInLocal); return null;

        default: return ParseGenericStmt();
      }
    }

    /// <summary>
    /// if = "if", expr, "do", code_block [ "else", code_block ] ;
    /// </summary>
    /// <returns></returns>
    public IfNode ParseIf()
    {
      // "if"
      var node = new IfNode();
      node.Lexem = GetLexem();
      SkipLexem();

      // <condition>
      node.Condition = ParseExpr();

      // "do"
      if (PeekLexem(LexemType.Do))
        SkipLexem();
      else if(!PeekLexem(LexemType.CurlyOpen))
        Error(Resources.errDoExpected);

      // code_block
      node.TrueBlock = ParseCodeBlock();

      // ? "else"
      if(PeekLexem(LexemType.Else))
      {
        SkipLexem();
        node.FalseBlock = ParseCodeBlock();
      }

      return node;
    }


    /// <summary>
    /// while = "while", expr, "do", loop_block ;
    /// </summary>
    /// <returns></returns>
    public WhileNode ParseWhile()
    {
      // "while"
      var node = new WhileNode();
      node.Lexem = GetLexem();
      SkipLexem();

      // <condition>
      node.Condition = ParseExpr();

      // "do"
      if (PeekLexem(LexemType.Do))
        SkipLexem();
      else if (!PeekLexem(LexemType.CurlyOpen))
        Error(Resources.errDoExpected);

      // code_block
      InLoop++;
      node.Body = ParseCodeBlock();
      InLoop--;

      return node;
    }


    /// <summary>
    /// for = "for", var_name, "in", expr, "do", loop_block ;
    /// </summary>
    /// <returns></returns>
    public ForNode ParseFor()
    {
      // "for"
      var node = new ForNode();
      node.Lexem = GetLexem();
      SkipLexem();

      // <variable>
      if(!PeekLexem(LexemType.Identifier)) Error(Resources.errVariableNameExpected);
      var variable = GetLexem();
      SkipLexem();

      // ? "=>"
      if(PeekLexem(LexemType.Arrow))
      {
        SkipLexem();
        if (!PeekLexem(LexemType.Identifier)) Error(Resources.errVariableNameExpected);

        node.Key = variable;
        node.Item = GetLexem();
        SkipLexem();
      }
      else
        node.Item = variable;

      // "in"
      if (!PeekLexem(LexemType.In)) Error(Resources.errInExpected);
      SkipLexem();

      node.Iterable = ParseExpr();

      // "do"
      if (PeekLexem(LexemType.Do))
        SkipLexem();
      else if (!PeekLexem(LexemType.CurlyOpen))
        Error(Resources.errDoExpected);

      // code_block
      InLoop++;
      node.Body = ParseCodeBlock();
      InLoop--;

      return node;
    }


    /// <summary>
    /// return = "return", [ expr ], NL ;
    /// </summary>
    /// <returns></returns>
    public ReturnNode ParseReturn()
    {
      if (Compiler.Emitter.CurrentMethod == null)
        Error(Resources.errReturnOutsideMethod);

      var node = new ReturnNode();
      node.Lexem = GetLexem();

      // "return"
      SkipLexem();

      if(PeekLexem(LexemType.NewLine))
      {
        if (Compiler.Emitter.CurrentMethod.Type.Signature != "void")
          Error(Resources.errReturnExpressionExpected);
      }

      if (Compiler.Emitter.CurrentMethod.Type.Signature == "void")
      {
        if (!PeekLexem(LexemType.NewLine))
          Error(String.Format(Resources.errReturnVoidExpected, Compiler.Emitter.CurrentMethod.Name));
      }
      else
        node.Expression = ParseExpr();

      return node;
    }


    /// <summary>
    /// print = "print" 
    /// </summary>
    /// <returns></returns>
    public PrintNode ParsePrint()
    {
      var node = new PrintNode();
      if (PeekLexem(LexemType.PrintLine))
        node.PrintLine = true;

      // "print" | "println"
      SkipLexem();

      // ? NL
      if(PeekLexem(LexemType.NewLine, LexemType.EOF))
        return node;

      // ? param
      node.Parameters.Add(ParseExpr());

      // ? ",", param
      while(!PeekLexem(LexemType.NewLine) && PeekLexem(LexemType.Comma))
      {
        SkipLexem();
        node.Parameters.Add(ParseExpr());
      }

      // "NL"
      if (!PeekLexem(LexemType.NewLine, LexemType.EOF)) Error(Resources.errNewLineExpected);
      SkipLexem(true);

      return node;
    }


    /// <summary>
    /// code_block = local_stmt | "{", { local_stmt }, "}"
    /// </summary>
    /// <returns></returns>
    public CodeBlockNode ParseCodeBlock()
    {
      var block = new CodeBlockNode();

      // ? "{"
      if (PeekLexem(LexemType.CurlyOpen))
      {
        SkipLexem();

        // { local_stmt }
        
        while (!PeekLexem(LexemType.CurlyClose, LexemType.EOF))
        {
          var stmt = ParseLocalStmt();
          block.Statements.Add(stmt);

          // check return
          if (stmt is ReturnNode)
            block.AllPathsReturn = true;
          else if (stmt is IfNode)
            block.AllPathsReturn = (stmt as IfNode).AllPathsReturn;
        }

        if (!PeekLexem(LexemType.CurlyClose)) Error(Resources.errBlockCurlyBrace);
        SkipLexem();

        return block;
      }
      else
      {
        var stmt = ParseLocalStmt();
        block.Statements.Add(stmt);

        // check return
        if (stmt is ReturnNode)
          block.AllPathsReturn = true;
        else if (stmt is IfNode)
          block.AllPathsReturn = (stmt as IfNode).AllPathsReturn;
      }

      return block;
    }


    /// <summary>
    /// loop_stmt = "break" | "redo" ;
    /// </summary>
    /// <param name="type">StatementType</param>
    /// <returns></returns>
    public SyntaxTreeNode ParseLoopStmt()
    {
      if (InLoop == 0)
        Error(Resources.errBreakRedoOutsideLoop);

      var stmt = GetLexem();
      SyntaxTreeNode node;
      SkipLexem();

      // "break" | "redo"
      if (stmt.Type == LexemType.Break)
        node = new BreakNode();
      else
        node = new RedoNode();

      node.Lexem = stmt;

      // NL
      if (!PeekLexem(LexemType.NewLine)) Error(Resources.errNewLineExpected);
      SkipLexem(true);

      return node;
    }


    /// <summary>
    /// exit = "exit"
    /// </summary>
    /// <returns></returns>
    public ExitNode ParseExit()
    {
      var node = new ExitNode();
      node.Lexem = GetLexem();
      SkipLexem();
      return node;
    }


    /// <summary>
    /// var_decl = "var", var_name, "=", expr | matrix
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseVarDeclaration()
    {
      SyntaxTreeNode result = null;

      // "var"
      SkipLexem();

      // <name>
      if (!PeekLexem(LexemType.Identifier)) Error(Resources.errVariableNameExpected);
      var varName = GetLexem();
      if (varName.Data[0] == '@') Error(Resources.errVariableNameExpected);
      SkipLexem();

      // basic assignment
      if (PeekLexem(LexemType.Assign))
      {
        // "="
        SkipLexem();

        var node = new VarDeclarationNode(varName.Data);
        node.Lexem = varName;

        // matrix | expr
        if (PeekLexem(LexemType.DoubleSquareOpen))
          node.Expression = ParseMatrix();
        else
          node.Expression = ParseExpr();

        result = node;
      }

      // splat assignment
      else if(PeekLexem(LexemType.Comma))
      {
        var node = new VarSplatNode();
        node.Names.Add(varName);

        while(!PeekLexem(LexemType.Assign, LexemType.EOF))
        {
          // ","
          if (!PeekLexem(LexemType.Comma)) Error(Resources.errCommaExpected);
          SkipLexem();

          // "<name>"
          if (!PeekLexem(LexemType.Identifier)) Error(Resources.errVariableNameExpected);

          varName = GetLexem();
          SkipLexem();

          if (varName.Data[0] == '@') Error(Resources.errVariableNameExpected);
          node.Names.Add(varName);
        }

        // "="
        if (!PeekLexem(LexemType.Assign)) Error(Resources.errVariableAssignmentExpected);
        SkipLexem();

        node.Expression = ParseExpr();

        result = node;
      }

      else
        Error(Resources.errVariableAssignmentExpected);

      // NL
      if (!PeekLexem(LexemType.NewLine, LexemType.EOF))
        Error(Resources.errNewLineExpected);

      return result;
    }


    /// <summary>
    /// generic_stmt = expr, [ "=", expr ], NL;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseGenericStmt()
    {
      var expr = ParseExpr();

      if (PeekLexem(LexemTypeGroup.Assignment))
      {
        if (expr is IAssignable)
        {
          var lexem = GetLexem();
          SkipLexem();

          // ? "="
          if (lexem.Type == LexemType.Assign)
            expr = ParseAssignable(expr);

          // ? "<=>"
          else if(lexem.Type == LexemType.Exchange)
          {
            var rightExpr = ParseExpr();
            if(!(rightExpr is IdentifierNode))
              Error(Resources.errLvalueExpected, lexem);

            expr = Expr.Exchange(expr as IdentifierNode, rightExpr as IdentifierNode);
          }

          else
            expr = Expr.ShortAssign(expr as IdentifierNode, lexem.Type, ParseExpr());
        }
        else
          Error(Resources.errLvalueExpected);
      }

      // NL
      if (!PeekLexem(LexemType.NewLine, LexemType.EOF)) Error(Resources.errNewLineExpected);
      SkipLexem(true);

      return expr;
    }


    /// <summary>
    /// assignable = matrix | expr
    /// </summary>
    /// <param name="curr"></param>
    /// <returns></returns>
    public SyntaxTreeNode ParseAssignable(SyntaxTreeNode curr)
    {
      // ? "[["
      SyntaxTreeNode expr;
      if (PeekLexem(LexemType.DoubleSquareOpen))
        expr = ParseMatrix();
      else
        expr = ParseExpr();

      // "a" -> "a="
      if (curr is IdentifierGetNode)
        return Expr.IdentifierSet(curr as IdentifierGetNode, expr);

      // "a[...]" -> "a[...]="
      else if (curr is ArrayGetNode)
        return Expr.ArraySet(curr as ArrayGetNode, expr);

      // "a[...,...]" -> "a[...,...]="
      else
        return Expr.MatrixSet(curr as MatrixGetNode, expr);
    }


    /// <summary>
    /// expr = block1, { sign1, block1 } ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseExpr()
    {
      // block1
      var node = ParseBlock1();

      // { sign1, block1 }
      while(PeekLexem(LexemTypeGroup.Logical))
      {
        var lexem = GetLexem(0, true);
        if (lexem.Type == LexemType.NewLine) return node;

        SkipLexem();
        BinaryOperatorNode newNode = null;
        switch(lexem.Type)
        {
          case LexemType.And: newNode = new OperatorAndNode(); break;
          case LexemType.Or: newNode = new OperatorOrNode(); break;
        }

        // block1
        newNode.Left = node;
        newNode.Right = ParseBlock1();
        newNode.Lexem = lexem;

        node = newNode;
      }

      // ? "as" type
      if(PeekLexem(LexemType.As))
      {
        SkipLexem();
        var lexem = GetLexem();
        var type = ParseSignature();
        node = new AsNode(node, type.Signature);
        node.Lexem = lexem;
      }

      return node;
    }


    /// <summary>
    /// block1 = block2 { sign2, block2 } 
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock1()
    {
      // block2
      var node = ParseBlock2();

      // [ sign2, block2 ]
      if (PeekLexem(LexemTypeGroup.Sign2))
      {
        var lexem = GetLexem(0, true);
        if (lexem.Type == LexemType.NewLine) return node;

        SkipLexem();
        BinaryOperatorNode newNode;
        if (lexem.Type == LexemType.In)
          newNode = new InNode();
        else
          newNode = new OperatorCompareNode(lexem.Type);

        // block2
        newNode.Left = node;
        newNode.Right = ParseBlock2();
        newNode.Lexem = lexem;

        node = newNode;
      }

      return node;
    }


    /// <summary>
    /// block2 = block3, { sign3, block3 } ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock2()
    {
      // ? [ "-" | "!" ]
      Lexem invertLexem = null;
      if(PeekLexem(LexemType.Subtract, LexemType.Not))
      {
        invertLexem = GetLexem();
        SkipLexem();
      }

      // block3
      var node = ParseBlock3();

      if(invertLexem != null)
      {
        if (invertLexem.Type == LexemType.Subtract)
          node = new OperatorInvertNode(node);
        else
          node = new OperatorNegateNode(node);

        node.Lexem = invertLexem;
      }

      // { sign3, block3 }
      while (PeekLexem(LexemTypeGroup.Sign3))
      {
        var lexem = GetLexem(0, true);
        if (lexem.Type == LexemType.NewLine) return node;

        SkipLexem();
        BinaryOperatorNode newNode;
        switch (lexem.Type)
        {
          case LexemType.Add: newNode = new OperatorAddNode(); break;
          case LexemType.Subtract: newNode = new OperatorSubtractNode(); break;
          default: throw new NotImplementedException();
        }

        // block3
        newNode.Left = node;
        newNode.Right = ParseBlock3();
        newNode.Lexem = lexem;

        node = newNode;
      }

      return node;
    }


    /// <summary>
    /// block3 = block4, { sign4, block4 } ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock3()
    {
      // block4
      var node = ParseBlock4();

      // { sign4, block4 }
      while (PeekLexem(LexemTypeGroup.Sign4))
      {
        var lexem = GetLexem(0, true);
        if (lexem.Type == LexemType.NewLine) return node;

        SkipLexem();
        BinaryOperatorNode newNode;
        switch (lexem.Type)
        {
          case LexemType.Multiply: newNode = new OperatorMultiplyNode(); break;
          case LexemType.Divide: newNode = new OperatorDivideNode(); break;
          case LexemType.Remainder: newNode = new OperatorRemainderNode(); break;
          default: throw new NotImplementedException();
        }

        // block4
        newNode.Left = node;
        newNode.Right = ParseBlock4();
        newNode.Lexem = lexem;

        node = newNode;
      }

      return node;
    }


    /// <summary>
    /// block4 = block5, { sign5, block5 } ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock4()
    {
      // block5
      var node = ParseBlock5();

      // { sign5, block5 }
      while (PeekLexem(LexemType.Power, LexemType.NewLine))
      {
        var lexem = GetLexem(0, true);
        if (lexem.Type == LexemType.NewLine) return node;

        var newNode = new OperatorPowerNode();
        newNode.Lexem = lexem;
        SkipLexem();

        // block5
        newNode.Left = node;
        newNode.Right = ParseBlock5();
        node = newNode;
      }

      return node;
    }


    /// <summary>
    /// block5 = block6, { sign6, block6 } ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock5()
    {
      // block6
      var node = ParseBlock6();

      // { sign6, block6 }
      while (PeekLexem(LexemTypeGroup.Binary))
      {
        var lexem = GetLexem(0, true);
        if (lexem.Type == LexemType.NewLine) return node;

        SkipLexem();
        BinaryOperatorNode newNode = null;
        switch (lexem.Type)
        {
          case LexemType.BinaryAnd: newNode = new OperatorBinaryAndNode(); break;
          case LexemType.BinaryOr: newNode = new OperatorBinaryOrNode(); break;
          case LexemType.BinaryXor: newNode = new OperatorBinaryXorNode(); break;
          case LexemType.BinaryShiftLeft: newNode = new OperatorBinaryShiftLeftNode(); break;
          case LexemType.BinaryShiftRight: newNode = new OperatorBinaryShiftRightNode(); break;
        }

        // block6
        newNode.Left = node;
        newNode.Right = ParseBlock6();
        newNode.Lexem = lexem;

        node = newNode;
      }

      return node;
    }


    /// <summary>
    /// block6 = block7, { ".", accessor } ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock6()
    {
      IdentifierNode newNode;
      // block7
      var node = ParseBlock7();

      // { sign7, block7 }
      while (true)
      {
        if(PeekLexem(LexemType.Dot))
        {
          // a dot can be spanned across lines
          SkipLexem();

          newNode = ParseAccessor();
        }
        else if(PeekLexem(LexemType.NewLine))
          break;
        else if(PeekLexem(LexemType.SquareOpen))
        {
          // an array indexer, however, cannot
          // because an expression can start with an array
          // and spanning indexer across lines may accidentally consider an array below an indexer
          newNode = ParseIndex();
          newNode.Lexem = node.Lexem;
        }
        else
          break;

        // affix to current expression
        newNode.ExpressionPrefix = node;
        node = newNode;
      }

      return node;
    }


    /// <summary>
    /// block7 = block8, [ "..", block8 ] ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock7()
    {
      var node = ParseBlock8();

      if(PeekLexem(LexemType.NewLine))
        return node;

      // integer range
      else if(PeekLexem(LexemType.DoubleDot))
      {
        var range = new RangeNode();
        range.Lexem = GetLexem();
        SkipLexem();

        range.From = node;
        range.To = ParseBlock8();

        node = range;
      }

      // float range with step
      else if(PeekLexem(LexemType.Tilde))
      {
        var range = new FloatRangeNode();
        range.Lexem = GetLexem();
        SkipLexem();

        range.From = node;
        range.Step = ParseBlock8();

        // "~"
        if (!PeekLexem(LexemType.Tilde))
          Error(Resources.errTildeExpected);
        SkipLexem();

        range.To = ParseBlock8();

        node = range;
      }

      return node;
    }


    /// <summary>
    /// block8 = [ type_name, ":" ], accessor | literal | newobj | "(", expr, ")" ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseBlock8()
    {
      // ? literals
      if (PeekLexem(LexemTypeGroup.Literal))
        return ParseLiteral();

      // ? "{"
      else if (PeekLexem(LexemType.CurlyOpen))
        return ParseDict();

      // ? "new"
      else if (PeekLexem(LexemType.New))
        return ParseNew();

      // ? "null"
      else if (PeekLexem(LexemType.Null))
      {
        var lexem = GetLexem();
        var node = new NullNode();
        node.Lexem = lexem;
        SkipLexem();
        return node;
      }

      // ? "flow_sim"
      else if (PeekLexem(LexemType.Simulate))
        return ParseSimulate();

      // ? identifier
      else if (PeekLexem(LexemType.Identifier))
      {
        var lexem = GetLexem();
        if (lexem.Data[0] == '@')
          return ParseAtmarkIdentifier();

        else
        {
          // classprefix ":"
          Lexem classPrefix = null;
          if (PeekLexem(1, LexemType.Colon))
          {
            classPrefix = GetLexem();
            SkipLexem();
            SkipLexem();
          }

          var node = ParseAccessor();
          node.TypePrefix = classPrefix;
          return node;
        }
      }

      // ? subexpr
      else if (PeekLexem(LexemType.ParenOpen))
      {
        // "("
        SkipLexem();

        var node = ParseExpr();

        // ")"
        if (!PeekLexem(LexemType.ParenClose)) Error(Resources.errParenExpected);
        SkipLexem(true);

        return node;
      }

      else if (PeekLexem(LexemType.DoubleSquareOpen))
        Error(Resources.errMatrixInExpression);

      else
        Error(Resources.errExpressionExpected);

      return null;
    }


    /// <summary>
    /// Parses out an identifier starting with a @ and performs additional checks
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseAtmarkIdentifier()
    {
      var lexem = GetLexem();

      // bare @
      if (lexem.Data.Length == 1)
      {
        // make sure we have a method and it's not static
        if (Compiler.Emitter.CurrentMethod == null || Compiler.Emitter.CurrentMethod.Static)
          Error(Resources.errThisInStaticContext);

        SkipLexem();

        var node = new ThisNode();
        node.Lexem = lexem;
        return node;
      }
      else
      {
        lexem.Data = lexem.Data.SafeSubstring(1, lexem.Data.Length - 1);

        // make sure we're inside a method
        if (Compiler.Emitter.CurrentMethod == null)
          Error(Resources.errFieldOutsideType);

        // parse accessor
        var lexemId = GetLexemId();
        var node = ParseAccessor();

        node.AtmarkPrefix = true;
        return node;
      }
    }


    /// <summary>
    /// literal = int | float | string | bool | complex ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseLiteral()
    {
      var lexem = GetLexem();
      SyntaxTreeNode node = null;

      switch (lexem.Type)
      {
        case LexemType.IntLiteral: node = new IntNode(Compiler.Lexer.RetrieveInt(lexem.Data)); break;
        case LexemType.FloatLiteral: node = new FloatNode(Compiler.Lexer.RetrieveFloat(lexem.Data)); break;
        case LexemType.ComplexLiteral: node = new ComplexNode(0, Compiler.Lexer.RetrieveComplex(lexem.Data)); break;
        case LexemType.StringLiteral: node = new StringNode(lexem.Data); break;
        case LexemType.TrueLiteral: node = new BoolNode(true); break;
        case LexemType.FalseLiteral: node = new BoolNode(false); break;
      }

      SkipLexem();
      node.Lexem = lexem;
      return node;
    }


    /// <summary>
    /// array = "new", "[", { expr }, "]"
    /// </summary>
    /// <returns></returns>
    public ArrayNode ParseArray()
    {
      // "new"
      var lexem = GetLexem();
      SkipLexem();

      // "["
      SkipLexem();

      if (PeekLexem(LexemType.SquareClose))
        Error(Resources.errEmptyArray, lexem);

      var node = new ArrayNode();
      node.Lexem = lexem;
      while (!PeekLexem(LexemType.SquareClose, LexemType.EOF))
      {
        // parse expression
        node.Values.Add(ParseArrayExpr());

        // ? ";"
        if (PeekLexem(LexemType.Semicolon)) SkipLexem();
      }

      // "]"
      if (!PeekLexem(LexemType.SquareClose)) Error(Resources.errSquareBracketExpected);
      SkipLexem(true);

      return node;
    }

    /// <summary>
    /// array_expr = [ "-" ], ( literal | identifier | "(" expr ")" ) ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseArrayExpr()
    {
      SyntaxTreeNode node = null;

      // ? "-"
      var invert = false;
      if (PeekLexem(LexemType.Subtract))
      {
        invert = true;
        SkipLexem();
      }

      if (PeekLexem(LexemTypeGroup.Literal))
        node = ParseLiteral();
      else if (PeekLexem(LexemType.Identifier))
      {
        var lexem = GetLexem();
        var idNode = new IdentifierGetNode(lexem.Data);

        if (lexem.Data[0] == '@')
        {
          idNode.Name = idNode.Name.Substring(1, idNode.Name.Length - 1);
          idNode.AtmarkPrefix = true;
        }

        node = idNode;
        node.Lexem = lexem;
        SkipLexem();
      }
      else if (PeekLexem(LexemType.ParenOpen))
      {
        // "("
        SkipLexem();

        node = ParseExpr();

        // ")"
        if (!PeekLexem(LexemType.ParenClose)) Error(Resources.errParenExpected);
        SkipLexem(true);
      }
      else
        Error(Resources.errArrayExpressionParensExpected);

      // invert if needed
      if (invert)
        node = new OperatorInvertNode(node);

      return node;
    }


    /// <summary>
    /// "[[", { { expr }, NL }, "]]" ;
    /// </summary>
    /// <returns></returns>
    public MatrixNode ParseMatrix()
    {
      // "[["
      var lexem = GetLexem();
      SkipLexem();

      if (PeekLexem(LexemType.DoubleSquareClose))
        Error(Resources.errEmptyMatrix);

      var matrix = new MatrixNode();
      matrix.Lexem = lexem;

      // { { expr } NL }
      while(!PeekLexem(LexemType.DoubleSquareClose, LexemType.EOF))
      {
        var list = new List<SyntaxTreeNode>();

        // <expr>
        while(!PeekLexem(LexemType.NewLine, LexemType.DoubleSquareClose, LexemType.EOF))
          list.Add(ParseMatrixExpr());

        // NL
        if(PeekLexem(LexemType.NewLine))
          SkipLexem(true);

        if(list.Count > 0)
          matrix.MatrixItems.Add(list);
      }

      // "]]"
      if (!PeekLexem(LexemType.DoubleSquareClose))
        Error(Resources.errDoubleSquareBracketExpected);

      SkipLexem();

      return matrix;
    }


    /// <summary>
    /// matrix_expr = [ "-" ], ( int | float | identifier | "(" expr ")" ) ;
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseMatrixExpr()
    {
      SyntaxTreeNode node = null;

      // ? "-"
      var invert = false;
      if(PeekLexem(LexemType.Subtract))
      {
        invert = true;
        SkipLexem();
      }

      if (PeekLexem(LexemType.IntLiteral, LexemType.FloatLiteral))
        node = ParseLiteral();
      else if(PeekLexem(LexemType.Identifier))
      {
        var lexem = GetLexem();
        var idNode = new IdentifierGetNode(lexem.Data);

        if(lexem.Data[0] == '@')
        {
          idNode.Name = idNode.Name.Substring(1, idNode.Name.Length - 1);
          idNode.AtmarkPrefix = true;
        }

        node = idNode;
        node.Lexem = lexem;
        SkipLexem();
      }
      else if (PeekLexem(LexemType.ParenOpen))
      {
        // "("
        SkipLexem();

        node = ParseExpr();

        // ")"
        if (!PeekLexem(LexemType.ParenClose)) Error(Resources.errParenExpected);
        SkipLexem(true);
      }
      else
        Error(Resources.errMatrixExpressionParensExpected);

      // invert if needed
      if(invert)
        node = new OperatorInvertNode(node);

      return node;
    }


    /// <summary>
    /// new = "new", type_name,  [ ( "(", { expr, "," }, ")" ]
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseNew()
    {
      // ? "new", "["
      if (PeekLexem(1, LexemType.SquareOpen))
        return ParseArray();

      SkipLexem();
      if (!PeekLexem(LexemType.Identifier)) Error(Resources.errTypeNameExpected);
      var node = new NewNode();
      node.Lexem = GetLexem();
      var accessorNode = ParseAccessor();

      if (accessorNode is IdentifierGetNode)
        node.Name = (accessorNode as IdentifierGetNode).Name;
      else if (accessorNode is IdentifierInvokeNode)
      {
        node.Name = (accessorNode as IdentifierInvokeNode).Name;
        node.Parameters = (accessorNode as IdentifierInvokeNode).Parameters;
      }

      return node;
    }

    /// <summary>
    /// dict = "{", { expr, "=>", expr, ( NL | ";") }, "}"
    /// </summary>
    /// <returns></returns>
    public DictNode ParseDict()
    {
      var node = new DictNode();

      // "{"
      SkipLexem();

      while(!PeekLexem(LexemType.CurlyClose, LexemType.EOF))
      {
        SyntaxTreeNode key = null;

        // ? <string> | <identifier>
        if (PeekLexem(LexemType.Identifier, LexemType.IntLiteral, LexemType.FloatLiteral, LexemType.StringLiteral))
        {
          key = Expr.String(GetLexem().Data);
          SkipLexem();
        }

        // ? "(" expr ")"
        else if (PeekLexem(LexemType.ParenOpen))
        {
          SkipLexem();
          key = ParseExpr();

          if (!PeekLexem(LexemType.ParenClose))
            Error(Resources.errParenExpected);
          SkipLexem();
        }
        else
          Error(Resources.errDictKeyExpected);

        // "=>"
        if (!PeekLexem(LexemType.Arrow)) Error(Resources.errDictArrowExpected);
        SkipLexem();

        // <value>
        var value = ParseExpr();

        node.Data.Add(new Tuple<SyntaxTreeNode, SyntaxTreeNode>(key, value));

        // ? ";" | NL
        if (PeekLexem(LexemType.Semicolon))
          SkipLexem();
      }

      if (!PeekLexem(LexemType.CurlyClose))
        Error(Resources.errBlockCurlyBrace);

      // "}"
      SkipLexem();
      return node;
    }
    
    /// <summary>
    /// index = "[", expr, [ ",", expr ], "]"
    /// </summary>
    /// <param name="parent">Parent node get index from</param>
    /// <returns></returns>
    public IdentifierNode ParseIndex()
    {
      // "["
      var lexem = GetLexem();
      SkipLexem();
      var index1 = ParseExpr();

      if (PeekLexem(LexemType.Comma))
      {
        SkipLexem();
        // ? ","
        // double index!
        var index2 = ParseExpr();

        // "]"
        if (!PeekLexem(LexemType.SquareClose)) Error(Resources.errSquareBracketExpected);
        SkipLexem(true);

        // look ahead: if "=" then assignment and break loop
        var node = new MatrixGetNode();
        node.Index1 = index1;
        node.Index2 = index2;
        node.Lexem = lexem;
        return node;
      }
      else
      {
        // "]"
        if (!PeekLexem(LexemType.SquareClose)) Error(Resources.errSquareBracketExpected);
        SkipLexem(true);
        
        var node = new ArrayGetNode();
        node.Index = index1;
        node.Lexem = lexem;
        return node;
      }
    }


    /// <summary>
    /// accessor = identifier, [ "(", { expr, "," }, ")" ]
    /// </summary>
    /// <returns></returns>
    public IdentifierNode ParseAccessor(bool allowInvoke = true)
    {
      // <name>
      IdentifierNode node;
      if (!PeekLexem(LexemType.Identifier)) Error(Resources.errIdentifierExpected);
      Lexem name = GetLexem();
      SkipLexem();

      // ? "("
      // invoke a function with argument list in brackets
      if (allowInvoke && PeekLexem(LexemType.ParenOpen))
      {
        SkipLexem();

        node = new IdentifierInvokeNode(name.Data);
        bool first = true;
        while(!PeekLexem(LexemType.ParenClose, LexemType.EOF))
        {
          // "," before each but the first argument
          if (!first)
          {
            if (!PeekLexem(LexemType.Comma)) Error(Resources.errCommaExpected);
            SkipLexem();
          }

          (node as IdentifierInvokeNode).Parameters.Add(ParseExpr());
          first = false;
        }

        // ")"
        if (!PeekLexem(LexemType.ParenClose)) Error(Resources.errParenExpected);
        SkipLexem();
      }
      // bare invoke
      else if (allowInvoke && !PeekLexem(LexemType.NewLine) && PeekLexem(LexemTypeGroup.Parameter))
      {
        node = new IdentifierInvokeNode(name.Data);
        (node as IdentifierInvokeNode).Parameters.Add(ParseExpr());

        while(PeekLexem(LexemType.Comma))
        {
          SkipLexem();
          (node as IdentifierInvokeNode).Parameters.Add(ParseExpr());
        }

        // check if a comma has been skipped?
        var nextLexem = GetLexem(0, true);
        if(LexemTypeGroup.Parameter.Contains(nextLexem.Type))
          Error(Resources.errCommaExpected);
      }
      else
        node = new IdentifierGetNode(name.Data);

      node.Lexem = name;

      return node;
    }

    /// <summary>
    /// emit = "emit", invoke, "every", expr, [ "with", expr ], [ "limit", expr ], [ "until", expr ]
    /// </summary>
    /// <returns></returns>
    public EmitNode ParseEmit()
    {
      // "emit"
      var node = new EmitNode();
      node.Lexem = GetLexem();
      SkipLexem();
      var type = Compiler.Emitter.CreateType(node.EmitterID.ToString(), ".emitter");

      // <action>
      node.Action = ParseExpr();
      var action = Compiler.Emitter.CreateMethod(type, "Action", new SignatureNode("void"));
      action.BuiltIn = true;

      if (PeekLexem(LexemType.Once))
      {
        // "once"
        SkipLexem();

        // "at"
        if (!PeekLexem(LexemType.Identifier) || GetLexem().Data != "at")
          Error(Resources.errAtExpected);

        // <time>
        node.Step = ParseExpr();
        node.Limit = Expr.Int(1);
      }
      else
      {
        // "every"
        if (PeekLexem(LexemType.Every))
        {
          SkipLexem();
          node.Step = ParseExpr();
        }

        // ? "with" <distribution>
        if (PeekLexem(LexemType.With))
        {
          SkipLexem();
          node.Distribution = ParseExpr();
        }

        // check for error
        if (node.Step == null && node.Distribution == null)
          Error(Resources.errTimeSpanExpected);

        // ? "limit" <limit>
        if (PeekLexem(LexemType.Limit))
        {
          SkipLexem();
          node.Limit = ParseExpr();
        }

        // ? "until" <condition>
        if (PeekLexem(LexemType.Until))
        {
          SkipLexem();
          node.Condition = ParseExpr();
          var condition = Compiler.Emitter.CreateMethod(type, "Condition", new SignatureNode("bool"));
          condition.BuiltIn = true;
        }
      }

      return node;
    }

    /// <summary>
    /// flow_sim = "flow_sim" expr
    /// </summary>
    /// <returns></returns>
    public SyntaxTreeNode ParseSimulate()
    {
      // "simulate"
      var lexem = GetLexem();
      SkipLexem();

      // "any" | "planner"
      var simType = GetLexem();
      if (simType.Type != LexemType.Identifier || !simType.Data.IsAnyOf("any", "planner"))
        Error(Resources.errSimulateType);
      SkipLexem();

      // ? "any"
      if (simType.Data == "any")
      {
        var node = new SimulateAnyNode();
        node.Lexem = lexem;

        if (!PeekLexem(LexemType.NewLine))
        {
          // <processors>
          node.Processors = ParseExpr();

          // ? "," <queue>
          if (PeekLexem(LexemType.Comma))
          {
            SkipLexem();
            node.MaxQueue = ParseExpr();
          }
        }

        return node;
      }

      // ? "planner"
      else
      {
        SimulatePlannerNode node = null;
        try
        {
          node = new SimulatePlannerNode();
        }
        catch(CompilerException ex)
        {
          ex.AffixToLexem(lexem);
          throw;
        }

        node.Lexem = lexem;
        node.Action = ParseExpr();

        Compiler.Emitter.CreatePlannerType();

        return node;
      }
    }
  }
}
