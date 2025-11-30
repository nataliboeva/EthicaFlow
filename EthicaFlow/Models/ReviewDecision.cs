using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EthicaFlow.Models
{
    public class ReviewDecision
    {
        [Key]
        public int DecisionId { get; set; }

        [Required]
        public int SubmissionId { get; set; }
        [ForeignKey("SubmissionId")]
        public virtual EthicsSubmission Submission { get; set; }

        [Required]
        public int ReviewerId { get; set; }
        [ForeignKey("ReviewerId")]
        public virtual User Reviewer { get; set; }

        [Required]
        public string Decision { get; set; } // "Approved" or "Revision Required"

        public string Comments { get; set; }

        public DateTime DecisionDate { get; set; } = DateTime.Now;
    }
}

