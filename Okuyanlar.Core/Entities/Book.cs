using System.ComponentModel.DataAnnotations;

namespace Okuyanlar.Core.Entities
{
  /// <summary>
  /// Represents a book in the library inventory.
  /// </summary>
  public class Book
  {
    public int Id { get; set; }

    /// <summary>
    /// The title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The author of the book.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// International Standard Book Number (Unique Identifier).
    /// </summary>
    public string ISBN { get; set; } = string.Empty;

    /// <summary>
    /// The number of physical copies available in the library.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Indicates if the book is available for borrowing.
    /// </summary>
    public bool IsActive { get; set; } = true;
  }
}