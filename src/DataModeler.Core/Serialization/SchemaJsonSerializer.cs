using System.IO;
using System.Runtime.Serialization.Json;
using DataModeler.Core.Models;

namespace DataModeler.Core.Serialization
{
    public sealed class SchemaJsonSerializer
    {
        public SchemaModel DeserializeFromFile(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var serializer = new DataContractJsonSerializer(typeof(SchemaModel));
                return (SchemaModel)serializer.ReadObject(stream);
            }
        }

        public void SerializeToFile(SchemaModel schema, string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                var serializer = new DataContractJsonSerializer(typeof(SchemaModel));
                serializer.WriteObject(stream, schema);
            }
        }
    }
}
