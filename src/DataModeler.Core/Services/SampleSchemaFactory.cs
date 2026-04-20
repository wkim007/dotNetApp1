using System.Collections.Generic;
using DataModeler.Core.Models;

namespace DataModeler.Core.Services
{
    public static class SampleSchemaFactory
    {
        public const string RetailSchemaFileName = "retail-schema.json";

        public static SchemaModel CreateRetailSchema()
        {
            return new SchemaModel(
                "Retail Ordering Domain",
                new List<EntityDefinition>
                {
                    new EntityDefinition(
                        "Customer",
                        new List<PropertyDefinition>
                        {
                            new PropertyDefinition("CustomerId", "Guid", isPrimaryKey: true, isNullable: false),
                            new PropertyDefinition("Email", "string", isNullable: false),
                            new PropertyDefinition("DisplayName", "string", isNullable: false),
                            new PropertyDefinition("CreatedUtc", "DateTime", isNullable: false)
                        }),
                    new EntityDefinition(
                        "Order",
                        new List<PropertyDefinition>
                        {
                            new PropertyDefinition("OrderId", "Guid", isPrimaryKey: true, isNullable: false),
                            new PropertyDefinition("CustomerId", "Guid", isNullable: false),
                            new PropertyDefinition("Status", "OrderStatus", isNullable: false),
                            new PropertyDefinition("SubmittedUtc", "DateTime", isNullable: true)
                        }),
                    new EntityDefinition(
                        "OrderLine",
                        new List<PropertyDefinition>
                        {
                            new PropertyDefinition("OrderLineId", "Guid", isPrimaryKey: true, isNullable: false),
                            new PropertyDefinition("OrderId", "Guid", isNullable: false),
                            new PropertyDefinition("ProductId", "Guid", isNullable: false),
                            new PropertyDefinition("Quantity", "int", isNullable: false),
                            new PropertyDefinition("UnitPrice", "decimal", isNullable: false)
                        }),
                    new EntityDefinition(
                        "Product",
                        new List<PropertyDefinition>
                        {
                            new PropertyDefinition("ProductId", "Guid", isPrimaryKey: true, isNullable: false),
                            new PropertyDefinition("Sku", "string", isNullable: false),
                            new PropertyDefinition("Name", "string", isNullable: false),
                            new PropertyDefinition("CurrentPrice", "decimal", isNullable: false)
                        })
                },
                new List<RelationshipDefinition>
                {
                    new RelationshipDefinition("CustomerOrders", "Customer", "Order", Cardinality.One, Cardinality.Many),
                    new RelationshipDefinition("OrderLines", "Order", "OrderLine", Cardinality.One, Cardinality.Many),
                    new RelationshipDefinition("ProductOrderLines", "Product", "OrderLine", Cardinality.One, Cardinality.Many)
                });
        }
    }
}
