# 📚 دليل الفرونت الشامل لـ Doctor Documents APIs

> **الهدف**: دليل كامل ومفصل لكل الـ endpoints الخاصة بإدارة مستندات الدكاترة في منصة شُريان الطبية

---

## 📑 فهرس المحتويات

1. [معلومات أساسية](#معلومات-أساسية)
2. [أنواع المستندات (Document Types)](#أنواع-المستندات)
3. [حالات المستندات (Document Status)](#حالات-المستندات)
4. [نماذج البيانات (Data Models)](#نماذج-البيانات)
5. [Endpoints - المستندات العامة](#endpoints---المستندات-العامة)
6. [Endpoints - المستندات المتخصصة](#endpoints---المستندات-المتخصصة)
7. [أمثلة عملية للتكامل](#أمثلة-عملية-للتكامل)
8. [معالجة الأخطاء](#معالجة-الأخطاء)
9. [Best Practices](#best-practices)

---

## 🎯 معلومات أساسية

### Base URL
```
https://your-api-domain.com/api/Doctors
```

### Authentication
**كل الـ endpoints محتاجة Authentication** عن طريق JWT Token:

```javascript
headers: {
  'Authorization': 'Bearer YOUR_JWT_TOKEN',
  'Content-Type': 'multipart/form-data' // للـ upload endpoints
}
```

### Authorization Roles
- **Doctor**: الدكتور نفسه (يقدر يرفع ويعدل مستنداته)
- **Admin**: الأدمن (يقدر يوافق/يرفض/يدير كل المستندات)

### Response Format
كل الـ responses بتيجي في الـ format ده:

```typescript
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
  statusCode: number;
}
```

---

## 📄 أنواع المستندات

### DoctorDocumentType Enum

المستندات مقسمة لنوعين:

#### 1️⃣ **مستندات إجبارية (Required Documents)**
دي مهمة عشان التحقق من هوية الدكتور والموافقة عليه:

```typescript
enum RequiredDocumentTypes {
  NationalId = 0,                    // "البطاقة الشخصية"
  MedicalPracticeLicense = 1,        // "رخصة مزاولة المهنة"
  SyndicateMembershipCard = 2,       // "عضوية النقابة"
  MedicalGraduationCertificate = 3,  // "شهادة التخرج من كلية الطب"
  SpecialtyCertificate = 4           // "شهادة التخصص"
}
```

#### 2️⃣ **مستندات اختيارية (Optional Documents)**
دي بتعزز البروفايل المهني للدكتور:

```typescript
enum OptionalDocumentTypes {
  AdditionalCertificates = 5,     // "شهادات مهنية اضافية"
  AwardsAndRecognitions = 6,      // "جوائز وتقديرات"
  PublishedResearch = 7,          // "الأبحاث المنشورة"
  ProfessionalMemberships = 8     // "العضويات المهنية"
}
```

### 📌 ملاحظة مهمة
- الأرقام (0-8) دي هي اللي هتبعتها في الـ request
- الأسماء العربية هتيجي في الـ response في حقل `typeName`

---

## 🚦 حالات المستندات

### VerificationDocumentStatus Enum

كل مستند بيمر بدورة حياة (lifecycle) من 5 حالات:

```typescript
enum VerificationDocumentStatus {
  Draft = 0,      // "مسودة" - لسه مترفع بس مش متقدم للمراجعة
  Pending = 1,    // "في الانتظار" - متقدم للمراجعة ومستني الأدمن
  Approved = 2,   // "مقبول" - الأدمن وافق عليه
  Rejected = 3,   // "مرفوض" - الأدمن رفضه مع سبب
  Expired = 4     // "منتهي الصلاحية" - المستند خلص وقته
}
```

### 🔄 دورة حياة المستند

```
Draft ➞ Submit ➞ Pending ➞ Approve/Reject ➞ Approved/Rejected
                                        ↓
                                    (after time)
                                        ↓
                                     Expired
```

---

## 📦 نماذج البيانات

### 1. UploadDoctorDocumentRequest (Request Body)

**الاستخدام**: لما تيجي ترفع أو تعدل مستند

```typescript
interface UploadDoctorDocumentRequest {
  documentFile: File;           // الملف نفسه (PDF, JPG, PNG, etc.)
  type: DoctorDocumentType;     // نوع المستند (0-8)
}
```

**مثال FormData:**
```javascript
const formData = new FormData();
formData.append('documentFile', fileInput.files[0]);
formData.append('type', '0'); // NationalId
```

---

### 2. DoctorDocumentResponse (Response Data)

**الاستخدام**: ده اللي بييجي في الـ response من أي endpoint

```typescript
interface DoctorDocumentResponse {
  // من BaseAuditableDto
  id: string;                   // GUID - معرف المستند
  createdAt: string;            // ISO DateTime - تاريخ الإنشاء
  createdBy: string | null;     // GUID - اللي أنشأ المستند
  updatedAt: string | null;     // ISO DateTime - آخر تعديل
  updatedBy: string | null;     // GUID - اللي عدل المستند
  
  // خاص بالمستند
  documentUrl: string;          // رابط الملف المرفوع
  type: number;                 // نوع المستند (0-8)
  typeName: string;             // اسم النوع بالعربي
  status: number;               // حالة المستند (0-4)
  statusName: string;           // اسم الحالة بالعربي
  rejectionReason: string | null; // سبب الرفض (لو مرفوض)
  doctorId: string;             // GUID - معرف الدكتور
  doctorName: string;           // اسم الدكتور
}
```

**مثال Response:**
```json
{
  "success": true,
  "message": "Document uploaded successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createdAt": "2024-10-27T20:30:00Z",
    "createdBy": "1234-5678-9012",
    "updatedAt": null,
    "updatedBy": null,
    "documentUrl": "https://storage.com/docs/national-id.pdf",
    "type": 0,
    "typeName": "البطاقة الشخصية",
    "status": 0,
    "statusName": "مسودة",
    "rejectionReason": null,
    "doctorId": "1234-5678-9012",
    "doctorName": "د. أحمد محمد"
  },
  "errors": null,
  "statusCode": 201
}
```

---

## 🎯 Endpoints - المستندات المتخصصة

دي الـ endpoints اللي الفرونت هيستخدمها بشكل أساسي للتعامل مع المستندات المتخصصة.

---

### 1️⃣ رفع/تحديث مستند إجباري

**Upload or Update Required Document**

```
POST /api/Doctors/me/documents/required
```

**Authorization**: `Doctor` فقط  
**Content-Type**: `multipart/form-data`

**Request Body:**
```typescript
{
  documentFile: File,    // الملف
  type: 0-4             // نوع من المستندات الإجبارية فقط
}
```

**الأنواع المسموحة:**
- `0` = NationalId
- `1` = MedicalPracticeLicense
- `2` = SyndicateMembershipCard
- `3` = MedicalGraduationCertificate
- `4` = SpecialtyCertificate

**Response:**
```typescript
ApiResponse<DoctorDocumentResponse>
```

**Status Codes:**
- `200 OK` - تم الرفع/التحديث بنجاح
- `400 Bad Request` - بيانات غلط أو نوع مش من الإجبارية
- `401 Unauthorized` - مش مسجل دخول
- `500 Internal Server Error` - خطأ في السيرفر

**مثال JavaScript:**
```javascript
async function uploadRequiredDocument(file, documentType) {
  const formData = new FormData();
  formData.append('documentFile', file);
  formData.append('type', documentType.toString());

  const response = await fetch('/api/Doctors/me/documents/required', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  return await response.json();
}

// استخدام
const result = await uploadRequiredDocument(file, 0); // National ID
```

---

### 2️⃣ رفع شهادة جائزة أو تقدير

**Upload Award Certificate**

```
POST /api/Doctors/me/documents/awards
```

**Authorization**: `Doctor` فقط  
**Content-Type**: `multipart/form-data`

**Request Body:**
```typescript
{
  documentFile: File,
  type: 6              // لازم يكون AwardsAndRecognitions
}
```

**Response:**
```typescript
ApiResponse<DoctorDocumentResponse>
```

**Status Codes:**
- `201 Created` - تم الرفع بنجاح
- `400 Bad Request` - النوع مش AwardsAndRecognitions
- `401 Unauthorized` - مش مسجل دخول
- `500 Internal Server Error` - خطأ في السيرفر

**مثال JavaScript:**
```javascript
async function uploadAward(file) {
  const formData = new FormData();
  formData.append('documentFile', file);
  formData.append('type', '6'); // AwardsAndRecognitions

  const response = await fetch('/api/Doctors/me/documents/awards', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  return await response.json();
}
```

---

### 3️⃣ رفع بحث علمي

**Upload Research Paper**

```
POST /api/Doctors/me/documents/research
```

**Authorization**: `Doctor` فقط  
**Content-Type**: `multipart/form-data`

**Request Body:**
```typescript
{
  documentFile: File,
  type: 7              // لازم يكون PublishedResearch
}
```

**Response:**
```typescript
ApiResponse<DoctorDocumentResponse>
```

**Status Codes:**
- `201 Created` - تم الرفع بنجاح
- `400 Bad Request` - النوع مش PublishedResearch
- `401 Unauthorized` - مش مسجل دخول
- `500 Internal Server Error` - خطأ في السيرفر

**مثال JavaScript:**
```javascript
async function uploadResearch(file) {
  const formData = new FormData();
  formData.append('documentFile', file);
  formData.append('type', '7'); // PublishedResearch

  const response = await fetch('/api/Doctors/me/documents/research', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  return await response.json();
}
```

---

### 4️⃣ جلب المستندات الإجبارية

**Get Required Documents**

```
GET /api/Doctors/me/documents/required
```

**Authorization**: `Doctor` فقط

**Response:**
```typescript
ApiResponse<DoctorDocumentResponse[]>
```

**Status Codes:**
- `200 OK` - تم الجلب بنجاح
- `401 Unauthorized` - مش مسجل دخول
- `404 Not Found` - الدكتور مش موجود
- `500 Internal Server Error` - خطأ في السيرفر

**مثال JavaScript:**
```javascript
async function getRequiredDocuments() {
  const response = await fetch('/api/Doctors/me/documents/required', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  return await response.json();
}
```

---

### 5️⃣ جلب الأبحاث العلمية

**Get Research Papers**

```
GET /api/Doctors/me/documents/research
```

**Authorization**: `Doctor` فقط

**Response:**
```typescript
ApiResponse<DoctorDocumentResponse[]>
```

**Status Codes:**
- `200 OK` - تم الجلب بنجاح
- `401 Unauthorized` - مش مسجل دخول
- `404 Not Found` - الدكتور مش موجود
- `500 Internal Server Error` - خطأ في السيرفر

**مثال JavaScript:**
```javascript
async function getResearchPapers() {
  const response = await fetch('/api/Doctors/me/documents/research', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  return await response.json();
}
```

---

### 6️⃣ جلب الجوائز والشهادات

**Get Award Certificates**

```
GET /api/Doctors/me/documents/awards
```

**Authorization**: `Doctor` فقط

**Response:**
```typescript
ApiResponse<DoctorDocumentResponse[]>
```

**Status Codes:**
- `200 OK` - تم الجلب بنجاح
- `401 Unauthorized` - مش مسجل دخول
- `404 Not Found` - الدكتور مش موجود
- `500 Internal Server Error` - خطأ في السيرفر

**مثال JavaScript:**
```javascript
async function getAwardCertificates() {
  const response = await fetch('/api/Doctors/me/documents/awards', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  return await response.json();
}
```

---

## 💡 أمثلة عملية للتكامل

### مثال كامل: React Component لرفع المستندات

```jsx
import { useState } from 'react';

function DocumentUploader() {
  const [file, setFile] = useState(null);
  const [documentType, setDocumentType] = useState('0');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
    setError(null);
  };

  const handleUpload = async () => {
    if (!file) {
      setError('من فضلك اختر ملف');
      return;
    }

    setLoading(true);
    setError(null);
    setSuccess(false);

    try {
      const formData = new FormData();
      formData.append('documentFile', file);
      formData.append('type', documentType);

      const token = localStorage.getItem('authToken');
      
      const response = await fetch('/api/Doctors/me/documents/required', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });

      const result = await response.json();

      if (result.success) {
        setSuccess(true);
        setFile(null);
        // يمكنك هنا تحديث الـ state أو عرض رسالة نجاح
      } else {
        setError(result.message || 'حدث خطأ أثناء رفع المستند');
      }
    } catch (err) {
      setError('فشل الاتصال بالسيرفر');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="document-uploader">
      <h3>رفع مستند إجباري</h3>
      
      <select 
        value={documentType} 
        onChange={(e) => setDocumentType(e.target.value)}
        disabled={loading}
      >
        <option value="0">البطاقة الشخصية</option>
        <option value="1">رخصة مزاولة المهنة</option>
        <option value="2">عضوية النقابة</option>
        <option value="3">شهادة التخرج</option>
        <option value="4">شهادة التخصص</option>
      </select>

      <input 
        type="file" 
        onChange={handleFileChange}
        accept=".pdf,.jpg,.jpeg,.png"
        disabled={loading}
      />

      <button onClick={handleUpload} disabled={loading || !file}>
        {loading ? 'جاري الرفع...' : 'رفع المستند'}
      </button>

      {error && <div className="error">{error}</div>}
      {success && <div className="success">تم رفع المستند بنجاح!</div>}
    </div>
  );
}
```

---

### مثال: عرض قائمة المستندات

```jsx
import { useEffect, useState } from 'react';

function RequiredDocumentsList() {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDocuments();
  }, []);

  const fetchDocuments = async () => {
    try {
      const token = localStorage.getItem('authToken');
      
      const response = await fetch('/api/Doctors/me/documents/required', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      const result = await response.json();

      if (result.success) {
        setDocuments(result.data);
      }
    } catch (err) {
      console.error('Error fetching documents:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>جاري التحميل...</div>;

  return (
    <div className="documents-list">
      <h3>المستندات الإجبارية</h3>
      {documents.length === 0 ? (
        <p>لا توجد مستندات</p>
      ) : (
        <ul>
          {documents.map((doc) => (
            <li key={doc.id}>
              <div>
                <strong>{doc.typeName}</strong>
                <span className={`status status-${doc.status}`}>
                  {doc.statusName}
                </span>
              </div>
              <div>
                <a href={doc.documentUrl} target="_blank" rel="noopener noreferrer">
                  عرض المستند
                </a>
                <small>تم الرفع: {new Date(doc.createdAt).toLocaleDateString('ar-EG')}</small>
              </div>
              {doc.rejectionReason && (
                <div className="rejection-reason">
                  سبب الرفض: {doc.rejectionReason}
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
```

---

## ⚠️ معالجة الأخطاء

### الأخطاء الشائعة وحلولها

#### 1. خطأ 401 Unauthorized
```javascript
// المشكلة: التوكن مش موجود أو منتهي
// الحل:
if (response.status === 401) {
  // امسح التوكن القديم
  localStorage.removeItem('authToken');
  // وجه اليوزر لصفحة تسجيل الدخول
  window.location.href = '/login';
}
```

#### 2. خطأ 400 Bad Request
```javascript
// المشكلة: البيانات المرسلة غلط
// الحل: تحقق من:
// - نوع المستند صحيح (0-8)
// - الملف موجود
// - الملف بالامتداد الصحيح

const result = await response.json();
if (!result.success && result.errors) {
  // اعرض الأخطاء للمستخدم
  result.errors.forEach(error => {
    console.error(error);
  });
}
```

#### 3. خطأ 500 Internal Server Error
```javascript
// المشكلة: خطأ في السيرفر
// الحل: حاول تاني بعد شوية
if (response.status === 500) {
  setTimeout(() => {
    // أعد المحاولة
    retryUpload();
  }, 3000);
}
```

---

## ✅ Best Practices

### 1. التعامل مع الملفات الكبيرة

```javascript
// تحقق من حجم الملف قبل الرفع
const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

function validateFile(file) {
  if (file.size > MAX_FILE_SIZE) {
    throw new Error('حجم الملف كبير جداً. الحد الأقصى 10 ميجابايت');
  }
  
  const allowedTypes = ['application/pdf', 'image/jpeg', 'image/png'];
  if (!allowedTypes.includes(file.type)) {
    throw new Error('نوع الملف غير مدعوم. استخدم PDF أو JPG أو PNG');
  }
  
  return true;
}
```

### 2. Progress Indicator للرفع

```javascript
async function uploadWithProgress(file, documentType, onProgress) {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    const formData = new FormData();
    formData.append('documentFile', file);
    formData.append('type', documentType);

    xhr.upload.addEventListener('progress', (e) => {
      if (e.lengthComputable) {
        const percentComplete = (e.loaded / e.total) * 100;
        onProgress(percentComplete);
      }
    });

    xhr.addEventListener('load', () => {
      if (xhr.status === 200 || xhr.status === 201) {
        resolve(JSON.parse(xhr.responseText));
      } else {
        reject(new Error('Upload failed'));
      }
    });

    xhr.addEventListener('error', () => reject(new Error('Network error')));

    xhr.open('POST', '/api/Doctors/me/documents/required');
    xhr.setRequestHeader('Authorization', `Bearer ${token}`);
    xhr.send(formData);
  });
}
```

### 3. Caching الـ Documents List

```javascript
// استخدم React Query أو SWR للـ caching
import { useQuery } from '@tanstack/react-query';

function useRequiredDocuments() {
  return useQuery({
    queryKey: ['requiredDocuments'],
    queryFn: async () => {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/Doctors/me/documents/required', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      const result = await response.json();
      return result.data;
    },
    staleTime: 5 * 60 * 1000, // 5 دقائق
    cacheTime: 10 * 60 * 1000, // 10 دقائق
  });
}
```

### 4. Retry Logic

```javascript
async function uploadWithRetry(file, documentType, maxRetries = 3) {
  let lastError;
  
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await uploadDocument(file, documentType);
    } catch (error) {
      lastError = error;
      if (i < maxRetries - 1) {
        // انتظر قبل المحاولة التالية (exponential backoff)
        await new Promise(resolve => setTimeout(resolve, Math.pow(2, i) * 1000));
      }
    }
  }
  
  throw lastError;
}
```

### 5. TypeScript Types

```typescript
// أنشئ ملف types.ts
export enum DoctorDocumentType {
  NationalId = 0,
  MedicalPracticeLicense = 1,
  SyndicateMembershipCard = 2,
  MedicalGraduationCertificate = 3,
  SpecialtyCertificate = 4,
  AdditionalCertificates = 5,
  AwardsAndRecognitions = 6,
  PublishedResearch = 7,
  ProfessionalMemberships = 8
}

export enum VerificationDocumentStatus {
  Draft = 0,
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Expired = 4
}

export interface DoctorDocumentResponse {
  id: string;
  createdAt: string;
  createdBy: string | null;
  updatedAt: string | null;
  updatedBy: string | null;
  documentUrl: string;
  type: DoctorDocumentType;
  typeName: string;
  status: VerificationDocumentStatus;
  statusName: string;
  rejectionReason: string | null;
  doctorId: string;
  doctorName: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
  statusCode: number;
}
```

---

## 🎉 الخلاصة

**الملف ده بيوفر لك:**
- ✅ كل الـ endpoints اللي محتاجها للمستندات المتخصصة
- ✅ أنواع المستندات وحالاتها
- ✅ نماذج البيانات الكاملة
- ✅ أمثلة عملية بـ React
- ✅ معالجة الأخطاء
- ✅ Best practices للتكامل

**لو عندك أي استفسار أو محتاج توضيح أكتر، ارجع للـ Backend Team! 🚀**