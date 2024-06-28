using collector.Domain;

namespace collector.Repository
{
    public interface IDataBase
    {
        Task SaveRequestAsync(RequestHistory request);
        Task<IEnumerable<RequestHistory>> GetRequestsByUserIdAsync(string userId);
    }
}