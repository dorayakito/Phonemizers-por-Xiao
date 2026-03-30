using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese F-SAMPA Phonemizer", "PT-BR F-SAMPA", "xiao")]
    public class FSampaPhonemizer : Phonemizer {
        private readonly string[] vowels = { "a", "e", "i", "o", "u", "@", "7", "1", "0", "Q", "X", "V", "y", "w" };
        private readonly string[] consonants = { "b", "ch", "d", "dZ", "f", "g", "h", "j", "k", "l", "lh", "m", "n", "nh", "p", "r", "rh", "s", "sh", "t", "v", "z" };

        private static readonly Dictionary<string, string> g2p = new Dictionary<string, string>();

        static FSampaPhonemizer() {
            var vBase = new Dictionary<string, string> {
                { "a", "a" }, { "á", "a" }, { "à", "a" }, { "ã", "@" }, { "â", "a" },
                { "e", "e" }, { "é", "7" }, { "ê", "e" },
                { "i", "i" }, { "í", "i" },
                { "o", "o" }, { "ó", "0" }, { "ô", "o" }, { "õ", "Q" },
                { "u", "u" }, { "ú", "u" }
            };

            foreach (var v in vBase) g2p[v.Key] = v.Value;

            string[] cBase = { "b", "d", "f", "k", "l", "m", "n", "p", "s", "t", "v", "z" };

            foreach (var c in cBase) {
                foreach (var v in vBase) {
                    string cTarget = c;
                    if (c == "d" && v.Key == "i") cTarget = "dZ";
                    if (c == "t" && v.Key == "i") cTarget = "ch";
                    g2p[c + v.Key] = cTarget + v.Value;
                }
            }

            foreach (var v in vBase) {
                g2p["r" + v.Key] = "rh" + v.Value;
                g2p["c" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "s" : "k") + v.Value;
                g2p["g" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "j" : "g") + v.Value;
                g2p["nh" + v.Key] = "nh" + v.Value;
                g2p["lh" + v.Key] = "lh" + v.Value;
                g2p["ch" + v.Key] = "sh" + v.Value;
                g2p["x" + v.Key] = "sh" + v.Value;
                g2p["j" + v.Key] = "j" + v.Value;
                g2p["h" + v.Key] = v.Value;
            }
        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric.ToLower();
            }

            string phoneme = g2p.ContainsKey(lyric) ? g2p[lyric] : lyric;
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
                var startAlias = $"- {c}{v}";
                if (!singer.TryGetMappedOto(startAlias, tone, out _)) startAlias = c + v;
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
                    var vcAlias = $"{prevV} {c}";
                    if (singer.TryGetMappedOto(vcAlias, tone, out var oto)) {
                        phonemes.Add(new Phoneme { phoneme = vcAlias, position = -oto.Preutterance });
                    }
                }

                var cvAlias = c + v;
                if (singer.TryGetMappedOto(cvAlias, tone, out _)) {
                    phonemes.Add(new Phoneme { phoneme = cvAlias });
                } else {
                    phonemes.Add(new Phoneme { phoneme = v });
                }
            }

            return new Result { phonemes = phonemes.ToArray() };
        }
    }
}
