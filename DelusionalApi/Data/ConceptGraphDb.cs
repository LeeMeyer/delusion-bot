using DelusionalApi.Model;
using Neo4j.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelusionalApi
{
    public class ConceptGraphDb : IDisposable, IConceptGraphDb
    {
        private readonly IDriver _driver;

        public ConceptGraphDb(AppSetttings appSetttings)
        {
            _driver = GraphDatabase.Driver(appSetttings.ConceptGraphSettings.Uri, 
                AuthTokens.Basic(appSetttings.ConceptGraphSettings.Username, appSetttings.ConceptGraphSettings.Password));
        }


        public async Task<List<Association>> GetAssociations(string startConcept, string endConcept)
        {
            var session = _driver.AsyncSession();
            var returnList = new List<Association>();

            await session.ReadTransactionAsync(async tx =>
            {
                var result = await tx.RunAsync(
                    $@"MATCH path = shortestPath(
                        (concept1:`/c/en/{startConcept}`)-[*1..30]-(concept2:`{endConcept}`)
                      )
                      RETURN path;");

                if (await result.FetchAsync())
                {
                    var segments = result.Current;
                    var path = segments["path"].As<IPath>();
                    var relationships = path.Relationships;

                    foreach (var relationship in relationships.Distinct(new RelationshipComparer()))
                    {
                        var startNode = path.Nodes.Single(n => n.Id == relationship.StartNodeId);
                        var endNode = path.Nodes.Single(n => n.Id == relationship.EndNodeId);

                        var association = new Association
                        {
                            From = startNode.Labels.Single(),
                            Relationship = relationship.Type,
                            To = endNode.Labels.Single()
                        };

                        returnList.Add(association);
                    }
                }
            });

            return returnList;
        }

        public async Task SavePoem(PhonePoem phonePoem)
        {
            var session = _driver.AsyncSession();

            await session.WriteTransactionAsync(async tx =>
            {
                await tx.RunAsync("MATCH (n:Madlib {phoneNumber: '" + phonePoem.PhoneNumber + "'}) DELETE n");
                await tx.RunAsync(string.Format("CREATE (n:Madlib {{phoneNumber: '{0}', excerptIndex: {1}, tokenIndexes: {2}, replacementWords: {3} }} )", 
                    phonePoem.PhoneNumber, 
                    phonePoem.ExcerptIndex, 
                    JsonConvert.SerializeObject(phonePoem.TokenIndexes)),
                    JsonConvert.SerializeObject(phonePoem.ReplacementWords));
                await tx.CommitAsync();
            });
        }

        public async Task<PhonePoem> GetPoem(string phoneNumber)
        {
            var session = _driver.AsyncSession();
            PhonePoem phonePoem = null;

            await session.ReadTransactionAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH(n:Madlib { phoneNumber: '" + phoneNumber + "'}) RETURN n");

                if (await result.FetchAsync())
                {
                    phonePoem = result.Current.As<PhonePoem>();
                }
            });

            return phonePoem;
        }




        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}