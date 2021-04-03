using Neo4j.Driver;
using System;
using System.Threading.Tasks;

namespace DelusionBotDatabaseBuilder
{
    public class ConceptGraphDbWriter : IDisposable
    {
        private readonly IDriver _driver;

        public ConceptGraphDbWriter(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        public async Task Save(ConceptAssociation association)
        {
            var session = _driver.AsyncSession();

            await session.WriteTransactionAsync(async tx =>
            {
                var result = await tx.RunAsync(
                    @$"MERGE (concept1: `{association.From}`) 
                        MERGE(concept2: `{association.To}`)
                        MERGE(concept1) -[:`{association.Relationship}`]->(concept2)");
            });
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
