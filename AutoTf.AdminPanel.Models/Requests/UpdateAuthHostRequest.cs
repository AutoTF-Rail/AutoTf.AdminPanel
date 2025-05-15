namespace AutoTf.AdminPanel.Models.Requests;

public class UpdateAuthHostRequest
{
    public required string CurrentHost { get; set; }
    public required string NewHost { get; set; }
}