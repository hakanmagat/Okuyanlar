using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Service.Services
{
  /// <summary>
  /// Encapsulates business logic for Book management (Inventory).
  /// </summary>
  public class BookService
  {
    private readonly IBookRepository _bookRepository;
    private readonly IBookRatingRepository? _bookRatingRepository;
    private readonly IUserRepository? _userRepository;

    public BookService(IBookRepository bookRepository, IBookRatingRepository bookRatingRepository, IUserRepository userRepository)
    {
      _bookRepository = bookRepository;
      _bookRatingRepository = bookRatingRepository;
      _userRepository = userRepository;
    }

    // Backward-compatible constructor for tests/other usages not needing ratings.
    public BookService(IBookRepository bookRepository)
    {
      _bookRepository = bookRepository;
      _bookRatingRepository = null;
      _userRepository = null;
    }

    /// <summary>
    /// Adds a new book to the inventory after validation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if ISBN already exists.</exception>
    /// <exception cref="ArgumentException">Thrown if stock is negative.</exception>
    public void AddBook(Book book)
    {
      if (book.Stock < 0)
      {
        throw new ArgumentException("Stock cannot be negative.");
      }

      var existingBook = _bookRepository.GetByISBN(book.ISBN);
      if (existingBook != null)
      {
        throw new InvalidOperationException("A book with this ISBN already exists.");
      }

      _bookRepository.Add(book);
    }

    /// <summary>
    /// Updates existing book details.
    /// </summary>
    public void UpdateBook(Book book)
    {
      var existingBook = _bookRepository.GetById(book.Id);
      if (existingBook == null)
      {
        throw new Exception("Book not found.");
      }

      // Update fields
      existingBook.Title = book.Title;
      existingBook.Author = book.Author;
      existingBook.Stock = book.Stock;
      existingBook.ISBN = book.ISBN;
      existingBook.IsActive = book.IsActive;

      _bookRepository.Update(existingBook);
    }

    /// <summary>
    /// Removes a book from the inventory.
    /// </summary>
    public void DeleteBook(int id)
    {
      var existingBook = _bookRepository.GetById(id);
      if (existingBook != null)
      {
        _bookRepository.Delete(id);
      }
    }

    /// <summary>
    /// Retrieves all books from the inventory.
    /// </summary>
    public IEnumerable<Book> GetAllBooks()
    {
      return _bookRepository.GetAll();
    }

    public Book? GetBookById(int id)
    {
      return _bookRepository.GetById(id);
    }

    /// <summary>
    /// Searches for books by title or author.
    /// Returns all books if the search term is empty.
    /// </summary>
    /// <param name="searchTerm">The query string entered by the user.</param>
    /// <returns>List of matching books.</returns>
    public IEnumerable<Book> SearchBooks(string searchTerm)
    {
      // We can add extra logic here later (e.g., logging searches)
      return _bookRepository.SearchBooks(searchTerm);
    }

    /// <summary>
    /// Retrieves the top 10 books sorted by rating.
    /// </summary>
    /// <returns>List of top 10 rated books.</returns>
    public IEnumerable<Book> GetTopRatedBooks(int count = 10)
    {
      return _bookRepository.GetTopRatedBooks(count);
    }

    /// <summary>
    /// Adds or updates a rating for a book.
    /// Calculates the average rating based on total ratings.
    /// </summary>
    /// <param name="bookId">The ID of the book to rate.</param>
    /// <param name="ratingValue">The rating value (typically 1-5).</param>
    /// <exception cref="Exception">Thrown if book not found.</exception>
    /// <summary>
    /// Adds or updates a rating for a book by a specific user.
    /// Enforces: user must exist, be EndUser, and can rate a book only once (but may update).
    /// Updates the aggregate book rating and count after save.
    /// </summary>
    public void RateBook(int bookId, string userEmail, decimal ratingValue)
    {
      if (ratingValue < 0 || ratingValue > 5)
      {
        throw new ArgumentException("Rating must be between 0 and 5.");
      }

      var book = _bookRepository.GetById(bookId);
      if (book == null)
      {
        throw new Exception("Book not found.");
      }

      if (string.IsNullOrWhiteSpace(userEmail))
      {
        throw new UnauthorizedAccessException("User must be logged in.");
      }

      if (_userRepository == null || _bookRatingRepository == null)
      {
        throw new InvalidOperationException("Rating feature is not configured.");
      }

      var user = _userRepository.GetByEmail(userEmail);
      if (user == null)
      {
        throw new UnauthorizedAccessException("User not found.");
      }
      if (user.Role != Okuyanlar.Core.Enums.UserRole.EndUser)
      {
        throw new UnauthorizedAccessException("Only end users can rate books.");
      }

      var existing = _bookRatingRepository.GetByBookAndUser(bookId, user.Id);
      if (existing == null)
      {
        var newRating = new BookRating
        {
          BookId = bookId,
          UserId = user.Id,
          Rating = ratingValue,
          CreatedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow
        };
        _bookRatingRepository.Add(newRating);
      }
      else
      {
        existing.Rating = ratingValue;
        existing.UpdatedAt = DateTime.UtcNow;
        _bookRatingRepository.Update(existing);
      }

      // Recompute aggregate
      var ratings = _bookRatingRepository.GetRatingsForBook(bookId).ToList();
      book.RatingCount = ratings.Count;
      book.Rating = ratings.Count == 0 ? 0 : ratings.Average(r => r.Rating);
      _bookRepository.Update(book);
    }
  }
}