using Microsoft.Extensions.DependencyInjection;
using TestTask.Infrasturcture.Contract;
using TestTask.Infrasturcture.Services;

namespace TestTask.Infrasturcture.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection serviceColletion)
    {
        serviceColletion
                .AddSingleton<IXbrlSerizalizer, DefaultXbrlParser>()
                .AddSingleton<IContextComparer, ContextComparer>()
                .AddSingleton<IEntityComparer, EntityComparer>()
                .AddSingleton<IFactComparer, FactComparer>()
                .AddSingleton<IUnitComparer, UnitComparer>()
                .AddSingleton<IPeriodComparer, PeriodComparer>()
                .AddSingleton<IScenarioComparer, ScenarioComparer>();

        return serviceColletion;
    }
}
