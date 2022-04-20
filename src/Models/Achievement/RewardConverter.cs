using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    public class RewardConverter : JsonConverter<Reward>
    {
        private const string TypeValuePropertyName = "TypeValue";
        public override bool CanConvert(Type typeToConvert)
            => typeof(Reward).IsAssignableFrom(typeToConvert);

        public override Reward Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            Reward ParseReward<T>(ref Utf8JsonReader jsonReader)
                where T : Reward
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

            Reward reward;
            TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
            switch (typeDiscriminator)
            {
                case TypeDiscriminator.EmptyReward:
                    reward = ParseReward<EmptyReward>(ref reader);
                    break;
                case TypeDiscriminator.MultiTierReward:
                    reward = ParseReward<MultiTierReward>(ref reader);
                    break;
                case TypeDiscriminator.ItemReward:
                    reward = ParseReward<ItemReward>(ref reader);
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return reward;
        }

        public override void Write(Utf8JsonWriter writer, Reward value, JsonSerializerOptions options)
        {
            void WriteTypeDiscriminator<T>(Utf8JsonWriter jsonWriter, T reward, TypeDiscriminator typeDiscriminator)
                where T : Reward
            {

                jsonWriter.WriteNumber(nameof(TypeDiscriminator), (int)typeDiscriminator);
                jsonWriter.WritePropertyName(TypeValuePropertyName);
                JsonSerializer.Serialize(jsonWriter, reward);
            }

            writer.WriteStartObject();

            switch (value)
            {
                case MultiTierReward multiTierReward:
                    WriteTypeDiscriminator(writer, multiTierReward, TypeDiscriminator.MultiTierReward);
                    break;
                case ItemReward itemReward:
                    WriteTypeDiscriminator(writer, itemReward, TypeDiscriminator.ItemReward);
                    break;
                case EmptyReward emptyReward:
                    WriteTypeDiscriminator(writer, emptyReward, TypeDiscriminator.EmptyReward);
                    break;
                default:
                    throw new NotSupportedException();
            }

            writer.WriteEndObject();
        }

        private enum TypeDiscriminator
        {
            EmptyReward = 0,
            ItemReward = 1,
            MultiTierReward = 2,
        }
    }
}
