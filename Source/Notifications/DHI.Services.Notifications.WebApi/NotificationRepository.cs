namespace DHI.Services.Notifications.WebApi
{
    public class NotificationRepository : JsonNotificationRepository
    {
        public NotificationRepository(string filePath) : base(filePath)
        {
            ConfigureJsonSerializer(options =>
            {
                options.WriteIndented = true;
            });
        }
    }
}
