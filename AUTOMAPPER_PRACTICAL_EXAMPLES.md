# 🎯 AutoMapper - أمثلة عملية من ShurYan Project

## 📋 جدول المحتويات
1. [Patient Management Examples](#patient-examples)
2. [Pharmacy Management Examples](#pharmacy-examples)
3. [Doctor Management Examples](#doctor-examples)
4. [Appointment Booking Examples](#appointment-examples)
5. [Advanced Scenarios](#advanced-scenarios)
6. [Common Mistakes & Solutions](#common-mistakes)

---

<a name="patient-examples"></a>
## 1️⃣ Patient Management Examples

### **مثال 1: Patient Registration Flow (التسجيل الكامل)**

#### **الـ Request من الـ Frontend:**
```json
POST /api/auth/register/patient
{
  "firstName": "أحمد",
  "lastName": "محمد",
  "email": "ahmed@example.com",
  "password": "SecurePass123!",
  "phoneNumber": "+201234567890",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "nationalId": "29005151234567",
  "bloodType": "O+"
}
```

#### **الـ Flow الكامل:**

```csharp
// 1. في AuthController
[HttpPost("register/patient")]
public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest request)
{
    // Validation تلقائي من الـ Data Annotations
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // استدعاء الـ Service
    var response = await _authService.RegisterPatientAsync(request);
    
    return Ok(ApiResponse<PatientResponse>.Success(response, "Patient registered successfully"));
}

// 2. في AuthService
public async Task<PatientResponse> RegisterPatientAsync(RegisterPatientRequest request)
{
    // Check if email exists
    if (await _patientService.IsEmailUniqueAsync(request.Email))
        throw new BadRequestException("Email already exists");
    
    // 🎯 AutoMapper: Request → Entity
    var patient = _mapper.Map<Patient>(request);
    
    // Hash Password
    patient.PasswordHash = _passwordHasher.HashPassword(patient, request.Password);
    patient.Id = Guid.NewGuid();
    patient.CreatedAt = DateTime.UtcNow;
    patient.EmailConfirmed = false;
    
    // Save to database
    await _unitOfWork.Patients.AddAsync(patient);
    await _unitOfWork.SaveChangesAsync();
    
    // Send verification email
    await _emailService.SendVerificationEmailAsync(patient.Email);
    
    // 🎯 AutoMapper: Entity → Response
    return _mapper.Map<PatientResponse>(patient);
}
```

#### **الـ Mapping Configuration:**
```csharp
public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        // RegisterPatientRequest → Patient
        CreateMap<RegisterPatientRequest, Patient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore());
        
        // Patient → PatientResponse
        CreateMap<Patient, PatientResponse>();
    }
}
```

#### **الـ Response للـ Frontend:**
```json
{
  "isSuccess": true,
  "message": "Patient registered successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "أحمد",
    "lastName": "محمد",
    "email": "ahmed@example.com",
    "phoneNumber": "+201234567890",
    "dateOfBirth": "1990-05-15",
    "gender": "Male",
    "bloodType": "O+",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

---

### **مثال 2: Update Patient Profile (تحديث جزئي)**

#### **السيناريو:**
المريض عايز يحدث بس الـ Phone Number والـ Address، باقي البيانات تفضل زي ما هي.

#### **الـ Request:**
```json
PATCH /api/patients/me
{
  "phoneNumber": "+201098765432",
  "address": {
    "street": "شارع الجامعة",
    "city": "القاهرة",
    "governorate": "Cairo"
  }
}
```

#### **الكود:**
```csharp
// Controller
[HttpPatch("me")]
[Authorize(Roles = "Patient")]
public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePatientRequest request)
{
    var userId = User.GetUserId();
    var response = await _patientService.UpdatePatientAsync(userId, request);
    
    return Ok(ApiResponse<PatientResponse>.Success(response, "Profile updated successfully"));
}

// Service
public async Task<PatientResponse> UpdatePatientAsync(Guid id, UpdatePatientRequest request)
{
    var patient = await _unitOfWork.Patients.GetByIdAsync(id);
    
    if (patient == null)
        throw new NotFoundException("Patient not found");
    
    // 🎯 AutoMapper: Partial Update
    // بس الـ properties اللي في الـ request (مش null) هي اللي بتتحدث
    _mapper.Map(request, patient);
    
    patient.UpdatedAt = DateTime.UtcNow;
    patient.UpdatedBy = id;
    
    await _unitOfWork.SaveChangesAsync();
    
    return _mapper.Map<PatientResponse>(patient);
}
```

#### **الـ Mapping Configuration (السر هنا!):**
```csharp
CreateMap<UpdatePatientRequest, Patient>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    //                                                              ^^^^^^^^^^^^^^^^^^
    //                                    بس لو الـ property مش null هي اللي بتتحدث
```

#### **كيف بيشتغل؟**

```csharp
// قبل الـ Mapping:
patient.FirstName = "أحمد";
patient.LastName = "محمد";
patient.PhoneNumber = "+201234567890";
patient.Email = "ahmed@example.com";

// الـ Request:
request.PhoneNumber = "+201098765432";  // ✅ موجود
request.FirstName = null;               // ❌ null
request.LastName = null;                // ❌ null
request.Email = null;                   // ❌ null

// بعد الـ Mapping:
patient.FirstName = "أحمد";             // ✅ مفضلش زي ما هو
patient.LastName = "محمد";              // ✅ مفضلش زي ما هو
patient.PhoneNumber = "+201098765432";  // ✅ اتحدث!
patient.Email = "ahmed@example.com";    // ✅ مفضلش زي ما هو
```

---

### **مثال 3: Get Patient Medical History (Nested Objects)**

#### **السيناريو:**
عايزين نجيب المريض مع كل الـ Medical History بتاعته.

#### **الـ Entities:**
```csharp
public class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // Navigation Property
    public ICollection<MedicalHistoryItem> MedicalHistory { get; set; }
}

public class MedicalHistoryItem
{
    public Guid Id { get; set; }
    public string Condition { get; set; }
    public DateTime DiagnosedDate { get; set; }
    public string Treatment { get; set; }
    public string Notes { get; set; }
}
```

#### **الكود:**
```csharp
// Service
public async Task<PatientWithHistoryResponse> GetPatientWithHistoryAsync(Guid id)
{
    var patient = await _unitOfWork.Patients
        .Include(p => p.MedicalHistory)  // ✅ Eager Loading
        .FirstOrDefaultAsync(p => p.Id == id);
    
    if (patient == null)
        throw new NotFoundException("Patient not found");
    
    // 🎯 AutoMapper بيعمل mapping للـ nested collections تلقائياً
    return _mapper.Map<PatientWithHistoryResponse>(patient);
}
```

#### **الـ Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "أحمد",
  "lastName": "محمد",
  "medicalHistory": [
    {
      "id": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
      "condition": "Diabetes Type 2",
      "diagnosedDate": "2020-03-15",
      "treatment": "Metformin 500mg",
      "notes": "Monitor blood sugar regularly"
    },
    {
      "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
      "condition": "Hypertension",
      "diagnosedDate": "2021-06-20",
      "treatment": "Amlodipine 5mg",
      "notes": "Check blood pressure weekly"
    }
  ]
}
```

---

<a name="pharmacy-examples"></a>
## 2️⃣ Pharmacy Management Examples

### **مثال 4: Pharmacy Working Hours (Custom Mapping)**

#### **المشكلة:**
الـ Database بيخزن الوقت كـ `TimeOnly` (SQL Server 2022+) بس الـ API محتاج `TimeSpan` عشان الـ JSON serialization.

#### **الـ Entity:**
```csharp
public class PharmacyWorkingHours
{
    public Guid Id { get; set; }
    public Guid PharmacyId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }  // Database: TimeOnly (09:00)
    public TimeOnly EndTime { get; set; }    // Database: TimeOnly (21:00)
    public bool IsClosed { get; set; }
}
```

#### **الـ DTO:**
```csharp
public class PharmacyWorkingHoursResponse
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }  // API: TimeSpan (09:00:00)
    public TimeSpan EndTime { get; set; }    // API: TimeSpan (21:00:00)
    public bool IsClosed { get; set; }
}
```

#### **الـ Mapping Configuration:**
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
    var workingHours = await _unitOfWork.PharmacyWorkingHours
        .Where(wh => wh.PharmacyId == pharmacyId)
        .ToListAsync();
    
    // 🎯 AutoMapper بيعمل الـ conversion تلقائياً
    return _mapper.Map<IEnumerable<PharmacyWorkingHoursResponse>>(workingHours);
}
```

#### **الـ Response:**
```json
[
  {
    "id": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
    "dayOfWeek": "Monday",
    "startTime": "09:00:00",
    "endTime": "21:00:00",
    "isClosed": false
  },
  {
    "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "dayOfWeek": "Friday",
    "startTime": "00:00:00",
    "endTime": "00:00:00",
    "isClosed": true
  }
]
```

---

### **مثال 5: Check if Pharmacy is Open Now**

#### **السيناريو:**
المريض عايز يعرف الصيدلية مفتوحة دلوقتي ولا لأ.

```csharp
public async Task<IsOpenResponse> IsPharmacyOpenAsync(Guid pharmacyId)
{
    var now = DateTime.UtcNow;
    var currentDay = now.DayOfWeek;
    var currentTime = TimeOnly.FromDateTime(now);
    
    var workingHours = await _unitOfWork.PharmacyWorkingHours
        .FirstOrDefaultAsync(wh => 
            wh.PharmacyId == pharmacyId && 
            wh.DayOfWeek == currentDay);
    
    if (workingHours == null || workingHours.IsClosed)
    {
        return new IsOpenResponse 
        { 
            IsOpen = false, 
            Message = "Pharmacy is closed today" 
        };
    }
    
    var isOpen = currentTime >= workingHours.StartTime && 
                 currentTime <= workingHours.EndTime;
    
    return new IsOpenResponse
    {
        IsOpen = isOpen,
        Message = isOpen 
            ? "Pharmacy is currently open" 
            : $"Pharmacy opens at {workingHours.StartTime}",
        OpeningTime = workingHours.StartTime.ToTimeSpan(),
        ClosingTime = workingHours.EndTime.ToTimeSpan()
    };
}
```

---

<a name="doctor-examples"></a>
## 3️⃣ Doctor Management Examples

### **مثال 6: Doctor with Availability (Complex Query)**

#### **السيناريو:**
المريض بيدور على دكتور متاح في وقت معين.

```csharp
public async Task<IEnumerable<DoctorResponse>> SearchAvailableDoctorsAsync(
    SearchDoctorsRequest request)
{
    var query = _unitOfWork.Doctors
        .Include(d => d.Specialization)
        .Include(d => d.Clinic)
        .Include(d => d.Availability)
        .AsQueryable();
    
    // Filter by specialization
    if (request.SpecializationId.HasValue)
        query = query.Where(d => d.SpecializationId == request.SpecializationId);
    
    // Filter by governorate
    if (request.Governorate.HasValue)
        query = query.Where(d => d.Clinic.Address.Governorate == request.Governorate);
    
    // Filter by availability
    if (request.Date.HasValue)
    {
        var dayOfWeek = request.Date.Value.DayOfWeek;
        query = query.Where(d => d.Availability.Any(a => 
            a.DayOfWeek == dayOfWeek && 
            !a.IsBlocked));
    }
    
    var doctors = await query.ToListAsync();
    
    // 🎯 AutoMapper بيعمل mapping للـ complex objects
    return _mapper.Map<IEnumerable<DoctorResponse>>(doctors);
}
```

---

<a name="appointment-examples"></a>
## 4️⃣ Appointment Booking Examples

### **مثال 7: Book Appointment (Multi-Step Process)**

#### **الـ Flow الكامل:**

```csharp
public async Task<AppointmentResponse> BookAppointmentAsync(CreateAppointmentRequest request)
{
    // 1. Validate doctor exists and is available
    var doctor = await _unitOfWork.Doctors
        .Include(d => d.Availability)
        .FirstOrDefaultAsync(d => d.Id == request.DoctorId);
    
    if (doctor == null)
        throw new NotFoundException("Doctor not found");
    
    // 2. Check if time slot is available
    var isAvailable = await IsTimeSlotAvailableAsync(
        request.DoctorId, 
        request.AppointmentDate, 
        request.StartTime);
    
    if (!isAvailable)
        throw new BadRequestException("Time slot is not available");
    
    // 3. 🎯 AutoMapper: Request → Entity
    var appointment = _mapper.Map<Appointment>(request);
    
    appointment.Id = Guid.NewGuid();
    appointment.PatientId = _currentUserId;
    appointment.Status = AppointmentStatus.Pending;
    appointment.CreatedAt = DateTime.UtcNow;
    
    // 4. Calculate end time
    appointment.EndTime = appointment.StartTime.Add(TimeSpan.FromMinutes(30));
    
    // 5. Save to database
    await _unitOfWork.Appointments.AddAsync(appointment);
    await _unitOfWork.SaveChangesAsync();
    
    // 6. Send notifications
    await _notificationService.NotifyDoctorNewAppointmentAsync(appointment);
    await _notificationService.NotifyPatientAppointmentConfirmationAsync(appointment);
    
    // 7. 🎯 AutoMapper: Entity → Response
    return _mapper.Map<AppointmentResponse>(appointment);
}
```

---

<a name="advanced-scenarios"></a>
## 5️⃣ Advanced Scenarios

### **مثال 8: Prescription with Medications (Deep Nesting)**

#### **الـ Entities:**
```csharp
public class Prescription
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime PrescribedDate { get; set; }
    public string Diagnosis { get; set; }
    public string Notes { get; set; }
    
    // Navigation Properties
    public Doctor Doctor { get; set; }
    public Patient Patient { get; set; }
    public ICollection<PrescribedMedication> PrescribedMedications { get; set; }
}

public class PrescribedMedication
{
    public Guid Id { get; set; }
    public Guid PrescriptionId { get; set; }
    public Guid MedicationId { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public int DurationDays { get; set; }
    
    // Navigation Properties
    public Medication Medication { get; set; }
}

public class Medication
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ActiveIngredient { get; set; }
    public string Manufacturer { get; set; }
}
```

#### **الـ DTO:**
```csharp
public class PrescriptionDetailsResponse
{
    public Guid Id { get; set; }
    public string DoctorName { get; set; }
    public string PatientName { get; set; }
    public DateTime PrescribedDate { get; set; }
    public string Diagnosis { get; set; }
    public string Notes { get; set; }
    public IEnumerable<PrescribedMedicationResponse> Medications { get; set; }
}

public class PrescribedMedicationResponse
{
    public Guid Id { get; set; }
    public string MedicationName { get; set; }
    public string ActiveIngredient { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public int DurationDays { get; set; }
}
```

#### **الـ Mapping:**
```csharp
public class PrescriptionMappingProfile : Profile
{
    public PrescriptionMappingProfile()
    {
        CreateMap<Prescription, PrescriptionDetailsResponse>()
            .ForMember(dest => dest.DoctorName, 
                      opt => opt.MapFrom(src => 
                          src.Doctor != null 
                              ? $"Dr. {src.Doctor.FirstName} {src.Doctor.LastName}" 
                              : string.Empty))
            .ForMember(dest => dest.PatientName, 
                      opt => opt.MapFrom(src => 
                          src.Patient != null 
                              ? $"{src.Patient.FirstName} {src.Patient.LastName}" 
                              : string.Empty))
            .ForMember(dest => dest.Medications, 
                      opt => opt.MapFrom(src => src.PrescribedMedications));
        
        CreateMap<PrescribedMedication, PrescribedMedicationResponse>()
            .ForMember(dest => dest.MedicationName, 
                      opt => opt.MapFrom(src => src.Medication.Name))
            .ForMember(dest => dest.ActiveIngredient, 
                      opt => opt.MapFrom(src => src.Medication.ActiveIngredient));
    }
}
```

#### **الاستخدام:**
```csharp
public async Task<PrescriptionDetailsResponse> GetPrescriptionDetailsAsync(Guid prescriptionId)
{
    var prescription = await _unitOfWork.Prescriptions
        .Include(p => p.Doctor)
        .Include(p => p.Patient)
        .Include(p => p.PrescribedMedications)
            .ThenInclude(pm => pm.Medication)
        .FirstOrDefaultAsync(p => p.Id == prescriptionId);
    
    if (prescription == null)
        throw new NotFoundException("Prescription not found");
    
    // 🎯 AutoMapper بيعمل كل الـ deep mapping تلقائياً
    return _mapper.Map<PrescriptionDetailsResponse>(prescription);
}
```

#### **الـ Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "doctorName": "Dr. محمد أحمد",
  "patientName": "أحمد محمد",
  "prescribedDate": "2024-01-15T10:30:00Z",
  "diagnosis": "Upper Respiratory Tract Infection",
  "notes": "Rest and drink plenty of fluids",
  "medications": [
    {
      "id": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
      "medicationName": "Amoxicillin",
      "activeIngredient": "Amoxicillin Trihydrate",
      "dosage": "500mg",
      "frequency": "3 times daily",
      "durationDays": 7
    },
    {
      "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
      "medicationName": "Paracetamol",
      "activeIngredient": "Acetaminophen",
      "dosage": "500mg",
      "frequency": "As needed (max 4 times daily)",
      "durationDays": 5
    }
  ]
}
```

---

### **مثال 9: Paginated Search with Filters**

```csharp
public async Task<PaginatedResponse<PatientResponse>> SearchPatientsAsync(
    SearchTermPatientsRequest request)
{
    var query = _unitOfWork.Patients.AsQueryable();
    
    // Apply filters
    if (!string.IsNullOrEmpty(request.SearchTerm))
    {
        query = query.Where(p => 
            p.FirstName.Contains(request.SearchTerm) ||
            p.LastName.Contains(request.SearchTerm) ||
            p.Email.Contains(request.SearchTerm) ||
            p.PhoneNumber.Contains(request.SearchTerm));
    }
    
    if (request.Gender.HasValue)
        query = query.Where(p => p.Gender == request.Gender);
    
    if (request.BloodType.HasValue)
        query = query.Where(p => p.BloodType == request.BloodType);
    
    // Get total count
    var totalCount = await query.CountAsync();
    
    // Apply pagination
    var patients = await query
        .OrderBy(p => p.FirstName)
        .Skip((request.PageNumber - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync();
    
    // 🎯 AutoMapper: Collection mapping
    var patientResponses = _mapper.Map<IEnumerable<PatientResponse>>(patients);
    
    return new PaginatedResponse<PatientResponse>
    {
        Data = patientResponses,
        PageNumber = request.PageNumber,
        PageSize = request.PageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
        HasPreviousPage = request.PageNumber > 1,
        HasNextPage = request.PageNumber < totalPages
    };
}
```

---

<a name="common-mistakes"></a>
## 6️⃣ Common Mistakes & Solutions

### **❌ Mistake 1: Forgetting to Include Navigation Properties**

```csharp
// ❌ Bad - Lazy loading disabled
var prescription = await _context.Prescriptions
    .FirstOrDefaultAsync(p => p.Id == id);

var response = _mapper.Map<PrescriptionDetailsResponse>(prescription);
// ❌ Doctor و Patient هيكونوا null!

// ✅ Good - Eager loading
var prescription = await _context.Prescriptions
    .Include(p => p.Doctor)
    .Include(p => p.Patient)
    .Include(p => p.PrescribedMedications)
        .ThenInclude(pm => pm.Medication)
    .FirstOrDefaultAsync(p => p.Id == id);

var response = _mapper.Map<PrescriptionDetailsResponse>(prescription);
// ✅ كل الـ data موجودة!
```

---

### **❌ Mistake 2: Mapping Null Objects**

```csharp
// ❌ Bad - No null check
var patient = await _repository.GetByIdAsync(id);
var response = _mapper.Map<PatientResponse>(patient);
// ❌ لو patient = null هيرمي exception!

// ✅ Good - Null check
var patient = await _repository.GetByIdAsync(id);

if (patient == null)
    throw new NotFoundException("Patient not found");

var response = _mapper.Map<PatientResponse>(patient);
// ✅ Safe!
```

---

### **❌ Mistake 3: Over-fetching Data**

```csharp
// ❌ Bad - Loading everything
var patients = await _context.Patients
    .Include(p => p.Address)
    .Include(p => p.MedicalHistory)
    .Include(p => p.Appointments)
    .ToListAsync();

var response = _mapper.Map<List<PatientBasicResponse>>(patients);
// ❌ جبنا data كتير مش محتاجينها!

// ✅ Good - ProjectTo
var response = await _context.Patients
    .ProjectTo<PatientBasicResponse>(_mapper.ConfigurationProvider)
    .ToListAsync();
// ✅ SQL بيجيب بس الـ columns المطلوبة!
```

---

### **❌ Mistake 4: Not Using Conditional Mapping for Updates**

```csharp
// ❌ Bad - All properties updated (even nulls)
CreateMap<UpdatePatientRequest, Patient>();

// When you do:
_mapper.Map(request, patient);
// كل الـ properties بتتحدث حتى لو null!

// ✅ Good - Conditional mapping
CreateMap<UpdatePatientRequest, Patient>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

// When you do:
_mapper.Map(request, patient);
// بس الـ non-null properties بتتحدث!
```

---

## 🎯 الخلاصة

### **Key Takeaways:**

1. ✅ **Always use Eager Loading** للـ navigation properties
2. ✅ **Always check for null** قبل الـ mapping
3. ✅ **Use ProjectTo** للـ queries عشان الـ performance
4. ✅ **Use Conditional Mapping** للـ partial updates
5. ✅ **Organize mappings** في profiles منفصلة
6. ✅ **Test your mappings** مع unit tests

---

**تم إعداد هذه الأمثلة من ShurYan Backend Project** 🚀
