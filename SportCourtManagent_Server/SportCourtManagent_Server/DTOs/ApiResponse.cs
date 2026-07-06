namespace SportCourtManagent_Server.DTOs;

/// <summary>
/// Standard API response wrapper used by every endpoint.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; }

    /// <summary>Creates a successful response.</summary>
    public static ApiResponse<T> Ok(T? data, string message = "Success", int statusCode = 200)
        => new() { Success = true, Message = message, Data = data, StatusCode = statusCode };

    /// <summary>Creates a 201 Created response.</summary>
    public static ApiResponse<T> Created(T? data, string message = "Created successfully")
        => new() { Success = true, Message = message, Data = data, StatusCode = 201 };

    /// <summary>Creates a failure response.</summary>
    public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
        => new() { Success = false, Message = message, Data = default, Errors = errors, StatusCode = statusCode };
}
