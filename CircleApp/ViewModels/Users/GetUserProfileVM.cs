using CircleApp.Data.Models;
namespace CircleApp.ViewModels.Users
{
    public class GetUserProfileVM
    {
        public User User { get; set; }
        public List<Post> Posts { get; set; }

        public int CurrentUserId { get; set; }
        public string RelationshipStatus { get; set; } = "None";
        public int? FriendshipId { get; set; }
        public int? FriendRequestId { get; set; }

        
        public List<User> FriendsList { get; set; } = new List<User>();
        public int FriendsCount { get; set; }
    }
}