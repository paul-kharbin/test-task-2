using System.Globalization;
using System.Xml.Linq;
using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.Infrasturcture.Services;

internal sealed class DefaultXbrlParser : IXbrlSerizalizer
{
    private static readonly XNamespace xbrli = "http://www.xbrl.org/2003/instance";
    private static readonly XNamespace xbrldi = "http://xbrl.org/2006/xbrldi";

    public Instance Deserialize(XDocument xDocument)
    {
        var instance = new Instance();

        ParseContexts(xDocument, instance);
        ParseUnits(xDocument, instance);
        ParseFacts(xDocument, instance);

        return instance;
    }

    public Context DeserializeConext(XElement contextElement)
    {
        var context = new Context
        {
            Id = (string)contextElement.Attribute("id")
        };

        var identifier = contextElement
            .Element(xbrli + "entity")?
            .Element(xbrli + "identifier");

        if (identifier != null)
        {
            context.EntityValue = identifier.Value?.Trim();
            context.EntityScheme = (string)identifier.Attribute("scheme");
        }

        var segment = contextElement.Element(xbrli + "entity")?.Element(xbrli + "segment");

        if (segment != null)
        {
            context.EntitySegment = segment.ToString();
        }

        var period = contextElement.Element(xbrli + "period");
        if (period != null)
        {
            var instant = period.Element(xbrli + "instant");
            if (instant != null)
                context.PeriodInstant = ParseDate(instant.Value);

            var startDate = period.Element(xbrli + "startDate");
            if (startDate != null)
                context.PeriodStartDate = ParseDate(startDate.Value);

            var endDate = period.Element(xbrli + "endDate");
            if (endDate != null)
                context.PeriodEndDate = ParseDate(endDate.Value);

            context.PeriodForever = period.Element(xbrli + "forever") != null;
        }

        var scenarioElement = contextElement.Element(xbrli + "scenario");
        if (scenarioElement != null)
        {
            ParseScenarios(scenarioElement, context);
        }

        return context;
    }

