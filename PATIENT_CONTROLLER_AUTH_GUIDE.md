# 🔐 دليل Authentication & Authorization - Patient Controller

## 📋 المحتويات
1. [نظرة عامة](#overview)
2. [الـ Authorization Levels](#levels)
3. [الـ Endpoints وصلاحياتها](#endpoints)
4. [إزاي تستخدمها](#usage)
5. [أمثلة عملية](#examples)

---

## 🎯 نظرة عامة {#overview}

الـ **Patient Controller** دلوقتي محمي بـ:
1. **Authentication**: لازم User يكون مسجل دخول (JWT Token)
2. **Authorization**: لازم User يكون من نوع **Patient**
3. **Data Access Control**: Patient يقدر يشوف/يعدل بياناته بس

---

## 🔒 الـ Authorization Levels {#levels}

### Level 1: Controller-Level Authorization
```csharp
[Authorize(Roles = "Patient")] // على مستوى الـ Controller
public class PatientsController : ControllerBase
```
**المعنى:** كل الـ Endpoints في الـ Controller محتاجة:
- ✅ JWT Token صحيح
- ✅ User من نوع **Patient**

---

### Level 2: Method-Level Authorization
بعض الـ Endpoints عندها صلاحيات إضافية:

```csharp
[Authorize(Roles = "Admin")] // على مستوى الـ Method
public async Task<ActionResult> CreatePatient(...)
```
**المعنى:** الـ Endpoint ده محتاج **Admin** مش Patient

---

### Level 3: Data-Level Authorization
```csharp
if (!IsAccessingOwnData(id))
{
    return Forbid(); // 403 Forbidden
}
```
**المعنى:** Patient يقدر يشوف/يعدل بياناته بس

---

## 📍 الـ Endpoints وصلاحياتها {#endpoints}

### ✅ Endpoints للـ Patient (Own Data Only)

#### 1. Get My Profile
```http
GET /api/patients/me
Authorization: Bearer [patient-token]
```
**الصلاحيات:**
- ✅ Patient يشوف بياناته
- ❌ مش محتاج ID (بياخده من الـ Token)

**Response:**
```json
{
  "id": "patient-guid",
  "firstName": "Ahmed",
  "lastName": "Mohamed",
  "email": "ahmed@test.com",
  ...
}
```

---

#### 2. Get Patient By ID
```http
GET /api/patients/{id}
Authorization: Bearer [patient-token]
```
**الصلاحيات:**
- ✅ Patient يشوف بياناته (لو ID = Patient ID)
- ❌ Patient مش يقدر يشوف بيانات patient تاني

**Success Response (Own Data):**
```json
{
  "id": "patient-guid",
  "firstName": "Ahmed",
  ...
}
```

**Error Response (Other Patient):**
```http
403 Forbidden
```

---

#### 3. Update Patient
```http
PUT /api/patients/{id}
Authorization: Bearer [patient-token]
Content-Type: application/json

{
  "firstName": "Ahmed Updated",
  "lastName": "Mohamed",
  "phoneNumber": "01098765432"
}
```
**الصلاحيات:**
- ✅ Patient يعدل بياناته (لو ID = Patient ID)
- ❌ Patient مش يقدر يعدل بيانات patient تاني

---

#### 4. Get My Medical History
```http
GET /api/patients/{patientId}/medical-history
Authorization: Bearer [patient-token]
```
**الصلاحيات:**
- ✅ Patient يشوف medical history بتاعه
- ❌ مش يقدر يشوف medical history لـ patient تاني

---

#### 5. Get My Appointments
```http
GET /api/patients/{patientId}/appointments
Authorization: Bearer [patient-token]
```
**الصلاحيات:**
- ✅ Patient يشوف appointments بتاعته
- ❌ مش يقدر يشوف appointments لـ patient تاني

---

#### 6. Get My Prescriptions
```http
GET /api/patients/{patientId}/prescriptions
Authorization: Bearer [patient-token]
```
**الصلاحيات:**
- ✅ Patient يشوف prescriptions بتاعته
- ❌ مش يقدر يشوف prescriptions لـ patient تاني

---

#### 7. Update My Address
```http
PUT /api/patients/{patientId}/address
Authorization: Bearer [patient-token]
Content-Type: application/json

{
  "street": "123 Main St",
  "city": "Cairo",
  "governorate": "Cairo",
  "postalCode": "12345"
}
```
**الصلاحيات:**
- ✅ Patient يعدل address بتاعه
- ❌ مش يقدر يعدل address لـ patient تاني

---

### 🔐 Endpoints للـ Admin فقط

#### 1. Create Patient
```http
POST /api/patients
Authorization: Bearer [admin-token]
Content-Type: application/json

{
  "firstName": "Ahmed",
  "lastName": "Mohamed",
  "email": "ahmed@test.com",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1990-01-15"
}
```
**الصلاحيات:**
- ✅ Admin يقدر يعمل Create
- ❌ Patient مش يقدر يعمل Create

---

#### 2. Delete Patient
```http
DELETE /api/patients/{id}
Authorization: Bearer [admin-token]
```
**الصلاحيات:**
- ✅ Admin يقدر يعمل Delete
- ❌ Patient مش يقدر يعمل Delete

---

#### 3. Get All Patients
```http
GET /api/patients
Authorization: Bearer [admin-token]
```
**الصلاحيات:**
- ✅ Admin يشوف كل الـ Patients
- ❌ Patient مش يقدر يشوف كل الـ Patients

---

## 🧪 إزاي تستخدمها {#usage}

### الخطوة 1: Login كـ Patient
```http
POST http://localhost:5117/api/auth/login
Content-Type: application/json

{
  "email": "ahmed.patient@test.com",
  "password": "Test@123456"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "ahmed.patient@test.com",
      "userType": "Patient"
    }
  }
}
```

**انسخ:**
- ✅ `accessToken`
- ✅ `user.id` (Patient ID)

---

### الخطوة 2: استخدم الـ Token في Swagger

1. اضغط **Authorize** 🔓 في Swagger
2. حط: `Bearer [your-access-token]`
3. اضغط **Authorize**
4. اضغط **Close**

---

### الخطوة 3: جرب الـ Endpoints

#### ✅ Test 1: Get My Profile
```http
GET /api/patients/me
Authorization: Bearer [token]
```
**Expected:** `200 OK` + Your data

---

#### ✅ Test 2: Get Own Data By ID
```http
GET /api/patients/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer [token]
```
**Expected:** `200 OK` + Your data

---

#### ❌ Test 3: Get Another Patient's Data
```http
GET /api/patients/different-patient-id
Authorization: Bearer [token]
```
**Expected:** `403 Forbidden`

---

#### ✅ Test 4: Update Own Data
```http
PUT /api/patients/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer [token]
{
  "firstName": "Ahmed Updated",
  "phoneNumber": "01098765432"
}
```
**Expected:** `200 OK` + Updated data

---

#### ❌ Test 5: Update Another Patient's Data
```http
PUT /api/patients/different-patient-id
Authorization: Bearer [token]
{...}
```
**Expected:** `403 Forbidden`

---

#### ❌ Test 6: Create Patient (Patient Token)
```http
POST /api/patients
Authorization: Bearer [patient-token]
{...}
```
**Expected:** `403 Forbidden` - "Insufficient permissions"

---

#### ✅ Test 7: Create Patient (Admin Token)
```http
POST /api/patients
Authorization: Bearer [admin-token]
{...}
```
**Expected:** `201 Created`

---

## 📊 جدول الصلاحيات

| Endpoint | Patient (Own Data) | Patient (Other Data) | Admin |
|----------|-------------------|---------------------|-------|
| `GET /me` | ✅ | N/A | ✅ |
| `GET /{id}` | ✅ | ❌ 403 | ✅ |
| `POST /` | ❌ 403 | ❌ 403 | ✅ |
| `PUT /{id}` | ✅ | ❌ 403 | ✅ |
| `DELETE /{id}` | ❌ 403 | ❌ 403 | ✅ |
| `GET /` (All) | ❌ 403 | ❌ 403 | ✅ |
| `GET /{id}/medical-history` | ✅ | ❌ 403 | ✅ |
| `GET /{id}/appointments` | ✅ | ❌ 403 | ✅ |
| `GET /{id}/prescriptions` | ✅ | ❌ 403 | ✅ |
| `PUT /{id}/address` | ✅ | ❌ 403 | ✅ |

---

## 🔍 الكود الداخلي

### Helper Methods

```csharp
/// <summary>
/// Get current authenticated patient ID from JWT token
/// </summary>
private Guid GetCurrentPatientId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return Guid.Empty;
    }
    return userId;
}

/// <summary>
/// Check if current user is accessing their own data
/// </summary>
private bool IsAccessingOwnData(Guid patientId)
{
    var currentUserId = GetCurrentPatientId();
    return currentUserId == patientId;
}
```

---

### Authorization Flow

```
1. Request يوصل للـ Controller
   ↓
2. ASP.NET Core يتحقق من JWT Token
   ↓
3. يتحقق من Role = "Patient"
   ↓
4. لو مش Patient → 403 Forbidden
   ↓
5. لو Patient → ينفذ الـ Method
   ↓
6. الـ Method يتحقق من IsAccessingOwnData()
   ↓
7. لو مش Own Data → 403 Forbidden
   ↓
8. لو Own Data → ينفذ الـ Logic
```

---

## 🚨 Error Responses

### 401 Unauthorized
```json
{
  "message": "Authentication required"
}
```
**السبب:**
- مفيش Token
- Token غلط
- Token expired

---

### 403 Forbidden
```json
{
  "message": "Insufficient permissions"
}
```
**السبب:**
- User مش Patient (Role غلط)
- Patient بيحاول يشوف/يعدل بيانات patient تاني

---

### 404 Not Found
```json
{
  "message": "Patient with ID {id} not found"
}
```
**السبب:**
- Patient ID مش موجود
- Patient محذوف

---

## 🎯 ملخص سريع

### ✅ Patient يقدر:
- يشوف بياناته
- يعدل بياناته
- يشوف medical history بتاعه
- يشوف appointments بتاعته
- يشوف prescriptions بتاعته
- يعدل address بتاعه

### ❌ Patient مش يقدر:
- يشوف بيانات patients تانيين
- يعدل بيانات patients تانيين
- يعمل Create لـ patient جديد
- يعمل Delete لأي patient
- يشوف كل الـ Patients

### ✅ Admin يقدر:
- كل حاجة Patient يقدر يعملها
- يشوف كل الـ Patients
- يعمل Create لـ patient جديد
- يعمل Delete لأي patient
- يشوف/يعدل بيانات أي patient

---

## 🔐 Security Best Practices

1. **Always use HTTPS** في Production
2. **Never share JWT tokens**
3. **Token expiration**: 15 دقيقة (configurable)
4. **Refresh tokens**: 7 أيام (configurable)
5. **Validate all inputs** (FluentValidation)
6. **Log all access attempts**
7. **Rate limiting** للـ sensitive endpoints

---

## 📝 Notes

- الـ `[Authorize]` attribute على مستوى الـ Controller بيطبق على كل الـ Methods
- لو عاوز Method معين يكون **Public** (بدون authentication)، استخدم `[AllowAnonymous]`
- الـ `IsAccessingOwnData()` method بتتحقق من الـ Patient ID في الـ Token
- الـ JWT Token فيه Claims زي: `sub` (User ID), `role` (Patient/Doctor/Admin)

---

**تم! 🎉 دلوقتي الـ Patient Controller محمي بالكامل!**
