# تحديث البيانات الشخصية للمريض - Partial Update

## نظرة عامة

الـ endpoint ده بيسمح للمريض إنه يحدث بياناته الشخصية بنظام **Partial Update**، يعني:
- **مش لازم تبعت كل الـ fields** - بعت بس الحاجات اللي عايز تعدلها
- **الحاجات اللي مبعتهاش هتفضل زي ما هي** - مش هتتمسح أو تبقى null
- **مرن جداً** - ممكن تحدث حاجة واحدة أو أكتر في نفس الوقت

---

## Endpoint Details

### URL
```
PUT /api/patients/me/profile
```

### Authentication
- **مطلوب**: Bearer Token (JWT)
- المريض بيحدث بياناته الشخصية بس

### Content-Type
```
Content-Type: application/json
```

---

## Request Body (كل الـ Fields اختيارية)

```json
{
  "firstName": "string (optional)",
  "lastName": "string (optional)",
  "phoneNumber": "string (optional)",
  "birthDate": "datetime (optional)",
  "gender": "Male/Female (optional)"
}
```

### Validation Rules

| Field | Type | Min Length | Max Length | Notes |
|-------|------|-----------|-----------|-------|
| `firstName` | string | 2 | 50 | اختياري |
| `lastName` | string | 2 | 50 | اختياري |
| `phoneNumber` | string | 10 | 20 | لازم يكون رقم تليفون صحيح |
| `birthDate` | DateTime | - | - | لازم يكون في الماضي |
| `gender` | enum | - | - | Male = 0, Female = 1 |

---

## أمثلة عملية

### مثال 1: تحديث الاسم الأول بس

**Request:**
```json
PUT /api/patients/me/profile
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "firstName": "أحمد"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "تم تحديث المعلومات بنجاح",
  "data": {
    "id": "guid",
    "firstName": "أحمد",
    "lastName": "محمد",  // لم يتغير
    "email": "ahmed@example.com",
    "phoneNumber": "01234567890",  // لم يتغير
    "birthDate": "1990-01-01",  // لم يتغير
    "gender": 0,  // لم يتغير
    ...
  },
  "statusCode": 200
}
```

---

### مثال 2: تحديث الاسم والتليفون

**Request:**
```json
PUT /api/patients/me/profile
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "firstName": "أحمد",
  "phoneNumber": "01098765432"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "تم تحديث المعلومات بنجاح",
  "data": {
    "id": "guid",
    "firstName": "أحمد",  // تم التحديث
    "lastName": "محمد",  // لم يتغير
    "phoneNumber": "01098765432",  // تم التحديث
    "phoneNumberConfirmed": false,  // تم reset عشان الرقم اتغير
    ...
  },
  "statusCode": 200
}
```

**ملاحظة مهمة:** لو غيرت الـ `phoneNumber`, الـ `phoneNumberConfirmed` هيرجع `false` تلقائياً عشان لازم تأكد الرقم الجديد.

---

### مثال 3: تحديث كل البيانات

**Request:**
```json
PUT /api/patients/me/profile
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "firstName": "أحمد",
  "lastName": "علي",
  "phoneNumber": "01098765432",
  "birthDate": "1995-05-15",
  "gender": 0
}
```

---

### مثال 4: تحديث تاريخ الميلاد والنوع بس

**Request:**
```json
PUT /api/patients/me/profile
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "birthDate": "1992-03-20",
  "gender": 1
}
```

---

## Response Codes

| Status Code | Description |
|------------|-------------|
| **200 OK** | تم التحديث بنجاح |
| **400 Bad Request** | بيانات غير صحيحة (validation error) |
| **401 Unauthorized** | مفيش token أو token غير صحيح |
| **500 Internal Server Error** | خطأ في السيرفر |

---

## Error Responses

### 400 Bad Request - Validation Error

```json
{
  "success": false,
  "message": "بيانات غير صحيحة",
  "errors": [
    "الاسم الأول يجب أن يكون بين 2-50 حرف",
    "صيغة رقم الهاتف غير صحيحة"
  ],
  "statusCode": 400
}
```

### 400 Bad Request - Birth Date in Future

