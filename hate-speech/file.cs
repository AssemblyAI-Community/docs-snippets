using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FileUploader
{
    public async Task<string> UploadFileAsync(string apiToken, string path)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(apiToken);

        using var fileContent = new ByteArrayContent(File.ReadAllBytes(path));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync("https://api.assemblyai.com/v2/upload", fileContent);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error: {e.Message}");
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);
            return json["upload_url"].ToString();
        }
        else
        {
            Console.Error.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }
    }
}

public class TranscriptFetcher
{
    // Function to fetch transcript asynchronously
    public async Task<dynamic> GetTranscriptAsync(string apiToken, string audioUrl)
    {
        // The URL of the AssemblyAI API endpoint for transcription
        string url = "https://api.assemblyai.com/v2/transcript";

        // Create a new dictionary with the audio URL
        var data = new Dictionary<string, dynamic>()
        {
            { "audio_url", audioUrl },
            { "content_safety", true }
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

            // Get the ID of the transcription from the response JSON
            string transcriptId = responseJson.id;

            // Create the polling endpoint URL with the transcription ID
            string pollingEndpoint = $"https://api.assemblyai.com/v2/transcript/{transcriptId}";

            // Poll the API endpoint until the transcription is completed or an error occurs
            while (true)
            {
                // Send a GET request to the polling endpoint URL
                var pollingResponse = await client.GetAsync(pollingEndpoint);

                // Read the polling response content as a string
                var pollingResponseContent = await pollingResponse.Content.ReadAsStringAsync();

                // Deserialize the polling response content into a dynamic object
                var pollingResponseJson = JsonConvert.DeserializeObject<dynamic>(pollingResponseContent);

                // Check if the transcription is completed
                if (pollingResponseJson.status == "completed")
                {
                    // Return the entire transcript object
                    return pollingResponseJson;
                }
                else if (pollingResponseJson.status == "error")
                {
                    // Check if an error occurred during transcription, then throw an exception with the error message
                    throw new Exception($"Transcription failed: {pollingResponseJson.error}");
                }
                else
                {
                    // Sleep for 3 seconds before polling the API endpoint again
                    Thread.Sleep(3000);
                }
            }
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        string apiToken = "{your_api_token}";

        var path = "/path/to/foo.wav";
        var fileUploader = new FileUploader();
        var uploadUrl = await fileUploader.UploadFileAsync(apiToken, path);

        // Create an instance of the TranscriptFetcher class
        TranscriptFetcher transcriptFetcher = new TranscriptFetcher();

        try
        {
            // Fetch the transcript object using the GetTranscriptAsync function
            dynamic transcript = await transcriptFetcher.GetTranscriptAsync(apiToken, uploadUrl);

            // Check for hate speech in the content safety labels
            var contentSafetyLabels = transcript.content_safety_labels;
            if (contentSafetyLabels != null)
            {
                var results = safetyLabels.results;
                if (results != null)
                {
                    foreach (var result in results)
                    {
                        var labels = result.labels;
                        if (labels != null)
                        {
                            foreach (var label in labels)
                            {
                                if (label.label == "hate_speech")
                                {
                                    Console.WriteLine($"Hate speech detected: {label.confidence}");
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Print the error message if an exception is caught
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}