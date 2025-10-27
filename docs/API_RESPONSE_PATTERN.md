# 🎯 **ApiResponse<T> Pattern - Complete Guide**

## **Overview**

The `ApiResponse<T>` wrapper provides a **consistent, predictable response structure** for all API endpoints, making it significantly easier for frontend developers to handle responses.

---

## **📦 ApiResponse Structure**

```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }          // Quick success/failure check
    public string Message { get; set; }          // Human-readable message
    public T? Data { get; set; }                 // Actual response data
    public IEnumerable<string>? Errors { get; set; }  // Validation/error details
    public int? StatusCode { get; set; }         // HTTP status code
}
```

---

## **✅ Benefits for Frontend**

### **1. Consistent Response Structure**
Every endpoint returns the same structure - no guessing!

```typescript
// Frontend TypeScript Interface
interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
  statusCode?: number;
}
```

### **2. Easy Error Handling**
```typescript
// Single error handling pattern for ALL endpoints
async function fetchData<T>(url: string): Promise<T> {
  const response = await fetch(url);
  const result: ApiResponse<T> = await response.json();
  
  if (!result.isSuccess) {
    // Handle error consistently
    throw new Error(result.message);
  }
  
  return result.data!;
}
```

### **3. Built-in Validation Errors**
```typescript
// Validation errors are always in the same place
if (!result.isSuccess && result.errors) {
  result.errors.forEach(error => {
    showValidationError(error);
  });
}
```

---

## **🔧 Backend Implementation Patterns**

### **Pattern 1: Success Response**

```csharp
// Single item
return Ok(ApiResponse<PatientResponse>.Success(
    patient,
    "Patient retrieved successfully"
));

// Collection
return Ok(ApiResponse<IEnumerable<AppointmentResponse>>.Success(
    appointments,
    $"Retrieved {appointments.Count()} appointments"
));

// No data (operation success)
return Ok(ApiResponse<object>.Success(
    null,
    "Operation completed successfully"
));
```

### **Pattern 2: Validation Error (400)**

```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState.Values
        .SelectMany(v => v.Errors)
        .Select(e => e.ErrorMessage);
    
    return BadRequest(ApiResponse<object>.Failure(
        "Validation failed",
        errors,
        400
    ));
}
```

### **Pattern 3: Not Found (404)**

```csharp
if (patient == null)
{
    return NotFound(ApiResponse<object>.Failure(
        $"Patient with ID {patientId} not found",
        statusCode: 404
    ));
}
```

