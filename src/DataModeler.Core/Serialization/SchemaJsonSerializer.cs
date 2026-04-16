using System.Text.Json;
using DataModeler.Core.Models;

namespace DataModeler.Core.Serialization;

public sealed class SchemaJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SchemaModel Deserialize(string json)
    {
        var schema = JsonSerializer.Deserialize<SchemaModel>(json, SerializerOptions);
        return schema ?? throw new InvalidOperationException("Schema payload did not deserialize into a valid schema model.");
    }

    public SchemaModel DeserializeFromFile(string filePath) =>
        Deserialize(File.ReadAllText(filePath));

    public string Serialize(SchemaModel schema) =>
        JsonSerializer.Serialize(schema, SerializerOptions);

    public void SerializeToFile(SchemaModel schema, string filePath) =>
        File.WriteAllText(filePath, Serialize(schema));
}
