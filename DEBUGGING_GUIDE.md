# 🐛 دليل الـ Debugging الشامل - ShurYan Backend

## 📋 المحتويات
1. [الإعداد الأولي](#setup)
2. [استخدام Breakpoints](#breakpoints)
3. [استخدام Logging](#logging)
4. [اختبار الـ Endpoints](#testing)
5. [تتبع المشاكل الشائعة](#common-issues)
6. [أدوات مساعدة](#tools)

---

## 🔧 الإعداد الأولي {#setup}

### الخطوة 1: تشغيل المشروع في Debug Mode

```bash
# في Terminal
cd "e:\ShurYan Project\ShurYan-Backend"
dotnet run --project src/Shuryan.API --configuration Debug
```

**أو في Visual Studio:**
- اضغط `F5` لتشغيل المشروع مع Debugging
- أو اضغط `Ctrl + F5` للتشغيل بدون Debugging

### الخطوة 2: تأكد من الـ Configuration

**في `appsettings.Development.json`:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Shuryan": "Debug"  // عشان تشوف كل الـ Logs بتاعتك
    }
  }
}
```

### الخطوة 3: تثبيت الأدوات المطلوبة

**VS Code Extensions:**
- **C# Dev Kit** - للـ Debugging
- **REST Client** - لاختبار الـ HTTP Requests
- **.NET Core Test Explorer** - للـ Unit Tests

**أو استخدم Visual Studio 2022** - كل حاجة built-in

---

## 🎯 استخدام Breakpoints {#breakpoints}

### إيه هو الـ Breakpoint؟
**ببساطة:** نقطة توقف في الكود، لما البرنامج يوصلها بيقف وتقدر تشوف قيم المتغيرات والـ State

### إزاي تحط Breakpoint؟

#### في Visual Studio:
1. افتح الملف اللي عاوز تعمل فيه Debug (مثلاً `AuthController.cs`)
2. اضغط على الهامش الأيسر جنب رقم السطر
3. هتظهر دائرة حمرا 🔴

#### في VS Code:
1. افتح الملف
2. اضغط `F9` على السطر اللي عاوزه
3. أو اضغط على الهامش الأيسر

### أماكن مهمة تحط فيها Breakpoints:

#### 1. أول سطر في الـ Controller Method
```csharp
[HttpPost("register/patient")]
public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest dto)
{
    var ipAddress = GetIpAddress(); // 🔴 حط Breakpoint هنا
    var result = await _authService.RegisterPatientAsync(dto, ipAddress);
    // ...
}
```

**الهدف:** تتأكد إن الـ Request وصل للـ Controller وإن الـ DTO فيه البيانات الصح

#### 2. قبل استدعاء الـ Service
```csharp
var ipAddress = GetIpAddress();
var result = await _authService.RegisterPatientAsync(dto, ipAddress); // 🔴 هنا
```

**الهدف:** تشوف الـ Parameters اللي بتتبعت للـ Service

#### 3. بعد رجوع النتيجة من الـ Service
```csharp
var result = await _authService.RegisterPatientAsync(dto, ipAddress);

if (!result.IsSuccess) // 🔴 هنا
{
    return StatusCode(result.StatusCode ?? 400, result);
}
```

**الهدف:** تشوف الـ Result اللي رجع من الـ Service وإيه الـ Status

#### 4. في الـ Service Method نفسه
```csharp
public async Task<ServiceResult<AuthResponse>> RegisterPatientAsync(RegisterPatientRequest dto, string? ipAddress)
{
    // 🔴 حط Breakpoint هنا
    var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
    
    if (existingUser != null) // 🔴 وهنا
    {
        return ServiceResult<AuthResponse>.Failure("Email already exists", 400);
    }
    // ...
}
```

**الهدف:** تتبع الـ Logic خطوة بخطوة

### إزاي تستخدم الـ Breakpoints؟

1. **شغل المشروع في Debug Mode** (`F5`)
2. **ابعت Request** من Postman أو HTTP file
3. **البرنامج هيقف** عند الـ Breakpoint
4. **دلوقتي تقدر:**
   - **تشوف قيم المتغيرات** - بص على الـ Variables Panel
   - **تنفذ Expressions** - في الـ Debug Console
   - **تتحرك خطوة بخطوة:**
     - `F10` - Step Over (ينفذ السطر ويروح للسطر اللي بعده)
     - `F11` - Step Into (يدخل جوا الـ Method)
     - `Shift + F11` - Step Out (يخرج من الـ Method)
     - `F5` - Continue (يكمل لحد الـ Breakpoint اللي بعده)

### مثال عملي:

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest dto)
{
    var ipAddress = GetIpAddress(); // 🔴 Breakpoint 1
    // لما توقف هنا:
    // - شوف قيمة dto.Email
    // - شوف قيمة dto.Password (مش هتشوفها واضحة لأسباب أمنية)
    // - شوف قيمة ipAddress
    
    var result = await _authService.LoginAsync(dto, ipAddress); // 🔴 Breakpoint 2
    // لما توقف هنا:
    // - اضغط F11 عشان تدخل جوا LoginAsync
    // - أو اضغط F10 عشان تنفذها وتشوف النتيجة
    
    if (!result.IsSuccess) // 🔴 Breakpoint 3
    {
        // لما توقف هنا:
        // - شوف result.IsSuccess
        // - شوف result.Message
        // - شوف result.StatusCode
        return StatusCode(result.StatusCode ?? 401, result);
    }
    
    return Ok(result); // 🔴 Breakpoint 4
    // لما توقف هنا:
    // - شوف result.Data (الـ Token والـ User Info)
}
```

---

## 📝 استخدام Logging {#logging}

### ليه الـ Logging مهم؟
- **بيسجل كل حاجة بتحصل** في البرنامج
- **بيساعدك تتبع المشاكل** في Production
- **بيوفر عليك وقت** في الـ Debugging

### إزاي تستخدم الـ Logger؟

#### 1. حقن الـ Logger في الـ Constructor
```csharp
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger; // ✅ هنا
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger; // ✅ وهنا
    }
}
```

#### 2. استخدام الـ Logger في الـ Methods

```csharp
[HttpPost("register/patient")]
public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest dto)
{
    // ✅ Log المعلومات المهمة
    _logger.LogInformation("RegisterPatient called with email: {Email}", dto.Email);
    
    var ipAddress = GetIpAddress();
    _logger.LogDebug("IP Address: {IpAddress}", ipAddress);
    
    try
    {
        var result = await _authService.RegisterPatientAsync(dto, ipAddress);
        
        if (!result.IsSuccess)
        {
            // ✅ Log الأخطاء
            _logger.LogWarning("Registration failed for {Email}: {Message}", 
                dto.Email, result.Message);
            return StatusCode(result.StatusCode ?? 400, result);
        }
        
        // ✅ Log النجاح
        _logger.LogInformation("Patient registered successfully: {Email}", dto.Email);
        return StatusCode(201, result);
    }
    catch (Exception ex)
    {
        // ✅ Log الـ Exceptions
        _logger.LogError(ex, "Error registering patient {Email}", dto.Email);
        return StatusCode(500, new { Message = "Internal server error" });
    }
}
```

### مستويات الـ Logging:

| Level | متى تستخدمه | مثال |
|-------|-------------|------|
| **Trace** | تفاصيل دقيقة جداً | `_logger.LogTrace("Entering method")` |
| **Debug** | معلومات للـ Development | `_logger.LogDebug("User ID: {Id}", userId)` |
| **Information** | معلومات عامة | `_logger.LogInformation("User logged in")` |
| **Warning** | تحذيرات (مش أخطاء) | `_logger.LogWarning("Invalid login attempt")` |
| **Error** | أخطاء | `_logger.LogError(ex, "Database error")` |
| **Critical** | أخطاء حرجة | `_logger.LogCritical("System failure")` |

### إزاي تشوف الـ Logs؟

#### في Console:
```bash
# لما تشغل المشروع، الـ Logs هتظهر في Console
info: Shuryan.API.Controllers.AuthController[0]
      RegisterPatient called with email: test@example.com
```

#### في Output Window (Visual Studio):
- اضغط `Ctrl + Alt + O`
- اختار "Debug" من الـ Dropdown

#### في ملف:
**أضف في `Program.cs`:**
```csharp
builder.Logging.AddFile("Logs/shuryan-{Date}.txt");
```

---

## 🧪 اختبار الـ Endpoints {#testing}

### الطريقة 1: استخدام HTTP File

**الملف:** `test-requests.http`

```http
### Test Login
POST https://localhost:7001/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!@#"
}
```

**الخطوات:**
1. افتح الملف في VS Code
2. اضغط "Send Request" فوق الـ Request
3. شوف الـ Response في الـ Panel اللي على اليمين

### الطريقة 2: استخدام Postman

1. **إنشاء Collection جديدة** - "ShurYan API"
2. **إضافة Request:**
   - Method: POST
   - URL: `https://localhost:7001/api/auth/login`
   - Body: Raw → JSON
   ```json
   {
     "email": "test@example.com",
     "password": "Test123!@#"
   }
   ```
3. **اضغط Send**
4. **شوف الـ Response:**
   - Status Code
   - Response Body
   - Response Time

### الطريقة 3: استخدام cURL

```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!@#"
  }'
```

### سيناريو اختبار كامل:

#### 1. Register Patient
```http
POST https://localhost:7001/api/auth/register/patient
Content-Type: application/json

{
  "email": "newpatient@test.com",
  "password": "Test123!@#",
  "firstName": "Ahmed",
  "lastName": "Mohamed",
  "phoneNumber": "01234567890",
  "dateOfBirth": "1990-01-01"
}
```

**Expected Response:**
```json
{
  "isSuccess": true,
  "message": "Registration successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "refresh-token-here",
    "user": {
      "id": "guid-here",
      "email": "newpatient@test.com",
      "userType": "Patient"
    }
  }
}
```

#### 2. Verify Email
```http
POST https://localhost:7001/api/auth/verify-email
Content-Type: application/json

{
  "email": "newpatient@test.com",
  "otp": "123456"
}
```

#### 3. Login
```http
POST https://localhost:7001/api/auth/login
Content-Type: application/json

{
  "email": "newpatient@test.com",
  "password": "Test123!@#"
}
```

#### 4. Get Current User (يحتاج Token)
```http
GET https://localhost:7001/api/auth/me
Authorization: Bearer eyJhbGc...
```

---

## 🔍 تتبع المشاكل الشائعة {#common-issues}

### المشكلة 1: 401 Unauthorized

**الأسباب المحتملة:**
- الـ Token مش موجود في الـ Header
- الـ Token expired
- الـ Token غلط

**الحل:**
```csharp
// حط Breakpoint هنا
[Authorize]
[HttpGet("me")]
public async Task<IActionResult> GetCurrentUser()
{
    var userId = GetCurrentUserId(); // 🔴 شوف الـ userId
    
    if (userId == Guid.Empty) // 🔴 لو Empty، يبقى مفيش Token
    {
        _logger.LogWarning("Unauthorized access attempt");
        return Unauthorized(new { message = "User not authenticated" });
    }
    // ...
}
```

**التحقق:**
1. شوف الـ Request Headers في Postman
2. تأكد إن فيه `Authorization: Bearer <token>`
3. جرب تعمل Login تاني وتاخد Token جديد

### المشكلة 2: 400 Bad Request

**الأسباب المحتملة:**
- الـ DTO فيه بيانات غلط
- الـ Validation فشل
- الـ Required Fields مش موجودة

**الحل:**
```csharp
[HttpPost("register/patient")]
public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest dto)
{
    if (!ModelState.IsValid) // 🔴 حط Breakpoint هنا
    {
        _logger.LogWarning("Invalid model state: {Errors}", 
            string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)));
        
        return BadRequest(ModelState); // شوف الـ Errors
    }
    // ...
}
```

**التحقق:**
1. شوف الـ ModelState.Errors
2. تأكد إن كل الـ Required Fields موجودة
3. تأكد إن الـ Data Types صحيحة

### المشكلة 3: 404 Not Found

**الأسباب المحتملة:**
- الـ ID مش موجود في الـ Database
- الـ Route غلط
- الـ Entity محذوف (Soft Delete)

**الحل:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<PatientResponse>> GetPatient(Guid id)
{
    _logger.LogInformation("GetPatient called with ID: {Id}", id); // 🔴 Log الـ ID
    
    var patient = await _patientService.GetPatientByIdAsync(id);
    
    if (patient == null) // 🔴 حط Breakpoint هنا
    {
        _logger.LogWarning("Patient not found: {Id}", id);
        return NotFound(new { Message = $"Patient with ID {id} not found" });
    }
    
    return Ok(patient);
}
```

**التحقق:**
1. تأكد إن الـ ID صحيح
2. شوف الـ Database وتأكد إن الـ Record موجود
3. تأكد إن IsDeleted = false

### المشكلة 4: 500 Internal Server Error

**الأسباب المحتملة:**
- Exception مش متوقع
- مشكلة في الـ Database
- Null Reference Exception

**الحل:**
```csharp
[HttpPost]
public async Task<ActionResult<PatientResponse>> CreatePatient([FromBody] CreatePatientRequest request)
{
    try
    {
        var patient = await _patientService.CreatePatientAsync(request);
        return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
    }
    catch (Exception ex) // 🔴 حط Breakpoint هنا
    {
        _logger.LogError(ex, "Error creating patient: {Message}", ex.Message);
        
        // شوف الـ Exception Details
        // - ex.Message
        // - ex.StackTrace
        // - ex.InnerException
        
        return StatusCode(500, new { 
            Message = "An error occurred while creating the patient", 
            Details = ex.Message 
        });
    }
}
```

---

## 🛠️ أدوات مساعدة {#tools}

### 1. Swagger UI
**URL:** `https://localhost:7001/swagger`

**الفوائد:**
- تشوف كل الـ Endpoints
- تختبر الـ Endpoints مباشرة
- تشوف الـ Request/Response Models

### 2. SQL Server Profiler
**الفائدة:** تشوف الـ SQL Queries اللي بتتنفذ

### 3. Postman Collections
**الفائدة:** تحفظ كل الـ Requests وتشغلها بسرعة

### 4. Visual Studio Diagnostic Tools
**الفائدة:** تشوف الـ Memory Usage والـ CPU Usage

---

## 📊 Debugging Workflow المثالي

```
1. فهم المشكلة
   ↓
2. حط Breakpoints في الأماكن المهمة
   ↓
3. شغل المشروع في Debug Mode
   ↓
4. ابعت Request
   ↓
5. لما يوقف عند Breakpoint:
   - شوف قيم المتغيرات
   - نفذ Expressions
   - تحرك خطوة بخطوة
   ↓
6. لو لقيت المشكلة:
   - اعمل Fix
   - اعمل Re-test
   ↓
7. لو ملقتش المشكلة:
   - حط Breakpoints أكتر
   - شوف الـ Logs
   - ارجع للخطوة 3
```

---

**نصيحة أخيرة:** الـ Debugging مهارة بتتحسن مع الممارسة. كل ما تعمل Debug أكتر، كل ما هتبقى أسرع في لقاء المشاكل! 🚀
