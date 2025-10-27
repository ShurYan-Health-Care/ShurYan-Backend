# ✅ Checklist مراجعة الـ Controllers - ShurYan Backend

## 📌 إزاي تستخدم الـ Checklist ده؟

1. **افتح الـ Controller** اللي عاوز تراجعه
2. **اقرأ كل Endpoint** واحد واحد
3. **علّم ✅** على كل نقطة لما تتأكد منها
4. **سجل أي مشاكل** تلاقيها في ملف منفصل
5. **اعمل Fix** للمشاكل وارجع راجع تاني

---

## 🔐 AuthController Review Checklist

### ✅ General Controller Setup
- [ ] الـ `[ApiController]` Attribute موجود
- [ ] الـ `[Route("api/[controller]")]` صحيح
- [ ] الـ Constructor بيستقبل `IAuthService` و `ILogger`
- [ ] الـ Dependency Injection شغال صح
- [ ] فيه XML Comments لكل Method

### ✅ Registration Endpoints

#### `/api/auth/register/patient`
- [ ] الـ HTTP Method: `POST`
- [ ] الـ `[FromBody]` موجود على الـ DTO
- [ ] بيجيب الـ IP Address من `GetIpAddress()`
- [ ] بيمرر الـ DTO والـ IP للـ Service
- [ ] بيتعامل مع `result.IsSuccess` صح
- [ ] بيرجع `201 Created` للنجاح
- [ ] بيرجع `400 Bad Request` للفشل
- [ ] الـ `ProducesResponseType` موجود
- [ ] فيه Logging للـ Registration Attempts

**Test Cases:**
- [ ] تسجيل مريض جديد بنجاح
- [ ] محاولة تسجيل بـ Email موجود (Should Fail)
- [ ] محاولة تسجيل بـ Password ضعيف (Should Fail)
- [ ] محاولة تسجيل بدون Required Fields (Should Fail)

#### `/api/auth/register/doctor`
- [ ] نفس النقاط السابقة
- [ ] بيتحقق من الـ Specialization
- [ ] بيتحقق من الـ License Number

#### `/api/auth/register/laboratory`
- [ ] نفس النقاط السابقة
- [ ] بيتحقق من الـ Laboratory Name
- [ ] بيتحقق من الـ License Number

#### `/api/auth/register/pharmacy`
- [ ] نفس النقاط السابقة
- [ ] بيتحقق من الـ Pharmacy Name
- [ ] بيتحقق من الـ License Number

### ✅ Email Verification Endpoints

#### `/api/auth/verify-email`
- [ ] الـ HTTP Method: `POST`
- [ ] بيتحقق من الـ OTP صح
- [ ] بيحدث `IsEmailVerified = true`
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `400 Bad Request` للـ OTP غلط
- [ ] الـ OTP بيexpire بعد مدة معينة

**Test Cases:**
- [ ] تأكيد Email بـ OTP صحيح
- [ ] محاولة تأكيد بـ OTP غلط (Should Fail)
- [ ] محاولة تأكيد بـ OTP منتهي (Should Fail)
- [ ] محاولة تأكيد Email متأكد فعلاً (Should Handle)

#### `/api/auth/resend-verification`
- [ ] الـ HTTP Method: `POST`
- [ ] بيبعت OTP جديد
- [ ] فيه Rate Limiting (429 Too Many Requests)
- [ ] بيرجع `200 OK` للنجاح

**Test Cases:**
- [ ] إعادة إرسال OTP بنجاح
- [ ] محاولة إعادة إرسال أكتر من مرة بسرعة (Should Rate Limit)

### ✅ Login Endpoints

#### `/api/auth/login`
- [ ] الـ HTTP Method: `POST`
- [ ] بيتحقق من Email و Password
- [ ] بيتحقق إن Email متأكد
- [ ] بيرجع Access Token + Refresh Token
- [ ] بيسجل الـ IP Address
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `401 Unauthorized` للـ Credentials غلط
- [ ] بيرجع `403 Forbidden` للـ Email مش متأكد

