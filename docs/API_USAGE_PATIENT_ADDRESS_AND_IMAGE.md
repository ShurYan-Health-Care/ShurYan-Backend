# إدارة العنوان والصورة الشخصية للمريض - API Documentation

## نظرة عامة

هذا الدليل يشرح كيفية استخدام الـ endpoints الخاصة بـ:
1. **إدارة العنوان** (Address Management)
2. **إدارة الصورة الشخصية** (Profile Image Management)

كل الـ endpoints دي بتشتغل بنظام **Partial Update** - يعني بتحدث بس الحاجات اللي انت بعتها.

---

# 📍 إدارة العنوان (Address Management)

## 1. الحصول على العنوان - GET

### Endpoint
```
GET /api/patients/me/address
```

### Authentication
- **مطلوب**: Bearer Token (JWT)

### Response Examples

#### Success - عنوان موجود (200 OK)
```json
{
  "success": true,
  "message": "تم جلب العنوان بنجاح",
  "data": {
    "id": "guid",
    "street": "شارع الجامعة",
    "city": "القاهرة",
    "governorate": 1,
    "buildingNumber": "123",
    "latitude": 30.0444,
    "longitude": 31.2357,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-15T00:00:00Z"
  },
  "statusCode": 200
}
```

#### Success - مفيش عنوان (200 OK)
```json
{
  "success": true,
  "message": "لا يوجد عنوان مسجل",
  "data": null,
  "statusCode": 200
}
```

#### Error - Unauthorized (401)
```json
{
  "success": false,
  "message": "غير مصرح لك بالوصول",
  "statusCode": 401
}
```

---

## 2. تحديث/إنشاء العنوان - PUT

### Endpoint
```
PUT /api/patients/me/address
```

### Authentication
- **مطلوب**: Bearer Token (JWT)

### Content-Type
```
Content-Type: application/json
```

### Request Body (كل الـ Fields اختيارية)

```json
{
  "street": "string (optional)",
  "city": "string (optional)",
  "governorate": "number (optional)",
  "buildingNumber": "string (optional)",
  "latitude": "number (optional)",
  "longitude": "number (optional)"
}
```

### Validation Rules

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `street` | string | No | اسم الشارع |
| `city` | string | No | اسم المدينة |
| `governorate` | enum | No | المحافظة (Cairo=1, Giza=2, etc.) |
| `buildingNumber` | string | No | رقم المبنى |
| `latitude` | double | No | خط العرض |
| `longitude` | double | No | خط الطول |

---

### أمثلة عملية

#### مثال 1: إنشاء عنوان جديد (كل البيانات)

**Request:**
```json
PUT /api/patients/me/address
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "street": "شارع الجامعة",
  "city": "القاهرة",
  "governorate": 1,
  "buildingNumber": "123",
  "latitude": 30.0444,
  "longitude": 31.2357
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "تم إنشاء العنوان بنجاح",
  "data": {
    "id": "new-guid",
    "street": "شارع الجامعة",
    "city": "القاهرة",
    "governorate": 1,
    "buildingNumber": "123",
    "latitude": 30.0444,
    "longitude": 31.2357,
    "createdAt": "2024-01-15T10:30:00Z"
  },
  "statusCode": 200
}
```

---

#### مثال 2: تحديث الشارع بس (Partial Update)

**Request:**
```json
PUT /api/patients/me/address
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "street": "شارع التحرير"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "تم تحديث العنوان بنجاح",
  "data": {
    "id": "existing-guid",
    "street": "شارع التحرير",  // تم التحديث
    "city": "القاهرة",  // لم يتغير
    "governorate": 1,  // لم يتغير
    "buildingNumber": "123",  // لم يتغير
    "latitude": 30.0444,  // لم يتغير
    "longitude": 31.2357,  // لم يتغير
    "updatedAt": "2024-01-15T11:00:00Z"
  },
  "statusCode": 200
}
```

---

#### مثال 3: تحديث الموقع الجغرافي بس

**Request:**
```json
PUT /api/patients/me/address
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "latitude": 30.0555,
  "longitude": 31.2468
}
```

---

### Response Codes

| Status Code | Description |
|------------|-------------|
| **200 OK** | تم الإنشاء/التحديث بنجاح |
| **400 Bad Request** | بيانات غير صحيحة |
| **401 Unauthorized** | مفيش token أو token غير صحيح |
| **404 Not Found** | المريض غير موجود |
| **500 Internal Server Error** | خطأ في السيرفر |

