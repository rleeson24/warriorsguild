using System.Threading.Tasks;

namespace WarriorsGuild.DataAccess
{
    /// <summary>
    /// Coordinates persistence for a logical unit of work. Use when a process updates the database
    /// via multiple repositories and requires all operations to run in a single transaction.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Persists all changes made in this scope to the database.
        /// Call once after all repository operations for a cross-repository transaction.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
