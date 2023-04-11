namespace NewsProcessor.Domain;

public class SearchIndexSortedSet : SortedSet<SearchIndexEntryId>
{
    public SearchIndexSortedSet() : base(SearchIndexEntryIdEqualityComparer.Default)
    {
    }
}

public class SearchIndexEntryIdEqualityComparer : IComparer<SearchIndexEntryId>
{
    public int Compare(SearchIndexEntryId x, SearchIndexEntryId y)
    {
        return x.CompareTo(y) switch
        {
            -1 => 1, 0 => 0, 1 => -1,
            _ => throw new ArgumentException("x CompareTo y gives not 1, -1 or 0")
        };
    }

    public static readonly SearchIndexEntryIdEqualityComparer Default = new ();
}