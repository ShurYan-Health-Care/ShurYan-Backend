# رحلة الـ HTTP Request في ASP.NET Core - الجزء التاني
## من الـ Authentication للـ Controllers

---

## 🔐 الفصل العاشر: Authentication Middleware - مين انت؟

### الـ Authentication ده بيعمل إيه؟

ببساطة، بيسأل سؤال واحد: **"مين انت؟"**. مش بيقولك تدخل ولا متدخلش، ده بس بيحاول يعرف هويتك.

### JWT Bearer Authentication

لما الـ Request بتاعنا وصل وفيه Header:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### الـ JWT Token من جوه

الـ JWT ده 3 أجزاء:

**الجزء الأول (Header):**
```json
{
  "alg": "HS256",  // نوع التشفير
  "typ": "JWT"     // نوع الـ Token
}
```

**الجزء التاني (Payload):**
```json
{
  "sub": "1234567890",           // الـ User ID
  "name": "محمد أحمد",
  "email": "mohamed@example.com",
  "role": "Admin",
  "exp": 1516239022,              // وقت الانتهاء
  "iat": 1516235422               // وقت الإصدار
}
```

**الجزء التالت:** التوقيع (Signature)

#### إزاي بيتأكد من الـ Token؟

```csharp
// 1. بيشيل كلمة Bearer وياخد الـ Token
string token = authorization.Substring("Bearer ".Length);

// 2. بيفك الـ Token ويتأكد منه
var handler = new JwtSecurityTokenHandler();
var validationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,           // مين اللي طلع الـ Token؟
    ValidIssuer = "shuryan.com",
    
    ValidateAudience = true,          // الـ Token ده لمين؟
    ValidAudience = "shuryan-api",
    
    ValidateLifetime = true,          // لسه مش منتهي؟
    ClockSkew = TimeSpan.Zero,
    
    ValidateIssuerSigningKey = true,  // التوقيع صح؟
    IssuerSigningKey = new SymmetricSecurityKey(key)
};

var principal = handler.ValidateToken(token, validationParameters, out _);
```

### Cookie Authentication

لو جاي من Browser:

```csharp
// 1. بيدور على الـ Cookie
var cookie = Request.Cookies[".AspNetCore.Identity.Application"];

// 2. بيفك تشفيرها
var protector = DataProtectionProvider.CreateProtector("Cookies");
var ticket = protector.Unprotect(cookie);

// 3. بيتأكد من الـ Security Stamp
var securityStamp = ticket.Principal.FindFirst("SecurityStamp")?.Value;
if (user.SecurityStamp != securityStamp)
{
    // الـ User غير حاجة مهمة، ارفضه
    return AuthenticateResult.Fail("Security stamp changed");
}
```

### النتيجة: HttpContext.User

بعد النجاح:

```csharp
HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.NameIdentifier, "123"),
    new Claim(ClaimTypes.Name, "محمد أحمد"),
    new Claim(ClaimTypes.Email, "mohamed@example.com"),
    new Claim(ClaimTypes.Role, "Admin")
}));
```

---

## 🚫 الفصل 11: Authorization - مسموحلك؟

### الفرق المهم

- **Authentication:** بطاقتك إيه؟
- **Authorization:** عندك تصريح تدخل هنا؟

### Role-Based Authorization

```csharp
[Authorize(Roles = "Admin,Manager")]
public class AdminController : ControllerBase
{
    // بس Admin أو Manager
}
```

### Policy-Based Authorization

```csharp
// تعريف Policy
services.AddAuthorization(options =>
{
    options.AddPolicy("AtLeast18", policy =>
        policy.RequireAssertion(context =>
        {
            var birthDate = context.User.FindFirst("BirthDate")?.Value;
            if (DateTime.TryParse(birthDate, out var date))
            {
                var age = DateTime.Today.Year - date.Year;
                return age >= 18;
            }
            return false;
        }));
});

// استخدام
[Authorize(Policy = "AtLeast18")]
public IActionResult AdultContent() { }
```

---

