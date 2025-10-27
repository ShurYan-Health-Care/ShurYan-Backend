# رحلة الـ HTTP Request في ASP.NET Core - الجزء الأول
## من الشبكة للـ Middleware Pipeline

---

## 🌐 الفصل الأول: الـ Request جاي من فين أصلاً؟

### الحكاية من الأول خالص

تخيل معايا إنك فاتح البراوزر وكتبت `https://shuryan.com/api/users/profile` ودوست Enter. إيه اللي بيحصل بالظبط؟

### 1. البراوزر بيعمل إيه؟

البراوزر أول حاجة بيعملها:
- بيحول الـ URL لـ IP Address (عن طريق DNS)
- بيجهز HTTP Request على شكل نص:

```http
POST /api/users/profile HTTP/1.1
Host: shuryan.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
Content-Length: 156

{
  "name": "محمد أحمد",
  "email": "mohamed@example.com",
  "phone": "01234567890"
}
```

### 2. الرحلة عبر الإنترنت

الـ Request ده بيتحول لـ **TCP Packets** (قطع صغيرة من البيانات) وبيسافر عبر:
- الراوتر بتاعك
- شركة النت (ISP)
- كذا سيرفر في النص (Routers و Switches)
- لحد ما يوصل للسيرفر بتاعك

---

## 🖥️ الفصل التاني: السيرفر استلم الـ Packet

### كارت الشبكة (Network Interface Card - NIC)

أول حاجة بتستقبل الـ Packets دي هي كارت الشبكة في السيرفر. الكارت ده:

1. **بيستقبل الإشارات الكهربائية** من الكابل
2. **بيحولها لـ Binary Data** (0 و 1)
3. **بيرفعها للـ Operating System** عن طريق حاجة اسمها **Interrupt**

### نظام التشغيل (Windows/Linux) بيعمل إيه؟

#### الـ TCP/IP Stack في الـ Kernel

الـ OS بياخد الـ Packets دي ويمررها على طبقات:

1. **طبقة الـ Link Layer:** بتشيل الـ Ethernet headers
2. **طبقة الـ Network Layer (IP):** بتتأكد إن الـ Packet ده ليك فعلاً
3. **طبقة الـ Transport Layer (TCP):** 
   - بتجمع الـ Packets المتقطعة مع بعض
   - بترتبهم بالترتيب الصح
   - بتتأكد إن مفيش حاجة ناقصة

4. **طبقة الـ Application Layer:** 
   - هنا بقى الـ OS بيبص، مين اللي مستني على Port 443 (HTTPS)؟
   - بيلاقي **Kestrel** قاعد مستني!

### الـ Socket Connection

الـ OS بيعمل **Socket** (قناة اتصال) بين:
- **البراوزر** (اللي باعت الـ Request)
- **Kestrel** (السيرفر بتاعك)

```csharp
// Kestrel من جواه بيعمل حاجة زي كده
var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
listener.Bind(new IPEndPoint(IPAddress.Any, 443));
listener.Listen(100); // يقدر يستقبل لحد 100 connection في نفس الوقت

// لما يجي connection جديد
var clientSocket = await listener.AcceptAsync();
```

---

## 🚀 الفصل التالت: Kestrel - البطل الحقيقي

### إيه هو Kestrel أصلاً؟

Kestrel ده الـ **Web Server** بتاع .NET Core. هو اللي بيستقبل الـ HTTP Requests ويبعت الـ Responses. تخيله كأنه **البواب** بتاع العمارة، أي حد عايز يدخل لازم يعدي عليه الأول.

### Kestrel بيشتغل إزاي؟

#### 1. Connection Listener

Kestrel عنده **Thread** مخصوص بس للـ Listening على الـ Port:

```csharp
// ده بيحصل في Background Thread
while (serverIsRunning)
{
    // استنى لحد ما حد يجي يطرق الباب
    var tcpClient = await tcpListener.AcceptTcpClientAsync();
    
    // لما حد يجي، اديله Thread يخدمه
    _ = Task.Run(() => HandleConnection(tcpClient));
}
```

