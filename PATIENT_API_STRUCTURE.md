# Patient API Structure

## Visual Endpoint Tree

```
/api/patients
│
├── /{id}                                    [GET]    ✅ Get patient by ID (Admin, Doctor)
│   ├── /medical-history                     [GET]    ✅ Get medical history
│   │   ├── /                                [POST]   ✅ Add medical history item
│   │   └── /{historyId}                     [PUT]    ✅ Update medical history item
│   │       └──                              [DELETE] ✅ Delete medical history item
│   │
│   ├── /appointments                        [GET]    ✅ Get all appointments
│   │   ├── /                                [POST]   ⏳ Book new appointment
│   │   ├── /{appointmentId}                 [GET]    ✅ Get specific appointment
│   │   │   └── /cancel                      [PUT]    ⏳ Cancel appointment
│   │   ├── /upcoming                        [GET]    ✅ Get upcoming appointments
│   │   └── /past                            [GET]    ✅ Get past appointments
│   │
│   ├── /prescriptions                       [GET]    ✅ Get all prescriptions
│   │   └── /{prescriptionId}                [GET]    ✅ Get specific prescription
│   │
│   ├── /lab-orders                          [GET]    ✅ Get all lab orders
│   │   ├── /{orderId}                       [GET]    ✅ Get specific lab order
│   │   ├── /pending                         [GET]    ✅ Get pending lab orders
│   │   └── /completed                       [GET]    ✅ Get completed lab orders
│   │
│   ├── /lab-prescriptions                   [GET]    ⏳ Get all lab prescriptions
│   │   └── /{prescriptionId}                [GET]    ⏳ Get specific lab prescription
│   │
│   ├── /pharmacy-orders                     [GET]    ⏳ Get all pharmacy orders
│   │   ├── /                                [POST]   ⏳ Create pharmacy order
│   │   └── /status/{status}                 [GET]    ⏳ Get orders by status
│   │
│   ├── /doctor-reviews                      [POST]   ⏳ Create doctor review
│   ├── /laboratory-reviews                  [POST]   ⏳ Create laboratory review
│   ├── /pharmacy-reviews                    [POST]   ⏳ Create pharmacy review
│   │
│   ├── /address                             [GET]    ✅ Get patient address
│   │
│   └──                                      [DELETE] ✅ Delete patient (Admin only)
│
├── /email/{email}                           [GET]    ✅ Get patient by email (Admin, Doctor)
│
└── /me                                      [GET]    ✅ Get current patient profile
    └──                                      [PUT]    ✅ Update current patient profile
```

---

## Endpoint Grouping by Feature

### 🔐 Authentication & Profile
```
GET    /api/patients/me                    # Get own profile
PUT    /api/patients/me                    # Update own profile
GET    /api/patients/{id}                  # Get patient by ID (Admin/Doctor)
GET    /api/patients/email/{email}         # Get patient by email (Admin/Doctor)
DELETE /api/patients/{id}                  # Delete patient (Admin)
```

### 🏥 Medical History
```
GET    /api/patients/{id}/medical-history                # Get all medical history
POST   /api/patients/{id}/medical-history                # Add medical history item
PUT    /api/patients/{id}/medical-history/{historyId}    # Update medical history item
DELETE /api/patients/{id}/medical-history/{historyId}    # Delete medical history item
```

### 📅 Appointments
```
GET    /api/patients/{id}/appointments                   # Get all appointments
GET    /api/patients/{id}/appointments/{appointmentId}   # Get specific appointment
POST   /api/patients/{id}/appointments                   # Book appointment ⏳
PUT    /api/patients/{id}/appointments/{appointmentId}/cancel  # Cancel appointment ⏳
GET    /api/patients/{id}/appointments/upcoming          # Get upcoming appointments
GET    /api/patients/{id}/appointments/past              # Get past appointments
```

### 💊 Prescriptions
```
GET    /api/patients/{id}/prescriptions                  # Get all prescriptions
GET    /api/patients/{id}/prescriptions/{prescriptionId} # Get specific prescription
```

