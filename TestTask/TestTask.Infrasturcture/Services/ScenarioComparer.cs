using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.Infrasturcture.Services;

internal sealed class ScenarioComparer : IScenarioComparer
{
    public bool Equals(Scenario? left, Scenario? right)
    {
        return ReferenceEquals(left, right)
            ||
            (
                left is not null && right is not null
                &&
                left.DimensionType.Is(right.DimensionType)
                &&
                left.DimensionName.Is(right.DimensionName)
                &&
                left.DimensionValue.Is(right.DimensionValue)
                &&
                left.DimensionCode.Is(right.DimensionCode)
            );
    }

    public int GetHashCode(Scenario obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return 0;
    }
}
