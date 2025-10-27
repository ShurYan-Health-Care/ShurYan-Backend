# ASP.NET Core Request Processing - Visual Architecture Diagrams

## Complete Request Flow Diagram

```mermaid
sequenceDiagram
    participant Client
    participant NIC as Network Card
    participant OS as OS TCP/IP Stack
    participant Kestrel
    participant Pipeline as Middleware Pipeline
    participant Controller
    participant Service as Business Logic
    participant DB as Database
    participant Cache as Cache Layer
    
    Client->>NIC: HTTP Request (TCP Packets)
    NIC->>OS: Ethernet Frames
    OS->>OS: TCP/IP Processing
    OS->>OS: SSL/TLS Decryption
    OS->>Kestrel: Socket Data
    
    Kestrel->>Kestrel: HTTP Parsing
    Kestrel->>Kestrel: Create HttpContext
    
    Kestrel->>Pipeline: Process Request
    
    Pipeline->>Pipeline: Exception Handler
    Pipeline->>Pipeline: HTTPS Redirect
    Pipeline->>Pipeline: CORS
    Pipeline->>Pipeline: Authentication
    Note over Pipeline: JWT/Cookie Validation
    Pipeline->>Pipeline: Authorization
    Pipeline->>Pipeline: Session
    Pipeline->>Pipeline: Routing
    
    Pipeline->>Controller: Invoke Action
    Controller->>Controller: Model Binding
    Controller->>Controller: Validation
    Controller->>Controller: Action Filters
    
    Controller->>Service: Business Logic
    Service->>Cache: Check Cache
    Cache-->>Service: Cache Miss
    
    Service->>DB: Query Data
    Note over DB: Async I/O
    DB-->>Service: Result Set
    
    Service->>Cache: Update Cache
    Service-->>Controller: DTO Result
    
    Controller->>Controller: Result Filters
    Controller-->>Pipeline: Action Result
    
    Pipeline->>Pipeline: Response Generation
    Pipeline->>Pipeline: JSON Serialization
    Pipeline-->>Kestrel: HTTP Response
    
    Kestrel->>OS: Response Data
    OS->>OS: SSL/TLS Encryption
    OS->>NIC: TCP Packets
    NIC->>Client: HTTP Response
```

## Thread Pool and Async Execution Flow

```mermaid
graph TD
    subgraph "Thread Pool"
        TP[Thread Pool Manager]
        GQ[Global Queue]
        LQ1[Local Queue 1]
        LQ2[Local Queue 2]
        LQ3[Local Queue 3]
        
        T1[Thread 1]
        T2[Thread 2]
        T3[Thread 3]
        TN[Thread N]
    end
    
    subgraph "Async Operation"
        REQ[HTTP Request]
        SM[State Machine]
        AW1[Await Point 1]
        AW2[Await Point 2]
        IOCP[I/O Completion Port]
    end
    
    REQ --> T1
    T1 --> SM
    SM --> AW1
    AW1 --> |Thread Returns| TP
    
    IOCP --> |I/O Complete| GQ
    GQ --> T2
    T2 --> SM
    SM --> AW2
    AW2 --> |Thread Returns| TP
    
    IOCP --> |I/O Complete| GQ
    GQ --> T3
    T3 --> |Complete Request| RESP[HTTP Response]
    
    style AW1 fill:#ff9999
    style AW2 fill:#ff9999
    style IOCP fill:#99ff99
```

## Memory Management Hierarchy

```mermaid
graph TB
    subgraph "Stack Memory"
        SV[Value Types]
        SP[Struct Parameters]
        LV[Local Variables]
    end
    
    subgraph "Heap Memory"
        subgraph "Generation 0"
            G0[New Objects < 85KB]
            REQ[Request Objects]
            DTO[DTOs]
        end
        
        subgraph "Generation 1"
            G1[Survived 1 GC]
            CACHE1[Request Cache]
        end
        
        subgraph "Generation 2"
            G2[Long-lived Objects]
            SERVICES[Singleton Services]
            CACHE2[Application Cache]
        end
        
        subgraph "Large Object Heap"
            LOH[Objects > 85KB]
            BUFFERS[Large Buffers]
            ARRAYS[Large Arrays]
        end
    end
    
    subgraph "Object Pools"
        HP[HttpContext Pool]
        BP[Buffer Pool]
        CP[Connection Pool]
    end
    
    G0 -->|Survives GC| G1
    G1 -->|Survives GC| G2
    BUFFERS -->|Return| BP
    REQ -->|Dispose| HP
```

