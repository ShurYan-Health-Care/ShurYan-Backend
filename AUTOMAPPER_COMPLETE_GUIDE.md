# 🗺️ دليل AutoMapper الشامل - ShurYan Backend

## 📚 جدول المحتويات
1. [إيه هو AutoMapper وليه محتاجينه؟](#what-is-automapper)
2. [المشكلة اللي بيحلها](#the-problem)
3. [كيف يعمل AutoMapper؟](#how-it-works)
4. [التطبيق في ShurYan Project](#implementation)
5. [Use Cases حقيقية من الـ Project](#real-use-cases)
6. [الـ Patterns والـ Best Practices](#patterns)
7. [كيف تعمل Mapper جديد بنفسك](#create-your-own)
8. [Performance والـ Optimization](#performance)

---

<a name="what-is-automapper"></a>
## 1️⃣ إيه هو AutoMapper وليه محتاجينه؟

### **التعريف البسيط:**
AutoMapper هو مكتبة بتحول Objects من نوع لنوع تاني بشكل تلقائي.

### **ليه محتاجينه؟**

تخيل عندك:
- **Entity** (الـ Database Model) - فيه كل حاجة من الـ Database
- **DTO** (Data Transfer Object) - اللي بتبعته للـ Client

```csharp
// Entity من الـ Database
public class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // ❌ مش عايزين نبعته للـ Client
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public Address Address { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
    public ICollection<MedicalHistoryItem> MedicalHistory { get; set; }
}

// DTO للـ Response
public class PatientResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    // ❌ مفيش PasswordHash هنا - أمان!
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

<a name="the-problem"></a>
## 2️⃣ المشكلة اللي بيحلها AutoMapper

### **❌ بدون AutoMapper - Manual Mapping:**

```csharp
// في كل Controller Action لازم تعمل كده:
public async Task<IActionResult> GetPatient(Guid id)
{
    var patient = await _context.Patients.FindAsync(id);
    
    // 😫 Manual Mapping - متعب ومُمل
    var response = new PatientResponse
    {
        Id = patient.Id,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        Email = patient.Email,
        DateOfBirth = patient.DateOfBirth,
        PhoneNumber = patient.PhoneNumber,
        CreatedAt = patient.CreatedAt
    };
    
    return Ok(response);
}
```

### **المشاكل:**
1. **كود متكرر** - نفس الـ mapping في كل مكان
2. **عرضة للأخطاء** - لو نسيت property هتنساه في كل مكان
3. **صعب الصيانة** - لو غيرت حاجة لازم تغيرها في 100 مكان
4. **Boilerplate Code** - كود كتير مش مفيد

### **✅ مع AutoMapper:**

```csharp
public async Task<IActionResult> GetPatient(Guid id)
{
    var patient = await _context.Patients.FindAsync(id);
    
    // 🎉 سطر واحد بس!
    var response = _mapper.Map<PatientResponse>(patient);
    
    return Ok(response);
}
```

### **الفوائد:**
1. ✅ **كود نظيف** - سطر واحد بدل 10 أسطر
2. ✅ **مفيش تكرار** - الـ mapping في مكان واحد
3. ✅ **سهل الصيانة** - تعديل واحد يطبق في كل مكان
4. ✅ **Type-Safe** - لو غيرت الـ types هيديك error في الـ compile time

---

<a name="how-it-works"></a>
## 3️⃣ كيف يعمل AutoMapper؟

### **الخطوات الأساسية:**

#### **1. التسجيل (Registration):**
```csharp
// في Program.cs
builder.Services.AddAutoMapper(typeof(MappingProfile));
```

#### **2. إنشاء Profile:**
```csharp
public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        // تعريف الـ Mapping
        CreateMap<Patient, PatientResponse>();
    }
}
```

#### **3. الاستخدام (Injection):**
```csharp
public class PatientService
{
    private readonly IMapper _mapper;
    
    public PatientService(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public async Task<PatientResponse> GetPatient(Guid id)
    {
        var patient = await _repository.GetByIdAsync(id);
        return _mapper.Map<PatientResponse>(patient);
    }
}
```

### **كيف بيشتغل تحت الغطاء؟**

```
1. Startup Time:
   ┌─────────────────────────────────────┐
   │ AutoMapper يقرأ كل الـ Profiles     │
   │ ويعمل Compilation للـ Mappings      │
   │ ويخزنهم في Memory                  │
   └─────────────────────────────────────┘

2. Runtime:
   ┌─────────────────────────────────────┐
   │ لما تطلب Map<TDestination>(source) │
   │ AutoMapper يدور على الـ Mapping     │
   │ المناسب ويطبقه بسرعة فائقة         │
   └─────────────────────────────────────┘
```

---

<a name="implementation"></a>
## 4️⃣ التطبيق في ShurYan Project

### **الهيكل المُنظم:**

```
Mappers/
├── CommonMappingProfile.cs        # Address
├── PatientMappingProfile.cs       # Patient & Medical History
├── DoctorMappingProfile.cs        # Doctor & Related
├── PharmacyMappingProfile.cs      # Pharmacy & Related
├── LaboratoryMappingProfile.cs    # Laboratory & Related
├── ClinicMappingProfile.cs        # Clinic & Related
├── AppointmentMappingProfile.cs   # Appointments
├── PrescriptionMappingProfile.cs  # Prescriptions
├── ReviewMappingProfile.cs        # Reviews
├── NotificationMappingProfile.cs  # Notifications
└── ConsultationMappingProfile.cs  # Consultations
```

### **مثال من PatientMappingProfile:**

```csharp
public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        #region Patient Mappings
        // Entity → Response (للقراءة)
        CreateMap<Patient, PatientResponse>();
        
        // Entity → Basic Response (للـ Lists)
        CreateMap<Patient, PatientBasicResponse>();
        
        // Request → Entity (للإنشاء)
        CreateMap<CreatePatientRequest, Patient>();
        
        // Request → Entity (للتحديث - Partial Update)
        CreateMap<UpdatePatientRequest, Patient>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        #endregion

        #region Medical History Mappings
        CreateMap<MedicalHistoryItem, MedicalHistoryItemResponse>();
        CreateMap<CreateMedicalHistoryItemRequest, MedicalHistoryItem>();
        CreateMap<UpdateMedicalHistoryItemRequest, MedicalHistoryItem>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        #endregion
    }
}
```

---

<a name="real-use-cases"></a>
## 5️⃣ Use Cases حقيقية من ShurYan Project

### **Use Case 1: Patient Registration (إنشاء مريض جديد)**

#### **السيناريو:**
مريض بيسجل في التطبيق لأول مرة.

#### **بدون AutoMapper:**
```csharp
public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
{
    // 😫 Manual Mapping
    var patient = new Patient
    {
        Id = Guid.NewGuid(),
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        PhoneNumber = request.PhoneNumber,
        DateOfBirth = request.DateOfBirth,
        Gender = request.Gender,
        NationalId = request.NationalId,
        BloodType = request.BloodType,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = _currentUserId
    };
    
    await _repository.AddAsync(patient);
    await _unitOfWork.SaveChangesAsync();
    
    // 😫 Manual Mapping مرة تانية
    var response = new PatientResponse
    {
        Id = patient.Id,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        Email = patient.Email,
        PhoneNumber = patient.PhoneNumber,
        DateOfBirth = patient.DateOfBirth,
        Gender = patient.Gender,
        NationalId = patient.NationalId,
        BloodType = patient.BloodType,
        CreatedAt = patient.CreatedAt
    };
    
    return response;
}
```

#### **مع AutoMapper:**
```csharp
public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
{
    // 🎉 سطر واحد للـ mapping
    var patient = _mapper.Map<Patient>(request);
    
    patient.Id = Guid.NewGuid();
    patient.CreatedAt = DateTime.UtcNow;
    patient.CreatedBy = _currentUserId;
    
    await _repository.AddAsync(patient);
    await _unitOfWork.SaveChangesAsync();
    
    // 🎉 سطر واحد للـ mapping
    return _mapper.Map<PatientResponse>(patient);
}
```

**الفرق:** 30 سطر → 10 أسطر 🚀

---

### **Use Case 2: Update Patient Profile (تحديث جزئي)**

#### **السيناريو:**
مريض عايز يحدث بس الـ PhoneNumber والـ Address، باقي البيانات زي ما هي.

#### **المشكلة بدون AutoMapper:**
```csharp
public async Task<PatientResponse> UpdatePatientAsync(Guid id, UpdatePatientRequest request)
{
    var patient = await _repository.GetByIdAsync(id);
    
    // 😫 لازم تشيك كل property لوحده
    if (request.FirstName != null)
        patient.FirstName = request.FirstName;
        
    if (request.LastName != null)
        patient.LastName = request.LastName;
        
    if (request.PhoneNumber != null)
        patient.PhoneNumber = request.PhoneNumber;
        
    if (request.DateOfBirth.HasValue)
        patient.DateOfBirth = request.DateOfBirth.Value;
        
    // ... و20 property تاني 😫
    
    patient.UpdatedAt = DateTime.UtcNow;
    patient.UpdatedBy = _currentUserId;
    
    await _unitOfWork.SaveChangesAsync();
    
    return _mapper.Map<PatientResponse>(patient);
}
```

#### **الحل مع AutoMapper:**
```csharp
public async Task<PatientResponse> UpdatePatientAsync(Guid id, UpdatePatientRequest request)
{
    var patient = await _repository.GetByIdAsync(id);
    
    // 🎉 AutoMapper بيعمل Partial Update تلقائياً
    // بس الـ properties اللي مش null هي اللي بتتحدث
    _mapper.Map(request, patient);
    
    patient.UpdatedAt = DateTime.UtcNow;
    patient.UpdatedBy = _currentUserId;
    
    await _unitOfWork.SaveChangesAsync();
    
    return _mapper.Map<PatientResponse>(patient);
}
```

**السر في الـ Mapping Configuration:**
```csharp
CreateMap<UpdatePatientRequest, Patient>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    //                                                              ^^^^^^^^^^^^^^^^
    //                                    بس لو الـ property مش null
```

---

### **Use Case 3: Get Pharmacy with Working Hours (Custom Mapping)**

#### **السيناريو:**
عايزين نجيب صيدلية مع مواعيد العمل بتاعتها، بس الـ Database بيخزن الوقت كـ `TimeOnly` والـ API محتاج `TimeSpan`.

#### **الـ Entity:**
```csharp
public class PharmacyWorkingHours
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }  // Database: TimeOnly
    public TimeOnly EndTime { get; set; }    // Database: TimeOnly
    public bool IsClosed { get; set; }
}
```

#### **الـ DTO:**
```csharp
public class PharmacyWorkingHoursResponse
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }  // API: TimeSpan
    public TimeSpan EndTime { get; set; }    // API: TimeSpan
    public bool IsClosed { get; set; }
}
```

#### **الـ Custom Mapping:**
```csharp
public class PharmacyMappingProfile : Profile
{
    public PharmacyMappingProfile()
    {
        CreateMap<PharmacyWorkingHours, PharmacyWorkingHoursResponse>()
            .ForMember(dest => dest.StartTime, 
                      opt => opt.MapFrom(src => src.StartTime.ToTimeSpan()))
            .ForMember(dest => dest.EndTime, 
                      opt => opt.MapFrom(src => src.EndTime.ToTimeSpan()));
    }
}
```

#### **الاستخدام:**
```csharp
public async Task<IEnumerable<PharmacyWorkingHoursResponse>> GetWorkingHoursAsync(Guid pharmacyId)
{
    var workingHours = await _repository.GetWorkingHoursAsync(pharmacyId);
    
    // 🎉 AutoMapper بيعمل الـ conversion تلقائياً
    return _mapper.Map<IEnumerable<PharmacyWorkingHoursResponse>>(workingHours);
}
```

---

### **Use Case 4: Prescription with Nested Objects (Complex Mapping)**

#### **السيناريو:**
عايزين نجيب روشتة مع اسم الدكتور واسم المريض والأدوية.

#### **الـ Entities:**
```csharp
public class Prescription
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime PrescribedDate { get; set; }
    
    // Navigation Properties
    public Doctor Doctor { get; set; }
    public Patient Patient { get; set; }
    public ICollection<PrescribedMedication> PrescribedMedications { get; set; }
}

