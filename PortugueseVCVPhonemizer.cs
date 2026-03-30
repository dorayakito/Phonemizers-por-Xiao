using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese VCV BRAPA Phonemizer", "PT-BR VCV", "xiao")]
    public class PortugueseVCVPhonemizer : Phonemizer {
        protected USinger singer;

        public override void SetSinger(USinger singer) => this.singer = singer;

        private readonly string[] vowels = { "a", "ao", "ah", "ahn", "ax", "an", "e", "en", "eh", "ehn", "ae", "aen", "i", "in", "i0", "o", "on", "oh", "ohn", "u", "un", "u0", "rh", "y", "w" };
        private readonly string[] consonants = { "b", "bv", "ch", "d", "dj", "f", "g", "gv", "h", "hr", "j", "k", "l", "lh", "l0", "m", "n", "ng", "nh", "p", "r", "rr", "rw", "s", "sh", "t", "v", "x", "z" };

        private static readonly Dictionary<string, string> lyricToPhoneme = new Dictionary<string, string>();

        static PortugueseVCVPhonemizer() {
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
                    if (c == "t" && v.Key == "i") cTarget = "chi";
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
            if (lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric.ToLower();
            }

            string phoneme = lyricToPhoneme.ContainsKey(lyric) ? lyricToPhoneme[lyric] : lyric;
            int tone = notes[0].tone;
            string prevV = string.Empty;

            if (prevNeighbour != null) {
                var prevLyric = prevNeighbour.Value.lyric.ToLower();
                var prevPhoneme = lyricToPhoneme.ContainsKey(prevLyric) ? lyricToPhoneme[prevLyric] : prevLyric;
                foreach (var v in vowels.OrderByDescending(x => x.Length)) {
                    if (prevPhoneme.EndsWith(v)) {
                        prevV = v;
                        break;
                    }
                }
            }

            string alias = string.IsNullOrEmpty(prevV) ? $"- {phoneme}" : $"{prevV} {phoneme}";
            if (singer.TryGetMappedOto(alias, tone, out _)) {
                return new Result { phonemes = new[] { new Phoneme { phoneme = alias } } };
            }

            return new Result { phonemes = new[] { new Phoneme { phoneme = phoneme } } };
        }
    }
}
