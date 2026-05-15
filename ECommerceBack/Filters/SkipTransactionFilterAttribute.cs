namespace ECommerceBack.API.Filters
{
    /// <summary>
    /// سمة (Attribute) تستخدم لاستثناء Controllers أو Actions محددة من إدارة المعاملات التلقائية.
    /// عند تطبيقها، سيتخطى TransactionFilter تنفيذ المعاملة لقاعدة البيانات.
    /// </summary>
    /// <remarks>
    /// هذا مفيد للـ Controllers التي تدير معاملاتها الداخلية بنفسها (مثل OrderService
    /// التي تستخدم RowVersion وآلية إعادة المحاولة).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SkipTransactionFilterAttribute : Attribute
    {
    }
}