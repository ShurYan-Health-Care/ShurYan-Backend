# رحلة الـ HTTP Request في ASP.NET Core - الجزء التالت
## الـ Database والـ Threading والـ Memory بالعمق

---

## 💾 الفصل 19: Entity Framework Core - الطريق للـ Database

### الـ DbContext Lifecycle

الـ DbContext ده زي **المترجم** بين الكود بتاعك والـ Database. بيحول الـ LINQ لـ SQL.

```csharp
// في الـ Controller
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public UsersController(AppDbContext context)
    {
        _context = context; // ده Scoped، جديد لكل Request
    }
}
```

### من LINQ لـ SQL

تعالى نشوف الرحلة دي خطوة خطوة:

```csharp
// الكود اللي انت كاتبه
var user = await _context.Users
    .Where(u => u.Age > 18)
    .OrderBy(u => u.Name)
    .FirstOrDefaultAsync();
```

#### الخطوة 1: Expression Tree

الـ Compiler بيحول الكود لـ **Expression Tree**:

```csharp
Expression tree = 
    MethodCall(
        MethodCall(
            MethodCall(
                _context.Users,
                "Where",
                Lambda(u => u.Age > 18)
            ),
            "OrderBy",
            Lambda(u => u.Name)
        ),
        "FirstOrDefaultAsync"
    );
```

#### الخطوة 2: Query Provider

الـ EF Core بياخد الـ Expression Tree ويحلله:

```csharp
// EF Core من جوه
public class SqlServerQueryProvider
{
    public string TranslateToSql(Expression expression)
    {
        // بيمشي على الـ Tree
        if (expression is MethodCallExpression methodCall)
        {
            switch (methodCall.Method.Name)
            {
                case "Where":
                    return $"WHERE {TranslateCondition(methodCall.Arguments[0])}";
                case "OrderBy":
                    return $"ORDER BY {TranslateProperty(methodCall.Arguments[0])}";
            }
        }
    }
}
```

#### الخطوة 3: SQL Generation

النتيجة النهائية:

```sql
SELECT TOP(1) [u].[Id], [u].[Name], [u].[Age], [u].[Email]
FROM [Users] AS [u]
WHERE [u].[Age] > 18
ORDER BY [u].[Name]
```

### الـ Change Tracker

الـ Change Tracker ده اللي بيتابع التغييرات في الـ Entities:

```csharp
// لما بتجيب Entity
var user = await _context.Users.FindAsync(123);
// Change Tracker بيحفظ نسخة (Snapshot) من البيانات الأصلية

// لما بتغير
user.Name = "اسم جديد";
// Change Tracker بيعرف إن Name اتغير

// لما بتحفظ
await _context.SaveChangesAsync();
// بيقارن الـ Current Values بالـ Original Values
// ويعمل UPDATE statements بس للحاجات اللي اتغيرت
```

### كيف الـ Change Tracker بيشتغل؟

```csharp
public class ChangeTracker
{
    private Dictionary<object, EntityEntry> _entries = new();
    
    public class EntityEntry
    {
        public object Entity { get; set; }
        public EntityState State { get; set; }  // Added, Modified, Deleted, Unchanged
        public Dictionary<string, object> OriginalValues { get; set; }
        public Dictionary<string, object> CurrentValues { get; set; }
        
        public bool IsModified()
        {
            foreach (var property in CurrentValues.Keys)
            {
                if (!Equals(CurrentValues[property], OriginalValues[property]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
```

---

## 🔄 الفصل 20: Async/Await - السحر الأسود

### إيه اللي بيحصل لما تكتب async/await؟

الـ Compiler بيحول الـ Method لـ **State Machine**:

```csharp
// الكود اللي انت كاتبه
public async Task<User> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);
    await LogAsync($"User {id} retrieved");
    return user;
}

// الـ Compiler بيحوله لحاجة زي كده
public Task<User> GetUserAsync(int id)
{
    var stateMachine = new GetUserAsyncStateMachine
    {
        _id = id,
        _builder = AsyncTaskMethodBuilder<User>.Create(),
        _state = -1
    };
    
    stateMachine._builder.Start(ref stateMachine);
    return stateMachine._builder.Task;
}

private struct GetUserAsyncStateMachine : IAsyncStateMachine
{
    public int _state;
    public int _id;
    public AsyncTaskMethodBuilder<User> _builder;
    private TaskAwaiter<User> _awaiter1;
    private TaskAwaiter _awaiter2;
    
    public void MoveNext()
    {
        try
        {
            switch (_state)
            {
                case -1: // البداية
                    _awaiter1 = _context.Users.FindAsync(_id).GetAwaiter();
                    if (!_awaiter1.IsCompleted)
                    {
                        _state = 0;
                        _builder.AwaitUnsafeOnCompleted(ref _awaiter1, ref this);
                        return; // ارجع الـ Thread للـ Thread Pool
                    }
                    goto case 0;
                    
                case 0: // بعد FindAsync
                    var user = _awaiter1.GetResult();
                    _awaiter2 = LogAsync($"User {_id} retrieved").GetAwaiter();
                    if (!_awaiter2.IsCompleted)
                    {
                        _state = 1;
                        _builder.AwaitUnsafeOnCompleted(ref _awaiter2, ref this);
                        return; // ارجع الـ Thread تاني
                    }
                    goto case 1;
                    
                case 1: // بعد LogAsync
                    _awaiter2.GetResult();
                    _builder.SetResult(user);
                    return;
            }
        }
        catch (Exception ex)
        {
            _builder.SetException(ex);
        }
    }
}
```

