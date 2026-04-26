using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Shuryan.Application.Services;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;
using Xunit;

namespace Shuryan.Tests.Emergency
{
    public class EmergencyAuditServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEmergencyAuditLogRepository> _mockAuditRepo;
        private readonly Mock<IDoctorRepository> _mockDoctorRepo;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly EmergencyAuditService _sut;

        public EmergencyAuditServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuditRepo = new Mock<IEmergencyAuditLogRepository>();
            _mockDoctorRepo = new Mock<IDoctorRepository>();
            _mockPatientRepo = new Mock<IPatientRepository>();

            _mockUnitOfWork.Setup(u => u.EmergencyAuditLogs).Returns(_mockAuditRepo.Object);
            _mockUnitOfWork.Setup(u => u.Doctors).Returns(_mockDoctorRepo.Object);
            _mockUnitOfWork.Setup(u => u.Patients).Returns(_mockPatientRepo.Object);

            _sut = new EmergencyAuditService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task LogActionAsync_ShouldAddLogAndSaveChanges()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var action = EmergencyActionType.EmergencyModeActivated;

            // Act
            await _sut.LogActionAsync(doctorId, patientId, action);

            // Assert
            _mockAuditRepo.Verify(r => r.AddAsync(It.Is<EmergencyAuditLog>(l =>
                l.DoctorId == doctorId &&
                l.PatientId == patientId &&
                l.Action == action &&
                l.Timestamp <= DateTime.UtcNow
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
