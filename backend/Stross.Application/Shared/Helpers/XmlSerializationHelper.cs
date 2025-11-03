using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.SubsonicModels;

namespace Stross.Application.Shared.Helpers;

internal sealed class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

public static class XmlSerializationHelper
{
    private static readonly XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces();
    private static readonly XmlSerializer ResponseSerializer = new XmlSerializer(typeof(Response));

    static XmlSerializationHelper()
    {
        Namespaces.Add(string.Empty, "http://subsonic.org/restapi");
    }

    public static string SerializeSubsonicResponse(ISubsonicResponse subsonicResponse)
    {
        Response response = subsonicResponse.Response;

        XmlWriterSettings settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
            NamespaceHandling = NamespaceHandling.OmitDuplicates
        };

        using Utf8StringWriter stringWriter = new Utf8StringWriter();
        using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings);

        ResponseSerializer.Serialize(xmlWriter, response, Namespaces);

        return stringWriter.ToString();
    }
}