using Chic.Constraints;

namespace Chic.Abstractions
{
    /// <summary>
    /// IRepository instance designed to be Scoped.
    /// </summary>
    /// <typeparam name="TModel">Database Model</typeparam>
    public interface IRepository<TModel> : IRepository<TModel, int>
        where TModel : class, IKeyedEntity
    {
    }
}
