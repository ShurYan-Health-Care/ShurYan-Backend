using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shuryan.Application.Interfaces;
using Shuryan.Application.Services;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;
using Xunit;

namespace Shuryan.Tests.Emergency
{
    public class EmergencyDashboardTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
        private readonly Mock<IEmergencyEventRepository> _mockEventRepo;
        private readonly Mock<IEmergencyAuditService> _mockAuditService;
        private readonly Mock<ILogger<EmergencyModeService>> _mockLogger;
        private readonly EmergencyModeService _sut;

        public EmergencyDashboardTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockAppointmentRepo = new Mock<IAppointmentRepository>();
            _mockEventRepo = new Mock<IEmergencyEventRepository>();
            _mockAuditService = new Mock<IEmergencyAuditService>();
            _mockLogger = new Mock<ILogger<EmergencyModeService>>();

            _mockUnitOfWork.Setup(u => u.Patients).Returns(_mockPatientRepo.Object);
            _mockUnitOfWork.Setup(u => u.Appointments).Returns(_mockAppointmentRepo.Object);
            _mockUnitOfWork.Setup(u => u.EmergencyEvents).Returns(_mockEventRepo.Object);

            _sut = new EmergencyModeService(
                _mockUnitOfWork.Object,
                _mockAuditService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetDoctorEmergencyPatientsAsync_ShouldReturnActiveEmergencyPatients()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var patients = new List<Core.Entities.Identity.Patient>
            {
                new Core.Entities.Identity.Patient
                {
                    Id = patientId,
                    FirstName = "أحمد",
                    LastName = "محمد",
                    EmergencyModeActive = true,
                    EmergencyModeActivatedById = doctorId,
                    BloodType = BloodType.OPlus,
                    WeightKg = 80,
                    HeightCm = 175,
                    EmergencyContactName = "علي",
                    EmergencyContactPhone = "01012345678",
                    EmergencyContactRelationship = "أخ"
                }
            };

            _mockPatientRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Core.Entities.Identity.Patient, bool>>>()))
                .ReturnsAsync(patients);

            // Act
            var result = await _sut.GetDoctorEmergencyPatientsAsync(doctorId);

            // Assert
            result.Should().HaveCount(1);
            var item = result.First();
            item.PatientId.Should().Be(patientId);
            item.PatientName.Should().Contain("أحمد");
        }

        [Fact]
        public async Task GetDoctorSosEventsAsync_ShouldReturnEventsForDoctorsPatients()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var patients = new List<Core.Entities.Identity.Patient>
            {
                new Core.Entities.Identity.Patient { Id = patientId, FirstName = "سارة", LastName = "أحمد", EmergencyModeActivatedById = doctorId }
            };

            var emergencyEvents = new List<EmergencyEvent>
            {
                new EmergencyEvent
                {
                    Id = eventId,
                    PatientId = patientId,
                    ActivatingDoctorId = doctorId,
                    Latitude = 30.0444,
                    Longitude = 31.2357,
                    Timestamp = DateTime.UtcNow,
                    Status = EmergencyEventStatus.Dispatched,
                    Patient = patients[0]
                }
            };

            _mockPatientRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Core.Entities.Identity.Patient, bool>>>()))
                .ReturnsAsync(patients);

            _mockEventRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<EmergencyEvent, bool>>>()))
                .ReturnsAsync(emergencyEvents);

            // Act
            var result = await _sut.GetDoctorSosEventsAsync(doctorId);

            // Assert
            result.Should().HaveCount(1);
            var item = result.First();
            item.Id.Should().Be(eventId);
            item.PatientId.Should().Be(patientId);
            item.Status.Should().Be(EmergencyEventStatus.Dispatched);
        }

        [Fact]
        public async Task ResolveEmergencyEventAsync_ShouldUpdateStatusToResolved()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var emergencyEvent = new EmergencyEvent
            {
                Id = eventId,
                PatientId = patientId,
                ActivatingDoctorId = doctorId,
                Status = EmergencyEventStatus.Dispatched
            };

            _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(emergencyEvent);

            // Act
            var result = await _sut.ResolveEmergencyEventAsync(doctorId, eventId);

            // Assert
            result.Should().BeTrue();
            emergencyEvent.Status.Should().Be(EmergencyEventStatus.Resolved);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockAuditService.Verify(s => s.LogActionAsync(doctorId, patientId, EmergencyActionType.SosResolved), Times.Once);
        }

        [Fact]
        public async Task ResolveEmergencyEventAsync_ShouldThrow_WhenDoctorIsNotOwner()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var otherDoctorId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var emergencyEvent = new EmergencyEvent
            {
                Id = eventId,
                PatientId = Guid.NewGuid(),
                ActivatingDoctorId = otherDoctorId,
                Status = EmergencyEventStatus.Dispatched
            };

            _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(emergencyEvent);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.ResolveEmergencyEventAsync(doctorId, eventId));
        }
    }
}
