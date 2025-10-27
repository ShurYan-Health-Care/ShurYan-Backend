# ASP.NET Core Request Processing Architecture: The Definitive Guide
## Part 2: MVC Pipeline, Data Access & Response Generation

## MVC/API Controller Pipeline

### Stage 8: Routing and Endpoint Selection

**Components**: EndpointRoutingMiddleware, EndpointMiddleware, RouteTable

```csharp
public class EndpointRoutingMiddleware {
    private readonly EndpointDataSource _endpointDataSource;
    private readonly MatcherFactory _matcherFactory;
    
    public async Task InvokeAsync(HttpContext httpContext) {
        // 1. Create matcher (cached after first use)
        var matcher = _matcherFactory.CreateMatcher(_endpointDataSource);
        
        // 2. Find matching endpoint
        await matcher.MatchAsync(httpContext);
        
        // 3. Store endpoint in HttpContext
        var endpoint = httpContext.GetEndpoint();
        if (endpoint != null) {
            // Extract route values
            var routeValues = httpContext.Request.RouteValues;
            
            // Log endpoint selection
            _logger.LogDebug("Request matched endpoint: {EndpointName}", 
                endpoint.DisplayName);
        }
        
        await _next(httpContext);
    }
}
```

**Route Matching Algorithm**:

```csharp
public class DfaMatcher : Matcher {
    private readonly DfaTree _dfaTree;  // Precompiled route tree
    
    public override Task MatchAsync(HttpContext httpContext) {
        var path = httpContext.Request.Path.Value;
        var method = httpContext.Request.Method;
        
        // 1. DFA traversal for path matching
        var candidates = _dfaTree.FindCandidates(path);
        
        // 2. Apply constraints
        candidates = ApplyConstraints(candidates, httpContext);
        
        // 3. HTTP method filtering
        candidates = FilterByHttpMethod(candidates, method);
        
        // 4. Select best match (lowest order wins)
        var endpoint = SelectBestCandidate(candidates);
        
        if (endpoint != null) {
            httpContext.SetEndpoint(endpoint);
            
            // Extract route parameters
            ExtractRouteValues(httpContext, endpoint, path);
        }
        
        return Task.CompletedTask;
    }
    
    private void ExtractRouteValues(HttpContext context, Endpoint endpoint, string path) {
        var template = endpoint.Metadata.GetMetadata<RoutePattern>();
        var values = new RouteValueDictionary();
        
        // Parse path segments
        var pathSegments = path.Split('/');
        var templateSegments = template.Segments;
        
        for (int i = 0; i < templateSegments.Count; i++) {
            var segment = templateSegments[i];
            if (segment.IsParameter) {
                values[segment.Name] = pathSegments[i];
                
                // Type conversion will happen in model binding
                context.Request.RouteValues[segment.Name] = pathSegments[i];
            }
        }
    }
}
```

**Memory Impact**:
- DFA tree: ~100KB for typical application (100 routes)
- Route value dictionary: ~500 bytes per request
- Endpoint metadata cache: Shared across requests

---

### Stage 9: Controller Instantiation

```csharp
public class ControllerActionInvoker : IActionInvoker {
    private readonly ControllerContext _controllerContext;
    private readonly IControllerFactory _controllerFactory;
    
    public async Task InvokeAsync() {
        // 1. Create controller instance
        var controller = _controllerFactory.CreateController(_controllerContext);
        
        try {
            // 2. Property injection (if needed)
            await InitializeControllerAsync(controller);
            
            // 3. Execute filters and action
            await InvokeFilterPipelineAsync();
        }
        finally {
            // 4. Release controller
            _controllerFactory.ReleaseController(_controllerContext, controller);
        }
    }
    
    private async Task InitializeControllerAsync(object controller) {
        if (controller is ControllerBase controllerBase) {
            // Inject HttpContext, User, Url helper, etc.
            controllerBase.ControllerContext = _controllerContext;
            controllerBase.Url = _urlHelperFactory.GetUrlHelper(_controllerContext);
            
            // Execute IAsyncActionFilter on controller if present
            if (controller is IAsyncActionFilter filter) {
                await filter.OnActionExecutionAsync(
                    _controllerContext, 
                    () => Task.CompletedTask);
            }
        }
    }
}
```

**Dependency Injection Resolution**:

