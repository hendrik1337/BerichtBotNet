using BerichtBotNet.Data;
using Newtonsoft.Json;

namespace BerichtBotNet.Berichtsheft;

public class BerichtsheftApiConnector
{
    private static HttpClient _sharedClient = new()
    {
        BaseAddress = new Uri("http://localhost:8001"),
    };
    
    public static async Task<List<Lesson>?> GetAsync()
    {
        using HttpResponseMessage response = await _sharedClient.GetAsync("");

        response.EnsureSuccessStatusCode();
    
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<List<Lesson>>(jsonResponse);

        return data;
    }
    
}