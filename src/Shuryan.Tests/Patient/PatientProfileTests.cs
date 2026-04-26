using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.Services;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;
using Xunit;

namespace Shuryan.Tests.Patient
{
    public class PatientProfileTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ILogger<PatientService>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly PatientService _sut;

        public PatientProfileTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockMapper = new Mock<IMapper>();

            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<PatientService>>();

            _mockUnitOfWork.Setup(u => u.Patients).Returns(_mockPatientRepo.Object);

            _sut = new PatientService(
                _mockPatientRepo.Object,
                null,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object,
                null
            );
        }

        [Fact]
        public async Task UpdatePatientAsync_ShouldUpdateEmergencyFields()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = new Core.Entities.Identity.Patient { Id = patientId };
            
            var request = new UpdatePatientRequest
            {
                BloodType = BloodType.OPlus,
                WeightKg = 75.5m,
                HeightCm = 180,
                EmergencyContactName = "John Doe",
                EmergencyContactPhone = "01012345678",
                EmergencyContactRelationship = "Brother"
            };

            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            // Act
            await _sut.UpdatePatientAsync(patientId, request);

            // Assert
            patient.BloodType.Should().Be(BloodType.OPlus);
            patient.WeightKg.Should().Be(75.5m);
            patient.HeightCm.Should().Be(180);
            patient.EmergencyContactName.Should().Be("John Doe");
            patient.EmergencyContactPhone.Should().Be("01012345678");
            patient.EmergencyContactRelationship.Should().Be("Brother");
            
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
