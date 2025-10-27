# Patient Appointment Endpoints Documentation

## Overview
Comprehensive REST API endpoints for patient appointment management in the ShurYan Healthcare system. All endpoints require **Patient role authentication** via JWT token.

---

## 🔐 Authentication
All endpoints require:
- **Authorization Header**: `Bearer {JWT_TOKEN}`
- **Role**: `Patient`
- Patients can only access their own appointment data

---

## 📋 Endpoints Summary

### Base URL: `/api/patients`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/me/appointments` | Get all appointments for authenticated patient |
| GET | `/{patientId}/appointments` | Get all appointments for specific patient |
| GET | `/appointments/{appointmentId}` | Get specific appointment by ID |
| POST | `/appointments` | Create new appointment |
| PUT | `/appointments/{appointmentId}` | Update appointment |
| POST | `/appointments/{appointmentId}/cancel` | Cancel appointment |
| POST | `/appointments/{appointmentId}/reschedule` | Reschedule appointment |
| DELETE | `/appointments/{appointmentId}` | Delete appointment (soft delete) |
| GET | `/me/appointments/upcoming` | Get upcoming appointments |
| GET | `/me/appointments/past` | Get past appointments |
| GET | `/me/appointments/status/{status}` | Get appointments by status |
| GET | `/me/appointments/count` | Get total appointments count |
| GET | `/appointments/check-availability` | Check doctor time slot availability |
| GET | `/me/appointments/date-range` | Get appointments by date range |

---

## 📖 Detailed Endpoint Documentation

### 1. Get My Appointments
Get all appointments for the currently authenticated patient.

**Endpoint**: `GET /api/patients/me/appointments`

**Authorization**: Required (Patient role)

**Response**: `200 OK`
```json
[
  {
    "id": "guid",
    "patientId": "guid",
    "doctorId": "guid",
    "scheduledStartTime": "2024-10-25T10:00:00Z",
    "scheduledEndTime": "2024-10-25T10:30:00Z",
    "consultationType": 1,
    "consultationFee": 200.00,
    "sessionDurationMinutes": 30,
    "status": 1,
    "cancellationReason": null,
    "cancelledAt": null,
    "patient": { ... },
    "doctor": { ... },
    "consultationRecord": null
  }
]
```

---

### 2. Get Specific Appointment
Get a specific appointment by ID (with authorization check).

**Endpoint**: `GET /api/patients/appointments/{appointmentId}`

**Parameters**:
- `appointmentId` (path, required): Appointment GUID

**Response**: `200 OK` | `404 Not Found` | `403 Forbidden`

---

### 3. Create Appointment
Book a new appointment with a doctor.

**Endpoint**: `POST /api/patients/appointments`

**Request Body**:
```json
{
  "patientId": "guid",
  "doctorId": "guid",
  "scheduledStartTime": "2024-10-25T10:00:00Z",
  "scheduledEndTime": "2024-10-25T10:30:00Z",
  "consultationType": 1
}
```

**Validation**:
- ✅ Patient must exist
- ✅ Doctor must exist
- ✅ End time > Start time
- ✅ Appointment must be in the future
- ✅ No time slot conflicts
- ✅ Doctor offers the consultation type

