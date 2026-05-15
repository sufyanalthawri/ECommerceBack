namespace ECommerceBack.Core.DTOs
{
    /// <summary>
    /// كائن نقل بيانات (DTO) يحمل معلومات الدفع من العميل إلى الخادم.
    /// يستخدم في عملية إنشاء الطلب لنقل بيانات البطاقة (محاكاة).
    /// ملاحظة: في نظام حقيقي، لا يتم تخزين هذه البيانات ويجب استخدام بوابة دفع آمنة.
    /// </summary>
    public class PaymentInfo
    {
        /// <summary>رقم بطاقة الائتمان / الخصم (محاكاة، للاختبار فقط).</summary>
        public string CardNumber { get; set; } = string.Empty;

        /// <summary>تاريخ انتهاء صلاحية البطاقة (شهر/سنة) – محاكاة.</summary>
        public string ExpiryDate { get; set; } = string.Empty;

        public string Cvv { get; set; } = string.Empty;
    }
}