### ليه Async/Await مهم؟

تخيل عندك 100 Request في نفس الوقت:

**بدون Async (Synchronous):**
```csharp
public User GetUser(int id)
{
    var user = _context.Users.Find(id);  // Thread بيستنى 100ms
    return user;
}
// محتاج 100 Thread عشان تخدم 100 Request!
```

**مع Async:**
```csharp
public async Task<User> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);  // Thread بيرجع للـ Pool
    return user;
}
// ممكن 10 Threads بس يخدموا 100 Request!
```

---

## 🧵 الفصل 21: Thread Pool - مصنع الـ Threads

### إيه هو الـ Thread Pool؟

مجموعة من الـ Threads جاهزة للشغل. بدل ما تعمل Thread جديد كل مرة (مكلف)، تاخد واحد جاهز من الـ Pool.

```csharp
public class ThreadPool
{
    private Queue<Thread> _availableThreads = new();
    private int _minThreads = Environment.ProcessorCount;  // 8 مثلاً
    private int _maxThreads = Environment.ProcessorCount * 100;  // 800
    
    public void QueueWorkItem(Action work)
    {
        if (_availableThreads.Count > 0)
        {
            var thread = _availableThreads.Dequeue();
            thread.Execute(work);
        }
        else if (_totalThreads < _maxThreads)
        {
            var newThread = CreateNewThread();
            newThread.Execute(work);
        }
        else
        {
            // استنى لحد ما thread يفضى
            _workQueue.Enqueue(work);
        }
    }
}
```

### Thread Pool Starvation

لما كل الـ Threads مشغولة:

```csharp
// ❌ غلط - بيسد الـ Thread
public string BadMethod()
{
    var result = HttpClient.GetStringAsync("https://api.com").Result;  // .Result بيسد!
    return result;
}

// ✅ صح - بيسيب الـ Thread
public async Task<string> GoodMethod()
{
    var result = await HttpClient.GetStringAsync("https://api.com");  // await بيحرر!
    return result;
}
```

---

## 🔌 الفصل 22: I/O Completion Ports (IOCP)

### إيه هو IOCP؟

آلية في Windows (وLinux ليها epoll) عشان تتعامل مع I/O Operations بكفاءة.

#### الفكرة:

بدل ما Thread يستنى الـ Database:
1. Thread بيطلب البيانات من الـ Database
2. بيسجل نفسه مع IOCP ويقول "نبهني لما يخلص"
3. Thread بيرجع للـ Thread Pool
4. الـ Database Driver بيشتغل
5. لما يخلص، IOCP بينبه الـ Thread Pool
6. أي Thread فاضي بيكمل الشغل

```csharp
// Simplified IOCP Flow
public class IOCompletionPort
{
    private Queue<IOOperation> _pendingOperations = new();
    
    public void RegisterOperation(IOOperation operation)
    {
        _pendingOperations.Enqueue(operation);
        
        // ابعت للـ OS Kernel
        NativeMethods.RegisterIOCompletion(
            operation.Handle,
            operation.Callback
        );
    }
    
    // لما الـ I/O يخلص
    private void OnIOComplete(IntPtr handle, byte[] data)
    {
        var operation = FindOperation(handle);
        
        // جيب أي Thread من الـ Pool
        ThreadPool.QueueUserWorkItem(() =>
        {
            operation.Callback(data);
        });
    }
}
```

---

## 🗄️ الفصل 23: Database Connection Pooling

### ليه Connection Pooling؟

فتح Connection جديد مكلف (ممكن ياخد 100-200ms). الـ Pooling بيخلي الـ Connections مفتوحة وجاهزة.

