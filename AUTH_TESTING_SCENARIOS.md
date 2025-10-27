# 🧪 سيناريوهات اختبار AuthController - شاملة

## 📋 المحتويات
1. [RegisterPatient - سيناريوهات الاختبار](#register-patient)
2. [VerifyEmail - سيناريوهات الاختبار](#verify-email)
3. [Login - سيناريوهات الاختبار](#login)
4. [Password Management - سيناريوهات الاختبار](#password)
5. [Token Management - سيناريوهات الاختبار](#tokens)

---

## 🎯 RegisterPatient - سيناريوهات الاختبار {#register-patient}

### ✅ Success Cases (المفروض تنجح)

#### Test 1: تسجيل مريض جديد بكل البيانات صحيحة
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Ahmed",
  "lastName": "Mohamed",
  "email": "ahmed.mohamed@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1990-01-15"
}
```

**Expected Response:**
- Status Code: `201 Created`
- Response Body:
```json
{
  "isSuccess": true,
  "message": "Registration successful! Please check your email for the verification code.",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "refresh-token-here",
    "user": {
      "id": "guid-here",
      "email": "ahmed.mohamed@test.com",
      "firstName": "Ahmed",
      "lastName": "Mohamed",
      "userType": "Patient",
      "isEmailVerified": false
    }
  },
  "statusCode": 201
}
```

**Validation Points:**
- ✅ User created in database
- ✅ Password hashed (not stored as plain text)
- ✅ OTP generated and sent to email
- ✅ JWT Token generated
- ✅ Refresh Token generated and stored
- ✅ User assigned "Patient" role
- ✅ EmailConfirmed = false initially

---

### ❌ Failure Cases (المفروض تفشل)

#### Test 2: Email موجود فعلاً (Duplicate Email)
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Mohamed",
  "lastName": "Ali",
  "email": "ahmed.mohamed@test.com",  // نفس الإيميل من Test 1
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01098765432",
  "dateOfBirth": "1992-05-20"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Response Body:
```json
{
  "isSuccess": false,
  "message": "Email already registered",
  "errors": ["A user with this email already exists"],
  "statusCode": 400
}
```

**Validation Points:**
- ✅ No new user created
- ✅ Clear error message
- ✅ No OTP sent

---

#### Test 3: Password ضعيف (مفيش حروف كبيرة)
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Khaled",
  "lastName": "Hassan",
  "email": "khaled.hassan@test.com",
  "password": "test@123456",  // كله حروف صغيرة
  "confirmPassword": "test@123456",
  "phoneNumber": "01112345678",
  "dateOfBirth": "1988-03-10"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Response Body:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Password": [
      "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character"
    ]
  }
}
```

**Validation Points:**
- ✅ ModelState validation triggered
- ✅ No user created
- ✅ Clear validation error

---

#### Test 4: Password مفيهوش رموز خاصة
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Sara",
  "lastName": "Ahmed",
  "email": "sara.ahmed@test.com",
  "password": "Test12345678",  // مفيش رموز خاصة
  "confirmPassword": "Test12345678",
  "phoneNumber": "01212345678",
  "dateOfBirth": "1995-07-22"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: Password must contain special character

---

#### Test 5: Password و ConfirmPassword مش متطابقين
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Omar",
  "lastName": "Mahmoud",
  "email": "omar.mahmoud@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@654321",  // مختلف
  "phoneNumber": "01512345678",
  "dateOfBirth": "1993-11-05"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "Password and confirmation password do not match"

---

#### Test 6: Email بصيغة غلط
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Fatma",
  "lastName": "Ibrahim",
  "email": "fatma.ibrahim.test.com",  // مفيش @
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01612345678",
  "dateOfBirth": "1991-09-18"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "Invalid email format"

---

#### Test 7: PhoneNumber بصيغة غلط (مش رقم مصري)
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Youssef",
  "lastName": "Khaled",
  "email": "youssef.khaled@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "05512345678",  // مش بيبدأ بـ 01
  "dateOfBirth": "1994-04-12"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "Invalid Egyptian phone number format"

---

#### Test 8: PhoneNumber أقل من 11 رقم
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Nour",
  "lastName": "Samir",
  "email": "nour.samir@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "0101234567",  // 10 أرقام بس
  "dateOfBirth": "1996-06-25"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "Invalid Egyptian phone number format"

---

#### Test 9: FirstName أقل من 2 حروف
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "A",  // حرف واحد بس
  "lastName": "Mohamed",
  "email": "a.mohamed@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1990-01-01"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "First name must be between 2-50 characters"

---

#### Test 10: FirstName أكتر من 50 حرف
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "AhmedMohamedKhaledHassanIbrahimMahmoudOmarYoussefSamir",  // 55 حرف
  "lastName": "Ali",
  "email": "long.name@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1990-01-01"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "First name must be between 2-50 characters"

---

#### Test 11: Required Fields مش موجودة (FirstName مفقود)
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "lastName": "Mohamed",
  "email": "test@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1990-01-01"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "First name is required"

---

#### Test 12: DateOfBirth في المستقبل
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Future",
  "lastName": "Baby",
  "email": "future.baby@test.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "01012345678",
  "dateOfBirth": "2030-01-01"  // في المستقبل
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "Invalid date of birth" (لو في Custom Validation)

---

#### Test 13: Password أقل من 8 حروف
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": "Short",
  "lastName": "Pass",
  "email": "short.pass@test.com",
  "password": "Test@12",  // 7 حروف بس
  "confirmPassword": "Test@12",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1990-01-01"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Validation Error: "Password must be at least 8 characters"

---

#### Test 14: Empty Request Body
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Multiple validation errors for all required fields

---

#### Test 15: Null Values
```http
POST http://localhost:5117/api/auth/register/patient
Content-Type: application/json

{
  "firstName": null,
  "lastName": null,
  "email": null,
  "password": null,
  "confirmPassword": null,
  "phoneNumber": null,
  "dateOfBirth": null
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Multiple validation errors

---

## 📧 VerifyEmail - سيناريوهات الاختبار {#verify-email}

### ✅ Success Case

#### Test 16: تأكيد Email بـ OTP صحيح
```http
POST http://localhost:5117/api/auth/verify-email
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com",
  "otp": "123456"  // الـ OTP اللي اتبعت في الإيميل
}
```

**Expected Response:**
- Status Code: `200 OK`
- Response Body:
```json
{
  "isSuccess": true,
  "message": "Email verified successfully",
  "data": true,
  "statusCode": 200
}
```

**Validation Points:**
- ✅ EmailConfirmed = true في Database
- ✅ OTP deleted from database (used)
- ✅ User can now access full features

---

### ❌ Failure Cases

#### Test 17: OTP غلط
```http
POST http://localhost:5117/api/auth/verify-email
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com",
  "otp": "999999"  // OTP غلط
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Response Body:
```json
{
  "isSuccess": false,
  "message": "Invalid or expired OTP",
  "errors": ["The OTP code is incorrect or has expired"],
  "statusCode": 400
}
```

---

#### Test 18: OTP منتهي (Expired)
```http
POST http://localhost:5117/api/auth/verify-email
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com",
  "otp": "123456"  // OTP قديم (مر عليه أكتر من 10 دقائق مثلاً)
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Message: "Invalid or expired OTP"

---

#### Test 19: Email مش موجود
```http
POST http://localhost:5117/api/auth/verify-email
Content-Type: application/json

{
  "email": "notfound@test.com",
  "otp": "123456"
}
```

**Expected Response:**
- Status Code: `400 Bad Request` أو `404 Not Found`
- Message: "User not found"

---

#### Test 20: Email متأكد فعلاً
```http
POST http://localhost:5117/api/auth/verify-email
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com",  // Email متأكد من قبل
  "otp": "123456"
}
```

**Expected Response:**
- Status Code: `400 Bad Request`
- Message: "Email is already verified"

---

## 🔐 Login - سيناريوهات الاختبار {#login}

### ✅ Success Case

#### Test 21: تسجيل دخول بنجاح
```http
POST http://localhost:5117/api/auth/login
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com",
  "password": "Test@123456"
}
```

**Expected Response:**
- Status Code: `200 OK`
- Response Body:
```json
{
  "isSuccess": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "refresh-token-here",
    "user": {
      "id": "guid-here",
      "email": "ahmed.mohamed@test.com",
      "firstName": "Ahmed",
      "lastName": "Mohamed",
      "userType": "Patient",
      "isEmailVerified": true
    }
  },
  "statusCode": 200
}
```

**Validation Points:**
- ✅ JWT Token valid
- ✅ Refresh Token stored in database
- ✅ Login attempt logged with IP address

---

### ❌ Failure Cases

#### Test 22: Password غلط
```http
POST http://localhost:5117/api/auth/login
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com",
  "password": "WrongPassword123"
}
```

**Expected Response:**
- Status Code: `401 Unauthorized`
- Message: "Invalid email or password"

---

#### Test 23: Email مش موجود
```http
POST http://localhost:5117/api/auth/login
Content-Type: application/json

