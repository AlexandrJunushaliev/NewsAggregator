using System.Runtime.Serialization;

namespace NewsProcessor.Domain;

[DataContract]
public class NewsMessageEntry
{
    [DataMember(EmitDefaultValue = false)]
    public string Id { get; set; } = null!;

    [DataMember(EmitDefaultValue = false)]
    public string Title { get; set; } = null!;

    [DataMember(EmitDefaultValue = false)]
    public string? Header { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Text { get; set; } = null!;

    [DataMember(EmitDefaultValue = false)]
    public string[]? Images { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string? MainImage { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string SourceUrl { get; set; } = null!;
    
    [DataMember(EmitDefaultValue = false)]
    public string SourceName { get; set; } = null!;

    [DataMember(EmitDefaultValue = false)]
    public string NewsUrl { get; set; } = null!;

    [DataMember(EmitDefaultValue = false)]
    public string[]? VideoUrls { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string? UpdDate { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string RegDate { get; set; } = null!;
}