```json
{
  "success": false,
  "message": "Birth date cannot be in the future",
  "statusCode": 400
}
```

### 401 Unauthorized

```json
{
  "success": false,
  "message": "غير مصرح لك بالوصول",
  "statusCode": 401
}
```

---

## ملاحظات مهمة

### 1. الصورة الشخصية والعنوان ليهم endpoints منفصلة

- **تحديث الصورة الشخصية:**
  ```
  PUT /api/patients/me/profile-image
  Content-Type: multipart/form-data
  ```

- **تحديث العنوان:**
  ```
  PUT /api/patients/me/address
  Content-Type: application/json
  ```

### 2. Partial Update Behavior

- **لو بعتت field فاضي أو null:** مش هيتحدث
- **لو مبعتش field خالص:** هيفضل زي ما هو
- **لو مفيش أي تغييرات:** هيرجع البيانات الحالية من غير ما يعمل save

### 3. Phone Number Confirmation

لو غيرت الـ `phoneNumber`:
- الـ `phoneNumberConfirmed` هيرجع `false` تلقائياً
- لازم المريض يأكد الرقم الجديد

### 4. Birth Date Validation

- لازم يكون في الماضي
- مينفعش يكون في المستقبل

---

## Testing with Postman/Insomnia

### 1. احصل على Token أولاً
```
POST /api/auth/login
{
  "email": "patient@example.com",
  "password": "your-password"
}
```

### 2. استخدم الـ Token في الـ Header
```
Authorization: Bearer {your-token}
```

### 3. ابعت الـ Request
```
PUT /api/patients/me/profile
Content-Type: application/json

{
  "firstName": "أحمد"
}
```

---

## Code Example (C# - HttpClient)

```csharp
using System.Net.Http;
using System.Net.Http.Json;

var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", yourToken);

var updateRequest = new 
{
    firstName = "أحمد",
    phoneNumber = "01234567890"
};

var response = await client.PutAsJsonAsync(
    "https://your-api.com/api/patients/me/profile", 
    updateRequest
);

if (response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PatientResponse>>();
    Console.WriteLine($"Success: {result.Message}");
}
```

---

## Code Example (JavaScript - Fetch)

```javascript
const token = 'your-jwt-token';

const updateProfile = async () => {
  try {
    const response = await fetch('https://your-api.com/api/patients/me/profile', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        firstName: 'أحمد',
        phoneNumber: '01234567890'
      })
    });

    const result = await response.json();
    
    if (result.success) {
      console.log('تم التحديث بنجاح:', result.data);
    } else {
      console.error('خطأ:', result.message);
    }
  } catch (error) {
    console.error('خطأ في الاتصال:', error);
  }
};

updateProfile();
```

---

## الفرق بين الـ Endpoint القديم والجديد

| Feature | القديم | الجديد |
|---------|--------|--------|
| **Content-Type** | `multipart/form-data` | `application/json` |
| **Partial Update** | ✅ موجود | ✅ محسّن |
| **Profile Image** | ✅ في نفس الـ endpoint | ❌ endpoint منفصل |
| **Address** | ✅ في نفس الـ endpoint | ❌ endpoint منفصل |
| **Validation** | ✅ أساسي | ✅ محسّن |
| **Error Handling** | ✅ أساسي | ✅ محسّن |
| **Performance** | ⚠️ بطيء (بسبب الصورة) | ✅ سريع |
| **Change Tracking** | ❌ مفيش | ✅ موجود |

---

## Best Practices

1. **بعت بس الحاجات اللي اتغيرت** - مش كل الـ fields
2. **استخدم الـ endpoints المنفصلة** للصورة والعنوان
3. **Handle الـ errors صح** - خصوصاً الـ validation errors
4. **احفظ الـ token** بشكل آمن
5. **استخدم HTTPS** في الـ production

---

## Summary

الـ endpoint الجديد ده:
- ✅ **أسرع** - مفيش upload للصور
- ✅ **أبسط** - JSON بس
- ✅ **أكثر مرونة** - Partial Update حقيقي
- ✅ **أفضل validation** - رسائل خطأ واضحة
- ✅ **Better separation of concerns** - كل حاجة في endpoint منفصل

---

**تم التحديث:** نوفمبر 2025
