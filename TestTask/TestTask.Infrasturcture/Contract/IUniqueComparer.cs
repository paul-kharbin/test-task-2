namespace TestTask.Infrasturcture.Contract;

public interface IUniqueComparer<in T> : IEqualityComparer<T> where T : class
{
}
