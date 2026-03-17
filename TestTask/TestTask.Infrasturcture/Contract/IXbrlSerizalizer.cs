using System.Xml.Linq;
using TestTask.Model;

namespace TestTask.Infrasturcture.Contract;

public interface IXbrlSerizalizer
{
    Instance Deserialize(XDocument xDocument);
    Context DeserializeConext(XElement xElement);
    XDocument Serialize(Instance instance);
}
