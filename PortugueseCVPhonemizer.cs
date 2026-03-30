using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese CV BRAPA Phonemizer", "PT-BR CV", "xiao")]
    public class PortugueseCVPhonemizer : Phonemizer {
        protected USinger singer;

        public override void SetSinger(USinger singer) => this.singer = singer;

        private static readonly Dictionary<string, string> lyricToPhoneme = new Dictionary<string, string>();

        static PortugueseCVPhonemizer() {
            var vBase = new Dictionary<string, string> {
                { "a", "a" }, { "á", "a" }, { "à", "a" }, { "â", "ax" }, { "ã", "an" },
                { "e", "e" }, { "é", "eh" }, { "ê", "e" },
                { "i", "i" }, { "í", "i" },
                { "o", "o" }, { "ó", "oh" }, { "ô", "o" }, { "õ", "on" },
                { "u", "u" }, { "ú", "u" }
            };

            foreach (var v in vBase) lyricToPhoneme[v.Key] = v.Value;

            string[] cBase = { "b", "d", "f", "j", "k", "l", "m", "n", "p", "s", "t", "v", "z" };

            foreach (var c in cBase) {
                foreach (var v in vBase) {
                    string cTarget = c;
                    if (c == "d" && v.Key == "i") cTarget = "dj";
                    if (c == "t" && v.Key == "i") cTarget = "ch";
                    lyricToPhoneme[c + v.Key] = cTarget + v.Value;
                }
            }

            foreach (var v in vBase) {
                lyricToPhoneme["r" + v.Key] = "Rr" + v.Value;
                lyricToPhoneme["c" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "s" : "k") + v.Value;
                lyricToPhoneme["g" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "j" : "g") + v.Value;
                lyricToPhoneme["x" + v.Key] = "sh" + v.Value;
                lyricToPhoneme["h" + v.Key] = v.Value;
                lyricToPhoneme["q" + v.Key] = "k" + v.Value;
            }
        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyricToPhoneme.ContainsKey(lyric)) {
                lyric = lyricToPhoneme[lyric];
            }

            int tone = notes[0].tone;
            string startAlias = $"- {lyric}";
            if (prevNeighbour == null && singer.TryGetMappedOto(startAlias, tone, out _)) {
                return new Result {
                    phonemes = new[] { new Phoneme { phoneme = startAlias } }
                };
            }

            return new Result {
                phonemes = new[] { new Phoneme { phoneme = lyric } }
            };
        }
    }
}