## Middleware Pipeline Architecture

```mermaid
graph LR
    subgraph "Request Flow →"
        R[Request] --> EH[Exception Handler]
        EH --> HTTPS[HTTPS Redirect]
        HTTPS --> CORS[CORS]
        CORS --> AUTH[Authentication]
        AUTH --> AUTHZ[Authorization]
        AUTHZ --> SESSION[Session]
        SESSION --> ROUTING[Routing]
        ROUTING --> EP[Endpoint]
        EP --> ACTION[Controller Action]
    end
    
    subgraph "Response Flow ←"
        ACTION --> RESULT[Result Execution]
        RESULT --> SERIAL[Serialization]
        SERIAL --> COMPRESS[Compression]
        COMPRESS --> HEADERS[Response Headers]
        HEADERS --> CACHE[Response Cache]
        CACHE --> LOG[Logging]
        LOG --> RESP[Response]
    end
    
    style R fill:#99ff99
    style RESP fill:#9999ff
    style ACTION fill:#ffff99
```

## Database Connection Pool Management

```mermaid
stateDiagram-v2
    [*] --> Available: Connection Created
    Available --> Rented: Request Connection
    Rented --> InUse: Begin Transaction
    InUse --> InUse: Execute Queries
    InUse --> Rented: Commit/Rollback
    Rented --> Available: Return to Pool
    Rented --> Disposed: Connection Error
    Available --> Disposed: Idle Timeout
    Disposed --> [*]
    
    Available --> Available: Validation Check
    
    note right of Available
        Min Pool Size: 0
        Max Pool Size: 100
        Connection Lifetime: 0
    end note
    
    note right of InUse
        Command Timeout: 30s
        Connection Timeout: 15s
    end note
```

## JWT Authentication Flow

```mermaid
flowchart TD
    START[Request with JWT] --> EXTRACT[Extract Bearer Token]
    EXTRACT --> VALIDATE{Valid Token?}
    
    VALIDATE -->|No| UNAUTH[401 Unauthorized]
    VALIDATE -->|Yes| SIGNATURE[Verify Signature]
    
    SIGNATURE --> CLAIMS[Extract Claims]
    CLAIMS --> EXPIRY{Token Expired?}
    
    EXPIRY -->|Yes| REFRESH{Refresh Token?}
    EXPIRY -->|No| PRINCIPAL[Create ClaimsPrincipal]
    
    REFRESH -->|No| UNAUTH
    REFRESH -->|Yes| NEWTOKEN[Issue New Token]
    
    NEWTOKEN --> PRINCIPAL
    PRINCIPAL --> CONTEXT[Set HttpContext.User]
    CONTEXT --> CONTINUE[Continue Pipeline]
    
    style UNAUTH fill:#ff9999
    style CONTINUE fill:#99ff99
```

## EF Core Query Execution Pipeline

```mermaid
flowchart TB
    LINQ[LINQ Query] --> EXPR[Expression Tree]
    EXPR --> VISITOR[Query Visitor]
    VISITOR --> PROVIDER[Query Provider]
    PROVIDER --> TRANSLATOR[SQL Translator]
    TRANSLATOR --> SQL[SQL Command]
    
    SQL --> CACHE{Query Cache?}
    CACHE -->|Hit| CACHED[Use Cached Plan]
    CACHE -->|Miss| COMPILE[Compile Query]
    
    COMPILE --> CACHED
    CACHED --> CONN[Get Connection]
    
    CONN --> POOL{From Pool?}
    POOL -->|Yes| EXECUTE[Execute Query]
    POOL -->|No| CREATE[Create Connection]
    
    CREATE --> EXECUTE
    EXECUTE --> READER[Data Reader]
    
    READER --> MATERIALIZE[Materialize Objects]
    MATERIALIZE --> TRACKING{Track Changes?}
    
    TRACKING -->|Yes| TRACKER[Change Tracker]
    TRACKING -->|No| RESULT[Return Results]
    
    TRACKER --> RESULT
    
    style SQL fill:#ffff99
    style RESULT fill:#99ff99
```

