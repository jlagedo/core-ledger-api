Here's the revised implementation plan with the project layers pattern requirement:
You are an expert .NET 10 backend engineer. Modify an existing .NET 10 Web API project by adding a new controller and supporting services to implement real-time notifications using Server-Sent Events (SSE) and Redis Pub/Sub.

Do NOT rewrite or restructure the existing API. Only add the necessary files, services, and controller.

REQUIREMENTS
------------

0. PROJECT STRUCTURE
   - Before writing any code, analyze the existing project structure and patterns
   - Identify the layering convention used (e.g., Clean Architecture, Onion, N-Tier, Vertical Slices)
   - Identify folder organization patterns (e.g., by feature, by type, by layer)
   - Identify naming conventions for:
       - Interfaces (e.g., IService, IRepository)
       - Implementations (e.g., Service, Repository)
       - Models/DTOs/Records
       - Configuration classes
       - Extensions
   - Identify existing patterns for:
       - Dependency injection registration
       - Options/configuration binding
       - BackgroundService implementations
       - Controller structure and routing conventions
   - All new code MUST follow the identified patterns exactly
   - Place files in appropriate directories matching existing conventions
   - If project uses separate projects/assemblies for layers, respect those boundaries

1. SSE ENDPOINT
   - Add GET /notifications?userId={id}
   - Must return "text/event-stream" with headers:
       - Content-Type: text/event-stream
       - Cache-Control: no-cache
       - Connection: keep-alive
   - Must send SSE-formatted messages: "data: {json}\n\n"
   - Must send heartbeat comment every 15 seconds: ": heartbeat\n\n"
   - Heartbeats must NOT update LastActivity (only real messages should)
   - Must flush after each message and heartbeat
   - Must use context.RequestAborted to detect disconnects
   - Must remove the user channel on disconnect (in finally block)
   - Must not block threads (use async Channel.Reader.ReadAllAsync with cancellation)
   - Must handle Channel completion gracefully (exit loop cleanly)
   - Must be implemented inside a new controller class, not minimal APIs
   - Must log connection open/close events with userId

2. CHANNEL STORE (per API instance)
   - Add a new service: INotificationChannelStore + NotificationChannelStore
   - Store: userId → UserChannel (ConcurrentDictionary<string, UserChannel>)
   - UserChannel contains:
       - Channel<string> Channel (BoundedChannel with configurable capacity, default 100)
       - DateTime LastActivity (updated only on successful message write)
       - DateTime CreatedAt
   - BoundedChannel must use BoundedChannelFullMode.DropOldest when full
   - Must be thread-safe
   - Must be registered as a Singleton in DI
   - Must expose:
       - UserChannel GetOrCreate(string userId)
       - bool TryGet(string userId, out UserChannel? channel)
       - bool Remove(string userId)
       - IEnumerable<KeyValuePair<string, UserChannel>> GetAll()
       - int ActiveCount { get; }
   - Must log when channels are created or removed

3. REDIS PUB/SUB SUBSCRIBER
   - Add a BackgroundService: RedisSubscriberService
   - Subscribe to Redis channel (configurable, default: "jobs.status-change")
   - Deserialize messages into JobStatusChangeMessage record
   - On deserialization failure: log warning and skip message (do not crash)
   - If the user is connected on this API instance:
       - Attempt to write message into the user's Channel<string>
       - Update LastActivity only on successful write
       - If write fails (channel full after drop or completed): remove the channel
   - Must handle Redis disconnections gracefully:
       - Auto-reconnect with exponential backoff (1s, 2s, 4s, max 30s)
       - Log connection state changes (connected, disconnected, reconnecting)
       - Use StackExchange.Redis connection multiplexer events
   - Must respond to host shutdown CancellationToken
   - Must not block threads
   - Must be registered in DI as a hosted service

