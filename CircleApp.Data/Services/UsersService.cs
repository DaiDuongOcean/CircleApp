using CircleApp.Data.Models;
using CircleApp.Migrations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleApp.Data.Services
{
    public class UsersService : IUsersService
    {
        private readonly AppDbContext _appDbContext;
        public UsersService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<User> GetUser(int loggedInUserId)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == loggedInUserId) ?? new User();
        }

        public async Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl)
        {
            var userDb = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == loggedInUserId);
            if(userDb != null)
            {
                userDb.ProfilePictureUrl = profilePictureUrl;
                _appDbContext.Users.Update(userDb);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Post>> GetUserPosts(int userId)
        {
            var allPosts = await _appDbContext.Posts
                .AsNoTracking() // 1. Bỏ tracking để tiết kiệm tối đa RAM
                .AsSplitQuery()
                .Where(p => p.UserId == userId && p.Reports.Count < 5 && !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Favorites)
                // .Include(p => p.Reports) <-- BẠN CÓ THỂ XÓA DÒNG NÀY (xem giải thích bên dưới)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User) // 2. Đổi 'p => p.User' thành 'c => c.User' cho chuẩn (c là comment)
                .OrderByDescending(p => p.DateCreated)
                .AsSplitQuery() // 3. Tách truy vấn để chống bùng nổ dữ liệu (Cartesian Explosion)
                .ToListAsync();

            return allPosts;
        }
    }
}