## HTTP/2 Stream Multiplexing

```mermaid
graph TD
    subgraph "HTTP/2 Connection"
        subgraph "Stream 1"
            S1H[HEADERS Frame]
            S1D1[DATA Frame 1]
            S1D2[DATA Frame 2]
        end
        
        subgraph "Stream 3"
            S3H[HEADERS Frame]
            S3D[DATA Frame]
        end
        
        subgraph "Stream 5"
            S5H[HEADERS Frame]
            S5D1[DATA Frame 1]
            S5D2[DATA Frame 2]
            S5D3[DATA Frame 3]
        end
        
        CONTROL[Control Frames<br/>SETTINGS, PING, GOAWAY]
    end
    
    S1H --> MUX[Frame Multiplexer]
    S1D1 --> MUX
    S3H --> MUX
    S5H --> MUX
    S3D --> MUX
    S1D2 --> MUX
    S5D1 --> MUX
    S5D2 --> MUX
    S5D3 --> MUX
    CONTROL --> MUX
    
    MUX --> TCP[TCP Stream]
    
    style MUX fill:#ffff99
```

## Request Lifecycle State Machine

```mermaid
stateDiagram-v2
    [*] --> ConnectionAccepted
    ConnectionAccepted --> RequestParsing
    RequestParsing --> ContextCreated
    
    ContextCreated --> MiddlewareProcessing
    
    state MiddlewareProcessing {
        [*] --> ExceptionHandler
        ExceptionHandler --> Authentication
        Authentication --> Authorization
        Authorization --> Routing
        Routing --> ActionInvocation
    }
    
    ActionInvocation --> ModelBinding
    ModelBinding --> Validation
    Validation --> Filters
    Filters --> ActionExecution
    
    ActionExecution --> BusinessLogic
    
    state BusinessLogic {
        [*] --> ServiceCall
        ServiceCall --> DatabaseQuery
        DatabaseQuery --> CacheUpdate
        CacheUpdate --> [*]
    }
    
    BusinessLogic --> ResultGeneration
    ResultGeneration --> ResponseSerialization
    ResponseSerialization --> ResponseTransmission
    ResponseTransmission --> Cleanup
    Cleanup --> [*]
    
    RequestParsing --> ErrorResponse: Parse Error
    Authorization --> ErrorResponse: Unauthorized
    Validation --> ErrorResponse: Bad Request
    ActionExecution --> ErrorResponse: Exception
    
    ErrorResponse --> ResponseTransmission
```

## Performance Bottleneck Analysis

```mermaid
graph LR
    subgraph "Fast < 1ms"
        PARSE[HTTP Parsing]
        MW[Middleware]
        ROUTE[Routing]
        BIND[Model Binding]
    end
    
    subgraph "Medium 1-10ms"
        JWT[JWT Validation]
        CACHE[Cache Lookup]
        DBFAST[Indexed Query]
        JSON[JSON Serialization]
    end
    
    subgraph "Slow > 10ms"
        DBSLOW[Complex Query]
        DBJOIN[Multiple Joins]
        EXTERNAL[External API]
        RENDER[View Rendering]
    end
    
    subgraph "Critical Path"
        REQ[Request] --> PARSE
        PARSE --> MW
        MW --> JWT
        JWT --> ROUTE
        ROUTE --> BIND
        BIND --> DBSLOW
        DBSLOW --> JSON
        JSON --> RESP[Response]
    end
    
    style DBSLOW fill:#ff9999
    style DBJOIN fill:#ff9999
    style EXTERNAL fill:#ff9999
```

## Service Lifetime Scopes

