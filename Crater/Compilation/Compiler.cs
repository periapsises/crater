using System.Runtime.CompilerServices;
using Crater.SyntaxTree;

namespace Crater.Compilation;

public class Compiler
{
    private readonly LuaWriter _writer;

    private Compiler()
    {
        _writer = new LuaWriter();
    }

    public static string Compile(Program program)
    {
        var compiler = new Compiler();
        compiler.CompileProgram(program);

        return compiler._writer.ToString();
    }

    private void CompileProgram(Program program)
    {
        CompileBlock(program.block);
    }

    private void CompileBlock(Block block)
    {
        foreach (var statement in block.statements)
        {
            switch (statement)
            {
                case VariableDeclaration variableDeclaration:
                    CompileVariableDeclaration(variableDeclaration);
                    break;
                case FunctionDeclaration functionDeclaration:
                    CompileFunctionDeclaration(functionDeclaration);
                    break;
                case DoStatement doStatement:
                    CompileDoStatement(doStatement);
                    break;
                case IfStatement ifStatement:
                    CompileIfStatement(ifStatement);
                    break;
                case FunctionCall functionCall:
                    CompileFunctionCall(functionCall);
                    _writer.WriteLine();
                    break;
                case Assignment assignment:
                    CompileAssignment(assignment);
                    break;
                case WhileLoop whileLoop:
                    CompileWhileLoop(whileLoop);
                    break;
                case RepeatLoop repeatLoop:
                    CompileRepeatLoop(repeatLoop);
                    break;
                case NumericForLoop numericForLoop:
                    CompileNumericForLoop(numericForLoop);
                    break;
                case GenericForLoop genericForLoop:
                    CompileGenericForLoop(genericForLoop);
                    break;
                case ReturnStatement returnStatement:
                    CompileReturnStatement(returnStatement);
                    break;
                case BreakStatement breakStatement:
                    CompileBreakStatement(breakStatement);
                    break;
                default:
                    throw new SwitchExpressionException(statement);
            }
        }
    }

    private void CompileVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        if (variableDeclaration.local)
            _writer.Write("local ");

        var declaratorCount = variableDeclaration.declarators.Count;
        for (var i = 0; i < declaratorCount; i++)
        {
            _writer.Write(variableDeclaration.declarators[i].name);
            if (i < declaratorCount - 1)
                _writer.Write(", ");
        }

        var initializerCount = variableDeclaration.initializers.Count;
        if (initializerCount == 0)
        {
            _writer.WriteLine();
            return;
        }

        _writer.Write(" = ");

        for (var i = 0; i < initializerCount; i++)
        {
            CompileExpression(variableDeclaration.initializers[i]);
            if (i < initializerCount - 1)
                _writer.Write(", ");
        }

