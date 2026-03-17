namespace TestTask.Infrastructure.Contract;

public interface IUniqueComparer<in T> : IEqualityComparer<T> where T : class
{
}