```mermaid
graph TD
    subgraph "Application Lifetime"
        APP[Application Start]
        SINGLE[Singleton Services]
        APP --> SINGLE
    end
    
    subgraph "Request 1"
        REQ1[HTTP Request 1]
        SCOPE1[Scoped Container 1]
        TRANS1A[Transient A]
        TRANS1B[Transient B]
        
        REQ1 --> SCOPE1
        SCOPE1 --> TRANS1A
        SCOPE1 --> TRANS1B
    end
    
    subgraph "Request 2"
        REQ2[HTTP Request 2]
        SCOPE2[Scoped Container 2]
        TRANS2A[Transient A]
        TRANS2B[Transient B]
        
        REQ2 --> SCOPE2
        SCOPE2 --> TRANS2A
        SCOPE2 --> TRANS2B
    end
    
    SINGLE -.-> SCOPE1
    SINGLE -.-> SCOPE2
    
    SCOPE1 --> DISPOSE1[Dispose Scope 1]
    SCOPE2 --> DISPOSE2[Dispose Scope 2]
    
    style SINGLE fill:#9999ff
    style SCOPE1 fill:#99ff99
    style SCOPE2 fill:#99ff99
    style TRANS1A fill:#ffff99
    style TRANS1B fill:#ffff99
    style TRANS2A fill:#ffff99
    style TRANS2B fill:#ffff99
```

## Configuration System Flow

```mermaid
graph TD
    subgraph "Configuration Sources"
        JSON[appsettings.json]
        ENV[Environment Variables]
        SECRETS[User Secrets]
        CMD[Command Line Args]
        AZURE[Azure Key Vault]
    end
    
    subgraph "Configuration Builder"
        BUILDER[ConfigurationBuilder]
        PROVIDERS[Configuration Providers]
        ROOT[IConfigurationRoot]
    end
    
    subgraph "Configuration Binding"
        OPTIONS[IOptions<T>]
        SNAPSHOT[IOptionsSnapshot<T>]
        MONITOR[IOptionsMonitor<T>]
    end
    
    JSON --> BUILDER
    ENV --> BUILDER
    SECRETS --> BUILDER
    CMD --> BUILDER
    AZURE --> BUILDER
    
    BUILDER --> PROVIDERS
    PROVIDERS --> ROOT
    
    ROOT --> OPTIONS
    ROOT --> SNAPSHOT
    ROOT --> MONITOR
    
    OPTIONS --> |Singleton| SERVICE1[Service]
    SNAPSHOT --> |Scoped| SERVICE2[Service]
    MONITOR --> |Change Notification| SERVICE3[Service]
    
    style ROOT fill:#ffff99
```

## Complete System Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        BROWSER[Browser]
        MOBILE[Mobile App]
        API[API Client]
    end
    
    subgraph "Network Layer"
        LB[Load Balancer]
        CDN[CDN]
        WAF[Web Application Firewall]
    end
    
    subgraph "Application Layer"
        subgraph "Kestrel Server"
            HTTP[HTTP Handler]
            MW[Middleware Pipeline]
            MVC[MVC Framework]
        end
    end
    
    subgraph "Business Layer"
        SERVICES[Services]
        REPOS[Repositories]
        DOMAIN[Domain Logic]
    end
    
    subgraph "Data Layer"
        EF[Entity Framework Core]
        REDIS[Redis Cache]
        SQL[SQL Server]
        BLOB[Blob Storage]
    end
    
    subgraph "Infrastructure"
        LOG[Logging]
        METRIC[Metrics]
        TRACE[Distributed Tracing]
        HEALTH[Health Checks]
    end
    
    BROWSER --> CDN
    MOBILE --> WAF
    API --> WAF
    
    CDN --> LB
    WAF --> LB
    LB --> HTTP
    
    HTTP --> MW
    MW --> MVC
    MVC --> SERVICES
    
    SERVICES --> REPOS
    SERVICES --> DOMAIN
    REPOS --> EF
    
    EF --> SQL
    SERVICES --> REDIS
    SERVICES --> BLOB
    
    SERVICES -.-> LOG
    SERVICES -.-> METRIC
    SERVICES -.-> TRACE
    MW -.-> HEALTH
    
    style LB fill:#99ff99
    style SERVICES fill:#ffff99
    style SQL fill:#9999ff
```
