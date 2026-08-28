namespace Crater.SemanticAnalysis.Types;

public class TableType(IReadOnlyDictionary<string, Type> fields, Type baseType) : Type("table", baseType)
{
    public readonly IReadOnlyDictionary<string, Type> Fields = fields;

    public override bool CanHold(Type other)
    {
        if (other is not TableType)
            return false;

        return IsSameType(other);
    }

    public override bool IsSameType(Type other)
    {
        if (other is not TableType otherTable)
            return false;

        if (Fields.Count != otherTable.Fields.Count)
            return false;

        foreach (var (key, type) in Fields)
        {
            if (!otherTable.Fields.TryGetValue(key, out var fieldType))
                return false;

            if (!type.IsSameType(fieldType))
                return false;
        }

        return true;
    }
}
