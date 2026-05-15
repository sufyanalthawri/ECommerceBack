namespace ECommerceBack.Core.Interfaces
{
    /// <summary>واجهة لإرسال البريد الإلكتروني ).</summary>
    public interface IEmailSender
    {
        /// <summary>إرسال بريد إلكتروني.</summary>
        /// <param name="to">عنوان المستلم.</param>
        /// <param name="subject">عنوان البريد.</param>
        /// <param name="body">محتوى البريد  .</param>
        Task SendEmailAsync(string to, string subject, string body);
    }
}