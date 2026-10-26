namespace OmniManage.Modules.Manufacturing.Presentation.Controllers;

public sealed record CreateWorkOrderRequest(
    string? OrderNumber,
    Guid BillOfMaterialId,
    decimal QuantityPlanned,
    DateOnly? DueDate,
    string? Notes);
