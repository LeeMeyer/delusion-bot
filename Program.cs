using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DelusionBotDatabaseBuilder
{
    public class Program
    {
        static async Task Main(string[] args)
        {   
            using (var reader = new ConceptCsvReader(@"C:\Users\lee\Downloads\assertions.csv"))
            using (var writer = new ConceptGraphDbWriter("bolt://localhost:7687", "***", "***"))
            {
                while (reader.Read())
                {
                    var association = reader.CurrentAssociation;
                    await writer.Save(association);
                    Console.WriteLine($"{association.From} {association.Relationship} {association.To}");
                }
            }
        }
    }
}
