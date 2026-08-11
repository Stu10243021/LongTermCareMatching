using System;
using System.ComponentModel.DataAnnotations;

namespace LongTermCareMatching.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } 

        public string Title { get; set; } 

        public string Message { get; set; } 

        public string Url { get; set; } 

        public bool IsRead { get; set; } = false; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}