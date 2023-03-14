require 'net/http'
require 'httparty'

# The URL for the AssemblyAI API endpoint for creating a new transcription job
url = "https://api.assemblyai.com/v2/transcript"

headers = {
    # The authorization header with your AssemblyAI API token
    "authorization" => "{your_api_token}",

    # The content-type header for the request body
    "content-type" => "application/json"
}

data = {
    # The URL of the audio file to be transcribed
    "audio_url" => "https://bit.ly/3yxKEIY"
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
        break
    elsif transcription_result["status"] == "error"
        # If an error occurred during the transcription job, raise an exception with the error message
        raise "Transcription failed: #{transcription_result["error"]}"
    else
        # If the transcription job is still in progress, wait for 3 seconds before checking again
        sleep(3)
    end
end