```csharp
public class DefaultControllerActivator : IControllerActivator {
    public object Create(ControllerContext context) {
        var controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();
        
        // Get constructor
        var constructors = controllerType.GetConstructors();
        var constructor = constructors.Single();  // Should have exactly one
        
        // Resolve constructor parameters from DI
        var services = context.HttpContext.RequestServices;
        var parameters = constructor.GetParameters();
        var arguments = new object[parameters.Length];
        
        for (int i = 0; i < parameters.Length; i++) {
            arguments[i] = services.GetRequiredService(parameters[i].ParameterType);
        }
        
        // Create instance
        var controller = Activator.CreateInstance(controllerType, arguments);
        
        // Property injection
        InjectProperties(controller, services);
        
        return controller;
    }
}
```

---

### Stage 10: Model Binding

**Model Binding Sources** (in priority order):
1. [FromBody] - Request body (JSON/XML)
2. [FromForm] - Form data
3. [FromRoute] - Route values
4. [FromQuery] - Query string
5. [FromHeader] - HTTP headers
6. [FromServices] - Dependency injection

```csharp
public class ModelBinder {
    private readonly List<IValueProvider> _valueProviders;
    private readonly IModelMetadataProvider _metadataProvider;
    
    public async Task<ModelBindingResult> BindModelAsync(
        ModelBindingContext bindingContext) {
        
        // 1. Get model metadata
        var metadata = _metadataProvider.GetMetadataForType(bindingContext.ModelType);
        
        // 2. Create model instance
        var model = CreateModel(bindingContext.ModelType);
        
        // 3. Bind each property
        foreach (var property in metadata.Properties) {
            var propertyBinder = GetPropertyBinder(property);
            
            // Try each value provider
            foreach (var valueProvider in _valueProviders) {
                var value = await valueProvider.GetValueAsync(property.Name);
                if (value != ValueProviderResult.None) {
                    // Convert and set value
                    var convertedValue = ConvertValue(value, property.ModelType);
                    property.SetValue(model, convertedValue);
                    break;
                }
            }
        }
        
        return ModelBindingResult.Success(model);
    }
}
```

**Complex Type Binding** (JSON from Body):

```csharp
public class JsonBodyModelBinder : IModelBinder {
    public async Task BindModelAsync(ModelBindingContext bindingContext) {
        var request = bindingContext.HttpContext.Request;
        
        // Enable request buffering for re-reading
        request.EnableBuffering();
        
        try {
            // Read body stream
            using var reader = new StreamReader(
                request.Body, 
                encoding: Encoding.UTF8, 
                detectEncodingFromByteOrderMarks: false, 
                leaveOpen: true);
            
            var json = await reader.ReadToEndAsync();
            
            // Deserialize
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var model = JsonSerializer.Deserialize(
                json, 
                bindingContext.ModelType, 
                options);
            
            bindingContext.Result = ModelBindingResult.Success(model);
        }
        finally {
            // Reset stream position for potential re-reading
            request.Body.Position = 0;
        }
    }
}
```

**Memory Allocations**:
- JSON deserialization: Size of object graph + ~20% overhead
- String allocations for each property
- Temporary buffers: 16KB default for body reading

---

### Stage 11: Model Validation

```csharp
public class ModelValidator {
    public ValidationResult Validate(object model, ModelMetadata metadata) {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        
        // 1. Data Annotations validation
        foreach (var property in metadata.Properties) {
            var value = property.GetValue(model);
            var propertyContext = new ValidationContext(model) {
                MemberName = property.Name
            };
            
            // Check each validation attribute
            var attributes = property.ValidatorMetadata.OfType<ValidationAttribute>();
            foreach (var attribute in attributes) {
                var result = attribute.GetValidationResult(value, propertyContext);
                if (result != ValidationResult.Success) {
                    results.Add(result);
                }
            }
        }
        
        // 2. IValidatableObject validation
        if (model is IValidatableObject validatable) {
            var customResults = validatable.Validate(context);
            results.AddRange(customResults);
        }
        
        // 3. Populate ModelState
        foreach (var result in results) {
            foreach (var memberName in result.MemberNames) {
                modelState.AddModelError(memberName, result.ErrorMessage);
            }
        }
        
        return results.Count == 0 
            ? ValidationResult.Success 
            : new ValidationResult(results);
    }
}
```

**Custom Validation Example**:

