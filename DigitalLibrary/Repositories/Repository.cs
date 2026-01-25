namespace DigitalLibrary.Repositories
{
    public interface Repository<T>
    {
     
        Task<bool> Add(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Update(T entity);
        Task<T?> Find(string id);
        Task<ICollection<T>> GetAllAsync();
        Task<int> GetMaxId();
    }
}
