using CircleApp.Data.Helpers;
using CircleApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CircleApp.Data.Services
{
    public class HashtagsService : IHashtagsService
    {
        private readonly AppDbContext _context;
        public HashtagsService(AppDbContext context)
        {
            _context = context;
        }
        public async Task ProcessHashtagsForNewPostAsync(string content)
        {
            var postHashtags = HashtagHelper.GetHashtags(content);
            foreach (var hashtag in postHashtags)
            {
                var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(h => h.Name == hashtag);
                if (hashtagDb != null)
                {
                    hashtagDb.Count += 1;
                    hashtagDb.DateUpdated = DateTime.UtcNow;
                    _context.Hashtags.Update(hashtagDb);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var newHashtag = new Hashtag()
                    {
                        Name = hashtag,
                        Count = 1,
                        DateCreated = DateTime.UtcNow,
                        DateUpdated = DateTime.UtcNow,
                    };
                    await _context.Hashtags.AddAsync(newHashtag);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task ProcessHashtagsForRemovedPostAsync(string content)
        {
            var postHashtags = HashtagHelper.GetHashtags(content);
            foreach (var hashtag in postHashtags)
            {
                var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(h => h.Name == hashtag);
                if (hashtagDb != null)
                {
                    hashtagDb.Count -= 1;
                    hashtagDb.DateUpdated = DateTime.UtcNow;

                    // MỚI: nếu không còn bài viết nào dùng hashtag này -> xóa hẳn khỏi DB
                    if (hashtagDb.Count <= 0)
                    {
                        _context.Hashtags.Remove(hashtagDb);
                    }
                    else
                    {
                        _context.Hashtags.Update(hashtagDb);
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}