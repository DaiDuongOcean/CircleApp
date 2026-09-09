using CircleApp.Controllers.Base;
using CircleApp.Data.Helpers.Constants;
using CircleApp.Data.Helpers.Enums;
using CircleApp.Data.Models;
using CircleApp.Data.Services;
using CircleApp.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;

namespace CircleApp.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _fileService;
        private readonly INotificationsService _notificationsService;

        private readonly IFriendsService _friendsService;


        public HomeController(ILogger<HomeController> logger, IPostsService postsService, IHashtagsService hashtagsService, IFilesService fileService, INotificationsService notificationsService, IFriendsService friendsService)
        {
            _logger = logger;
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _fileService = fileService;
            _notificationsService = notificationsService;

            _friendsService = friendsService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index");

            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var posts = await _postsService.SearchPostsAsync(query);
            var friends = await _friendsService.SearchFriendsAsync(query, loggedInUserId.Value);

            var result = new SearchResultVM
            {
                Query = query,
                Posts = posts,
                Friends = friends
            };

            return View("Search", result);
        }



        public async Task<IActionResult> Index()
        {

            var loggedInUserId = GetUserId();
            if (loggedInUserId == null)
                return RedirectToLogin();
            var allPosts = await _postsService.GetAllPostsAsync(loggedInUserId.Value);

            return View(allPosts);
        }

        public async Task<IActionResult> Details(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post)
        {
            var loggedInUserId = GetUserId();
            var userName = GetUserFullName();
            if (loggedInUserId == null)
                return RedirectToLogin();

            var imageUploadPath = await _fileService.UploadImageAsync(post.Image, ImageFileType.PostImage);

            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = loggedInUserId.Value,
            };

            var createdPost = await _postsService.CreatePostAsync(newPost);
            await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content);

            // Thông báo cho bạn bè biết có bài viết mới (bỏ qua nếu bài viết ở chế độ riêng tư)
            if (!createdPost.IsPrivate)
            {
                var friends = await _friendsService.GetFriendsAsync(loggedInUserId.Value);
                var friendIds = friends
                    .Select(f => f.SenderId == loggedInUserId.Value ? f.ReceiverId : f.SenderId)
                    .Distinct();

                foreach (var friendId in friendIds)
                {
                    await _notificationsService.AddNewNotificationAsync(friendId, NotificationType.NewPost, userName, createdPost.Id);
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> PostRemove(PostRemoveVM postRemoveVM, string returnUrl = null)
        {
            var postRemoved = await _postsService.RemovePostAsync(postRemoveVM.PostId);
            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(postRemoved.Content);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // Like
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostLike(PostLikeVM postLikeVM)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (userId == null) return RedirectToLogin();

            var result = await _postsService.TogglePostLikeAsync(postLikeVM.PostId, userId.Value);
            var post = await _postsService.GetPostByIdAsync(postLikeVM.PostId);

            if (result.SendNotification && userId != post.UserId)
                await _notificationsService.AddNewNotificationAsync(post.UserId, NotificationType.Like, userName, postLikeVM.PostId);

            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (userId == null)
                return RedirectToLogin();

            var newComment = new Comment()
            {
                UserId = userId.Value,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                ParentCommentId = postCommentVM.ParentCommentId,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            };

            await _postsService.AddPostCommentAsync(newComment);

            var post = await _postsService.GetPostByIdAsync(postCommentVM.PostId);

            // Thông báo cho chủ bài viết VÀ tất cả những ai từng bình luận/reply dưới bài viết này,
            // trừ chính người vừa gửi bình luận. Dùng HashSet để không gửi trùng cho cùng 1 người.
            var recipientIds = new HashSet<int> { post.UserId };
            CollectCommenterIds(post.Comments, recipientIds);
            recipientIds.Remove(userId.Value);

            foreach (var recipientId in recipientIds)
            {
                await _notificationsService.AddNewNotificationAsync(recipientId, NotificationType.Comment, userName, postCommentVM.PostId);
            }

            return PartialView("Home/_Post", post);
        }

        // Duyệt đệ quy toàn bộ comment + reply của 1 bài viết để lấy hết UserId đã từng bình luận
        private void CollectCommenterIds(IEnumerable<Comment> comments, HashSet<int> recipientIds)
        {
            if (comments == null) return;

            foreach (var comment in comments)
            {
                recipientIds.Add(comment.UserId);
                CollectCommenterIds(comment.Replies, recipientIds);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM)
        {
            await _postsService.RemovePostCommentAsync(removeCommentVM.CommentId);

            var post = await _postsService.GetPostByIdAsync(removeCommentVM.PostId);

            return PartialView("Home/_Post", post);
        }

        // Favorite
        [HttpPost]
        public async Task<IActionResult> TogglePostFavorite(PostFavoriteVM postFavoriteVM)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (userId == null) return RedirectToLogin();
            var result = await _postsService.TogglePostFavoriteAsync(postFavoriteVM.PostId,
                userId.Value);

            var post = await _postsService.GetPostByIdAsync(postFavoriteVM.PostId);

            if (result.SendNotification && userId != post.UserId)
                await _notificationsService.AddNewNotificationAsync(post.UserId, NotificationType.Favorite, userName, postFavoriteVM.PostId);

            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostVisibility(PostVisibilityVM postVisibilityVM)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null)
                return RedirectToLogin();
            await _postsService.TogglePostVisibilityAsync(postVisibilityVM.PostId, loggedInUserId.Value);

            return RedirectToAction("Index");
        }

        // Reports
        [HttpPost]
        public async Task<IActionResult> AddPostReport(PostReportVM postReportVM)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null)
                return RedirectToLogin();
            await _postsService.ReportPostAsync(postReportVM.PostId, loggedInUserId.Value);

            return RedirectToAction("Index");
        }
    }
}
