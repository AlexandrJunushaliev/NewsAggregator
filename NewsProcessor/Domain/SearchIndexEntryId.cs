using System.Runtime.Serialization;

namespace NewsProcessor.Domain;

[DataContract]
public readonly struct SearchIndexEntryId : IComparable
{
    [DataMember]
    public readonly int Id;

    [DataMember]
    public readonly DateTime DateTime;

    public SearchIndexEntryId(int id, DateTime dateTime)
    {
        Id = id;
        DateTime = dateTime.ToUniversalTime().Date;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not SearchIndexEntryId id)
        {
            throw new ArgumentException($"Wrong parameter type {obj}");
        }

        return id.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public int CompareTo(object? obj)
    {
        if (obj is not SearchIndexEntryId id)
        {
            throw new ArgumentException($"Wrong parameter type {obj}");
        }

        var cmp = DateTime.CompareTo(id.DateTime);
        if (cmp == 0)
            return Id.CompareTo(id.Id);
        return cmp;
    }
}