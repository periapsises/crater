parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: block EOF;

block: statement*;

statement
    : variableDeclaration
    | doStatement
    | ifStatement
    | assignment
    ;

variableDeclaration: LOCAL? variableDeclarator (COMMA variableDeclarator)* (ASSIGN expressionList)?;

variableDeclarator: name=IDENTIFIER COLON typeName;

doStatement: DO block END;

ifStatement: IF expression THEN block (elseIfStatement)* elseStatement END;

elseIfStatement: ELSEIF expression THEN block;

elseStatement: ELSE block;

assignment: IDENTIFIER ASSIGN expression;

typeName: IDENTIFIER QMARK?;

expressionList: expression (COMMA expression)*;

expression
    : IDENTIFIER                                                # VariableReference
    | op=MINUS expression                                       # UnaryExpression
    | left=expression operator=(STAR | SLASH) right=expression  # MultiplicativeOperation
    | left=expression operator=(PLUS | MINUS) right=expression  # AdditiveOperation
    | NUMBER                                                    # NumberLiteral
    | STRING                                                    # StringLiteral
    | BOOLEAN                                                   # BooleanLiteral
    | NIL                                                       # NilLiteral
    ;
