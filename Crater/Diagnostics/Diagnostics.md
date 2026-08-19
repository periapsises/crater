# Diagnostics

Diagnostics are the error, warning and informational messages that the compiler or language server generates when it
encounters problematic behavior in the source code.

## Diagnostic categories

Each diagnostic can fall in one of six categories. The first number in the error code is the category id.

| Code Range     | Category Name      | Severity  | Description                                         |
|----------------|--------------------|-----------|-----------------------------------------------------|
| `CRA`*`0`*`xx` | Internal Errors    | **Fatal** | Unexpected states, bugs in the compiler.            |
| `CRA`*`1`*`xx` | Syntax Errors      | **Error** | Tokenization and parsing failures, malformed code.  |
| `CRA`*`2`*`xx` | Type Errors        | **Error** | Type mismatches, inference failures, invalid casts. |
| `CRA`*`3`*`xx` | Name Resolution    | **Error** | Undefined variables, duplicate definitions          |
| `CRA`*`4`*`xx` | Semantic Warnings  | **Warn**  | Unused variables, unreachable code.                 |
| `CRA`*`5`*`xx` | Linter Information | **Info**  | Non-critical suggestions for cleaner code.          |
