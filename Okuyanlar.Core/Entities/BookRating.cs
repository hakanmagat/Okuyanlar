namespace Okuyanlar.Core.Entities
{
  /// <summary>
  /// Represents a per-user rating for a specific book.
  /// Enforces one rating per user per book; users can update their rating.
  /// </summary>
  public class BookRating
  {
    public int Id { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public decimal Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}
