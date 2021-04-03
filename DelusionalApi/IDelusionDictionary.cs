using DelusionalApi.Model;

namespace DelusionalApi
{
    public interface IDelusionDictionary
    {
        string DescribeDelusion(DelusionType delusionType);
        string RandomConcept(DelusionType delusionType);
    }
}