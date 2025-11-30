using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Governorate = table.Column<int>(type: "int", nullable: false),
                    BuildingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<double>(type: "float(18)", precision: 18, scale: 12, nullable: true),
                    Longitude = table.Column<double>(type: "float(18)", precision: 18, scale: 12, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserRole = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OAuthProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OAuthProviderId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsOAuthAccount = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultationTypeEnum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserRole = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    GenericName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Strength = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DosageForm = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RequestedFromIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VerificationType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                    table.CheckConstraint("CK_EmailVerification_AttemptCount", "[AttemptCount] >= 0 AND [AttemptCount] <= 10");
                    table.ForeignKey(
                        name: "FK_EmailVerifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "EGP"),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProviderResponse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefundedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfileUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileUsers_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.CheckConstraint("CK_RefreshToken_Expiration", "[ExpiresAt] > [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Verifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verifiers_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SuggestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContextJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenCount = table.Column<int>(type: "int", nullable: true),
                    ResponseTimeMs = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationMessages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProviderResponse = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Patients_ProfileUsers_Id",
                        column: x => x.Id,
                        principalTable: "ProfileUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalSpecialty = table.Column<int>(type: "int", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    Biography = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.CheckConstraint("CK_Doctor_YearsOfExperience", "[YearsOfExperience] >= 0 AND [YearsOfExperience] <= 60");
                    table.ForeignKey(
                        name: "FK_Doctors_ProfileUsers_Id",
                        column: x => x.Id,
                        principalTable: "ProfileUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Doctors_Verifiers_VerifierId",
                        column: x => x.VerifierId,
                        principalTable: "Verifiers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Laboratories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LaboratoryStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    OffersHomeSampleCollection = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HomeSampleCollectionFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laboratories", x => x.Id);
                    table.CheckConstraint("CK_Laboratory_HomeSampleFee", "[HomeSampleCollectionFee] IS NULL OR [HomeSampleCollectionFee] >= 0");
                    table.ForeignKey(
                        name: "FK_Laboratories_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Laboratories_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Laboratories_Verifiers_VerifierId",
                        column: x => x.VerifierId,
                        principalTable: "Verifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pharmacies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PharmacyStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    OffersDelivery = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pharmacies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pharmacies_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pharmacies_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pharmacies_Verifiers_VerifierId",
                        column: x => x.VerifierId,
                        principalTable: "Verifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistoryItems_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousAppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScheduledStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsultationType = table.Column<int>(type: "int", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SessionDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.CheckConstraint("CK_Appointment_ConsultationFee", "[ConsultationFee] >= 0");
                    table.CheckConstraint("CK_Appointment_SessionDuration", "[SessionDurationMinutes] > 0 AND [SessionDurationMinutes] <= 480");
                    table.CheckConstraint("CK_Appointment_TimeValidation", "[ScheduledStartTime] < [ScheduledEndTime]");
                    table.ForeignKey(
                        name: "FK_Appointments_Appointments_PreviousAppointmentId",
                        column: x => x.PreviousAppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClinicStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    FacilityVideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clinics_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAvailabilities", x => x.Id);
                    table.CheckConstraint("CK_DoctorAvailability_TimeValidation", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorConsultations",
                columns: table => new
                {
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SessionDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorConsultations", x => new { x.DoctorId, x.ConsultationTypeId });
                    table.CheckConstraint("CK_DoctorConsultation_Duration", "[SessionDurationMinutes] >= 15 AND [SessionDurationMinutes] <= 120");
                    table.CheckConstraint("CK_DoctorConsultation_Fee", "[ConsultationFee] >= 0");
                    table.ForeignKey(
                        name: "FK_DoctorConsultations_ConsultationTypes_ConsultationTypeId",
                        column: x => x.ConsultationTypeId,
                        principalTable: "ConsultationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorConsultations_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorDocuments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorOverrides", x => x.Id);
                    table.CheckConstraint("CK_DoctorOverride_TimeValidation", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_DoctorOverrides_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorPartnerSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuggestedPharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PharmacySuggestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuggestedLaboratoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LaboratorySuggestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorPartnerSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorPartnerSuggestions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaboratoryDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LaboratoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboratoryDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaboratoryDocuments_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaboratoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LabSpecificNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabServices", x => x.Id);
                    table.CheckConstraint("CK_LabService_Price", "[Price] >= 0");
                    table.ForeignKey(
                        name: "FK_LabServices_LabTests_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabServices_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabWorkingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaboratoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabWorkingHours", x => x.Id);
                    table.CheckConstraint("CK_LaboratoryWorkingHours_Time", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_LabWorkingHours_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyDocuments_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyWorkingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    PharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyWorkingHours", x => x.Id);
                    table.CheckConstraint("CK_PharmacyWorkingHours_Time", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_PharmacyWorkingHours_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HistoryOfPresentIllness = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PhysicalExamination = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ManagementPlan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationRecords_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallSatisfaction = table.Column<int>(type: "int", nullable: false),
                    WaitingTime = table.Column<int>(type: "int", nullable: false),
                    CommunicationQuality = table.Column<int>(type: "int", nullable: false),
                    ClinicCleanliness = table.Column<int>(type: "int", nullable: false),
                    ValueForMoney = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DoctorReply = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DoctorRepliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorReviews", x => x.Id);
                    table.CheckConstraint("CK_DoctorReview_ClinicCleanliness", "[ClinicCleanliness] >= 1 AND [ClinicCleanliness] <= 5");
                    table.CheckConstraint("CK_DoctorReview_CommunicationQuality", "[CommunicationQuality] >= 1 AND [CommunicationQuality] <= 5");
                    table.CheckConstraint("CK_DoctorReview_OverallSatisfaction", "[OverallSatisfaction] >= 1 AND [OverallSatisfaction] <= 5");
                    table.CheckConstraint("CK_DoctorReview_ValueForMoney", "[ValueForMoney] >= 1 AND [ValueForMoney] <= 5");
                    table.CheckConstraint("CK_DoctorReview_WaitingTime", "[WaitingTime] >= 1 AND [WaitingTime] <= 5");
                    table.ForeignKey(
                        name: "FK_DoctorReviews_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorReviews_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorReviews_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabPrescriptions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabPrescriptions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabPrescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DigitalSignature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GeneralInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicPhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicPhoneNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicPhoneNumbers_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicPhotos", x => x.Id);
                    table.CheckConstraint("CK_ClinicPhoto_DisplayOrder", "[DisplayOrder] >= 0 AND [DisplayOrder] <= 5");
                    table.ForeignKey(
                        name: "FK_ClinicPhotos_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicServices_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaboratoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SampleCollectionType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TestsTotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SampleCollectionDeliveryCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    ConfirmedByLabAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SamplesCollectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabOrders", x => x.Id);
                    table.CheckConstraint("CK_LabOrder_Costs", "[TestsTotalCost] >= 0 AND [SampleCollectionDeliveryCost] >= 0");
                    table.ForeignKey(
                        name: "FK_LabOrders_LabPrescriptions_LabPrescriptionId",
                        column: x => x.LabPrescriptionId,
                        principalTable: "LabPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabOrders_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabOrders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabPrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabPrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabPrescriptionItems_LabPrescriptions_LabPrescriptionId",
                        column: x => x.LabPrescriptionId,
                        principalTable: "LabPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabPrescriptionItems_LabTests_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispensingRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispensingRecords", x => x.Id);
                    table.CheckConstraint("CK_DispensingRecord_TotalCost", "[TotalCost] >= 0");
                    table.ForeignKey(
                        name: "FK_DispensingRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispensingRecords_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispensingRecords_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DeliveryType = table.Column<int>(type: "int", nullable: false),
                    EstimatedDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryPersonPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryPersonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActualDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PatientConfirmed = table.Column<bool>(type: "bit", nullable: true),
                    PatientConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PatientNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PatientDigitalSignature = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyOrders", x => x.Id);
                    table.CheckConstraint("CK_PharmacyOrder_Costs", "[TotalCost] >= 0 AND [DeliveryFee] >= 0");
                    table.ForeignKey(
                        name: "FK_PharmacyOrders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyOrders_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyOrders_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PrescribedMedications",
                columns: table => new
                {
                    MedicationPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescribedMedications", x => new { x.MedicationPrescriptionId, x.MedicationId });
                    table.CheckConstraint("CK_PrescribedMedication_Duration", "[DurationDays] > 0");
                    table.ForeignKey(
                        name: "FK_PrescribedMedications_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrescribedMedications_Prescriptions_MedicationPrescriptionId",
                        column: x => x.MedicationPrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaboratoryReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaboratoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallSatisfaction = table.Column<int>(type: "int", nullable: false),
                    ResultAccuracy = table.Column<int>(type: "int", nullable: false),
                    DeliverySpeed = table.Column<int>(type: "int", nullable: false),
                    ServiceQuality = table.Column<int>(type: "int", nullable: false),
                    ValueForMoney = table.Column<int>(type: "int", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboratoryReviews", x => x.Id);
                    table.CheckConstraint("CK_LaboratoryReview_DeliverySpeed", "[DeliverySpeed] >= 1 AND [DeliverySpeed] <= 5");
                    table.CheckConstraint("CK_LaboratoryReview_OverallSatisfaction", "[OverallSatisfaction] >= 1 AND [OverallSatisfaction] <= 5");
                    table.CheckConstraint("CK_LaboratoryReview_ResultAccuracy", "[ResultAccuracy] >= 1 AND [ResultAccuracy] <= 5");
                    table.CheckConstraint("CK_LaboratoryReview_ServiceQuality", "[ServiceQuality] >= 1 AND [ServiceQuality] <= 5");
                    table.CheckConstraint("CK_LaboratoryReview_ValueForMoney", "[ValueForMoney] >= 1 AND [ValueForMoney] <= 5");
                    table.ForeignKey(
                        name: "FK_LaboratoryReviews_LabOrders_LabOrderId",
                        column: x => x.LabOrderId,
                        principalTable: "LabOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LaboratoryReviews_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LaboratoryReviews_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceRange = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabResults_LabOrders_LabOrderId",
                        column: x => x.LabOrderId,
                        principalTable: "LabOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabResults_LabTests_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispensedMedicationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispensingRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityDispensed = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispensedMedicationItems", x => x.Id);
                    table.CheckConstraint("CK_DispensedMedicationItem_Prices", "[UnitPrice] >= 0 AND [TotalPrice] >= 0");
                    table.CheckConstraint("CK_DispensedMedicationItem_Quantity", "[QuantityDispensed] > 0");
                    table.ForeignKey(
                        name: "FK_DispensedMedicationItems_DispensingRecords_DispensingRecordId",
                        column: x => x.DispensingRecordId,
                        principalTable: "DispensingRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DispensedMedicationItems_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacyOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedMedicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    AlternativeMedicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AlternativeUnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    AlternativeNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyOrderItems", x => x.Id);
                    table.CheckConstraint("CK_PharmacyOrderItem_AlternativePrices", "[AlternativeUnitPrice] IS NULL OR [AlternativeUnitPrice] >= 0");
                    table.CheckConstraint("CK_PharmacyOrderItem_Prices", "[UnitPrice] IS NULL OR [UnitPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_PharmacyOrderItems_Medications_AlternativeMedicationId",
                        column: x => x.AlternativeMedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyOrderItems_Medications_RequestedMedicationId",
                        column: x => x.RequestedMedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyOrderItems_PharmacyOrders_PharmacyOrderId",
                        column: x => x.PharmacyOrderId,
                        principalTable: "PharmacyOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacyOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallSatisfaction = table.Column<int>(type: "int", nullable: false),
                    MedicationAvailability = table.Column<int>(type: "int", nullable: false),
                    ServiceQuality = table.Column<int>(type: "int", nullable: false),
                    DeliverySpeed = table.Column<int>(type: "int", nullable: false),
                    ValueForMoney = table.Column<int>(type: "int", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyReviews", x => x.Id);
                    table.CheckConstraint("CK_PharmacyReview_DeliverySpeed", "[DeliverySpeed] >= 1 AND [DeliverySpeed] <= 5");
                    table.CheckConstraint("CK_PharmacyReview_MedicationAvailability", "[MedicationAvailability] >= 1 AND [MedicationAvailability] <= 5");
                    table.CheckConstraint("CK_PharmacyReview_OverallSatisfaction", "[OverallSatisfaction] >= 1 AND [OverallSatisfaction] <= 5");
                    table.CheckConstraint("CK_PharmacyReview_ServiceQuality", "[ServiceQuality] >= 1 AND [ServiceQuality] <= 5");
                    table.CheckConstraint("CK_PharmacyReview_ValueForMoney", "[ValueForMoney] >= 1 AND [ValueForMoney] <= 5");
                    table.ForeignKey(
                        name: "FK_PharmacyReviews_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyReviews_Pharmacies_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyReviews_PharmacyOrders_PharmacyOrderId",
                        column: x => x.PharmacyOrderId,
                        principalTable: "PharmacyOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_Coordinates",
                table: "Addresses",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Address_Governorate",
                table: "Addresses",
                column: "Governorate");

            migrationBuilder.CreateIndex(
                name: "IX_Address_IsDeleted",
                table: "Addresses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Doctor_StartTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "ScheduledStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Doctor_Status_StartTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "Status", "ScheduledStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Patient_Status",
                table: "Appointments",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ScheduledStartTime",
                table: "Appointments",
                column: "ScheduledStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Status",
                table: "Appointments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PreviousAppointmentId",
                table: "Appointments",
                column: "PreviousAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_FullName",
                table: "AspNetUsers",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_User_IsDeleted",
                table: "AspNetUsers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPhoneNumber_Clinic_Number",
                table: "ClinicPhoneNumbers",
                columns: new[] { "ClinicId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPhoneNumber_Clinic_Type",
                table: "ClinicPhoneNumbers",
                columns: new[] { "ClinicId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPhoneNumber_ClinicId",
                table: "ClinicPhoneNumbers",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPhoto_Clinic_Order",
                table: "ClinicPhotos",
                columns: new[] { "ClinicId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPhoto_ClinicId",
                table: "ClinicPhotos",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinic_AddressId",
                table: "Clinics",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clinic_DoctorId",
                table: "Clinics",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clinic_Status",
                table: "Clinics",
                column: "ClinicStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicService_Clinic_Service",
                table: "ClinicServices",
                columns: new[] { "ClinicId", "ServiceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicService_ClinicId",
                table: "ClinicServices",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicService_ServiceType",
                table: "ClinicServices",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecord_AppointmentId",
                table: "ConsultationRecords",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessage_Conversation_CreatedAt",
                table: "ConversationMessages",
                columns: new[] { "ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessage_ConversationId",
                table: "ConversationMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessage_Role",
                table: "ConversationMessages",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_IsActive",
                table: "Conversations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_LastMessageAt",
                table: "Conversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_User_IsActive",
                table: "Conversations",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_UserId",
                table: "Conversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DispensedMedicationItem_DispensingRecordId",
                table: "DispensedMedicationItems",
                column: "DispensingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DispensedMedicationItem_MedicationId",
                table: "DispensedMedicationItems",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_DispensedMedicationItem_Record_Medication",
                table: "DispensedMedicationItems",
                columns: new[] { "DispensingRecordId", "MedicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_DispensingRecord_Patient_Date",
                table: "DispensingRecords",
                columns: new[] { "PatientId", "DispensedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DispensingRecord_PatientId",
                table: "DispensingRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DispensingRecord_Pharmacy_Date",
                table: "DispensingRecords",
                columns: new[] { "PharmacyId", "DispensedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DispensingRecord_PharmacyId",
                table: "DispensingRecords",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_DispensingRecord_PrescriptionId",
                table: "DispensingRecords",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DispensingRecord_ReceiptNumber",
                table: "DispensingRecords",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_Doctor_Day",
                table: "DoctorAvailabilities",
                columns: new[] { "DoctorId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_IsDeleted",
                table: "DoctorAvailabilities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_Unique",
                table: "DoctorAvailabilities",
                columns: new[] { "DoctorId", "DayOfWeek", "StartTime", "EndTime" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorConsultations_ConsultationTypeId",
                table: "DoctorConsultations",
                column: "ConsultationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDocument_Doctor_Type",
                table: "DoctorDocuments",
                columns: new[] { "DoctorId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDocument_DoctorId",
                table: "DoctorDocuments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDocument_Status",
                table: "DoctorDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorOverride_Doctor_Type",
                table: "DoctorOverrides",
                columns: new[] { "DoctorId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorOverride_DoctorId",
                table: "DoctorOverrides",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorOverride_StartTime",
                table: "DoctorOverrides",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorPartnerSuggestion_DoctorId",
                table: "DoctorPartnerSuggestions",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorPartnerSuggestion_LaboratoryId",
                table: "DoctorPartnerSuggestions",
                column: "SuggestedLaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorPartnerSuggestion_PharmacyId",
                table: "DoctorPartnerSuggestions",
                column: "SuggestedPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorReview_AppointmentId",
                table: "DoctorReviews",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorReview_DoctorId",
                table: "DoctorReviews",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorReview_IsAnonymous",
                table: "DoctorReviews",
                column: "IsAnonymous");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorReview_PatientId",
                table: "DoctorReviews",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_MedicalSpecialty",
                table: "Doctors",
                column: "MedicalSpecialty");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Verification_Specialty",
                table: "Doctors",
                columns: new[] { "VerificationStatus", "MedicalSpecialty" });

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_VerificationStatus",
                table: "Doctors",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_VerifierId",
                table: "Doctors",
                column: "VerifierId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerification_Email",
                table: "EmailVerifications",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerification_Email_OtpCode",
                table: "EmailVerifications",
                columns: new[] { "Email", "OtpCode" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerification_ExpiresAt",
                table: "EmailVerifications",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerification_IsUsed_ExpiresAt",
                table: "EmailVerifications",
                columns: new[] { "IsUsed", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerification_UserId",
                table: "EmailVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Laboratories_VerifierId",
                table: "Laboratories",
                column: "VerifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Laboratory_AddressId",
                table: "Laboratories",
                column: "AddressId",
                unique: true,
                filter: "[AddressId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Laboratory_Name",
                table: "Laboratories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Laboratory_Status_HomeCollection",
                table: "Laboratories",
                columns: new[] { "LaboratoryStatus", "OffersHomeSampleCollection" });

            migrationBuilder.CreateIndex(
                name: "IX_Laboratory_VerificationStatus",
                table: "Laboratories",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryDocument_Laboratory_Type",
                table: "LaboratoryDocuments",
                columns: new[] { "LaboratoryId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryDocument_LaboratoryId",
                table: "LaboratoryDocuments",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryDocument_Status",
                table: "LaboratoryDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryReview_LaboratoryId",
                table: "LaboratoryReviews",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryReview_LabOrderId",
                table: "LaboratoryReviews",
                column: "LabOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryReview_PatientId",
                table: "LaboratoryReviews",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_Laboratory_Status",
                table: "LabOrders",
                columns: new[] { "LaboratoryId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_LaboratoryId",
                table: "LabOrders",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_LabPrescriptionId",
                table: "LabOrders",
                column: "LabPrescriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_Patient_Status",
                table: "LabOrders",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_PatientId",
                table: "LabOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_Status",
                table: "LabOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LabPrescriptionItem_LabPrescriptionId",
                table: "LabPrescriptionItems",
                column: "LabPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LabPrescriptionItem_LabTestId",
                table: "LabPrescriptionItems",
                column: "LabTestId");

            migrationBuilder.CreateIndex(
                name: "IX_LabPrescriptionItem_Prescription_Test",
                table: "LabPrescriptionItems",
                columns: new[] { "LabPrescriptionId", "LabTestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabPrescription_AppointmentId",
                table: "LabPrescriptions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabPrescription_DoctorId",
                table: "LabPrescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_LabPrescription_PatientId",
                table: "LabPrescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResult_LabOrderId",
                table: "LabResults",
                column: "LabOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResult_LabTestId",
                table: "LabResults",
                column: "LabTestId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResult_Order_Test",
                table: "LabResults",
                columns: new[] { "LabOrderId", "LabTestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabService_Laboratory_Available",
                table: "LabServices",
                columns: new[] { "LaboratoryId", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_LabService_Laboratory_Test",
                table: "LabServices",
                columns: new[] { "LaboratoryId", "LabTestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabService_LaboratoryId",
                table: "LabServices",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LabService_LabTestId",
                table: "LabServices",
                column: "LabTestId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTest_Category",
                table: "LabTests",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LabTest_Code",
                table: "LabTests",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabTest_Name",
                table: "LabTests",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LabWorkingHours_Laboratory_Day",
                table: "LabWorkingHours",
                columns: new[] { "LaboratoryId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_LabWorkingHours_LaboratoryId",
                table: "LabWorkingHours",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LabWorkingHours_Unique",
                table: "LabWorkingHours",
                columns: new[] { "LaboratoryId", "Day", "StartTime", "EndTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistoryItem_Patient_Type",
                table: "MedicalHistoryItems",
                columns: new[] { "PatientId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistoryItem_PatientId",
                table: "MedicalHistoryItems",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistoryItem_Type",
                table: "MedicalHistoryItems",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Medication_BrandName",
                table: "Medications",
                column: "BrandName");

            migrationBuilder.CreateIndex(
                name: "IX_Medication_DosageForm",
                table: "Medications",
                column: "DosageForm");

            migrationBuilder.CreateIndex(
                name: "IX_Medication_GenericName",
                table: "Medications",
                column: "GenericName");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RelatedEntity",
                table: "Notifications",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_User_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_AddressId",
                table: "Patients",
                column: "AddressId",
                unique: true,
                filter: "[AddressId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderType_OrderId",
                table: "Payments",
                columns: new[] { "OrderType", "OrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderTransactionId",
                table: "Payments",
                column: "ProviderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CreatedAt",
                table: "PaymentTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ProviderTransactionId",
                table: "PaymentTransactions",
                column: "ProviderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacies_VerifierId",
                table: "Pharmacies",
                column: "VerifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacy_AddressId",
                table: "Pharmacies",
                column: "AddressId",
                unique: true,
                filter: "[AddressId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacy_Name",
                table: "Pharmacies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacy_Status_Delivery",
                table: "Pharmacies",
                columns: new[] { "PharmacyStatus", "OffersDelivery" });

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacy_VerificationStatus",
                table: "Pharmacies",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDocument_Pharmacy_Type",
                table: "PharmacyDocuments",
                columns: new[] { "PharmacyId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDocument_PharmacyId",
                table: "PharmacyDocuments",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDocument_Status",
                table: "PharmacyDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrderItem_Order_Medication",
                table: "PharmacyOrderItems",
                columns: new[] { "PharmacyOrderId", "RequestedMedicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrderItem_OrderId",
                table: "PharmacyOrderItems",
                column: "PharmacyOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrderItem_RequestedMedicationId",
                table: "PharmacyOrderItems",
                column: "RequestedMedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrderItem_Status",
                table: "PharmacyOrderItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrderItems_AlternativeMedicationId",
                table: "PharmacyOrderItems",
                column: "AlternativeMedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrder_OrderNumber",
                table: "PharmacyOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrder_Patient_Status",
                table: "PharmacyOrders",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrder_PatientId",
                table: "PharmacyOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrder_Pharmacy_Status",
                table: "PharmacyOrders",
                columns: new[] { "PharmacyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrder_PharmacyId",
                table: "PharmacyOrders",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrder_Status",
                table: "PharmacyOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrders_PrescriptionId",
                table: "PharmacyOrders",
                column: "PrescriptionId",
                unique: true,
                filter: "[PrescriptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyReview_PatientId",
                table: "PharmacyReviews",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyReview_PharmacyId",
                table: "PharmacyReviews",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyReview_PharmacyOrderId",
                table: "PharmacyReviews",
                column: "PharmacyOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyWorkingHours_Pharmacy_Day",
                table: "PharmacyWorkingHours",
                columns: new[] { "PharmacyId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyWorkingHours_PharmacyId",
                table: "PharmacyWorkingHours",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyWorkingHours_Unique",
                table: "PharmacyWorkingHours",
                columns: new[] { "PharmacyId", "DayOfWeek", "StartTime", "EndTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrescribedMedication_MedicationId",
                table: "PrescribedMedications",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescribedMedication_PrescriptionId",
                table: "PrescribedMedications",
                column: "MedicationPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_Number",
                table: "Prescriptions",
                column: "PrescriptionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_Patient_Status",
                table: "Prescriptions",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_Status",
                table: "Prescriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileUser_Gender",
                table: "ProfileUsers",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Verifier_CreatedByAdminId",
                table: "Verifiers",
                column: "CreatedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ClinicPhoneNumbers");

            migrationBuilder.DropTable(
                name: "ClinicPhotos");

            migrationBuilder.DropTable(
                name: "ClinicServices");

            migrationBuilder.DropTable(
                name: "ConsultationRecords");

            migrationBuilder.DropTable(
                name: "ConversationMessages");

            migrationBuilder.DropTable(
                name: "DispensedMedicationItems");

            migrationBuilder.DropTable(
                name: "DoctorAvailabilities");

            migrationBuilder.DropTable(
                name: "DoctorConsultations");

            migrationBuilder.DropTable(
                name: "DoctorDocuments");

            migrationBuilder.DropTable(
                name: "DoctorOverrides");

            migrationBuilder.DropTable(
                name: "DoctorPartnerSuggestions");

            migrationBuilder.DropTable(
                name: "DoctorReviews");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "LaboratoryDocuments");

            migrationBuilder.DropTable(
                name: "LaboratoryReviews");

            migrationBuilder.DropTable(
                name: "LabPrescriptionItems");

            migrationBuilder.DropTable(
                name: "LabResults");

            migrationBuilder.DropTable(
                name: "LabServices");

            migrationBuilder.DropTable(
                name: "LabWorkingHours");

            migrationBuilder.DropTable(
                name: "MedicalHistoryItems");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PharmacyDocuments");

            migrationBuilder.DropTable(
                name: "PharmacyOrderItems");

            migrationBuilder.DropTable(
                name: "PharmacyReviews");

            migrationBuilder.DropTable(
                name: "PharmacyWorkingHours");

            migrationBuilder.DropTable(
                name: "PrescribedMedications");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "DispensingRecords");

            migrationBuilder.DropTable(
                name: "ConsultationTypes");

            migrationBuilder.DropTable(
                name: "LabOrders");

            migrationBuilder.DropTable(
                name: "LabTests");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PharmacyOrders");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "LabPrescriptions");

            migrationBuilder.DropTable(
                name: "Laboratories");

            migrationBuilder.DropTable(
                name: "Pharmacies");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Verifiers");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "ProfileUsers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
