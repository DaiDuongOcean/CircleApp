using CircleApp.Data.Helpers.Enums;
using CircleApp.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CircleApp.Data.Services
{
    public class FilesService : IFilesService
    {
        public async Task<string> UploadImageAsync(IFormFile file, ImageFileType imageFileType)
        {
            string filePathUpload = imageFileType switch
            {
                ImageFileType.PostImage => Path.Combine("images","posts"),
                ImageFileType.StoryImage => Path.Combine("images", "stories"),
                ImageFileType.ProfilePicture => Path.Combine("images", "profilePictures"),
                ImageFileType.CoverImage => Path.Combine("images", "covers"),
                _ => throw new ArgumentException("Invalid file type")
            };
            // Check and save the image
            if (file != null && file.Length > 0)
            {
                string rootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                // Ngăn người dùng chọn nhầm kiểu file
                if (file.ContentType.Contains("image"))
                {
                    string rootFolderPathImages = Path.Combine(rootFolderPath, filePathUpload);
                    /* 
                    Directory.CreateDirectory(...): Lệnh này rất thông minh. 
                    Nếu thư mục images chưa tồn tại, nó sẽ tạo mới. 
                    Nếu đã tồn tại rồi, nó sẽ bỏ qua chứ không báo lỗi.
                    */
                    Directory.CreateDirectory(rootFolderPathImages);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(rootFolderPathImages, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    // Set the URL to newPost object (trong đó đường dẫn đến wwwroot được thêm tự động)
                    return $"{filePathUpload}/{fileName}";
                }
            }
            return "";
        }
    }
}
