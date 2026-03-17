using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.Infrasturcture.Services;

internal class EntityComparer : IEntityComparer
{
    public bool Equals(Context? left, Context? right)
    {
        return ReferenceEquals(left, right)
            ||
            (
                left is not null && right is not null
                &&
                left.EntitySegment.Is(right.EntitySegment)
                &&
                left.EntityScheme.Is(right.EntityScheme)
                &&
                left.EntityValue.Is(right.EntityValue)
            );
    }

    public int GetHashCode(Context obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return obj.GetHashCode();
    }
}
