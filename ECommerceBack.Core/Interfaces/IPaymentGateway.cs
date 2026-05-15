using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>واجهة بوابة الدفع (تطبيقها وهمي أو حقيقي).</summary>
    public interface IPaymentGateway
    {
        /// <summary>معالجة عملية دفع لمبلغ معين باستخدام رقم البطاقة.</summary>
        /// <param name="amount">المبلغ المطلوب.</param>
        /// <param name="cardNumber">رقم البطاقة (محاكاة – لا يُخزن).</param>
        /// <returns>نتيجة الدفع (نجاح/فشل، معرف المعاملة، رسالة).</returns>
        Task<PaymentResult> ProcessPaymentAsync(decimal amount, string cardNumber);
    }


}