using System;
using StockBuddy.Shared.Utilities;
using StockBuddy.Client.Shared.ViewModels;

namespace StockBuddy.Client.Shared.Messaging.Messages
{
    internal sealed class StockNotificationMessage
    {
        public StockNotificationMessage(StockViewModel stock, NotificationTypes notificationType)
        {
            Guard.AgainstNull(() => stock);
            Stock = stock;
            NotificationType = notificationType;
        }

        public StockViewModel Stock { get; }
        public NotificationTypes NotificationType { get; }
    }
}
