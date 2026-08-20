parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: block EOF;

block: statement*;

statement
    : variableDeclaration
    | doStatement
    ;

variableDeclaration: LOCAL? name=IDENTIFIER COLON typeName (ASSIGN expression)?;

doStatement: DO block END;

typeName: IDENTIFIER QMARK?;

expression: literal;

literal
    : NUMBER  #NumberLiteral
    | STRING  #StringLiteral
    | BOOLEAN #BooleanLiteral
    | NIL     #NilLiteral
    ;
