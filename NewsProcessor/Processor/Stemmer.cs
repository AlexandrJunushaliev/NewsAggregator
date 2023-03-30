using System.Text.RegularExpressions;

namespace NewsProcessor.Processor;

public static class ChatgptStem
{
    public class RussianSnowballStemmer
    {
        private static readonly Regex perfectiveGerundRegex = new Regex("(ив(ши(й|е|х|ми?)?)?|ыв(ши(й|е|х|ми?)?)?)$");
        private static readonly Regex reflexiveRegex = new Regex("(ся|сь)$");
        private static readonly Regex adjectiveRegex = new Regex("(ее|ие|ые|ое|ими|ыми|ей|ий|ый|ой|ем|им|ым|ом|его|ого|ему|ому|их|ых|ую|юю|ая|яя|ою|ею)?$");
        private static readonly Regex participleRegex = new Regex("(ивш(и(й|е|х|ми?)?)?|ывш(и(й|е|х|ми?)?)?|ующ(и(й|е|х|ми?)?)?)$");
        private static readonly Regex verbRegex = new Regex("(ла|на|ете|йте|ли|й|л|ем|н(о|а|ы)?|ло|ет(е)?|ют|ны|ть(ся)?|ешь)?$");
        private static readonly Regex nounRegex = new Regex("(а|ев|ов|ье|иями|ями|ами|еи|ии|и|ей|ой|ий|й(о|е|а)?|иям|ям|ием|ем|ам|ом|о(в|й|м)?|у|ах|иях|ях|ы)?$");
        private static readonly Regex superlativeRegex = new Regex("(ейш(е|ий|ая|ую|ем|им|их|ую|ые|ое|ие|ый)?)?$");
        private static readonly Regex derivationalRegex = new Regex("(ост(ь)?|ость)?$");

        public static string Stem(string word)
        {
            word = word.ToLower();
            word = perfectiveGerundRegex.Replace(word, "");
            word = reflexiveRegex.Replace(word, "");
            word = adjectiveRegex.Replace(word, "");
            word = participleRegex.Replace(word, "");
            word = verbRegex.Replace(word, "");
            word = nounRegex.Replace(word, "");
            word = superlativeRegex.Replace(word, "");
            word = derivationalRegex.Replace(word, "");

            return word;
        }
    }

}
public static class Stemmer
{

    private static Regex _perfectiveground = new("((ив|ивши|ившись|ыв|ывши|ывшись)|((<;=[ая])(в|вши|вшись)))$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _reflexive = new("(с[яь])$");

    private static Regex _adjective = new("(ее|ие|ые|ое|ими|ыми|ей|ий|ый|ой|ем|им|ым|ом|его|ого|ему|ому|их|ых|ую|юю|ая|яя|ою|ею)$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _participle = new("((ивш|ывш|ующ)|((?<=[ая])(ем|нн|вш|ющ|щ)))$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _verb = new("((ила|ыла|ена|ейте|уйте|ите|или|ыли|ей|уй|ил|ыл|им|ым|ен|ило|ыло|ено|ят|ует|уют|ит|ыт|ены|ить|ыть|ишь|ую|ю)|((?<=[ая])(ла|на|ете|йте|ли|й|л|ем|н|ло|но|ет|ют|ны|ть|ешь|нно)))$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _noun = new("(а|ев|ов|ие|ье|е|иями|ями|ами|еи|ии|и|ией|ей|ой|ий|й|иям|ям|ием|ем|ам|ом|о|у|ах|иях|ях|ы|ь|ию|ью|ю|ия|ья|я)$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _rvre = new("^(.*?[аеиоуыэюя])(.*)$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _derivational = new(".*[^аеиоуыэюя]+[аеиоуыэюя].*ость?$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _der = new("ость?$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _superlative = new("(ейше|ейш)$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

    private static Regex _ = new("и$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
    private static Regex _p = new("ь$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
    private static Regex _nn = new("нн$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

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