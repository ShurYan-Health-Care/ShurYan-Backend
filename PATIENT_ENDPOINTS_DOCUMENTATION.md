# Patient Endpoints Documentation

This document provides comprehensive documentation for all 30 patient endpoints implemented in the ShurYan Health Care API.

## Base URL
```
/api/patients
```

---

## 1. Patient Profile Management (5 Endpoints)

### 1.1 Get Patient by ID
**Endpoint:** `GET /api/patients/{id}`  
**Authorization:** Admin, Doctor  
**Description:** Retrieve full patient details by ID

**Response:** `200 OK`
```json
{
  "id": "guid",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phoneNumber": "string",
  "birthDate": "datetime",
  "gender": "Male/Female",
  "profileImageUrl": "string",
  "address": { ... },
  "medicalHistory": [ ... ]
}
```

---

### 1.2 Get Patient by Email
**Endpoint:** `GET /api/patients/email/{email}`  
**Authorization:** Admin, Doctor  
**Description:** Search for a patient using their email address

**Response:** `200 OK` - Same as Get Patient by ID

---

### 1.3 Delete Patient Account
**Endpoint:** `DELETE /api/patients/{id}`  
**Authorization:** Admin only  
**Description:** Permanently delete a patient account from the system

**Response:** `200 OK`
```json
{
  "message": "Patient deleted successfully"
}
```

---

### 1.4 Get Current Patient Profile
**Endpoint:** `GET /api/patients/me`  
**Authorization:** Patient (authenticated)  
**Description:** Get the authenticated patient's own profile based on JWT token

**Response:** `200 OK` - Patient profile with full details

---

### 1.5 Update Current Patient Profile
**Endpoint:** `PUT /api/patients/me`  
**Authorization:** Patient (authenticated)  
**Description:** Update the authenticated patient's own profile information

**Request Body:**
```json
{
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string",
  "birthDate": "datetime",
  "gender": "Male/Female",
  "profileImageUrl": "string",
  "address": {
    "street": "string",
    "city": "string",
    "governorate": "string",
    "postalCode": "string"
  }
}
```

**Response:** `200 OK` - Updated patient profile

---

## 2. Medical History Management (4 Endpoints)

### 2.1 Get Patient Medical History
**Endpoint:** `GET /api/patients/{id}/medical-history`  
**Authorization:** Patient (own only), Doctor, Admin  
**Description:** Retrieve all medical history items for a patient (allergies, chronic diseases, past surgeries, etc.)

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "type": "Allergy/ChronicDisease/Surgery/Medication",
    "title": "string",
    "description": "string",
    "diagnosedDate": "datetime",
    "notes": "string"
  }
]
```

---

### 2.2 Add Medical History Item
**Endpoint:** `POST /api/patients/{id}/medical-history`  
**Authorization:** Patient (own only), Doctor  
**Description:** Add a new medical history item (e.g., newly discovered allergy, diagnosed chronic disease)

**Request Body:**
```json
{
  "type": "Allergy/ChronicDisease/Surgery/Medication",
  "title": "string",
  "description": "string",
  "diagnosedDate": "datetime",
  "notes": "string"
}
```

**Response:** `201 Created` - Created medical history item

---

### 2.3 Update Medical History Item
**Endpoint:** `PUT /api/patients/{id}/medical-history/{historyId}`  
**Authorization:** Patient (own only), Doctor  
**Description:** Update an existing medical history item

**Request Body:**
```json
{
  "title": "string",
  "description": "string",
  "notes": "string"
}
```

**Response:** `200 OK` - Updated medical history item

---

### 2.4 Delete Medical History Item
**Endpoint:** `DELETE /api/patients/{id}/medical-history/{historyId}`  
**Authorization:** Patient (own only), Doctor, Admin  
**Description:** Delete a medical history item (use with caution as medical history is sensitive)

**Response:** `200 OK`
```json
{
  "message": "Medical history item deleted successfully"
}
```

---

## 3. Appointment Management (6 Endpoints)

### 3.1 Get All Patient Appointments
**Endpoint:** `GET /api/patients/{id}/appointments`  
**Authorization:** Patient (own only), Doctor, Admin  
**Description:** Retrieve all appointments (past and upcoming) for a patient

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "patientId": "guid",
    "doctorId": "guid",
    "scheduledStartTime": "datetime",
    "scheduledEndTime": "datetime",
    "consultationType": "InPerson/Online",
    "consultationFee": 0.00,
    "status": "Scheduled/Confirmed/Completed/Cancelled",
    "doctor": { ... },
    "consultationRecord": { ... }
  }
]
```

