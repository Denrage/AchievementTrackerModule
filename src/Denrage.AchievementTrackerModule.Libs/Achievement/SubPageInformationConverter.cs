using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Denrage.AchievementTrackerModule.Libs.Achievement
{
    public class SubPageInformationConverter : JsonConverter<SubPageInformation>
    {
        private const string TypeValuePropertyName = "TypeValue";

        public override bool CanConvert(Type typeToConvert)
            => typeof(SubPageInformation).IsAssignableFrom(typeToConvert);

        public override SubPageInformation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            SubPageInformation ParseEntry<T>(ref Utf8JsonReader jsonReader)
                where T : SubPageInformation
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

                return result is null ? throw new JsonException() : (SubPageInformation)result;
            }

            SubPageInformation entry;
            var typeDiscriminator = (TypeDiscriminator)reader.GetInt32();

            switch (typeDiscriminator)
            {
                case TypeDiscriminator.Item:
                    entry = ParseEntry<ItemSubPageInformation>(ref reader);
                    break;
                case TypeDiscriminator.Location:
                    entry = ParseEntry<LocationSubPageInformation>(ref reader);
                    break;
                case TypeDiscriminator.Npc:
                    entry = ParseEntry<NpcSubPageInformation>(ref reader);
                    break;
                case TypeDiscriminator.Quest:
                    entry = ParseEntry<QuestSubPageInformation>(ref reader);
                    break;
                case TypeDiscriminator.Text:
                    entry = ParseEntry<TextSubPageInformation>(ref reader);
                    break;
                default:
                    entry = null;
                    break;
            }

            return !reader.Read() || reader.TokenType != JsonTokenType.EndObject ? throw new JsonException() : entry;
        }

        public override void Write(Utf8JsonWriter writer, SubPageInformation value, JsonSerializerOptions options)
        {
            void WriteTypeDiscriminator<T>(Utf8JsonWriter jsonWriter, T reward, TypeDiscriminator typeDiscriminator)
                where T : SubPageInformation
            {
                jsonWriter.WriteNumber(nameof(TypeDiscriminator), (int)typeDiscriminator);
                jsonWriter.WritePropertyName(TypeValuePropertyName);
                JsonSerializer.Serialize(jsonWriter, reward);
            }

            writer.WriteStartObject();

            switch (value)
            {
                case ItemSubPageInformation itemSubPage:
                    WriteTypeDiscriminator(writer, itemSubPage, TypeDiscriminator.Item);
                    break;
                case LocationSubPageInformation locationSubPage:
                    WriteTypeDiscriminator(writer, locationSubPage, TypeDiscriminator.Location);
                    break;
                case NpcSubPageInformation npcSubPage:
                    WriteTypeDiscriminator(writer, npcSubPage, TypeDiscriminator.Npc);
                    break;
                case QuestSubPageInformation questSubPage:
                    WriteTypeDiscriminator(writer, questSubPage, TypeDiscriminator.Quest);
                    break;
                case TextSubPageInformation textSubPage:
                    WriteTypeDiscriminator(writer, textSubPage, TypeDiscriminator.Text);
                    break;
                default:
                    break;
            }

            writer.WriteEndObject();
        }

        private enum TypeDiscriminator
        {
            Item = 0,
            Location = 1,
            Npc = 2,
            Quest = 3,
            Text = 4,
        }
    }
}
