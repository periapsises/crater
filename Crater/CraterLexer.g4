lexer grammar CraterLexer;

// ==================================================
// Keywords
// ==================================================

// Control Flow
IF      : 'if';
THEN    : 'then';
ELSEIF  : 'elseif';
ELSE    : 'else';
DO      : 'do';
END     : 'end';
RETURN  : 'return';

// Declarations & Scoping
LOCAL   : 'local';
FUNCTION: 'function';

// Logical Operators
AND     : 'and';
OR      : 'or';
NOT     : 'not';

// Literal Keywords
TRUE    : 'true';
FALSE   : 'false';
NIL     : 'nil';
VOID    : 'void';

// ==================================================
// Operators & Punctuation
// ==================================================

// Multi-Character

// Single-Character
PLUS    : '+';
MINUS   : '-';
STAR    : '*';
SLASH   : '/';

COMMA   : ',';
COLON   : ':';
ASSIGN  : '=';
QMARK   : '?';

LPAREN  : '(';
RPAREN  : ')';

// ==================================================
// Identifiers
// ==================================================

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

// ==================================================
// Dynamic Pattern Literals
// ==================================================

NUMBER: Integer Decimal?;

fragment Integer: [1-9][0-9]* | '0';
fragment Decimal: '.' [0-9]+;

STRING: '"' (EscapeSequence | ~('\\' | '"'))* '"';

fragment EscapeSequence
    : '\\' [abfnrtvz"'\\]
    | '\\' '\r'? '\n'
    | '\\' [0-9] [0-9]? [0-9]?
    | '\\x' [a-fA-F0-9]+
    | '\\u{' [a-fA-F0-9]+ '}'
    ;

// ==================================================
// Whitespace & Comments
// ==================================================

WHITESPACE: (' ' | '\t' | '\n' | '\r')+ -> channel(HIDDEN);
