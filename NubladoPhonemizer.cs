using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese NUBLADO Phonemizer", "PT-BR NUBLADO", "xiao")]
    public class NubladoPhonemizer : Phonemizer {
        protected USinger singer;

        public override void SetSinger(USinger singer) => this.singer = singer;

        private readonly string[] vowels = { "a", "e", "i", "o", "u", "3", "0", "a]", "e]", "i]", "o]", "u]" };
        private readonly string[] consonants = { "w", "r", "t", "y", "p", "s", "d", "f", "g", "h", "j", "k", "l", "z", "x", "v", "b", "n", "m", "ch", "Dd" };

        private static readonly Dictionary<string, string> g2p = new Dictionary<string, string>();

        static NubladoPhonemizer() {
            var vBase = new Dictionary<string, string> {
                { "a", "a" }, { "á", "a" }, { "à", "a" }, { "ã", "a]" }, { "â", "a]" },
                { "e", "e" }, { "é", "3" }, { "ê", "e" },
                { "i", "i" }, { "í", "i" },
                { "o", "o" }, { "ó", "0" }, { "ô", "o" }, { "õ", "o]" },
                { "u", "u" }, { "ú", "u" }
            };

            foreach (var v in vBase) g2p[v.Key] = v.Value;

            string[] cBase = { "b", "d", "f", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "z" };

            foreach (var c in cBase) {
                foreach (var v in vBase) {
                    string cTarget = c;
                    if (c == "d" && v.Key == "i") cTarget = "Dd";
                    if (c == "t" && v.Key == "i") cTarget = "ch";
                    g2p[c + v.Key] = cTarget + v.Value;
                }
            }

            foreach (var v in vBase) {
                g2p["c" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "s" : "k") + v.Value;
                g2p["g" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "j" : "g") + v.Value;
                g2p["x" + v.Key] = "sh" + v.Value;
                g2p["ch" + v.Key] = "sh" + v.Value;
                g2p["h" + v.Key] = v.Value;
            }
        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric.ToLower();
            }

            string phoneme = g2p.ContainsKey(lyric) ? g2p[lyric] : lyric;

            if (lyric.EndsWith("ão")) phoneme = phoneme.Replace("a]", "awn");
            if (lyric.EndsWith("om")) phoneme = phoneme.Replace("o]", "own");
            if (lyric.EndsWith("em")) phoneme = phoneme.Replace("e]", "ewn");

            string c = string.Empty;
            string v = phoneme;

            foreach (var con in consonants.OrderByDescending(x => x.Length)) {
                if (phoneme.StartsWith(con)) {
                    c = con;
                    v = phoneme.Substring(con.Length);
                    break;
                }
            }

            var phonemes = new List<Phoneme>();
            int tone = notes[0].tone;

            if (prevNeighbour == null) {
                var startAlias = $"(- {c}{v})";
                if (!singer.TryGetMappedOto(startAlias, tone, out _)) startAlias = $"({c}{v})";
                phonemes.Add(new Phoneme { phoneme = startAlias });
            } else {
                var prevLyric = prevNeighbour.Value.lyric.ToLower();
                var prevPhoneme = g2p.ContainsKey(prevLyric) ? g2p[prevLyric] : prevLyric;
                
                string prevV = string.Empty;
                foreach (var vv in vowels.OrderByDescending(x => x.Length)) {
                    if (prevPhoneme.EndsWith(vv)) {
                        prevV = vv;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(c)) {
                    var vcAlias = $"({prevV}{c})";
                    if (singer.TryGetMappedOto(vcAlias, tone, out var oto)) {
                        phonemes.Add(new Phoneme { phoneme = vcAlias, position = -oto.Preutterance });
                    }
                }

                var cvAlias = $"({c}{v})";
                if (singer.TryGetMappedOto(cvAlias, tone, out _)) {
                    phonemes.Add(new Phoneme { phoneme = cvAlias });
                } else {
                    phonemes.Add(new Phoneme { phoneme = $"({v})" });
                }
            }

            if (nextNeighbour == null && lyric.EndsWith("r")) {
                var rFinal = $"({v}R)";
                if (singer.TryGetMappedOto(rFinal, tone, out _)) {
                    phonemes.Add(new Phoneme { phoneme = rFinal });
                }
            }

            return new Result { phonemes = phonemes.ToArray() };
        }
    }
}
