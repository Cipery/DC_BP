using System.Net;

namespace User.Exceptions;

public class ApiException : Exception
{
    public int HttpStatusCode { get; private set; }

    public ApiException(int httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
    }

    public ApiException(HttpStatusCode httpStatusCode, string message) : base(message)
    {
        HttpStatusCode = (int)httpStatusCode;
    }
}