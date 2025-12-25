<!-- HttpRequestExtensions -->

##### HttpRequestExtensions

````csharp

// Middleware to enable buffering for request body
// wenn RawBody gelesen werden soll, z.B. um Webhook Payloads zu loggen/verarbeiten
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering(); // Allows request body to be read multiple times
    await next();
});


public static class HttpRequestExtensions
{
    /// <summary>
    /// Retrieve the raw body as a string from the Request.Body stream
    /// </summary>
    /// <param name="request">Request instance to apply to</param>
    /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
    /// <returns></returns>
    public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding)
    {
        using (StreamReader reader = new StreamReader(request.Body, encoding))
            return await reader.ReadToEndAsync();
    }

    // <summary>
    /// Retrieve the raw body as a string from the Request.Body stream
    /// </summary>
    /// <param name="request">Request instance to apply to</param>
    /// <returns></returns>
    public static async Task<string> GetRawBodyStringAsync(this HttpRequest request)
    {
        var encoding = Encoding.UTF8;

        using (StreamReader reader = new StreamReader(request.Body, encoding))
            return await reader.ReadToEndAsync();
    }


    /// <summary>
    /// Retrieves the raw body as a byte array from the Request.Body stream
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<byte[]> GetRawBodyBytesAsync(this HttpRequest request)
    {
        using (var ms = new MemoryStream(2048))
        {
            await request.Body.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}

````