---

# 🖼️ إدارة الصورة الشخصية (Profile Image Management)

## 1. تحديث الصورة الشخصية - PUT

### Endpoint
```
PUT /api/patients/me/profile-image
```

### Authentication
- **مطلوب**: Bearer Token (JWT)

### Content-Type
```
Content-Type: multipart/form-data
```

### Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `profileImage` | IFormFile | Yes | الصورة الجديدة |

### Supported Image Formats
- JPG/JPEG
- PNG
- GIF
- WebP

### Max File Size
- 5 MB (يمكن تغييرها في الإعدادات)

---

### كيفية العمل:

1. **يجلب بيانات المريض** للتحقق من وجود صورة قديمة
2. **يحذف الصورة القديمة من Cloudinary** (لو موجودة)
3. **يرفع الصورة الجديدة على Cloudinary**
4. **يحدث الـ URL في قاعدة البيانات**

---

### مثال عملي

**Request (Postman/Insomnia):**
```
PUT /api/patients/me/profile-image
Authorization: Bearer {your-token}
Content-Type: multipart/form-data

Form Data:
- profileImage: [select file]
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "تم تحديث الصورة الشخصية بنجاح",
  "data": {
    "patientId": "guid",
    "profileImageUrl": "https://res.cloudinary.com/your-cloud/image/upload/v1234567890/patients/guid.jpg"
  },
  "statusCode": 200
}
```

---

### Error Responses

#### 400 Bad Request - Invalid File
```json
{
  "success": false,
  "message": "بيانات غير صحيحة",
  "errors": [
    "The ProfileImage field is required.",
    "Invalid file format. Only JPG, PNG, GIF, and WebP are allowed."
  ],
  "statusCode": 400
}
```

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "غير مصرح لك بالوصول",
  "statusCode": 401
}
```

#### 404 Not Found
```json
{
  "success": false,
  "message": "المريض غير موجود",
  "statusCode": 404
}
```

---

## 2. حذف الصورة الشخصية - DELETE

### Endpoint
```
DELETE /api/patients/me/profile-image
```

### Authentication
- **مطلوب**: Bearer Token (JWT)

### كيفية العمل:

1. **يجلب بيانات المريض** للتحقق من وجود صورة
2. **يحذف الصورة من Cloudinary**
3. **يحذف الـ URL من قاعدة البيانات** (يخليه null)

---

### مثال عملي

**Request:**
```
DELETE /api/patients/me/profile-image
Authorization: Bearer {your-token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "تم حذف الصورة الشخصية بنجاح",
  "data": {
    "patientId": "guid"
  },
  "statusCode": 200
}
```

---

### Response Codes

| Status Code | Description |
|------------|-------------|
| **200 OK** | تم الحذف بنجاح أو مفيش صورة أصلاً |
| **401 Unauthorized** | مفيش token أو token غير صحيح |
| **404 Not Found** | المريض غير موجود |
| **500 Internal Server Error** | خطأ في السيرفر |

---

### حالات خاصة

#### لو مفيش صورة أصلاً (200 OK)
```json
{
  "success": true,
  "message": "لا توجد صورة شخصية لحذفها",
  "data": {
    "patientId": "guid"
  },
  "statusCode": 200
}
```

---

# 💡 ملاحظات مهمة

## Partial Update Behavior

### للعنوان:
- **لو بعتت field فاضي أو null:** مش هيتحدث
- **لو مبعتش field خالص:** هيفضل زي ما هو
- **لو العنوان مش موجود:** هيتعمل create تلقائياً

### للصورة الشخصية:
- **PUT:** بيحذف القديمة ويرفع الجديدة
- **DELETE:** بيحذف الصورة من Cloudinary والـ database

---

## Cloudinary Integration

### تحديث الصورة (PUT):
```
1. Get patient data
2. Delete old image from Cloudinary (if exists)
3. Upload new image to Cloudinary
4. Update URL in database
```

### حذف الصورة (DELETE):
```
1. Get patient data
2. Delete image from Cloudinary
3. Set ProfileImageUrl = null in database
```

---

## Error Handling

### لو فشل حذف الصورة من Cloudinary:
- الـ endpoint **مش هيفشل**
- هيكمل العملية ويحدث الـ database
- هيسجل warning في الـ logs

### لو فشل رفع الصورة على Cloudinary:
- الـ endpoint **هيفشل**
- مش هيحدث الـ database
- هيرجع error 500

---

# 🧪 Testing Examples

## Testing with cURL

### GET Address
```bash
curl -X GET "https://your-api.com/api/patients/me/address" \
  -H "Authorization: Bearer your-token"