**Response**: `201 Created`
```json
{
  "id": "guid",
  "patientId": "guid",
  "doctorId": "guid",
  "scheduledStartTime": "2024-10-25T10:00:00Z",
  "scheduledEndTime": "2024-10-25T10:30:00Z",
  "consultationType": 1,
  "consultationFee": 200.00,
  "sessionDurationMinutes": 30,
  "status": 1,
  "createdAt": "2024-10-23T08:00:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: Validation failed, time slot conflict, or invalid data
- `403 Forbidden`: Trying to create appointment for another patient
- `404 Not Found`: Patient or doctor not found

---

### 4. Update Appointment
Update appointment time slot (only for confirmed appointments).

**Endpoint**: `PUT /api/patients/appointments/{appointmentId}`

**Request Body**:
```json
{
  "scheduledStartTime": "2024-10-25T11:00:00Z",
  "scheduledEndTime": "2024-10-25T11:30:00Z"
}
```

**Business Rules**:
- Only confirmed appointments can be updated
- New time must be in the future
- No conflicts with other appointments

**Response**: `200 OK` | `400 Bad Request` | `403 Forbidden` | `404 Not Found`

---

### 5. Cancel Appointment
Cancel an appointment with a reason.

**Endpoint**: `POST /api/patients/appointments/{appointmentId}/cancel`

**Request Body**:
```json
{
  "cancellationReason": "Personal emergency"
}
```

**Validation**:
- Reason: 5-500 characters (required)

**Business Rules**:
- Cannot cancel completed appointments
- Cannot cancel already cancelled appointments
- ⚠️ Warning if cancelling < 24 hours before appointment

**Response**: `200 OK`
```json
{
  "id": "guid",
  "status": 6,
  "cancellationReason": "Personal emergency",
  "cancelledAt": "2024-10-23T08:30:00Z"
}
```

---

### 6. Reschedule Appointment
Move appointment to a new time slot.

**Endpoint**: `POST /api/patients/appointments/{appointmentId}/reschedule`

**Request Body**:
```json
{
  "newScheduledStartTime": "2024-10-26T14:00:00Z",
  "newScheduledEndTime": "2024-10-26T14:30:00Z"
}
```

**Business Rules**:
- Cannot reschedule completed or cancelled appointments
- New time must be in the future
- No conflicts with doctor's schedule

**Response**: `200 OK` | `400 Bad Request` | `403 Forbidden` | `404 Not Found`

---

### 7. Delete Appointment
Soft delete an appointment (marks as cancelled).

**Endpoint**: `DELETE /api/patients/appointments/{appointmentId}`

**Response**: `204 No Content` | `403 Forbidden` | `404 Not Found`

---

### 8. Get Upcoming Appointments
Get all future appointments for authenticated patient.

**Endpoint**: `GET /api/patients/me/appointments/upcoming`

**Response**: `200 OK`
```json
[
  {
    "id": "guid",
    "scheduledStartTime": "2024-10-25T10:00:00Z",
    "status": 1,
    "doctor": {
      "id": "guid",
      "fullName": "Dr. Ahmed Hassan",
      "medicalSpecialty": 1
    }
  }
]
```

---

### 9. Get Past Appointments
Get all historical appointments for authenticated patient.

**Endpoint**: `GET /api/patients/me/appointments/past`

**Response**: `200 OK`

---

### 10. Get Appointments by Status
Filter appointments by status for authenticated patient.

**Endpoint**: `GET /api/patients/me/appointments/status/{status}`

**Parameters**:
- `status` (path, required): Appointment status enum
  - `1` = Confirmed
  - `2` = CheckedIn
  - `3` = InProgress
  - `4` = Completed
  - `5` = NoShow
  - `6` = Cancelled

**Response**: `200 OK`

---

### 11. Get Appointments Count
Get total number of appointments for authenticated patient.

**Endpoint**: `GET /api/patients/me/appointments/count`

**Response**: `200 OK`
```json
{
  "patientId": "guid",
  "appointmentsCount": 15
}
```

---

### 12. Check Time Slot Availability
Check if a doctor's time slot is available for booking.

**Endpoint**: `GET /api/patients/appointments/check-availability`

**Query Parameters**:
- `doctorId` (required): Doctor GUID
- `startTime` (required): Start datetime
- `endTime` (required): End datetime

**Example**:
```
GET /api/patients/appointments/check-availability?doctorId=123&startTime=2024-10-25T10:00:00Z&endTime=2024-10-25T10:30:00Z
```

**Response**: `200 OK`
```json
{
  "doctorId": "guid",
  "startTime": "2024-10-25T10:00:00Z",
  "endTime": "2024-10-25T10:30:00Z",
  "isAvailable": true
}
```

---

### 13. Get Appointments by Date Range
Get appointments within a specific date range.

**Endpoint**: `GET /api/patients/me/appointments/date-range`

**Query Parameters**:
- `startDate` (required): Start date
- `endDate` (required): End date

**Example**:
```
GET /api/patients/me/appointments/date-range?startDate=2024-10-01&endDate=2024-10-31
```

**Response**: `200 OK`

---

## 🔒 Authorization & Security

### Access Control Rules
1. **Patient can only access their own data**
   - Enforced via `IsAccessingOwnData()` helper method
   - Returns `403 Forbidden` if accessing other patient's data

2. **JWT Token Validation**
   - Token must contain valid `NameIdentifier` claim
   - Returns `401 Unauthorized` for invalid/missing tokens

3. **Role-Based Access**
   - All endpoints require `Patient` role
   - Enforced via `[Authorize(Roles = "Patient")]` attribute

---

## 📊 Appointment Status Flow

```
Confirmed (1)
    ↓
