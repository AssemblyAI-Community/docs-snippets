using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TranscriptFetcher
{
    // Function to fetch transcript asynchronously
    public async Task<dynamic> GetTranscriptAsync(string audioUrl, string apiToken)
    {
        // The URL of the AssemblyAI API endpoint for transcription
        string url = "https://api.assemblyai.com/v2/transcript";

        // Create a new dictionary with the audio URL
        var data = new Dictionary<string, string>()
        {
            { "audio_url", audioUrl },
            { "auto_chapters", true }
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
        // Define the audio URL and API token
        string audioUrl = "https://bit.ly/3yxKEIY";
        string apiToken = "{your_api_token}";

        // Create an instance of the TranscriptFetcher class
        TranscriptFetcher transcriptFetcher = new TranscriptFetcher();

        try
        {
            // Fetch the transcript object using the GetTranscriptAsync function
            dynamic transcript = await transcriptFetcher.GetTranscriptAsync(audioUrl, apiToken);

            // Print the chapters
            Console.WriteLine("\nChapters:");
            int chapterIndex = 1;
            foreach (var chapter in transcript.chapters)
            {
                Console.WriteLine($"\nChapter {chapterIndex}:");
                Console.WriteLine($"  Summary: {chapter.summary}");
                Console.WriteLine($"  Headline: {chapter.headline}");
                Console.WriteLine($"  Gist: {chapter.gist}");
                Console.WriteLine($"  Start: {chapter.start}");
                Console.WriteLine($"  End: {chapter.end}");
                chapterIndex++;
            }
        }
        catch (Exception ex)
        {
            // Print the error message if an exception is caught
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
