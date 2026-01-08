using System.ComponentModel.DataAnnotations;

namespace Okuyanlar.Core.Entities
{
    /// <summary>
    /// Represents a borrowed book by a user.
    /// </summary>
    public class BookBorrow
    {
        /// <summary>
        /// Unique identifier for the borrow record (Primary Key).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the Book being borrowed.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Navigation property to the Book.
        /// </summary>
        public Book? Book { get; set; }

        /// <summary>
        /// Foreign key to the User who borrowed the book.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to the User.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// The timestamp when the book was borrowed.
        /// </summary>
        public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The timestamp when the book is due to be returned.
        /// Default is 14 days from BorrowedAt.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Status of the borrow.
        /// Possible values: Active, ReturnRequested, Returned, Overdue
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Indicates if user has requested to return the book.
        /// </summary>
        public bool ReturnRequested { get; set; } = false;

        /// <summary>
        /// The timestamp when return was requested.
        /// </summary>
        public DateTime? ReturnRequestedAt { get; set; }

        /// <summary>
        /// The timestamp when the book was actually returned.
        /// </summary>
        public DateTime? ReturnedAt { get; set; }

        /// <summary>
        /// The librarian who approved the borrow (optional).
        /// </summary>
        public int? ApprovedByLibrarianId { get; set; }

        /// <summary>
        /// The librarian who accepted the return (optional).
        /// </summary>
        public int? ReturnAcceptedByLibrarianId { get; set; }
    }
}
