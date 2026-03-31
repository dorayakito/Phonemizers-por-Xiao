using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese CRiSTAL VCCV Phonemizer", "PT-BR CRiSTAL", "xiao")]
    public class CRiSTALVCCVPhonemizer : Phonemizer {
        protected USinger singer;

        public override void SetSinger(USinger singer) => this.singer = singer;

        private readonly string[] vowels = { "a", "an", "ax", "e", "eh", "en", "i", "in", "o", "oh", "on", "u", "un" };
        private readonly string[] consonants = { "b", "ch", "d", "dj", "f", "g", "h", "j", "k", "l", "lh", "m", "n", "nh", "p", "r", "rr", "rw", "s", "sh", "t", "v", "w", "x", "y", "z" };

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

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyric == "+" && prevNeighbour != null) {
                return new Result { phonemes = new Phoneme[0] };
            }

            string phonemeStr = g2p.ContainsKey(lyric) ? g2p[lyric] : lyric;
            var phonemeList = phonemeStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int tone = notes[0].tone;

            var result = new List<Phoneme>();
            string prevV = string.Empty;

            if (prevNeighbour != null) {
                var prevLyric = prevNeighbour.Value.lyric.ToLower();
                var prevPhonemeStr = g2p.ContainsKey(prevLyric) ? g2p[prevLyric] : prevLyric;
                var prevPhonemes = prevPhonemeStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in prevPhonemes.Reverse()) {
                    if (vowels.Contains(p)) {
                        prevV = p;
                        break;
                    }
                }
            }

            for (int i = 0; i < phonemeList.Length; i++) {
                string current = phonemeList[i];
                if (vowels.Contains(current)) {
                    if (string.IsNullOrEmpty(prevV)) {
                        string starter = $"- {current}";
                        if (!singer.TryGetMappedOto(starter, tone, out _)) starter = $"-{current}";
                        if (!singer.TryGetMappedOto(starter, tone, out _)) starter = current;
                        result.Add(new Phoneme { phoneme = starter });
                    } else {
                        string transition = $"{prevV} {current}";
                        if (!singer.TryGetMappedOto(transition, tone, out _)) transition = current;
                        result.Add(new Phoneme { phoneme = transition });
                    }
                    prevV = current;
                } else if (consonants.Contains(current)) {
                    string nextV = string.Empty;
                    for (int j = i + 1; j < phonemeList.Length; j++) {
                        if (vowels.Contains(phonemeList[j])) {
                            nextV = phonemeList[j];
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(prevV)) {
                        string starter = string.IsNullOrEmpty(nextV) ? current : $"- {current} {nextV}";
                        if (!singer.TryGetMappedOto(starter, tone, out _)) starter = $"-{current}{nextV}";
                        if (!singer.TryGetMappedOto(starter, tone, out _)) starter = $"- {current}";
                        if (!singer.TryGetMappedOto(starter, tone, out _)) starter = current;
                        result.Add(new Phoneme { phoneme = starter });
                    } else if (!string.IsNullOrEmpty(nextV)) {
                        string vc = $"{prevV} {current}";
                        if (singer.TryGetMappedOto(vc, tone, out var oto)) {
                            result.Add(new Phoneme { phoneme = vc, position = -MsToTick(oto.Preutter) });
                        }

                        string cv = $"{current} {nextV}";
                        if (!singer.TryGetMappedOto(cv, tone, out _)) cv = $"{current}{nextV}";
                        if (!singer.TryGetMappedOto(cv, tone, out _)) cv = nextV;
                        result.Add(new Phoneme { phoneme = cv });
                        i++;
                        prevV = nextV;
                    } else {
                        string vc = $"{prevV} {current}";
                        if (!singer.TryGetMappedOto(vc, tone, out _)) vc = $"{prevV}{current}";
                        if (!singer.TryGetMappedOto(vc, tone, out _)) vc = current;
                        result.Add(new Phoneme { phoneme = vc });
                    }
                }
            }

            if (nextNeighbour == null) {
                string lastPhoneme = phonemeList.Last();
                if (vowels.Contains(lastPhoneme)) {
                    string end = $"{lastPhoneme} -";
                    if (!singer.TryGetMappedOto(end, tone, out _)) end = $"{lastPhoneme}-";
                    if (singer.TryGetMappedOto(end, tone, out _)) {
                        result.Add(new Phoneme { phoneme = end, position = 100 });
                    }
                }
            }

            return new Result { phonemes = result.ToArray() };
        }
    }
}
