namespace EKsuNewsScrapperService.Validators;

public interface IValidator<T>
{
    public bool IsValid(T entity, Uri requestUri);
}