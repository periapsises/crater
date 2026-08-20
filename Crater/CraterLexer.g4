lexer grammar CraterLexer;

DO: 'do';
END: 'end';
LOCAL: 'local';

NUMBER: Integer Decimal?;

fragment Integer: [1-9][0-9]* | '0';
fragment Decimal: '.' [0-9]+;

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

COLON: ':';

WHITESPACE: (' ' | '\t' | '\n' | '\r')+ -> channel(HIDDEN);
