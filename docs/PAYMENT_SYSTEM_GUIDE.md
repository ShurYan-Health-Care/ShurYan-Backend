# 💳 نظام الدفع - Payment System

## 📋 نظرة عامة (Overview)

بص يا معلم، نظام الدفع اللي عملناه ده شامل كل حاجة محتاجها عشان تدير عمليات الدفع في المشروع. النظام بيدعم طرق دفع مختلفة ومتكامل مع الـ Orders (سواء صيدلية أو معمل).

---

## 🏗️ البنية المعمارية (Architecture)

### 1. **الـ Entities (الكيانات)**

#### **Payment Entity**
الجدول الرئيسي اللي بيحفظ كل عملية دفع:
- معلومات المستخدم والطلب
- المبلغ والعملة
- طريقة الدفع (كاش، فيزا، محفظة إلكترونية)
- الحالة (Pending, Completed, Failed, Refunded)
- بيانات مزود الدفع (Paymob, Fawry, etc.)
- معلومات الاسترجاع (Refund)

#### **PaymentTransaction Entity**
سجل تفصيلي لكل خطوة في عملية الدفع:
- نوع العملية (Initiation, Confirmation, Refund)
- المبلغ والحالة
- رد مزود الدفع
- رسائل الخطأ لو حصل مشكلة

---

### 2. **الـ Enums (التعدادات)**

#### **PaymentMethod** - طرق الدفع
```csharp
- CashOnDelivery = 1        // كاش عند الاستلام
- CreditCard = 2            // بطاقة ائتمان/خصم
- VodafoneCash = 3          // فودافون كاش
- Wallet = 4                // محفظة إلكترونية
- BankTransfer = 5          // تحويل بنكي
```

#### **PaymentStatus** - حالات الدفع
```csharp
- Pending = 1               // في انتظار الدفع
- Processing = 2            // جاري المعالجة
- Completed = 3             // تم الدفع بنجاح
- Failed = 4                // فشل الدفع
- Cancelled = 5             // تم الإلغاء
- Refunded = 6              // تم الاسترجاع
- PartiallyRefunded = 7     // استرجاع جزئي
```

#### **PaymentProvider** - مزودي الدفع
```csharp
- Internal = 1              // نظام داخلي
- Paymob = 2                // باي موب
- Fawry = 3                 // فوري
- Opay = 4                  // أوبي
- VodafoneCash = 5          // فودافون كاش
```

#### **OrderType** - أنواع الطلبات
```csharp
- PharmacyOrder = 1         // طلب صيدلية
- LabOrder = 2              // طلب معمل
- ConsultationBooking = 3   // حجز استشارة
```

---

### 3. **الـ DTOs (Data Transfer Objects)**

#### **Requests (الطلبات)**

**InitiatePaymentRequest** - بدء عملية دفع
```csharp
{
    "orderType": "PharmacyOrder",
    "orderId": "guid",
    "amount": 150.50,
    "paymentMethod": 2,
    "provider": 2,
    "notes": "ملاحظات اختيارية",
    "returnUrl": "https://app.com/payment/callback"
}
```

**ConfirmPaymentRequest** - تأكيد الدفع
```csharp
{
    "paymentId": "guid",
    "providerTransactionId": "TXN123456",
    "providerResponse": "Success"
}
```

**RefundPaymentRequest** - استرجاع مبلغ
```csharp
{
    "paymentId": "guid",
    "amount": 50.00,
    "reason": "سبب الاسترجاع"
}
```

#### **Responses (الردود)**

**InitiatePaymentResponse**
```csharp
{
    "paymentId": "guid",
    "paymentUrl": "https://payment-gateway.com/pay/xxx",
    "qrCode": "base64_qr_code",
    "referenceNumber": "REF123",
    "requiresRedirect": true,
    "message": "يرجى إكمال عملية الدفع"
}
```

**PaymentResponse**
```csharp
{
    "id": "guid",
    "userId": "guid",
    "orderType": "PharmacyOrder",
    "orderId": "guid",
    "amount": 150.50,
    "currency": "EGP",
    "paymentMethod": 2,
    "paymentMethodName": "بطاقة ائتمان/خصم",
    "status": 3,
    "statusName": "تم الدفع بنجاح",
    "provider": 2,
    "providerName": "باي موب",
    "createdAt": "2024-11-06T10:00:00Z",
    "completedAt": "2024-11-06T10:05:00Z",
    "transactions": [...]
}
```