#### 2. الـ SSL/TLS Handshake (لو HTTPS)

قبل ما نقرأ أي HTTP Request، لازم نعمل **تشفير** للاتصال:

```
Client: "عايز أكلمك بالسر" (Client Hello)
Kestrel: "تمام، ده الشهادة بتاعتي" (Server Certificate)
Client: "شهادتك تمام، يلا نتفق على مفتاح سري" (Key Exchange)
Kestrel: "اتفقنا، كلمني بالمفتاح ده" (Finished)
```

الكلام ده كله بيحصل في **milliseconds** قبل ما نبدأ نقرأ الـ HTTP Request.

#### 3. قراءة الـ HTTP Request

دلوقتي Kestrel بيبدأ يقرأ البيانات من الـ Socket:

```csharp
// Kestrel بيقرأ البيانات شوية شوية
byte[] buffer = new byte[4096]; // buffer صغير 4KB
int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

// بيحول الـ bytes لـ text
string httpRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
```

#### 4. تحليل الـ HTTP Headers

Kestrel بيبدأ يفكك الـ Request:

```csharp
// مثال مبسط للي بيحصل
string[] lines = httpRequest.Split("\r\n");

// السطر الأول: POST /api/users/profile HTTP/1.1
var requestLine = lines[0].Split(' ');
var method = requestLine[0];      // POST
var path = requestLine[1];         // /api/users/profile
var httpVersion = requestLine[2];  // HTTP/1.1

// باقي السطور: Headers
Dictionary<string, string> headers = new();
for (int i = 1; i < lines.Length; i++)
{
    if (string.IsNullOrEmpty(lines[i])) break; // نهاية الـ Headers
    
    var parts = lines[i].Split(": ");
    headers[parts[0]] = parts[1];
}
```

#### 5. الـ HTTP/2 و HTTP/3

لو البراوزر بيدعم HTTP/2 أو HTTP/3، Kestrel بيقدر يتعامل معاهم:

- **HTTP/2:** بيسمح بأكتر من Request في نفس الـ Connection
- **HTTP/3:** بيستخدم QUIC Protocol (أسرع من TCP)

---

## 🏗️ الفصل الرابع: بناء الـ HttpContext

### الـ HttpContext ده إيه؟

تخيل إن الـ HttpContext ده زي **الملف الطبي** للمريض في المستشفى. كل دكتور (Middleware) بيفتحه، يقرأ المعلومات، يكتب ملاحظاته، ويسلمه للي بعده.

### مكونات الـ HttpContext

```csharp
public class HttpContext
{
    // معلومات الـ Request اللي جاي
    public HttpRequest Request { get; set; }
    
    // الـ Response اللي هنبعته
    public HttpResponse Response { get; set; }
    
    // معلومات عن الـ Connection
    public ConnectionInfo Connection { get; set; }
    
    // الـ User (لو متسجل دخول)
    public ClaimsPrincipal User { get; set; }
    
    // مخزن للبيانات المؤقتة
    public IDictionary<object, object> Items { get; set; }
    
    // الـ Services المتاحة
    public IServiceProvider RequestServices { get; set; }
    
    // Features إضافية
    public IFeatureCollection Features { get; set; }
}
```

### بناء الـ HttpRequest

Kestrel بيملى الـ HttpRequest بالمعلومات:

```csharp
var request = new HttpRequest
{
    Method = "POST",
    Path = "/api/users/profile",
    Headers = new HeaderDictionary
    {
        ["Host"] = "shuryan.com",
        ["Authorization"] = "Bearer eyJhbGciOiJIUzI1NiIs...",
        ["Content-Type"] = "application/json",
        ["Content-Length"] = "156"
    },
    Body = new MemoryStream(bodyBytes), // الـ JSON body
    QueryString = new QueryString("?page=1&limit=10"),
    Cookies = new RequestCookieCollection(),
    Scheme = "https",
    IsHttps = true
};
```

