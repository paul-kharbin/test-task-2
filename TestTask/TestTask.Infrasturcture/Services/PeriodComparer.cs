using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.Infrasturcture.Services;

internal sealed class PeriodComparer : IPeriodComparer
{
    public bool Equals(Context? left, Context? right)
    {
        var result = ReferenceEquals(left, right)
            ||
            (
                left is not null && right is not null
                &&
                (
                    left.PeriodInstant == right.PeriodInstant
                    ||
                    left.PeriodForever && right.PeriodForever
                    ||
                    (
                        left.PeriodStartDate.HasValue && right.PeriodStartDate.HasValue
                        &&
                        left.PeriodStartDate == right.PeriodStartDate
                        &&
                        left.PeriodEndDate == right.PeriodEndDate
                    )
                )
            );

        return result;
    }

    public int GetHashCode(Context obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return obj.GetHashCode();
    }
}