public class Doctor
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

#### **الـ DTO:**
```csharp
public class PrescriptionDetailsResponse
{
    public Guid Id { get; set; }
    public string DoctorName { get; set; }      // ❗ مش موجود في الـ Entity
    public string PatientName { get; set; }     // ❗ مش موجود في الـ Entity
    public DateTime PrescribedDate { get; set; }
    public IEnumerable<PrescribedMedicationResponse> Medications { get; set; }
}
```

#### **الـ Custom Mapping:**
```csharp
public class PrescriptionMappingProfile : Profile
{
    public PrescriptionMappingProfile()
    {
        CreateMap<Prescription, PrescriptionDetailsResponse>()
            // Custom mapping للـ DoctorName
            .ForMember(dest => dest.DoctorName, 
                      opt => opt.MapFrom(src => src.Doctor != null 
                          ? $"{src.Doctor.FirstName} {src.Doctor.LastName}" 
                          : string.Empty))
            // Custom mapping للـ PatientName
            .ForMember(dest => dest.PatientName, 
                      opt => opt.MapFrom(src => src.Patient != null 
                          ? $"{src.Patient.FirstName} {src.Patient.LastName}" 
                          : string.Empty))
            // Nested collection mapping
            .ForMember(dest => dest.Medications, 
                      opt => opt.MapFrom(src => src.PrescribedMedications));
    }
}
```