### بناء الـ HttpResponse

في نفس الوقت، بيجهز HttpResponse فاضي:

```csharp
var response = new HttpResponse
{
    StatusCode = 200, // Default
    Headers = new HeaderDictionary(),
    Body = new MemoryStream(), // فاضي في البداية
    Cookies = new ResponseCookies()
};
```

### الـ Request Features

الـ Features دي زي **الأدوات الإضافية** اللي ممكن نحتاجها:

```csharp
// مثلاً معلومات عن الـ TLS/SSL
var tlsFeature = httpContext.Features.Get<ITlsConnectionFeature>();
if (tlsFeature != null)
{
    var clientCertificate = tlsFeature.ClientCertificate;
    var tlsVersion = tlsFeature.Protocol; // TLS 1.2, TLS 1.3, etc.
}

// أو معلومات عن الـ HTTP/2
var http2Feature = httpContext.Features.Get<IHttp2Feature>();
```

---

## 🎭 الفصل الخامس: الـ Dependency Injection Scope

### Request Scope إيه ده؟

كل Request بيجي، بنعمله **عالم خاص بيه** من الـ Services. زي كأن كل مريض في المستشفى ليه طاقم طبي خاص بيه.

```csharp
// لما الـ Request يوصل
using (var scope = serviceProvider.CreateScope())
{
    // كل الـ Services دي مخصوصة للـ Request ده بس
    httpContext.RequestServices = scope.ServiceProvider;
    
    // الـ Scoped Services زي DbContext
    var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
    // ده DbContext جديد مخصوص للـ Request ده
    
    // بعد ما الـ Request يخلص
} // هنا الـ scope بيتقفل وكل حاجة بتتحذف
```

### أنواع الـ Service Lifetimes

1. **Singleton:** واحد بس للبرنامج كله
   ```csharp
   services.AddSingleton<IConfiguration>(); // نفس الـ instance للكل
   ```

2. **Scoped:** واحد لكل Request
   ```csharp
   services.AddScoped<AppDbContext>(); // واحد جديد لكل Request
   ```

3. **Transient:** واحد جديد كل مرة حد يطلبه
   ```csharp
   services.AddTransient<IEmailService>(); // جديد كل مرة
   ```

---

## 🚇 الفصل السادس: الـ Middleware Pipeline

### الـ Pipeline ده إيه؟

تخيل محطات المترو. الـ Request لازم يعدي على كل محطة (Middleware) بالترتيب. كل محطة بتعمل حاجة معينة وتسلم للي بعدها.

### ترتيب الـ Middlewares مهم جداً!

```csharp
var app = builder.Build();

// الترتيب ده مهم جداً!
app.UseExceptionHandler();      // 1. لو حصل Exception
app.UseHttpsRedirection();       // 2. حول لـ HTTPS
app.UseCors();                   // 3. تأكد من الـ CORS
app.UseAuthentication();         // 4. مين انت؟
app.UseAuthorization();          // 5. مسموحلك تعمل كده؟
app.UseRouting();                // 6. روح فين؟
app.MapControllers();            // 7. نفذ الـ Action
```

### كل Middleware بيشتغل إزاي؟

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. قبل ما نروح للي بعدنا
        Console.WriteLine("قبل");
        
        // 2. خلي اللي بعدنا يشتغل
        await _next(context);
        
        // 3. بعد ما اللي بعدنا يخلص
        Console.WriteLine("بعد");
    }
}
```

الموضوع بيبقى كده:
```
Request →
    Middleware1 (قبل) →
        Middleware2 (قبل) →
            Middleware3 (قبل) →
                Controller Action
            Middleware3 (بعد) ←
        Middleware2 (بعد) ←
    Middleware1 (بعد) ←
