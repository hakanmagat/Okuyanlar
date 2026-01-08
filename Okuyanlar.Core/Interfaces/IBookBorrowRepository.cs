using Okuyanlar.Core.Entities;

namespace Okuyanlar.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing book borrows.
    /// </summary>
    public interface IBookBorrowRepository
    {
        /// <summary>
        /// Gets a borrow record by its ID.
        /// </summary>
        BookBorrow? GetById(int id);

        /// <summary>
        /// Gets all borrow records for a specific user.
        /// </summary>
        IEnumerable<BookBorrow> GetByUserId(int userId);

        /// <summary>
        /// Gets all active borrows for a specific user.
        /// </summary>
        IEnumerable<BookBorrow> GetActiveBorrowsByUserId(int userId);

        /// <summary>
        /// Gets all borrow records for a specific book.
        /// </summary>
        IEnumerable<BookBorrow> GetByBookId(int bookId);

        /// <summary>
        /// Gets all active borrows.
        /// </summary>
        IEnumerable<BookBorrow> GetAllActiveBorrows();

        /// <summary>
        /// Gets all return requests (pending approval).
        /// </summary>
        IEnumerable<BookBorrow> GetReturnRequests();

        /// <summary>
        /// Adds a new borrow record.
        /// </summary>
        void Add(BookBorrow borrow);

        /// <summary>
        /// Updates an existing borrow record.
        /// </summary>
        void Update(BookBorrow borrow);

        /// <summary>
        /// Deletes a borrow record.
        /// </summary>
        void Delete(BookBorrow borrow);

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        void Save();

        /// <summary>
        /// Gets the count of active borrows for a user.
        /// </summary>
        int GetActiveBorrowCountByUserId(int userId);
    }
}
