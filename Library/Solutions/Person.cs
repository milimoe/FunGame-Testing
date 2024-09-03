using System.Text.Json;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Library.Common.Architecture;

namespace ConverterExample
{
    public class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public Address Address { get; set; } = new();
    }

    public class Address
    {
        public string State { get; set; } = "";
        public string City { get; set; } = "";
    }

    public class AddressConverter : BaseEntityConverter<Address>
    {
        public override Address NewInstance()
        {
            return new();
        }

        public override void ReadPropertyName(ref Utf8JsonReader reader, string propertyName, JsonSerializerOptions options, ref Address result)
        {
            result ??= new();
            switch (propertyName)
            {
                case "city":
                    result.City = reader.GetString() ?? "";
                    break;

                case "state":
                    result.State = reader.GetString() ?? "";
                    break;
            }
        }

        public override void Write(Utf8JsonWriter writer, Address value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("state", value.State);
            writer.WriteString("city", value.City);

            writer.WriteEndObject();
        }
    }

    public class PersonConverter : BaseEntityConverter<Person>
    {
        public override Person NewInstance()
        {
            return new();
        }

        public override void ReadPropertyName(ref Utf8JsonReader reader, string propertyName, JsonSerializerOptions options, ref Person result)
        {
            result ??= new();
            switch (propertyName)
            {
                case "name":
                    result.Name = reader.GetString() ?? "";
                    break;

                case "age":
                    result.Age = reader.GetInt32();
                    break;

                case "address":
                    result.Address = NetworkUtility.JsonDeserialize<Address>(reader.GetString() ?? "", options) ?? new Address();
                    break;
            }
        }

        public override void Write(Utf8JsonWriter writer, Person value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("name", value.Name);
            writer.WriteNumber("age", value.Age);
            writer.WriteString("address", NetworkUtility.JsonSerialize(value.Address, options));

            writer.WriteEndObject();
        }
    }
}