```csharp
public class UserModel : IValidatableObject {
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; }
    
    public string ConfirmPassword { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext context) {
        if (Password != ConfirmPassword) {
            yield return new ValidationResult(
                "Passwords do not match", 
                new[] { nameof(ConfirmPassword) });
        }
        
        // Business logic validation
        var dbContext = context.GetService<AppDbContext>();
        var emailExists = dbContext.Users.Any(u => u.Email == Email);
        
        if (emailExists) {
            yield return new ValidationResult(
                "Email already exists", 
                new[] { nameof(Email) });
        }
    }
}
```

---

### Stage 12: Action Filters

**Filter Pipeline Execution Order**:

```
Request →
    Authorization Filters →
        Resource Filters →
            Model Binding →
                Action Filters (OnExecuting) →
                    Action Method →
                Action Filters (OnExecuted) →
            Exception Filters →
        Resource Filters →
    Result Filters →
Response
```

```csharp
public class ActionFilterPipeline {
    public async Task<IActionResult> InvokeAsync() {
        // 1. Authorization Filters
        foreach (var filter in GetFilters<IAuthorizationFilter>()) {
            await filter.OnAuthorizationAsync(context);
            if (context.Result != null) return context.Result; // Short-circuit
        }
        
        // 2. Resource Filters (Before)
        foreach (var filter in GetFilters<IResourceFilter>()) {
            filter.OnResourceExecuting(context);
            if (context.Result != null) return context.Result;
        }
        
        try {
            // 3. Action Filters (Before)
            foreach (var filter in GetFilters<IActionFilter>()) {
                filter.OnActionExecuting(context);
                if (context.Result != null) return context.Result;
            }
            
            // 4. Execute Action Method
            var result = await ExecuteActionAsync();
            
            // 5. Action Filters (After) - reverse order
            foreach (var filter in GetFilters<IActionFilter>().Reverse()) {
                filter.OnActionExecuted(context);
            }
            
            // 6. Result Filters
            foreach (var filter in GetFilters<IResultFilter>()) {
                filter.OnResultExecuting(resultContext);
                if (resultContext.Cancel) return;
            }
            
            // Execute result
            await result.ExecuteResultAsync(context);
            
            foreach (var filter in GetFilters<IResultFilter>().Reverse()) {
                filter.OnResultExecuted(resultContext);
            }
        }
        catch (Exception ex) {
            // 7. Exception Filters
            foreach (var filter in GetFilters<IExceptionFilter>()) {
                filter.OnException(exceptionContext);
                if (exceptionContext.ExceptionHandled) break;
            }
        }
        finally {
            // 8. Resource Filters (After) - reverse order
            foreach (var filter in GetFilters<IResourceFilter>().Reverse()) {
                filter.OnResourceExecuted(context);
            }
        }
    }
}
```

**Common Filter Examples**:

```csharp
// Caching Filter
public class CacheFilter : IAsyncActionFilter {
    private readonly IMemoryCache _cache;
    
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next) {
        
        // Generate cache key
        var cacheKey = GenerateCacheKey(context);
        
        // Try get from cache
        if (_cache.TryGetValue(cacheKey, out var cachedResult)) {
            context.Result = new OkObjectResult(cachedResult);
            return; // Short-circuit
        }
        
        // Execute action
        var executedContext = await next();
        
        // Cache result
        if (executedContext.Result is OkObjectResult okResult) {
            _cache.Set(cacheKey, okResult.Value, TimeSpan.FromMinutes(5));
        }
    }
}

// Performance Logging Filter
public class PerformanceFilter : IAsyncActionFilter {
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next) {
        
        var stopwatch = Stopwatch.StartNew();
        
        var executedContext = await next();
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 500) {
            _logger.LogWarning(
                "Slow request: {Action} took {ElapsedMs}ms",
                context.ActionDescriptor.DisplayName,
                stopwatch.ElapsedMilliseconds);
        }
        
        // Add timing header
        context.HttpContext.Response.Headers["X-Response-Time"] = 
            $"{stopwatch.ElapsedMilliseconds}ms";
    }
}
```

---

## Business Logic & Data Access Layer

### Stage 13: Service Layer Execution

