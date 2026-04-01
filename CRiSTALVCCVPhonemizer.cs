using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.G2p;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("Portuguese CRiSTAL VCCV Phonemizer", "PT-BR CRiSTAL", "xiao")]
    public class CRiSTALVCCVPhonemizer : SyllableBasedPhonemizer {
        private readonly string[] vowels = { "a", "an", "ax", "e", "eh", "en", "i", "in", "o", "oh", "on", "u", "un" };
        private readonly string[] consonants = { "b", "ch", "d", "dj", "f", "g", "h", "j", "k", "l", "lh", "m", "n", "nh", "p", "r", "rr", "rw", "s", "sh", "t", "v", "w", "x", "y", "z" };

        private static readonly IG2p g2pEngine = new PortugueseG2p();
        private static readonly Dictionary<string, string> phonemeMapping = new Dictionary<string, string> {
            { "a~", "an" }, { "E", "eh" }, { "e~", "en" }, { "i~", "in" }, { "O", "oh" }, { "o~", "on" }, { "u~", "un" },
            { "tS", "ch" }, { "dZ", "dj" }, { "X", "rr" }, { "R", "rr" }, { "Z", "j" }, { "L", "lh" }, { "J", "nh" }, { "S", "sh" },
            { "w~", "w" }, { "j", "y" }, { "j~", "y" }
        };

        private static readonly Dictionary<string, string> g2p = new Dictionary<string, string>();

        static CRiSTALVCCVPhonemizer() {
            var vBase = new Dictionary<string, string> {
                { "a", "a" }, { "á", "a" }, { "à", "a" }, { "â", "ax" }, { "ã", "an" },
                { "e", "e" }, { "é", "eh" }, { "ê", "e" },
                { "i", "i" }, { "í", "i" },
                { "o", "o" }, { "ó", "oh" }, { "ô", "o" }, { "õ", "on" },
                { "u", "u" }, { "ú", "u" }
            };

            foreach (var v in vBase) g2p[v.Key] = v.Value;

            string[] cBase = { "b", "d", "f", "j", "k", "l", "m", "n", "p", "s", "t", "v", "z" };

            foreach (var c in cBase) {
                foreach (var v in vBase) {
                    string cTarget = c;
                    if (c == "d" && v.Key == "i") cTarget = "dj";
                    if (c == "t" && v.Key == "i") cTarget = "ch";
                    g2p[c + v.Key] = cTarget + " " + v.Value;
                }
            }

            foreach (var v in vBase) {
                g2p["r" + v.Key] = "rr " + v.Value;
                g2p["c" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "s" : "k") + " " + v.Value;
                g2p["g" + v.Key] = (new[] { "e", "é", "ê", "i", "í" }.Contains(v.Key) ? "j" : "g") + " " + v.Value;
                g2p["nh" + v.Key] = "nh " + v.Value;
                g2p["lh" + v.Key] = "lh " + v.Value;
                g2p["ch" + v.Key] = "ch " + v.Value;
                g2p["x" + v.Key] = "x " + v.Value;
                g2p["h" + v.Key] = "h " + v.Value;
            }
        }

        protected override string[] GetVowels() => vowels;
        protected override string[] GetConsonants() => consonants;
        protected override string GetDictionaryName() => "";
        protected override Dictionary<string, string> GetDictionaryPhonemesReplacement() => new Dictionary<string, string>();
        protected override double GetTransitionBasicLengthMs(string alias = "") => 70.0;

        protected override IG2p LoadBaseDictionary() {
            return G2pDictionary.NewBuilder().Build();
        }

        protected override string[] GetSymbols(Note note) {
            var lyric = note.lyric.ToLower();
            if (g2p.ContainsKey(lyric)) {
                return g2p[lyric].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            var g2pResult = g2pEngine.Query(lyric);
            if (g2pResult != null && g2pResult.Length > 0) {
                return g2pResult.Select(p => phonemeMapping.ContainsKey(p) ? phonemeMapping[p] : p).ToArray();
            }
            return new string[] { lyric };
        }

        protected override List<string> ProcessSyllable(Syllable syllable) {
            var prevV = syllable.prevV;
            var v = syllable.v;
            var cc = syllable.cc;
            var phonemes = new List<string>();

            if (syllable.IsStartingV) {
                if (!TryAddPhoneme(phonemes, syllable.vowelTone, $"- {v}", v)) {
                    phonemes.Add(v);
                }
            } else if (syllable.IsVV) {
                if (!TryAddPhoneme(phonemes, syllable.vowelTone, $"{prevV} {v}", v)) {
                    phonemes.Add(v);
                }
            } else if (syllable.IsStartingCV) {
                // Try [- C1 C2 V]
                var rccv = $"- {string.Join(" ", cc)} {v}";
                if (HasOto(rccv, syllable.vowelTone)) {
                    phonemes.Add(rccv);
                } else {
                    // Try [- C1 V] only if there is exactly one consonant.
                    // For clusters (like "pra"), we prefer to decompose into [- p][p r][r a] 
                    // instead of skipping "r" to use a "- pa" alias.
                    var rcv = $"- {cc[0]} {v}";
                    if (cc.Length == 1 && HasOto(rcv, syllable.vowelTone)) {
                        phonemes.Add(rcv);
                    } else {
                        // Start with [- C1]
                        if (!TryAddPhoneme(phonemes, syllable.tone, $"- {cc[0]}", cc[0])) {
                            phonemes.Add(cc[0]);
                        }
                        // Chain clusters
                        for (int i = 0; i < cc.Length - 1; i++) {
                            TryAddPhoneme(phonemes, syllable.tone, $"{cc[i]} {cc[i + 1]}");
                        }
                        // End with [Cn V]
                        phonemes.Add($"{cc.Last()} {v}");
                    }
                }
            } else {
                // VCV or VCCV
                if (syllable.IsVCVWithOneConsonant) {
                    TryAddPhoneme(phonemes, syllable.tone, $"{prevV} {cc[0]}");
                    phonemes.Add($"{cc[0]} {v}");
                } else {
                    TryAddPhoneme(phonemes, syllable.tone, $"{prevV} {cc[0]}");
                    for (int i = 0; i < cc.Length - 1; i++) {
                        TryAddPhoneme(phonemes, syllable.tone, $"{cc[i]} {cc[i + 1]}");
                    }
                    phonemes.Add($"{cc.Last()} {v}");
                }
            }
            return phonemes;
        }

        protected override List<string> ProcessEnding(Ending ending) {
            var prevV = ending.prevV;
            var cc = ending.cc;
            var phonemes = new List<string>();

            if (ending.IsEndingV) {
                TryAddPhoneme(phonemes, ending.tone, $"{prevV} -", $"{prevV}-");
            } else {
                TryAddPhoneme(phonemes, ending.tone, $"{prevV} {cc[0]}");
                for (int i = 0; i < cc.Length - 1; i++) {
                    TryAddPhoneme(phonemes, ending.tone, $"{cc[i]} {cc[i + 1]}");
                }
                TryAddPhoneme(phonemes, ending.tone, $"{cc.Last()} -", $"{cc.Last()}-");
            }
            return phonemes;
        }
    }
}