---

### 3.2 Get Specific Appointment Details
**Endpoint:** `GET /api/patients/{id}/appointments/{appointmentId}`  
**Authorization:** Patient (own only), Doctor, Admin  
**Description:** Get detailed information about a specific appointment

**Response:** `200 OK` - Single appointment object with full details

---

### 3.3 Book New Appointment
**Endpoint:** `POST /api/patients/{id}/appointments`  
**Authorization:** Patient (authenticated)  
**Description:** Book a new appointment with a doctor

**Request Body:**
```json
{
  "doctorId": "guid",
  "scheduledStartTime": "datetime",
  "scheduledEndTime": "datetime",
  "consultationType": "InPerson/Online"
}
```

**Response:** `201 Created` - Created appointment details

**Note:** Currently returns 501 - Implementation pending with AppointmentService

---

### 3.4 Cancel Appointment
**Endpoint:** `PUT /api/patients/{id}/appointments/{appointmentId}/cancel`  
**Authorization:** Patient (authenticated)  
**Description:** Cancel an existing appointment

**Request Body:**
```json
{
  "cancellationReason": "string"
}
```

**Response:** `200 OK` - Updated appointment with cancelled status

**Note:** Currently returns 501 - Implementation pending with AppointmentService

---

### 3.5 Get Upcoming Appointments
**Endpoint:** `GET /api/patients/{id}/appointments/upcoming`  
**Authorization:** Patient (own only), Doctor, Admin  
**Description:** Get only future appointments to prepare for upcoming consultations

**Response:** `200 OK` - Array of upcoming appointments

---

### 3.6 Get Past Appointments
**Endpoint:** `GET /api/patients/{id}/appointments/past`  
**Authorization:** Patient (own only), Doctor, Admin  
**Description:** Get historical appointments to review past consultations and prescriptions

**Response:** `200 OK` - Array of past appointments

---

## 4. Prescription Management (2 Endpoints)

### 4.1 Get All Patient Prescriptions
**Endpoint:** `GET /api/patients/{id}/prescriptions`  
**Authorization:** Patient (own only), Doctor, Pharmacy, Admin  
**Description:** Retrieve all prescriptions (current and historical) for a patient

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "appointmentId": "guid",
    "doctorId": "guid",
    "patientId": "guid",
    "prescriptionDate": "datetime",
    "expiryDate": "datetime",
    "diagnosis": "string",
    "medications": [ ... ],
    "digitalSignature": "string"
  }
]
```

---

### 4.2 Get Specific Prescription Details
**Endpoint:** `GET /api/patients/{id}/prescriptions/{prescriptionId}`  
**Authorization:** Patient (own only), Doctor, Pharmacy, Admin  
**Description:** Get detailed information about a specific prescription including medications, dosages, and instructions

**Response:** `200 OK` - Single prescription object with full details

---

## 5. Laboratory Orders & Prescriptions (6 Endpoints)

### 5.1 Get All Lab Orders
**Endpoint:** `GET /api/patients/{id}/lab-orders`  
**Authorization:** Patient (own only), Doctor, Laboratory, Admin  
**Description:** Retrieve all lab orders (pending and completed) for a patient

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "labPrescriptionId": "guid",
    "laboratoryId": "guid",
    "patientId": "guid",
    "status": "Pending/Confirmed/InProgress/Completed/Cancelled",
    "sampleCollectionType": "AtLab/HomeCollection",
    "testsTotalCost": 0.00,
    "sampleCollectionDeliveryCost": 0.00,
    "confirmedByLabAt": "datetime"
  }
]
```

---

### 5.2 Get Specific Lab Order Details
**Endpoint:** `GET /api/patients/{id}/lab-orders/{orderId}`  
**Authorization:** Patient (own only), Doctor, Laboratory, Admin  
**Description:** Get detailed information about a specific lab order including tests and results

**Response:** `200 OK` - Single lab order with full details

---