```csharp
public class UserService : IUserService {
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    private readonly IMemoryCache _cache;
    
    public async Task<UserDto> UpdateUserProfileAsync(int userId, UpdateProfileDto dto) {
        // 1. Begin unit of work
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try {
            // 2. Load entity with tracking
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) {
                throw new NotFoundException($"User {userId} not found");
            }
            
            // 3. Business logic validation
            if (dto.Email != user.Email) {
                var emailExists = await _userRepository.EmailExistsAsync(dto.Email);
                if (emailExists) {
                    throw new ValidationException("Email already in use");
                }
            }
            
            // 4. Update entity
            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.ModifiedDate = DateTime.UtcNow;
            
            // 5. Persist changes
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
            
            // 6. Commit transaction
            await transaction.CommitAsync();
            
            // 7. Invalidate cache
            _cache.Remove($"user_{userId}");
            
            // 8. Return DTO
            return _mapper.Map<UserDto>(user);
        }
        catch {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

---

### Stage 14: Entity Framework Core Processing

**DbContext Lifecycle**:

```csharp
public class AppDbContext : DbContext {
    private readonly ChangeTracker _changeTracker;
    private readonly DatabaseFacade _database;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder
            .UseSqlServer(connectionString, options => {
                options.EnableRetryOnFailure(3);
                options.CommandTimeout(30);
            })
            .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
            .EnableSensitiveDataLogging(isDevelopment)
            .EnableServiceProviderCaching()
            .UseMemoryCache(cache);
    }
}
```

**Query Translation Pipeline**:

```csharp
// LINQ Query
var query = dbContext.Users
    .Include(u => u.Roles)
    .ThenInclude(r => r.Permissions)
    .Where(u => u.IsActive && u.Email.Contains("@example"))
    .OrderBy(u => u.CreatedDate)
    .Skip(20)
    .Take(10);

// Expression Tree (simplified)
Expression.Call(
    Expression.Call(
        Expression.Call(
            Expression.Constant(dbContext.Users),
            "Where",
            Expression.Lambda(
                Expression.AndAlso(
                    Expression.Property(param, "IsActive"),
                    Expression.Call(
                        Expression.Property(param, "Email"),
                        "Contains",
                        Expression.Constant("@example"))
                )
            )
        ),
        "OrderBy",
        Expression.Lambda(Expression.Property(param, "CreatedDate"))
    ),
    "Skip",
    Expression.Constant(20)
);

// Generated SQL
/*
SELECT [t].[Id], [t].[Email], [t].[IsActive], [t].[CreatedDate],
       [r].[Id], [r].[UserId], [r].[RoleId],
       [p].[Id], [p].[RoleId], [p].[Name]
FROM (
    SELECT [u].[Id], [u].[Email], [u].[IsActive], [u].[CreatedDate]
    FROM [Users] AS [u]
    WHERE [u].[IsActive] = 1 
      AND [u].[Email] LIKE N'%@example%'
    ORDER BY [u].[CreatedDate]
    OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
) AS [t]
LEFT JOIN [UserRoles] AS [r] ON [t].[Id] = [r].[UserId]
LEFT JOIN [Permissions] AS [p] ON [r].[RoleId] = [p].[RoleId]
ORDER BY [t].[CreatedDate], [t].[Id], [r].[Id]
*/
```

**Async Database Operations**:

```csharp
public async Task<List<User>> GetUsersAsync() {
    // 1. Build expression tree
    var query = BuildQueryExpression();
    
    // 2. Translate to SQL
    var sqlCommand = TranslateToSql(query);
    
    // 3. Get database connection from pool
    var connection = await GetPooledConnectionAsync();
    
    // 4. Create command
    using var command = connection.CreateCommand();
    command.CommandText = sqlCommand.Sql;
    command.Parameters = sqlCommand.Parameters;
    
    // 5. Execute async (releases thread)
    var tcs = new TaskCompletionSource<DbDataReader>();
    
    // Register for I/O completion
    command.BeginExecuteReader(ar => {
        try {
            var reader = command.EndExecuteReader(ar);
            tcs.SetResult(reader);
        }
        catch (Exception ex) {
            tcs.SetException(ex);
        }
    }, null);
    
    // Thread returns to pool here
    var reader = await tcs.Task;
    
    // 6. Materialize results (thread pool thread)
    var results = new List<User>();
    while (await reader.ReadAsync()) {
        var user = MaterializeUser(reader);
        results.Add(user);
        
        // Track entity if needed
        if (QueryTrackingBehavior == QueryTrackingBehavior.TrackAll) {
            _changeTracker.TrackEntity(user);
        }
    }
    
    return results;
}
```

**Change Tracking**:

```csharp
public class ChangeTracker {
    private readonly Dictionary<object, EntityEntry> _trackedEntities;
    private readonly Dictionary<object, object> _originalValues;
    
