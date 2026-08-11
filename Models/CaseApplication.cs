using System;
using System.ComponentModel.DataAnnotations;

namespace LongTermCareMatching.Models
{
    public class CaseApplication
    {
        [Key]
        public int Id { get; set; }

        public int CaseId { get; set; }

        public int CaregiverUserId { get; set; }
        public string CaregiverName { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }
}