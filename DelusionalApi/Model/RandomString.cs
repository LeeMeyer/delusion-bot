
using System;
using System.Linq;
using System.Reflection;

namespace DelusionalApi.Model
{
    public class RandomString : Random<string>
    {
        public string Next()
        {
            IResourceLoader resourceLoader = new ResourceLoader();
            var moo = resourceLoader.GetEmbeddedResourceString(this.GetType().Assembly, "WordList.txt");

            var randomWord = moo.Split("\n")
                .OrderBy(w => Guid.NewGuid())
                .First();

            UsedItems.Add(randomWord);

            return randomWord;

        }
    }
}