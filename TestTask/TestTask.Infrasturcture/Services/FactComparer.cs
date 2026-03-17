using System.Diagnostics.CodeAnalysis;
using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.Infrasturcture.Services;

internal sealed class FactComparer(IContextComparer contextComparer) : IFactComparer
{
    public bool Equals(Fact? left, Fact? right)
    {
        var result = ReferenceEquals(left, right)
            ||
            (
                left is not null && right is not null
                &&
                // Факты являются идентифицируются по содержанию контекста(см.описание уникальности)
                // и имени ветки значения(например purcb-dic:Kod_Okato3)
                left.Name.Is(right.Name)
                &&
                /*
                    Если я правильно понял, то если контексты уникальны,
                    то и факты с одинаковым именем ссылающиеся на эти контексты тоже уникальны,
                    вне зависимости от оатсльный аттрибутов и значений
                */
                contextComparer.Equals(left.Context, right.Context)
                //&&
                // остальные параметы...
            );

        return result;
    }

    public int GetHashCode([DisallowNull] Fact obj)
    {
        return 0;
    }
}
