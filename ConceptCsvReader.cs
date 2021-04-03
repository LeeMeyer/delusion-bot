using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DelusionBotDatabaseBuilder
{
    public class ConceptCsvReader : IDisposable
    {
        private readonly CsvReader _csvReader;
        private readonly StreamReader _streamReader; 
        private readonly List<string> relationshipWhitelist = new List<string>()
                    {
                        "/r/IsA",
                        "/r/PartOf",
                        "/r/HasA",
                        "/r/UsedFor",
                        "/r/CapableOf",
                        "/r/AtLocation",
                        "/r/Causes",
                        "/r/HasSubevent",
                        "/r/HasFirstSubevent",
                        "/r/HasLastSubevent",
                        "/r/HasPrerequisite",
                        "/r/HasProperty",
                        "/r/MotivatedByGoal",
                        "/r/Desires",
                        "/r/CreatedBy",
                        "/r/SymbolOf",
                        "/r/DefinedAs",
                        "/r/MannerOf",
                        "/r/LocatedNear",
                        "/r/HasContext",
                        "/r/SimilarTo",
                        "/r/CausesDesire",
                        "/r/MadeOf",
                        "/r/ReceivesAction"
                    };


        public ConceptAssociation CurrentAssociation { get; private set; }

        public ConceptCsvReader(string csvPath)
        {
            _streamReader = new StreamReader(csvPath);
            
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "\t",
                BadDataFound = null
            };

            _csvReader = new CsvReader(_streamReader, configuration);
        }

        public bool Read()
        {
            CurrentAssociation = null;
            var nextRowFound = _csvReader.Read();

            while (nextRowFound && CurrentAssociation == null)
            {
                var association = ReadAssociation();

                if (association.From.Contains("/en/") && association.To.Contains("/en/")
                    && relationshipWhitelist.Any(r => association.Relationship.Equals(r, StringComparison.InvariantCultureIgnoreCase)))
                {
                    CurrentAssociation = association;
                }
                else
                {
                    nextRowFound = _csvReader.Read();
                }
            }
            

            return nextRowFound;
        }

        private ConceptAssociation ReadAssociation()
        {
            return new ConceptAssociation
            {
                From = _csvReader.GetField(2),
                To = _csvReader.GetField(3),
                Relationship = _csvReader.GetField(1)
            };
        }

        public void Dispose()
        {
            _streamReader.Dispose();
            _csvReader.Dispose();

        }
    }
}
