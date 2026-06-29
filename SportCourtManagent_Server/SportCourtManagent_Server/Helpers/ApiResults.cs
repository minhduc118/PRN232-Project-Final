namespace SportCourtManagent_Server.Helpers
{
    public static class ApiResults
    {
        public static object Ok(object? data, string? message = null, int statusCode = 200) => new
        {
            success = true,
            data,
            message,
            errors = (object?)null,
            statusCode
        };

        public static object Fail(string message, int statusCode = 400) => new
        {
            success = false,
            data = (object?)null,
            message,
            errors = (object?)null,
            statusCode
        };
    }
}
