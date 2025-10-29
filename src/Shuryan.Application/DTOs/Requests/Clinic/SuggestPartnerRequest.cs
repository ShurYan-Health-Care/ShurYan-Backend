using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    /// <summary>
    /// Request لاقتراح شريك (صيدلية أو معمل)
    /// الدكتور ممكن يضيف صيدلية واحدة و/أو معمل واحد
    /// </summary>
    public class SuggestPartnerRequest
    {
        /// <summary>
        /// معرف الصيدلية المقترحة (اختياري)
        /// </summary>
        public Guid? PharmacyId { get; set; }

        /// <summary>
        /// معرف المعمل المقترح (اختياري)
        /// </summary>
        public Guid? LaboratoryId { get; set; }
    }
}
