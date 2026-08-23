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
                case DoStatement doStatement:
                    CompileDoStatement(doStatement);
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
            return;

        _writer.Write(" = ");

        for (var i = 0; i < initializerCount; i++)
        {
            CompileExpression(variableDeclaration.initializers[i]);
            if (i < initializerCount - 1)
                _writer.Write(", ");
        }

        _writer.WriteLine();
    }

    private void CompileDoStatement(DoStatement doStatement)
    {
        _writer.WriteLine("do");
        _writer.Indent();
        CompileBlock(doStatement.block);
        _writer.Outdent();
        _writer.WriteLine("end");
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
            case Literal literal:
                CompileLiteral(literal);
                break;
            case VariableReference variableReference:
                CompileVariableReference(variableReference);
                break;
            case FunctionCall functionCall:
                CompileFunctionCall(functionCall);
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

    private void CompileLiteral(Literal literal)
    {
        _writer.Write(literal.value);
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
}
