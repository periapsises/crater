parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: variableDeclaration+ EOF;

variableDeclaration: name=IDENTIFIER COLON type=IDENTIFIER;