**Test Cases:**
- [ ] تسجيل دخول بنجاح
- [ ] محاولة تسجيل دخول بـ Password غلط (Should Fail)
- [ ] محاولة تسجيل دخول بـ Email مش موجود (Should Fail)
- [ ] محاولة تسجيل دخول بـ Email مش متأكد (Should Fail)

#### `/api/auth/google-login`
- [ ] الـ HTTP Method: `POST`
- [ ] بيتحقق من Google ID Token
- [ ] بيعمل Register لو User جديد
- [ ] بيعمل Login لو User موجود
- [ ] بيرجع `200 OK` للـ Login
- [ ] بيرجع `201 Created` للـ Register

**Test Cases:**
- [ ] تسجيل دخول بـ Google لأول مرة (Should Register)
- [ ] تسجيل دخول بـ Google لمستخدم موجود (Should Login)
- [ ] محاولة تسجيل دخول بـ Token غلط (Should Fail)

### ✅ Password Management Endpoints

#### `/api/auth/forgot-password`
- [ ] الـ HTTP Method: `POST`
- [ ] بيبعت OTP على الإيميل
- [ ] فيه Rate Limiting
- [ ] بيرجع `200 OK` للنجاح

#### `/api/auth/reset-password`
- [ ] الـ HTTP Method: `POST`
- [ ] بيتحقق من الـ OTP الأول
- [ ] بيحدث الـ Password
- [ ] بيعمل Hash للـ Password الجديد
- [ ] بيرجع `200 OK` للنجاح

#### `/api/auth/change-password`
- [ ] الـ HTTP Method: `POST`
- [ ] الـ `[Authorize]` Attribute موجود
- [ ] بيجيب User ID من JWT Claims
- [ ] بيتحقق من Old Password
- [ ] بيحدث للـ Password الجديد
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `401 Unauthorized` لو مفيش Token

**Test Cases:**
- [ ] تغيير Password بنجاح
- [ ] محاولة تغيير بـ Old Password غلط (Should Fail)
- [ ] محاولة تغيير بدون Token (Should Fail)

### ✅ Token Management Endpoints

#### `/api/auth/refresh-token`
- [ ] الـ HTTP Method: `POST`
- [ ] بيتحقق من Refresh Token
- [ ] بيرجع Access Token جديد
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `401 Unauthorized` للـ Token غلط

#### `/api/auth/logout`
- [ ] الـ HTTP Method: `POST`
- [ ] الـ `[Authorize]` Attribute موجود
- [ ] بيعمل Revoke للـ Refresh Token
- [ ] بيرجع `200 OK` للنجاح

### ✅ User Info Endpoint

#### `/api/auth/me`
- [ ] الـ HTTP Method: `GET`
- [ ] الـ `[Authorize]` Attribute موجود
- [ ] بيجيب User ID من JWT Claims صح
- [ ] بيرجع كل بيانات المستخدم
- [ ] بيتعامل مع كل User Types
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `401 Unauthorized` لو مفيش Token

### ✅ Helper Methods

#### `GetCurrentUserId()`
- [ ] بيجيب Claim من `ClaimTypes.NameIdentifier`
- [ ] بيعمل Parse للـ Guid صح
- [ ] بيرجع `Guid.Empty` لو فشل

#### `GetIpAddress()`
- [ ] بيتحقق من `X-Forwarded-For` Header
- [ ] بيرجع IP من `HttpContext.Connection.RemoteIpAddress`
- [ ] بيتعامل مع Null Cases

---

## 👥 PatientsController Review Checklist

### ✅ General Controller Setup
- [ ] الـ `[ApiController]` Attribute موجود
- [ ] الـ `[Route("api/[controller]")]` صحيح
- [ ] الـ Constructor بيستقبل `IPatientService`
- [ ] فيه XML Comments لكل Method

### ✅ Basic CRUD Operations

#### `GET /api/patients/{id}`
- [ ] بيرجع Patient بكل التفاصيل
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `404 Not Found` لو مش موجود
- [ ] الـ Response Type واضح

#### `POST /api/patients`
- [ ] بيعمل Validation على ModelState
- [ ] بيرجع `201 Created` مع Location Header
- [ ] بيرجع `400 Bad Request` للـ Validation Errors
- [ ] فيه Try-Catch للـ Exceptions