#### **الاستخدام:**
```csharp
public async Task<PrescriptionDetailsResponse> GetPrescriptionDetailsAsync(Guid prescriptionId)
{
    var prescription = await _repository.GetPrescriptionWithDetailsAsync(prescriptionId);
    
    // 🎉 AutoMapper بيعمل كل الـ complex mapping تلقائياً
    return _mapper.Map<PrescriptionDetailsResponse>(prescription);
}
```

---

### **Use Case 5: Paginated List (Collection Mapping)**

#### **السيناريو:**
عايزين نجيب list من المرضى مع pagination.

```csharp
public async Task<PaginatedResponse<PatientResponse>> GetPaginatedPatientsAsync(
    PaginationParams paginationParams)
{
    var patients = await _repository.GetPaginatedAsync(
        paginationParams.PageNumber, 
        paginationParams.PageSize);
    
    var totalCount = await _repository.GetTotalCountAsync();
    
    // 🎉 AutoMapper بيعمل mapping للـ collection كاملة
    var patientResponses = _mapper.Map<IEnumerable<PatientResponse>>(patients);
    
    return new PaginatedResponse<PatientResponse>
    {
        Data = patientResponses,
        PageNumber = paginationParams.PageNumber,
        PageSize = paginationParams.PageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
        HasPreviousPage = paginationParams.PageNumber > 1,
        HasNextPage = paginationParams.PageNumber < totalPages
    };
}
```