CheckedIn (2)
    ↓
InProgress (3)
    ↓
Completed (4)

OR

Confirmed (1) → Cancelled (6)
Confirmed (1) → NoShow (5)
```

---

## 🎯 Consultation Types

| Value | Description | Arabic |
|-------|-------------|--------|
| 1 | Regular | كشف عادي |
| 2 | FollowUp | إعادة |

---

## ⚠️ Business Rules & Validations

### Appointment Creation
- ✅ Patient and doctor must exist
- ✅ Appointment must be in the future
- ✅ End time must be after start time
- ✅ No time slot conflicts
- ✅ Doctor must offer the consultation type
- ✅ Consultation fee auto-calculated from doctor's rates

### Appointment Cancellation
- ✅ Cannot cancel completed appointments
- ✅ Cannot cancel already cancelled appointments
- ⚠️ Warning logged if cancelling < 24 hours before

### Appointment Rescheduling
- ✅ Cannot reschedule completed/cancelled appointments
- ✅ New time must be in the future
- ✅ No conflicts with new time slot

### Appointment Updates
- ✅ Only confirmed appointments can be updated
- ✅ Time validation applies

---

## 🧪 Example Usage Scenarios

### Scenario 1: Book an Appointment
```bash
# 1. Check availability
GET /api/patients/appointments/check-availability?doctorId={doctorId}&startTime=2024-10-25T10:00:00Z&endTime=2024-10-25T10:30:00Z

# 2. Create appointment
POST /api/patients/appointments
{
  "patientId": "{currentPatientId}",
  "doctorId": "{doctorId}",
  "scheduledStartTime": "2024-10-25T10:00:00Z",
  "scheduledEndTime": "2024-10-25T10:30:00Z",
  "consultationType": 1
}
```

### Scenario 2: Cancel an Appointment
```bash
POST /api/patients/appointments/{appointmentId}/cancel
{
  "cancellationReason": "Unable to attend due to work commitment"
}
```

### Scenario 3: View Upcoming Appointments
```bash
GET /api/patients/me/appointments/upcoming
```

---

## 🐛 Error Handling

### Common Error Responses

**400 Bad Request**
```json
{
  "message": "The selected time slot is not available. Please choose another time."
}
```

**401 Unauthorized**
```json
{
  "message": "Invalid or missing authentication token"
}
```

**403 Forbidden**
```json
{
  "message": "Access denied"
}
```

**404 Not Found**
```json
{
  "message": "Appointment with ID {id} not found"
}
```

**500 Internal Server Error**
```json
{
  "message": "An error occurred while creating the appointment",
  "details": "..."
}
```

---

## 📝 Notes

1. **Soft Deletes**: Deleted appointments are marked as cancelled and `IsDeleted = true`
2. **Audit Trail**: All appointments track `CreatedAt`, `UpdatedAt`, `CancelledAt`
3. **Time Zones**: All times are in UTC
4. **Consultation Fees**: Auto-calculated from doctor's consultation type rates
5. **Session Duration**: Auto-calculated from time difference

---

## 🚀 Future Enhancements

- [ ] Appointment reminders (email/SMS)
- [ ] Recurring appointments
- [ ] Video consultation links
- [ ] Payment integration
- [ ] Appointment ratings/reviews
- [ ] Waitlist functionality
- [ ] Multi-language support for error messages

---

## 📞 Support

For issues or questions, contact the development team or refer to the main API documentation.