{
  "email": "notfound@test.com",
  "password": "Test@123456"
}
```

**Expected Response:**
- Status Code: `401 Unauthorized`
- Message: "Invalid email or password"

---

#### Test 24: Email مش متأكد
```http
POST http://localhost:5117/api/auth/login
Content-Type: application/json

{
  "email": "unverified@test.com",  // Email مش متأكد
  "password": "Test@123456"
}
```

**Expected Response:**
- Status Code: `403 Forbidden`
- Message: "Please verify your email before logging in"

---

## 🔑 Password Management - سيناريوهات الاختبار {#password}

### Forgot Password

#### Test 25: طلب إعادة تعيين كلمة السر
```http
POST http://localhost:5117/api/auth/forgot-password
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com"
}
```

**Expected Response:**
- Status Code: `200 OK`
- Message: "Password reset OTP sent to your email"

**Validation Points:**
- ✅ OTP generated and sent
- ✅ OTP stored in database with expiry time

---

### Reset Password

#### Test 26: إعادة تعيين كلمة السر بنجاح
```http
POST http://localhost:5117/api/auth/reset-password
Content-Type: application/json

{
  "email": "ahmed.mohamed@test.com",
  "otp": "654321",
  "newPassword": "NewPass@123456"
}
```

**Expected Response:**
- Status Code: `200 OK`
- Message: "Password reset successfully"

**Validation Points:**
- ✅ Password updated in database
- ✅ Password hashed
- ✅ OTP deleted (used)
- ✅ All refresh tokens invalidated

---

### Change Password

#### Test 27: تغيير كلمة السر (يحتاج Auth)
```http
POST http://localhost:5117/api/auth/change-password
Content-Type: application/json
Authorization: Bearer eyJhbGc...

