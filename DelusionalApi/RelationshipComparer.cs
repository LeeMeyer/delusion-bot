using Neo4j.Driver;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DelusionalApi
{
    public class RelationshipComparer : IEqualityComparer<IRelationship>
    {
        public bool Equals(IRelationship x, IRelationship y)
        {
            return (x.StartNodeId == y.StartNodeId && x.EndNodeId == y.EndNodeId)
                || (x.EndNodeId == y.StartNodeId && x.StartNodeId == y.EndNodeId)
                || (x.StartNodeId == y.EndNodeId && x.EndNodeId == y.StartNodeId);
        }

        public int GetHashCode(IRelationship obj)
        {
            return (int)obj.StartNodeId * (int)obj.EndNodeId;
        }
    }
}
