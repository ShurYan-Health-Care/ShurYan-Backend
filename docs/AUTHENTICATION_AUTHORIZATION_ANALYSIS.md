# 🔐 Authentication & Authorization Analysis - ShurYan Backend

## 📊 Current Implementation Status

### **What We're Doing RIGHT:**

#### 1. **PatientsController - EXCELLENT Implementation** ⭐⭐⭐⭐⭐

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Patient")]  // Controller-level authorization
public class PatientsController : ControllerBase
{
    #region Helper Methods
    private Guid GetCurrentPatientId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Guid.Empty;
        }
        return userId;
    }

    private bool IsAccessingOwnData(Guid patientId)
    {
        var currentUserId = GetCurrentPatientId();
        return currentUserId == patientId;
    }
    #endregion

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> GetMyProfile()
    {
        var currentPatientId = GetCurrentPatientId();  // Using helper method
        
        if (currentPatientId == Guid.Empty)
        {
            _logger.LogWarning("Unauthorized attempt to access patient profile");
            return Unauthorized(new { Message = "Invalid or missing authentication token" });
        }
        
        // ... rest of implementation
    }
}
```

**Strengths:**
- Controller-level `[Authorize(Roles = "Patient")]`
- Helper methods for reusability
- Own data access validation
- Consistent pattern across all endpoints
- Proper logging of unauthorized attempts

---

#### 2. **PharmaciesController - MIXED Implementation** ⚠️

```csharp
[ApiController]
[Route("api/[controller]")]
public class PharmaciesController : ControllerBase  // NO controller-level authorization
{
    // GOOD: /me endpoints
    [HttpGet("me")]
    [Authorize(Roles = "Pharmacy")]
    public async Task<ActionResult<ApiResponse<PharmacyResponse>>> GetCurrentPharmacy()
    {
        var pharmacyIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // Correct claim
        if (string.IsNullOrEmpty(pharmacyIdClaim) || !Guid.TryParse(pharmacyIdClaim, out var pharmacyId))
        {
            _logger.LogWarning("Unauthorized attempt to access pharmacy profile");
            return Unauthorized(ApiResponse<object>.Failure(
                "Invalid or missing authentication token",
                statusCode: 401
            ));
        }
        // ... implementation
    }

