using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StajP
{
    // NetTopologySuite.Geometries.Geometry sınıfı için özel bir JSON dönüştürücüsü.
    // Bu dönüştürücü, Geometry nesnesini JSON çıktısında WKT (Well-Known Text) formatına dönüştürür.
    public class GeometryJsonConverter : JsonConverter<Geometry>
    {
        private static readonly WKTReader WktReader = new WKTReader();
        private static readonly WKTWriter WktWriter = new WKTWriter();

        // JSON çıktısına yazma işlemi
        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Geometry nesnesini WKT formatında bir stringe dönüştürün
            var wktString = WktWriter.Write(value);
            writer.WriteStringValue(wktString);
        }

        // JSON çıktısından okuma işlemi (deserileştirme)
        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("WKT verisi bir string olmalıdır.");
            }

            var wktString = reader.GetString();

            if (string.IsNullOrEmpty(wktString))
            {
                return null;
            }

            try
            {
                // WKT stringini Geometry nesnesine dönüştürün
                return WktReader.Read(wktString);
            }
            catch (ParseException ex)
            {
                throw new JsonException($"Geçersiz WKT formatı: {wktString}", ex);
            }
        }
    }
}
