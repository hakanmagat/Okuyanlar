using Okuyanlar.Core.Entities;

namespace Okuyanlar.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing book reservations.
    /// </summary>
    public interface IBookReservationRepository
    {
        /// <summary>
        /// Gets a reservation by its ID.
        /// </summary>
        BookReservation? GetById(int id);

        /// <summary>
        /// Gets all reservations for a specific user.
        /// </summary>
        IEnumerable<BookReservation> GetByUserId(int userId);

        /// <summary>
        /// Gets all active reservations for a specific user.
        /// </summary>
        IEnumerable<BookReservation> GetActiveReservationsByUserId(int userId);

        /// <summary>
        /// Gets all reservations for a specific book.
        /// </summary>
        IEnumerable<BookReservation> GetByBookId(int bookId);

        /// <summary>
        /// Gets all active reservations.
        /// </summary>
        IEnumerable<BookReservation> GetAllActiveReservations();

        /// <summary>
        /// Gets all check-in requests (pending approval).
        /// </summary>
        IEnumerable<BookReservation> GetCheckInRequests();

        /// <summary>
        /// Adds a new reservation.
        /// </summary>
        void Add(BookReservation reservation);

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        void Update(BookReservation reservation);

        /// <summary>
        /// Deletes a reservation.
        /// </summary>
        void Delete(BookReservation reservation);

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        void Save();

        /// <summary>
        /// Gets the count of active reservations for a user.
        /// </summary>
        int GetActiveReservationCountByUserId(int userId);

        /// <summary>
        /// Checks if a book has an active reservation.
        /// </summary>
        bool HasActiveReservation(int bookId);
    }
}