    // PROBLEM: Endpoints with {id} parameter
    [HttpGet("{id}/orders")]
    [Authorize(Roles = "Pharmacy,Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyOrderResponse>>>> GetPharmacyOrders(Guid id, ...)
    {
        // NO validation that the pharmacy is accessing their own data!
        // Any authenticated pharmacy can access ANY pharmacy's orders by changing the {id}
        var orders = await _pharmacyService.GetPharmacyOrdersAsync(id, pageNumber, pageSize);
        return Ok(ApiResponse<IEnumerable<PharmacyOrderResponse>>.Success(orders, ...));
    }

    [HttpPut("{id}/orders/{orderId}/accept")]
    [Authorize(Roles = "Pharmacy")]
    public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> AcceptOrder(Guid id, Guid orderId)
    {
        // NO validation that the pharmacy owns this order!
        // Pharmacy A can accept orders for Pharmacy B
        var order = await _pharmacyService.AcceptOrderAsync(id, orderId);
        return Ok(ApiResponse<PharmacyOrderResponse>.Success(order, ...));
    }
}
```

---

## 🚨 **CRITICAL SECURITY ISSUES in PharmaciesController:**

### **Issue #1: No Own-Data Validation** 🔴

**Affected Endpoints (22 endpoints):**
1. `GET /api/pharmacies/{id}/orders` - Any pharmacy can view any pharmacy's orders
2. `GET /api/pharmacies/{id}/orders/{orderId}` - Cross-pharmacy order access
3. `PUT /api/pharmacies/{id}/orders/{orderId}/status` - Can update other pharmacy's orders
4. `GET /api/pharmacies/{id}/orders/pending` - View other pharmacy's pending orders
5. `GET /api/pharmacies/{id}/orders/in-progress` - View other pharmacy's in-progress orders
6. `GET /api/pharmacies/{id}/orders/completed` - View other pharmacy's completed orders
7. `PUT /api/pharmacies/{id}/orders/{orderId}/accept` - Accept orders for other pharmacies
8. `PUT /api/pharmacies/{id}/orders/{orderId}/reject` - Reject orders for other pharmacies
9. `PUT /api/pharmacies/{id}/orders/{orderId}/prepare` - Prepare orders for other pharmacies
10. `PUT /api/pharmacies/{id}/orders/{orderId}/ready` - Mark orders ready for other pharmacies
11. `PUT /api/pharmacies/{id}/orders/{orderId}/dispatch` - Dispatch orders for other pharmacies
12. `PUT /api/pharmacies/{id}/orders/{orderId}/deliver` - Mark delivered for other pharmacies
13. `GET /api/pharmacies/{id}/working-hours` - View other pharmacy's hours
14. `POST /api/pharmacies/{id}/working-hours` - Add hours for other pharmacies
15. `PUT /api/pharmacies/{id}/working-hours/{workingHoursId}` - Update other pharmacy's hours
16. `DELETE /api/pharmacies/{id}/working-hours/{workingHoursId}` - Delete other pharmacy's hours
17. `GET /api/pharmacies/{id}/documents` - View other pharmacy's documents
18. `POST /api/pharmacies/{id}/documents` - Upload documents for other pharmacies
19. `GET /api/pharmacies/{id}/prescriptions/{prescriptionId}` - View other pharmacy's prescriptions
20. `GET /api/pharmacies/{id}/address` - View other pharmacy's address (less critical)
21. `GET /api/pharmacies/{id}/reviews` - View reviews (less critical - should be public)
22. `GET /api/pharmacies/{id}/reviews/statistics` - View stats (less critical - should be public)

**Example Attack Scenario:**
```bash
# Pharmacy A (ID: 123) is authenticated
# They can access Pharmacy B's (ID: 456) orders:
GET /api/pharmacies/456/orders
Authorization: Bearer <pharmacy_a_token>

# Response: 200 OK with Pharmacy B's orders! 🚨
```

---

## 🎯 **RECOMMENDED FIXES:**

### **Solution 1: Add Helper Methods (Like PatientsController)** ⭐ RECOMMENDED

```csharp
public class PharmaciesController : ControllerBase
{
    #region Helper Methods
    private Guid GetCurrentPharmacyId()
    {
        var pharmacyIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(pharmacyIdClaim) || !Guid.TryParse(pharmacyIdClaim, out var pharmacyId))
        {
            return Guid.Empty;
        }
        return pharmacyId;
    }

    private bool IsAccessingOwnData(Guid pharmacyId)
    {
        // Admins can access any pharmacy's data
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var currentPharmacyId = GetCurrentPharmacyId();
        return currentPharmacyId == pharmacyId;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }
    #endregion

    [HttpGet("{id}/orders")]
    [Authorize(Roles = "Pharmacy,Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyOrderResponse>>>> GetPharmacyOrders(Guid id, ...)
    {
        // Validate own data access
        if (!IsAccessingOwnData(id))
        {
            _logger.LogWarning("Pharmacy {CurrentId} attempted to access orders for pharmacy {TargetId}", 
                GetCurrentPharmacyId(), id);
            return Forbid();  // 403 Forbidden
        }

        _logger.LogInformation("Get pharmacy orders request: {PharmacyId}", id);
        
        try
        {
            var orders = await _pharmacyService.GetPharmacyOrdersAsync(id, pageNumber, pageSize);
            return Ok(ApiResponse<IEnumerable<PharmacyOrderResponse>>.Success(orders, ...));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for pharmacy: {PharmacyId}", id);
            return StatusCode(500, ApiResponse<object>.Failure(...));
        }
    }

    [HttpPut("{id}/orders/{orderId}/accept")]
    [Authorize(Roles = "Pharmacy")]
    public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> AcceptOrder(Guid id, Guid orderId)
    {
        // Validate own data access
        if (!IsAccessingOwnData(id))
        {
            _logger.LogWarning("Pharmacy {CurrentId} attempted to accept order for pharmacy {TargetId}", 
                GetCurrentPharmacyId(), id);
            return Forbid();
        }

        _logger.LogInformation("Accept order request: {PharmacyId}, OrderId: {OrderId}", id, orderId);
        
        try
        {
            var order = await _pharmacyService.AcceptOrderAsync(id, orderId);
            return Ok(ApiResponse<PharmacyOrderResponse>.Success(order, ...));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting order: {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.Failure(...));
        }
    }
}
```

---

### **Solution 2: Use Authorization Policies** (More Advanced)

```csharp
// In Program.cs or Startup.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnPharmacyOrAdmin", policy =>
        policy.Requirements.Add(new OwnResourceOrAdminRequirement()));
});

// Authorization Handler
public class OwnPharmacyOrAdminHandler : AuthorizationHandler<OwnResourceOrAdminRequirement, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnResourceOrAdminRequirement requirement,
        Guid pharmacyId)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId) && userId == pharmacyId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// In Controller
