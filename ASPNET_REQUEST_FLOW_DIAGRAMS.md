# ASP.NET Core Request Flow - الرسومات والـ Diagrams

## 📊 الـ Diagrams بالتفصيل

---

## 1️⃣ الـ Complete Request Flow

```mermaid
sequenceDiagram
    participant Browser
    participant NIC as Network Card
    participant OS as Operating System
    participant Kestrel
    participant MW as Middleware Pipeline
    participant Controller
    participant DB as Database
    
    Browser->>NIC: TCP Packets
    NIC->>OS: Network Interrupts
    OS->>OS: TCP/IP Stack Processing
    OS->>Kestrel: Socket Connection
    
    Note over Kestrel: SSL/TLS Handshake
    
    Kestrel->>Kestrel: Parse HTTP Request
    Kestrel->>Kestrel: Create HttpContext
    
    Kestrel->>MW: Start Pipeline
    
    MW->>MW: ExceptionHandler
    MW->>MW: HTTPS Redirection
    MW->>MW: CORS
    MW->>MW: Authentication
    Note right of MW: JWT/Cookie Validation
    MW->>MW: Authorization
    Note right of MW: Role/Policy Check
    MW->>MW: Routing
    Note right of MW: Endpoint Selection
    
    MW->>Controller: Execute Action
    
    Controller->>Controller: Model Binding
    Controller->>Controller: Validation
    Controller->>Controller: Filters
    
    Controller->>DB: Query (Async)
    Note right of DB: Thread Returns to Pool
    DB-->>Controller: Results
    Note right of Controller: Thread Resumes
    
    Controller-->>MW: Action Result
    
    MW-->>MW: Result Execution
    MW-->>MW: Response Filters
    MW-->>Kestrel: Complete Response
    
    Kestrel-->>OS: Send Response
    OS-->>NIC: TCP Packets
    NIC-->>Browser: HTTP Response
    
    Note over Kestrel: Cleanup & Disposal
```

---

## 2️⃣ الـ Middleware Pipeline بالتفصيل

```mermaid
graph TD
    Request[HTTP Request] --> Exception[Exception Handler]
    
    Exception --> HTTPS{HTTPS?}
    HTTPS -->|No| Redirect[301 Redirect]
    HTTPS -->|Yes| CORS[CORS Middleware]
    
    CORS --> Preflight{Preflight?}
    Preflight -->|Yes| PreflightResp[204 Response]
    Preflight -->|No| Auth[Authentication]
    
    Auth --> JWT{Has Token?}
    JWT -->|No| Anonymous[Anonymous User]
    JWT -->|Yes| Validate[Validate Token]
    
    Validate --> Valid{Valid?}
    Valid -->|No| Unauthorized[401 Unauthorized]
    Valid -->|Yes| SetUser[Set HttpContext.User]
    
    Anonymous --> Authorization
    SetUser --> Authorization[Authorization Middleware]
    
    Authorization --> Authorized{Authorized?}
    Authorized -->|No| Forbidden[403 Forbidden]
    Authorized -->|Yes| Routing[Routing]
    
    Routing --> Match{Route Match?}
    Match -->|No| NotFound[404 Not Found]
    Match -->|Yes| Endpoint[Endpoint Execution]
    
    Endpoint --> Controller[Controller Action]
    Controller --> Response[HTTP Response]
    
    style Request fill:#e1f5fe
    style Response fill:#c8e6c9
    style Unauthorized fill:#ffcdd2
    style Forbidden fill:#ffcdd2
    style NotFound fill:#ffcdd2
```

---

## 3️⃣ الـ Async State Machine

```mermaid
stateDiagram-v2
    [*] --> Initial: Method Called
    
    Initial --> Await1: Start Execution
    
    Await1 --> Suspended1: await DbContext.FindAsync()
    Suspended1 --> ThreadPool1: Return Thread
    
    ThreadPool1 --> IOComplete1: I/O Completion
    IOComplete1 --> Resume1: Get Thread
    
    Resume1 --> Await2: Continue Execution
    
    Await2 --> Suspended2: await SaveChangesAsync()
    Suspended2 --> ThreadPool2: Return Thread
    
    ThreadPool2 --> IOComplete2: I/O Completion
    IOComplete2 --> Resume2: Get Thread
    
    Resume2 --> Complete: Return Result
    Complete --> [*]
    
    note right of Suspended1
        Thread returns to pool
        State saved in heap
    end note
    
    note right of IOComplete1
        IOCP signals completion
        Any thread can continue
    end note
```

