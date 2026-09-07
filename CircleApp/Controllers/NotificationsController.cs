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
            if (!userId.HasValue) RedirectToLogin();

            var count = await _notificationsService.GetUnreadNotificationsCountAsync(userId.Value);
            return Json(count);
        }

        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();
            if (!userId.HasValue) RedirectToLogin();

            var notifications = await _notificationsService.GetNotifications(userId.Value);
            return PartialView("Notifications/_Notifications", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> SetNotificationAsRead(int notificationId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) RedirectToLogin();
            await _notificationsService.SetNotificationAsReadAsync(notificationId);

            var notifications = await _notificationsService.GetNotifications(userId.Value);
            return PartialView("Notifications/_Notifications", notifications);
        }
    }
}
