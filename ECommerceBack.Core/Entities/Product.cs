using System.ComponentModel.DataAnnotations;

namespace ECommerceBack.Core.Entities
{
    /// <summary>
    /// يمثل منتجاً معروضاً للبيع.
    /// يحتوي على مخزون وسعر، ويستخدم RowVersion للتزامن المتفائل (منع البيع الزائد).
    /// </summary>
    public class Product
    {
        /// <summary>المعرف الفريد للمنتج.</summary>
        public int Id { get; set; }

        /// <summary>اسم المنتج.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>سعر المنتج الحالي.</summary>
        public decimal Price { get; set; }

        /// <summary>الكمية المتاحة في المخزون.</summary>
        public int Stock { get; set; }

        /// <summary>
        /// حقل التزامن المتفائل (Optimistic Concurrency).
        /// يتغير تلقائياً عند كل تحديث. يُستخدم بواسطة EF Core لمنع تحديث نفس الصف بشكل متزامن.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        /// <summary>تاريخ آخر تحديث للمنتج (يساعد في التخزين المؤقت والمراقبة).</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}