---

## 4️⃣ الـ Thread Pool Lifecycle

```mermaid
graph LR
    subgraph Thread Pool
        Available[Available Threads<br/>Min: 8, Max: 800]
        Busy[Busy Threads]
        Queue[Work Queue]
    end
    
    Request1[Request 1] --> CheckPool{Thread<br/>Available?}
    Request2[Request 2] --> CheckPool
    Request3[Request 3] --> CheckPool
    
    CheckPool -->|Yes| Assign[Assign Thread]
    CheckPool -->|No| CheckMax{Below Max?}
    
    CheckMax -->|Yes| Create[Create New Thread]
    CheckMax -->|No| Enqueue[Add to Queue]
    
    Assign --> Execute[Execute Work]
    Create --> Execute
    Enqueue --> Wait[Wait for Thread]
    
    Execute --> AsyncIO{Async I/O?}
    AsyncIO -->|Yes| Release[Release Thread]
    AsyncIO -->|No| Complete[Complete Work]
    
    Release --> Available
    Complete --> Return[Return to Pool]
    Return --> Available
    
    Wait --> Available
    
    style Available fill:#c8e6c9
    style Busy fill:#ffeb3b
    style Queue fill:#ffcdd2
```

---

## 5️⃣ الـ Database Operation Flow

```mermaid
sequenceDiagram
    participant Controller
    participant DbContext
    participant ChangeTracker
    participant QueryProvider
    participant ConnectionPool
    participant SqlServer
    
    Controller->>DbContext: Users.Where(u => u.Age > 18)
    
    DbContext->>QueryProvider: Build Expression Tree
    QueryProvider->>QueryProvider: Translate to SQL
    
    Note over QueryProvider: SELECT * FROM Users<br/>WHERE Age > 18
    
    QueryProvider->>ConnectionPool: Get Connection
    
    alt Connection Available
        ConnectionPool-->>QueryProvider: Existing Connection
    else No Connection
        ConnectionPool->>SqlServer: Open New Connection
        SqlServer-->>ConnectionPool: Connection Opened
        ConnectionPool-->>QueryProvider: New Connection
    end
    
    QueryProvider->>SqlServer: Execute SQL Query
    
    Note over SqlServer: Query Execution
    
    SqlServer-->>QueryProvider: Result Set
    QueryProvider->>DbContext: Materialize Entities
    
    DbContext->>ChangeTracker: Track Entities
    ChangeTracker->>ChangeTracker: Create Snapshots
    
    DbContext-->>Controller: Return Entities
    
    Controller->>DbContext: Modify Entity
    DbContext->>ChangeTracker: Detect Changes
    
    Controller->>DbContext: SaveChangesAsync()
    
    ChangeTracker->>ChangeTracker: Compare Current vs Original
    ChangeTracker->>QueryProvider: Generate UPDATE SQL
    
    QueryProvider->>SqlServer: Execute UPDATE
    SqlServer-->>QueryProvider: Rows Affected
    
    QueryProvider->>ConnectionPool: Return Connection
    
    DbContext-->>Controller: Save Complete
```

---

## 6️⃣ الـ Memory Management

```mermaid
graph TD
    subgraph Stack
        LocalVars[Local Variables<br/>int, bool, structs]
        MethodParams[Method Parameters]
        ReturnAddr[Return Addresses]
    end
    
    subgraph Heap
        subgraph "Generation 0"
            NewObj[New Objects<br/>Temp Data]
        end
        
        subgraph "Generation 1"
            SurvivedObj[Survived Objects<br/>Request Data]
        end
        
        subgraph "Generation 2"
            OldObj[Old Objects<br/>Singletons, Caches]
        end
        
        subgraph "Large Object Heap"
            LargeObj[Objects > 85KB<br/>Big Arrays]
        end
    end
    
    NewObj -->|Survives GC| SurvivedObj
    SurvivedObj -->|Survives GC| OldObj
    
    GC[Garbage Collector] --> Gen0Collect[Collect Gen 0<br/>~1ms]
    GC --> Gen1Collect[Collect Gen 1<br/>~10ms]
    GC --> Gen2Collect[Collect Gen 2<br/>~100ms]
    
    Gen0Collect --> NewObj
    Gen1Collect --> SurvivedObj
    Gen2Collect --> OldObj
    
    style NewObj fill:#ffeb3b
    style SurvivedObj fill:#ff9800
    style OldObj fill:#f44336
    style LargeObj fill:#9c27b0
```

