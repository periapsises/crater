# Crater

The strongly typed language that compiles directly to Lua.

## Generating ANTLR parser

To generate the lexer and parser from the grammar files, you'll need the antlr4 executable and a valid java installation.  
Make sure the target language is `CSharp` and that the output directory is under `Crater/Antlr`.  
The listener is can be omited (`-no-listener`) but the visitor is required.

Here is what those commands should look like:

```
java -jar antlr4.jar -Dlanguage=CSharp -package Crater.Antlr -message-format antlr -no-listener -visitor -o Crater/Antlr -Xexact-output-dir ./Crater/CraterLexer.g4
java -jar antlr4.jar -Dlanguage=CSharp -package Crater.Antlr -message-format antlr -no-listener -visitor -o Crater/Antlr -Xexact-output-dir ./Crater/CraterParser.g4
```

After running this you should see the following under `Crater/Antlr`:  
- CraterLexer.cs
- CraterLexer.interp
- CraterLexer.tokens
- CraterParser.cs
- CraterParser.interp
- CraterParser.tokens
- CraterParserBaseVisitor.cs
- CraterParserVisitor.cs

The project will not build without those files!
