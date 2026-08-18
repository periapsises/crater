parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: variableDeclaration+ EOF;

variableDeclaration: LOCAL? name=IDENTIFIER COLON type=IDENTIFIER;