    public XDocument Serialize(Instance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var root = new XElement(xbrli + "xbrl",
            new XAttribute(XNamespace.Xmlns + "xbrli", xbrli),
            new XAttribute(XNamespace.Xmlns + "xbrldi", xbrldi),
            new XAttribute(XNamespace.Xmlns + "purcb-dic", "http://www.cbr.ru/xbrl/nso/purcb/dic")
        );

        foreach (var context in instance.Contexts)
        {
            root.Add(SerializeContext(context));
        }

        foreach (var unit in instance.Units)
        {
            root.Add(SerializeUnit(unit));
        }

        foreach (var fact in instance.Facts)
        {
            root.Add(SerializeFact(fact));
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
    }

    private void ParseContexts(XDocument doc, Instance instance)
    {
        foreach (var contextElement in doc.Descendants(xbrli + "context"))
        {
            var context = DeserializeConext(contextElement);

            instance.Contexts.Add(context);
        }
    }

    private static XElement SerializeContext(Context context)
    {
        var contextElement = new XElement(xbrli + "context");

        if (!string.IsNullOrWhiteSpace(context.Id))
            contextElement.SetAttributeValue("id", context.Id);

        var entity = new XElement(xbrli + "entity");

        var identifier = new XElement(xbrli + "identifier", context.EntityValue ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(context.EntityScheme))
            identifier.SetAttributeValue("scheme", context.EntityScheme);

        entity.Add(identifier);

        if (!string.IsNullOrWhiteSpace(context.EntitySegment))
        {
            try
            {
                entity.Add(XElement.Parse(context.EntitySegment));
            }
            catch
            {
                entity.Add(new XElement(xbrli + "segment", context.EntitySegment));
            }
        }

        contextElement.Add(entity);

        var period = new XElement(xbrli + "period");

        if (context.PeriodInstant.HasValue)
        {
            period.Add(new XElement(xbrli + "instant", FormatDate(context.PeriodInstant.Value)));
        }
        else if (context.PeriodStartDate.HasValue || context.PeriodEndDate.HasValue)
        {
            if (context.PeriodStartDate.HasValue)
                period.Add(new XElement(xbrli + "startDate", FormatDate(context.PeriodStartDate.Value)));

            if (context.PeriodEndDate.HasValue)
                period.Add(new XElement(xbrli + "endDate", FormatDate(context.PeriodEndDate.Value)));
        }
        else if (context.PeriodForever)
        {
            period.Add(new XElement(xbrli + "forever"));
        }

        contextElement.Add(period);

        if (context.Scenarios != null && context.Scenarios.Any())
        {
            var scenario = new XElement(xbrli + "scenario");

            foreach (var s in context.Scenarios)
                scenario.Add(SerializeScenario(s));

            contextElement.Add(scenario);
        }

        return contextElement;
    }

    private static void ParseScenarios(XElement scenarioElement, Context context)
    {
        foreach (var child in scenarioElement.Elements())
        {
            if (child.Name == xbrldi + "explicitMember")
            {
                context.Scenarios.Add(new Scenario
                {
                    DimensionType = "explicitMember",
                    DimensionName = (string)child.Attribute("dimension"),
                    DimensionValue = child.Value?.Trim()
                });

                continue;
            }

            if (child.Name == xbrldi + "typedMember")
            {
                var inner = child.Elements().FirstOrDefault();

                context.Scenarios.Add(new Scenario
                {
                    DimensionType = "typedMember",
                    DimensionName = (string)child.Attribute("dimension"),
                    DimensionCode = inner?.Name.LocalName,
                    DimensionValue = inner?.Value?.Trim()
                });

                continue;
            }

            // fallback (custom elements inside scenario)
            //context.Scenarios.Add(new Scenario
            //{
            //    DimensionType = child.Name.LocalName,
            //    DimensionName = (string)child.Attribute("dimension"),
            //    DimensionCode = child.Name.LocalName,
            //    DimensionValue = child.Value?.Trim()
            //});
        }
    }

    private static XElement SerializeScenario(Scenario scenario)
    {
        if (scenario.DimensionType.Is("explicitMember"))
        {
            var el = new XElement(xbrldi + "explicitMember", scenario.DimensionValue ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(scenario.DimensionName))
                el.SetAttributeValue("dimension", scenario.DimensionName);

            return el;
        }

        if (scenario.DimensionType.Is("typedMember"))
        {
            var el = new XElement(xbrldi + "typedMember");

            if (!string.IsNullOrWhiteSpace(scenario.DimensionName))
                el.SetAttributeValue("dimension", scenario.DimensionName);

            var innerName = !string.IsNullOrWhiteSpace(scenario.DimensionCode)
                ? scenario.DimensionCode
                : "typedValue";

            el.Add(new XElement(innerName, scenario.DimensionValue ?? string.Empty));

            return el;
        }

        // fallback
        var name = !string.IsNullOrWhiteSpace(scenario.DimensionType)
            ? scenario.DimensionType
            : "dimension";

        var fallback = new XElement(name, scenario.DimensionValue ?? string.Empty);

        if (!string.IsNullOrWhiteSpace(scenario.DimensionName))
            fallback.SetAttributeValue("dimension", scenario.DimensionName);

        return fallback;
    }

    private static void ParseUnits(XDocument doc, Instance instance)
    {
        foreach (var unitElement in doc.Descendants(xbrli + "unit"))
        {
            var unit = new Unit
            {
                Id = (string)unitElement.Attribute("id")
            };

            var measures = unitElement.Elements(xbrli + "measure").ToList();
            if (measures.Count != 0)
            {
                unit.Measure = string.Join(";", measures.Select(x => x.Value.Trim()));
            }

            var divide = unitElement.Element(xbrli + "divide");
            if (divide != null)
            {
                var numeratorMeasures = divide
                    .Element(xbrli + "unitNumerator")?
                    .Elements(xbrli + "measure")
                    .Select(x => x.Value.Trim())
                    .ToList();

                if (numeratorMeasures != null && numeratorMeasures.Count != 0)
                    unit.Numerator = string.Join(";", numeratorMeasures);

                var denominatorMeasures = divide
                    .Element(xbrli + "unitDenominator")?
                    .Elements(xbrli + "measure")
                    .Select(x => x.Value.Trim())
                    .ToList();

                if (denominatorMeasures != null && denominatorMeasures.Count != 0)
                    unit.Denominator = string.Join(";", denominatorMeasures);
            }

            instance.Units.Add(unit);
        }
    }

    private static XElement SerializeUnit(Unit unit)
    {
        var unitElement = new XElement(xbrli + "unit");

        if (!string.IsNullOrWhiteSpace(unit.Id))
            unitElement.SetAttributeValue("id", unit.Id);

        if (!string.IsNullOrWhiteSpace(unit.Measure))
        {
            foreach (var measure in Split(unit.Measure))
                unitElement.Add(new XElement(xbrli + "measure", measure));

            return unitElement;
        }

        if (!string.IsNullOrWhiteSpace(unit.Numerator) || !string.IsNullOrWhiteSpace(unit.Denominator))
        {
            var divide = new XElement(xbrli + "divide");

            var numerator = new XElement(xbrli + "unitNumerator");
            foreach (var m in Split(unit.Numerator))
                numerator.Add(new XElement(xbrli + "measure", m));

            var denominator = new XElement(xbrli + "unitDenominator");
            foreach (var m in Split(unit.Denominator))
                denominator.Add(new XElement(xbrli + "measure", m));

            divide.Add(numerator);
            divide.Add(denominator);

            unitElement.Add(divide);
        }

        return unitElement;
    }

    private static void ParseFacts(XDocument doc, Instance instance)
    {
        var contextMap = instance.Contexts
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .ToDictionary(c => c.Id, c => c, StringComparer.Ordinal);

        var unitMap = instance.Units
            .Where(u => !string.IsNullOrWhiteSpace(u.Id))
            .ToDictionary(u => u.Id, u => u, StringComparer.Ordinal);

        // any element with contextRef = fact
        var factElements = doc
            .Descendants()
            .Where(e => e.Attribute("contextRef") != null);

        foreach (var factElement in factElements)
        {
            var fact = new Fact
            {
                Id = (string)factElement.Attribute("id"),
                Name = factElement.Name.LocalName,
                ContextRef = (string)factElement.Attribute("contextRef"),
                UnitRef = (string)factElement.Attribute("unitRef"),
                Decimals = ParseNullableInt((string)factElement.Attribute("decimals")),
                Precision = ParseNullableInt((string)factElement.Attribute("precision")),
                Value = factElement.Value?.Trim()
            };

            if (!string.IsNullOrWhiteSpace(fact.ContextRef) && contextMap.TryGetValue(fact.ContextRef, out var context))
            {
                fact.Context = context;
            }

            if (!string.IsNullOrWhiteSpace(fact.UnitRef) && unitMap.TryGetValue(fact.UnitRef, out var unit))
            {
                fact.Unit = unit;
            }

            instance.Facts.Add(fact);
        }
    }

    private static XElement SerializeFact(Fact fact)
    {
        var elementName = !string.IsNullOrWhiteSpace(fact.Name)
            ? fact.Name
            : "fact";

        var xml = $"<{elementName}></{elementName}>";
        var factElement = XElement.Parse(xml);

        factElement.Value = fact.Value ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(fact.Id))
            factElement.SetAttributeValue("id", fact.Id);

        var contextRef = fact.ContextRef ?? fact.Context?.Id;
        var unitRef = fact.UnitRef ?? fact.Unit?.Id;

        if (!string.IsNullOrWhiteSpace(contextRef))
            factElement.SetAttributeValue("contextRef", contextRef);

        if (!string.IsNullOrWhiteSpace(unitRef))
            factElement.SetAttributeValue("unitRef", unitRef);

        if (fact.Decimals.HasValue)
            factElement.SetAttributeValue("decimals", fact.Decimals.Value.ToString(CultureInfo.InvariantCulture));

        if (fact.Precision.HasValue)
            factElement.SetAttributeValue("precision", fact.Precision.Value.ToString(CultureInfo.InvariantCulture));

        return factElement;
    }

    private static string[] Split(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static DateTime? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTime.TryParse(value, out var dt) ? dt : null;
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static int? ParseNullableInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (string.Equals(value, "INF", StringComparison.OrdinalIgnoreCase))
            return null;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }
}