---

## 🔄 سيناريوهات الاستخدام (Use Cases)

### السيناريو 1: دفع طلب صيدلية بالفيزا

```
1. المريض يطلب أدوية من الصيدلية
2. الصيدلية تحدد السعر
3. المريض يختار الدفع بالفيزا
4. النظام ينشئ Payment record بحالة Pending
5. النظام يرجع PaymentUrl للمريض
6. المريض يدفع من خلال Paymob
7. Paymob يرجع callback للنظام
8. النظام يحدث Payment لـ Completed
9. النظام يحدث PharmacyOrder لـ PaidPendingPharmacyConfirmation
10. الصيدلية تشوف الطلب وتبدأ التحضير
```

### السيناريو 2: دفع كاش عند الاستلام

```
1. المريض يطلب أدوية
2. الصيدلية تحدد السعر
3. المريض يختار Cash on Delivery
4. النظام ينشئ Payment بحالة Pending
5. الطلب يتحول لـ PendingPayment
6. عند التسليم، السائق يحصل الفلوس
7. الصيدلية تأكد الدفع من الـ Dashboard
8. النظام يحدث Payment لـ Completed
```

### السيناريو 3: استرجاع مبلغ (Refund)

```
1. Admin يفتح صفحة الدفعات
2. يختار Payment معين
3. يضغط Refund
4. يدخل المبلغ المراد استرجاعه والسبب
5. النظام يحدث Payment:
   - RefundedAmount += المبلغ
   - Status = Refunded أو PartiallyRefunded
6. يتم إنشاء PaymentTransaction من نوع Refund
```

---

## 🎯 API Endpoints

### **POST** `/api/payments/initiate`
بدء عملية دفع جديدة
- **Auth**: Required
- **Body**: `InitiatePaymentRequest`
- **Response**: `InitiatePaymentResponse`

### **POST** `/api/payments/confirm`
تأكيد عملية الدفع
- **Auth**: Required
- **Body**: `ConfirmPaymentRequest`
- **Response**: `PaymentResponse`

### **POST** `/api/payments/{paymentId}/cancel`
إلغاء عملية دفع
- **Auth**: Required
- **Response**: `PaymentResponse`

### **POST** `/api/payments/refund`
استرجاع مبلغ (Admin فقط)
- **Auth**: Required (Admin Role)
- **Body**: `RefundPaymentRequest`
- **Response**: `PaymentResponse`

### **GET** `/api/payments/{paymentId}`
الحصول على تفاصيل عملية دفع
- **Auth**: Required
- **Response**: `PaymentResponse`

### **GET** `/api/payments/my-payments`
الحصول على عمليات الدفع الخاصة بالمستخدم
- **Auth**: Required
- **Query**: `pageNumber`, `pageSize`
- **Response**: `List<PaymentResponse>`

### **GET** `/api/payments/order/{orderType}/{orderId}`
الحصول على عمليات الدفع الخاصة بطلب معين
- **Auth**: Required
- **Response**: `List<PaymentResponse>`

### **POST** `/api/payments/callback/{provider}`
Webhook لاستقبال ردود مزودي الدفع
- **Auth**: None (Public)
- **Note**: يجب تأمينه بـ Signature Verification

---

## 🔧 التكامل مع الطلبات (Order Integration)

### PharmacyOrder Flow
```
PendingPharmacyResponse → WaitingForPatientConfirmation 
→ PendingPayment → [Payment] → PaidPendingPharmacyConfirmation 
→ Confirmed → PreparationInProgress → OutForDelivery → Delivered
```

### LabOrder Flow
```
PendingPayment → [Payment] → PaidPendingLabConfirmation 
→ ConfirmedByLab → InProgress → ResultsReady → Completed
```

---

## 🛡️ الأمان (Security)

### 1. **Authentication & Authorization**
- كل الـ Endpoints محمية بـ JWT
- Refund محمي بـ Admin Role فقط
- المستخدم يقدر يشوف دفعاته بس

### 2. **Payment Provider Security**
- التحقق من Signature في الـ Callback
- تخزين Provider Response للمراجعة
- IP Address Tracking