---

## 7️⃣ الـ JWT Authentication Flow

```mermaid
flowchart TD
    Start[Request with JWT] --> Extract[Extract Bearer Token]
    
    Extract --> Validate{Token Present?}
    Validate -->|No| NoAuth[No Authentication]
    Validate -->|Yes| Parse[Parse JWT Parts]
    
    Parse --> Header[Decode Header]
    Parse --> Payload[Decode Payload]
    Parse --> Signature[Verify Signature]
    
    Header --> Algorithm{Algorithm Valid?}
    Algorithm -->|No| Invalid[Invalid Token]
    Algorithm -->|Yes| CheckSign
    
    Signature --> CheckSign{Signature Valid?}
    CheckSign -->|No| Invalid
    CheckSign -->|Yes| CheckClaims
    
    Payload --> CheckClaims{Claims Valid?}
    
    CheckClaims --> Issuer{Issuer OK?}
    Issuer -->|No| Invalid
    Issuer -->|Yes| Audience
    
    Audience{Audience OK?} -->|No| Invalid
    Audience -->|Yes| Expiry
    
    Expiry{Not Expired?} -->|No| Expired[Token Expired]
    Expiry -->|Yes| CreatePrincipal
    
    CreatePrincipal[Create ClaimsPrincipal] --> SetUser[Set HttpContext.User]
    
    SetUser --> Success[Authenticated]
    NoAuth --> Continue[Continue as Anonymous]
    Invalid --> Fail[401 Unauthorized]
    Expired --> Fail
    
    style Success fill:#c8e6c9
    style Fail fill:#ffcdd2
    style Continue fill:#fff3e0
```

---

## 8️⃣ الـ Connection Pool Management

```mermaid
stateDiagram-v2
    [*] --> Idle: Connection Created
    
    Idle --> InUse: Request Connection
    InUse --> Executing: Execute Query
    Executing --> InUse: Query Complete
    
    InUse --> Idle: Return to Pool
    
    Idle --> Validation: Periodic Check
    Validation --> Valid: Connection OK
    Validation --> Invalid: Connection Dead
    
    Valid --> Idle
    Invalid --> Disposed: Remove from Pool
    
    Idle --> Timeout: Max Lifetime
    Timeout --> Disposed
    
    Disposed --> [*]
    
    note right of Idle
        Min Pool Size: 5
        Max Pool Size: 100
    end note
    
    note right of Validation
        Check every 30s
        Ping database
    end note
```

---

## 9️⃣ الـ Model Binding Process

```mermaid
flowchart LR
    Request[HTTP Request] --> Sources{Binding Source?}
    
    Sources --> Body[FromBody]
    Sources --> Query[FromQuery]
    Sources --> Route[FromRoute]
    Sources --> Header[FromHeader]
    Sources --> Form[FromForm]
    
    Body --> JSON[Parse JSON]
    Query --> QueryString[Parse Query String]
    Route --> RouteData[Extract Route Values]
    Header --> Headers[Read Headers]
    Form --> FormData[Parse Form Data]
    
    JSON --> Deserialize[Deserialize to Object]
    QueryString --> Convert[Type Conversion]
    RouteData --> Convert
    Headers --> Convert
    FormData --> Convert
    
    Deserialize --> Validate[Model Validation]
    Convert --> Validate
    
    Validate --> Valid{Valid?}
    Valid -->|Yes| Success[Bind Success]
    Valid -->|No| ModelState[ModelState Errors]
    
    Success --> Action[Execute Action]
    ModelState --> BadRequest[400 Bad Request]
```

---

## 🔟 الـ Response Generation Pipeline

