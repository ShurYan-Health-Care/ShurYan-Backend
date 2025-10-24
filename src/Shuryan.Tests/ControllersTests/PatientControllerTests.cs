using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shuryan.API.Controllers;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.Interfaces;
using Xunit;

namespace Shuryan.Tests.ControllersTests
{
    public class PatientControllerTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<IAppointmentService> _mockAppointmentService;
        private readonly Mock<ILogger<PatientsController>> _mockLogger;
        private readonly PatientsController _controller;

        public PatientControllerTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockAppointmentService = new Mock<IAppointmentService>();
            _mockLogger = new Mock<ILogger<PatientsController>>();
            _controller = new PatientsController(_mockPatientService.Object, _mockAppointmentService.Object, _mockLogger.Object);

            // Mock the User property to simulate an authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetMyProfile_ReturnsOkResult_WithPatientProfile()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var mockPatient = new PatientResponse { Id = patientId, FirstName = "John Doe" };

            _mockPatientService
                .Setup(service => service.GetPatientByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(mockPatient);

            // Act
            var result = await _controller.GetMyProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<PatientResponse>>(okResult.Value);
            Assert.Equal(patientId, response.Data.Id);
        }
    }
}
