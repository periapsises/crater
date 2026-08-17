lexer grammar CraterLexer;

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

WHITESPACE: (' ' | '\t' | '\n' | '\r')+ -> channel(HIDDEN);
