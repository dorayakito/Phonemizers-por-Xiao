using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese CATIPA Phonemizer", "PT-BR CATIPA", "xiao")]
    public class CatipaPhonemizer : Phonemizer {
        private readonly string[] vowels = { "a", "e", "i", "o", "u", "a'", "e'", "i'", "o'", "u'", "a~", "e~", "i~", "o~", "u~" };
        private readonly string[] consonants = { "b", "d", "dj", "f", "g", "h", "j", "k", "l", "lh", "m", "n", "nh", "p", "r", "rr", "rrr", "wr", "s", "t", "tch", "v", "w", "x", "y", "z" };

        private readonly Dictionary<string, string> g2p = new Dictionary<string, string> {
            { "a", "a" }, { "á", "a" }, { "à", "a" }, { "ã", "a~" }, { "â", "a'" },
            { "e", "e" }, { "é", "e'" }, { "ê", "e" },
            { "i", "i" }, { "í", "i" },
            { "o", "o" }, { "ó", "o'" }, { "ô", "o" }, { "õ", "o~" },
            { "u", "u" }, { "ú", "u" },

            { "ba", "ba" }, { "be", "be" }, { "bi", "bi" }, { "bo", "bo" }, { "bu", "bu" },
            { "ca", "ka" }, { "ce", "se" }, { "ci", "si" }, { "co", "ko" }, { "cu", "ku" },
            { "da", "da" }, { "de", "de" }, { "di", "dj i" }, { "do", "do" }, { "du", "du" },
            { "fa", "fa" }, { "fe", "fe" }, { "fi", "fi" }, { "fo", "fo" }, { "fu", "fu" },
            { "ga", "ga" }, { "ge", "je" }, { "gi", "ji" }, { "go", "go" }, { "gu", "gu" },
            { "ha", "a" }, { "he", "e" }, { "hi", "i" }, { "ho", "o" }, { "hu", "u" },
            { "ja", "ja" }, { "je", "je" }, { "ji", "ji" }, { "jo", "jo" }, { "ju", "ju" },
            { "la", "la" }, { "le", "le" }, { "li", "li" }, { "lo", "lo" }, { "lu", "lu" },
            { "ma", "ma" }, { "me", "me" }, { "mi", "mi" }, { "mo", "mo" }, { "mu", "mu" },
            { "na", "na" }, { "ne", "ne" }, { "ni", "ni" }, { "no", "no" }, { "nu", "nu" },
            { "pa", "pa" }, { "pe", "pe" }, { "pi", "pi" }, { "po", "po" }, { "pu", "pu" },
            { "ra", "rr a" }, { "re", "rr e" }, { "ri", "rr i" }, { "ro", "rr o" }, { "ru", "rr u" },
            { "sa", "sa" }, { "se", "se" }, { "si", "si" }, { "so", "so" }, { "su", "su" },
            { "ta", "ta" }, { "te", "te" }, { "ti", "tch i" }, { "to", "to" }, { "tu", "tu" },
            { "va", "va" }, { "ve", "ve" }, { "vi", "vi" }, { "vo", "vo" }, { "vu", "vu" },
            { "xa", "xa" }, { "xe", "xe" }, { "xi", "xi" }, { "xo", "xo" }, { "xu", "xu" },
            { "za", "za" }, { "ze", "ze" }, { "zi", "zi" }, { "zo", "zo" }, { "zu", "zu" },
        };

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric.ToLower();
            }

            string phoneme = lyric;
            if (g2p.ContainsKey(lyric)) phoneme = g2p[lyric];

            string c = string.Empty;
            string v = phoneme;

            foreach (var con in consonants.OrderByDescending(x => x.Length)) {
                if (phoneme.StartsWith(con)) {
                    c = con;
                    v = phoneme.Substring(con.Length).Trim();
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
                if (g2p.ContainsKey(prevLyric)) prevLyric = g2p[prevLyric];
                
                string prevV = string.Empty;
                foreach (var vv in vowels.OrderByDescending(x => x.Length)) {
                    if (prevLyric.EndsWith(vv)) {
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
