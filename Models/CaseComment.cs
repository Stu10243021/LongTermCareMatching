using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LongTermCareMatching.Models
{
    public class CaseComment
    {
        [Key]
        public int Id { get; set; }

        public int CaseId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = "";

        public string UserRole { get; set; } = "";

        [Required(ErrorMessage = "請輸入留言內容")]
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}