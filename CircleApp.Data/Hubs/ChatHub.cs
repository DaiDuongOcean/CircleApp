using CircleApp.Data.Models;
using CircleApp.Data;
using CircleApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CircleApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // MỚI: khi user mở kết nối (mở trang web/tab mới)
        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.UserIdentifier;
            if (int.TryParse(userIdStr, out int userId))
            {
                bool justCameOnline = OnlineUsersTracker.AddConnection(userId);
                if (justCameOnline)
                {
                    // Báo cho TẤT CẢ client biết user này vừa online
                    await Clients.All.SendAsync("UserStatusChanged", userId, true);
                }
            }
            await base.OnConnectedAsync();
        }

        // MỚI: khi user đóng tab/mất mạng
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdStr = Context.UserIdentifier;
            if (int.TryParse(userIdStr, out int userId))
            {
                bool justWentOffline = OnlineUsersTracker.RemoveConnection(userId);
                if (justWentOffline)
                {
                    await Clients.All.SendAsync("UserStatusChanged", userId, false);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string message, string attachmentUrl = null, string attachmentType = null)
        {
            var senderId = Context.UserIdentifier;

            if (int.TryParse(senderId, out int parsedSenderId) && int.TryParse(receiverId, out int parsedReceiverId))
            {
                var newMessage = new Message
                {
                    SenderId = parsedSenderId,
                    ReceiverId = parsedReceiverId,
                    Content = message ?? "",
                    AttachmentUrl = attachmentUrl,
                    AttachmentType = attachmentType,
                    DateSent = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(newMessage);
                await _context.SaveChangesAsync();

                await Clients.User(receiverId).SendAsync(
                    "ReceiveMessage",
                    senderId,
                    message,
                    attachmentUrl,
                    attachmentType,
                    newMessage.DateSent.ToString("o")
                );

                var unreadCount = await _context.Messages
                    .CountAsync(m => m.ReceiverId == parsedReceiverId && !m.IsRead);

                await Clients.User(receiverId).SendAsync("UpdateUnreadCount", unreadCount);
            }
        }
    }
}