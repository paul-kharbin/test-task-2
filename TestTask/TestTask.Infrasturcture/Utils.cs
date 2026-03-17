using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TestTask.Infrasturcture;

public static class Utils
{
    private static readonly Lazy<XmlNamespaceManager> NamespaceManager = new(() =>
    {
        var ns = new XmlNamespaceManager(new NameTable());
        ns.AddNamespace("xbrli", "http://www.xbrl.org/2003/instance");
        ns.AddNamespace("xbrldi", "http://xbrl.org/2006/xbrldi");

        return ns;
    });

    public static bool Is(this string left, string right)
    {
        return ReferenceEquals(left, right) || (left?.Equals(right, StringComparison.InvariantCultureIgnoreCase) ?? false);
    }

    public static IList<XElement> QueryByXPath<T>(this XDocument xbrlDocuent, string xPathQuery) where T : class
    {
        var result = xbrlDocuent.XPathSelectElements(xPathQuery, NamespaceManager.Value);

        return [.. result];
    }

    public static async Task<XDocument> LoadDocAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Ignore
        };


        using (var xmlReader = XmlReader.Create(filePath, settings))
        {
            XDocument doc = await XDocument.LoadAsync(xmlReader, LoadOptions.None, cancellationToken);
            return doc;
        }
    }

    public static async Task SaveDocAsync(XDocument doc, string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(filePath);

        var settings = new XmlWriterSettings
        {
            Async = true,
            Indent = true,
            Encoding = new UTF8Encoding(false)
        };

        await using (var writer = XmlWriter.Create(filePath, settings))
        {
            await doc.SaveAsync(writer, cancellationToken);
        }
    }
}