### 🔬 Laboratory
```
GET    /api/patients/{id}/lab-orders                     # Get all lab orders
GET    /api/patients/{id}/lab-orders/{orderId}           # Get specific lab order
GET    /api/patients/{id}/lab-orders/pending             # Get pending lab orders
GET    /api/patients/{id}/lab-orders/completed           # Get completed lab orders
GET    /api/patients/{id}/lab-prescriptions              # Get all lab prescriptions ⏳
GET    /api/patients/{id}/lab-prescriptions/{prescriptionId}  # Get specific lab prescription ⏳
```

### 💉 Pharmacy
```
GET    /api/patients/{id}/pharmacy-orders                # Get all pharmacy orders ⏳
POST   /api/patients/{id}/pharmacy-orders                # Create pharmacy order ⏳
GET    /api/patients/{id}/pharmacy-orders/status/{status}  # Get orders by status ⏳
```

### ⭐ Reviews
```
POST   /api/patients/{id}/doctor-reviews                 # Create doctor review ⏳
POST   /api/patients/{id}/laboratory-reviews             # Create laboratory review ⏳
POST   /api/patients/{id}/pharmacy-reviews               # Create pharmacy review ⏳
```

### 📍 Address
```
GET    /api/patients/{id}/address                        # Get patient address
```

---

## Data Flow Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Application                       │
│                    (Mobile App / Web Frontend)                   │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP Request + JWT Token
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API Gateway / Middleware                    │
│                  (Authentication, Authorization)                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      PatientsController                          │
│  • Route handling                                                │
│  • Authorization checks (Role + User ID validation)              │
│  • Request validation                                            │
│  • Response formatting                                           │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Service Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Patient      │  │ Appointment  │  │ Prescription │          │
│  │ Service      │  │ Service      │  │ Service      │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ LabOrder     │  │ PharmacyOrder│  │ Review       │          │
│  │ Service      │  │ Service      │  │ Service      │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│  • Business logic                                                │
│  • Data validation                                               │
│  • Transaction management                                        │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Repository Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Patient      │  │ Appointment  │  │ Prescription │          │
│  │ Repository   │  │ Repository   │  │ Repository   │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│  • Data access                                                   │
│  • Query building                                                │
│  • Entity mapping                                                │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Database (EF Core)                       │
│  • Patients                                                      │
│  • MedicalHistory                                                │
│  • Appointments                                                  │
│  • Prescriptions                                                 │
│  • LabOrders                                                     │
│  • PharmacyOrders                                                │
│  • Reviews                                                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## Authorization Matrix

| Endpoint Category | Patient (Own) | Patient (Other) | Doctor | Laboratory | Pharmacy | Admin |
|-------------------|---------------|-----------------|--------|------------|----------|-------|
| Profile Management | ✅ Read/Write | ❌ | ✅ Read | ❌ | ❌ | ✅ Full |
| Medical History | ✅ Full | ❌ | ✅ Full | ❌ | ❌ | ✅ Full |
| Appointments | ✅ Full | ❌ | ✅ Read | ❌ | ❌ | ✅ Full |
| Prescriptions | ✅ Read | ❌ | ✅ Full | ❌ | ✅ Read | ✅ Full |
| Lab Orders | ✅ Read | ❌ | ✅ Read | ✅ Full | ❌ | ✅ Full |
| Lab Prescriptions | ✅ Read | ❌ | ✅ Full | ✅ Read | ❌ | ✅ Full |
| Pharmacy Orders | ✅ Full | ❌ | ❌ | ❌ | ✅ Full | ✅ Full |
| Reviews | ✅ Write | ❌ | ❌ | ❌ | ❌ | ✅ Full |
| Address | ✅ Read | ❌ | ✅ Read | ✅ Read | ✅ Read | ✅ Full |

**Legend:**
- ✅ Full: Complete CRUD access
- ✅ Read: Read-only access
- ✅ Write: Create/Update access
- ❌ No access

---

