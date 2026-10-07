using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using JEO3.Schema;
using Microsoft.Extensions.Caching.Hybrid;

namespace JEO3.Site.Infrastructure
{
    public class PayloadSerializer : IHybridCacheSerializer<DatabaseContext>
    {
        #region Properties

        // JsonSerializerOptions is a heavy object that builds an internal reflection/converter
        // cache on first use. Making it static readonly means it is created once per AppDomain
        // and all subsequent serializations reuse the already-warmed cache.
        private static readonly JsonSerializerOptions _options = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            MaxDepth = 128
        };

        #endregion

        #region Serialization / Deserialization

        public DatabaseContext Deserialize(ReadOnlySequence<byte> source)
        {
            // Create a reader state tracking block depth constraint
            var state = new JsonReaderState(new JsonReaderOptions
            {
                MaxDepth = 128 // This breaks the 64 safety lock right at the stream root!
            });
            var reader = new Utf8JsonReader(source, isFinalBlock: true, state);
            return JsonSerializer.Deserialize<DatabaseContext>(ref reader, _options)
                ?? throw new InvalidOperationException("Failed to deserialize schema payload graph.");
        }

        public void Serialize(DatabaseContext value, IBufferWriter<byte> target)
        {
            using var writer = new Utf8JsonWriter(target);
            JsonSerializer.Serialize(writer, value, _options);
        }

        #endregion
    }
}
