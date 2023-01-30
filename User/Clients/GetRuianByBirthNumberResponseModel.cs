using System.Text.Json.Serialization;

namespace User.Clients;

public class GetRuianByBirthNumberResponseModel
{
    [JsonPropertyName("ruian")]
    public int? Ruian { get; set; }
}