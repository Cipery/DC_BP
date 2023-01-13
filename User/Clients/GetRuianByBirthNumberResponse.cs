using System.Text.Json.Serialization;

namespace User.Clients;

public class GetRuianByBirthNumberResponse
{
    [JsonPropertyName("ruian")]
    public int? Ruian { get; set; }
}