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
    /// 2016 (c) Vsevolod Dyomkin vseloved@gmail.com, Dmitry Chaplinsky chaplinsky.dmitry@gmail.com
    /// </summary>
    public class TokenizeUk
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
м.".Trim().Split();

        readonly Regex tokenizationRules = new Regex(@"
[\w\u0301]+://(?:[a-zA-Z]|[0-9]|[$-_@.&+])+
|[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+
|[0-9]+-[а-яА-ЯіїІЇ'’`\u0301]+
|[+-]?[0-9](?:[0-9,.-]*[0-9])?
|[\w\u0301](?:[\w'’`-\u0301]?[\w\u0301]+)*
|[\w\u0301].(?:[\w\u0301].)+[\w\u0301]?
|[""#$%&*+,/:;<=>@^`~…()⟨⟩{}\[\|\]‒–—―«»“”‘’'№]
|[.!?]+
|-+", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        readonly char[] endings = {'.', '!', '?', '…', '»'};
        #endregion

        /// <summary>
        /// Tokenize input text to paragraphs, sentences and words.
        ///
        /// Tokenization to paragraphs is done using simple Newline algorithm
        /// For sentences and words tokenizers above are used
        /// </summary>
        /// <param name="input">Text to tokenize</param>
        /// <returns>Text, tokenized into paragraphs, sentences and words</returns>
        public List<List<List<string>>> TokenizeText(string input)
        {
            return input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => TokenizeSentences(line)
                    .Select(sent => TokenizeWords(sent).ToList())
                    .ToList())
                .ToList();
        }

        /// <summary>
        /// Tokenize input stream to paragraphs, sentences and words and writes it in other stream
        ///
        /// Tokenization to paragraphs is done using simple Newline algorithm
        /// For sentences and words tokenizers above are used
        /// </summary>
        /// <param name="reader">Reader of input stream</param>
        /// <param name="writer">Writer of output stream</param>
        public void TokenizeStream(StreamReader reader, StreamWriter writer)
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                foreach (var sent in TokenizeSentences(line))
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

        /// <summary>
        /// Tokenize input text to sentences.
        /// </summary>
        /// <param name="s">Text to tokenize</param>
        /// <returns>sentences</returns>
        public IEnumerable<string> TokenizeSentences(string s)
        {
            var spans = Regex.Matches(s, @"[^\s]+");

            var off = 0;

            for (int i = 0; i < spans.Count; i++)
            {
                var span = spans[i];
                var tok = span.Value;
                if (i == spans.Count - 1)
                {
                    var length = span.Index + span.Length - off;
                    yield return s.Substring(off, length);
                }
                else if (endings.Contains(tok[tok.Length - 1]))
                {
                    var tok1 = tok.Length > 1 ? tok[tok.Length - 2] : default(char);
                    var nextTok = spans[i + 1].Value;
                    if (nextTok[0].IsUpper()
                        && !tok1.IsUpper()
                        && !(tok[tok.Length - 1] != '.' ||
                            tok1 == '(' || 
                            abbrs.Contains(tok)))
                    {
                        var length = span.Index + span.Length - off;
                        yield return s.Substring(off, length);
                        off = spans[i + 1].Index;
                    }
                }
            }
        }

        /// <summary>
        /// Tokenize input text to words.
        /// </summary>
        /// <param name="s">Text to tokenize</param>
        /// <returns>words</returns>
        public IEnumerable<string> TokenizeWords(string s)
        {
            return tokenizationRules.Matches(s).Cast<Match>().Select(m => m.Value);
        }
    }
}
