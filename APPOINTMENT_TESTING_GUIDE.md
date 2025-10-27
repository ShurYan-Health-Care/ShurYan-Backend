# Appointment Endpoints Testing Guide

## Quick Test Scenarios for Patient Appointments

### Prerequisites
- Valid JWT token with Patient role
- Patient ID from token
- Valid Doctor ID
- Base URL: `https://localhost:7xxx/api/patients`

---

## 🧪 Test Scenarios

### 1. Complete Appointment Booking Flow

```http
### Step 1: Check doctor availability
GET {{baseUrl}}/appointments/check-availability?doctorId={{doctorId}}&startTime=2024-10-25T10:00:00Z&endTime=2024-10-25T10:30:00Z
Authorization: Bearer {{token}}

### Step 2: Create appointment
POST {{baseUrl}}/appointments
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "patientId": "{{patientId}}",
  "doctorId": "{{doctorId}}",
  "scheduledStartTime": "2024-10-25T10:00:00Z",
  "scheduledEndTime": "2024-10-25T10:30:00Z",
  "consultationType": 1
}

### Step 3: View created appointment
GET {{baseUrl}}/appointments/{{appointmentId}}
Authorization: Bearer {{token}}
```

---

### 2. View My Appointments

```http
### Get all my appointments
GET {{baseUrl}}/me/appointments
Authorization: Bearer {{token}}

### Get upcoming appointments
GET {{baseUrl}}/me/appointments/upcoming
Authorization: Bearer {{token}}

### Get past appointments
GET {{baseUrl}}/me/appointments/past
Authorization: Bearer {{token}}

### Get appointments count
GET {{baseUrl}}/me/appointments/count
Authorization: Bearer {{token}}
```

---

### 3. Filter Appointments

```http
### Get confirmed appointments
GET {{baseUrl}}/me/appointments/status/1
Authorization: Bearer {{token}}

### Get cancelled appointments
GET {{baseUrl}}/me/appointments/status/6
Authorization: Bearer {{token}}

### Get appointments by date range
GET {{baseUrl}}/me/appointments/date-range?startDate=2024-10-01&endDate=2024-10-31
Authorization: Bearer {{token}}
```

---

### 4. Modify Appointments

```http
### Update appointment time
PUT {{baseUrl}}/appointments/{{appointmentId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "scheduledStartTime": "2024-10-25T11:00:00Z",
  "scheduledEndTime": "2024-10-25T11:30:00Z"
}

### Reschedule appointment
POST {{baseUrl}}/appointments/{{appointmentId}}/reschedule
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "newScheduledStartTime": "2024-10-26T14:00:00Z",
  "newScheduledEndTime": "2024-10-26T14:30:00Z"
}

### Cancel appointment
POST {{baseUrl}}/appointments/{{appointmentId}}/cancel
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "cancellationReason": "Personal emergency - unable to attend"
}

### Delete appointment
DELETE {{baseUrl}}/appointments/{{appointmentId}}
Authorization: Bearer {{token}}
```

---

## 🔍 Expected Results

### ✅ Success Cases

| Test | Expected Status | Expected Response |
|------|----------------|-------------------|
| Create valid appointment | 201 Created | Appointment object with ID |
| Get my appointments | 200 OK | Array of appointments |
| Check availability (free slot) | 200 OK | `isAvailable: true` |
| Cancel appointment | 200 OK | Updated appointment with status 6 |
| Reschedule appointment | 200 OK | Updated appointment with new times |

### ❌ Error Cases

| Test | Expected Status | Expected Message |
|------|----------------|------------------|
| Create appointment in past | 400 Bad Request | "Appointment must be scheduled for a future date" |
| Create with conflicting time | 400 Bad Request | "The selected time slot is not available" |
| Access other patient's appointment | 403 Forbidden | Access denied |
| Cancel completed appointment | 400 Bad Request | "Cannot cancel a completed appointment" |
| Invalid appointment ID | 404 Not Found | "Appointment with ID ... not found" |
| Missing auth token | 401 Unauthorized | "Invalid or missing authentication token" |

---

## 🎯 Test Data Setup

### Sample Request Bodies

**Valid Appointment Creation**
```json
{
  "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "doctorId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "scheduledStartTime": "2024-12-25T10:00:00Z",
  "scheduledEndTime": "2024-12-25T10:30:00Z",
  "consultationType": 1
}
```

