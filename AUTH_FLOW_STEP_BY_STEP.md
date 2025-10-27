# 🔄 تتبع الـ Flow الكامل - RegisterPatient

## 📋 الهدف
نتتبع رحلة الـ Request من أول ما يوصل للـ API لحد ما يرجع Response

---

## 🚀 الرحلة الكاملة

### 1️⃣ **Client يبعت Request**
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Ahmed",
  "lastName": "Mohamed",
  "email": "ahmed@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1990-01-15"
}
```

---

### 2️⃣ **ASP.NET Core Middleware Pipeline**

#### 📍 الموقع: `src/Shuryan.API/Program.cs`

**الـ Request بيعدي على:**
1. **Routing Middleware** - بيحدد الـ Route: `/api/auth/register/patient`
2. **Authentication Middleware** - بيتحقق من الـ Token (مش محتاج هنا)
3. **Authorization Middleware** - بيتحقق من الـ Permissions (مش محتاج هنا)
4. **Model Binding** - بيحول الـ JSON لـ `RegisterPatientRequest` object

---

### 3️⃣ **Controller - Entry Point**

#### 📍 الموقع: `src/Shuryan.API/Controllers/AuthController.cs`
#### 📍 Method: `RegisterPatient` (Lines 31-61)

```csharp
public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest dto)
{
    // 🔍 Checkpoint 1: حط Breakpoint هنا
    // شوف قيمة dto وتأكد إن كل الحقول موجودة
    
    // ✅ Step 1: Validate ModelState
    if (!ModelState.IsValid)
    {
        // 🔍 Checkpoint 2: لو وصلت هنا، يبقى في Validation Error
        // شوف ModelState.Errors
        _logger.LogWarning("Invalid registration request for email: {Email}", dto.Email);
        return BadRequest(ModelState);
    }
    
    // ✅ Step 2: Log the attempt
    _logger.LogInformation("Patient registration attempt for email: {Email}", dto.Email);
    
    try
    {
        // ✅ Step 3: Get IP Address
        var ipAddress = GetIpAddress();
        // 🔍 Checkpoint 3: شوف قيمة ipAddress
        
        // ✅ Step 4: Call Service
        var result = await _authService.RegisterPatientAsync(dto, ipAddress);
        // 🔍 Checkpoint 4: لما ترجع من Service، شوف result.IsSuccess
        
        if (!result.IsSuccess)
        {
            // 🔍 Checkpoint 5: لو فشل، شوف result.Message و result.Errors
            _logger.LogWarning("Registration failed for {Email}: {Message}", dto.Email, result.Message);
            return StatusCode(result.StatusCode ?? 400, result);
        }
        
        // ✅ Step 5: Success - Return 201
        _logger.LogInformation("Patient registered successfully: {Email}", dto.Email);
        return StatusCode(201, result);
    }
    catch (Exception ex)
    {
        // 🔍 Checkpoint 6: لو حصل Exception، شوف ex.Message و ex.StackTrace
        _logger.LogError(ex, "Error during patient registration for {Email}", dto.Email);
        return StatusCode(500, new { Message = "An unexpected error occurred during registration" });
    }
}
```

**✅ الـ Controller خلص دوره:**
- Validated ModelState ✅
- Logged the attempt ✅
- Called Service ✅
- Returned appropriate Response ✅

---

### 4️⃣ **Service - Business Logic**

#### 📍 الموقع: `src/Shuryan.Application/Services/Auth/AuthService.cs`
#### 📍 Method: `RegisterPatientAsync` (Lines 63-136)

```csharp
public async Task<ApiResponse<AuthResponseDto>> RegisterPatientAsync(
    RegisterPatientRequest dto,
    string? ipAddress = null)
{
    try
    {
        // 🔍 Checkpoint 7: حط Breakpoint هنا
        // شوف dto و ipAddress
        
        // ✅ Step 1: Check if email exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        // 🔍 Checkpoint 8: شوف existingUser
        
        if (existingUser != null)
        {
            // 🔍 Checkpoint 9: لو Email موجود، هنرجع Failure
            return ApiResponse<AuthResponseDto>.Failure(
                "Email already registered",
                new[] { "A user with this email already exists" },
                400);
        }
        
        // ✅ Step 2: Create Patient Entity
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };
        // 🔍 Checkpoint 10: شوف patient object
        
        // ✅ Step 3: Create user with password (ASP.NET Identity)
        var result = await _userManager.CreateAsync(patient, dto.Password);
        // 🔍 Checkpoint 11: شوف result.Succeeded
        
        if (!result.Succeeded)
        {
            // 🔍 Checkpoint 12: لو فشل، شوف result.Errors
            return ApiResponse<AuthResponseDto>.Failure(
                "Registration failed",
                result.Errors.Select(e => e.Description),
                400);
        }
        
        // ✅ Step 4: Assign Patient role
        await EnsureRoleExistsAsync(UserRole.Patient);
        await _userManager.AddToRoleAsync(patient, UserRole.Patient.ToString());
        // 🔍 Checkpoint 13: تأكد إن الـ Role اتضاف
        
        // ✅ Step 5: Generate and send OTP
        var otpCode = await _otpService.GenerateAndStoreOtpAsync(
            patient.Id,
            patient.Email,
            VerificationTypes.EmailVerification,
            ipAddress);
        // 🔍 Checkpoint 14: شوف otpCode (هيكون 6 أرقام)
        
        await _emailService.SendVerificationOtpAsync(
            patient.Email,
            patient.FirstName,
            otpCode);
        // 🔍 Checkpoint 15: تأكد إن الإيميل اتبعت
        
        _logger.LogInformation("Patient registered successfully: {Email}", patient.Email);
        
        // ✅ Step 6: Generate tokens
        var authResponse = await GenerateAuthResponseAsync(patient, ipAddress);
        // 🔍 Checkpoint 16: شوف authResponse (AccessToken, RefreshToken, User)
        
        return ApiResponse<AuthResponseDto>.Success(
            authResponse,
            "Registration successful! Please check your email for the verification code.",
            201);
    }
    catch (Exception ex)
    {
        // 🔍 Checkpoint 17: لو حصل Exception
        _logger.LogError(ex, "Error during patient registration");
        return ApiResponse<AuthResponseDto>.Failure(
            "An error occurred during registration",
            new[] { ex.Message },
            500);
    }
}
```

**✅ الـ Service خلص دوره:**
- Checked email uniqueness ✅
- Created Patient entity ✅
- Hashed password (ASP.NET Identity) ✅
- Assigned role ✅
- Generated OTP ✅
- Sent email ✅
- Generated JWT tokens ✅

---

### 5️⃣ **Database Operations**

#### 📍 الموقع: Database (SQL Server / PostgreSQL)

**الـ Tables المتأثرة:**

1. **AspNetUsers** (من ASP.NET Identity)
   ```sql
   INSERT INTO AspNetUsers (
       Id, 
       UserName, 
       Email, 
       EmailConfirmed, 
       PasswordHash,
       FirstName,
       LastName,
       CreatedAt,
       Discriminator  -- "Patient"
   ) VALUES (...)
   ```

2. **AspNetUserRoles**
   ```sql
   INSERT INTO AspNetUserRoles (UserId, RoleId)
   VALUES ('patient-guid', 'patient-role-guid')
   ```

3. **Otps** (Custom table)
   ```sql
   INSERT INTO Otps (
       Id,
       UserId,
       Email,
       OtpCode,
       VerificationType,
       ExpiresAt,
       IsUsed,
       IpAddress,
       CreatedAt
   ) VALUES (...)
   ```

4. **RefreshTokens** (Custom table)
   ```sql
   INSERT INTO RefreshTokens (
       Id,
       UserId,
       Token,
       ExpiresAt,
       CreatedAt,
       CreatedByIp,
       IsRevoked
   ) VALUES (...)
   ```

---

### 6️⃣ **External Services**

#### Email Service

#### 📍 الموقع: `src/Shuryan.Infrastructure/ExternalServices/EmailService.cs`

```csharp
public async Task SendVerificationOtpAsync(string email, string firstName, string otpCode)
{
    // ✅ Step 1: Prepare email template
    var subject = "Verify Your Email - ShurYan";
    var body = $@"
        <h2>Welcome {firstName}!</h2>
        <p>Your verification code is: <strong>{otpCode}</strong></p>
        <p>This code will expire in 10 minutes.</p>
    ";
    
    // ✅ Step 2: Send email via SMTP / SendGrid / etc.
    await _smtpClient.SendAsync(email, subject, body);
    
    // 🔍 Checkpoint 18: تأكد إن الإيميل اتبعت بنجاح
}
```

---

### 7️⃣ **Response يرجع للـ Client**

```json
{
  "isSuccess": true,
  "message": "Registration successful! Please check your email for the verification code.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4e5f6...",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "ahmed@test.com",
      "firstName": "Ahmed",
      "lastName": "Mohamed",
      "userType": "Patient",
      "isEmailVerified": false
    }
  },
  "statusCode": 201
}
```

---

## 🔍 Debugging Checklist

### في Controller:
- [ ] ModelState.IsValid = true
- [ ] dto.Email موجود
- [ ] dto.Password موجود
- [ ] ipAddress موجود
- [ ] result.IsSuccess = true

### في Service:
- [ ] existingUser = null (Email مش موجود)
- [ ] patient object created successfully
- [ ] result.Succeeded = true (User created)
- [ ] Role assigned successfully
- [ ] otpCode generated (6 digits)
- [ ] Email sent successfully
- [ ] authResponse.AccessToken موجود
- [ ] authResponse.RefreshToken موجود

### في Database:
- [ ] User موجود في AspNetUsers
- [ ] Role موجود في AspNetUserRoles
- [ ] OTP موجود في Otps table
- [ ] RefreshToken موجود في RefreshTokens table

### في Email:
- [ ] Email وصل للـ Inbox
- [ ] OTP موجود في الإيميل
- [ ] OTP صحيح (6 أرقام)

---

## 🎯 الملفات المطلوب مراجعتها بالترتيب

### 1. Controller Layer
- ✅ `src/Shuryan.API/Controllers/AuthController.cs`

### 2. DTO Layer
- ✅ `src/Shuryan.Application/DTOs/Requests/Auth/RegisterPatientRequest.cs`
- ⏳ `src/Shuryan.Application/DTOs/Responses/Auth/AuthResponseDto.cs`
- ⏳ `src/Shuryan.Application/DTOs/Common/Base/ApiResponse.cs`

### 3. Service Layer
- ⏳ `src/Shuryan.Application/Services/Auth/IAuthService.cs`
- ⏳ `src/Shuryan.Application/Services/Auth/AuthService.cs`

### 4. Domain Layer
- ⏳ `src/Shuryan.Core/Entities/Identity/Patient.cs`
- ⏳ `src/Shuryan.Core/Entities/Identity/User.cs`
- ⏳ `src/Shuryan.Core/Entities/System/Otp.cs`
- ⏳ `src/Shuryan.Core/Entities/System/RefreshToken.cs`

### 5. Infrastructure Layer
- ⏳ `src/Shuryan.Infrastructure/Services/Token/TokenService.cs`
- ⏳ `src/Shuryan.Infrastructure/Services/Otp/OtpService.cs`
- ⏳ `src/Shuryan.Infrastructure/ExternalServices/EmailService.cs`

### 6. Database Layer
- ⏳ `src/Shuryan.Infrastructure/Data/ApplicationDbContext.cs`

---

## 📝 الخطوة التالية

**دلوقتي انت جاهز تبدأ الاختبار!**

1. **شغل المشروع** في Debug Mode
2. **حط Breakpoints** في الأماكن اللي فوق
3. **ابعت Request** من `test-requests.http`
4. **تتبع الكود** خطوة بخطوة
5. **شوف قيم المتغيرات** في كل Checkpoint
6. **سجل أي مشاكل** تلاقيها

**يلا نبدأ! 🚀**