## Request/Response Flow Example

### Example: Get Current Patient Profile

```
1. Client Request
   ┌─────────────────────────────────────────────┐
   │ GET /api/patients/me                        │
   │ Authorization: Bearer eyJhbGc...            │
   │ Content-Type: application/json              │
   └─────────────────────────────────────────────┘
                    ↓
2. Middleware Authentication
   ┌─────────────────────────────────────────────┐
   │ • Validate JWT token                        │
   │ • Extract user claims (ID, Role)            │
   │ • Set User.Identity                         │
   └─────────────────────────────────────────────┘
                    ↓
3. Controller Authorization
   ┌─────────────────────────────────────────────┐
   │ [Authorize(Roles = "Patient")]              │
   │ • Check if user has Patient role            │
   │ • Extract userId from claims                │
   └─────────────────────────────────────────────┘
                    ↓
4. Service Call
   ┌─────────────────────────────────────────────┐
   │ _patientService.GetCurrentPatientAsync(     │
   │     userId                                  │
   │ )                                           │
   └─────────────────────────────────────────────┘
                    ↓
5. Repository Query
   ┌─────────────────────────────────────────────┐
   │ • Query database for patient                │
   │ • Include related entities (Address, etc.)  │
   │ • Map to PatientResponse DTO                │
   └─────────────────────────────────────────────┘
                    ↓
6. Response
   ┌─────────────────────────────────────────────┐
   │ HTTP 200 OK                                 │
   │ {                                           │
   │   "id": "guid",                             │
   │   "firstName": "Ahmed",                     │
   │   "lastName": "Mohamed",                    │
   │   "email": "ahmed@example.com",             │
   │   "phoneNumber": "+201234567890",           │
   │   "birthDate": "1990-01-15",                │
   │   "gender": "Male",                         │
   │   "address": { ... },                       │
   │   "medicalHistory": [ ... ]                 │
   │ }                                           │
   └─────────────────────────────────────────────┘
```

---

## Error Handling Flow

```
Request → Controller → Service → Repository
                ↓         ↓         ↓
            Exception  Exception  Exception
                ↓         ↓         ↓
         Global Exception Handler
                ↓
         Format Error Response
                ↓
         Return to Client
```

### Common Error Scenarios

| Scenario | HTTP Status | Response |
|----------|-------------|----------|
| Invalid JWT token | 401 | `{ "message": "User not authenticated" }` |
| Patient accessing other's data | 403 | `{ "message": "Access denied" }` |
| Resource not found | 404 | `{ "message": "Patient not found" }` |
| Validation error | 400 | `{ "message": "...", "errors": {...} }` |
| Service not implemented | 501 | `{ "message": "Feature pending..." }` |
| Server error | 500 | `{ "message": "Internal server error" }` |

---

## Performance Considerations

### Implemented Optimizations
- ✅ Async/await for non-blocking I/O
- ✅ DTOs to prevent over-fetching
- ✅ Role-based authorization at controller level

### Recommended Optimizations
- 🔄 Add caching for frequently accessed data (patient profiles)
- 🔄 Implement pagination for list endpoints
- 🔄 Add response compression
- 🔄 Implement rate limiting
- 🔄 Add database query optimization (indexes)
- 🔄 Implement lazy loading for related entities

---

## Testing Strategy

### Unit Tests
```csharp
// Test authorization logic
[Fact]
public async Task GetPatientById_AsDoctor_ReturnsPatient()

// Test patient can only access own data
[Fact]
public async Task GetCurrentPatient_DifferentUserId_ReturnsForbidden()

// Test not found scenarios
[Fact]
public async Task GetPatientById_NonExistent_ReturnsNotFound()
```

### Integration Tests
```csharp
// Test full request/response cycle
[Fact]
public async Task GetPatientProfile_WithValidToken_Returns200()

// Test authorization
[Fact]
public async Task GetPatientById_WithoutToken_Returns401()
```

---

**Architecture Version:** 1.0  
**Last Updated:** October 20, 2025