### 5.3 Get Pending Lab Orders
**Endpoint:** `GET /api/patients/{id}/lab-orders/pending`  
**Authorization:** Patient (own only), Doctor, Laboratory, Admin  
**Description:** Get lab orders that are waiting to be completed

**Response:** `200 OK` - Array of pending lab orders

---

### 5.4 Get Completed Lab Orders
**Endpoint:** `GET /api/patients/{id}/lab-orders/completed`  
**Authorization:** Patient (own only), Doctor, Laboratory, Admin  
**Description:** Get completed lab orders with downloadable results

**Response:** `200 OK` - Array of completed lab orders

---

### 5.5 Get All Lab Prescriptions
**Endpoint:** `GET /api/patients/{id}/lab-prescriptions`  
**Authorization:** Patient (own only), Doctor, Laboratory, Admin  
**Description:** Get all lab test prescriptions written by doctors

**Response:** `200 OK` - Array of lab prescriptions

**Note:** Currently returns 501 - Implementation pending with LabPrescriptionService

---

### 5.6 Get Specific Lab Prescription
**Endpoint:** `GET /api/patients/{id}/lab-prescriptions/{prescriptionId}`  
**Authorization:** Patient (own only), Doctor, Laboratory, Admin  
**Description:** Get detailed information about a specific lab prescription including required tests and preparation instructions

**Response:** `200 OK` - Single lab prescription with full details

**Note:** Currently returns 501 - Implementation pending with LabPrescriptionService

---

## 6. Pharmacy Orders (3 Endpoints)

