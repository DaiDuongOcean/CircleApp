using CircleApp.Data;
using CircleApp.Data.Models;
using CircleApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace CircleApp.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private readonly AppDbContext _context;
        public ChatsController(AppDbContext context)
        {
            _context = context;
        }

        public static DateTime ToVietnamTime(DateTime utcDateTime)
        {
            TimeZoneInfo vietnamTimeZone;
            try
            {
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }

            var utcMarked = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utcMarked, vietnamTimeZone);
        }

        public async Task<IActionResult> Index(int? receiverId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.CurrentUserId = currentUserId;

            var chatUsers = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id != currentUserId &&
                            (u.MessagesSent.Any(m => m.ReceiverId == currentUserId) ||
                             u.MessagesReceived.Any(m => m.SenderId == currentUserId)))
                .ToListAsync();

            if (!chatUsers.Any())
            {
                chatUsers = await _context.Users.AsNoTracking().Where(u => u.Id != currentUserId).Take(10).ToListAsync();
            }
            ViewBag.ChatUsers = chatUsers;

            ViewBag.OnlineUserIds = OnlineUsersTracker.GetOnlineUserIds(chatUsers.Select(u => u.Id));

            if (receiverId.HasValue)
            {
                var receiver = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == receiverId.Value);
                ViewBag.Receiver = receiver;

                ViewBag.ReceiverIsOnline = OnlineUsersTracker.IsOnline(receiverId.Value);

                var messages = await _context.Messages
                    .AsNoTracking()
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == receiverId) ||
                                (m.SenderId == receiverId && m.ReceiverId == currentUserId))
                    .OrderBy(m => m.DateSent)
                    .ToListAsync();

                var unreadFromThisUser = await _context.Messages
                    .Where(m => m.SenderId == receiverId && m.ReceiverId == currentUserId && !m.IsRead)
                    .ToListAsync();

                if (unreadFromThisUser.Any())
                {
                    foreach (var m in unreadFromThisUser) m.IsRead = true;
                    await _context.SaveChangesAsync();
                }

                return View(messages);
            }

            return View(new List<Message>());
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentUserId)) return Json(0);

            var count = await _context.Messages
                .CountAsync(m => m.ReceiverId == currentUserId && !m.IsRead);

            return Json(count);
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Không có file được gửi lên.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".zip", ".rar", ".txt" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("Định dạng file không được hỗ trợ.");

            var uploadsFolder = Path.Combine("wwwroot", "uploads", "chat");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string attachmentType = ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
                ".pdf" => "pdf",
                ".doc" or ".docx" => "doc",
                ".xls" or ".xlsx" => "excel",
                ".ppt" or ".pptx" => "ppt",
                _ => "file"
            };

            return Json(new
            {
                url = $"/uploads/chat/{uniqueFileName}",
                type = attachmentType,
                fileName = file.FileName
            });
        }
    }
}