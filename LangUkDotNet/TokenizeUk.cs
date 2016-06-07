﻿using System;
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
            return input.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(line => TokenizeSents(line)
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

        /// <summary>
        /// Tokenize input text to sentences.
        /// </summary>
        /// <param name="s">Text to tokenize</param>
        /// <returns>sentences</returns>
        public IEnumerable<string> TokenizeSents(string s)
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