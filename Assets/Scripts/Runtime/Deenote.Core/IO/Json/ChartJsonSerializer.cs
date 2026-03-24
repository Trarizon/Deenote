#nullable enable

using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Deenote.CoreB.IO.Json
{
    internal static class ChartJsonSerializer
    {
        private static readonly JsonSerializerSettings DeemoV2Settings = new() {
            ContractResolver = new ContractResolver(ChartSerializationVersions.DeemoV2, JsonPropertyHandlers.IgnoreEmptySounds)
        };
        private static readonly JsonSerializerSettings DeemoIIV2Settings = new() {
            ContractResolver = new ContractResolver(ChartSerializationVersions.DeemoIIV2, JsonPropertyHandlers.EmptySoundsToNull)
        };

        public static string Serialize(ChartData chart, ChartVersion version)
        {
            var settings = version switch {
                ChartVersion.DeemoV2 => DeemoV2Settings,
                ChartVersion.DeemoIIV2 => DeemoIIV2Settings,
                _ => throw new SwitchExpressionException(version),
            };
            return JsonConvert.SerializeObject(chart, settings);
        }

        public static ChartData? Deserialize(string json)
        {
            try {
                return JsonConvert.DeserializeObject<ChartData>(json);
            } catch (Exception) {
                //Debug.LogError("Exception on parse chart json");
                /* ignored */
            }

            try {
                return ChartJsonAdapter.ParseDeV3Json(json);
            } catch (Exception) {
                //Debug.LogError("Exception on parse v3 chart json");
                /* ignored */
            }

            return null;
        }

        private sealed class ContractResolver : DefaultContractResolver
        {
            private readonly ChartSerializationVersions _version;
            private readonly Action<JsonProperty>? _jpropHandler;

            public ContractResolver(ChartSerializationVersions versions, Action<JsonProperty>? jsonPropertyHandler)
            {
                _version = versions;
                _jpropHandler = jsonPropertyHandler;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var jp = base.CreateProperty(member, memberSerialization);
                var attr = member.GetCustomAttribute<ChartSerializationAttribute>();

                if (attr?.Versions.HasFlag(_version) is false) {
                    jp.ShouldSerialize = _ => false;
                }
                else {
                    _jpropHandler?.Invoke(jp);
                }
                return jp;
            }
        }

        private static class JsonPropertyHandlers
        {
            public static readonly Action<JsonProperty> IgnoreEmptySounds = jp =>
            {
                if (jp.PropertyName == "sounds")
                    jp.ShouldSerialize = o => o is not NoteData note || note.HasSounds;
            };

            public static readonly Action<JsonProperty> EmptySoundsToNull = jp =>
            {
                if (jp.PropertyName == "sounds")
                    jp.Converter = EmptyListToNullConverter<PianoSoundData>.Instance;
            };

            private sealed class EmptyListToNullConverter<T> : JsonConverter
            {
                public static EmptyListToNullConverter<T> Instance = new();

                public override bool CanConvert(Type objectType) => true;
                public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
                    => existingValue;
                public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
                    => serializer.Serialize(writer, ((List<T>)value!).Count > 0 ? value : null);
            }
        }
    }
}
