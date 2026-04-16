namespace DataModeler.Core.Models;

public sealed record SchemaModel(
    string Name,
    IReadOnlyList<EntityDefinition> Entities,
    IReadOnlyList<RelationshipDefinition> Relationships);

public sealed record EntityDefinition(
    string Name,
    IReadOnlyList<PropertyDefinition> Properties);

public sealed record PropertyDefinition(
    string Name,
    string Type,
    bool IsPrimaryKey = false,
    bool IsNullable = true);

public sealed record RelationshipDefinition(
    string Name,
    string SourceEntity,
    string TargetEntity,
    Cardinality SourceCardinality,
    Cardinality TargetCardinality);

public enum Cardinality
{
    One,
    Many
}
