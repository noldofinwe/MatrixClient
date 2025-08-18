using System.Xml.Linq;

namespace XmppApi.Network.XML.Messages
{
    public interface IXElementable
    {
        XElement toXElement(XNamespace ns);
    }
}
