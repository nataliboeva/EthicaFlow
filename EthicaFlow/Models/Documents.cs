using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EthicaFlow.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public int SubmissionId { get; set; }
        [ForeignKey("SubmissionId")]
        public virtual EthicsSubmission Submission { get; set; }
    }
}