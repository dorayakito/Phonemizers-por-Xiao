using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.G2p;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese CVVC BRAPA Phonemizer", "PT-BR CVVC", "xiao")]
    public class PortugueseCVVCPhonemizer : Phonemizer {
        private readonly string[] vowels = { "a", "ao", "ah", "ahn", "ax", "an", "e", "en", "eh", "ehn", "ae", "aen", "i", "in", "i0", "o", "on", "oh", "ohn", "u", "un", "u0", "rh", "y", "w" };
        private readonly string[] consonants = { "b", "bv", "ch", "d", "dj", "f", "g", "gv", "h", "hr", "j", "k", "l", "lh", "l0", "m", "n", "ng", "nh", "p", "r", "rr", "rw", "s", "sh", "t", "v", "x", "z" };

        private readonly Dictionary<string, string> lyricToPhoneme = new Dictionary<string, string> {
            { "a", "a" }, { "á", "a" }, { "à", "a" }, { "â", "ax" }, { "ã", "an" },
            { "e", "e" }, { "é", "eh" }, { "ê", "e" },
            { "i", "i" }, { "í", "i" },
            { "o", "o" }, { "ó", "oh" }, { "ô", "o" }, { "õ", "on" },
            { "u", "u" }, { "ú", "u" },
            { "ba", "ba" }, { "be", "be" }, { "bi", "bi" }, { "bo", "bo" }, { "bu", "bu" },
            { "ca", "ka" }, { "ce", "se" }, { "ci", "si" }, { "co", "ko" }, { "cu", "ku" },
            { "da", "da" }, { "de", "de" }, { "di", "dji" }, { "do", "do" }, { "du", "du" },
            { "fa", "fa" }, { "fe", "fe" }, { "fi", "fi" }, { "fo", "fo" }, { "fu", "fu" },
            { "ga", "ga" }, { "ge", "je" }, { "gi", "ji" }, { "go", "go" }, { "gu", "gu" },
            { "ha", "a" }, { "he", "e" }, { "hi", "i" }, { "ho", "o" }, { "hu", "u" },
            { "ja", "ja" }, { "je", "je" }, { "ji", "ji" }, { "jo", "jo" }, { "ju", "ju" },
            { "ka", "ka" }, { "ke", "ke" }, { "ki", "ki" }, { "ko", "ko" }, { "ku", "ku" },
            { "la", "la" }, { "le", "le" }, { "li", "li" }, { "lo", "lo" }, { "lu", "lu" },
            { "ma", "ma" }, { "me", "me" }, { "mi", "mi" }, { "mo", "mo" }, { "mu", "mu" },
            { "na", "na" }, { "ne", "ne" }, { "ni", "ni" }, { "no", "no" }, { "nu", "nu" },
            { "pa", "pa" }, { "pe", "pe" }, { "pi", "pi" }, { "po", "po" }, { "pu", "pu" },
            { "qa", "ka" }, { "qe", "ke" }, { "qi", "ki" }, { "qo", "ko" }, { "qu", "ku" },
            { "ra", "Rra" }, { "re", "Rre" }, { "ri", "Rri" }, { "ro", "Rro" }, { "ru", "Rru" },
            { "sa", "sa" }, { "se", "se" }, { "si", "si" }, { "so", "so" }, { "su", "su" },
            { "ta", "ta" }, { "te", "te" }, { "ti", "chi" }, { "to", "to" }, { "tu", "tu" },
            { "va", "va" }, { "ve", "ve" }, { "vi", "vi" }, { "vo", "vo" }, { "vu", "vu" },
            { "xa", "sha" }, { "xe", "she" }, { "xi", "shi" }, { "xo", "sho" }, { "xu", "shu" },
            { "za", "za" }, { "ze", "ze" }, { "zi", "zi" }, { "zo", "zo" }, { "zu", "zu" },
        };

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric;
            if (lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric;
            }


            string phoneme = lyric;
            if (lyricToPhoneme.ContainsKey(lyric)) {
                phoneme = lyricToPhoneme[lyric];
            }


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
                if (singer.TryGetMappedOto(startAlias, tone, out var oto)) {
                    phonemes.Add(new Phoneme { phoneme = startAlias });
                } else {
                    phonemes.Add(new Phoneme { phoneme = c + v });
                }
            } else {

                var prevLyric = prevNeighbour.Value.lyric;

                
                string prevV = prevLyric;
                if (lyricToPhoneme.ContainsKey(prevV)) prevV = lyricToPhoneme[prevV];


                if (!string.IsNullOrEmpty(c)) {
                    var vcAlias = $"{prevV} {c}";
                    if (singer.TryGetMappedOto(vcAlias, tone, out var oto)) {
                        phonemes.Add(new Phoneme { 
                            phoneme = vcAlias, 
                            position = -oto.Preutterance 
                        });
                    }
                }


                var cvAlias = c + v;
                if (singer.TryGetMappedOto(cvAlias, tone, out var otoCV)) {
                    phonemes.Add(new Phoneme { phoneme = cvAlias });
                } else {
                    phonemes.Add(new Phoneme { phoneme = v });
                }
            }

            if (nextNeighbour == null) {
                var endAlias = $"{v} -";
                if (singer.TryGetMappedOto(endAlias, tone, out var otoEnd)) {
                }
            }

            return new Result {
                phonemes = phonemes.ToArray()
            };
        }
    }
}
