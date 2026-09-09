using CircleApp.Controllers.Base;
using CircleApp.Data.Helpers.Constants;
using CircleApp.Data.Models;
using CircleApp.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CircleApp.Controllers
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        public INotificationsService _notificationsService { get; set; }
        public NotificationsController(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var count = await _notificationsService.GetUnreadNotificationsCountAsync(userId.Value);
            return Json(count);
        }

        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var notifications = await _notificationsService.GetNotifications(userId.Value);
            return PartialView("Notifications/_Notifications", notifications);
        }

        // Giữ lại cho trường hợp cần reload cả danh sách kèm partial view
        [HttpPost]
        public async Task<IActionResult> SetNotificationAsRead(int notificationId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();
            await _notificationsService.SetNotificationAsReadAsync(notificationId);

            var notifications = await _notificationsService.GetNotifications(userId.Value);
            return PartialView("Notifications/_Notifications", notifications);
        }

        // Endpoint nhẹ: đánh dấu đã đọc + trả về số lượng chưa đọc mới nhất.
        // Dùng khi người dùng click trực tiếp vào 1 thông báo (trang sẽ điều hướng đi ngay sau đó).
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            await _notificationsService.SetNotificationAsReadAsync(notificationId);
            var count = await _notificationsService.GetUnreadNotificationsCountAsync(userId.Value);
            return Json(new { count });
        }
    }
}
