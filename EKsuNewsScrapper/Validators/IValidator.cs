using NLog;

namespace EKsuNewsScrapper.Validators;

public interface IValidator<T>
{
    public bool IsValid(T entity, Uri requestUri);
}