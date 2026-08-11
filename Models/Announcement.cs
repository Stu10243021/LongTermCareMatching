using System;
using System.ComponentModel.DataAnnotations;

namespace LongTermCareMatching.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "請輸入公告標題")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "請輸入公告內容")]
        public string Content { get; set; }

        public string Category { get; set; } = "System";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPinned { get; set; } = false;
    }
}