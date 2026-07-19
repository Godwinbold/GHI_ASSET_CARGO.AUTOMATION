## Audit Logging Implementation Guide

This document provides step-by-step instructions for integrating audit logging into your service layer.

### Overview
- **AuditLog Entity**: Stores audit trail data
- **IAuditService**: Interface for audit operations
- **AuditService**: Implementation handling audit logging
- **AuditHelper**: Utility for change tracking
- **BaseEntity.LastUpdatedBy**: Tracks which user made the last update

---

### Integration Steps

#### 1. Add Audit Service Dependency
Inject `IAuditService` into your service constructor:

```csharp
public class ShipmentService : IShipmentService
{
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public ShipmentService(
        IRepository repository, 
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }
}
```

---

### 2. Extract User Info from HttpContext (In Controller)

Before calling service methods, extract user information:

```csharp
[HttpPost("shipments/create")]
public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentDto dto)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
    var userName = User.FindFirst("FullName")?.Value;
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

    var result = await _shipmentService.CreateShipmentAsync(
        dto, 
        userId, 
        userEmail, 
        userName, 
        ipAddress);
    
    // ... rest of controller logic
}
```

---

### 3. Log on CREATE Operation

```csharp
public async Task<Result<ShipmentResponseDto>> CreateShipmentAsync(
    CreateShipmentDto dto,
    string userId,
    string userEmail,
    string userName,
    string ipAddress)
{
    try
    {
        var shipment = new Shipment
        {
            AirlineId = Guid.Parse(dto.AirlineId),
            AirwayBillNumber = dto.AirwayBillNumber,
            Status = ShipmentStatus.Pending,
            ShipmentDate = dto.ShipmentDate
        };

        await _repository.Add(shipment);
        await _unitOfWork.SaveChangesAsync();

        // Log audit trail
        await _auditService.LogAuditAsync(
            userId: Guid.Parse(userId),
            userName: userName,
            userEmail: userEmail,
            action: "Create",
            entityName: nameof(Shipment),
            entityId: shipment.Id,
            changes: $"Shipment created: AWB {shipment.AirwayBillNumber}",
            ipAddress: ipAddress
        );

        return Result<ShipmentResponseDto>.Success(MapToResponse(shipment));
    }
    catch (Exception ex)
    {
        return new Error[] { new("Error", ex.Message) };
    }
}
```

---

### 4. Log on UPDATE Operation

```csharp
public async Task<Result<ShipmentResponseDto>> UpdateShipmentAsync(
    Guid shipmentId,
    UpdateShipmentDto dto,
    string userId,
    string userEmail,
    string userName,
    string ipAddress)
{
    try
    {
        var shipment = await _repository.FindById<Shipment>(shipmentId);
        if (shipment == null)
            return new Error[] { new("NotFound", "Shipment not found") };

        // Store old values for audit trail
        var oldValues = new Dictionary<string, object?>
        {
            { nameof(Shipment.Status), shipment.Status },
            { nameof(Shipment.ShipmentDate), shipment.ShipmentDate }
        };

        // Update entity
        shipment.Status = dto.Status ?? shipment.Status;
        shipment.ShipmentDate = dto.ShipmentDate ?? shipment.ShipmentDate;
        shipment.UpdatedDate = DateTimeOffset.UtcNow;
        shipment.LastUpdatedBy = Guid.Parse(userId);

        _repository.Update(shipment);
        await _unitOfWork.SaveChangesAsync();

        // Get new values
        var newValues = new Dictionary<string, object?>
        {
            { nameof(Shipment.Status), shipment.Status },
            { nameof(Shipment.ShipmentDate), shipment.ShipmentDate }
        };

        // Generate human-readable change summary
        var changeSummary = AuditHelper.GetChangeSummary(oldValues, newValues);

        // Log audit trail
        await _auditService.LogAuditAsync(
            userId: Guid.Parse(userId),
            userName: userName,
            userEmail: userEmail,
            action: "Update",
            entityName: nameof(Shipment),
            entityId: shipment.Id,
            changes: changeSummary,
            oldValues: AuditHelper.SerializeToJson(oldValues),
            newValues: AuditHelper.SerializeToJson(newValues),
            ipAddress: ipAddress
        );

        return Result<ShipmentResponseDto>.Success(MapToResponse(shipment));
    }
    catch (Exception ex)
    {
        return new Error[] { new("Error", ex.Message) };
    }
}
```

