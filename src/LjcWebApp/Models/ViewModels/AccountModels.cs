using System.ComponentModel.DataAnnotations;

namespace LjcWebApp.Models.ViewModels
{
    public class AccountModels
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "必填")]
        [StringLength(20, ErrorMessage = "请输入5-20个字符内！", MinimumLength = 5)]
        [Display(Name = "用户名")]
        [RegularExpression(@"^([a-zA-Z0-9]+)$", ErrorMessage = "请输入正确的用户名")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [StringLength(30, ErrorMessage = "请输入5-30个字符内！", MinimumLength = 5)]
        [Required(ErrorMessage = "必填")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

    }
}
