parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: block EOF;

block: statement*;

statement
    : variableDeclaration
    | doStatement
    | assignment
    ;

variableDeclaration: LOCAL? variableDeclarator (COMMA variableDeclarator)* (ASSIGN expressionList)?;

variableDeclarator: name=IDENTIFIER COLON typeName;

doStatement: DO block END;

assignment: IDENTIFIER ASSIGN expression;

typeName: IDENTIFIER QMARK?;

expressionList: expression (COMMA expression)*;

expression
    : op=MINUS expression                                       # UnaryExpression
    | left=expression operator=(STAR | SLASH) right=expression  # MultiplicativeOperation
    | left=expression operator=(PLUS | MINUS) right=expression  # AdditiveOperation
    | NUMBER                                                    # NumberLiteral
    | STRING                                                    # StringLiteral
    | BOOLEAN                                                   # BooleanLiteral
    | NIL                                                       # NilLiteral
    ;
