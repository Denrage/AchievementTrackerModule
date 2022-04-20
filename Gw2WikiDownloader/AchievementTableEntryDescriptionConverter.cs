// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Gw2WikiDownload
{
    public class AchievementTableEntryDescriptionConverter : JsonConverter<AchievementTableEntryDescription>
    {
        private const string TypeValuePropertyName = "TypeValue";
        public override bool CanConvert(Type typeToConvert)
            => typeof(AchievementTableEntryDescription).IsAssignableFrom(typeToConvert);

        public override AchievementTableEntryDescription? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != nameof(TypeDiscriminator))
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }

            static AchievementTableEntryDescription ParseDescription<T>(ref Utf8JsonReader reader)
                where T : AchievementTableEntryDescription
            {
                if (!reader.Read() || reader.GetString() != TypeValuePropertyName)
                {
                    throw new JsonException();
                }
                if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var result = (T?)JsonSerializer.Deserialize(ref reader, typeof(T));

                if (result is null)
                {
                    throw new JsonException();
                }

                return result;
            }

            AchievementTableEntryDescription description;
            TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
            switch (typeDiscriminator)
            {
                case TypeDiscriminator.String:
                    description = ParseDescription<StringDescription>(ref reader);
                    break;
                case TypeDiscriminator.Objective:
                    description = ParseDescription<ObjectivesDescription>(ref reader);
                    break;
                case TypeDiscriminator.Collection:
                    description = ParseDescription<CollectionDescription>(ref reader);
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return description;
        }

        public override void Write(Utf8JsonWriter writer, AchievementTableEntryDescription value, JsonSerializerOptions options)
        {
            void WriteTypeDiscriminator<T>(Utf8JsonWriter writer, T description, TypeDiscriminator typeDiscriminator)
                where T : AchievementTableEntryDescription
            {
                writer.WriteNumber(nameof(TypeDiscriminator), (int)typeDiscriminator);
                writer.WritePropertyName(TypeValuePropertyName);
                JsonSerializer.Serialize(writer, description);
            }

            writer.WriteStartObject();

            switch (value)
            {
                case StringDescription stringDescription:
                    WriteTypeDiscriminator(writer, stringDescription, TypeDiscriminator.String);
                    break;
                case ObjectivesDescription objectivesDescription:
                    WriteTypeDiscriminator(writer, objectivesDescription, TypeDiscriminator.Objective);
                    break;
                case CollectionDescription collectionDescription:
                    WriteTypeDiscriminator(writer, collectionDescription, TypeDiscriminator.Collection);
                    break;
                default:
                    throw new NotSupportedException();
            }

            writer.WriteEndObject();
        }

        private enum TypeDiscriminator
        {
            String = 0,
            Objective = 1,
            Collection = 2,
        }
    }
}
