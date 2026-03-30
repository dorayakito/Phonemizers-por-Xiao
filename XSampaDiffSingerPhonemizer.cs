using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese X-SAMPA DiffSinger Phonemizer", "DIFF XS", "xiao")]
    public class XSampaDiffSingerPhonemizer : Phonemizer {
        private static readonly Dictionary<string, string> g2p = new Dictionary<string, string>();

        static XSampaDiffSingerPhonemizer() {
            var vBase = new Dictionary<string, string> {
                { "a", "a" }, { "á", "a" }, { "à", "a" }, { "ã", "a~" }, { "â", "6" },
                { "e", "e" }, { "é", "E" }, { "ê", "e" },
                { "i", "i" }, { "í", "i" },
                { "o", "o" }, { "ó", "O" }, { "ô", "o" }, { "õ", "o~" },
                { "u", "u" }, { "ú", "u" }
            };

            foreach (var v in vBase) g2p[v.Key] = v.Value;

            string[] cBase = { "b", "d", "f", "k", "l", "m", "n", "p", "s", "t", "v", "z" };

            foreach (var c in cBase) {
                foreach (var v in vBase) {
                    string cTarget = c;
                    if (c == "d" && v.Key == "i") cTarget = "dZ";
                    if (c == "t" && v.Key == "i") cTarget = "tS";
                    g2p[c + v.Key] = cTarget + " " + v.Value;
                }
            }

            foreach (var v in vBase) {
                g2p["r" + v.Key] = "R " + v.Value;
                g2p["c" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "s" : "k") + " " + v.Value;
                g2p["g" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "Z" : "g") + " " + v.Value;
                g2p["nh" + v.Key] = "J " + v.Value;
                g2p["lh" + v.Key] = "L " + v.Value;
                g2p["ch" + v.Key] = "S " + v.Value;
                g2p["x" + v.Key] = "S " + v.Value;
                g2p["j" + v.Key] = "Z " + v.Value;
                g2p["h" + v.Key] = v.Value;
            }
        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyric == "+" && prevNeighbour != null) {
                return new Result { phonemes = new Phoneme[0] };
            }

            string phonemeStr = g2p.ContainsKey(lyric) ? g2p[lyric] : lyric;
            var phonemeList = phonemeStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            return new Result { 
                phonemes = phonemeList.Select(p => new Phoneme { phoneme = p }).ToArray() 
            };
        }
    }
}
