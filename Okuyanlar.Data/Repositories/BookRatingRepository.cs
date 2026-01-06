using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Data.Repositories
{
  /// <summary>
  /// EF Core implementation of IBookRatingRepository.
  /// </summary>
  public class BookRatingRepository : IBookRatingRepository
  {
    private readonly OkuyanlarDbContext _context;

    public BookRatingRepository(OkuyanlarDbContext context)
    {
      _context = context;
    }

    public void Add(BookRating rating)
    {
      _context.BookRatings.Add(rating);
      _context.SaveChanges();
    }

    public void Update(BookRating rating)
    {
      _context.BookRatings.Update(rating);
      _context.SaveChanges();
    }

    public BookRating? GetByBookAndUser(int bookId, int userId)
    {
      return _context.BookRatings.FirstOrDefault(r => r.BookId == bookId && r.UserId == userId);
    }

    public IEnumerable<BookRating> GetRatingsForBook(int bookId)
    {
      return _context.BookRatings.Where(r => r.BookId == bookId).ToList();
    }
  }
}
