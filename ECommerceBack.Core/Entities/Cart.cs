namespace ECommerceBack.Core.Entities
{
    /// <summary>
    /// يمثل سلة التسوق المؤقتة لمستخدم واحد.
    /// تحتوي السلة على عدة عناصر (CartItem) وتُفرغ تلقائياً بعد إتمام الطلب.
    /// </summary>
    public class Cart
    {
        /// <summary>المعرف الفريد للسلة.</summary>
        public int Id { get; set; }

        /// <summary>معرف المستخدم المرتبط بالسلة (Foreign Key).</summary>
        public int UserId { get; set; }

        /// <summary>المستخدم الذي تمتلك هذه السلة (علاقة واحد لواحد).</summary>
        public virtual User User { get; set; } = null!;

        /// <summary>تاريخ إنشاء السلة (توقيت UTC).</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>قائمة عناصر السلة (علاقة واحد إلى متعدد).</summary>
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}