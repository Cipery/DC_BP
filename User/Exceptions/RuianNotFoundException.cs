namespace User.Exceptions;

public class RuianNotFoundException : ApiException
{
    public RuianNotFoundException(int httpStatusCode = StatusCodes.Status500InternalServerError) : base(httpStatusCode)
    {
    }
}