using System;
using System.ComponentModel.DataAnnotations;

namespace LongTermCareMatching.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請輸入姓名")]
        [Display(Name = "姓名")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "請輸入 Email")]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        [Display(Name = "電子郵件")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = "";

        [Required]
        [Display(Name = "身分角色")]
        public string Role { get; set; } = "";


        [Display(Name = "服務/居住區域")]
        public string Area { get; set; } = "";

        [Display(Name = "證照檔案路徑")]
        public string? CertificateUrl { get; set; }

        [Display(Name = "審核狀態")]
        public bool IsApproved { get; set; } = false;
        public bool IsBanned { get; set; } = false; // 預設未停權

        [Display(Name = "註冊時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}