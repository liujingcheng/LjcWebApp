using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LjcWebApp.Models.ViewModels
{
    public class UploadModel
    {
        [Required]
        [Display(Name = "附件")]
        [FileExtensions(Extensions = ".xml", ErrorMessage = "只上传xml格式的文件")]
        public IFormFile UploadedFile { get; set; }
    }
}
