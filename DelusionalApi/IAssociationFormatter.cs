using DelusionalApi.Model;
using System.Collections.Generic;

namespace DelusionalApi
{
    public interface IAssociationFormatter
    {
        string Format(List<Association> associations);
    }
}