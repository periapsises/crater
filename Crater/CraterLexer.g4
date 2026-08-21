lexer grammar CraterLexer;

DO: 'do';
END: 'end';
LOCAL: 'local';

NUMBER: Integer Decimal?;

fragment Integer: [1-9][0-9]* | '0';
fragment Decimal: '.' [0-9]+;

STRING: '"' (EscapeSequence | ~('\\' | '"'))* '"';

fragment EscapeSequence
    : '\\' [abfnrtvz"'|$#\\]
    | '\\' '\r'? '\n'
    | '\\' Integer
    | '\\x' [a-fA-F0-9]+
    | '\\u{' [a-fA-F0-9]+ '}'
    ;

BOOLEAN: 'true' | 'false';

NIL: 'nil';

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

COLON: ':';
ASSIGN: '=';
QMARK: '?';
PLUS: '+';
MINUS: '-';
STAR: '*';
SLASH: '/';

WHITESPACE: (' ' | '\t' | '\n' | '\r')+ -> channel(HIDDEN);
