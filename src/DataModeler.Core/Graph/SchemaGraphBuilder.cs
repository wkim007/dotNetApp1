using System.Linq;
using DataModeler.Core.Models;

namespace DataModeler.Core.Graph
{
    public sealed class SchemaGraphBuilder
    {
        public DiagramGraph Build(SchemaModel schema)
        {
            var nodes = schema.Entities
                .Select(entity => new DiagramNode(
                    entity.Name,
                    entity.Name,
                    entity.Properties.Select(FormatProperty).ToArray()))
                .ToArray();

            var edges = schema.Relationships
                .Select(relationship => new DiagramEdge(
                    relationship.Name,
                    relationship.SourceEntity,
                    relationship.TargetEntity,
                    string.Format("{0} -> {1}", FormatCardinality(relationship.SourceCardinality), FormatCardinality(relationship.TargetCardinality))))
                .ToArray();

            return new DiagramGraph(schema.Name, nodes, edges);
        }

        private static string FormatProperty(PropertyDefinition property)
        {
            var nullability = property.IsNullable ? "nullable" : "required";
            var keyMarker = property.IsPrimaryKey ? " PK" : string.Empty;
            return string.Format("{0}: {1} ({2}){3}", property.Name, property.Type, nullability, keyMarker);
        }

        private static string FormatCardinality(Cardinality cardinality)
        {
            return cardinality == Cardinality.One ? "1" : "*";
        }
    }
}