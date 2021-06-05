using DelusionalApi.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DelusionalApi
{
    public interface IConceptGraphDb
    {
        Task<List<Association>> GetAssociations(string startConcept, string endConcept);

        Task SavePoem(PhonePoem phonePoem);
        Task<PhonePoem> GetPoem(string phoneNumber);
    }
}