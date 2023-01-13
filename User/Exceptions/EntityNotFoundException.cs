namespace User.Exceptions;

public class EntityNotFoundException : ApiException
{
    public EntityNotFoundException(int httpStatusCode = StatusCodes.Status400BadRequest) : base(httpStatusCode)
    {
    }
}