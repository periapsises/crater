parser grammar CraterParser;

options { tokenVocab = CraterLexer; }

program: IDENTIFIER+ EOF;
