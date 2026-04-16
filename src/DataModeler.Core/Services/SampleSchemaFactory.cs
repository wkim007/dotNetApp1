using DataModeler.Core.Models;

namespace DataModeler.Core.Services;

public static class SampleSchemaFactory
{
    public const string RetailSchemaFileName = "retail-schema.json";

    public static SchemaModel CreateRetailSchema() =>
        new(
            "Retail Ordering Domain",
            new[]
            {
                new EntityDefinition(
                    "Customer",
                    new[]
                    {
                        new PropertyDefinition("CustomerId", "Guid", IsPrimaryKey: true, IsNullable: false),
                        new PropertyDefinition("Email", "string", IsNullable: false),
                        new PropertyDefinition("DisplayName", "string", IsNullable: false),
                        new PropertyDefinition("CreatedUtc", "DateTime", IsNullable: false)
                    }),
                new EntityDefinition(
                    "Order",
                    new[]
                    {
                        new PropertyDefinition("OrderId", "Guid", IsPrimaryKey: true, IsNullable: false),
                        new PropertyDefinition("CustomerId", "Guid", IsNullable: false),
                        new PropertyDefinition("Status", "OrderStatus", IsNullable: false),
                        new PropertyDefinition("SubmittedUtc", "DateTime", IsNullable: true)
                    }),
                new EntityDefinition(
                    "OrderLine",
                    new[]
                    {
                        new PropertyDefinition("OrderLineId", "Guid", IsPrimaryKey: true, IsNullable: false),
                        new PropertyDefinition("OrderId", "Guid", IsNullable: false),
                        new PropertyDefinition("ProductId", "Guid", IsNullable: false),
                        new PropertyDefinition("Quantity", "int", IsNullable: false),
                        new PropertyDefinition("UnitPrice", "decimal", IsNullable: false)
                    }),
                new EntityDefinition(
                    "Product",
                    new[]
                    {
                        new PropertyDefinition("ProductId", "Guid", IsPrimaryKey: true, IsNullable: false),
                        new PropertyDefinition("Sku", "string", IsNullable: false),
                        new PropertyDefinition("Name", "string", IsNullable: false),
                        new PropertyDefinition("CurrentPrice", "decimal", IsNullable: false)
                    })
            },
            new[]
            {
                new RelationshipDefinition("CustomerOrders", "Customer", "Order", Cardinality.One, Cardinality.Many),
                new RelationshipDefinition("OrderLines", "Order", "OrderLine", Cardinality.One, Cardinality.Many),
                new RelationshipDefinition("ProductOrderLines", "Product", "OrderLine", Cardinality.One, Cardinality.Many)
            });
}
