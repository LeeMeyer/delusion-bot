using DelusionalApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DelusionalApi
{
    public class DelusionDictionary : IDelusionDictionary
    {
        private readonly Dictionary<DelusionType, List<string>> _delusionConcepts = new Dictionary<DelusionType, List<string>>
        {
            {
                DelusionType.Witchcraft,
                new List<string> {
                    "/c/en/black_magic",
                    "/c/en/voodoo",
                    "/c/en/witch",
                    "/c/en/warlock",
                    "/c/en/wand",
                    "/c/en/illegal",
                    "/c/en/criminal",
                    "/c/en/evil"
                }
            },
            {
                DelusionType.Wires,
                new List<string> {
                    "/c/en/wires",
                    "/c/en/wired",
                    "/c/en/sabotage",
                    "/c/en/electricity",
                    "/c/en/energy",
                    "/c/en/power",
                    "/c/en/violence",
                    "/c/en/domestic_violence",
                    "/c/en/abusive"
                }
            },
            {
                DelusionType.Impregnate,
                new List<string>() {
                    "/c/en/pregnant",
                    "/c/en/conceived",
                    "/c/en/breed",
                    "/c/en/birth",
                    "/c/en/newborn",
                    "/c/en/creation",
                    "/c/en/childhood"
                }
            },
            {
                DelusionType.Intruders,
                new List<string>() {
                    "/c/en/invasion",
                    "/c/en/self_defense",
                    "/c/en/bathtub",
                    "/c/en/manslaughter"
                }
            }

        };

        public string RandomConcept(DelusionType delusionType)
        {
            return _delusionConcepts[delusionType].OrderBy(d => Guid.NewGuid()).First();
        }

        public string DescribeDelusion(DelusionType delusionType)
        {
            switch (delusionType)
            {
                case DelusionType.Witchcraft:
                    return "The legislation bares semblance to witchcraft, which is illegal.";
                case DelusionType.Intruders:
                    return "I killed the invader in the bathtub. I grabbed him, and ran to the tub, strangling and suffocating him with the wires under cold water.";
                case DelusionType.Impregnate:
                    return "You know you were sired via illegal artificial insemination.";
                case DelusionType.Wires:
                    return "Hotty tampered with the wires to make us freeze to death as a form of domestic violence.";
            }

            return string.Empty;
        }
    }
}