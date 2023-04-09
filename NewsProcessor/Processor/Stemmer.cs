using System.Text.RegularExpressions;

namespace NewsProcessor.Processor;

public static class Stemmer
{
    private static Regex _perfectiveground = new("((ив|ивши|ившись|ыв|ывши|ывшись)|((<;=[ая])(в|вши|вшись)))$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _reflexive = new("(с[яь])$");

    private static Regex _adjective =
        new("(ее|ие|ые|ое|ими|ыми|ей|ий|ый|ой|ем|им|ым|ом|его|ого|ему|ому|их|ых|ую|юю|ая|яя|ою|ею)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _participle = new("((ивш|ывш|ующ)|((?<=[ая])(ем|нн|вш|ющ|щ)))$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _verb =
        new(
            "((ила|ыла|ена|ейте|уйте|ите|или|ыли|ей|уй|ил|ыл|им|ым|ен|ило|ыло|ено|ят|ует|уют|ит|ыт|ены|ить|ыть|ишь|ую|ю)|((?<=[ая])(ла|на|ете|йте|ли|й|л|ем|н|ло|но|ет|ют|ны|ть|ешь|нно)))$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _noun =
        new(
            "(а|ев|ов|ие|ье|е|иями|ями|ами|еи|ии|и|ией|ей|ой|ий|й|иям|ям|ием|ем|ам|ом|о|у|ах|иях|ях|ы|ь|ию|ью|ю|ия|ья|я)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _rvre = new("^(.*?[аеиоуыэюя])(.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _derivational = new(".*[^аеиоуыэюя]+[аеиоуыэюя].*ость?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _der = new("ость?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _superlative = new("(ейше|ейш)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _ = new("и$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _p = new("ь$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Regex _nn = new("нн$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static string GetStemmed(string word)
    {
        word = word.Replace('ё', 'е');
        var m = _rvre.Matches(word);
        if (m.Count > 0)
        {
            var match = m[0];
            var groupCollection = match.Groups;
            var pre = groupCollection[1].ToString();
            var rv = groupCollection[2].ToString();

            var temp = _perfectiveground.Matches(rv);
            var stringTemp = ReplaceFirst(temp, rv);


            if (stringTemp.Equals(rv))
            {
                var @tempRV = _reflexive.Matches(rv);
                rv = ReplaceFirst(@tempRV, rv);
                temp = _adjective.Matches(rv);
                stringTemp = ReplaceFirst(temp, rv);
                if (!stringTemp.Equals(rv))
                {
                    rv = stringTemp;
                    @tempRV = _participle.Matches(rv);
                    rv = ReplaceFirst(@tempRV, rv);
                }
                else
                {
                    temp = _verb.Matches(rv);
                    stringTemp = ReplaceFirst(temp, rv);
                    if (stringTemp.Equals(rv))
                    {
                        @tempRV = _noun.Matches(rv);
                        rv = ReplaceFirst(@tempRV, rv);
                    }
                    else
                    {
                        rv = stringTemp;
                    }
                }
            }
            else
            {
                rv = stringTemp;
            }

            var tempRv = _.Matches(rv);
            rv = ReplaceFirst(tempRv, rv);
            if (_derivational.Matches(rv).Count > 0)
            {
                tempRv = _der.Matches(rv);
                rv = ReplaceFirst(tempRv, rv);
            }

            temp = _p.Matches(rv);
            stringTemp = ReplaceFirst(temp, rv);
            if (stringTemp.Equals(rv))
            {
                tempRv = _superlative.Matches(rv);
                rv = ReplaceFirst(tempRv, rv);
                tempRv = _nn.Matches(rv);
                rv = ReplaceFirst(tempRv, rv);
            }
            else
            {
                rv = stringTemp;
            }

            word = pre + rv;
        }
        return word;
    }

    public static string ReplaceFirst(MatchCollection collection, string part)
    {
        string stringTemp;
        if (collection.Count == 0)
        {
            return part;
        }
        /*else if(collection.Count == 1) 
        { 
        return StringTemp; 
        }*/
        else
        {
            stringTemp = part;
            for (var i = 0; i < collection.Count; i++)
            {
                var groupCollection = collection[i].Groups;
                if (stringTemp.Contains(groupCollection[i].ToString()))
                {
                    var deletePart = groupCollection[i].ToString();
                    stringTemp = stringTemp.Replace(deletePart, string.Empty);
                }
            }
        }

        return stringTemp;
    }
}