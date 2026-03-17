using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.Infrasturcture.Services;

internal sealed class ContextComparer(
        IEntityComparer entityComparer,
        IPeriodComparer periodComparer,
        IScenarioComparer scenarioComparer)
    : IContextComparer
{
    public bool Equals(Context? left, Context? right)
    {
        var result = ReferenceEquals(left, right)
                ||
                (
                    left is not null && right is not null
                    &&
                    entityComparer.Equals(left, right)
                    &&
                    periodComparer.Equals(left, right)
                    &&
                    left.Scenarios.Count == right.Scenarios.Count
                    &&
                    left.Scenarios.Intersect(right.Scenarios, scenarioComparer).Count() == left.Scenarios.Count
                );

        return result;
    }

    public int GetHashCode(Context obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return 0;
    }
}
