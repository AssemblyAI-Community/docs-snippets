require "net/http"
require "httparty"

# Function to upload a local file to the AssemblyAI API
def upload_file(api_token, path)
    uri = URI("https://api.assemblyai.com/v2/upload")
    request = Net::HTTP::Post.new(uri)
    request["authorization"] = api_token
    request.body = File.read(path)
  
    response = Net::HTTP.start(uri.hostname, uri.port, use_ssl: true) do |http|
      http.request(request)
    end
  
    if response.code == "200"
      JSON.parse(response.body)["upload_url"]
    else
      puts "Error: #{response.code} - #{response.body}"
      nil
    end
  end
  
# Function to create a new transcription job and return the transcript object when it"s complete
def create_transcript(api_token, audio_url)
  # The URL for the AssemblyAI API endpoint for creating a new transcription job
  url = "https://api.assemblyai.com/v2/transcript"

  headers = {
      # The authorization header with your AssemblyAI API token
      "authorization" => api_token,

      # The content-type header for the request body
      "content-type" => "application/json"
  }

  data = {
      # The URL of the audio file to be transcribed
      "audio_url" => audio_url
  }

  # Parse the API endpoint URL into a URI object
  uri = URI.parse(url)

  # Create a new Net::HTTP object for making the API request
  http = Net::HTTP.new(uri.host, uri.port)

  # Use SSL for secure communication
  http.use_ssl = true

  # Create a new HTTP POST request with the API endpoint URL and headers
  request = Net::HTTP::Post.new(uri.request_uri, headers)

  # Set the request body to the data hash, converted to JSON format
  request.body = data.to_json

  # Send the API request and store the response object
  response = http.request(request)

  # Get the ID of the new transcription job from the response
  transcript_id = response.parsed_response["id"]

  # Construct the polling endpoint URL for checking the status of the transcription job
  polling_endpoint = "https://api.assemblyai.com/v2/transcript/#{transcript_id}"

  # Start an infinite loop that will continue until the transcription job is completed or an error occurs
  while true
      # Send a GET request to the polling endpoint and store the response
      polling_response = HTTParty.get(polling_endpoint, headers: headers)

      # Parse the response JSON and store it in a hash
      transcription_result = polling_response.parsed_response

      # If the transcription job has completed successfully, break out of the loop
      if transcription_result["status"] == "completed"
          return transcription_result
      elsif transcription_result["status"] == "error"
          # If an error occurred during the transcription job, raise an exception with the error message
          raise "Transcription failed: #{transcription_result["error"]}"
      else
          # If the transcription job is still in progress, wait for 3 seconds before checking again
          sleep(3)
      end
  end
end

# Replace {your_api_token} with your actual API token
api_token = "{your_api_token}"

path = "/path/to/foo.wav"
upload_url = upload_file(api_token, path)

transcript = create_transcript(api_token, upload_url)
puts transcript
