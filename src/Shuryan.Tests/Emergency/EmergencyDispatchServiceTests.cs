using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shuryan.Application.DTOs.Requests.Emergency;
using Shuryan.Application.Interfaces;
using Shuryan.Application.Services;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;
using Xunit;
using System.Text.Json;

namespace Shuryan.Tests.Emergency
{
    public class EmergencyDispatchServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly Mock<IEmergencyEventRepository> _mockEmergencyEventRepo;
        private readonly Mock<IEmergencyAuditService> _mockAuditService;
        private readonly Mock<INotificationHubService> _mockNotificationHub;
        private readonly Mock<ILogger<EmergencyDispatchService>> _mockLogger;
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly EmergencyDispatchService _sut;

        public EmergencyDispatchServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockEmergencyEventRepo = new Mock<IEmergencyEventRepository>();
            _mockAuditService = new Mock<IEmergencyAuditService>();
            _mockNotificationHub = new Mock<INotificationHubService>();
            _mockLogger = new Mock<ILogger<EmergencyDispatchService>>();
            _mockPatientService = new Mock<IPatientService>();

            _mockUnitOfWork.Setup(u => u.Patients).Returns(_mockPatientRepo.Object);
            _mockUnitOfWork.Setup(u => u.EmergencyEvents).Returns(_mockEmergencyEventRepo.Object);

            _sut = new EmergencyDispatchService(
                _mockUnitOfWork.Object,
                _mockAuditService.Object,
                _mockNotificationHub.Object,
                _mockPatientService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task DispatchSosAsync_ShouldSucceed_WhenEmergencyModeActive()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            
            var patient = new Core.Entities.Identity.Patient 
            { 
                Id = patientId,
                EmergencyModeActive = true,
                EmergencyModeActivatedById = doctorId
            };

            var request = new SosDispatchRequest
            {
                Latitude = 30.0444,
                Longitude = 31.2357,
                Timestamp = DateTime.UtcNow
            };

            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync(patient);
            _mockPatientService.Setup(s => s.GetPatientMedicalRecordAsync(patientId))
                .ReturnsAsync(new Application.DTOs.Responses.Patient.MedicalRecordResponse());

            // Act
            var result = await _sut.DispatchSosAsync(patientId, request);

            // Assert
            result.Should().BeTrue();
            _mockEmergencyEventRepo.Verify(r => r.AddAsync(It.Is<EmergencyEvent>(e => 
                e.PatientId == patientId && 
                e.ActivatingDoctorId == doctorId &&
                e.Latitude == 30.0444 &&
                e.Longitude == 31.2357 &&
                e.Status == EmergencyEventStatus.Dispatched
            )), Times.Once);
            
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            
            _mockAuditService.Verify(s => s.LogActionAsync(doctorId, patientId, EmergencyActionType.SosDispatched), Times.Once);
            
            _mockNotificationHub.Verify(h => h.SendNotificationToUserAsync(
                doctorId, 
                "حالة طوارئ", 
                It.Is<string>(msg => msg.Contains("تفعيل زر الطوارئ")), 
                It.IsAny<object>()), 
            Times.Once);
        }

        [Fact]
        public async Task DispatchSosAsync_ShouldFail_WhenEmergencyModeInactive()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = new Core.Entities.Identity.Patient 
            { 
                Id = patientId,
                EmergencyModeActive = false
            };

            var request = new SosDispatchRequest { Timestamp = DateTime.UtcNow };

            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync(patient);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DispatchSosAsync(patientId, request));
        }
    }
}
