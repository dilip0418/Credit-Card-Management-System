using Azure;
using CCMS3.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.OpenApi.Validations.Rules;


namespace CCMS3.Controllers
{
    public static class DemoEndpoint
    {

        public static IEndpointRouteBuilder MapAuthorizationDemoEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/Authorized/{name}", HelloAuthorized);

            app.MapPost("/upload",  SaveCompressedPdf)
                .WithName("SaveCompressedPdf");

            return app;
        }

        private static string HelloAuthorized(string name)
        { return "hello " + name; }



        
        private static async Task<IResult> SaveCompressedPdf(HttpContext context, FileStorageService fileStorageService)
        {
            if (context.Items["CompressedPdfStream"] is MemoryStream compressedPdfStream)
            {
                try
                {
                    var fileName = $"{Guid.NewGuid()}_{context.Items["FileName"]}"; // Generate a unique file name

                    // Save the compressed PDF using the file storage service
                    await fileStorageService.SaveFileAsync(compressedPdfStream, fileName);

                    return Results.Ok(new
                    {
                        message = "Compressed PDF uploaded and saved successfully.",
                        fileName,
                        originalFileSize = context.Items["OriginalFileSize"],
                        compressedFileSize = compressedPdfStream.Length
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error saving PDF",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
                finally
                {
                    // Dispose the stream after saving
                    await compressedPdfStream.DisposeAsync();
                }
            }
            return Results.BadRequest("No valid PDF file was uploaded.");
        }
    }
}