---

<a name="patterns"></a>
## 6️⃣ الـ Patterns والـ Best Practices

### **Pattern 1: Conditional Mapping (Partial Updates)**

```csharp
// ✅ Best Practice للـ Update Requests
CreateMap<UpdatePatientRequest, Patient>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
```

**متى تستخدمه:**
- Update endpoints
- PATCH requests
- Partial updates

---

### **Pattern 2: Custom Value Resolvers**

```csharp
// للـ Complex Logic
public class FullNameResolver : IValueResolver<Patient, PatientResponse, string>
{
    public string Resolve(Patient source, PatientResponse destination, 
                         string destMember, ResolutionContext context)
    {
        return $"{source.FirstName} {source.LastName}";
    }
}

// في الـ Profile
CreateMap<Patient, PatientResponse>()
    .ForMember(dest => dest.FullName, opt => opt.MapFrom<FullNameResolver>());
```

---

### **Pattern 3: Ignore Properties**

```csharp
// لو مش عايز تنقل property معين
CreateMap<Patient, PatientResponse>()
    .ForMember(dest => dest.SomeProperty, opt => opt.Ignore());
```

---

### **Pattern 4: Reverse Mapping**

```csharp
// لو عايز الـ mapping في الاتجاهين
CreateMap<Patient, PatientResponse>().ReverseMap();
// بدل ما تكتب:
// CreateMap<Patient, PatientResponse>();
// CreateMap<PatientResponse, Patient>();
```

---

### **Pattern 5: Flattening**

```csharp
// Entity
public class Patient
{
    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

// DTO
public class PatientResponse
{
    public string AddressCity { get; set; }    // AutoMapper بيفهم تلقائياً
    public string AddressStreet { get; set; }  // إن دول من Address.City و Address.Street
}

// Mapping
CreateMap<Patient, PatientResponse>();  // ✅ بيشتغل تلقائياً!
```

---

<a name="create-your-own"></a>
## 7️⃣ كيف تعمل Mapper جديد بنفسك؟

### **خطوة بخطوة:**

#### **1. حدد الـ Domain:**
```
مثال: عايز أضيف Payment Module
```

#### **2. أنشئ الـ Profile File:**
```csharp
// Mappers/PaymentMappingProfile.cs
using AutoMapper;
using Shuryan.Application.DTOs.Requests.Payment;
using Shuryan.Application.DTOs.Responses.Payment;
using Shuryan.Core.Entities.Payment;

namespace Shuryan.Application.Mappers
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            #region Payment Mappings
            // Entity → Response
            CreateMap<Payment, PaymentResponse>();
            
            // Request → Entity
            CreateMap<CreatePaymentRequest, Payment>();
            
            // Update Request → Entity (Partial)
            CreateMap<UpdatePaymentRequest, Payment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
            
            #region Payment Method Mappings
            CreateMap<PaymentMethod, PaymentMethodResponse>();
            CreateMap<CreatePaymentMethodRequest, PaymentMethod>();
            #endregion
        }
    }
}
```

#### **3. AutoMapper هيكتشفه تلقائياً:**
```
✅ مفيش حاجة تانية محتاج تعملها!
AutoMapper بيدور على كل الـ classes اللي بترث من Profile
```

