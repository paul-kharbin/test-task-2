namespace TestTask.Infrasturcture;

public class Diff<T> where T : class
{
    public IList<T> Equal { get; init; } = [];
    public IList<T> MissingLeft { get; init; } = [];
    public IList<T> MissingRight { get; init; } = [];
    public IList<T> Different { get; init; } = [];
}
