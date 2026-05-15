namespace ECommerceBack.Core.Entities
{
    /// <summary>
    /// يمثل عنصراً داخل سلة التسوق، أي منتج محدد مع الكمية المطلوبة.
    /// </summary>
    public class CartItem
    {
        /// <summary>المعرف الفريد لعنصر السلة.</summary>
        public int Id { get; set; }

        /// <summary>معرف السلة التي ينتمي إليها هذا العنصر (Foreign Key).</summary>
        public int CartId { get; set; }

        /// <summary>السلة المرتبطة (علاقة متعدد إلى واحد).</summary>
        public virtual Cart Cart { get; set; } = null!;

        /// <summary>معرف المنتج المطلوب (Foreign Key).</summary>
        public int ProductId { get; set; }

        /// <summary>المنتج المرتبط (علاقة متعدد إلى واحد).</summary>
        public virtual Product Product { get; set; } = null!;

        /// <summary>الكمية المطلوبة من هذا المنتج في السلة.</summary>
        public int Quantity { get; set; }
    }
}