### 3. **Data Validation**
- FluentValidation على كل الـ Requests
- Amount validation (> 0)
- Order ownership verification

---

## 📊 Repository Methods

```csharp
// الحصول على دفعة مع كل الـ Transactions
GetPaymentWithTransactionsAsync(Guid paymentId)

// دفعات المستخدم
GetPaymentsByUserIdAsync(Guid userId, pageNumber, pageSize)

// دفعات طلب معين
GetPaymentsByOrderAsync(string orderType, Guid orderId)

// البحث بـ Provider Transaction ID
GetPaymentByProviderTransactionIdAsync(string providerTransactionId)

// دفعات حسب الحالة
GetPaymentsByStatusAsync(PaymentStatus status, pageNumber, pageSize)

// الدفعات المعلقة
GetPendingPaymentsAsync(pageNumber, pageSize)

// إجمالي الإيرادات
GetTotalRevenueAsync(DateTime? startDate, DateTime? endDate)

// عدد الدفعات حسب الحالة
GetPaymentCountByStatusAsync(PaymentStatus status)
```

---

## 🚀 الخطوات التالية (Next Steps)

### 1. **تكامل مع Payment Providers**
- تنفيذ `HandleProviderCallbackAsync` لكل Provider
- إضافة Signature Verification
- معالجة Webhooks بشكل صحيح

### 2. **إضافة Payment Settings**
في `appsettings.json`:
```json
{
  "PaymentSettings": {
    "Paymob": {
      "ApiKey": "",
      "IntegrationId": "",
      "IframeId": ""
    },
    "Fawry": {
      "MerchantCode": "",
      "SecurityKey": ""
    }
  }
}
```

### 3. **Notifications**
- إرسال إشعار للمستخدم عند نجاح/فشل الدفع
- إرسال Email confirmation
- Push notifications للموبايل

### 4. **Analytics & Reporting**
- Dashboard للإيرادات
- تقارير الدفعات الفاشلة
- معدلات التحويل (Conversion rates)

### 5. **Testing**
- Unit tests للـ PaymentService
- Integration tests للـ Payment flow
- Mock Payment Provider للـ Testing

---

## 📝 ملاحظات مهمة

1. **الـ Migration**: لازم تعمل migration عشان تضيف الجداول للـ Database
   ```bash
   dotnet ef migrations add AddPaymentSystem
   dotnet ef database update
   ```

2. **الـ AutoMapper**: تم إضافة `PaymentMappingProfile` للـ AutoMapper

3. **الـ UnitOfWork**: تم إضافة `IPaymentRepository` للـ UnitOfWork

4. **الـ DbContext**: تم إضافة `DbSet<Payment>` و `DbSet<PaymentTransaction>`

5. **الـ DI Container**: تم تسجيل `IPaymentService` في `ServiceExtensions`

---

## 🎓 أمثلة عملية (Examples)

### مثال 1: بدء دفع من الكود
```csharp
var request = new InitiatePaymentRequest
{
    OrderType = "PharmacyOrder",
    OrderId = pharmacyOrderId,
    Amount = 250.00m,
    PaymentMethod = PaymentMethod.CreditCard,
    Provider = PaymentProvider.Paymob
};

var result = await _paymentService.InitiatePaymentAsync(userId, request, ipAddress);

if (result.IsSuccess)
{
    // Redirect user to result.Data.PaymentUrl
}
```

### مثال 2: تأكيد دفع
```csharp
var request = new ConfirmPaymentRequest
{
    PaymentId = paymentId,
    ProviderTransactionId = "TXN123456",
    ProviderResponse = "Success"
};

var result = await _paymentService.ConfirmPaymentAsync(request);
```

---

## 💡 Tips & Best Practices

1. **Always log payment operations** - كل عملية دفع لازم تتسجل في الـ Logs
2. **Use transactions** - استخدم Database Transactions للعمليات المعقدة
3. **Idempotency** - تأكد إن الـ Payment مش بيتكرر لو المستخدم ضغط مرتين
4. **Timeout handling** - حدد timeout للدفعات المعلقة
5. **Retry mechanism** - لو فشل الـ Callback، حاول تاني
6. **Monitoring** - راقب معدلات نجاح/فشل الدفع

---

تم بحمد الله! 🎉
نظام دفع متكامل جاهز للاستخدام
