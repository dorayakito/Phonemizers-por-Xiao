using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugins {
    [Phonemizer("Portuguese X-SAMPA Phonemizer", "PT-BR X-SAMPA", "xiao")]
    public class XSampaPhonemizer : Phonemizer {
        private readonly string[] vowels = { "a", "e", "E", "i", "o", "O", "u", "a~", "e~", "i~", "o~", "u~", "j", "w" };
        private readonly string[] consonants = { "p", "b", "t", "d", "k", "g", "f", "v", "s", "z", "S", "Z", "m", "n", "J", "l", "L", "r", "R" };

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var lyric = notes[0].lyric;
            if (lyric == "+" && prevNeighbour != null) {
                lyric = prevNeighbour.Value.lyric;
            }

            string c = string.Empty;
            string v = lyric;

            foreach (var con in consonants.OrderByDescending(x => x.Length)) {
                if (lyric.StartsWith(con)) {
                    c = con;
                    v = lyric.Substring(con.Length);
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
                var prevLyric = prevNeighbour.Value.lyric;
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
