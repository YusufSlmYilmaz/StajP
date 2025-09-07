using ObjectEntity = StajP.Entities.Object;

namespace StajP.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<ObjectEntity> Objects { get; }
        int Complete();
    }
}