4. HEARTBEAT SERVICE
   - Add a BackgroundService: HeartbeatService
   - Runs every 15 seconds (configurable)
   - Iterates all active channels and writes ": heartbeat\n\n"
   - Heartbeat writes must NOT update LastActivity
   - On write failure: log and continue (SSE endpoint will handle cleanup)
   - Must respond to host shutdown CancellationToken

5. IDLE CHANNEL CLEANUP SERVICE
   - Add a BackgroundService: ChannelCleanupService
   - Runs every 30 seconds (configurable)
   - Removes channels idle for more than configured timeout (default: 15 minutes)
   - Idle means: (UtcNow - LastActivity) > IdleTimeout
   - On removal: complete the channel to signal SSE endpoint to close
   - Must log each channel removal with userId and idle duration
   - Must respond to host shutdown CancellationToken

6. MESSAGE CONTRACTS
   - JobStatusChangeMessage record (must match existing worker publisher format exactly):
       ```csharp
       public record JobStatusChangeMessage(
           int UserId,
           Guid JobId,
           string Message,
           string Status,
           string? CorrelationId
       );
       ```
   - SSE message format sent to client:
       ```
       event: jobStatusChange
       id: {JobId}
       data: {"userId":1000,"jobId":"...","message":"...","status":"...","correlationId":"..."}

       ```
   - Heartbeat format: ": heartbeat\n\n"

7. CONFIGURATION
   - Add NotificationOptions class with IOptions<T> pattern:
       ```csharp
       public class NotificationOptions
       {
           public string RedisChannel { get; set; } = "jobs.status-change";
           public int ChannelCapacity { get; set; } = 100;
           public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(15);
           public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromSeconds(30);
           public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(15);
           public TimeSpan RedisReconnectMaxDelay { get; set; } = TimeSpan.FromSeconds(30);
       }
       ```
   - Must be configurable via appsettings.json under "Notifications" section
   - Must be registered in DI following existing configuration patterns

8. GRACEFUL SHUTDOWN
   - All BackgroundServices must respond to IHostApplicationLifetime cancellation
   - On shutdown:
       - Stop accepting new SSE connections (return 503)
       - Complete all active channels to signal SSE endpoints
       - Allow in-flight messages to drain (up to 5 seconds)
       - Unsubscribe from Redis cleanly
   - Add a shutdown flag to INotificationChannelStore: bool IsShuttingDown { get; }

9. ERROR HANDLING
   - Redis deserialization failure: log warning, skip message, continue
   - Channel write failure: log, remove channel, continue
   - SSE write failure: log, clean up channel, exit endpoint gracefully
   - Redis connection failure: log error, retry with backoff, do not crash service
   - Never throw exceptions that crash BackgroundServices

10. OBSERVABILITY
    - Add structured logging (ILogger<T>) to all services
    - Log events:
        - SSE connection opened (Info): userId
        - SSE connection closed (Info): userId, duration, messageCount
        - Channel created (Debug): userId
        - Channel removed (Debug): userId, reason (disconnect/idle/shutdown)
        - Redis message received (Debug): userId, jobId
        - Redis connection state change (Warning/Info): state
        - Errors (Error): with exception details
    - Expose metrics endpoint or properties:
        - Active SSE connections count
        - Messages sent (counter)
        - Channel cleanup count (counter)
    - Include CorrelationId in logs when available

11. STABILITY REQUIREMENTS
    - SSE must not timeout or block the thread pool
    - Must handle 5000+ concurrent SSE connections
    - Must clean up channels on disconnect, idle timeout, or shutdown
    - Must work with horizontal autoscaling (no sticky sessions)
    - Redis Pub/Sub must be the only cross-instance event router
    - No memory leaks from abandoned channels

12. CODE QUALITY
    - Use .NET 10 conventions and C# 13 features where appropriate
    - Use dependency injection everywhere
    - No static classes or static state
    - Use async/await everywhere (no sync-over-async)
    - Use CancellationToken in all async methods
    - Add XML documentation comments to public interfaces
    - Add only the new files and registrations required
    - Do not modify existing controllers or services
    - Follow Microsoft naming conventions

