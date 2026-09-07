using CircleApp.Controllers.Base;
using CircleApp.Data.Helpers.Constants;
using CircleApp.Data.Models;
using CircleApp.Data.Services;
using CircleApp.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CircleApp.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly UserManager<User> _userManager;
        private readonly IFriendsService _friendsService;

        public UsersController(IUsersService usersService, UserManager<User> userManager, IFriendsService friendsService)
        {
            _usersService = usersService;
            _userManager = userManager;
            _friendsService = friendsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var userPosts = await _usersService.GetUserPosts(userId);

            var userProfileVM = new GetUserProfileVM()
            {
                User = user,
                Posts = userPosts,
            };

            var currentUserId = GetUserId();
            userProfileVM.CurrentUserId = currentUserId ?? 0;

            if (currentUserId.HasValue)
            {
                if (currentUserId.Value == userId)
                {
                    userProfileVM.RelationshipStatus = "Self";
                }
                else
                {
                    var friends = await _friendsService.GetFriendsAsync(currentUserId.Value);
                    var existingFriendship = friends.FirstOrDefault(f =>
                        f.SenderId == userId || f.ReceiverId == userId);

                    if (existingFriendship != null)
                    {
                        userProfileVM.RelationshipStatus = "Friends";
                        userProfileVM.FriendshipId = existingFriendship.Id;
                    }
                    else
                    {
                        var sentRequests = await _friendsService.GetSentFriendRequestAsync(currentUserId.Value);
                        var sentToThisUser = sentRequests.FirstOrDefault(r => r.ReceiverId == userId);

                        if (sentToThisUser != null)
                        {
                            userProfileVM.RelationshipStatus = "PendingSent";
                            userProfileVM.FriendRequestId = sentToThisUser.Id;
                        }
                        else
                        {
                            var receivedRequests = await _friendsService.GetReceivedFriendRequestAsync(currentUserId.Value);
                            var receivedFromThisUser = receivedRequests.FirstOrDefault(r => r.SenderId == userId);

                            if (receivedFromThisUser != null)
                            {
                                userProfileVM.RelationshipStatus = "PendingReceived";
                                userProfileVM.FriendRequestId = receivedFromThisUser.Id;
                            }
                            else
                            {
                                userProfileVM.RelationshipStatus = "None";
                            }
                        }
                    }
                }
            }

            var profileUserFriendships = await _friendsService.GetFriendsAsync(userId);

            userProfileVM.FriendsCount = profileUserFriendships.Count;
            userProfileVM.FriendsList = profileUserFriendships
                .Select(f => f.SenderId == userId ? f.Receiver : f.Sender)
                .ToList();

            return View(userProfileVM);
        }
    }
}