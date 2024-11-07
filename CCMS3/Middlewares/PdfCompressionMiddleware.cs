namespace CCMS3.Middlewares
{
    using iText.Kernel.Pdf;
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using System.Threading.Tasks;

    public class PdfCompressionMiddleware
    {
        private readonly RequestDelegate _next;

        public PdfCompressionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post &&
            context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                var file = form.Files.GetFile("pdfFile");

                if (file != null && file.ContentType == "application/pdf")
                {
                    try
                    {
                        byte[] compressedBytes;

                        using (var inputStream = file.OpenReadStream())
                        {
                            using var tempStream = new MemoryStream();
                            await inputStream.CopyToAsync(tempStream);

                            using var outputStream = new MemoryStream();
                            using (var reader = new PdfReader(new MemoryStream(tempStream.ToArray())))
                            using (var writer = new PdfWriter(outputStream, new WriterProperties().SetCompressionLevel(CompressionConstants.BEST_COMPRESSION)))
                            {
                                using var pdfDoc = new PdfDocument(reader, writer);

                                for (int i = 1; i < pdfDoc.GetNumberOfPages(); i++)
                                {
                                    var resources = pdfDoc.GetPage(i).GetResources();
                                    var imageNames = resources.GetResourceNames();

                                    foreach (var name in imageNames)
                                    {
                                        var img = resources.GetImage(name);
                                        if (img != null)
                                        {
                                            img.GetPdfObject().SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);
                                        }
                                    }
                                }
                                pdfDoc.Close();
                            }

                            compressedBytes = outputStream.ToArray();
                        }

                        // Create a new MemoryStream from compressed bytes and store in context
                        var compressedStream = new MemoryStream(compressedBytes);
                        context.Items["CompressedPdfStream"] = compressedStream;
                        context.Items["FileName"] = file.FileName;
                        context.Items["OriginalFileSize"] = file.Length;
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync($"Error processing PDF: {ex.Message}");
                        return;
                    }
                }
            }
            await _next(context);

        }
    }

}