```mermaid
graph TB
    Action[Action Result] --> Type{Result Type?}
    
    Type --> OK[OkObjectResult]
    Type --> BadReq[BadRequestResult]
    Type --> NotFound[NotFoundResult]
    Type --> File[FileResult]
    Type --> View[ViewResult]
    
    OK --> JSON[JSON Serialization]
    BadReq --> Problem[Problem Details]
    NotFound --> Status404[404 Status]
    File --> Stream[File Stream]
    View --> Razor[Razor Rendering]
    
    JSON --> Format[Content Negotiation]
    Problem --> Format
    Status404 --> Headers[Set Headers]
    Stream --> Headers
    Razor --> HTML[HTML Output]
    HTML --> Headers
    
    Format --> Headers
    
    Headers --> Body[Write Body]
    Body --> Compress{Compression?}
    
    Compress -->|Yes| Gzip[Gzip/Brotli]
    Compress -->|No| Raw[Raw Response]
    
    Gzip --> Send[Send to Client]
    Raw --> Send
    
    Send --> TCP[TCP Packets]
    TCP --> Network[Network Transmission]
    
    style Action fill:#e3f2fd
    style Send fill:#c8e6c9
```

---

## 1️⃣1️⃣ الـ Complete Architecture Overview

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Browser/Mobile App]
        HTTP[HTTP Client]
    end
    
    subgraph "Network Layer"
        Internet[Internet]
        LoadBalancer[Load Balancer]
    end
    
    subgraph "Web Server Layer"
        Kestrel1[Kestrel Instance 1]
        Kestrel2[Kestrel Instance 2]
        Kestrel3[Kestrel Instance 3]
    end
    
    subgraph "Application Layer"
        subgraph "Middleware Pipeline"
            Auth[Authentication]
            Author[Authorization]
            Routing[Routing]
        end
        
        subgraph "MVC"
            Controllers[Controllers]
            Services[Services]
            Repos[Repositories]
        end
    end
    
    subgraph "Data Layer"
        EFCore[Entity Framework Core]
        Redis[Redis Cache]
        SqlServer[(SQL Server)]
    end
    
    Browser --> HTTP
    HTTP --> Internet
    Internet --> LoadBalancer
    
    LoadBalancer --> Kestrel1
    LoadBalancer --> Kestrel2
    LoadBalancer --> Kestrel3
    
    Kestrel1 --> Auth
    Kestrel2 --> Auth
    Kestrel3 --> Auth
    
    Auth --> Author
    Author --> Routing
    Routing --> Controllers
    
    Controllers --> Services
    Services --> Repos
    
    Repos --> EFCore
    Services --> Redis
    
    EFCore --> SqlServer
    
    style Browser fill:#e3f2fd
    style SqlServer fill:#fff3e0
    style Redis fill:#ffebee
```

---

## 1️⃣2️⃣ الـ Performance Bottlenecks

```mermaid
graph LR
    subgraph "Common Bottlenecks"
        ThreadPool[Thread Pool<br/>Starvation]
        Memory[Memory<br/>Pressure]
        Database[Database<br/>N+1 Queries]
        Network[Network<br/>Latency]
        CPU[CPU<br/>Intensive]
    end
    
    subgraph "Solutions"
        Async[Use Async/Await]
        Pool[Object Pooling]
        Include[Use Include()]
        Cache[Response Caching]
        Background[Background Jobs]
    end
    
    ThreadPool --> Async
    Memory --> Pool
    Database --> Include
    Network --> Cache
    CPU --> Background
    
    style ThreadPool fill:#ffcdd2
    style Memory fill:#ffcdd2
    style Database fill:#ffcdd2
    style Network fill:#ffcdd2
    style CPU fill:#ffcdd2
    
    style Async fill:#c8e6c9
    style Pool fill:#c8e6c9
    style Include fill:#c8e6c9
    style Cache fill:#c8e6c9
    style Background fill:#c8e6c9
```

---

## 📊 خلاصة الـ Diagrams

الـ Diagrams دي بتوضح:

1. **Request Flow:** من البراوزر للـ Database والرجوع
2. **Middleware Pipeline:** كل خطوة بالتفصيل
3. **Async State Machine:** إزاي الـ Threads بتتحرر
4. **Thread Pool:** إدارة الـ Threads
5. **Database Operations:** من LINQ لـ SQL
6. **Memory Management:** Generations والـ GC
7. **JWT Authentication:** التحقق من الهوية
8. **Connection Pool:** إدارة الاتصالات
9. **Model Binding:** تحويل البيانات
10. **Response Generation:** بناء الرد
11. **Architecture Overview:** الصورة الكاملة
12. **Performance:** المشاكل والحلول

---

**كده كل حاجة بقت واضحة بالرسم! 🎨**
