using System.ComponentModel.DataAnnotations;

namespace Okuyanlar.Core.Entities
{
    /// <summary>
    /// Represents a book reservation made by a user.
    /// </summary>
    public class BookReservation
    {
        /// <summary>
        /// Unique identifier for the reservation (Primary Key).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the Book being reserved.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Navigation property to the Book.
        /// </summary>
        public Book? Book { get; set; }

        /// <summary>
        /// Foreign key to the User who reserved the book.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to the User.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// The timestamp when the reservation was created.
        /// </summary>
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The timestamp when the reservation expires.
        /// Default is 24 hours from ReservedAt.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Status of the reservation.
        /// Possible values: Active, CheckedIn, Expired, Cancelled
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Indicates if user has requested check-in.
        /// </summary>
        public bool CheckInRequested { get; set; } = false;

        /// <summary>
        /// The timestamp when check-in was requested.
        /// </summary>
        public DateTime? CheckInRequestedAt { get; set; }

        /// <summary>
        /// The timestamp when the reservation was checked in (converted to borrow).
        /// </summary>
        public DateTime? CheckedInAt { get; set; }
    }
}
