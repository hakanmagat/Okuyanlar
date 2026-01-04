using Okuyanlar.Core.Entities;

namespace Okuyanlar.Core.Interfaces
{
  /// <summary>
  /// Defines the contract for data persistence operations related to Users.
  /// Decouples the business logic from the specific database implementation.
  /// </summary>
  public interface IUserRepository
  {
    /// <summary>
    /// Persists a new user entity to the underlying data store.
    /// </summary>
    void Add(User user);

    /// <summary>
    /// Updates an existing user's information.
    /// </summary>
    void Update(User user);

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <returns>The User entity if found; otherwise, null.</returns>
    User? GetByEmail(string email);
    User? GetById(int id);
        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <returns>The User entity if found; otherwise, null.</returns>
        User? GetByUsername(string username);

        IEnumerable<User> GetAll();

    }
}