#### `PUT /api/patients/{id}`
- [ ] بيعمل Validation على ModelState
- [ ] بيحدث الـ Patient والـ Address
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `404 Not Found` لو مش موجود

#### `DELETE /api/patients/{id}`
- [ ] بيعمل Soft Delete (IsDeleted = true)
- [ ] بيرجع `204 No Content` للنجاح
- [ ] بيرجع `404 Not Found` لو مش موجود

#### `POST /api/patients/{id}/restore`
- [ ] بيسترجع Patient محذوف
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `404 Not Found` لو مش موجود أو مش محذوف

### ✅ Query Operations

#### `GET /api/patients/paginated`
- [ ] الـ Pagination شغالة (PageNumber, PageSize)
- [ ] بيرجع Total Count
- [ ] بيرجع Has Next/Previous Page
- [ ] الـ Performance كويس

#### `POST /api/patients/search`
- [ ] بيدعم Filters متعددة
- [ ] الـ Search Term بيشتغل على Name و Email
- [ ] الـ Pagination شغالة
- [ ] بيرجع Results مترتبة

### ✅ Medical History Operations

#### `GET /api/patients/{patientId}/medical-history`
- [ ] بيتحقق إن Patient موجود
- [ ] بيرجع كل Medical History Items
- [ ] بيرجع `200 OK` للنجاح
- [ ] بيرجع `404 Not Found` لو Patient مش موجود

#### `POST /api/patients/{patientId}/medical-history`
- [ ] بيعمل Validation على DTO
- [ ] بيتحقق إن Patient موجود
- [ ] بيضيف Item جديد
- [ ] بيرجع `201 Created`

#### `PUT /api/patients/{patientId}/medical-history/{itemId}`
- [ ] بيتحقق إن Patient و Item موجودين
- [ ] بيحدث Item
- [ ] بيرجع `200 OK`

#### `DELETE /api/patients/{patientId}/medical-history/{itemId}`
- [ ] بيحذف Item
- [ ] بيرجع `204 No Content`

### ✅ Appointments Operations

#### `GET /api/patients/{patientId}/appointments`
- [ ] بيرجع كل Appointments
- [ ] بيتحقق إن Patient موجود
- [ ] بيرجع `200 OK`

#### `GET /api/patients/{patientId}/appointments/upcoming`
- [ ] بيفلتر Appointments (DateTime > Now)
- [ ] بيرتب من الأقرب للأبعد
- [ ] بيرجع `200 OK`

#### `GET /api/patients/{patientId}/appointments/past`
- [ ] بيفلتر Appointments (DateTime < Now)
- [ ] بيرتب من الأحدث للأقدم
- [ ] بيرجع `200 OK`

#### `GET /api/patients/{patientId}/appointments/next`
- [ ] بيرجع أقرب Appointment
- [ ] بيرجع `404 Not Found` لو مفيش
- [ ] بيرجع `200 OK`

### ✅ Prescriptions Operations

#### `GET /api/patients/{patientId}/prescriptions`
- [ ] بيرجع كل Prescriptions
- [ ] بيشمل Medications
- [ ] بيشمل Doctor Info

#### `GET /api/patients/{patientId}/prescriptions/active`
- [ ] بيفلتر Active Prescriptions (EndDate > Now)
- [ ] بيرجع `200 OK`

### ✅ Lab Orders Operations

#### `GET /api/patients/{patientId}/lab-orders`
- [ ] بيرجع كل Lab Orders
- [ ] بيشمل Tests المطلوبة

#### `GET /api/patients/{patientId}/lab-orders/pending`
- [ ] بيفلتر حسب Status = Pending
- [ ] بيرجع `200 OK`

### ✅ Address Operations

#### `GET /api/patients/{patientId}/address`
- [ ] بيرجع Address لو موجود
- [ ] بيرجع `404 Not Found` لو مفيش

#### `PUT /api/patients/{patientId}/address`
- [ ] بيحدث Address
- [ ] بيتعامل مع Governorate Enum صح
- [ ] بيحفظ Coordinates

### ✅ Profile Operations

#### `PUT /api/patients/{patientId}/profile-image`
- [ ] بيحدث ProfileImageUrl
- [ ] بيرجع `200 OK`

