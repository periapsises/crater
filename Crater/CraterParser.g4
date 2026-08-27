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
    | returnStatement
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

assignment: IDENTIFIER (COMMA IDENTIFIER)? ASSIGN expressionList;

whileLoop: WHILE condition=expression DO block END;

returnStatement: RETURN expressionList?;

typeName: typeName (QMARK | LSQRBRACKET RSQRBRACKET) | primaryType;

primaryType: IDENTIFIER;

expressionList: expression (COMMA expression)*;

expression
    : primaryExpression                                         # BaseExpression
    | op=(MINUS | NOT) expression                               # UnaryExpression
    | left=expression operator=(STAR | SLASH) right=expression  # MultiplicativeOperation
    | left=expression operator=(PLUS | MINUS) right=expression  # AdditiveOperation
    | left=expression logicalOperator right=expression          # LogicalOperation
    | left=expression AND right=expression                      # AndOperation
    | left=expression OR right=expression                       # OrOperation
    | LBRACKET expressionList? RBRACKET                         # ArrayLiteral
    | NUMBER                                                    # NumberLiteral
    | STRING                                                    # StringLiteral
    | (TRUE | FALSE)                                            # BooleanLiteral
    | NIL                                                       # NilLiteral
    ;

primaryExpression: prefixExpression postfixExpression*;

prefixExpression: IDENTIFIER # VariableReference;

postfixExpression
    : postfixFunctionCall
    | postfixBracketIndexing
    ;

postfixFunctionCall: LPAREN expressionList? RPAREN;

postfixBracketIndexing: LSQRBRACKET expression RSQRBRACKET;

logicalOperator
    : EQUAL
    | NOT_EQUAL
    | LESSER
    | GREATER
    | LESSER_EQUAL
    | GREATER_EQUAL
    ;
