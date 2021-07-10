using Catalyst;
using System;
using System.Linq;

namespace DelusionalApi.Model
{
    public class RandomString : Random<string>
    {
        public string Next()
        {
            var randomWord = WordNet.Nouns.GetAll()
                .OrderBy(w => Guid.NewGuid())
                .First(w => w.Word.Length > 3 && w.PartOfSpeech == PartOfSpeech.NOUN && w.Word.Length < 5 && Char.IsLower(w.Word.First()) && !UsedItems.Contains(w.Word))
                .Word;

            UsedItems.Add(randomWord);

            return randomWord;

        }
    }
}