#### `DELETE /api/patients/{patientId}/profile-image`
- [ ] بيحط null في ProfileImageUrl
- [ ] بيرجع `200 OK`

---

## 💊 PharmaciesController Review Checklist

### ✅ General Controller Setup
- [ ] الـ `[ApiController]` Attribute موجود
- [ ] الـ `[Route("api/[controller]")]` صحيح
- [ ] الـ Constructor بيستقبل `IPharmacyService` و `ILogger`
- [ ] فيه XML Comments لكل Method

### ✅ Profile Management

#### `GET /api/pharmacies/{id}`
- [ ] بيرجع Pharmacy بكل التفاصيل
- [ ] بيرجع `200 OK` أو `404 Not Found`

#### `GET /api/pharmacies/me`
- [ ] الـ `[Authorize(Roles = "Pharmacy")]` موجود
- [ ] بيجيب Pharmacy ID من JWT Claims
- [ ] بيرجع `200 OK`

#### `PUT /api/pharmacies/me`
- [ ] الـ `[Authorize(Roles = "Pharmacy")]` موجود
- [ ] بيحدث بيانات Pharmacy
- [ ] بيرجع `200 OK`

#### `DELETE /api/pharmacies/{id}`
- [ ] الـ `[Authorize(Roles = "Admin")]` موجود
- [ ] بيحذف Pharmacy
- [ ] بيرجع `204 No Content`

### ✅ Working Hours Management

#### `GET /api/pharmacies/{id}/working-hours`
- [ ] بيرجع كل Working Hours
- [ ] بيرتب حسب DayOfWeek

#### `POST /api/pharmacies/{id}/working-hours`
- [ ] الـ `[Authorize(Roles = "Pharmacy")]` موجود
- [ ] بيضيف Working Hours
- [ ] بيعمل Validation على الأوقات
- [ ] بيرجع `201 Created`

#### `PUT /api/pharmacies/{id}/working-hours/{workingHoursId}`
- [ ] بيحدث Working Hours
- [ ] بيرجع `200 OK`

#### `DELETE /api/pharmacies/{id}/working-hours/{workingHoursId}`
- [ ] بيحذف Working Hours
- [ ] بيرجع `204 No Content`

#### `GET /api/pharmacies/{id}/is-open`
- [ ] بيحسب إذا Pharmacy مفتوحة دلوقتي
- [ ] بيرجع IsOpen و NextOpenTime
- [ ] بيرجع `200 OK`

### ✅ Orders Management

#### Order Status Flow
- [ ] Pending → Accepted → Preparing → Ready → Dispatched → Delivered
- [ ] مينفعش يرجع لـ Status سابق
- [ ] كل Status Transition بيحدث Timestamp

#### `PUT /api/pharmacies/{id}/orders/{orderId}/accept`
- [ ] بيحدث Status = Accepted
- [ ] بيحدث AcceptedAt
- [ ] بيرجع `200 OK`

#### `PUT /api/pharmacies/{id}/orders/{orderId}/reject`
- [ ] بيحتاج Rejection Reason
- [ ] بيحدث Status = Rejected
- [ ] بيرجع `200 OK`

#### `PUT /api/pharmacies/{id}/orders/{orderId}/prepare`
- [ ] بيتحقق إن Status = Accepted
- [ ] بيحدث Status = Preparing
- [ ] بيرجع `200 OK`

#### `PUT /api/pharmacies/{id}/orders/{orderId}/ready`
- [ ] بيتحقق إن Status = Preparing
- [ ] بيحدث Status = Ready
- [ ] بيرجع `200 OK`

#### `PUT /api/pharmacies/{id}/orders/{orderId}/dispatch`
- [ ] بيتحقق إن Status = Ready
- [ ] بيحدث Status = Dispatched
- [ ] بيحدث DispatchedAt
- [ ] بيرجع `200 OK`

#### `PUT /api/pharmacies/{id}/orders/{orderId}/deliver`
- [ ] بيتحقق إن Status = Dispatched
- [ ] بيحدث Status = Delivered
- [ ] بيحدث DeliveredAt
- [ ] بيرجع `200 OK`

