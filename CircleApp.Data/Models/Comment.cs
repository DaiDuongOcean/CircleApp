using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleApp.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        // Foreign keys
        public int PostId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public User User { get; set; }

        public int? ParentCommentId { get; set; }
        public virtual Comment ParentComment { get; set; } // Thêm dòng này để EF Core hiểu đây là bình luận gốc
        public virtual ICollection<Comment> Replies { get; set; }
    }
}
