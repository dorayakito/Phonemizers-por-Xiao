using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese IPEA Phonemizer", "PT-BR IPEA", "ly ft. xiao")]
    public class IPEAPhonemizer : Phonemizer {

        private readonly string[] vowels = {
            "Ao", "am", "em", "im", "om", "um",
            "6im", "eim", "oim", "uim",
            "Ei", "Oi", "ai", "ei", "oi", "ui",
            "Eu", "Ou", "au", "eu", "iu", "ou",
            "E", "O", "6",
            "a", "e", "i", "o", "u"
        };

        private readonly string[] consonants = {
            "tch", "rr", "rw", "lh", "nh", "dj", "qu",
            "ch", "b", "d", "f", "g", "j", "k", "l",
            "m", "n", "p", "r", "s", "t", "v", "w",
            "x", "y", "z"
        };

        private static readonly Dictionary<string, string> g2p = new Dictionary<string, string>();

        static IPEAPhonemizer() {
            var vBase = new Dictionary<string, string> {
                { "a", "a" }, { "á", "a" }, { "à", "a" }, { "â", "6" }, { "ã", "am" },
                { "e", "e" }, { "é", "E" }, { "ê", "e" },
                { "i", "i" }, { "í", "i" },
                { "o", "o" }, { "ó", "O" }, { "ô", "o" }, { "õ", "om" },
                { "u", "u" }, { "ú", "u" },
                { "ai", "ai" }, { "ei", "ei" }, { "oi", "oi" }, { "ui", "ui" },
                { "éi", "Ei" }, { "ói", "Oi" },
                { "au", "au" }, { "eu", "eu" }, { "iu", "iu" }, { "ou", "ou" },
                { "éu", "Eu" }, { "ól", "Ou" },
                { "ão", "Ao" }, { "6im", "6im" }, { "eim", "eim" }, { "oim", "oim" }, { "uim", "uim" }
            };

            foreach (var v in vBase) g2p[v.Key] = v.Value;

            string[] cBase = { "b", "d", "f", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "z", "ch", "nh", "lh", "rr", "rw", "qu", "dj", "tch", "x", "y" };

            foreach (var c in cBase) {
                string cTarget = c;
                if (c == "x") cTarget = "ch";
                foreach (var v in vBase) {
                    g2p[c + v.Key] = cTarget + " " + v.Value;
                }
            }

            foreach (var v in vBase) {
                g2p["c" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "s" : "k") + " " + v.Value;
                g2p["g" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "j" : "g") + " " + v.Value;
                g2p["ç" + v.Key] = "s " + v.Value;
                g2p["h" + v.Key] = v.Value;
            }
        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (notes[0].lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric.ToLower();
            }

            string phoneme = g2p.ContainsKey(lyric) ? g2p[lyric] : lyric;
            string c = string.Empty;
            string v = phoneme;

            foreach (var con in consonants.OrderByDescending(x => x.Length)) {
                var prefix = con + " ";
                if (phoneme.StartsWith(prefix)) {
                    c = con;
                    v = phoneme.Substring(prefix.Length);
                    break;
                }
            }

            var phonemes = new List<Phoneme>();
            int tone = notes[0].tone;

            if (prevNeighbour == null) {
                var startAlias = $"- {(string.IsNullOrEmpty(c) ? v : c + " " + v)}";
                if (!singer.TryGetMappedOto(startAlias, tone, out _)) {
                    startAlias = string.IsNullOrEmpty(c) ? v : $"{c} {v}";
                }
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

                var cvAlias = string.IsNullOrEmpty(c) ? v : $"{c} {v}";
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
