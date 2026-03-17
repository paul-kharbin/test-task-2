using System.Diagnostics.CodeAnalysis;
using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.Infrasturcture.Services;

internal sealed class UnitComparer : IUnitComparer
{
    public bool Equals(Unit? left, Unit? right)
    {
        var result = ReferenceEquals(left, right)
            ||
            (
                left is not null && right is not null
                &&
                left.Measure.Is(right.Measure)
                &&
                left.Numerator.Is(right.Numerator)
            );

        return result;
    }

    public int GetHashCode([DisallowNull] Unit obj)
    {
        return 0;
    }
}
