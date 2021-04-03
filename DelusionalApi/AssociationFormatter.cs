using DelusionalApi.Model;
using System.Collections.Generic;

namespace DelusionalApi
{
    public class AssociationFormatter : IAssociationFormatter
    {
        private readonly Dictionary<string, string> friendlyNames = new Dictionary<string, string>()
                    {
                        {  "/r/IsA", "is" },
                        {  "/r/PartOf", "part of" },
                        { "/r/HasA", "has" },
                        {  "/r/UsedFor", "used for" },
                        { "/r/CapableOf", "can" },
                        { "/r/AtLocation", "found in" },
                        {  "/r/Causes", "causes" },
                        { "/r/HasSubevent", "involves" },
                        {  "/r/HasFirstSubevent", "begins with" },
                        { "/r/HasLastSubevent", "ends with" },
                        {  "/r/HasPrerequisite", "requires" },
                        {  "/r/HasProperty", "has" },
                        {  "/r/MotivatedByGoal", "is motivated by" },
                        {  "/r/Desires", "desires" },
                        {  "/r/CreatedBy", "is created by" },
                        {  "/r/SymbolOf", "is a symbol of" },
                        { "/r/DefinedAs", "is defined as" },
                        {  "/r/MannerOf", "is a kind of" },
                        { "/r/LocatedNear", "found near" },
                        { "/r/HasContext", "has the context" },
                        {  "/r/SimilarTo", "similar to" },
                        {  "/r/CausesDesire", "causes the desire to" },
                        {  "/r/MadeOf", "made of" },
                        {  "/r/ReceivesAction", "gets" }
                    };

        public string Format(List<Association> associations)
        {
            var humanizedAssociations = string.Empty;

            foreach (var association in associations)
            {
                string humanizedAssociation =
                    $"{Sanitize(association.From)} {friendlyNames[association.Relationship]} {Sanitize(association.To)}. ";

                humanizedAssociations += char.ToUpper(humanizedAssociation[0]) + humanizedAssociation.Substring(1).ToLower();
            }

            return humanizedAssociations;
        }

        private string Sanitize(string s)
        {
            return s.Split("/")[3].Replace("_", " ");
        }
    }
}
