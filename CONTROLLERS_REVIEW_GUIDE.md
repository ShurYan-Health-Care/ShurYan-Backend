# 🔍 دليل مراجعة الـ Controllers - ShurYan Backend

## 📋 جدول المحتويات
1. [نظرة عامة على الـ Controllers](#overview)
2. [AuthController](#auth)
3. [PatientsController](#patients)
4. [PharmaciesController](#pharmacies)
5. [خطة الـ Debugging](#debugging)
6. [Checklist للمراجعة](#checklist)

---

## 🎯 نظرة عامة {#overview}

عندك **3 Controllers** رئيسية:

| Controller | المسؤولية | عدد Endpoints | الأهمية |
|-----------|-----------|--------------|---------|
| **AuthController** | Authentication & Authorization | 13 | 🔴 حرجة |
| **PatientsController** | إدارة المرضى | 40+ | 🟡 مهمة |
| **PharmaciesController** | إدارة الصيدليات | 35+ | 🟡 مهمة |

---

## 🔐 AuthController {#auth}

**الموقع:** `src/Shuryan.API/Controllers/AuthController.cs`

### الـ Endpoints الرئيسية

#### 1. Registration (4 endpoints)
- `POST /api/auth/register/patient` - تسجيل مريض
- `POST /api/auth/register/doctor` - تسجيل دكتور
- `POST /api/auth/register/laboratory` - تسجيل معمل
- `POST /api/auth/register/pharmacy` - تسجيل صيدلية

#### 2. Email Verification (2 endpoints)
- `POST /api/auth/verify-email` - تأكيد البريد بـ OTP
- `POST /api/auth/resend-verification` - إعادة إرسال OTP

#### 3. Login (2 endpoints)
- `POST /api/auth/login` - تسجيل دخول عادي
- `POST /api/auth/google-login` - تسجيل دخول بـ Google

#### 4. Password Management (3 endpoints)
- `POST /api/auth/forgot-password` - طلب إعادة تعيين
- `POST /api/auth/reset-password` - إعادة تعيين بـ OTP
- `POST /api/auth/change-password` - تغيير (يحتاج Auth)

#### 5. Token Management (2 endpoints)
- `POST /api/auth/refresh-token` - تجديد Token
- `POST /api/auth/logout` - تسجيل خروج

#### 6. User Info (1 endpoint)
- `GET /api/auth/me` - بيانات المستخدم الحالي

---

## 👥 PatientsController {#patients}

**الموقع:** `src/Shuryan.API/Controllers/PatientsController.cs`

### الـ Endpoints مقسمة حسب الوظيفة

#### 1. Basic CRUD
- `GET /api/patients/{id}` - جلب مريض
- `POST /api/patients` - إنشاء مريض
- `PUT /api/patients/{id}` - تحديث مريض
- `DELETE /api/patients/{id}` - حذف (Soft Delete)
- `POST /api/patients/{id}/restore` - استرجاع محذوف

#### 2. Query Operations
- `GET /api/patients/email/{email}` - بحث بالإيميل
- `GET /api/patients` - كل المرضى
- `GET /api/patients/paginated` - مع Pagination
- `POST /api/patients/search` - بحث متقدم
- `GET /api/patients/count` - العدد الكلي

#### 3. Medical History
- `GET /api/patients/{id}/medical-history` - التاريخ الطبي
- `POST /api/patients/{id}/medical-history` - إضافة سجل
- `PUT /api/patients/{id}/medical-history/{itemId}` - تحديث
- `DELETE /api/patients/{id}/medical-history/{itemId}` - حذف

#### 4. Appointments
- `GET /api/patients/{id}/appointments` - كل المواعيد
- `GET /api/patients/{id}/appointments/upcoming` - القادمة
- `GET /api/patients/{id}/appointments/past` - السابقة
- `GET /api/patients/{id}/appointments/next` - القادم مباشرة

#### 5. Prescriptions
- `GET /api/patients/{id}/prescriptions` - كل الروشتات
- `GET /api/patients/{id}/prescriptions/active` - النشطة
- `GET /api/patients/{id}/prescriptions/{prescriptionId}` - روشتة معينة

#### 6. Lab Orders
- `GET /api/patients/{id}/lab-orders` - كل التحاليل
- `GET /api/patients/{id}/lab-orders/pending` - المعلقة
- `GET /api/patients/{id}/lab-orders/{orderId}` - تحليل معين

#### 7. Address
- `GET /api/patients/{id}/address` - جلب العنوان
- `PUT /api/patients/{id}/address` - تحديث العنوان
- `POST /api/patients/address` - إنشاء عنوان

#### 8. Profile
- `PUT /api/patients/{id}/profile-image` - تحديث الصورة
- `DELETE /api/patients/{id}/profile-image` - حذف الصورة

---

## 💊 PharmaciesController {#pharmacies}

**الموقع:** `src/Shuryan.API/Controllers/PharmaciesController.cs`

### الـ Endpoints مقسمة حسب الوظيفة

#### 1. Profile Management
- `GET /api/pharmacies/{id}` - جلب صيدلية
- `GET /api/pharmacies/email/{email}` - بحث بالإيميل
- `GET /api/pharmacies/me` - الصيدلية الحالية (Auth)
- `PUT /api/pharmacies/me` - تحديث البيانات (Auth)
- `DELETE /api/pharmacies/{id}` - حذف (Admin only)

#### 2. Working Hours
- `GET /api/pharmacies/{id}/working-hours` - ساعات العمل
- `POST /api/pharmacies/{id}/working-hours` - إضافة
- `PUT /api/pharmacies/{id}/working-hours/{hoursId}` - تحديث
- `DELETE /api/pharmacies/{id}/working-hours/{hoursId}` - حذف
- `GET /api/pharmacies/{id}/is-open` - هل مفتوحة؟

#### 3. Orders Management (13 endpoints)
- `GET /api/pharmacies/{id}/orders` - كل الطلبات
- `GET /api/pharmacies/{id}/orders/{orderId}` - طلب معين
- `PUT /api/pharmacies/{id}/orders/{orderId}/accept` - قبول
- `PUT /api/pharmacies/{id}/orders/{orderId}/reject` - رفض
- `PUT /api/pharmacies/{id}/orders/{orderId}/prepare` - تحضير
- `PUT /api/pharmacies/{id}/orders/{orderId}/ready` - جاهز
- `PUT /api/pharmacies/{id}/orders/{orderId}/dispatch` - إرسال
- `PUT /api/pharmacies/{id}/orders/{orderId}/deliver` - تسليم
- `GET /api/pharmacies/{id}/orders/pending` - المعلقة
- `GET /api/pharmacies/{id}/orders/in-progress` - قيد التنفيذ
- `GET /api/pharmacies/{id}/orders/completed` - المكتملة

#### 4. Search & Discovery
- `POST /api/pharmacies/search` - بحث متقدم
- `GET /api/pharmacies/governorate/{governorate}` - حسب المحافظة
- `GET /api/pharmacies/nearby` - القريبة

#### 5. Documents
- `GET /api/pharmacies/{id}/documents` - المستندات
- `POST /api/pharmacies/{id}/documents` - رفع مستند

#### 6. Reviews
- `GET /api/pharmacies/{id}/reviews` - التقييمات
- `GET /api/pharmacies/{id}/reviews/statistics` - إحصائيات

#### 7. Verification (Admin/Verifier)
- `GET /api/pharmacies/pending-verification` - المعلقة
- `POST /api/pharmacies/{id}/verify` - توثيق
- `POST /api/pharmacies/{id}/reject-verification` - رفض
- `POST /api/pharmacies/{id}/documents/{docId}/approve` - قبول مستند
- `POST /api/pharmacies/{id}/documents/{docId}/reject` - رفض مستند

---

## 🐛 خطة الـ Debugging المنهجية {#debugging}

### الخطوة 1: فهم الـ Endpoint
1. **اقرأ الكود** وافهم إيه اللي المفروض يحصل
2. **شوف الـ DTOs** المستخدمة (Request & Response)
3. **تتبع الـ Service Method** اللي بيتنادى

### الخطوة 2: تحضير بيئة الاختبار
```bash
# شغل المشروع في Debug Mode
dotnet run --project src/Shuryan.API
```

### الخطوة 3: استخدام Breakpoints
**في Visual Studio / VS Code:**
1. حط Breakpoint على أول سطر في الـ Controller Method
2. حط Breakpoint على السطر اللي بيرجع Response
3. حط Breakpoint في الـ Service Method

### الخطوة 4: اختبار بـ HTTP Files
**إنشاء ملف:** `test-requests.http`

```http
### 1. Register Patient
POST https://localhost:7001/api/auth/register/patient
Content-Type: application/json

{
  "email": "test@patient.com",
  "password": "Test123!@#",
  "firstName": "Ahmed",
  "lastName": "Mohamed",
  "phoneNumber": "01234567890",
  "dateOfBirth": "1990-01-01"
}

### 2. Login
POST https://localhost:7001/api/auth/login
Content-Type: application/json

{
  "email": "test@patient.com",
  "password": "Test123!@#"
}

### 3. Get Current User (يحتاج Token)
GET https://localhost:7001/api/auth/me
Authorization: Bearer {{accessToken}}
```

### الخطوة 5: فحص الـ Logs
```csharp
// أضف Logging في الـ Controller
_logger.LogInformation("RegisterPatient called with email: {Email}", dto.Email);
_logger.LogError("Registration failed: {Error}", result.Message);
```

### الخطوة 6: استخدام Postman
1. **إنشاء Collection** لكل Controller
2. **حفظ Environment Variables** (baseUrl, accessToken)
3. **عمل Tests** لكل Request

---

## ✅ Checklist للمراجعة {#checklist}

### للـ Controller ككل
- [ ] الـ Route صحيح (`[Route("api/[controller]")]`)
- [ ] الـ ApiController Attribute موجود
- [ ] الـ Dependency Injection شغال (Constructor)
- [ ] الـ Logger موجود ومستخدم

### لكل Endpoint
- [ ] الـ HTTP Method صحيح (GET, POST, PUT, DELETE)
- [ ] الـ Route Template صحيح
- [ ] الـ Authorization Attribute موجود لو محتاج
- [ ] الـ ProducesResponseType موجود لكل Status Code
- [ ] الـ ModelState Validation موجود لو في DTO
- [ ] الـ Try-Catch موجود للـ Exceptions
- [ ] الـ Status Codes صحيحة (200, 201, 400, 404, etc.)
- [ ] الـ Response Type واضح ومفيد

### للـ Error Handling
- [ ] الـ 400 Bad Request للـ Validation Errors
- [ ] الـ 401 Unauthorized للـ Authentication
- [ ] الـ 403 Forbidden للـ Authorization
- [ ] الـ 404 Not Found للـ Missing Resources
- [ ] الـ 500 Internal Server Error للـ Unexpected Errors
- [ ] الـ Error Messages واضحة ومفيدة

### للـ Security
- [ ] الـ Passwords بتتعمل Hash
- [ ] الـ JWT Tokens بتتولد صح
- [ ] الـ Refresh Tokens بتتخزن آمن
- [ ] الـ Sensitive Data مش بترجع في Response
- [ ] الـ CORS مظبوط
- [ ] الـ Rate Limiting شغال

### للـ Performance
- [ ] مفيش N+1 Query Problem
- [ ] الـ Pagination مستخدمة للـ Large Lists
- [ ] الـ Async/Await مستخدم صح
- [ ] الـ Database Indexes موجودة

---

## 🎯 الخطوات التالية

1. **ابدأ بـ AuthController** - الأهم والأخطر
2. **اختبر كل Endpoint** واحد واحد
3. **سجل الـ Issues** اللي تلاقيها
4. **اعمل Fix** للمشاكل
5. **اعمل Re-test** بعد الـ Fix
6. **انتقل للـ Controller التاني**

---

**ملحوظة:** الملف ده مرجع شامل، استخدمه جنب الكود الفعلي عشان تفهم كل حاجة صح! 🚀
