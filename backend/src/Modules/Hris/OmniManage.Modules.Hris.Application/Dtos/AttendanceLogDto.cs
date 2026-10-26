namespace OmniManage.Modules.Hris.Application.Dtos;

public class AttendanceLogDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}