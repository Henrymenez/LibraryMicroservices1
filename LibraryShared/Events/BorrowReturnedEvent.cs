using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryShared.Events
{
    public class BorrowReturnedEvent
    {
        public int BorrowId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime ReturnDate { get; set; }
    }
}
