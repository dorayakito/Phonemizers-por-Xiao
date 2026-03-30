using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese X-SAMPA DiffSinger Phonemizer", "DIFF XS", "xiao")]
    public class XSampaDiffSingerPhonemizer : Phonemizer {
        private readonly Dictionary<string, string> g2p = new Dictionary<string, string> {
            { "a", "a" }, { "á", "a" }, { "à", "a" }, { "ã", "a~" }, { "â", "6" },
            { "e", "e" }, { "é", "E" }, { "ê", "e" },
            { "i", "i" }, { "í", "i" },
            { "o", "o" }, { "ó", "O" }, { "ô", "o" }, { "õ", "o~" },
            { "u", "u" }, { "ú", "u" },

            { "ba", "b a" }, { "be", "b e" }, { "bi", "b i" }, { "bo", "b o" }, { "bu", "b u" },
            { "ca", "k a" }, { "ce", "s e" }, { "ci", "s i" }, { "co", "k o" }, { "cu", "k u" },
            { "da", "d a" }, { "de", "d e" }, { "di", "dZ i" }, { "do", "d o" }, { "du", "d u" },
            { "fa", "f a" }, { "fe", "f e" }, { "fi", "f i" }, { "fo", "f o" }, { "fu", "f u" },
            { "ga", "g a" }, { "ge", "Z e" }, { "gi", "Z i" }, { "go", "g o" }, { "gu", "g u" },
            { "ha", "a" }, { "he", "e" }, { "hi", "i" }, { "ho", "o" }, { "hu", "u" },
            { "ja", "Z a" }, { "je", "Z e" }, { "ji", "Z i" }, { "jo", "Z o" }, { "ju", "Z u" },
            { "la", "l a" }, { "le", "l e" }, { "li", "l i" }, { "lo", "l o" }, { "lu", "l u" },
            { "ma", "m a" }, { "me", "m e" }, { "mi", "m i" }, { "mo", "m o" }, { "mu", "m u" },
            { "na", "n a" }, { "ne", "n e" }, { "ni", "n i" }, { "no", "n o" }, { "nu", "n u" },
            { "pa", "p a" }, { "pe", "p e" }, { "pi", "p i" }, { "po", "p o" }, { "pu", "p u" },
            { "ra", "R a" }, { "re", "R e" }, { "ri", "R i" }, { "ro", "R o" }, { "ru", "R u" },
            { "sa", "s a" }, { "se", "s e" }, { "si", "s i" }, { "so", "s o" }, { "su", "s u" },
            { "ta", "t a" }, { "te", "t e" }, { "ti", "tS i" }, { "to", "t o" }, { "tu", "t u" },
            { "va", "v a" }, { "ve", "v e" }, { "vi", "v i" }, { "vo", "v o" }, { "vu", "v u" },
            { "xa", "S a" }, { "xe", "S e" }, { "xi", "S i" }, { "xo", "S o" }, { "xu", "S u" },
            { "za", "z a" }, { "ze", "z e" }, { "zi", "z i" }, { "zo", "z o" }, { "zu", "z u" },
        };

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric.ToLower();
            if (lyric == "+" && prevNeighbour != null) {
                return new Result { phonemes = new Phoneme[0] };
            }

            string phonemeStr = lyric;
            if (g2p.ContainsKey(lyric)) phonemeStr = g2p[lyric];

            var phonemeList = phonemeStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<Phoneme>();

            foreach (var p in phonemeList) {
                result.Add(new Phoneme { phoneme = p });
            }

            return new Result { phonemes = result.ToArray() };
        }
    }
}
