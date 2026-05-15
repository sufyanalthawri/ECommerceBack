namespace ECommerceBack.Core.Entities
{
    /// <summary>
    /// يمثل فاتورة تم إنشاؤها ل.
    /// </summary>
    public class Invoice
    {
        /// <summary>المعرف الفريد للفاتورة.</summary>
        public int Id { get; set; }

        /// <summary>معرف الطلب المرتبط (Foreign Key).</summary>
        public int OrderId { get; set; }

        /// <summary>الطلب الذي تم إنشاء الفاتورة من أجله (علاقة واحد لواحد).</summary>
        public virtual Order Order { get; set; } = null!;

        /// <summary>رقم الفاتورة الفريد (يمكن توليده بشكل تلقائي).</summary>
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>تاريخ إنشاء الفاتورة (توقيت UTC).</summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>رابط ملف PDF الخاص بالفاتورة (اختياري).</summary>
        public string? PdfUrl { get; set; }

        /// <summary>يشير إلى ما إذا تم إرسال الفاتورة إلى المستخدم عبر البريد الإلكتروني.</summary>
        public bool IsSent { get; set; }
    }
}