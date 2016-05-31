using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LangUkDotNet
{

    /// <summary>
    /// Ukrainian tokenization script based on
    /// [standard tokenization algorithm] (https://github.com/lang-uk/ner-uk/blob/master/doc/tokenization.md)
    /// 2016 (c) Vsevolod Dyomkin vseloved@gmail.com
    /// </summary>
    public class NerUk
    {
        #region constants
        readonly string[] abbrs = @"ім.
о.
вул.
просп.
бул.
пров.
пл.
г.
р.
див.
п.
с.
м.".Split();

        readonly Regex tokenizationRules = new Regex(@"\w+://(?:[a-zA-Z]|[0-9]|[$-_@.&+])+
|[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+
|[0-9]+-[а-яА-ЯіїІЇ'’`]+
|[+-]?[0-9](?:[0-9,.-]*[0-9])?
|[\w](?:[\w'’`-]?[\w]+)*
|\w.(?:\\w.)+\w?
|[""#$%&*+,/:;<=>@^`~…\\(\\)⟨⟩{}\[\|\]‒–—―«»“”‘’'№]
|[.!?]+
|-+",RegexOptions.Compiled);

        readonly char[] endings = {'.', '!', '?', '…', '»'};
        #endregion

        public string TokenizeText(string input)
        {
            var builder = new StringBuilder();
            foreach (var line in input.Split(new []{Environment.NewLine}, StringSplitOptions.None))
            {
                foreach (var sent in TokenizeSents(line))
                {
                    foreach (var word in TokenizeWords(sent))
                    {
                        builder.Append(word + " ");
                    }
                    builder.Append(Environment.NewLine);
                }
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }

        public void TokenizeStream(StreamReader reader, StreamWriter writer)
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                foreach (var sent in TokenizeSents(line))
                {
                    foreach (var word in TokenizeWords(sent))
                    {
                        writer.Write(word + " ");
                    }
                    writer.Write(Environment.NewLine);
                }
                writer.Write(Environment.NewLine);
            }
        }

        private IEnumerable<string> TokenizeSents(string s)
        {
            var spans = Regex.Matches(s, "[^\\s]+");

            var rez = new List<string>();
            var off = 0;

            for (int i = 0; i < spans.Count; i++)
            {
                var span = spans[i];
                var tok = s.Substring(span.Index, span.Length);
                if (i == spans.Count - 1)
                {
                    var length = span.Index + span.Length - off;
                    var sub = s.Substring(off, length);
                    rez.Add(sub);
                }
                else if (endings.Contains(tok[tok.Length - 1]))
                {
                    var tok1position = Regex.Match(tok, "[.!?…»]");
                    var tok1 = tok.Substring(tok1position.Index);
                    var nextTok = s.Substring(spans[i + 1].Index, spans[i + 1].Length);
                    if (nextTok[0].IsUpper()
                        && !tok1.IsUpper()
                        && !(
                            tok[tok.Length - 1] != '.' ||
                            tok1[0] == '(' || 
                            abbrs.Contains(tok)
                            )
                        )
                    {
                        var length = span.Index + span.Length - off;
                        rez.Add(s.Substring(off, length));
                        off = spans[i + 1].Index + spans[i + 1].Length;
                    }
                }
            }
            return rez;
        }

        private IEnumerable<string> TokenizeWords(string s)
        {
            return tokenizationRules.Matches(s).Cast<Match>().Select(m => m.Value);
        }
    }
}
