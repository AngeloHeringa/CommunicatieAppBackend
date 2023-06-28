using CommunicatieAppBackend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CommunicatieAppBackendTests{
    public interface IHubContextTest :IHubContext<NotificationHub>
    {
        Task sendNotification(String title, String body);
    }

}