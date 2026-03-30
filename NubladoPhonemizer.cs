using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese NUBLADO Phonemizer", "PT-BR NUBLADO", "xiao")]
    public class NubladoPhonemizer : Phonemizer {
        private readonly string[] vowels = { "a", "e", "i", "o", "u", "3", "0", "a]", "e]", "i]", "o]", "u]" };
        private readonly string[] consonants = { "w", "r", "t", "y", "p", "s", "d", "f", "g", "h", "j", "k", "l", "z", "x", "v", "b", "n", "m", "ch", "Dd" };

        private readonly Dictionary<string, string> g2p = new Dictionary<string, string> {
            { "a", "a" }, { "á", "a" }, { "à", "a" }, { "ã", "a]" }, { "â", "a]" },
            { "e", "e" }, { "é", "3" }, { "ê", "e" },
            { "i", "i" }, { "í", "i" },
            { "o", "o" }, { "ó", "0" }, { "ô", "o" }, { "õ", "o]" },
            { "u", "u" }, { "ú", "u" },

            { "ba", "ba" }, { "be", "be" }, { "bi", "bi" }, { "bo", "bo" }, { "bu", "bu" },
            { "ca", "ka" }, { "ce", "se" }, { "ci", "si" }, { "co", "ko" }, { "cu", "ku" },
            { "da", "da" }, { "de", "de" }, { "di", "Ddi" }, { "do", "do" }, { "du", "du" },
            { "fa", "fa" }, { "fe", "fe" }, { "fi", "fi" }, { "fo", "fo" }, { "fu", "fu" },
            { "ga", "ga" }, { "ge", "je" }, { "gi", "ji" }, { "go", "go" }, { "gu", "gu" },
            { "ha", "a" }, { "he", "e" }, { "hi", "i" }, { "ho", "o" }, { "hu", "u" },
            { "ja", "ja" }, { "je", "je" }, { "ji", "ji" }, { "jo", "jo" }, { "ju", "ju" },
            { "la", "la" }, { "le", "le" }, { "li", "li" }, { "lo", "lo" }, { "lu", "lu" },
            { "ma", "ma" }, { "me", "me" }, { "mi", "mi" }, { "mo", "mo" }, { "mu", "mu" },
            { "na", "na" }, { "ne", "ne" }, { "ni", "ni" }, { "no", "no" }, { "nu", "nu" },
            { "pa", "pa" }, { "pe", "pe" }, { "pi", "pi" }, { "po", "po" }, { "pu", "pu" },
            { "ra", "ra" }, { "re", "re" }, { "ri", "ri" }, { "ro", "ro" }, { "ru", "ru" },
            { "sa", "sa" }, { "se", "se" }, { "si", "si" }, { "so", "so" }, { "su", "su" },
            { "ta", "ta" }, { "te", "te" }, { "ti", "chi" }, { "to", "to" }, { "tu", "tu" },
            { "va", "va" }, { "ve", "ve" }, { "vi", "vi" }, { "vo", "vo" }, { "vu", "vu" },
            { "xa", "sha" }, { "xe", "she" }, { "xi", "shi" }, { "xo", "sho" }, { "xu", "shu" },
            { "za", "za" }, { "ze", "ze" }, { "zi", "zi" }, { "zo", "zo" }, { "zu", "zu" },
        };

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric.ToLower();
            }

            string phoneme = lyric;
            if (g2p.ContainsKey(lyric)) phoneme = g2p[lyric];


            if (lyric.EndsWith("ão")) phoneme = phoneme.Substring(0, phoneme.Length - 2) + "awn";
            if (lyric.EndsWith("om")) phoneme = phoneme.Substring(0, phoneme.Length - 2) + "own";
            if (lyric.EndsWith("em")) phoneme = phoneme.Substring(0, phoneme.Length - 2) + "ewn";

            string c = string.Empty;
            string v = phoneme;

            foreach (var con in consonants.OrderByDescending(x => x.Length)) {
                if (phoneme.StartsWith(con)) {
                    c = con;
                    v = phoneme.Substring(con.Length);
                    break;
                }
            }


            if (lyric.StartsWith("ny")) {
                c = "ny";
                v = phoneme.Substring(2);
            }

            var phonemes = new List<Phoneme>();
            int tone = notes[0].tone;

            if (prevNeighbour == null) {
                var startAlias = $"(- {c}{v})";
                if (!singer.TryGetMappedOto(startAlias, tone, out _)) startAlias = $"({c}{v})";
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


            if (nextNeighbour == null) {
                string finalV = v;
                if (lyric.EndsWith("r")) {
                    var rFinal = $"({v}R)";
                    if (singer.TryGetMappedOto(rFinal, tone, out _)) {
                        phonemes.Add(new Phoneme { phoneme = rFinal });
                    }
                }
            }

            return new Result { phonemes = phonemes.ToArray() };
        }
    }
}
