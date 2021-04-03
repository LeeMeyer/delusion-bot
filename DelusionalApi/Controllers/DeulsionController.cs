using System;
using System.Threading.Tasks;
using DelusionalApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace DelusionalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DelusionController : ControllerBase
    {
        private readonly IConceptGraphDb _conceptGraphDb;
        private readonly IAssociationFormatter _associationFormatter;
        private readonly IDelusionDictionary _delusionDictionary;

        public DelusionController(IConceptGraphDb conceptGraphDb, IAssociationFormatter associationFormatter,
            IDelusionDictionary delusionDictionary)
        {
            _conceptGraphDb = conceptGraphDb;
            _associationFormatter = associationFormatter;
            _delusionDictionary = delusionDictionary;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string word = "silence", DelusionType? delusionType = null)
        {
            if (!delusionType.HasValue)
            {
                delusionType = (DelusionType)new Random().Next(0, (int)DelusionType.Impregnate);
            }

            var endConcept = _delusionDictionary.RandomConcept(delusionType.Value);
            var associations = _associationFormatter.Format(await _conceptGraphDb.GetAssociations(word, endConcept));
            var delusionDescription = _delusionDictionary.DescribeDelusion(delusionType.Value);
            var output = $"{associations} {delusionDescription}";
            return Ok(output);
        }
    }
}