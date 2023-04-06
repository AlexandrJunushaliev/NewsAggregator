using System.Runtime.Serialization;

namespace NewsProcessor.Domain;

[DataContract]
public readonly struct SearchIndexEntry
{
    [DataMember]
    public readonly SearchIndexEntryId Id;

    public SearchIndexEntry(int id, DateTime dateTime)
    {
        Id = new SearchIndexEntryId(id, dateTime);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not SearchIndexEntry s)
            return false;
        return Id.Id.Equals(s.Id.Id);
    }

    public override int GetHashCode()
    {
        return Id.Id.GetHashCode();
    }
}