#### `GET /api/pharmacies/{id}/orders/pending`
- [ ] بيفلتر Orders حسب Status = Pending
- [ ] بيرجع `200 OK`

#### `GET /api/pharmacies/{id}/orders/in-progress`
- [ ] بيفلتر Orders (Accepted, Preparing, Ready, Dispatched)
- [ ] بيرجع `200 OK`

#### `GET /api/pharmacies/{id}/orders/completed`
- [ ] بيفلتر Orders (Delivered, Cancelled)
- [ ] بيرجع `200 OK`

### ✅ Search & Discovery

#### `POST /api/pharmacies/search`
- [ ] بيدعم Filters: Name, Governorate, IsOpen, IsVerified
- [ ] الـ Pagination شغالة
- [ ] بيرجع `200 OK`

#### `GET /api/pharmacies/governorate/{governorate}`
- [ ] بيفلتر حسب Governorate
- [ ] بيرجع `200 OK`

#### `GET /api/pharmacies/nearby`
- [ ] بيحسب Distance بـ Haversine Formula
- [ ] بيفلتر حسب Radius
- [ ] بيرتب حسب Distance
- [ ] بيرجع `200 OK`

### ✅ Documents Management

#### `GET /api/pharmacies/{id}/documents`
- [ ] الـ `[Authorize(Roles = "Pharmacy,Admin,Verifier")]` موجود
- [ ] بيرجع كل Documents
- [ ] بيرجع `200 OK`

#### `POST /api/pharmacies/{id}/documents`
- [ ] الـ `[Authorize(Roles = "Pharmacy")]` موجود
- [ ] بيرفع Document
- [ ] بيحفظ Document URL
- [ ] بيرجع `201 Created`

### ✅ Reviews Management

#### `GET /api/pharmacies/{id}/reviews`
- [ ] بيرجع كل Reviews
- [ ] بيرتب من الأحدث للأقدم

#### `GET /api/pharmacies/{id}/reviews/statistics`
- [ ] بيحسب Average Rating
- [ ] بيحسب Total Reviews
- [ ] بيحسب Rating Distribution

### ✅ Verification Management

#### `GET /api/pharmacies/pending-verification`
- [ ] الـ `[Authorize(Roles = "Admin,Verifier")]` موجود
- [ ] بيرجع Pharmacies بـ Status = Pending
- [ ] بيرجع `200 OK`

#### `POST /api/pharmacies/{id}/verify`
- [ ] الـ `[Authorize(Roles = "Admin,Verifier")]` موجود
- [ ] بيحدث Status = Verified
- [ ] بيحدث VerifiedAt
- [ ] بيرجع `200 OK`

#### `POST /api/pharmacies/{id}/reject-verification`
- [ ] الـ `[Authorize(Roles = "Admin,Verifier")]` موجود
- [ ] بيحتاج Rejection Reason
- [ ] بيحدث Status = Rejected
- [ ] بيرجع `200 OK`

---

## 🎯 Final Review Checklist

### Security
- [ ] كل الـ Passwords بتتعمل Hash
- [ ] الـ JWT Tokens بتتولد صح
- [ ] الـ Refresh Tokens بتتخزن آمن
- [ ] الـ Sensitive Data مش بترجع في Response
- [ ] الـ Authorization شغالة على كل Protected Endpoints

### Performance
- [ ] مفيش N+1 Query Problem
- [ ] الـ Pagination مستخدمة للـ Large Lists
- [ ] الـ Async/Await مستخدم صح
- [ ] الـ Database Indexes موجودة

### Error Handling
- [ ] كل الـ Exceptions بتتعامل معاها
- [ ] الـ Error Messages واضحة ومفيدة
- [ ] الـ Status Codes صحيحة
- [ ] فيه Logging للـ Errors

### Testing
- [ ] كل Endpoint اتاختبر بـ Success Case
- [ ] كل Endpoint اتاختبر بـ Failure Cases
- [ ] الـ Edge Cases اتاختبرت
- [ ] الـ Performance اتاختبر

---

**ملحوظة:** استخدم الـ Checklist ده كدليل، ومتنساش تسجل أي مشاكل تلاقيها عشان ترجعلها! 📝
