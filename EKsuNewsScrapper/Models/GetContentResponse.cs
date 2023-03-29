using System.Xml.Serialization;

namespace EKsuNewsScrapper.Models;

[XmlRoot(ElementName = "post")]
public class GetContentResponse
{
    [XmlElement("ID")]
    public string? Id { get; set; }

    [XmlElement("title")]
    public string? Title { get; set; }

    [XmlElement("HEADER")]
    public string? Header { get; set; }

    [XmlElement("text")]
    public string? Text { get; set; }

    [XmlElement("images")]
    public Images? Images { get; set; }

    [XmlElement("picture")]
    public string? Picture { get; set; }

    [XmlElement("m_url")]
    public string? UrlOfDepartment { get; set; }

    [XmlElement("c_url")]
    public string? UrlOfPost { get; set; }
    
    [XmlElement("video_url")]
    public string[]? VideoUrl { get; set; }

    [XmlElement("upd_date")]
    public string? UpdDate { get; set; }

    [XmlElement("reg_date")]
    public string? RegDate { get; set; }

    [XmlElement("status")]
    public string? Status { get; set; }
}

public class Images
{
    [XmlElement(ElementName = "image")]
    public string[]? ImagesUrls { get; set; }
}