#### **4. استخدمه في الـ Service:**
```csharp
public class PaymentService : IPaymentService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    
    public PaymentService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
    {
        var payment = _mapper.Map<Payment>(request);
        
        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<PaymentResponse>(payment);
    }
}
```

---

### **Checklist للـ Mapper جديد:**

```
✅ 1. أنشئ الـ Profile class
✅ 2. ورث من Profile
✅ 3. في الـ Constructor، استخدم CreateMap<Source, Destination>()
✅ 4. لو محتاج Custom Mapping، استخدم ForMember()
✅ 5. لو Update Request، استخدم Conditional Mapping
✅ 6. نظم الـ Mappings في #regions
✅ 7. AutoMapper هيكتشفه تلقائياً - مفيش حاجة تانية!
```

---

<a name="performance"></a>
## 8️⃣ Performance والـ Optimization

### **هل AutoMapper بطيء؟**

#### **❌ الخرافة:**
"AutoMapper بطيء ومش مناسب للـ Production"

#### **✅ الحقيقة:**
AutoMapper **سريع جداً** لو استخدمته صح!

### **Performance Benchmarks:**

```
Manual Mapping:     100 ns
AutoMapper:         150 ns
Difference:         50 ns (0.00005 milliseconds)

في request فيه 1000 object:
Manual:    0.1 ms
AutoMapper: 0.15 ms
Difference: 0.05 ms (غير محسوس!)
```

### **Best Practices للـ Performance:**

#### **1. ✅ استخدم Dependency Injection:**
```csharp
// ✅ Good - Singleton IMapper
public class PatientService
{
    private readonly IMapper _mapper;
    
    public PatientService(IMapper mapper)
    {
        _mapper = mapper;  // ✅ Fast!
    }
}

// ❌ Bad - Creating new mapper every time
public class PatientService
{
    public void DoSomething()
    {
        var config = new MapperConfiguration(cfg => {});
        var mapper = config.CreateMapper();  // ❌ Slow!
    }
}
```

#### **2. ✅ استخدم ProjectTo للـ Queries:**
```csharp
// ✅ Good - Query projection
var patients = await _context.Patients
    .ProjectTo<PatientResponse>(_mapper.ConfigurationProvider)
    .ToListAsync();
// SQL بيجيب بس الـ columns المطلوبة

// ❌ Bad - Load everything then map
var patients = await _context.Patients.ToListAsync();
var response = _mapper.Map<List<PatientResponse>>(patients);
// SQL بيجيب كل الـ columns حتى اللي مش محتاجينها
```

#### **3. ✅ تجنب Over-Mapping:**
```csharp
// ✅ Good - Basic response للـ lists
CreateMap<Patient, PatientBasicResponse>();  // بس الـ essential fields

// ❌ Bad - Full response للـ lists
CreateMap<Patient, PatientResponse>();  // كل الـ fields حتى اللي مش محتاجينها
```

---

## 🎯 الخلاصة النهائية

### **متى تستخدم AutoMapper؟**

#### **✅ استخدمه في:**
1. **Entity → DTO** (Response)
2. **DTO → Entity** (Create/Update)
3. **Complex Mappings** (Nested objects)
4. **Collection Mappings** (Lists)
5. **Partial Updates** (PATCH)

#### **❌ لا تستخدمه في:**
1. **Simple assignments** (property واحد أو اتنين)
2. **Complex business logic** (استخدم Custom Services)
3. **Performance-critical paths** (لو محتاج آخر نانو ثانية)

---

## 📖 المراجع والموارد

### **في ShurYan Project:**
- `Mappers/` folder - كل الـ Profiles
- `Mappers/README.md` - التوثيق
- `ServiceExtensions.cs` - التسجيل

### **External Resources:**
- [AutoMapper Documentation](https://docs.automapper.org/)
- [AutoMapper GitHub](https://github.com/AutoMapper/AutoMapper)
- [Performance Guide](https://docs.automapper.org/en/stable/Performance.html)

---

## 🎓 تمرين عملي

جرب تعمل Mapper جديد للـ Appointment:

```csharp
// 1. أنشئ AppointmentMappingProfile
// 2. أضف mappings للـ:
//    - Appointment → AppointmentResponse
//    - CreateAppointmentRequest → Appointment
//    - UpdateAppointmentRequest → Appointment (Partial)
// 3. استخدمه في AppointmentService
// 4. اختبره في الـ Controller
```

**الحل موجود في:** `Mappers/AppointmentMappingProfile.cs` 😉

---

**تم إعداد هذا الدليل خصيصاً لـ ShurYan Backend Project** 🚀
