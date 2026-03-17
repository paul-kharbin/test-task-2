using TestTask.Infrasturcture;
using TestTask.Infrasturcture.Contract;
using TestTask.Model;

namespace TestTask.App;

internal sealed class Application(IXbrlSerizalizer xbrlParser, XbrlProcessor xbrlProcessor)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var xDoc1 = await Utils.LoadDocAsync("./data/report1.xbrl", cancellationToken);
        var xDoc2 = await Utils.LoadDocAsync("./data/report2.xbrl", cancellationToken);

        var instance1 = xbrlParser.Deserialize(xDoc1);
        var instance2 = xbrlParser.Deserialize(xDoc2);

        /*
            1. Найти в файле report1.xbrl повторяющиеся контексты.
            Контекст считается уникальным, если у него уникальны набор значений объекта отчета entity,
            периода period (включая тип) и сценариев (включая имена).

            Идентификаторы контекстов (id) не учитываются при сравнении.
            Грубо говоря, должно быть полностью уникально содержащихся веток XML.
            Порядок самих веток в контексте не важен.
        */
        var duplicates = xbrlProcessor.GetDuplicatesContexts(instance1.Contexts);

        /*
            2. Объединить данные файлов report1.xbrl и report2.xbrl. 
            На выходе получить новый объединенный отчет(xbrl) с объединными списками уникальных контекстов context,
            уникальных единиц измерений unit и значений(фактов).
        */
        var merge = xbrlProcessor.Merge(instance1, instance2);
        var mergeDoc = xbrlParser.Serialize(merge);

        await Utils.SaveDocAsync(mergeDoc, "./data/merge.xbrl", cancellationToken);

        /*
            3. Сравнить данных файлов report1.xbrl и report2.xbrl,
            выявить различающихся фактов(отсутствующие или имеющие разные значения).
            Факты являются идентифицируются по содержанию контекста(см.описание уникальности)
            и имени ветки значения(например purcb-dic:Kod_Okato3). Идентификаторы фактов(id) не учитываются при сравнении. 
        */
        var factsDiff = xbrlProcessor.Diff(instance1, instance2);

        /*
            Написать запросы XPath для получения:
        */
        // контексты с периодом xbrli:period/xbrli:instant, равным "2019-04-30";
        var els1 = mergeDoc.QueryByXPath<Context>("//xbrli:context[xbrli:period/xbrli:instant='2019-04-30']");
        var contexts1 = els1.Select(xbrlParser.DeserializeConext).ToList();

        // контексты со сценарием, использующим измерение dimension = "dim-int:ID_sobstv_CZBTaxis";
        var els2 = mergeDoc.QueryByXPath<Context>("//xbrli:context[xbrli:scenario/*[@dimension = 'dim-int:ID_sobstv_CZBTaxis']]");
        var contexts2 = els2.Select(xbrlParser.DeserializeConext).ToList();

        // контексты без сценария
        var els3 = mergeDoc.QueryByXPath<Context>("//xbrli:context[not(xbrli:scenario)]");
        var contexts3 = els3.Select(xbrlParser.DeserializeConext).ToList();
    }
}
