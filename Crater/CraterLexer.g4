lexer grammar CraterLexer;

LOCAL: 'local';

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

COLON: ':';

WHITESPACE: (' ' | '\t' | '\n' | '\r')+ -> channel(HIDDEN);
