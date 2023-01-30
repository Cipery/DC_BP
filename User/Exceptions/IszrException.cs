namespace User.Exceptions;

public class IszrException : ApiException
{
    public IszrException(int httpStatusCode) : base(httpStatusCode)
    {
    }
}