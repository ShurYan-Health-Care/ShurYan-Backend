namespace Shuryan.Application.DTOs.Requests.LabTests
{
    public class LabSummarizeRequest
    {
        public LabSummarizePatientInfo Patient { get; set; } = new();
        public List<LabSummarizeResult> Results { get; set; } = new();
    }

    public class LabSummarizePatientInfo
    {
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Language { get; set; } = "ar";
    }

    public class LabSummarizeResult
    {
        public string TestCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string ResultValue { get; set; } = string.Empty;
        public string ReferenceRange { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
