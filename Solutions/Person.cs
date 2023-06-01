using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonString = @"{""name"": ""John"", ""age"": 30, ""address"": {""city"": ""New York"", ""state"": ""NY""}}";

            var options = new JsonSerializerOptions();
            options.Converters.Add(new PersonConverter());

            var person = JsonSerializer.Deserialize<Person>(jsonString, options);

            Console.WriteLine(person.Name);
            Console.WriteLine(person.Age);
            Console.WriteLine(person.Address.City);
            Console.WriteLine(person.Address.State);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }
    }

    public class PersonConverter : JsonConverter<Person>
    {
        public override Person Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string name = null;
            int age = 0;
            Address address = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();

                    switch (propertyName)
                    {
                        case "name":
                            name = reader.GetString();
                            break;

                        case "age":
                            age = reader.GetInt32();
                            break;

                        case "address":
                            address = JsonSerializer.Deserialize<Address>(ref reader, options);
                            break;
                    }
                }
            }

            return new Person { Name = name, Age = age, Address = address };
        }

        public override void Write(Utf8JsonWriter writer, Person value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("name", value.Name);
            writer.WriteNumber("age", value.Age);

            writer.WritePropertyName("address");
            JsonSerializer.Serialize(writer, value.Address, options);

            writer.WriteEndObject();
        }
    }
}
