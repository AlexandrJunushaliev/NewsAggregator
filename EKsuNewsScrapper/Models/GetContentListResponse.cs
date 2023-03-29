using System.Xml.Serialization;

namespace EKsuNewsScrapper.Models;

public class GetContentListResponseEntry
{
    [XmlElement("ID")]
    public string? Id { get; set; }

    [XmlElement("upd_date")]
    public string? UpdDate { get; set; }

    [XmlElement("reg_date")]
    public string? RegDate { get; set; }

    [XmlElement("status")]
    public string? Status { get; set; }
}

[XmlRoot(ElementName = "get_content_list")]
public class GetContentListResponse
{
    [XmlElement(ElementName = "get_content")]
    public GetContentListResponseEntry[]? GetContentListEntries { get; set; }
}