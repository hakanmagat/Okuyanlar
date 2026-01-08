using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Service.Services
{
    /// <summary>
    /// Service for managing book borrows.
    /// </summary>
    public class BookBorrowService
    {
        private readonly IBookBorrowRepository _borrowRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;

        public BookBorrowService(
            IBookBorrowRepository borrowRepository,
            IBookRepository bookRepository,
            IUserRepository userRepository)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Create a new borrow record.
        /// </summary>
        public void CreateBorrow(BookBorrow borrow)
        {
            // Check if user has reached maximum active borrows (3)
            var activeBorrowCount = _borrowRepository.GetActiveBorrowCountByUserId(borrow.UserId);
            if (activeBorrowCount >= 3)
                throw new Exception("User has reached the maximum of 3 active borrowed books.");

            _borrowRepository.Add(borrow);
            _borrowRepository.Save();
        }

        /// <summary>
        /// Request return of a borrowed book.
        /// </summary>
        public void RequestReturn(int borrowId, string userEmail)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            var borrow = _borrowRepository.GetById(borrowId);
            if (borrow == null)
                throw new Exception("Borrow record not found.");

            if (borrow.UserId != user.Id)
                throw new Exception("You can only return your own borrowed books.");

            if (borrow.Status == "Returned")
                throw new Exception("This book has already been returned.");

            borrow.ReturnRequested = true;
            borrow.ReturnRequestedAt = DateTime.UtcNow;
            borrow.Status = "ReturnRequested";

            _borrowRepository.Update(borrow);
            _borrowRepository.Save();
        }

        /// <summary>
        /// Accept return request (called by librarian).
        /// </summary>
        public void AcceptReturn(int borrowId, int librarianId)
        {
            var borrow = _borrowRepository.GetById(borrowId);
            if (borrow == null)
                throw new Exception("Borrow record not found.");

            if (!borrow.ReturnRequested)
                throw new Exception("No return request for this borrow.");

            // Restore stock
            var book = _bookRepository.GetById(borrow.BookId);
            if (book != null)
            {
                book.Stock++;
                _bookRepository.Update(book);
            }

            borrow.Status = "Returned";
            borrow.ReturnedAt = DateTime.UtcNow;
            borrow.ReturnAcceptedByLibrarianId = librarianId;

            _borrowRepository.Update(borrow);
            _borrowRepository.Save();
        }

        /// <summary>
        /// Get all active borrows for a user.
        /// </summary>
        public IEnumerable<BookBorrow> GetUserActiveBorrows(string userEmail)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            return _borrowRepository.GetActiveBorrowsByUserId(user.Id);
        }

        /// <summary>
        /// Get all borrows for a user.
        /// </summary>
        public IEnumerable<BookBorrow> GetUserBorrows(string userEmail)
        {
            var user = _userRepository.GetByEmail(userEmail);
            if (user == null)
                throw new Exception("User not found.");

            return _borrowRepository.GetByUserId(user.Id);
        }

        /// <summary>
        /// Get all active borrows (for librarian).
        /// </summary>
        public IEnumerable<BookBorrow> GetAllActiveBorrows()
        {
            return _borrowRepository.GetAllActiveBorrows();
        }

        /// <summary>
        /// Get all return requests (for librarian).
        /// </summary>
        public IEnumerable<BookBorrow> GetReturnRequests()
        {
            return _borrowRepository.GetReturnRequests();
        }

        /// <summary>
        /// Mark overdue borrows (background job or scheduled task).
        /// </summary>
        public void MarkOverdueBorrows()
        {
            var activeBorrows = _borrowRepository.GetAllActiveBorrows();
            var now = DateTime.UtcNow;

            foreach (var borrow in activeBorrows)
            {
                if (borrow.Status == "Active" && now > borrow.DueDate)
                {
                    borrow.Status = "Overdue";
                    _borrowRepository.Update(borrow);
                }
            }

            _borrowRepository.Save();
        }
    }
}
