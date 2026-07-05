using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);

webApplicationBuilder.Services.AddControllers();

// Compress streamed binary glTF payloads (their JSON chunk with object properties compresses very well).
webApplicationBuilder.Services.AddResponseCompression(responseCompressionOptions =>
{
    responseCompressionOptions.EnableForHttps = true;
    responseCompressionOptions.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["model/gltf-binary"]);
});

// Register all IGLTFNodeConverter implementations of this assembly with the generic DiGi.GLTF
// engine (plugin pattern): adding support for a new domain type only requires adding a new
// converter class under /Classes/Converter - no other code changes.
DiGi.GLTF.Modify.Register(typeof(Program).Assembly);

WebApplication webApplication = webApplicationBuilder.Build();

webApplication.UseResponseCompression();
webApplication.MapControllers();

webApplication.Run();
