using System;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace RealtimeTranscription
{
    class Program
    {
        static void Main(string[] args)
        {
            var authHeader = new string[] { "Authorization: {your_api_token}" };
            var sampleRate = 16000;
            var wordBoost = new string[] { "HackerNews", "Twitter" };
            var url = $"wss://api.assemblyai.com/v2/realtime/ws?word_boost={JsonConvert.SerializeObject(wordBoost)}&sample_rate={sampleRate}";
            var socket = new WebSocket(url, authHeader);

            socket.OnMessage += (sender, e) =>
            {
                var message = JObject.Parse(e.Data);
                if (message["message_type"].ToString() == "SessionBegins")
                {
                    var session_id = message["session_id"].ToString();
                    var expires_at = message["expires_at"].ToString();
                    Console.WriteLine($"Session ID: {session_id}, Expires At: {expires_at}");
                }
                else if (message["message_type"].ToString() == "PartialTranscript")
                {
                    var session_id = message["session_id"].ToString();
                    var audio_start = message["audio_start"].ToObject<int>();
                    var audio_end = message["audio_end"].ToObject<int>();
                    var confidence = message["confidence"].ToObject<float>();
                    var text = message["text"].ToString();
                    Console.WriteLine($"Session ID: {session_id}, Audio Start: {audio_start}, Audio End: {audio_end}, Confidence: {confidence}, Text: {text}");
                }
                else if (message["message_type"].ToString() == "Transcript")
                {
                    var session_id = message["session_id"].ToString();
                    var audio_start = message["audio_start"].ToObject<int>();
                    var audio_end = message["audio_end"].ToObject<int>();
                    var confidence = message["confidence"].ToObject<float>();
                    var text = message["text"].ToString();
                    Console.WriteLine($"Session ID: {session_id}, Audio Start: {audio_start}, Audio End: {audio_end}, Confidence: {confidence}, Text: {text}");
                }
            };

            socket.OnError += (sender, e) =>
            {
                var errorObject = JObject.Parse(e.Message);
                if (errorObject["error_code"].ToString() == "4031")
                {
                    Console.WriteLine("Session idle for too long");
                }
            };

            socket.OnClose += (sender, e) =>
            {
                Console.WriteLine("WebSocket closed");
            };

            socket.Connect();
        }

        static void SendAudio(WebSocket socket, byte[] audioData)
        {
            var payload = new
            {
                audio_data = Convert.ToBase64String(audioData),
            };
            socket.Send(JsonConvert.SerializeObject(payload));
        }

        static void TerminateSession(WebSocket socket)
        {
            var payload = new { terminate_session = true };
            var message = JsonConvert.SerializeObject(payload);
            socket.Send(message);
            socket.Close();
        }
    }
}