## 🗺️ الفصل 12: Routing - روح فين؟

### الـ Routing Algorithm

```
URL: /api/users/123
    ↓
بيدور في كل الـ Routes:
    - /api/users/{id} ✅ ده مطابق!
    - /api/products/{id} ❌
    - /api/orders/{id} ❌
    ↓
استخرج الـ Route Values:
    - controller = "Users"
    - action = "GetById"
    - id = "123"
```

### أنواع الـ Routes

**Conventional:**
```csharp
"{controller=Home}/{action=Index}/{id?}"
// /Products/Details/5 → ProductsController.Details(5)
```

**Attribute:**
```csharp
[Route("api/[controller]")]
public class UsersController
{
    [HttpGet("{id:int}")]  // GET api/users/123
    public IActionResult GetUser(int id) { }
}
```

### Route Constraints

```csharp
[HttpGet("{id:int:min(1)}")]        // رقم أكبر من صفر
[HttpGet("{name:alpha:length(3,10)}")]  // حروف من 3-10
[HttpGet("{date:datetime}")]        // تاريخ
[HttpGet("{price:decimal:range(0,1000)}")]  // سعر
```

---

## 🎮 الفصل 13: Controller Pipeline

### Model Binding - جيب البيانات منين؟

```csharp
[HttpPost("profile")]
public IActionResult UpdateProfile(
    [FromBody] UserDto body,      // من الـ JSON
    [FromQuery] int page,          // من ?page=2
    [FromRoute] int userId,        // من الـ URL
    [FromHeader] string auth)      // من Headers
{
    // كله اتملى تلقائي!
}
```

### كيف بيحول JSON لـ Object؟

```json
{
  "name": "أحمد",
  "age": 25
}
```
↓
```csharp
public class UserDto
{
    public string Name { get; set; }  // = "أحمد"
    public int Age { get; set; }      // = 25
}
```

---

## ✅ الفصل 14: Model Validation

### Data Annotations

```csharp
public class UserDto
{
    [Required(ErrorMessage = "الاسم مطلوب")]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; }
    
    [EmailAddress(ErrorMessage = "إيميل غلط")]
    public string Email { get; set; }
    
    [Range(18, 120, ErrorMessage = "السن من 18-120")]
    public int Age { get; set; }
    
    [RegularExpression(@"^01[0125]\d{8}$", 
        ErrorMessage = "رقم مصري غير صحيح")]
    public string Phone { get; set; }
}
```

### Custom Validation

```csharp
public class EgyptianPhoneAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(
        object value, 
        ValidationContext context)
    {
        var phone = value as string;
        if (!Regex.IsMatch(phone, @"^01[0125]\d{8}$"))
        {
            return new ValidationResult("مش رقم مصري");
        }
        return ValidationResult.Success;
    }
}
```

---

## 🔍 الفصل 15: Filters

### ترتيب التنفيذ

```
1. Authorization Filter (مسموح تدخل؟)
    ↓
2. Resource Filter Before (cache مثلاً)
    ↓
3. Model Binding
    ↓
4. Action Filter Before (logging)
    ↓
5. Action Execution ← Exception Filter
    ↓
6. Action Filter After
    ↓
7. Result Filter Before (headers)
    ↓
8. Result Execution
    ↓
9. Result Filter After
    ↓
10. Resource Filter After
```

### أمثلة

**Logging Filter:**
```csharp
public class LogActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        Log($"بدأ: {context.ActionDescriptor.DisplayName}");
    }
    
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        Log($"انتهى في {context.HttpContext.Response.StatusCode}");
    }
}
```

**Cache Filter:**
```csharp
public class CacheResourceFilter : IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var cacheKey = context.HttpContext.Request.Path;
        if (Cache.TryGet(cacheKey, out var cached))
        {
            context.Result = cached; // وقف وارجع من الـ Cache
        }
    }
    
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        if (context.Result != null)
        {
            Cache.Set(context.HttpContext.Request.Path, context.Result);
        }
    }
}
```

---

## ⚡ الفصل 16: Action Execution

