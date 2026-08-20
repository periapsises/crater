parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: block EOF;

block: statement*;

statement
    : variableDeclaration
    | doStatement
    ;

variableDeclaration: LOCAL? name=IDENTIFIER COLON type=IDENTIFIER (ASSIGN expression)?;

doStatement: DO block END;

expression: literal;

literal
    : NUMBER  #NumberLiteral
    | STRING  #StringLiteral
    | BOOLEAN #BooleanLiteral
    | NIL     #NilLiteral
    ;
