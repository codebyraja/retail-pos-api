using Microsoft.AspNetCore.SignalR;

namespace Payments.RealTime
{
    public class PaymentsHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        }
    }
}

//using Microsoft.AspNetCore.SignalR;

//namespace Payments.RealTime
//{
//    public class PaymentsHub : Hub
//    {
//        public override async Task OnConnectedAsync()
//        {
//            Console.WriteLine($"[Hub] Connected: {Context.ConnectionId}");
//            await base.OnConnectedAsync();
//        }

//        public override async Task OnDisconnectedAsync(Exception? exception)
//        {
//            Console.WriteLine($"[Hub] Disconnected: {Context.ConnectionId}, Error = {exception?.Message}");
//            await base.OnDisconnectedAsync(exception);
//        }

//        public async Task JoinOrderGroup(string orderId)
//        {
//            Console.WriteLine($"[Hub] JoinOrderGroup called. ConnectionId={Context.ConnectionId}, OrderId={orderId}");
//            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
//        }
//    }
//}
