namespace CircleApp.ViewModels.Home
{
    public class PostCommentVM
    {
        public int PostId { get; set; }
        public string Content { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