{
  "oldPassword": "Test@123456",
  "newPassword": "NewPass@654321"
}
```

**Expected Response:**
- Status Code: `200 OK`
- Message: "Password changed successfully"

---

## 🎫 Token Management - سيناريوهات الاختبار {#tokens}

### Refresh Token

#### Test 28: تجديد Access Token
```http
POST http://localhost:5117/api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "refresh-token-here"
}
```

**Expected Response:**
- Status Code: `200 OK`
- New Access Token returned

---

### Logout

#### Test 29: تسجيل خروج
```http
POST http://localhost:5117/api/auth/logout
Content-Type: application/json
Authorization: Bearer eyJhbGc...

{
  "refreshToken": "refresh-token-here"
}
```

**Expected Response:**
- Status Code: `200 OK`
- Message: "Logged out successfully"

**Validation Points:**
- ✅ Refresh Token revoked in database

---

## 📊 ملخص سيناريوهات الاختبار

| Category | Success Cases | Failure Cases | Total |
|----------|---------------|---------------|-------|
| Registration | 1 | 14 | 15 |
| Email Verification | 1 | 4 | 5 |
| Login | 1 | 3 | 4 |
| Password Management | 3 | - | 3 |
| Token Management | 2 | - | 2 |
| **Total** | **8** | **21** | **29** |

---

## 🎯 الخطوات التالية

1. **نفذ كل الـ Test Cases** واحد واحد
2. **سجل النتائج** في جدول
3. **لو في Test فشل**، حط Breakpoint وتتبع الكود
4. **اعمل Fix** للمشاكل
5. **أعد الاختبار** بعد الـ Fix

**بالتوفيق! 🚀**
