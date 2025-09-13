using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using System.Text.Json;

namespace UserApi.Infrastructure
{
    /// <summary>
    /// Custom JSON Output Formatter to fix .NET 9 PipeWriter compatibility issue.
    /// Uses Response.Body (Stream) instead of Response.BodyWriter (PipeWriter) to avoid
    /// the "PipeWriter does not implement PipeWriter.UnflushedBytes" error.
    /// </summary>
    public class CompatibleSystemTextJsonOutputFormatter : TextOutputFormatter
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public CompatibleSystemTextJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();

            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("text/json");
            SupportedMediaTypes.Add("application/*+json");

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;

            // Use Response.Body (Stream) instead of Response.BodyWriter (PipeWriter) to avoid .NET 9 issue
            await using var writer = new StreamWriter(response.Body, selectedEncoding, leaveOpen: true);
            var json = JsonSerializer.Serialize(context.Object, context.ObjectType ?? typeof(object), _jsonSerializerOptions);
            await writer.WriteAsync(json);
            await writer.FlushAsync();
        }
    }
}
