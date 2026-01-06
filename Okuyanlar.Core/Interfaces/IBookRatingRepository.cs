using Okuyanlar.Core.Entities;

namespace Okuyanlar.Core.Interfaces
{
  /// <summary>
  /// Contract for per-user book ratings persistence.
  /// </summary>
  public interface IBookRatingRepository
  {
    void Add(BookRating rating);
    void Update(BookRating rating);
    BookRating? GetByBookAndUser(int bookId, int userId);
    IEnumerable<BookRating> GetRatingsForBook(int bookId);
  }
}