**Valid Cancellation**
```json
{
  "cancellationReason": "Family emergency - need to reschedule"
}
```

**Valid Reschedule**
```json
{
  "newScheduledStartTime": "2024-12-26T14:00:00Z",
  "newScheduledEndTime": "2024-12-26T14:30:00Z"
}
```

---

## 🐛 Common Issues & Solutions

### Issue 1: 403 Forbidden
**Cause**: Trying to access another patient's data
**Solution**: Ensure `patientId` in request matches authenticated user's ID

### Issue 2: 400 Bad Request - Time Conflict
**Cause**: Doctor already has appointment at that time
**Solution**: Use check-availability endpoint first

### Issue 3: 401 Unauthorized
**Cause**: Missing or invalid JWT token
**Solution**: Ensure Authorization header is set correctly

### Issue 4: 400 Bad Request - Past Date
**Cause**: Trying to book appointment in the past
**Solution**: Use future dates only

### Issue 5: 400 Bad Request - Invalid Time Range
**Cause**: End time before start time
**Solution**: Ensure endTime > startTime

---

## 📊 Status Code Reference

| Code | Meaning | When It Occurs |
|------|---------|----------------|
| 200 | OK | Successful GET/PUT/POST |
| 201 | Created | Successful appointment creation |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation error, business rule violation |
| 401 | Unauthorized | Missing/invalid token |
| 403 | Forbidden | Accessing other patient's data |
| 404 | Not Found | Appointment/Patient/Doctor not found |
| 500 | Internal Server Error | Unexpected server error |

---

## 🔐 Authorization Testing

### Test 1: Valid Patient Access
```http
GET {{baseUrl}}/me/appointments
Authorization: Bearer {{validPatientToken}}
# Expected: 200 OK
```

### Test 2: No Token
```http
GET {{baseUrl}}/me/appointments
# Expected: 401 Unauthorized
```

### Test 3: Wrong Role (Doctor Token)
```http
GET {{baseUrl}}/me/appointments
Authorization: Bearer {{doctorToken}}
# Expected: 403 Forbidden
```

### Test 4: Access Other Patient's Data
```http
GET {{baseUrl}}/appointments/{{otherPatientAppointmentId}}
Authorization: Bearer {{patientToken}}
# Expected: 403 Forbidden
```

---

## 🧩 Integration Test Checklist

- [ ] Create appointment with valid data
- [ ] Create appointment with past date (should fail)
- [ ] Create appointment with conflicting time (should fail)
- [ ] Get all appointments for patient
- [ ] Get specific appointment by ID
- [ ] Update appointment time
- [ ] Cancel appointment with reason
- [ ] Reschedule appointment
- [ ] Delete appointment
- [ ] Get upcoming appointments
- [ ] Get past appointments
- [ ] Filter by status
- [ ] Filter by date range
- [ ] Check time slot availability
- [ ] Get appointments count
- [ ] Test authorization (access own data only)
- [ ] Test with missing token (401)
- [ ] Test with wrong role (403)

---

## 📝 Postman Collection Variables

```json
{
  "baseUrl": "https://localhost:7001/api/patients",
  "token": "your-jwt-token-here",
  "patientId": "your-patient-id",
  "doctorId": "valid-doctor-id",
  "appointmentId": "created-appointment-id"
}
```

---

## 🚀 Performance Testing

### Load Test Scenarios
1. **Concurrent Bookings**: 100 patients booking at same time
2. **Availability Checks**: 1000 requests/second
3. **List Appointments**: 500 requests/second
4. **Cancel Operations**: 200 requests/second

### Expected Response Times
- GET endpoints: < 200ms
- POST/PUT endpoints: < 500ms
- DELETE endpoints: < 300ms

---

## 📞 Debugging Tips

1. **Check Logs**: Look for detailed error messages in application logs
2. **Validate Token**: Decode JWT to verify claims
3. **Check Database**: Verify appointment exists and status is correct
4. **Time Zones**: Ensure all times are in UTC
5. **Business Rules**: Review validation rules in service layer

---

## ✅ Success Criteria

All tests pass when:
- ✅ Patients can create appointments for future dates
- ✅ Patients can view only their own appointments
- ✅ Patients can cancel/reschedule confirmed appointments
- ✅ Time conflicts are properly detected
- ✅ Authorization checks prevent unauthorized access
- ✅ All validation rules are enforced
- ✅ Proper error messages are returned
- ✅ Audit trail is maintained (timestamps)