---

### 5. Log on DELETE Operation

```csharp
public async Task<Result> DeleteShipmentAsync(
    Guid shipmentId,
    string userId,
    string userEmail,
    string userName,
    string ipAddress)
{
    try
    {
        var shipment = await _repository.FindById<Shipment>(shipmentId);
        if (shipment == null)
            return new Error[] { new("NotFound", "Shipment not found") };

        var shipmentData = $"AWB: {shipment.AirwayBillNumber}, Status: {shipment.Status}";

        _repository.Remove(shipment);
        await _unitOfWork.SaveChangesAsync();

        // Log deletion
        await _auditService.LogAuditAsync(
            userId: Guid.Parse(userId),
            userName: userName,
            userEmail: userEmail,
            action: "Delete",
            entityName: nameof(Shipment),
            entityId: shipmentId,
            changes: $"Shipment deleted: {shipmentData}",
            oldValues: AuditHelper.SerializeToJson(new Dictionary<string, object?> 
            { 
                { "AWB", shipment.AirwayBillNumber },
                { "Status", shipment.Status }
            }),
            ipAddress: ipAddress
        );

        return Result.Success();
    }
    catch (Exception ex)
    {
        return new Error[] { new("Error", ex.Message) };
    }
}
```

---

### 6. Query Audit Logs (Admin Endpoint)

The audit logs can be queried via:

**GET** `/api/admin/audit-logs`

**Query Parameters:**
- `userId` (optional): Filter by user
- `action` (optional): Filter by action type (Create, Update, Delete)
- `entityName` (optional): Filter by entity (Shipment, Airline, etc.)
- `fromDate` (optional): Filter from date
- `toDate` (optional): Filter to date
- `page` (default: 1): Page number
- `pageSize` (default: 20): Items per page

**Example Requests:**
```
# Get all audit logs
GET /api/admin/audit-logs

# Get all updates by specific user
GET /api/admin/audit-logs?userId=<user-id>&action=Update

# Get shipment deletions in date range
GET /api/admin/audit-logs?entityName=Shipment&action=Delete&fromDate=2026-01-01&toDate=2026-12-31

# Get paginated results
GET /api/admin/audit-logs?page=2&pageSize=50
```

---

### Database Migration

Run a database migration to add the new `AuditLog` table and `LastUpdatedBy` column:

```bash
dotnet ef migrations add AddAuditLogging
dotnet ef database update
```

---

### Best Practices

1. **Always set `LastUpdatedBy`** when updating entities
2. **Capture IP Address** at the controller level and pass to service
3. **Catch audit logging errors gracefully** - they shouldn't break your operation
4. **Include meaningful changes** - make the audit trail human-readable
5. **Serialize complex objects** - use `AuditHelper.SerializeToJson()` for detailed tracking
6. **Filter audit logs efficiently** - use appropriate date ranges in queries

---

### Error Handling

The audit service is designed to fail gracefully. If audit logging fails, it won't break your main operation:

```csharp
// Audit failure logged to debug console
// Main operation continues normally
await _auditService.LogAuditAsync(...); 
```

---

### Next Steps

1. Create a database migration
2. Add audit logging to all write operation services (ShipmentService, DocumentService, etc.)
3. Update controllers to extract and pass user information
4. Test the audit log endpoint with various filters
5. Consider adding an admin UI to view and export audit logs
