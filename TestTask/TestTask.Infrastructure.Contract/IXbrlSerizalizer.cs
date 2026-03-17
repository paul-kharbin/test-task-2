using System.Xml.Linq;
using TestTask.Model;

namespace TestTask.Infrastructure.Contract;

public interface IXbrlSerizalizer
{
    Instance Deserialize(XDocument xDocument);
    Context DeserializeConext(XElement xElement);
    XDocument Serialize(Instance instance);
}
