parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: block EOF;

block: statement*;

statement
    : variableDeclaration
    | functionDeclaration
    | doStatement
    | ifStatement
    | assignment
    | returnStatement
    ;

variableDeclaration: LOCAL? variableDeclarator (COMMA variableDeclarator)* (ASSIGN expressionList)?;

variableDeclarator: name=IDENTIFIER COLON typeName;

functionDeclaration: LOCAL? FUNCTION name=IDENTIFIER LPAREN parameters? RPAREN COLON returnTypes block END;

parameters: parameter (COMMA parameter)*;

parameter: name=IDENTIFIER COLON typeName;

returnTypes: VOID | typeName (COMMA typeName)*;

doStatement: DO block END;

ifStatement: IF expression THEN block (elseIfStatement)* elseStatement END;

elseIfStatement: ELSEIF expression THEN block;

elseStatement: ELSE block;

assignment: IDENTIFIER (COMMA IDENTIFIER)? ASSIGN expressionList;

returnStatement: RETURN expressionList?;

typeName: IDENTIFIER QMARK?;

expressionList: expression (COMMA expression)*;

expression
    : primaryExpression                                         # BaseExpression
    | op=(MINUS | NOT) expression                               # UnaryExpression
    | left=expression operator=(STAR | SLASH) right=expression  # MultiplicativeOperation
    | left=expression operator=(PLUS | MINUS) right=expression  # AdditiveOperation
    | left=expression AND right=expression                      # AndOperation
    | left=expression OR right=expression                       # OrOperation
    | NUMBER                                                    # NumberLiteral
    | STRING                                                    # StringLiteral
    | (TRUE | FALSE)                                            # BooleanLiteral
    | NIL                                                       # NilLiteral
    ;

primaryExpression: prefixExpression postfixExpression*;

prefixExpression: IDENTIFIER # VariableReference;

postfixExpression: postfixFunctionCall;

postfixFunctionCall: LPAREN expressionList? RPAREN;