[HttpGet("{id}/orders")]
[Authorize(Policy = "OwnPharmacyOrAdmin")]
public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyOrderResponse>>>> GetPharmacyOrders(Guid id, ...)
{
    // Authorization is already handled by the policy!
    var orders = await _pharmacyService.GetPharmacyOrdersAsync(id, pageNumber, pageSize);
    return Ok(ApiResponse<IEnumerable<PharmacyOrderResponse>>.Success(orders, ...));
}
```

---

## 📋 **Endpoints That Are SAFE (No Changes Needed):**

### **Public Endpoints (No Auth Required):**
1. `GET /api/pharmacies/{id}` - Public pharmacy info
2. `GET /api/pharmacies/email/{email}` - Public lookup
3. `POST /api/pharmacies/search` - Public search
4. `GET /api/pharmacies/governorate/{governorate}` - Public search
5. `GET /api/pharmacies/nearby` - Public search
6. `GET /api/pharmacies/{id}/is-open` - Public info

### **Admin-Only Endpoints (Correct):**
1. `DELETE /api/pharmacies/{id}` - Admin only ✅
2. `GET /api/pharmacies/pending-verification` - Admin/Verifier ✅
3. `POST /api/pharmacies/{id}/verify` - Admin/Verifier ✅
4. `POST /api/pharmacies/{id}/reject-verification` - Admin/Verifier ✅
5. `POST /api/pharmacies/{id}/documents/{documentId}/approve` - Admin/Verifier ✅
6. `POST /api/pharmacies/{id}/documents/{documentId}/reject` - Admin/Verifier ✅

### **Own Data Endpoints (Already Correct):**
1. `GET /api/pharmacies/me` - Uses ClaimTypes.NameIdentifier ✅
2. `PUT /api/pharmacies/me` - Uses ClaimTypes.NameIdentifier ✅

---

## 🎯 **Action Items - Priority Order:**

### **🔴 CRITICAL (Fix Immediately):**
1. Add `GetCurrentPharmacyId()` helper method
2. Add `IsAccessingOwnData(Guid pharmacyId)` helper method
3. Add own-data validation to all 22 vulnerable endpoints
4. Add proper logging for unauthorized access attempts

### **🟡 MEDIUM (Enhance):**
1. Consider implementing Authorization Policies for cleaner code
2. Add unit tests for authorization logic
3. Add integration tests for cross-pharmacy access attempts

### **🟢 LOW (Nice to Have):**
1. Add IP address logging for security audit
2. Implement rate limiting per pharmacy
3. Add audit trail for sensitive operations

---

## 📊 **Comparison Summary:**

| Feature | PatientsController | PharmaciesController | Status |
|---------|-------------------|---------------------|--------|
| Controller-level Auth | Yes | No | Add |
| Helper Methods | Yes | No | 🔴 Critical |
| Own Data Validation | Yes | No | 🔴 Critical |
| Consistent Pattern | Yes | Partial | Fix |
| Proper Logging | Yes | Yes | Good |
| ApiResponse Wrapper | Yes | Yes | Good |
| Error Handling | Yes | Yes | Good |

---

## 🎓 **Best Practices We Should Follow:**

### **1. Defense in Depth:**
```csharp
// Layer 1: Attribute-based authorization
[Authorize(Roles = "Pharmacy")]

// Layer 2: Own-data validation
if (!IsAccessingOwnData(id)) return Forbid();

// Layer 3: Service-level validation (optional but recommended)
await _pharmacyService.ValidateAccessAsync(currentPharmacyId, id);
```

### **2. Fail Securely:**
```csharp
// BAD: Returns data on validation failure
var currentId = GetCurrentPharmacyId();
if (currentId == Guid.Empty) 
{
    // Still continues to fetch data!
}
var data = await _service.GetDataAsync(id);

// GOOD: Fails immediately
var currentId = GetCurrentPharmacyId();
if (currentId == Guid.Empty) 
{
    return Unauthorized(...);
}
if (!IsAccessingOwnData(id))
{
    return Forbid();
}
var data = await _service.GetDataAsync(id);
```

### **3. Log Security Events:**
```csharp
// Always log unauthorized access attempts
_logger.LogWarning("Pharmacy {CurrentId} attempted to access data for pharmacy {TargetId}", 
    GetCurrentPharmacyId(), id);
```

---

## 🚀 **Next Steps:**

1. **Review this document** with the team
2. **Prioritize the fixes** based on business impact
3. **Implement helper methods** in PharmaciesController
4. **Add validation** to all 22 vulnerable endpoints
5. **Write tests** to verify the fixes
6. **Deploy** and monitor for any issues

---

**Created:** October 23, 2025  
**Status:** 🔴 CRITICAL SECURITY ISSUES IDENTIFIED  
**Action Required:** YES - Immediate fix needed for 22 endpoints
