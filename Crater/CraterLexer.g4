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
WHILE   : 'while';
REPEAT  : 'repeat';
UNTIL   : 'until';
FOR     : 'for';
IN      : 'in';
BREAK   : 'break';

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
FUN     : 'fun';

// ==================================================
// Operators & Punctuation
// ==================================================

// Multi-Character
EQUAL           : '==';
NOT_EQUAL       : '~=';
LESSER_EQUAL    : '<=';
GREATER_EQUAL   : '>=';

CONCAT  : '..';

VARARGS : '...';

// Single-Character
PLUS    : '+';
MINUS   : '-';
STAR    : '*';
SLASH   : '/';

LESSER  : '<';
GREATER : '>';

DOT     : '.';
COMMA   : ',';
COLON   : ':';
ASSIGN  : '=';
QMARK   : '?';

LPAREN      : '(';
RPAREN      : ')';
LBRACKET    : '{';
RBRACKET    : '}';
LSQRBRACKET : '[';
RSQRBRACKET : ']';

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

COMMENT: '--' ~[\r\n]* -> channel(HIDDEN);

SHEBANG: '#!' ~[\r\n]*;
