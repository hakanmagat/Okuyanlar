using Okuyanlar.Core.Entities;

namespace Okuyanlar.Core.Interfaces
{
  /// <summary>
  /// Defines the contract for data persistence operations related to Books.
  /// Acts as an abstraction layer over the database context.
  /// </summary>
  public interface IBookRepository
  {
    /// <summary>
    /// Persists a new book entity to the underlying data store.
    /// </summary>
    /// <param name="book">The book entity to be added.</param>
    void Add(Book book);

    /// <summary>
    /// Updates the details of an existing book in the data store.
    /// </summary>
    /// <param name="book">The book entity with updated values.</param>
    void Update(Book book);

    /// <summary>
    /// Permanently removes a book from the inventory based on its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Retrieves a specific book by its unique identifier (Primary Key).
    /// </summary>
    /// <param name="id">The unique identifier to search for.</param>
    /// <returns>The <see cref="Book"/> entity if found; otherwise, null.</returns>
    Book? GetById(int id);

    /// <summary>
    /// Retrieves a specific book by its International Standard Book Number (ISBN).
    /// Critical for preventing duplicate entries during creation.
    /// </summary>
    /// <param name="isbn">The unique ISBN string.</param>
    /// <returns>The <see cref="Book"/> entity if found; otherwise, null.</returns>
    Book? GetByISBN(string isbn);

    /// <summary>
    /// Searches for books where the title or author contains the search term.
    /// Case-insensitive search logic is handled by the implementation.
    /// </summary>
    /// <param name="searchTerm">The keyword to search for.</param>
    /// <returns>A collection of matching books.</returns>
    IEnumerable<Book> SearchBooks(string searchTerm);

    /// <summary>
    /// Retrieves all books currently registered in the system.
    /// Used primarily for full inventory lists.
    /// </summary>
    /// <returns>A collection of all books.</returns>
    IEnumerable<Book> GetAll();

    /// <summary>
    /// Retrieves the top N books sorted by rating in descending order.
    /// </summary>
    /// <param name="count">The number of top-rated books to retrieve (default 10).</param>
    /// <returns>A collection of top-rated books.</returns>
    IEnumerable<Book> GetTopRatedBooks(int count = 10);
  }
}