### تنفيذ الـ Action Method

```csharp
[HttpPost("profile")]
public async Task<IActionResult> UpdateProfile(UserDto dto)
{
    // 1. Model Binding خلص
    // 2. Validation خلص
    
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // 3. Business Logic
    var user = await _userService.UpdateAsync(dto);
    
    // 4. Return Result
    return Ok(new { 
        success = true, 
        data = user 
    });
}
```

### أنواع الـ Results

```csharp
// JSON
return Ok(data);           // 200 + JSON
return BadRequest(error);  // 400 + JSON
return NotFound();         // 404

// Status Codes
return StatusCode(201);    // أي كود
return NoContent();        // 204

// Files
return File(bytes, "application/pdf");
return PhysicalFile(@"C:\file.pdf");

// Redirects
return Redirect("https://google.com");
return RedirectToAction("Index");
```

---

## 🔙 الفصل 17: Response Pipeline (الرجوع)

### بعد الـ Action

الـ Response بيرجع بنفس الطريق بس بالعكس:

```
Action Result
    ↓
Result Filters (After)
    ↓
Resource Filters (After)
    ↓
CORS Headers
    ↓
Response Compression
    ↓
Response Caching
    ↓
Kestrel
    ↓
TCP/IP
    ↓
Browser
```

### Response Writing

```csharp
// Kestrel بيحول الـ Object لـ HTTP Response
HttpResponse response = context.Response;

// Status Line
response.StatusCode = 200;

// Headers
response.Headers["Content-Type"] = "application/json";
response.Headers["Cache-Control"] = "no-cache";

// Body
var json = JsonSerializer.Serialize(data);
var bytes = Encoding.UTF8.GetBytes(json);
await response.Body.WriteAsync(bytes);
```

### الـ HTTP Response النهائي

```http
HTTP/1.1 200 OK
Content-Type: application/json
Content-Length: 156
Cache-Control: no-cache
Date: Mon, 19 Sep 2024 10:30:00 GMT

{
  "success": true,
  "data": {
    "id": 123,
    "name": "محمد أحمد",
    "email": "mohamed@example.com"
  }
}
```

---

## 🧹 الفصل 18: Cleanup

### Request Scope Disposal

```csharp
// بعد ما الـ Response يتبعت
using (var scope = serviceProvider.CreateScope())
{
    // كل ده كان للـ Request
    var dbContext = scope.GetService<AppDbContext>();
    var userService = scope.GetService<IUserService>();
    
    // Response اتبعت
    
} // ← هنا كل حاجة بتتحذف

// DbContext.Dispose()
// Connections ترجع للـ Pool
// Memory تتحرر
```

### Garbage Collection

```csharp
// Gen 0: Objects جديدة (أغلبها بتموت بسرعة)
// Gen 1: Objects عاشت شوية
// Gen 2: Objects قديمة (Controllers, Services)

// بعد الـ Request
GC.Collect(0); // نضف الـ Gen 0
```

---

## 🎯 الخلاصة

### الرحلة كاملة:

1. **Browser** بعت Request
2. **TCP/IP + OS** وصلوه
3. **Kestrel** استقبله وعمل HttpContext
4. **Middlewares** مروه:
   - Exception Handler
   - HTTPS Redirect  
   - CORS
   - Authentication (مين انت؟)
   - Authorization (مسموح؟)
   - Routing (روح فين؟)
5. **Controller** اتنفذ:
   - Model Binding
   - Validation
   - Filters
   - Action Method
6. **Response** رجع بنفس الطريق
7. **Cleanup** نضف كل حاجة

### النصايح المهمة:

✅ **رتب الـ Middlewares صح** - الترتيب مهم جداً!
✅ **استخدم Async/Await** - متسدش الـ Threads
✅ **خلي الـ DbContext Scoped** - واحد لكل Request
✅ **استخدم Caching** - وفر في الـ Database
✅ **Log الأخطاء** - عشان تعرف المشاكل

---

**الجزء التالت: الـ Database والـ Threading بالتفصيل! 🚀**
