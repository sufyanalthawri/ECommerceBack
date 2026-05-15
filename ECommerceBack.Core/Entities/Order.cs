namespace ECommerceBack.Core.Entities
{
    /// <summary>حالة الطلب (قيد الانتظار، مدفوع، شُحن، ملغي).</summary>
    public enum OrderStatus { Pending, Paid, Shipped, Cancelled }

    /// <summary>
    /// يمثل طلب شراء نهائي تم تأكيده، ويحتوي على عناصر الطلب ومعاملة الدفع والفواتير.
    /// </summary>
    public class Order
    {
        /// <summary>المعرف الفريد للطلب.</summary>
        public int Id { get; set; }

        /// <summary>معرف المستخدم الذي قام بالطلب (Foreign Key).</summary>
        public int UserId { get; set; }

        /// <summary>المستخدم المرتبط (علاقة متعدد إلى واحد).</summary>
        public virtual User User { get; set; } = null!;

        /// <summary>تاريخ إنشاء الطلب (توقيت UTC).</summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>الإجمالي الكلي للطلب (مجموع UnitPrice * Quantity لكل OrderItem).</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>حالة الطلب الحالية (Pending, Paid, Shipped, Cancelled).</summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>معرف معاملة الدفع (Foreign Key إلى PaymentTransaction).</summary>
        public int? PaymentTransactionId { get; set; }

        /// <summary>معاملة الدفع المرتبطة (علاقة واحد لواحد).</summary>
        public virtual PaymentTransaction? PaymentTransaction { get; set; }

        /// <summary>قائمة عناصر الطلب (علاقة واحد إلى متعدد).</summary>
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}