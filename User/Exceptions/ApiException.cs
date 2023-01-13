namespace User.Exceptions;

public class ApiException : Exception
{
    public int HttpStatusCode { get; private set; }

    public ApiException(int httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
    }

}