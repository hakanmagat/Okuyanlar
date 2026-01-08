using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Service.Services
{
    /// <summary>
    /// Service for managing book reservations.
    /// </summary>
    public class BookReservationService
    {
        private readonly IBookReservationRepository _reservationRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;

        public BookReservationService(
            IBookReservationRepository reservationRepository,
            IBookRepository bookRepository,
            IUserRepository userRepository)
        {
            _reservationRepository = reservationRepository;
            _bookRepository = bookRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Reserve a book for a user.
        /// </summary>
        /// <param name="bookId">ID of the book to reserve</param>
        /// <param name="userEmail">Email of the user</param>
        /// <param name="reservationHours">Duration of reservation in hours (default 24)</param>
        public void ReserveBook(int bookId, string userEmail, int reservationHours = 24)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            var book = _bookRepository.GetById(bookId);
            if (book == null)
                throw new Exception("Book not found.");

            // Check if book is available
            if (book.Stock <= 0 || !book.IsActive)
                throw new Exception("Book is not available for reservation.");

            // Check if book already has an active reservation
            if (_reservationRepository.HasActiveReservation(bookId))
                throw new Exception("This book is already reserved by another user.");

            // Check user's active reservation count (max 3)
            var activeCount = _reservationRepository.GetActiveReservationCountByUserId(user.Id);
            if (activeCount >= 3)
                throw new Exception("You can only have a maximum of 3 active reservations.");

            // Create reservation
            var reservation = new BookReservation
            {
                BookId = bookId,
                UserId = user.Id,
                ReservedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(reservationHours),
                Status = "Active"
            };

            _reservationRepository.Add(reservation);

            // Decrease stock
            book.Stock--;
            _bookRepository.Update(book);

            _reservationRepository.Save();
        }

        /// <summary>
        /// Request check-in for a reservation.
        /// </summary>
        public void RequestCheckIn(int reservationId, string userEmail)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            var reservation = _reservationRepository.GetById(reservationId);
            if (reservation == null)
                throw new Exception("Reservation not found.");

            if (reservation.UserId != user.Id)
                throw new Exception("You can only check-in your own reservations.");

            if (reservation.Status != "Active")
                throw new Exception("This reservation is not active.");

            // Check if expired
            if (DateTime.UtcNow > reservation.ExpiresAt)
            {
                reservation.Status = "Expired";
                _reservationRepository.Update(reservation);
                _reservationRepository.Save();
                throw new Exception("This reservation has expired.");
            }

            reservation.CheckInRequested = true;
            reservation.CheckInRequestedAt = DateTime.UtcNow;

            _reservationRepository.Update(reservation);
            _reservationRepository.Save();
        }

        /// <summary>
        /// Accept check-in request and convert to borrow (called by librarian).
        /// </summary>
        public BookBorrow AcceptCheckIn(int reservationId, int librarianId, int borrowDays = 14)
        {
            var reservation = _reservationRepository.GetById(reservationId);
            if (reservation == null)
                throw new Exception("Reservation not found.");

            if (!reservation.CheckInRequested)
                throw new Exception("No check-in request for this reservation.");

            if (reservation.Status != "Active")
                throw new Exception("This reservation is not active.");

            // Create borrow record
            var borrow = new BookBorrow
            {
                BookId = reservation.BookId,
                UserId = reservation.UserId,
                BorrowedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(borrowDays),
                Status = "Active",
                ApprovedByLibrarianId = librarianId
            };

            // Update reservation status
            reservation.Status = "CheckedIn";
            reservation.CheckedInAt = DateTime.UtcNow;

            _reservationRepository.Update(reservation);
            _reservationRepository.Save();

            return borrow;
        }

        /// <summary>
        /// Cancel a reservation.
        /// </summary>
        public void CancelReservation(int reservationId, string userEmail)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            var reservation = _reservationRepository.GetById(reservationId);
            if (reservation == null)
                throw new Exception("Reservation not found.");

            if (reservation.UserId != user.Id)
                throw new Exception("You can only cancel your own reservations.");

            if (reservation.Status != "Active")
                throw new Exception("This reservation is not active.");

            // Restore stock
            var book = _bookRepository.GetById(reservation.BookId);
            if (book != null)
            {
                book.Stock++;
                _bookRepository.Update(book);
            }

            reservation.Status = "Cancelled";
            _reservationRepository.Update(reservation);
            _reservationRepository.Save();
        }

        /// <summary>
        /// Get all active reservations for a user.
        /// </summary>
        public IEnumerable<BookReservation> GetUserActiveReservations(string userEmail)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            return _reservationRepository.GetActiveReservationsByUserId(user.Id);
        }

        /// <summary>
        /// Get all reservations for a user.
        /// </summary>
        public IEnumerable<BookReservation> GetUserReservations(string userEmail)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            return _reservationRepository.GetByUserId(user.Id);
        }

        /// <summary>
        /// Get all active reservations (for librarian).
        /// </summary>
        public IEnumerable<BookReservation> GetAllActiveReservations()
        {
            return _reservationRepository.GetAllActiveReservations();
        }

        /// <summary>
        /// Get all check-in requests (for librarian).
        /// </summary>
        public IEnumerable<BookReservation> GetCheckInRequests()
        {
            return _reservationRepository.GetCheckInRequests();
        }

        /// <summary>
        /// Expire old reservations (background job or scheduled task).
        /// </summary>
        public void ExpireOldReservations()
        {
            var activeReservations = _reservationRepository.GetAllActiveReservations();
            var now = DateTime.UtcNow;

            foreach (var reservation in activeReservations)
            {
                if (now > reservation.ExpiresAt)
                {
                    reservation.Status = "Expired";
                    
                    // Restore stock
                    var book = _bookRepository.GetById(reservation.BookId);
                    if (book != null)
                    {
                        book.Stock++;
                        _bookRepository.Update(book);
                    }

                    _reservationRepository.Update(reservation);
                }
            }

            _reservationRepository.Save();
        }
    }
}
