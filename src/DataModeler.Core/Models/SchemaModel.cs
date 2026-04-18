using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataModeler.Core.Models
{
    [DataContract]
    public sealed class SchemaModel
    {
        public SchemaModel()
        {
            Entities = new List<EntityDefinition>();
            Relationships = new List<RelationshipDefinition>();
        }

        public SchemaModel(string name, IReadOnlyList<EntityDefinition> entities, IReadOnlyList<RelationshipDefinition> relationships)
        {
            Name = name;
            Entities = entities;
            Relationships = relationships;
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "entities")]
        public IReadOnlyList<EntityDefinition> Entities { get; set; }

        [DataMember(Name = "relationships")]
        public IReadOnlyList<RelationshipDefinition> Relationships { get; set; }
    }

    [DataContract]
    public sealed class EntityDefinition
    {
        public EntityDefinition()
        {
            Properties = new List<PropertyDefinition>();
        }

        public EntityDefinition(string name, IReadOnlyList<PropertyDefinition> properties)
        {
            Name = name;
            Properties = properties;
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "properties")]
        public IReadOnlyList<PropertyDefinition> Properties { get; set; }
    }

    [DataContract]
    public sealed class PropertyDefinition
    {
        public PropertyDefinition()
        {
            IsNullable = true;
        }

        public PropertyDefinition(string name, string type, bool isPrimaryKey = false, bool isNullable = true)
        {
            Name = name;
            Type = type;
            IsPrimaryKey = isPrimaryKey;
            IsNullable = isNullable;
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "isPrimaryKey")]
        public bool IsPrimaryKey { get; set; }

        [DataMember(Name = "isNullable")]
        public bool IsNullable { get; set; }
    }

    [DataContract]
    public sealed class RelationshipDefinition
    {
        public RelationshipDefinition()
        {
        }

        public RelationshipDefinition(string name, string sourceEntity, string targetEntity, Cardinality sourceCardinality, Cardinality targetCardinality)
        {
            Name = name;
            SourceEntity = sourceEntity;
            TargetEntity = targetEntity;
            SourceCardinality = sourceCardinality;
            TargetCardinality = targetCardinality;
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "sourceEntity")]
        public string SourceEntity { get; set; }

        [DataMember(Name = "targetEntity")]
        public string TargetEntity { get; set; }

        [DataMember(Name = "sourceCardinality")]
        public Cardinality SourceCardinality { get; set; }

        [DataMember(Name = "targetCardinality")]
        public Cardinality TargetCardinality { get; set; }
    }

    public enum Cardinality
    {
        One,
        Many
    }
}
