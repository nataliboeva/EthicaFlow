using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace EthicaFlow.Models
{
    public class EthicsSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Methodology { get; set; }
        public string Participants { get; set; }
        public string Risks { get; set; }

        public string Status { get; set; } = "Draft";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int ResearcherId { get; set; }
        [ForeignKey("ResearcherId")]
        public virtual User Researcher { get; set; }

        public int? ReviewerId { get; set; }
        [ForeignKey("ReviewerId")]
        public virtual User Reviewer { get; set; }

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<ReviewDecision> ReviewDecisions { get; set; } = new List<ReviewDecision>();
    }
}