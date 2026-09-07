using CircleApp.Data.Dtos;
using CircleApp.Data.Models;
using System.Collections.Generic;

namespace CircleApp.ViewModels.Home
{
    public class SearchResultVM
    {
        public string Query { get; set; }
        public List<Post> Posts { get; set; }
        public List<UserWithFriendsCountDto> Friends { get; set; }
    }
}
