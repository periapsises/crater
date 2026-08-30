parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: block EOF;

block: statement*;

statement
    : variableDeclaration
    | functionDeclaration
    | doStatement
    | ifStatement
    | functionCall
    | assignment
    | whileLoop
    | repeatLoop
    | numericForLoop
    | genericForLoop
    | returnStatement
    | breakStatement
    ;

variableDeclaration: LOCAL? variableDeclarator (COMMA variableDeclarator)* (ASSIGN expressionList)?;

variableDeclarator: name=IDENTIFIER COLON typeName;

functionDeclaration: LOCAL? FUNCTION name=IDENTIFIER LPAREN parameters? RPAREN COLON returnTypes block END;

parameters: parameter (COMMA parameter)*;

parameter
    : name=IDENTIFIER COLON typeName    # NamedParameter
    | VARARGS COLON typeName            # VarargParameter
    ;

returnTypes: VOID | typeName (COMMA typeName)*;

doStatement: DO block END;

ifStatement: IF expression THEN block (elseIfStatement)* elseStatement? END;

elseIfStatement: ELSEIF expression THEN block;

elseStatement: ELSE block;

functionCall: expression LPAREN expressionList? RPAREN;

assignment: storageType (COMMA storageType)? ASSIGN expressionList;

storageType
    : storageType LSQRBRACKET expression RSQRBRACKET # ArrayStorage
    | storageType DOT IDENTIFIER                     # MemberStorgage
    | IDENTIFIER                                     # VariableStorage
    ;

whileLoop: WHILE condition=expression DO block END;

repeatLoop: REPEAT block UNTIL condition=expression;

numericForLoop: FOR variable=IDENTIFIER ASSIGN initializer=expression COMMA limit=expression (COMMA increment=expression)? DO block END;

genericForLoop: FOR variableDeclarator (COMMA variableDeclarator)* IN expression DO block END;

returnStatement: RETURN expressionList?;

breakStatement: BREAK;

typeName: typeName (QMARK | LSQRBRACKET RSQRBRACKET) | primaryType;

primaryType
    : IDENTIFIER            # NamedType
    | functionDefinition    # FunctionType
    | tableDefinition       # TableType
    ;

functionDefinition: FUN LPAREN (typeName (COMMA typeName)* VARARGS?)? RPAREN COLON returnTypes;

tableDefinition: LBRACKET (variableDeclarator (COMMA variableDeclarator)*)? RBRACKET;

expressionList: expression (COMMA expression)*;

expression
    : primaryExpression                                         # BaseExpression
    | op=(MINUS | NOT) expression                               # UnaryExpression
    | left=expression operator=(STAR | SLASH) right=expression  # MultiplicativeOperation
    | left=expression operator=(PLUS | MINUS) right=expression  # AdditiveOperation
    | left=expression CONCAT right=expression                   # ConcatenationOperation
    | left=expression logicalOperator right=expression          # LogicalOperation
    | left=expression AND right=expression                      # AndOperation
    | left=expression OR right=expression                       # OrOperation
    | functionValue                                             # FunctionLiteral
    | LBRACKET tableValues? RBRACKET                            # TableLiteral
    | LBRACKET expressionList? RBRACKET                         # ArrayLiteral
    | NUMBER                                                    # NumberLiteral
    | STRING                                                    # StringLiteral
    | (TRUE | FALSE)                                            # BooleanLiteral
    | NIL                                                       # NilLiteral
    ;

primaryExpression: prefixExpression postfixExpression*;

prefixExpression
    : LPAREN expression RPAREN  # ParenthesizedExpression
    | IDENTIFIER                # VariableReference;

postfixExpression
    : postfixFunctionCall
    | postfixBracketIndexing
    | postfixDotIndexing
    ;

postfixFunctionCall: LPAREN expressionList? RPAREN;

postfixBracketIndexing: LSQRBRACKET expression RSQRBRACKET;

postfixDotIndexing: DOT IDENTIFIER;

functionValue: FUNCTION LPAREN parameters? RPAREN COLON returnTypes block END;

tableValues: tableValue (COMMA tableValue)* COMMA?;

tableValue: IDENTIFIER ASSIGN expression;

logicalOperator
    : EQUAL
    | NOT_EQUAL
    | LESSER
    | GREATER
    | LESSER_EQUAL
    | GREATER_EQUAL
    ;
