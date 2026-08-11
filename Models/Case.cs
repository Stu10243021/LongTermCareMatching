using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LongTermCareMatching.Models
{
    public class Case
    {
        [Key]
        public int Id { get; set; }

        public int FamilyUserId { get; set; }

        [Required(ErrorMessage = "請填寫需求標題")]
        [Display(Name = "需求標題")]
        public string Title { get; set; }

        [Required(ErrorMessage = "請選擇服務類別")]
        [Display(Name = "服務類別")]
        public string ServiceType { get; set; }

        public string? Hospital { get; set; }
        public string? Department { get; set; }

        [Required(ErrorMessage = "請選擇服務地區")]
        [Display(Name = "服務地區")]
        public string Location { get; set; }

        public string? Address { get; set; }

        public string? ServiceTime { get; set; }

        public string? Description { get; set; }

        [Display(Name = "預估金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedBudget { get; set; }

        public string PaymentStatus { get; set; } = "Pending";

        public string Status { get; set; } = "Open";

        public int? CaregiverUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}