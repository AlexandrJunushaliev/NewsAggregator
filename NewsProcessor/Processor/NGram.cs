namespace NewsProcessor.Processor;

public enum NGramRange
{
    One,
    Two,
    Three
}

public class NGram
{
    public static bool FindAsNGrams(IEnumerable<string> words, string[] wordToFind, int range)
    {
        return range switch
        {
            1 => FindAsOneGram(words, wordToFind[0]),
            2 => FindAsBiGram(words, (wordToFind[0], wordToFind[1])),
            3 => FindAsTriGram(words, (wordToFind[0], wordToFind[1], wordToFind[2])),
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
        };
    }

    private static bool FindAsTriGram(IEnumerable<string> words, (string, string, string) wordToFind)
    {
        string? fst = null;
        string? snd = null;
        foreach (var word in words)
        {
            if (fst is null)
            {
                fst = word;
                continue;
            }

            if (snd is null)
            {
                snd = word;
                continue;
            }

            if (string.Equals(fst, wordToFind.Item1, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(snd, wordToFind.Item2, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(word, wordToFind.Item3, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            fst = snd;
            snd = word;
        }

        return false;
    }

    private static bool FindAsBiGram(IEnumerable<string> words, (string, string) wordToFind)
    {
        string? fst = null;
        foreach (var word in words)
        {
            if (fst is null)
            {
                fst = word;
                continue;
            }

            if (string.Equals(fst, wordToFind.Item1, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(word, wordToFind.Item2, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            fst = word;
        }

        return false;
    }

    private static bool FindAsOneGram(IEnumerable<string> words, string wordToFind)
    {
        return words.Any(word => string.Equals(word, wordToFind, StringComparison.InvariantCultureIgnoreCase));
    }
}