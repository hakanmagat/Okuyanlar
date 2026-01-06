using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Data.Repositories
{
  /// <summary>
  /// Concrete implementation of IBookRepository using EF Core.
  /// </summary>
  public class BookRepository : IBookRepository
  {
    private readonly OkuyanlarDbContext _context;

    public BookRepository(OkuyanlarDbContext context)
    {
      _context = context;
    }

    /// <inheritdoc />
    public void Add(Book book)
    {
      _context.Books.Add(book);
      _context.SaveChanges();
    }

    /// <inheritdoc />
    public void Update(Book book)
    {
      _context.Books.Update(book);
      _context.SaveChanges();
    }

    /// <inheritdoc />
    public void Delete(int id)
    {
      var book = _context.Books.Find(id);
      if (book != null)
      {
        _context.Books.Remove(book);
        _context.SaveChanges();
      }
    }

    /// <inheritdoc />
    public Book? GetById(int id)
    {
      return _context.Books.Find(id);
    }

    /// <inheritdoc />
    public Book? GetByISBN(string isbn)
    {
      return _context.Books.FirstOrDefault(b => b.ISBN == isbn);
    }

    /// <inheritdoc />
    public IEnumerable<Book> GetAll()
    {
      return _context.Books.ToList();
    }

    /// <inheritdoc />
    public IEnumerable<Book> SearchBooks(string searchTerm)
    {
      // If the search term is empty, return all books.
      if (string.IsNullOrWhiteSpace(searchTerm))
      {
        return _context.Books.ToList();
      }

      // Search logic: Title OR Author contains the keyword (Case-insensitive usually in SQLite/SQL)
      return _context.Books
          .Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm))
          .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<Book> GetTopRatedBooks(int count = 10)
    {
      return _context.Books
          .Where(b => b.IsActive)
          .OrderByDescending(b => b.Rating)
          .ThenByDescending(b => b.RatingCount)
          .Take(count)
          .ToList();
    }
  }
}