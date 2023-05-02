using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class LemurFetcher
{
    // Function to make a POST request to LeMUR
    public async Task<dynamic> PostLemur(string apiToken, string transcriptId)
    {
        // The URL of the AssemblyAI API endpoint
        string url = "https://api.staging.assemblyai-labs.com/beta/generate/summary";

        // Create a new dictionary with the audio URL
        var data = new Dictionary<string, dynamic>()
        {
            { "transcript_ids", new List<string>{transcriptId} },
            { "context", "this is a sales call" },
            { "answer_format", "short summary"}
        };

        // Create a new HttpClient to make the HTTP requests
        using (var client = new HttpClient())
        {
            // Set the "authorization" header with your API token
            client.DefaultRequestHeaders.Add("authorization", apiToken);

            // Create a new JSON payload with the audio URL
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            // Send a POST request with the JSON payload to the API endpoint
            HttpResponseMessage response = await client.PostAsync(url, content);

            // Read the response content as a string
            var responseContent = await response.Content.ReadAsStringAsync();

            // Deserialize the response content into a dynamic object
            var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);

            // Get the LeMUR response from the output JSON
            string lemurResponse = responseJson.response;
            return lemurResponse;
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        string apiToken = "{your_api_token}";
        string transcriptId = "{transcript_id}";

        LemurFetcher lemurFetcher = new LemurFetcher();

        try
        {
            // Fetch the LeMUR output using the PostLemur function
            string lemurOutput = lemurFetcher.PostLemur(apiToken, transcriptId);

            // Print the LeMUR output to the console
            Console.WriteLine(lemurOutput);
        }
        catch (Exception ex)
        {
            // Print the error message if an exception is caught
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
