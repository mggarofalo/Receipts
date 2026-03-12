---
identifier: MGG-101
title: "Improve Logging & Security Practices"
id: d97fae43-7db0-40b0-a145-9e5685fb85ff
status: Done
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - triage
milestone: "Phase 8: Security Automation"
url: "https://linear.app/mggarofalo/issue/MGG-101/improve-logging-and-security-practices"
gitBranchName: mggarofalo/mgg-101-improve-logging-security-practices
createdAt: "2026-02-15T15:39:52.946Z"
updatedAt: "2026-03-04T16:33:39.372Z"
completedAt: "2026-03-04T16:33:39.352Z"
attachments:
  - title: "feat(api): add correlation IDs, global exception handling, and security event logging (MGG-101)"
    url: "https://github.com/mggarofalo/Receipts/pull/69"
---

# Improve Logging & Security Practices

**Title:** `feat(security): enhance API logging with correlation IDs, performance metrics, and security events`

**Description:**

### **Current State**

* Basic structured logging with request parameters
* Debug/Warning/Error levels appropriately used
* Exception logging in catch blocks
* Missing correlation IDs for request tracing
* No performance metrics logging
* Limited security-relevant event logging

### **Problems**

1. **Request Tracing**: Cannot correlate logs across service calls without correlation IDs
2. **Performance Monitoring**: No visibility into request duration or resource usage
3. **Security Auditing**: Missing logs for authentication/authorization events
4. **Error Context**: Limited context in error logs for debugging
5. **Log Volume**: Debug logs may be too verbose in production

### **Proposed Improvements**

#### **1. Add Correlation ID Middleware**

```csharp
// Add to Program.cs
builder.Services.AddHttpContextAccessor();
// Create CorrelationIdMiddleware
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or use existing correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

#### **2. Enhanced Controller Logging**

```csharp
public async Task<ActionResult<ReceiptWithItemsResponse>> GetReceiptWithItemsByReceiptId([FromRoute] Guid receiptId)
{
    var startTime = Stopwatch.GetTimestamp();
    
    try
    {
        logger.LogInformation("Processing GetReceiptWithItemsByReceiptId request", 
            new { receiptId, userId = User?.Identity?.Name });
        
        // ... existing logic ...
        
        var duration = Stopwatch.GetElapsedTime(startTime);
        logger.LogInformation("GetReceiptWithItemsByReceiptId completed successfully", 
            new { receiptId, durationMs = duration.TotalMilliseconds });
            
        return Ok(model);
    }
    catch (Exception ex)
    {
        var duration = Stopwatch.GetElapsedTime(startTime);
        logger.LogError(ex, "GetReceiptWithItemsByReceiptId failed", 
            new { receiptId, durationMs = duration.TotalMilliseconds, errorType = ex.GetType().Name });
        throw;
    }
}
```

#### **3. Security Event Logging**

```csharp
// In authentication middleware or controllers
logger.LogInformation("User authentication successful", 
    new { userId, ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() });
logger.LogWarning("Unauthorized access attempt", 
    new { requestedResource, userId, ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() });
```

#### **4. Structured Error Responses**

```csharp
catch (Exception ex)
{
    var errorId = Guid.NewGuid().ToString();
    logger.LogError(ex, "Request failed with error ID: {ErrorId}", errorId);
    
    return StatusCode(500, new 
    { 
        ErrorId = errorId,
        Message = "An error occurred while processing your request."
    });
}
```

### **Acceptance Criteria**

- [ ] Correlation IDs added to all requests
- [ ] Request duration logged for performance monitoring
- [ ] Security events (auth/authz failures) logged
- [ ] Error logs include sufficient context for debugging
- [ ] Log levels appropriate for production use
- [ ] Consider log aggregation/monitoring integration

### **Technical Details**

* Use Serilog or built-in .NET logging with enrichers
* Consider `Microsoft.Extensions.Logging` structured logging
* Add `System.Diagnostics.Stopwatch` for performance metrics
* Implement correlation ID propagation for future microservices

**Labels:** `backend`, `security`, `improvement`, `logging`

**Priority:** Medium

**Milestone:** Phase 6 (Correctness Hardening)

**Estimate:** 2-3 days

---

## 🎯 **Current Logging Assessment**

**✅ Good Practices:**

* Structured logging with placeholders (prevents injection)
* Appropriate log levels (Debug/Warning/Error)
* Exception logging with full context
* Using `nameof()` for method names

**🔄 Areas for Improvement:**

* Add correlation IDs for request tracing
* Log request performance metrics
* Enhanced security event logging
* More context in error scenarios
* Consider production log levels
