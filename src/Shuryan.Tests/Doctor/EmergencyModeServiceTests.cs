using System;
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

namespace Shuryan.Tests.Doctor
{
    public class EmergencyModeServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
        private readonly Mock<IEmergencyAuditService> _mockAuditService;
        private readonly Mock<ILogger<EmergencyModeService>> _mockLogger;
        private readonly EmergencyModeService _sut;

        public EmergencyModeServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockAppointmentRepo = new Mock<IAppointmentRepository>();
            _mockAuditService = new Mock<IEmergencyAuditService>();
            _mockLogger = new Mock<ILogger<EmergencyModeService>>();

            _mockUnitOfWork.Setup(u => u.Patients).Returns(_mockPatientRepo.Object);
            _mockUnitOfWork.Setup(u => u.Appointments).Returns(_mockAppointmentRepo.Object);

            _sut = new EmergencyModeService(
                _mockUnitOfWork.Object,
                _mockAuditService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task ActivateEmergencyMode_ShouldSucceed_WhenProfileComplete()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();

            var patient = new Core.Entities.Identity.Patient 
            { 
                Id = patientId,
                BloodType = BloodType.OPlus,
                WeightKg = 70,
                HeightCm = 175,
                EmergencyContactName = "John",
                EmergencyContactPhone = "01012345678",
                EmergencyContactRelationship = "Brother"
            };

            var appointment = new Appointment
            {
                Id = appointmentId,
                DoctorId = doctorId,
                PatientId = patientId,
                Status = Shuryan.Core.Enums.Appointments.AppointmentStatus.InProgress
            };

            _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync(patient);

            // Act
            var result = await _sut.ActivateEmergencyModeAsync(doctorId, appointmentId);

            // Assert
            result.Should().BeTrue();
            patient.EmergencyModeActive.Should().BeTrue();
            patient.EmergencyModeActivatedById.Should().Be(doctorId);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockAuditService.Verify(s => s.LogActionAsync(doctorId, patientId, EmergencyActionType.EmergencyModeActivated), Times.Once);
        }

        [Fact]
        public async Task ActivateEmergencyMode_ShouldThrowException_WhenProfileIncomplete()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();

            var patient = new Core.Entities.Identity.Patient 
            { 
                Id = patientId,
                BloodType = null // Missing blood type
            };

            var appointment = new Appointment
            {
                Id = appointmentId,
                DoctorId = doctorId,
                PatientId = patientId,
                Status = Shuryan.Core.Enums.Appointments.AppointmentStatus.InProgress
            };

            _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync(patient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.ActivateEmergencyModeAsync(doctorId, appointmentId));
            
            exception.Message.Should().Contain("Blood Type");
        }
    }
}
