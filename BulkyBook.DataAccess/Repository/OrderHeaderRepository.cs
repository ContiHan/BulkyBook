using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader orderHeader)
        {
            if (orderHeader is null)
            {
                throw new ArgumentNullException(nameof(orderHeader));
            }

            if (_db.OrderHeaders is null)
            {
                throw new ArgumentNullException(nameof(_db.OrderHeaders));
            }

            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderHeader = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderHeader is not null)
            {
                orderHeader.OrderStatus = orderStatus;
                if (paymentStatus is not null)
                {
                    orderHeader.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderHeader = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            orderHeader.PaymentDate = DateTime.Now;
            orderHeader.SessionId = sessionId;
            orderHeader.PaymentIntentId = paymentIntentId;
        }
    }
}