```

### PUT Address
```bash
curl -X PUT "https://your-api.com/api/patients/me/address" \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{
    "street": "شارع الجامعة",
    "city": "القاهرة"
  }'
```

### PUT Profile Image
```bash
curl -X PUT "https://your-api.com/api/patients/me/profile-image" \
  -H "Authorization: Bearer your-token" \
  -F "profileImage=@/path/to/image.jpg"
```

### DELETE Profile Image
```bash
curl -X DELETE "https://your-api.com/api/patients/me/profile-image" \
  -H "Authorization: Bearer your-token"
```

---

## Testing with JavaScript (Fetch)

### GET Address
```javascript
const getAddress = async () => {
  const response = await fetch('https://your-api.com/api/patients/me/address', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const result = await response.json();
  console.log(result);
};
```

### PUT Address
```javascript
const updateAddress = async () => {
  const response = await fetch('https://your-api.com/api/patients/me/address', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      street: 'شارع الجامعة',
      city: 'القاهرة'
    })
  });
  
  const result = await response.json();
  console.log(result);
};
```

### PUT Profile Image
```javascript
const updateProfileImage = async (file) => {
  const formData = new FormData();
  formData.append('profileImage', file);
  
  const response = await fetch('https://your-api.com/api/patients/me/profile-image', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  
  const result = await response.json();
  console.log(result);
};
```

### DELETE Profile Image
```javascript
const deleteProfileImage = async () => {
  const response = await fetch('https://your-api.com/api/patients/me/profile-image', {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const result = await response.json();
  console.log(result);
};
```

---

## Testing with C# (HttpClient)

### GET Address
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", yourToken);

var response = await client.GetAsync("https://your-api.com/api/patients/me/address");
var result = await response.Content.ReadFromJsonAsync<ApiResponse<AddressResponse>>();
```

### PUT Address
```csharp
var updateRequest = new 
{
    street = "شارع الجامعة",
    city = "القاهرة"
};

var response = await client.PutAsJsonAsync(
    "https://your-api.com/api/patients/me/address", 
    updateRequest
);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<AddressResponse>>();
```

### PUT Profile Image
```csharp
using var content = new MultipartFormDataContent();
using var fileStream = File.OpenRead("path/to/image.jpg");
using var streamContent = new StreamContent(fileStream);
streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
content.Add(streamContent, "profileImage", "image.jpg");

var response = await client.PutAsync(
    "https://your-api.com/api/patients/me/profile-image", 
    content
);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
```

### DELETE Profile Image
```csharp
var response = await client.DeleteAsync(
    "https://your-api.com/api/patients/me/profile-image"
);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
```

---

# 📊 Summary Table

| Endpoint | Method | Purpose | Partial Update | Cloudinary |
|----------|--------|---------|----------------|------------|
| `/me/address` | GET | جلب العنوان | - | |
| `/me/address` | PUT | تحديث/إنشاء العنوان | | |
| `/me/profile-image` | PUT | تحديث الصورة | | Delete + Upload |
| `/me/profile-image` | DELETE | حذف الصورة | | Delete |

---

# 🎯 Best Practices

1. **للعنوان:**
   - بعت بس الحاجات اللي اتغيرت
   - استخدم الـ Governorate enum الصحيح
   - تأكد من الـ latitude/longitude صحيحين

2. **للصورة الشخصية:**
   - استخدم صور بجودة معقولة (مش أكبر من 5MB)
   - استخدم الـ formats المدعومة بس
   - احذف الصورة القديمة قبل ما ترفع جديدة (automatic)

3. **Security:**
   - احفظ الـ token بشكل آمن
   - استخدم HTTPS في الـ production
   - متشاركش الـ Cloudinary URLs publicly

4. **Error Handling:**
   - Handle كل الـ status codes
   - اعرض رسائل خطأ واضحة للـ user
   - Log الـ errors للـ debugging

---

**تم التحديث:** نوفمبر 2025  
**الإصدار:** 2.0