DELIVERABLES
------------

First, analyze the existing project and document:
- Project structure and layering pattern
- Folder organization
- Naming conventions
- Existing similar implementations to use as reference

Then produce the following files (paths shown are examples - adjust to match project patterns):

1.  JobStatusChangeMessage record
    - Place in appropriate Models/DTOs/Contracts folder per project convention
    - Record definition matching existing worker publisher format:
      ```csharp
      public record JobStatusChangeMessage(
          int UserId,
          Guid JobId,
          string Message,
          string Status,
          string? CorrelationId
      );
      ```

2.  SseMessage helper
    - Helper class/record for formatting SSE messages
    - Place in appropriate location per project convention

3.  NotificationOptions configuration
    - Configuration POCO with defaults
    - Place in appropriate Configuration/Options folder per project convention

4.  INotificationChannelStore interface
    - Interface definition with XML docs
    - Place in appropriate Interfaces/Abstractions folder per project convention

5.  NotificationChannelStore implementation
    - Singleton implementation with ConcurrentDictionary
    - Place in appropriate Services/Infrastructure folder per project convention

6.  UserChannel class
    - Class containing BoundedChannel and metadata
    - Place alongside NotificationChannelStore or in Models per project convention

7.  RedisSubscriberService
    - BackgroundService for Redis subscription with reconnection logic
    - Place in appropriate BackgroundServices/HostedServices folder per project convention

8.  HeartbeatService
    - BackgroundService for SSE heartbeats
    - Place alongside other BackgroundServices per project convention

9.  ChannelCleanupService
    - BackgroundService for idle channel removal
    - Place alongside other BackgroundServices per project convention

10. NotificationsController
    - SSE endpoint implementation
    - Place in Controllers folder following existing controller patterns
    - Note: userId parameter is int type to match message format

11. Service registration extension
    - Extension method for IServiceCollection
    - Place in appropriate Extensions folder per project convention
    - Follow existing DI registration patterns

12. README-notifications.md
    - Brief documentation on usage and configuration
    - Place in project root or docs folder

13. Example appsettings.json section
    - Show all configurable options with comments

EXAMPLE REGISTRATIONS (to be added to Program.cs)
-------------------------------------------------

```csharp
// Add before builder.Build()
// Follow existing registration patterns in Program.cs
builder.Services.AddNotificationServices(builder.Configuration);

// Ensure Redis connection is configured (if not already present)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
```

EXAMPLE APPSETTINGS.JSON
------------------------

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Notifications": {
    "RedisChannel": "jobs.status-change",
    "ChannelCapacity": 100,
    "IdleTimeoutMinutes": 15,
    "CleanupIntervalSeconds": 30,
    "HeartbeatIntervalSeconds": 15,
    "RedisReconnectMaxDelaySeconds": 30
  }
}
```

CONSTRAINTS
-----------

- Target framework: .NET 10
- Required NuGet packages (assume already installed or add to list):
    - StackExchange.Redis (>= 2.7.0)
    - System.Threading.Channels (built-in)
    - Microsoft.Extensions.Hosting (built-in)
- Do not use SignalR
- Do not use WebSockets
- Do not use third-party SSE libraries
- Must compile without warnings
- Must not break existing API functionality
- Must follow existing project structure and patterns exactly

EXISTING INFRASTRUCTURE
-----------------------

The following already exists and should be used:
- Redis Pub/Sub publisher in worker service publishing to "jobs.status-change" channel
- Message format published by worker:
  ```csharp
  var message = new
  {
      UserId = 1000,        // int
      JobId = coreJob.Id,   // Guid
      Message = coreJob.JobDescription,  // string
      Status = coreJob.Status.ToString(), // string
      CorrelationId = correlationId       // string?
  };
  ```
- IConnectionMultiplexer registration in DI (verify and use existing)

The final output must be a complete, production-ready implementation that can be added to an existing .NET 10 API as a self-contained feature while maintaining consistency with the existing codebase architecture and conventions.
