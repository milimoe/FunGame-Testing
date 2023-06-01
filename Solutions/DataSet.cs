using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DataSetJsonConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Person p1 = new(1, "YES", DateTime.Now);
            Person p2 = new(2, "NO", DateTime.Now);

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            options.Converters.Add(new DataSetConverter());
            options.Converters.Add(new DateTimeConverter());

            var jsonString = JsonSerializer.Serialize(p1, options);
            jsonString += JsonSerializer.Serialize(p2, options);

            jsonString = "[" + jsonString.Replace("}{", "},{") + "]";

            var people = JsonSerializer.Deserialize<Person[]>(jsonString, options);

            foreach (var person in people)
            {
                Console.WriteLine(person.Name);
            }
        }
    }

    public class DataSetConverter : JsonConverter<DataSet>
    {
        private readonly string _format = "yyyy-MM-dd hh:mm:ss.fff";

        public override DataSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dataSet = new DataSet();
            var dataTable = new DataTable();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();

                    switch (propertyName)
                    {
                        case "TableName":
                            reader.Read();
                            string tableName = reader.GetString();
                            dataTable = new DataTable(tableName);
                            dataSet.Tables.Add(dataTable);
                            break;

                        case "Columns":
                            reader.Read();
                            ReadColumns(reader, dataTable);
                            break;

                        case "Rows":
                            reader.Read();
                            ReadRows(reader, dataTable);
                            break;
                    }
                }
            }

            return dataSet;
        }

        public override void Write(Utf8JsonWriter writer, DataSet value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("TableName", value.Tables[0].TableName);

            writer.WritePropertyName("Columns");
            writer.WriteStartArray();

            foreach (DataColumn column in value.Tables[0].Columns)
            {
                writer.WriteStartObject();

                writer.WriteString("ColumnName", column.ColumnName);
                writer.WriteString("DataType", column.DataType.FullName);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            writer.WritePropertyName("Rows");
            writer.WriteStartArray();

            foreach (DataRow row in value.Tables[0].Rows)
            {
                writer.WriteStartArray();

                for (int i = 0; i < value.Tables[0].Columns.Count; i++)
                {
                    var rowValue = row[i];

                    switch (value.Tables[0].Columns[i].DataType.FullName)
                    {
                        case "System.Boolean":
                            writer.WriteBooleanValue((bool)rowValue);
                            break;

                        case "System.Byte":
                            writer.WriteNumberValue((byte)rowValue);
                            break;

                        case "System.Char":
                            writer.WriteStringValue(value.ToString());
                            break;

                        case "System.DateTime":
                            writer.WriteStringValue(((DateTime)rowValue).ToString(_format));
                            break;

                        case "System.Decimal":
                            writer.WriteNumberValue((decimal)rowValue);
                            break;

                        case "System.Double":
                            writer.WriteNumberValue((double)rowValue);
                            break;

                        case "System.Guid":
                            writer.WriteStringValue(value.ToString());
                            break;

                        case "System.Int16":
                            writer.WriteNumberValue((short)rowValue);
                            break;

                        case "System.Int32":
                            writer.WriteNumberValue((int)rowValue);
                            break;

                        case "System.Int64":
                            writer.WriteNumberValue((long)rowValue);
                            break;

                        case "System.SByte":
                            writer.WriteNumberValue((sbyte)rowValue);
                            break;

                        case "System.Single":
                            writer.WriteNumberValue((float)rowValue);
                            break;

                        case "System.String":
                            writer.WriteStringValue((string)rowValue);
                            break;

                        case "System.UInt16":
                            writer.WriteNumberValue((ushort)rowValue);
                            break;

                        case "System.UInt32":
                            writer.WriteNumberValue((uint)rowValue);
                            break;

                        case "System.UInt64":
                            writer.WriteNumberValue((ulong)rowValue);
                            break;
                    }
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        private void ReadColumns(Utf8JsonReader reader, DataTable dataTable)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var column = new DataColumn();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            string propertyName = reader.GetString();

                            switch (propertyName)
                            {
                                case "ColumnName":
                                    reader.Read();
                                    column.ColumnName = reader.GetString();
                                    break;

                                case "DataType":
                                    reader.Read();
                                    Type dataType = Type.GetType(reader.GetString());
                                    column.DataType = dataType;
                                    break;
                            }
                        }

                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }
                    }

                    dataTable.Columns.Add(column);
                }
            }
        }

        private void ReadRows(Utf8JsonReader reader, DataTable dataTable)
        {
            var values = new object[dataTable.Columns.Count];

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    int index = 0;

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            break;
                        }

                        switch (dataTable.Columns[index].DataType.ToString())
                        {
                            case "System.Boolean":
                                values[index] = reader.GetBoolean();
                                break;

                            case "System.Byte":
                                values[index] = reader.GetByte();
                                break;

                            case "System.Char":
                                values[index] = reader.GetString()[0];
                                break;

                            case "System.DateTime":
                                string dateString = reader.GetString();
                                if (DateTime.TryParseExact(dateString, _format, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                                {
                                    values[index] = result;
                                }
                                break;

                            case "System.Decimal":
                                values[index] = reader.GetDecimal();
                                break;

                            case "System.Double":
                                values[index] = reader.GetDouble();
                                break;

                            case "System.Guid":
                                values[index] = Guid.Parse(reader.GetString());
                                break;

                            case "System.Int16":
                                values[index] = reader.GetInt16();
                                break;

                            case "System.Int32":
                                values[index] = reader.GetInt32();
                                break;

                            case "System.Int64":
                                values[index] = reader.GetInt64();
                                break;

                            case "System.SByte":
                                values[index] = reader.GetSByte();
                                break;

                            case "System.Single":
                                values[index] = reader.GetSingle();
                                break;

                            case "System.String":
                                values[index] = reader.GetString();
                                break;

                            case "System.UInt16":
                                values[index] = reader.GetUInt16();
                                break;

                            case "System.UInt32":
                                values[index] = reader.GetUInt32();
                                break;

                            case "System.UInt64":
                                values[index] = reader.GetUInt64();
                                break;
                        }
                        index++;
                    }
                    dataTable.Rows.Add(values);
                }
            }
        }
    }

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _format = "yyyy-MM-dd hh:mm:ss.fff";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            string dateString = reader.GetString();

            if (DateTime.TryParseExact(dateString, _format, null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }

    public class Person
    {
        public int ID { get; }
        public string Name { get; }
        public DateTime Time { get; }
        public DataSet ds { get; }

        [JsonConstructor]
        public Person(int ID, string Name, DateTime Time)
        {
            this.ID = ID;
            this.Name = Name;
            this.Time = Time;
            ds = new DataSet();
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Time", typeof(DateTime));
            var dr = dt.NewRow();
            dr["ID"] = ID;
            dr["Name"] = Name;
            dr["Time"] = Time;
            dt.Rows.Add(dr);
            ds.Tables.Add(dt);
        }
    }
}
