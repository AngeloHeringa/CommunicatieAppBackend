using System.Drawing.Printing;
using Microsoft.AspNetCore.SignalR;

namespace CommunicatieAppBackend.Hubs{
    public class NotificationHub : Hub{
        public async Task sendNotification(String title, String body){
            await Clients.All.SendAsync("ReceiveNotification",title,body);
            Console.WriteLine("notification requested"+title+body);
        }
    }
}