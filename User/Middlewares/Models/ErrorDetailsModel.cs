using System.Text.Json;

namespace User.Middlewares.Models;

public record ErrorDetailsModel
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}