using CircleApp.Data.Dtos;
using CircleApp.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleApp.Data.Services
{
    public class PostsService : IPostsService
    {
        private readonly AppDbContext _context;
        private readonly INotificationsService _notificationsService;
        public PostsService(AppDbContext context, INotificationsService notificationsService)
        {
            _context = context;
            _notificationsService = notificationsService;
        }
        //public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        //{
        //    var allPosts = await _context.Posts
        //        .Where(p => (!p.IsPrivate || p.UserId == loggedInUserId) && p.Reports.Count < 5 && !p.IsDeleted)
        //        .Include(p => p.User)
        //        .Include(p => p.Likes)
        //        .Include(p => p.Favorites)
        //        .Include(p => p.Reports)
        //        .Include(p => p.Comments).ThenInclude(p => p.User)
        //        .OrderByDescending(p => p.DateCreated)
        //        .ToListAsync();

        //    return allPosts;
        //}
        //public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        //{
        //    var allPosts = await _context.Posts
        //        .AsNoTracking() // 1. Bỏ tracking để tiết kiệm tối đa RAM
        //        .AsSplitQuery()
        //        .Where(p => (!p.IsPrivate || p.UserId == loggedInUserId) && p.Reports.Count < 5 && !p.IsDeleted)
        //        .Include(p => p.User)
        //        .Include(p => p.Likes)
        //        .Include(p => p.Favorites)
        //        // .Include(p => p.Reports) <-- BẠN CÓ THỂ XÓA DÒNG NÀY (xem giải thích bên dưới)
        //        .Include(p => p.Comments)
        //            .ThenInclude(c => c.User) // 2. Đổi 'p => p.User' thành 'c => c.User' cho chuẩn (c là comment)
        //        .OrderByDescending(p => p.DateCreated)
        //        .AsSplitQuery() // 3. Tách truy vấn để chống bùng nổ dữ liệu (Cartesian Explosion)
        //        .ToListAsync();

        //    return allPosts;
        //}
        public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        {
            var allPosts = await _context.Posts
                .AsNoTracking()
                .AsSplitQuery()
                .Where(p => (!p.IsPrivate || p.UserId == loggedInUserId) && p.Reports.Count < 5 && !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Favorites)
                // Nhánh 1: Lấy User của bình luận gốc
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                // Nhánh 2: Lấy bình luận con (Replies) và User của bình luận con
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();

            return allPosts;
        }

        //public async Task<Post> GetPostByIdAsync(int postId)
        //{
        //    var postDb = await _context.Posts
        //        .Include(p => p.User)
        //        .Include(p => p.Likes)
        //        .Include(p => p.Favorites)
        //        .Include(p => p.Comments).ThenInclude(p => p.User)
        //        .FirstOrDefaultAsync(p => p.Id == postId);

        //    return postDb;
        //}
        //public async Task<Post> GetPostByIdAsync(int postId)
        //{
        //    var postDb = await _context.Posts
        //        .AsNoTracking()
        //        .AsSplitQuery() // Tách thành các truy vấn nhỏ để tránh JOIN quá lớn
        //        .Include(p => p.User)
        //        .Include(p => p.Likes)
        //        .Include(p => p.Favorites)
        //        .Include(p => p.Comments).ThenInclude(p => p.User)
        //        .FirstOrDefaultAsync(p => p.Id == postId);

        //    return postDb;
        //}
        public async Task<Post> GetPostByIdAsync(int postId)
        {
            var postDb = await _context.Posts
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Favorites)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                // Thêm đoạn này cho chi tiết bài viết
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == postId);

            return postDb;
        }

        //public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        //{
        //    var allFavoritedPosts = await _context.Favorites
        //        .Include(f => f.Post.Reports)
        //        .Include(f => f.Post.User)
        //        .Include(f => f.Post.Comments)
        //            .ThenInclude(c => c.User)
        //        .Include(f => f.Post.Likes)
        //        .Include(f => f.Post.Favorites)
        //        .Where(n => n.UserId == loggedInUserId &&
        //            !n.Post.IsDeleted &&
        //            n.Post.Reports.Count < 5)
        //        .OrderByDescending(f => f.DateCreated)
        //        .Select(n => n.Post)
        //        .ToListAsync();

        //    return allFavoritedPosts;
        //}
        //public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        //{
        //    var allFavoritedPosts = await _context.Favorites
        //        .AsNoTracking() // Tối ưu 1: Không theo dõi (tracking) dữ liệu, giảm thiểu ăn RAM
        //        .AsSplitQuery()
        //        .Include(f => f.Post.Reports)
        //        .Include(f => f.Post.User)
        //        .Include(f => f.Post.Comments)
        //            .ThenInclude(c => c.User)
        //        .Include(f => f.Post.Likes)
        //        .Include(f => f.Post.Favorites)
        //        .Where(n => n.UserId == loggedInUserId &&
        //                    !n.Post.IsDeleted &&
        //                    n.Post.Reports.Count < 5)
        //        .OrderByDescending(f => f.DateCreated)
        //        .Select(n => n.Post)
        //        .AsSplitQuery() // Tối ưu 2: Tách truy vấn, chống "bùng nổ" JOIN bảng
        //        .ToListAsync();

        //    return allFavoritedPosts;
        //}
        //public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId, int pageIndex = 0, int pageSize = 10)
        //{
        //    // BƯỚC 1: Tìm đúng danh sách PostId ở trang hiện tại (Truy vấn siêu nhẹ)
        //    var pagedPostIds = await _context.Favorites
        //        .AsNoTracking()
        //        .Where(f => f.UserId == loggedInUserId &&
        //                    !f.Post.IsDeleted &&
        //                    f.Post.Reports.Count < 5)
        //        .OrderByDescending(f => f.DateCreated)
        //        .Skip(pageIndex * pageSize)
        //        .Take(pageSize)
        //        .Select(f => f.PostId)
        //        .ToListAsync();

        //    if (!pagedPostIds.Any())
        //    {
        //        return new List<Post>();
        //    }

        //    // BƯỚC 2: Chỉ lấy chi tiết (Comments, Likes...) cho đúng pageSize bài viết đó
        //    var favoritedPosts = await _context.Posts
        //        .AsNoTracking()
        //        .AsSplitQuery()
        //        .Include(p => p.User)
        //        .Include(p => p.Comments).ThenInclude(c => c.User)
        //        .Include(p => p.Likes)
        //        .Include(p => p.Favorites)
        //        .Where(p => pagedPostIds.Contains(p.Id))
        //        .ToListAsync();

        //    // BƯỚC 3: Xếp lại thứ tự trên RAM vì toán tử .Contains() trong SQL không giữ được thứ tự DateCreated ban đầu
        //    var orderedPosts = pagedPostIds
        //        .Select(id => favoritedPosts.First(p => p.Id == id))
        //        .ToList();

        //    return orderedPosts;
        //}
        public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId, int pageIndex = 0, int pageSize = 10)
        {
            var pagedPostIds = await _context.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == loggedInUserId && !f.Post.IsDeleted && f.Post.Reports.Count < 5)
                .OrderByDescending(f => f.DateCreated)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(f => f.PostId)
                .ToListAsync();

            if (!pagedPostIds.Any()) return new List<Post>();

            var favoritedPosts = await _context.Posts
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Favorites)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                // Thêm đoạn này cho bài viết yêu thích
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .Where(p => pagedPostIds.Contains(p.Id))
                .ToListAsync();

            var orderedPosts = pagedPostIds
                .Select(id => favoritedPosts.First(p => p.Id == id))
                .ToList();

            return orderedPosts;
        }

        public async Task AddPostCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<Post> RemovePostAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            if(postDb != null)
            {
                // _context.Posts.Remove(postDb);
                postDb.IsDeleted = true;
                _context.Posts.Update(postDb);
                await _context.SaveChangesAsync();
            }

            return postDb;
        }

        //public async Task RemovePostCommentAsync(int commentId)
        //{
        //    var commentDb = _context.Comments.FirstOrDefault(c => c.Id == commentId);
        //    if(commentDb != null)
        //    {
        //        _context.Comments.Remove(commentDb);
        //        await _context.SaveChangesAsync();
        //    }
        //}
        public async Task RemovePostCommentAsync(int commentId)
        {
            // BƯỚC 1: Tìm bình luận và nạp luôn danh sách các câu trả lời của nó
            var commentDb = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (commentDb != null)
            {
                // BƯỚC 2: Kiểm tra xem nó có bình luận con không. Nếu có, xóa hết sạch bọn chúng.
                if (commentDb.Replies != null && commentDb.Replies.Any())
                {
                    _context.Comments.RemoveRange(commentDb.Replies);
                }

                // BƯỚC 3: Cuối cùng mới xóa bình luận gốc
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReportPostAsync(int postId, int userId)
        {
            var newReport = new Report()
            {
                PostId = postId,
                UserId = userId,
                DateCreated = DateTime.UtcNow
            };

            await _context.Reports.AddAsync(newReport);
            await _context.SaveChangesAsync();

            var post = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);
            if(post != null)
            {
                post.NrOfReports += 1;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<GetNotificationDto> TogglePostFavoriteAsync(int postId, int userId)
        {
            var response = new GetNotificationDto()
            {
                Success = true,
                SendNotification = false
            };

            // Check if user has a already liked the post
            var favorite = await _context.Favorites
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postId,
                    UserId = userId,
                    DateCreated = DateTime.UtcNow
                };
                await _context.Favorites.AddAsync(newFavorite);
                await _context.SaveChangesAsync();

                response.SendNotification = true;
            }
            return response;
        }

        public async Task<GetNotificationDto> TogglePostLikeAsync(int postId, int userId)
        {
            var response = new GetNotificationDto()
            {
                Success = true,
                SendNotification = false
            };

            // Check if user has a already liked the post
            var like = await _context.Likes
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.Likes.AddAsync(newLike);
                await _context.SaveChangesAsync();

                response.SendNotification = true;
            }
            return response;
        }
        //public async Task TogglePostLikeAsync(int postId, int userId)
        //{
        //    // Cố gắng xóa Like trực tiếp trên DB (Không cần SELECT trước)
        //    // ExecuteDeleteAsync trả về số dòng bị ảnh hưởng (số dòng đã bị xóa)
        //    int deletedRows = await _context.Likes
        //        .Where(l => l.PostId == postId && l.UserId == userId)
        //        .ExecuteDeleteAsync();

        //    // Nếu không có dòng nào bị xóa, nghĩa là user chưa like -> Thực hiện Like
        //    if (deletedRows == 0)
        //    {
        //        var newLike = new Like()
        //        {
        //            PostId = postId,
        //            UserId = userId
        //        };

        //        // Dùng Add thay vì AddAsync
        //        _context.Likes.Add(newLike);
        //        await _context.SaveChangesAsync();
        //    }
        //}

        public async Task TogglePostVisibilityAsync(int postId, int userId)
        {
            // get post by id and loggedin user id
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);

            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Post>> SearchPostsAsync(string query)
        {
            // Tìm các bài viết mà nội dung có chứa từ khóa
            return await _context.Posts
                .Include(p => p.User)
                .Where(p => p.Content.Contains(query))
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();
        }
    }
}