Response ←
```

---

## 🛡️ الفصل السابع: Exception Handler Middleware

### ليه ده أول واحد؟

عشان لو أي حاجة اتفجرت في أي مكان، هو اللي يمسكها:

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";
        
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature != null)
        {
            var exception = feature.Error;
            
            // سجل الـ Error
            logger.LogError(exception, "حصل خطأ");
            
            // ابعت رد محترم للـ Client
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "حصل خطأ في السيرفر"
            }));
        }
    });
});
```

### في Development vs Production

**في Development:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // يظهر كل التفاصيل
}
```

**في Production:**
```csharp
else
{
    app.UseExceptionHandler("/Error"); // صفحة خطأ عادية
}
```

---

## 🔒 الفصل التامن: HTTPS Redirection Middleware

### ليه محتاجين HTTPS؟

HTTPS بيشفر البيانات بين البراوزر والسيرفر. من غيره، أي حد على نفس الشبكة يقدر يشوف الباسورد بتاعك!

### إزاي بيشتغل؟

```csharp
public class HttpsRedirectionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // لو مش HTTPS
        if (!context.Request.IsHttps)
        {
            // غير الـ URL لـ HTTPS
            var httpsUrl = "https://" + context.Request.Host + context.Request.Path;
            
            // ابعت رد 301 (Permanent Redirect)
            context.Response.StatusCode = 301;
            context.Response.Headers["Location"] = httpsUrl;
            
            // خلاص كده، مش هنكمل
            return;
        }
        
        // لو HTTPS، كمل عادي
        await _next(context);
    }
}
```

### الـ HSTS Headers

في Production، بنضيف Header اسمه HSTS عشان نقول للبراوزر "متجيش تاني غير HTTPS":

```csharp
app.UseHsts(); // Strict-Transport-Security header
```

---

## 🌍 الفصل التاسع: CORS Middleware

### CORS ده إيه؟

Cross-Origin Resource Sharing. لو عندك Frontend على `https://app.shuryan.com` وBackend على `https://api.shuryan.com`، البراوزر مش هيسمح للـ JavaScript يكلم الـ API إلا لو الـ API قال "أنا موافق".

### الـ Preflight Requests

قبل ما البراوزر يبعت الـ Request الحقيقي، بيبعت سؤال:

```http
OPTIONS /api/users/profile HTTP/1.1
Origin: https://app.shuryan.com
Access-Control-Request-Method: POST
Access-Control-Request-Headers: Authorization, Content-Type
```

### CORS Middleware بيرد:

```csharp
public class CorsMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"];
        
        // لو الـ Origin ده مسموح
        if (IsAllowedOrigin(origin))
        {
            // قول للبراوزر "تعالى"
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            
            // لو ده Preflight
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Authorization, Content-Type";
                context.Response.StatusCode = 204; // No Content
                return; // خلاص كده
            }
        }
        
        await _next(context);
    }
}
```

### تكوين CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.WithOrigins("https://app.shuryan.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("MyPolicy");
```

---

## النهاية (مؤقتاً)

كده احنا وصلنا الـ Request لحد الـ CORS Middleware. في الجزء الجاي هنكمل رحلتنا مع:
- Authentication & Authorization
- Routing
- Controllers
- Database
- والرجوع تاني!

🎯 **الخلاصة لحد دلوقتي:**

1. **الـ Request جه من البراوزر** كـ TCP Packets
2. **كارت الشبكة استقبله** وطلعه للـ OS
3. **الـ OS وصله لـ Kestrel** على Port 443
4. **Kestrel فك التشفير** (SSL/TLS)
5. **Kestrel حلل الـ HTTP** وعمل HttpContext
6. **بدأت رحلة الـ Middleware Pipeline:**
   - Exception Handler (يمسك أي غلطة)
   - HTTPS Redirection (يحول لـ HTTPS)
   - CORS (يسمح للـ Frontend يكلمنا)

**الجاي أحلى! 🚀**
