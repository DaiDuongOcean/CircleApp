using System;

namespace CircleApp.Data.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime DateSent { get; set; }
        public bool IsRead { get; set; } // Đánh dấu đã xem hay chưa

        // MỚI: thông tin file đính kèm (ảnh, pdf, word...)
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; } // "image", "pdf", "doc", "excel", "ppt", "file"


        // Người gửi
        public int SenderId { get; set; }
        public virtual User Sender { get; set; }

        // Người nhận
        public int ReceiverId { get; set; }
        public virtual User Receiver { get; set; }
    }
}