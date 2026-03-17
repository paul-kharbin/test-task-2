using TestTask.Infrasturcture;
using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.App;

public sealed class XbrlProcessor(
    IContextComparer contextComparer,
    IUnitComparer unitComparer,
    IFactComparer factComparer)
{
    public IList<Context[]> GetDuplicatesContexts(Contexts contexts)
    {
        ArgumentNullException.ThrowIfNull(contexts);

        var doubles = contexts.GroupBy(c => c, contextComparer).Where(c => c.Count() > 1);
        var result = doubles.Select(g => g.ToArray());

        return [.. result];
    }

    public Instance Merge(params Instance[] instances)
    {
        var distinctContexts = instances.SelectMany(i => i.Contexts).Distinct(contextComparer);
        var distinctUnits = instances.SelectMany(i => i.Units).Distinct(unitComparer);
        var distinctFacts = instances.SelectMany(i => i.Facts).Distinct(factComparer);

        var merge = new Instance
        {
            Contexts = [.. distinctContexts],
            Units = [.. distinctUnits],
            Facts = [.. distinctFacts]
        };

        return merge;
    }

    public Diff<Fact> Diff(Instance left, Instance right)
    {
        var all = new List<Instance> { left, right }.SelectMany(i => i.Facts);

        var distinctFacts = all.Distinct(factComparer);
        var different = all.Except(distinctFacts, factComparer);
        var missingLeft = right.Facts.Except(left.Facts, factComparer);
        var missingRight = left.Facts.Except(right.Facts, factComparer);

        return new Diff<Fact>
        {
            Equal = [.. distinctFacts],
            Different = [.. different],
            MissingLeft = [.. missingLeft],
            MissingRight = [.. missingRight],
        };
    }
}