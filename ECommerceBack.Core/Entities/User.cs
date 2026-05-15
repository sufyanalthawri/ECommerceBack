namespace ECommerceBack.Core.Entities
{
    public class User
    {
        public int Id { get; set; }

        // إما جعلها nullable باستخدام ? أو تعيين قيم افتراضية
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();  // تعيين قيمة افتراضية
        public virtual Cart? Cart { get; set; }           // سلة المستخدم (واحد لواحد)
    }
}



//الدور: يمثل الشخص المسجل في النظام (عميل).
//العلاقات:

//Cart: كل مستخدم له سلة واحدة (علاقة واحد لواحد).

//Orders: كل مستخدم يمكن أن يكون له عدة طلبات (علاقة واحد إلى متعدد).

//virtual: تسمح بـ Lazy Loading (تحميل البيانات المرتبطة عند الحاجة فقط).

