namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// خدمة لتحديد معدل الطلبات لكل عميل (IP أو UserId).
    /// يمكن استخدامها مع YARP أو Middleware مخصص.
    /// </summary>
    public interface IRateLimitService
    {
        /// <summary>التحقق مما إذا كان الطلب مسموحاً ضمن الحدود المحددة.</summary>
        /// <param name="clientId">معرّف العميل (IP، UserId).</param>
        /// <param name="permitLimit">العدد المسموح به في النافذة الزمنية.</param>
        /// <param name="windowSeconds">طول النافذة الزمنية بالثواني.</param>
        /// <returns>true إذا كان مسموحاً، false إذا تم تجاوز الحد.</returns>
        Task<bool> IsRequestAllowedAsync(string clientId, int permitLimit, int windowSeconds);
    }
}