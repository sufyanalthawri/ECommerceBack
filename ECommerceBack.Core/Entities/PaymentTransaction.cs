namespace ECommerceBack.Core.Entities
{
    /// <summary>حالة معاملة الدفع (قيد الانتظار، نجاح، فشل).</summary>
    public enum PaymentStatus { Pending, Success, Failed }

    /// <summary>
    /// يمثل محاولة دفع مرتبطة بطلب ما، ويحتوي على المبلغ والمرجع من بوابة الدفع والحالة.
    /// </summary>
    public class PaymentTransaction
    {
        /// <summary>المعرف الفريد لمعاملة الدفع.</summary>
        public int Id { get; set; }

        /// <summary>معرف الطلب المرتبط (Foreign Key).</summary>
        public int OrderId { get; set; }

        /// <summary>الطلب المرتبط (علاقة واحد لواحد).</summary>
        public Order Order { get; set; } = null!;

        /// <summary>المبلغ المدفوع.</summary>
        public decimal Amount { get; set; }

        /// <summary>تاريخ الدفع (توقيت UTC).</summary>
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        /// <summary>الرقم المرجعي من بوابة الدفع الخارجية (مثل Stripe, PayPal).</summary>
        public string GatewayReference { get; set; } = string.Empty;

        /// <summary>حالة الدفع (Pending, Success, Failed).</summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    }
}