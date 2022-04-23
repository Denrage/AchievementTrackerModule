using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    public class CollectionAchievementTableEntryConverter : JsonConverter<CollectionAchievementTable.CollectionAchievementTableEntry>
    {
        private const string TypeValuePropertyName = "TypeValue";

        public override bool CanConvert(Type typeToConvert)
            => typeof(CollectionAchievementTable.CollectionAchievementTableEntry).IsAssignableFrom(typeToConvert);

        public override CollectionAchievementTable.CollectionAchievementTableEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            CollectionAchievementTable.CollectionAchievementTableEntry ParseEntry<T>(ref Utf8JsonReader jsonReader)
                where T : CollectionAchievementTable.CollectionAchievementTableEntry
            {
                if (!jsonReader.Read() || jsonReader.GetString() != TypeValuePropertyName)
                {
                    throw new JsonException();
                }
                if (!jsonReader.Read() || jsonReader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var result = (T)JsonSerializer.Deserialize(ref jsonReader, typeof(T));

                if (result is null)
                {
                    throw new JsonException();
                }

                return result;
            }

            CollectionAchievementTable.CollectionAchievementTableEntry entry;
            var typeDiscriminator = (TypeDiscriminator)reader.GetInt32();

            switch (typeDiscriminator)
            {
                case TypeDiscriminator.Number:
                    entry = ParseEntry<CollectionAchievementTable.CollectionAchievementTableNumberEntry>(ref reader);
                    break;
                case TypeDiscriminator.Coin:
                    entry = ParseEntry<CollectionAchievementTable.CollectionAchievementTableCoinEntry>(ref reader);
                    break;
                case TypeDiscriminator.Item:
                    entry = ParseEntry<CollectionAchievementTable.CollectionAchievementTableItemEntry>(ref reader);
                    break;
                case TypeDiscriminator.Link:
                    entry = ParseEntry<CollectionAchievementTable.CollectionAchievementTableLinkEntry>(ref reader);
                    break;
                case TypeDiscriminator.Map:
                    entry = ParseEntry<CollectionAchievementTable.CollectionAchievementTableMapEntry>(ref reader);
                    break;
                case TypeDiscriminator.String:
                    entry = ParseEntry<CollectionAchievementTable.CollectionAchievementTableStringEntry>(ref reader);
                    break;
                case TypeDiscriminator.Empty:
                    entry = ParseEntry<CollectionAchievementTable.CollectionAchievementTableEmptyEntry>(ref reader);
                    break;
                default:
                    entry = null;
                    break;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return entry;
        }

        public override void Write(Utf8JsonWriter writer, CollectionAchievementTable.CollectionAchievementTableEntry value, JsonSerializerOptions options)
        {
            void WriteTypeDiscriminator<T>(Utf8JsonWriter jsonWriter, T reward, TypeDiscriminator typeDiscriminator)
                where T : CollectionAchievementTable.CollectionAchievementTableEntry
            {

                jsonWriter.WriteNumber(nameof(TypeDiscriminator), (int)typeDiscriminator);
                jsonWriter.WritePropertyName(TypeValuePropertyName);
                JsonSerializer.Serialize(jsonWriter, reward);
            }

            writer.WriteStartObject();


            switch (value)
            {
                case CollectionAchievementTable.CollectionAchievementTableNumberEntry numberEntry:
                    WriteTypeDiscriminator(writer, numberEntry, TypeDiscriminator.Number);
                    break;
                case CollectionAchievementTable.CollectionAchievementTableCoinEntry coinEntry:
                    WriteTypeDiscriminator(writer, coinEntry, TypeDiscriminator.Coin);
                    break;
                case CollectionAchievementTable.CollectionAchievementTableItemEntry itemEntry:
                    WriteTypeDiscriminator(writer, itemEntry, TypeDiscriminator.Item);
                    break;
                case CollectionAchievementTable.CollectionAchievementTableLinkEntry linkEntry:
                    WriteTypeDiscriminator(writer, linkEntry, TypeDiscriminator.Link);
                    break;
                case CollectionAchievementTable.CollectionAchievementTableMapEntry mapEntry:
                    WriteTypeDiscriminator(writer, mapEntry, TypeDiscriminator.Map);
                    break;
                case CollectionAchievementTable.CollectionAchievementTableStringEntry stringEntry:
                    WriteTypeDiscriminator(writer, stringEntry, TypeDiscriminator.String);
                    break;
                case CollectionAchievementTable.CollectionAchievementTableEmptyEntry empyEntry:
                    WriteTypeDiscriminator(writer, empyEntry, TypeDiscriminator.Empty);
                    break;
                default:
                    break;
            }

            writer.WriteEndObject();
        }

        private enum TypeDiscriminator
        {
            Number = 0,
            Coin = 1,
            Item = 2,
            Link = 3,
            Map = 4,
            String = 5,
            Empty = 6,
        }
    }
}