        _writer.WriteLine();
    }

    private void CompileFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        if (functionDeclaration.local)
            _writer.Write("local ");

        _writer.Write($"function {functionDeclaration.name}(");

        var parameterCount = functionDeclaration.parameters.Count;
        for (var i = 0; i < parameterCount; i++)
        {
            _writer.Write(functionDeclaration.parameters[i].name);
            if (i < parameterCount - 1)
                _writer.Write(", ");
        }

        if (functionDeclaration.varargParameter is not null)
        {
            if (parameterCount > 0)
                _writer.Write(", ");

            _writer.Write("...");
        }

        _writer.WriteLine(")");

        _writer.Indent();
        CompileBlock(functionDeclaration.block);
        _writer.Outdent();

        _writer.WriteLine("end");
    }

    private void CompileDoStatement(DoStatement doStatement)
    {
        _writer.WriteLine("do");
        _writer.Indent();
        CompileBlock(doStatement.block);
        _writer.Outdent();
        _writer.WriteLine("end");
    }

    private void CompileIfStatement(IfStatement ifStatement)
    {
        _writer.Write("if ");
        CompileExpression(ifStatement.condition);
        _writer.WriteLine(" then");

        _writer.Indent();
        CompileBlock(ifStatement.block);
        _writer.Outdent();

        foreach (var elseIfStatement in ifStatement.elseIfStatements)
        {
            _writer.Write("elseif ");
            CompileExpression(elseIfStatement.condition);
            _writer.WriteLine(" then");

            _writer.Indent();
            CompileBlock(elseIfStatement.block);
            _writer.Outdent();
        }

        if (ifStatement.elseStatement is not null)
        {
            _writer.WriteLine("else");
            _writer.Indent();
            CompileBlock(ifStatement.elseStatement.block);
            _writer.Outdent();
        }

        _writer.WriteLine("end");
    }

    private void CompileAssignment(Assignment assignment)
    {
        var variableCount = assignment.variables.Count;
        for (var i = 0; i < variableCount; i++)
        {
            _writer.Write(assignment.variables[i]);
            if (i < variableCount - 1)
                _writer.Write(", ");
        }

        var valueCount = assignment.values.Count;
        if (valueCount == 0)
            return;

        _writer.Write(" = ");

        for (var i = 0; i < valueCount; i++)
        {
            CompileExpression(assignment.values[i]);
            if (i < valueCount - 1)
                _writer.Write(", ");
        }

        _writer.WriteLine();
    }

    private void CompileWhileLoop(WhileLoop whileLoop)
    {
        _writer.Write("while ");
        CompileExpression(whileLoop.condition);
        _writer.WriteLine(" do");

        _writer.Indent();
        CompileBlock(whileLoop.block);
        _writer.Outdent();

        _writer.WriteLine("end");
    }

    private void CompileRepeatLoop(RepeatLoop repeatLoop)
    {
        _writer.WriteLine("repeat");

        _writer.Indent();
        CompileBlock(repeatLoop.block);
        _writer.Outdent();

        _writer.Write("until ");
        CompileExpression(repeatLoop.condition);
        _writer.WriteLine();
    }

    private void CompileNumericForLoop(NumericForLoop numericForLoop)
    {
        _writer.Write($"for {numericForLoop.variable} = ");
        CompileExpression(numericForLoop.initializer);
        _writer.Write(", ");
        CompileExpression(numericForLoop.limit);

        if (numericForLoop.increment is not null)
        {
            _writer.Write(", ");
            CompileExpression(numericForLoop.increment);
        }

        _writer.WriteLine(" do");

        _writer.Indent();
        CompileBlock(numericForLoop.block);
        _writer.Outdent();

        _writer.WriteLine("end");
    }

    private void CompileGenericForLoop(GenericForLoop genericForLoop)
    {
        _writer.Write("for ");

        var declaratorCount = genericForLoop.declarators.Count;
        for (var i = 0; i < declaratorCount; i++)
        {
            _writer.Write(genericForLoop.declarators[i].name);
            if (i < declaratorCount - 1)
                _writer.Write(", ");
        }

        _writer.Write(" in ");
        CompileExpression(genericForLoop.expression);
        _writer.WriteLine(" do");

        _writer.Indent();
        CompileBlock(genericForLoop.block);
        _writer.Outdent();

        _writer.WriteLine("end");
    }

    private void CompileReturnStatement(ReturnStatement returnStatement)
    {
        var returnCount = returnStatement.returnValues.Count;
        if (returnCount == 0)
        {
            _writer.WriteLine("return");
            return;
        }

        _writer.Write("return ");

        for (var i = 0; i < returnCount; i++)
        {
            CompileExpression(returnStatement.returnValues[i]);
            if (i < returnCount - 1)
                _writer.Write(", ");
        }

        _writer.WriteLine();
    }

    private void CompileBreakStatement(BreakStatement breakStatement)
    {
        _writer.WriteLine("break");
    }

    private void CompileExpression(Expression expression)
    {
        switch (expression)
        {
            case UnaryOperation unaryOperation:
                CompileUnaryOperation(unaryOperation);
                break;
            case BinaryOperation binaryOperation:
                CompileBinaryOperation(binaryOperation);
                break;
            case NumberLiteral numberLiteral:
                CompileNumberLiteral(numberLiteral);
                break;
            case StringLiteral stringLiteral:
                CompileStringLiteral(stringLiteral);
                break;
            case BooleanLiteral booleanLiteral:
                CompileBooleanLiteral(booleanLiteral);
                break;
            case TableLiteral tableLiteral:
                CompileTableLiteral(tableLiteral);
                break;
            case ArrayLiteral arrayLiteral:
                CompileArrayLiteral(arrayLiteral);
                break;
            case NilLiteral nilLiteral:
                CompileNilLiteral(nilLiteral);
                break;
            case ParenthesizedExpression parenthesizedExpression:
                CompileParenthesizedExpression(parenthesizedExpression);
                break;
            case VariableReference variableReference:
                CompileVariableReference(variableReference);
                break;
            case FunctionCall functionCall:
                CompileFunctionCall(functionCall);
                break;
            case BracketIndexing bracketIndexing:
                CompileBracketIndexing(bracketIndexing);
                break;
            case DotIndexing dotIndexing:
                CompileDotIndexing(dotIndexing);
                break;
            default:
                throw new SwitchExpressionException(expression);
        }
    }

    private void CompileUnaryOperation(UnaryOperation unaryOperation)
    {
        _writer.Write(unaryOperation.op);
        CompileExpression(unaryOperation.expression);
    }

    private void CompileBinaryOperation(BinaryOperation binaryOperation)
    {
        CompileExpression(binaryOperation.left);
        _writer.Write($" {binaryOperation.op} ");
        CompileExpression(binaryOperation.right);
    }

    private void CompileNumberLiteral(NumberLiteral numberLiteral)
    {
        _writer.Write(numberLiteral.value);
    }

    private void CompileStringLiteral(StringLiteral stringLiteral)
    {
        _writer.Write(stringLiteral.value);
    }

    private void CompileBooleanLiteral(BooleanLiteral booleanLiteral)
    {
        _writer.Write(booleanLiteral.value);
    }

    private void CompileTableLiteral(TableLiteral tableLiteral)
    {
        _writer.Write("{");

        for (var i = 0; i < tableLiteral.values.Count; i++)
        {
            var value = tableLiteral.values[i];

            _writer.Write($"{value.index} = ");
            CompileExpression(value.value);

            if (i < tableLiteral.values.Count - 1)
                _writer.Write(", ");
        }

        _writer.Write("}");
    }

    private void CompileArrayLiteral(ArrayLiteral arrayLiteral)
    {
        _writer.Write("{");

        for (var i = 0; i < arrayLiteral.values.Count; i++)
        {
            CompileExpression(arrayLiteral.values[i]);

            if (i < arrayLiteral.values.Count - 1)
                _writer.Write(", ");
        }

        _writer.Write("}");
    }

    private void CompileNilLiteral(NilLiteral nilLiteral)
    {
        _writer.Write(nilLiteral.value);
    }

    private void CompileParenthesizedExpression(ParenthesizedExpression parenthesizedExpression)
    {
        _writer.Write("(");
        CompileExpression(parenthesizedExpression.innerExpression);
        _writer.Write(")");
    }

    private void CompileVariableReference(VariableReference variableReference)
    {
        _writer.Write(variableReference.name);
    }

    private void CompileFunctionCall(FunctionCall functionCall)
    {
        CompileExpression(functionCall.function);
        _writer.Write("(");

        var argumentCount = functionCall.arguments.Count;
        for (var i = 0; i < argumentCount; i++)
        {
            CompileExpression(functionCall.arguments[i]);
            if (i < argumentCount - 1)
                _writer.Write(", ");
        }

        _writer.Write(")");
    }

    private void CompileBracketIndexing(BracketIndexing bracketIndexing)
    {
        CompileExpression(bracketIndexing.prefix);
        _writer.Write("[");
        CompileExpression(bracketIndexing.index);
        _writer.Write("]");
    }

    private void CompileDotIndexing(DotIndexing dotIndexing)
    {
        CompileExpression(dotIndexing.prefix);
        _writer.Write($".{dotIndexing.key}");
    }
}
