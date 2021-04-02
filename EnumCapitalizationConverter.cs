using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MailSort
{
	public class EnumCapitalizationConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
	{
		public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var s = reader.GetString();
			if (Enum.TryParse(s?.Capitalize(), out TEnum @enum))
			{
				return @enum;
			}

			return default;
		}

		public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}