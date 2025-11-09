using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Verifier
{
    /// <summary>
    /// Request لرفض مستند الدكتور
    /// </summary>
    public class RejectDocumentRequest
    {
        /// <summary>
        /// سبب رفض المستند (اختياري)
        /// </summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}
