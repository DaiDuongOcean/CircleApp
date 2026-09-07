using CircleApp.Controllers.Base;
using CircleApp.Data.Helpers.Constants;
using CircleApp.Data.Services;
using CircleApp.ViewModels.Friends;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CircleApp.Controllers
{
    [Authorize]
    public class FriendsController : BaseController
    {
        private readonly IFriendsService _friendsService;
        private readonly INotificationsService _notificationsService;
        public FriendsController(IFriendsService friendsService, INotificationsService notificationsService)
        {
            _friendsService = friendsService;
            _notificationsService = notificationsService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue) RedirectToLogin();

            var friendsData = new FriendshipVM()
            {
                Friends = await _friendsService.GetFriendsAsync(userId.Value),
                FriendRequestsSent = await _friendsService.GetSentFriendRequestAsync(userId.Value),
                FriendRequestsReceived = await _friendsService.GetReceivedFriendRequestAsync(userId.Value)
            };

            return View(friendsData);
        }

        //[HttpPost]
        //public async Task<IActionResult> SendFriendRequest(int receiverId)
        //{
        //    var userId = GetUserId();
        //    var userName = GetUserFullName();
        //    if (!userId.HasValue) RedirectToLogin();

        //    await _friendsService.SendRequestAsync(userId.Value, receiverId);

        //    await _notificationsService.AddNewNotificationAsync(receiverId, NotificationType.FriendRequest, userName, null);

        //    return RedirectToAction("Index", "Home");
        //}
        [HttpPost]
        public async Task<IActionResult> SendFriendRequest(int receiverId, string returnUrl = null)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue) RedirectToLogin();
            await _friendsService.SendRequestAsync(userId.Value, receiverId);
            await _notificationsService.AddNewNotificationAsync(receiverId, NotificationType.FriendRequest, userName, null);

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        //[HttpPost]
        //public async Task<IActionResult> CancelFriendRequest(int requestId)
        //{
        //    await _friendsService.UpdateRequestAsync(requestId, FriendshipStatus.Canceled);
        //    return RedirectToAction("Index");
        //}

        //[HttpPost]
        //public async Task<IActionResult> AcceptFriendRequest(int requestId)
        //{
        //    await _friendsService.UpdateRequestAsync(requestId, FriendshipStatus.Accepted);
        //    return RedirectToAction("Index");
        //}

        //[HttpPost]
        //public async Task<IActionResult> RejectFriendRequest(int requestId)
        //{
        //    await _friendsService.UpdateRequestAsync(requestId, FriendshipStatus.Rejected);
        //    return RedirectToAction("Index");
        //}

        //[HttpPost]
        //public async Task<IActionResult> UpdateFriendRequest(int requestId, string status)
        //{
        //    var userId = GetUserId();
        //    var userName = GetUserFullName();
        //    if (!userId.HasValue) RedirectToLogin();

        //     var request = await _friendsService.UpdateRequestAsync(requestId, status);

        //    if (status == FriendshipStatus.Accepted)
        //        await _notificationsService.AddNewNotificationAsync(request.SenderId, NotificationType.FriendRequestApproved, userName, null);

        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public async Task<IActionResult> UpdateFriendRequest(int requestId, string status, string returnUrl = null)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue) RedirectToLogin();
            var request = await _friendsService.UpdateRequestAsync(requestId, status);
            if (status == FriendshipStatus.Accepted)
                await _notificationsService.AddNewNotificationAsync(request.SenderId, NotificationType.FriendRequestApproved, userName, null);

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public async Task<IActionResult> RemoveFriend(int friendshipId)
        //{
        //    await _friendsService.RemoveFriendAsync(friendshipId);
        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public async Task<IActionResult> RemoveFriend(int friendshipId, string returnUrl = null)
        {
            await _friendsService.RemoveFriendAsync(friendshipId);

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
    }
}
