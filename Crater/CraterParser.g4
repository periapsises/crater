parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: block EOF;

block: statement*;

statement
    : variableDeclaration
    | doStatement
    ;

variableDeclaration: LOCAL? name=IDENTIFIER COLON type=IDENTIFIER;

doStatement: DO block END;
