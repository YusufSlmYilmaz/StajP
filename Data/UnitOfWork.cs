using StajP.Data.Repositories;
using StajP.Interfaces;
using ObjectEntity = StajP.Entities.Object;

namespace StajP.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ObjectDbContext _context;
        private IGenericRepository<ObjectEntity> _objects;

        public UnitOfWork(ObjectDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<ObjectEntity> Objects => _objects ??= new GenericRepository<ObjectEntity>(_context);

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}