```csharp
public class ConnectionPool
{
    private Queue<SqlConnection> _availableConnections = new();
    private List<SqlConnection> _activeConnections = new();
    private readonly string _connectionString;
    private readonly int _minSize = 5;
    private readonly int _maxSize = 100;
    
    public async Task<SqlConnection> GetConnectionAsync()
    {
        // لو في connection فاضي
        if (_availableConnections.Count > 0)
        {
            var connection = _availableConnections.Dequeue();
            _activeConnections.Add(connection);
            return connection;
        }
        
        // لو لسه مش وصلنا للحد الأقصى
        if (_activeConnections.Count < _maxSize)
        {
            var newConnection = new SqlConnection(_connectionString);
            await newConnection.OpenAsync();
            _activeConnections.Add(newConnection);
            return newConnection;
        }
        
        // استنى لحد ما connection يرجع
        await WaitForAvailableConnection();
    }
    
    public void ReturnConnection(SqlConnection connection)
    {
        _activeConnections.Remove(connection);
        
        if (connection.State == ConnectionState.Open)
        {
            // نضفها وحطها في الـ Available
            connection.ResetConnection();
            _availableConnections.Enqueue(connection);
        }
        else
        {
            connection.Dispose();
        }
    }
}
```

### Connection String Settings

```csharp
"Server=localhost;Database=ShuryanDB;Trusted_Connection=true;
 Min Pool Size=5;        // أقل عدد connections
 Max Pool Size=100;      // أكتر عدد
 Connection Lifetime=300; // عمر الـ connection بالثواني
 Connection Timeout=30;   // وقت الانتظار
 Pooling=true;"          // تفعيل الـ pooling
```

---

## 💾 الفصل 24: Memory Management

### الـ Stack vs Heap

**Stack:** سريع، صغير، للـ Value Types
```csharp
public void Method()
{
    int x = 5;        // على الـ Stack
    bool flag = true; // على الـ Stack
    DateTime now = DateTime.Now; // على الـ Stack
} // كله بيتمسح تلقائي
```

**Heap:** كبير، للـ Reference Types
```csharp
public void Method()
{
    var user = new User();  // الـ Object على الـ Heap
    var list = new List<int>(); // على الـ Heap
} // الـ GC هو اللي بينضف
```

### Garbage Collection Generations

```csharp
// Gen 0: Objects جديدة (معظمها بيموت بسرعة)
var tempData = new byte[1024]; // Gen 0

// Gen 1: Objects عاشت شوية
_cachedData = LoadData(); // بعد أول GC → Gen 1

// Gen 2: Objects قديمة (long-lived)
public static readonly Dictionary<string, object> GlobalCache; // Gen 2
```

### الـ GC Process

```csharp
public class GarbageCollector
{
    public void Collect()
    {
        // 1. وقف كل الـ Threads (Stop The World)
        SuspendAllThreads();
        
        // 2. ابدأ من الـ Roots (Stack, Static fields)
        var roots = GetGCRoots();
        
        // 3. علم كل Object حي (Mark)
        foreach (var root in roots)
        {
            MarkReachable(root);
        }
        
        // 4. امسح الـ Objects الميتة (Sweep)
        foreach (var obj in HeapObjects)
        {
            if (!obj.IsMarked)
            {
                FreeMemory(obj);
            }
        }
        
        // 5. رتب الـ Memory (Compact)
        CompactMemory();
        
        // 6. شغل الـ Threads تاني
        ResumeAllThreads();
    }
}
```

### Memory Leaks الشائعة

**1. Event Handlers:**
```csharp
// ❌ غلط - Memory Leak
public class Publisher
{
    public event EventHandler DataChanged;
}

public class Subscriber
{
    public void Subscribe(Publisher pub)
    {
        pub.DataChanged += OnDataChanged; // الـ Publisher بيمسك reference
    }
    // لو نسيت unsubscribe، الـ Subscriber مش هيتمسح!
}

// ✅ صح
public void Unsubscribe(Publisher pub)
{
    pub.DataChanged -= OnDataChanged;
}
```

**2. Static Collections:**
```csharp
// ❌ غلط - بيكبر للأبد
public static List<LogEntry> AllLogs = new();

// ✅ صح - حد أقصى
public static Queue<LogEntry> RecentLogs = new(maxSize: 1000);
```

---

## ⚡ الفصل 25: Performance Optimization

### 1. Object Pooling

بدل ما تعمل Objects جديدة:

```csharp
public class ObjectPool<T>
{
    private readonly Stack<T> _pool = new();
    private readonly Func<T> _factory;
    
    public T Rent()
    {
        return _pool.Count > 0 ? _pool.Pop() : _factory();
    }
    
    public void Return(T item)
    {
        _pool.Push(item);
    }
}

// استخدام
var bufferPool = new ObjectPool<byte[]>(() => new byte[4096]);
var buffer = bufferPool.Rent();
// استخدم الـ buffer
bufferPool.Return(buffer);
```

### 2. Span<T> و Memory<T>

للتعامل مع الـ Memory من غير allocations:

```csharp
// ❌ غلط - بيعمل string جديد
public string ExtractSubstring(string input)
{
    return input.Substring(5, 10); // allocation!
}

// ✅ صح - من غير allocation
public ReadOnlySpan<char> ExtractSpan(ReadOnlySpan<char> input)
{
    return input.Slice(5, 10); // مجرد pointer!
}
```

### 3. ValueTask بدل Task

للـ hot paths:

```csharp
// ❌ Task دايماً بيعمل allocation
public async Task<int> GetCachedValueAsync()
{
    if (_cache.ContainsKey("key"))
        return _cache["key"]; // Task allocation عالفاضي
        
    return await LoadFromDbAsync();
}

// ✅ ValueTask مش بيعمل allocation لو النتيجة جاهزة
public async ValueTask<int> GetCachedValueAsync()
{
    if (_cache.ContainsKey("key"))
        return _cache["key"]; // مفيش allocation!
        
    return await LoadFromDbAsync();
}
```

---

## 🔄 الفصل 26: حالات حقيقية

### Scenario 1: JWT Authentication Flow

```
1. Browser بيبعت: POST /api/users/profile
   Headers: Authorization: Bearer [TOKEN]
   Body: {"name": "محمد"}
   
2. Kestrel يستقبل → HttpContext
   
3. Authentication Middleware:
   - يفك الـ JWT Token
   - يتأكد من التوقيع
   - يعمل ClaimsPrincipal
   
4. Authorization Middleware:
   - يتأكد من الـ Role
   
5. Controller:
   - Model Binding للـ JSON
   - Validation
   - Update في Database (async)
   
6. Response: 200 OK
```

### Scenario 2: Cookie Authentication Flow

```
1. Browser: GET /dashboard
   Cookies: .AspNetCore.Identity=[ENCRYPTED]
   
2. Cookie Authentication:
   - يفك تشفير الـ Cookie
   - يتأكد من SecurityStamp
   
3. Authorization:
   - يتأكد من Authenticated
   
4. Controller:
   - يجيب البيانات من Database
   - Razor View rendering
   
5. Response: HTML page
```

---

## 🚀 الفصل 27: Best Practices

### 1. Async All The Way

```csharp
// ❌ غلط - ممكن يعمل Deadlock
public string Bad()
{
    return GetDataAsync().Result; // خطر!
}

// ✅ صح
public async Task<string> Good()
{
    return await GetDataAsync();
}
```

### 2. استخدم Cancellation Tokens

```csharp
public async Task<User> GetUserAsync(int id, CancellationToken ct)
{
    // لو الـ Client قفل، وقف العملية
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Id == id, ct);
}
```

### 3. تجنب N+1 Queries

```csharp
// ❌ غلط - Query لكل Order
var users = await _context.Users.ToListAsync();
foreach (var user in users)
{
    var orders = await _context.Orders
        .Where(o => o.UserId == user.Id)
        .ToListAsync(); // N+1!
}

// ✅ صح - Query واحد
var users = await _context.Users
    .Include(u => u.Orders) // Join
    .ToListAsync();
```

### 4. استخدم Response Caching

```csharp
[HttpGet]
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
public async Task<IActionResult> GetProducts()
{
    // Cache لمدة 5 دقائق
}
```

---

## 🎯 خلاصة الخلاصات

### الـ Request Journey كاملة:

1. **Network Layer:** Packet → OS → Kestrel
2. **HTTP Parsing:** بناء HttpContext
3. **Middleware Pipeline:** كل واحد بيعمل شغلته
4. **Authentication:** تحديد الهوية
5. **Authorization:** التصريحات
6. **Routing:** تحديد الوجهة
7. **Controller:** 
   - Model Binding
   - Validation
   - Business Logic
8. **Database:**
   - LINQ → SQL
   - Connection Pool
   - Async I/O
9. **Response:** الرجوع بنفس الطريق
10. **Cleanup:** تنضيف الـ Resources

### الـ Threading Model:

- **Thread Pool:** مجموعة Threads جاهزة
- **Async/Await:** State Machine بيحرر Threads
- **IOCP:** آلية OS للـ Async I/O
- **No SynchronizationContext:** مفيش Thread Affinity

### الـ Memory Model:

- **Stack:** Value Types، سريع
- **Heap:** Reference Types، GC بينضفه
- **Generations:** Gen0 → Gen1 → Gen2
- **Pooling:** إعادة استخدام Objects

---

**كده احنا غطينا كل حاجة! 🎉**

من أول الـ Packet لحد الـ Response، ومن الـ Thread للـ Memory، ومن الـ Authentication للـ Database.

**الدروس المستفادة:**
- ✅ استخدم Async/Await دايماً
- ✅ خلي بالك من الـ Memory
- ✅ استخدم Pooling
- ✅ رتب الـ Middlewares صح
- ✅ استخدم Caching
- ✅ تجنب الـ Blocking Operations
