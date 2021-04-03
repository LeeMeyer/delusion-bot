using System;
using System.Net;
using System.Threading.Tasks;
using DelusionalApi.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        /// <summary>
        /// Gets delusional text connecting the passed-in word to a delusion. Even when the parameters are the same,
        /// response text generation involves randomness and can vary for each request.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Delusion?word=swim&amp;delusionType=Intruders
        ///     
        /// Sample response:
        /// 
        ///     "Heat causes the desire to swim. Starting flame or fire requires heat. Starting flame or fire causes death. Death is created by homicide. Manslaughter is homicide.  I killed the invader in the bathtub. I grabbed him, and ran to the tub, strangling and suffocating him with the wires under cold water."
        ///
        /// Sample request:
        ///
        ///     GET /Delusion?word=swim&amp;delusionType=Impregnate
        ///     
        /// Sample response:
        /// 
        ///     "Fish can swim. Fish is animal. Animal can live. Live requires conceived.  You know you were sired via illegal artificial insemination."
        /// </remarks>
        /// <param name="word"></param>
        /// <param name="delusionType">If not specified, defaults to random</param>
        /// <returns>Delusional ramblings</returns>
        /// <response code="200">Returns a delusional string</response>           
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