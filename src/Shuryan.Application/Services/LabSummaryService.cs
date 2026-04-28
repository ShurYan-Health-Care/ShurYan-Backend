using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.LabTests;
using Shuryan.Application.DTOs.Responses.LabTests;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Identity;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shuryan.Application.Services
{
    public class LabSummaryService : ILabSummaryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPatientService _patientService;
        private readonly IPatientLabService _patientLabService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LabSummaryService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public LabSummaryService(
            IHttpClientFactory httpClientFactory,
            IPatientService patientService,
            IPatientLabService patientLabService,
            IConfiguration configuration,
            ILogger<LabSummaryService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _patientService = patientService;
            _patientLabService = patientLabService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LabSummaryResponse> SummarizeLabOrderAsync(Guid patientId, Guid orderId)
        {
            _logger.LogInformation("Summarizing lab order {OrderId} for patient {PatientId}", orderId, patientId);

            var patient = await _patientService.GetCurrentPatientAsync(patientId);
            if (patient == null)
                throw new KeyNotFoundException("المريض غير موجود");

            var results = (await _patientLabService.GetLabOrderResultsAsync(patientId, orderId)).ToList();
            if (!results.Any())
                throw new InvalidOperationException("لا توجد نتائج لهذا الطلب بعد");

            var age = patient.BirthDate.HasValue
                ? (int)Math.Floor((DateTime.UtcNow - patient.BirthDate.Value).TotalDays / 365.25)
                : 0;

            var genderString = patient.Gender == Gender.Female ? "female" : "male";

            var requestBody = new LabSummarizeRequest
            {
                Patient = new LabSummarizePatientInfo
                {
                    Age = age,
                    Gender = genderString,
                    Language = "ar"
                },
                Results = results.Select(r => new LabSummarizeResult
                {
                    TestCode = r.TestCode,
                    TestName = r.TestName,
                    ResultValue = r.ResultValue,
                    ReferenceRange = r.ReferenceRange ?? string.Empty,
                    Unit = r.Unit ?? string.Empty,
                    Notes = r.Notes ?? string.Empty
                }).ToList()
            };

            var baseUrl = _configuration["LabSummaryApi:BaseUrl"]
                ?? "https://diab7-shuryan-labsummary.hf.space";

            var client = _httpClientFactory.CreateClient("LabSummaryClient");

            // var payloadJson = JsonSerializer.Serialize(requestBody, _jsonOptions);
            // _logger.LogInformation("ML API payload: {Payload}", payloadJson);
            _logger.LogInformation("Calling ML summarization API at {BaseUrl}", baseUrl);

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await client.PostAsJsonAsync(
                    $"{baseUrl.TrimEnd('/')}/api/v1/lab-results/summarize",
                    requestBody,
                    _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to reach ML summarization API");
                throw new InvalidOperationException("تعذر الاتصال بخدمة التلخيص، يرجى المحاولة لاحقاً");
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError("ML API returned {StatusCode}: {Body}", httpResponse.StatusCode, errorBody);
                throw new InvalidOperationException("فشل في توليد الملخص من خدمة الذكاء الاصطناعي");
            }

            var mlResponse = await httpResponse.Content.ReadFromJsonAsync<MlApiResponse>(_jsonOptions);
            if (mlResponse == null || string.IsNullOrWhiteSpace(mlResponse.SummaryAr))
                throw new InvalidOperationException("استجابة غير صالحة من خدمة التلخيص");

            _logger.LogInformation("Successfully generated Arabic summary for order {OrderId}", orderId);

            return new LabSummaryResponse { SummaryAr = mlResponse.SummaryAr };
        }

        /// <summary>
        /// Internal model matching the ML API JSON response shape
        /// </summary>
        private class MlApiResponse
        {
            public string SummaryAr { get; set; } = string.Empty;
        }
    }
}
