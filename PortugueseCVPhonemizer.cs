using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese CV BRAPA Phonemizer", "PT-BR CV", "xiao")]
    public class PortugueseCVPhonemizer : Phonemizer {
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
            if (lyricToPhoneme.ContainsKey(lyric)) {
                lyric = lyricToPhoneme[lyric];
            }

            int tone = notes[0].tone;

            // Try "- CV" first, then "CV"
            string startAlias = $"- {lyric}";
            if (prevNeighbour == null && singer.TryGetMappedOto(startAlias, tone, out var oto)) {
                return new Result {
                    phonemes = new[] { new Phoneme { phoneme = startAlias } }
                };
            }

            return new Result {
                phonemes = new[] { new Phoneme { phoneme = lyric } }
            };
        }
    }
}
