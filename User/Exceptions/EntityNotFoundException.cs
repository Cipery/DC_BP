namespace User.Exceptions;

public class EntityNotFoundException : ApiException
{
    public EntityNotFoundException(int httpStatusCode = StatusCodes.Status404NotFound) : base(httpStatusCode)
    {
    }
}