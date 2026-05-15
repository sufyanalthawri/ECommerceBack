namespace ECommerceBack.Core.Entities
{
    /// <summary>
    /// يمثل عنصراً داخل طلب شراء نهائي، أي منتج محدد مع الكمية والسعر الثابت لحظة الشراء.
    /// </summary>
    public class OrderItem
    {
        /// <summary>المعرف الفريد لعنصر الطلب.</summary>
        public int Id { get; set; }

        /// <summary>معرف الطلب الذي ينتمي إليه هذا العنصر (Foreign Key).</summary>
        public int OrderId { get; set; }

        /// <summary>الطلب المرتبط (علاقة متعدد إلى واحد).</summary>
        public virtual Order Order { get; set; } = null!;

        /// <summary>معرف المنتج المشترى (Foreign Key).</summary>
        public int ProductId { get; set; }

        /// <summary>المنتج المرتبط (علاقة متعدد إلى واحد).</summary>
        public virtual Product Product { get; set; } = null!;

        /// <summary>الكمية التي تم شراؤها.</summary>
        public int Quantity { get; set; }

        /// <summary>سعر الوحدة عند وقت الشراء (ثابت، لا يتغير مع سعر المنتج لاحقاً).</summary>
        public decimal UnitPrice { get; set; }
    }
}