    public void DetectChanges() {
        foreach (var entry in _trackedEntities.Values) {
            if (entry.State == EntityState.Unchanged ||
                entry.State == EntityState.Modified) {
                
                var currentValues = entry.CurrentValues;
                var originalValues = entry.OriginalValues;
                
                foreach (var property in entry.Metadata.GetProperties()) {
                    var currentValue = currentValues[property];
                    var originalValue = originalValues[property];
                    
                    if (!Equals(currentValue, originalValue)) {
                        entry.State = EntityState.Modified;
                        entry.ModifiedProperties.Add(property);
                    }
                }
            }
        }
    }
    
    public async Task<int> SaveChangesAsync() {
        DetectChanges();
        
        var entries = _trackedEntities.Values
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();
        
        using var transaction = await _database.BeginTransactionAsync();
        
        try {
            int affected = 0;
            
            // Generate SQL for each change
            foreach (var entry in entries) {
                switch (entry.State) {
                    case EntityState.Added:
                        affected += await ExecuteInsertAsync(entry);
                        break;
                    case EntityState.Modified:
                        affected += await ExecuteUpdateAsync(entry);
                        break;
                    case EntityState.Deleted:
                        affected += await ExecuteDeleteAsync(entry);
                        break;
                }
            }
            
            await transaction.CommitAsync();
            
            // Accept changes
            foreach (var entry in entries) {
                entry.AcceptChanges();
            }
            
            return affected;
        }
        catch {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

**Connection Pool Management**:

```csharp
public class SqlConnectionPool {
    private readonly ConcurrentBag<PooledConnection> _available;
    private readonly SemaphoreSlim _poolSemaphore;
    private readonly ConnectionPoolSettings _settings;
    
    public async Task<PooledConnection> RentConnectionAsync() {
        // Wait if pool is exhausted
        await _poolSemaphore.WaitAsync();
        
        try {
            // Try get existing connection
            if (_available.TryTake(out var connection)) {
                if (connection.IsAlive()) {
                    return connection;
                }
                connection.Dispose();
            }
            
            // Create new connection
            var newConnection = new SqlConnection(_connectionString);
            await newConnection.OpenAsync();
            
            return new PooledConnection(newConnection, this);
        }
        catch {
            _poolSemaphore.Release();
            throw;
        }
    }
    
    public void ReturnConnection(PooledConnection connection) {
        if (connection.IsAlive() && _available.Count < _settings.MaxPoolSize) {
            connection.Reset(); // Clear state
            _available.Add(connection);
        } else {
            connection.Dispose();
        }
        
        _poolSemaphore.Release();
    }
}
```

---

## Response Generation

### Stage 15: Action Result Execution

```csharp
public abstract class ActionResult : IActionResult {
    public abstract Task ExecuteResultAsync(ActionContext context);
}

public class ObjectResult : ActionResult {
    public object Value { get; set; }
    public int? StatusCode { get; set; }
    
    public override async Task ExecuteResultAsync(ActionContext context) {
        // 1. Content negotiation
        var selectedFormatter = SelectFormatter(context);
        
        // 2. Set status code
        context.HttpContext.Response.StatusCode = StatusCode ?? 200;
        
        // 3. Serialize and write
        await selectedFormatter.WriteAsync(context, Value);
    }
    
    private IOutputFormatter SelectFormatter(ActionContext context) {
        var accept = context.HttpContext.Request.Headers["Accept"];
        
        // Match against available formatters
        foreach (var formatter in _formatters) {
            if (formatter.CanWriteType(Value?.GetType())) {
                foreach (var mediaType in formatter.SupportedMediaTypes) {
                    if (accept.Contains(mediaType)) {
                        return formatter;
                    }
                }
            }
        }
        
        // Default to JSON
        return _jsonFormatter;
    }
}
```

**JSON Serialization**:

```csharp
public class SystemTextJsonOutputFormatter : IOutputFormatter {
    private readonly JsonSerializerOptions _options;
    
    public async Task WriteAsync(OutputFormatterWriteContext context) {
        var response = context.HttpContext.Response;
        
        // Set content type
        response.ContentType = "application/json; charset=utf-8";
        
        // Configure response
        response.Headers["Cache-Control"] = "no-cache";
        
        // Serialize directly to response stream
        await JsonSerializer.SerializeAsync(
            response.Body,
            context.Object,
            context.ObjectType,
            _options,
            context.HttpContext.RequestAborted);
        
        // No need to flush - handled by Kestrel
    }
}
```

**Response Buffering vs Streaming**:

```csharp
// Buffered Response (default for small responses)
public async Task WriteBufferedResponse(HttpContext context, object data) {
    using var buffer = new MemoryStream();
    
    // Serialize to buffer
    await JsonSerializer.SerializeAsync(buffer, data);
    
    // Set Content-Length header
    context.Response.ContentLength = buffer.Length;
    
    // Write to response
    buffer.Position = 0;
    await buffer.CopyToAsync(context.Response.Body);
}

// Streaming Response (for large data)
public async Task WriteStreamingResponse(HttpContext context, IAsyncEnumerable<T> data) {
    context.Response.ContentType = "application/json";
    // No Content-Length header - chunked encoding
    
    await context.Response.WriteAsync("[");
    
    bool first = true;
    await foreach (var item in data) {
        if (!first) await context.Response.WriteAsync(",");
        
        await JsonSerializer.SerializeAsync(
            context.Response.Body, 
            item);
        
        first = false;
        
        // Flush periodically
        if (++count % 100 == 0) {
            await context.Response.Body.FlushAsync();
        }
    }
    
    await context.Response.WriteAsync("]");
}
```

---

### Stage 16: Response Transmission

**Response Pipeline (Reverse Middleware)**:

```csharp
// Middleware executes in reverse order for response
ResponseCompressionMiddleware
    ← ResponseCachingMiddleware
        ← CustomHeadersMiddleware
            ← LoggingMiddleware
```

**Kestrel Response Writing**:

```csharp
public class Http1Connection {
    private readonly Pipe _transport;
    
    public async Task WriteResponseAsync(HttpResponse response) {
        var writer = _transport.Output;
        
        // 1. Write status line
        WriteAsciiString(writer, "HTTP/1.1 ");
        WriteNumeric(writer, response.StatusCode);
        WriteAsciiString(writer, " ");
        WriteAsciiString(writer, ReasonPhrases.GetReasonPhrase(response.StatusCode));
        WriteAsciiString(writer, "\r\n");
        
        // 2. Write headers
        foreach (var header in response.Headers) {
            WriteAsciiString(writer, header.Key);
            WriteAsciiString(writer, ": ");
            WriteAsciiString(writer, header.Value);
            WriteAsciiString(writer, "\r\n");
        }
        
        // 3. End headers
        WriteAsciiString(writer, "\r\n");
        
        // 4. Flush headers
        await writer.FlushAsync();
        
        // 5. Write body
        if (response.Body != null) {
            await response.Body.CopyToAsync(writer);
        }
        
        // 6. Final flush
        await writer.FlushAsync();
    }
}
```

---

## Cleanup & Resource Disposal

### Stage 17: Request Cleanup

```csharp
public class RequestCleanup {
    public async Task CleanupAsync(HttpContext context) {
        try {
            // 1. Dispose request scope
            if (context.RequestServices is IDisposable disposable) {
                disposable.Dispose();
            }
            
            // 2. Dispose tracked services
            foreach (var service in _scopedServices) {
                if (service is IAsyncDisposable asyncDisposable) {
                    await asyncDisposable.DisposeAsync();
                } else if (service is IDisposable sync) {
                    sync.Dispose();
                }
            }
            
            // 3. Return DbContext to pool
            if (_dbContext != null) {
                _dbContextPool.Return(_dbContext);
            }
            
            // 4. Complete request metrics
            _metrics.RequestCompleted(
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                _stopwatch.ElapsedMilliseconds);
            
            // 5. Return HttpContext to pool
            _httpContextPool.Return(context);
            
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during request cleanup");
        }
    }
}
```

**Memory Cleanup Timeline**:

```
Immediate (< 1ms):
- Request scope disposal
- Transient services disposal
- HttpContext returned to pool

Deferred (GC dependent):
- Unreferenced objects → Gen0 collection (~10ms)
- Large objects → LOH collection (~100ms)
- Finalizer queue processing

Connection pooling:
- DbContext → Pool (immediate)
- SQL connections → Pool (immediate)
- HTTP clients → Pool (immediate)
```