### 6.1 Get All Pharmacy Orders
**Endpoint:** `GET /api/patients/{id}/pharmacy-orders`  
**Authorization:** Patient (own only), Pharmacy, Admin  
**Description:** Retrieve all medication orders from pharmacies

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "orderNumber": "string",
    "status": "Pending/Confirmed/Preparing/Dispatched/Delivered/Cancelled",
    "totalCost": 0.00,
    "deliveryFee": 0.00,
    "deliveryType": "Pickup/Delivery",
    "estimatedDeliveryTime": "datetime",
    "patientId": "guid",
    "pharmacyId": "guid",
    "prescriptionId": "guid"
  }
]
```

**Note:** Currently returns 501 - Implementation pending with PharmacyOrderService

---

### 6.2 Create Pharmacy Order
**Endpoint:** `POST /api/patients/{id}/pharmacy-orders`  
**Authorization:** Patient (authenticated)  
**Description:** Order medication delivery from a pharmacy

**Request Body:**
```json
{
  "pharmacyId": "guid",
  "prescriptionId": "guid",
  "deliveryType": "Pickup/Delivery",
  "deliveryAddressId": "guid",
  "deliveryNotes": "string"
}
```

**Response:** `201 Created` - Created pharmacy order

**Note:** Currently returns 501 - Implementation pending with PharmacyOrderService

---

### 6.3 Get Pharmacy Orders by Status
**Endpoint:** `GET /api/patients/{id}/pharmacy-orders/status/{status}`  
**Authorization:** Patient (own only), Pharmacy, Admin  
**Description:** Filter pharmacy orders by status (Pending, Preparing, Delivered, etc.)

**Response:** `200 OK` - Array of pharmacy orders matching the status

**Note:** Currently returns 501 - Implementation pending with PharmacyOrderService

---

## 7. Reviews Management (3 Endpoints)

### 7.1 Create Doctor Review
**Endpoint:** `POST /api/patients/{id}/doctor-reviews`  
**Authorization:** Patient (authenticated)  
**Description:** Write a review for a doctor after consultation

**Request Body:**
```json
{
  "appointmentId": "guid",
  "overallSatisfaction": 1-5,
  "waitingTime": 1-5,
  "communicationQuality": 1-5,
  "clinicCleanliness": 1-5,
  "valueForMoney": 1-5,
  "comment": "string",
  "isAnonymous": false
}
```

**Response:** `201 Created` - Created doctor review

**Note:** Currently returns 501 - Implementation pending with ReviewService

---

### 7.2 Create Laboratory Review
**Endpoint:** `POST /api/patients/{id}/laboratory-reviews`  
**Authorization:** Patient (authenticated)  
**Description:** Write a review for a laboratory after receiving results

**Request Body:**
```json
{
  "labOrderId": "guid",
  "overallSatisfaction": 1-5,
  "resultAccuracy": 1-5,
  "deliverySpeed": 1-5,
  "serviceQuality": 1-5,
  "cleanliness": 1-5,
  "valueForMoney": 1-5,
  "comment": "string",
  "isAnonymous": false
}
```

**Response:** `201 Created` - Created laboratory review

**Note:** Currently returns 501 - Implementation pending with ReviewService

---

### 7.3 Create Pharmacy Review
**Endpoint:** `POST /api/patients/{id}/pharmacy-reviews`  
**Authorization:** Patient (authenticated)  
**Description:** Write a review for a pharmacy after receiving order

**Request Body:**
```json
{
  "pharmacyOrderId": "guid",
  "overallSatisfaction": 1-5,
  "medicationAvailability": 1-5,
  "serviceQuality": 1-5,
  "deliverySpeed": 1-5,
  "valueForMoney": 1-5,
  "comment": "string",
  "isAnonymous": false
}
```

**Response:** `201 Created` - Created pharmacy review

**Note:** Currently returns 501 - Implementation pending with ReviewService

---

## 8. Address Management (1 Endpoint)

### 8.1 Get Patient Address
**Endpoint:** `GET /api/patients/{id}/address`  
**Authorization:** Patient (own only), Doctor, Laboratory, Pharmacy, Admin  
**Description:** Retrieve patient's registered address for delivery purposes

**Response:** `200 OK`
```json
{
  "id": "guid",
  "street": "string",
  "city": "string",
  "governorate": "string",
  "postalCode": "string",
  "country": "string",
  "latitude": 0.0,
  "longitude": 0.0
}
```

---

## Authentication & Authorization

### JWT Token Required
All endpoints require a valid JWT token in the Authorization header:
```
Authorization: Bearer {token}
```

### Role-Based Access Control
- **Patient**: Can only access their own data (enforced by checking user ID from JWT)
- **Doctor**: Can access patient data for medical purposes
- **Laboratory**: Can access patient data related to lab orders
- **Pharmacy**: Can access patient data related to pharmacy orders
- **Admin**: Full access to all patient data

### Access Control Implementation
The controller implements role-based access control with additional checks:
- Patients attempting to access other patients' data receive `403 Forbidden`
- Unauthenticated requests receive `401 Unauthorized`
- Requests with insufficient permissions receive `403 Forbidden`

---

## Error Responses

### 400 Bad Request
```json
{
  "message": "Validation error message",
  "errors": { ... }
}
```

### 401 Unauthorized
```json
{
  "message": "User not authenticated"
}
```

### 403 Forbidden
```json
{
  "message": "Access denied"
}
```

### 404 Not Found
```json
{
  "message": "Resource not found"
}
```

### 501 Not Implemented
```json
{
  "message": "Feature will be implemented with [ServiceName]"
}
```

---

## Implementation Status

### ✅ Fully Implemented (17 endpoints)
- All Patient Profile Management endpoints (5)
- All Medical History Management endpoints (4)
- Get appointments endpoints (4)
- All Prescription Management endpoints (2)
- All Lab Orders query endpoints (4)
- Address Management endpoint (1)

### ⏳ Pending Service Implementation (13 endpoints)
These endpoints are defined but return 501 status until the corresponding services are implemented:
- Book Appointment
- Cancel Appointment
- Lab Prescriptions (2 endpoints)
- Pharmacy Orders (3 endpoints)
- Reviews (3 endpoints)

---

## Next Steps

To complete the implementation:

1. **Implement AppointmentService** for appointment booking and cancellation
2. **Implement LabPrescriptionService** for lab prescription management
3. **Implement PharmacyOrderService** for pharmacy order management
4. **Implement ReviewService** for review management
5. **Add validation middleware** for request DTOs
6. **Implement rate limiting** to prevent abuse
7. **Add comprehensive logging** for audit trails
8. **Write unit and integration tests** for all endpoints

---

## API Testing

You can test these endpoints using:
- **Swagger UI**: Available at `/swagger` when running in development
- **Postman**: Import the API collection
- **curl**: Command-line testing

Example curl request:
```bash
curl -X GET "https://api.shuryan.com/api/patients/me" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json"
```

---

**Last Updated:** October 20, 2025  
**API Version:** 1.0  
**Contact:** ShurYan Health Care Development Team
