namespace MidcoServiceDeskBot.Models
{
    public class PasswordExpirationModel
    {
        public string UserId { get; set; }
        
        public string Name { get; set; }

        public string PasswordExpires { get; set; }

        public string TimeRemaining { get; set; }

        public string NotificationUrl { get; set; }
    }
}
