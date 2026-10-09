namespace AssetHub.Core.Common;

/// <summary>Thrown by services for expected, client-facing failures (404 / 400 / 409 style errors).</summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    public static ApiException NotFound(string entity, object key) =>
        new($"{entity} '{key}' was not found.", 404);
}