### **Pattern 4: Server Error (500)**

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error message");
    return StatusCode(500, ApiResponse<object>.Failure(
        "An unexpected error occurred",
        new[] { ex.Message },
        500
    ));
}
```

### **Pattern 5: Unauthorized (401)**

```csharp
if (currentPatientId == Guid.Empty)
{
    return Unauthorized(ApiResponse<object>.Failure(
        "Invalid or missing authentication token",
        statusCode: 401
    ));
}
```

### **Pattern 6: Forbidden (403)**

```csharp
if (!IsAccessingOwnData(patientId))
{
    return Forbid(ApiResponse<object>.Failure(
        "You don't have permission to access this resource",
        statusCode: 403
    ));
}
```

---

## **📝 Complete Endpoint Examples**

### **Example 1: GET Endpoint (Single Item)**

```csharp
[HttpGet("me")]
[ProducesResponseType(typeof(ApiResponse<PatientResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<PatientResponse>>> GetMyProfile()
{
    var currentPatientId = GetCurrentPatientId();
    _logger.LogInformation("Get profile for patient: {PatientId}", currentPatientId);

    try
    {
        var patient = await _patientService.GetPatientByIdAsync(currentPatientId);
        if (patient == null)
        {
            _logger.LogWarning("Patient not found: {PatientId}", currentPatientId);
            return NotFound(ApiResponse<object>.Failure(
                $"Patient with ID {currentPatientId} not found",
                statusCode: 404
            ));
        }

        _logger.LogInformation("Profile retrieved successfully");
        return Ok(ApiResponse<PatientResponse>.Success(
            patient,
            "Profile retrieved successfully"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving profile");
        return StatusCode(500, ApiResponse<object>.Failure(
            "An unexpected error occurred",
            new[] { ex.Message },
            500
        ));
    }
}
```

**Response Examples:**

**Success (200):**
```json
{
  "isSuccess": true,
  "message": "Profile retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com"
  },
  "errors": null,
  "statusCode": 200
}
```

**Not Found (404):**
```json
{
  "isSuccess": false,
  "message": "Patient with ID 123e4567-e89b-12d3-a456-426614174000 not found",
  "data": null,
  "errors": null,
  "statusCode": 404
}
```

**Server Error (500):**
```json
{
  "isSuccess": false,
  "message": "An unexpected error occurred",
  "data": null,
  "errors": [
    "Database connection timeout"
  ],
  "statusCode": 500
}
```

---

### **Example 2: GET Endpoint (Collection)**

```csharp
[HttpGet("me/appointments")]
[ProducesResponseType(typeof(ApiResponse<IEnumerable<AppointmentResponse>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponse>>>> GetMyAppointments()
{
    var currentPatientId = GetCurrentPatientId();
    _logger.LogInformation("Get appointments for patient: {PatientId}", currentPatientId);

    try
    {
        var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(currentPatientId);
        _logger.LogInformation("Retrieved {Count} appointments", appointments.Count());
        
        return Ok(ApiResponse<IEnumerable<AppointmentResponse>>.Success(
            appointments,
            $"Retrieved {appointments.Count()} appointments successfully"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving appointments");
        return StatusCode(500, ApiResponse<object>.Failure(
            "An unexpected error occurred while retrieving appointments",
            new[] { ex.Message },
            500
        ));
    }
}
```

**Response Example:**
```json
{
  "isSuccess": true,
  "message": "Retrieved 3 appointments successfully",
  "data": [
    {
      "id": "...",
      "patientId": "...",
      "doctorId": "...",
      "appointmentDate": "2025-10-25T10:00:00Z"
    }
  ],
  "errors": null,
  "statusCode": 200
}
```

---

### **Example 3: POST Endpoint (Create)**

```csharp
[HttpPost("me/appointments")]
[ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<AppointmentResponse>>> CreateAppointment(
    [FromBody] CreateAppointmentRequest request)
{
    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Invalid create appointment request");
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        return BadRequest(ApiResponse<object>.Failure(
            "Validation failed",
            errors,
            400
        ));
    }

    var currentPatientId = GetCurrentPatientId();
    _logger.LogInformation("Create appointment for patient: {PatientId}", currentPatientId);

    try
    {
        var appointment = await _appointmentService.CreateAppointmentAsync(request);
        _logger.LogInformation("Appointment created successfully: {AppointmentId}", appointment.Id);
        
        return CreatedAtAction(
            nameof(GetAppointmentById),
            new { appointmentId = appointment.Id },
            ApiResponse<AppointmentResponse>.Success(
                appointment,
                "Appointment created successfully",
                201
            )
        );
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Invalid operation");
        return BadRequest(ApiResponse<object>.Failure(
            ex.Message,
            statusCode: 400
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating appointment");
        return StatusCode(500, ApiResponse<object>.Failure(
            "An error occurred while creating the appointment",
            new[] { ex.Message },
            500
        ));
    }
}
```

**Response Examples:**

**Success (201):**
```json
{
  "isSuccess": true,
  "message": "Appointment created successfully",
  "data": {
    "id": "new-appointment-id",
    "patientId": "...",
    "appointmentDate": "2025-10-25T10:00:00Z"
  },
  "errors": null,
  "statusCode": 201
}
```

**Validation Error (400):**
```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Appointment date is required",
    "Doctor ID must be a valid GUID"
  ],
  "statusCode": 400
}
```

---

### **Example 4: PUT Endpoint (Update)**

```csharp
[HttpPut("me")]
[ProducesResponseType(typeof(ApiResponse<PatientResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<PatientResponse>>> UpdatePatient(
    [FromBody] UpdatePatientRequest request)
{
    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Invalid update request");
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        return BadRequest(ApiResponse<object>.Failure(
            "Validation failed",
            errors,
            400
        ));
    }

    var currentPatientId = GetCurrentPatientId();
    _logger.LogInformation("Update patient: {PatientId}", currentPatientId);

    try
    {
        var patient = await _patientService.UpdatePatientAsync(currentPatientId, request);
        _logger.LogInformation("Patient updated successfully");
        
        return Ok(ApiResponse<PatientResponse>.Success(
            patient,
            "Patient updated successfully"
        ));
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "Patient not found");
        return NotFound(ApiResponse<object>.Failure(
            ex.Message,
            statusCode: 404
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating patient");
        return StatusCode(500, ApiResponse<object>.Failure(
            "An error occurred while updating the patient",
            new[] { ex.Message },
            500
        ));
    }
}
```

---

### **Example 5: DELETE Endpoint**

```csharp
[HttpDelete("me/appointments/{appointmentId}")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<object>>> DeleteAppointment(Guid appointmentId)
{
    var currentPatientId = GetCurrentPatientId();
    _logger.LogInformation("Delete appointment: {AppointmentId}", appointmentId);

    try
    {
        var result = await _appointmentService.DeleteAppointmentAsync(appointmentId);
        if (!result)
        {
            _logger.LogWarning("Appointment not found: {AppointmentId}", appointmentId);
            return NotFound(ApiResponse<object>.Failure(
                $"Appointment with ID {appointmentId} not found",
                statusCode: 404
            ));
        }

        _logger.LogInformation("Appointment deleted successfully");
        return Ok(ApiResponse<object>.Success(
            null,
            "Appointment deleted successfully"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting appointment");
        return StatusCode(500, ApiResponse<object>.Failure(
            "An error occurred while deleting the appointment",
            new[] { ex.Message },
            500
        ));
    }
}
```

**Response Example:**
```json
{
  "isSuccess": true,
  "message": "Appointment deleted successfully",
  "data": null,
  "errors": null,
  "statusCode": 200
}
```

---

## **🎨 Frontend Integration Examples**

### **React/TypeScript Example**

```typescript
// api/types.ts
export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
  statusCode?: number;
}

// api/client.ts
export async function apiRequest<T>(
  url: string,
  options?: RequestInit
): Promise<T> {
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${getToken()}`,
      ...options?.headers,
    },
  });

  const result: ApiResponse<T> = await response.json();

  if (!result.isSuccess) {
    if (result.errors && result.errors.length > 0) {
      throw new ValidationError(result.message, result.errors);
    }
    throw new ApiError(result.message, result.statusCode || response.status);
  }

  return result.data!;
}

// Usage in components
export function usePatientProfile() {
  return useQuery({
    queryKey: ['patient', 'profile'],
    queryFn: () => apiRequest<PatientResponse>('/api/patients/me'),
  });
}

export function useUpdatePatient() {
  return useMutation({
    mutationFn: (data: UpdatePatientRequest) =>
      apiRequest<PatientResponse>('/api/patients/me', {
        method: 'PUT',
        body: JSON.stringify(data),
      }),
    onSuccess: (data) => {
      toast.success('Profile updated successfully!');
    },
    onError: (error: ApiError) => {
      if (error instanceof ValidationError) {
        error.errors.forEach(err => toast.error(err));
      } else {
        toast.error(error.message);
      }
    },
  });
}
```

### **Angular Example**

```typescript
// services/api.service.ts
@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  get<T>(url: string): Observable<T> {
    return this.http.get<ApiResponse<T>>(url).pipe(
      map(response => {
        if (!response.isSuccess) {
          throw new Error(response.message);
        }
        return response.data!;
      }),
      catchError(this.handleError)
    );
  }

  post<T>(url: string, data: any): Observable<T> {
    return this.http.post<ApiResponse<T>>(url, data).pipe(
      map(response => {
        if (!response.isSuccess) {
          if (response.errors) {
            throw new ValidationError(response.message, response.errors);
          }
          throw new Error(response.message);
        }
        return response.data!;
      }),
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    console.error('API Error:', error);
    return throwError(() => error);
  }
}

// Usage in components
export class PatientProfileComponent {
  constructor(private api: ApiService) {}

  loadProfile() {
    this.api.get<PatientResponse>('/api/patients/me')
      .subscribe({
        next: (patient) => {
          this.patient = patient;
        },
        error: (error) => {
          this.showError(error.message);
        }
      });
  }
}
```

---

## **📋 Migration Checklist**

### **For Each Endpoint:**

- [ ] Change return type to `ActionResult<ApiResponse<T>>`
- [ ] Update `ProducesResponseType` to use `ApiResponse<T>`
- [ ] Wrap success responses with `ApiResponse<T>.Success()`
- [ ] Wrap error responses with `ApiResponse<object>.Failure()`
- [ ] Extract ModelState errors for validation failures
- [ ] Update all status code responses (400, 404, 500, etc.)
- [ ] Test the endpoint with Swagger/Postman
- [ ] Update frontend code to handle new response structure

---

## **🎯 Best Practices**

### **1. Use Descriptive Messages**
```csharp
// ❌ Bad
return Ok(ApiResponse<PatientResponse>.Success(patient, "Success"));

// ✅ Good
return Ok(ApiResponse<PatientResponse>.Success(patient, "Patient profile retrieved successfully"));
```

### **2. Include Error Details in Development**
```csharp
catch (Exception ex)
{
    var errors = _env.IsDevelopment() 
        ? new[] { ex.Message, ex.StackTrace } 
        : new[] { ex.Message };
    
    return StatusCode(500, ApiResponse<object>.Failure(
        "An unexpected error occurred",
        errors,
        500
    ));
}
```

### **3. Consistent Status Codes**
```csharp
// Always match HTTP status code with ApiResponse statusCode
return NotFound(ApiResponse<object>.Failure("Not found", statusCode: 404));
return BadRequest(ApiResponse<object>.Failure("Bad request", statusCode: 400));
return StatusCode(500, ApiResponse<object>.Failure("Error", statusCode: 500));
```

### **4. Use Generic Object for No-Data Responses**
```csharp
// For DELETE, PATCH operations that don't return data
return Ok(ApiResponse<object>.Success(
    null,
    "Operation completed successfully"
));
```

---

## **🚀 Benefits Summary**

### **For Backend Developers:**
- ✅ Consistent response structure across all endpoints
- ✅ Reduced boilerplate code
- ✅ Better error handling patterns
- ✅ Easier to maintain and test

### **For Frontend Developers:**
- ✅ **Predictable response structure** - no guessing!
- ✅ **Single error handling pattern** for all endpoints
- ✅ **Built-in validation error support**
- ✅ **Type-safe with TypeScript**
- ✅ **Easier debugging** with consistent structure
- ✅ **Better UX** with meaningful messages

---

## **📊 Response Structure Comparison**

### **Before (Inconsistent):**
```json
// Success - sometimes just data
{ "id": "123", "name": "John" }

// Error - sometimes just string
"Patient not found"

// Error - sometimes object
{ "message": "Error occurred" }

// Validation - different structure
{ "errors": { "email": ["Invalid email"] } }
```

### **After (Consistent with ApiResponse):**
```json
// ALL responses follow the same structure
{
  "isSuccess": true/false,
  "message": "Human-readable message",
  "data": { ... },
  "errors": ["error1", "error2"],
  "statusCode": 200
}
```

---

## **🎉 Result**

**Frontend developers will love you!** ❤️

They can now:
- Write **one error handler** for all endpoints
- **Predict response structure** without checking docs
- **Display meaningful messages** to users easily
- **Handle validation errors** consistently
- **Debug faster** with clear error information

**This is production-ready, enterprise-grade API design!** 🚀
