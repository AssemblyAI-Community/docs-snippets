require "net/http"
require "json"
  
# Function that sends a request to the LeMUR API
def post_lemur(api_token, transcript_id)
  # The URL for the AssemblyAI API
  url = "https://api.staging.assemblyai-labs.com/beta/generate/question-answer"

  headers = {
      # The authorization header with your AssemblyAI API token
      "authorization" => api_token,

      # The content-type header for the request body
      "content-type" => "application/json"
  }

  data = {
      "transcript_ids" => [transcript_id],
      "questions" => [
            {
                "question" => "Is this caller a qualified buyer?",
                "answer_options" => [
                    "Yes",
                    "No"
                ]
            },
            {
                "question" => "Classify the call into one of the following scenarios",
                "answer_options" => [
                    "Follow-up",
                    "2nd Call"
                ],
                "context" => "Anytime it is clear that the caller is calling back a second time about the same topic"
            },
            {
                "question" => "What is the caller's mood?",
                "answer_format" => "Short sentence"
            }
        ]
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

  # Get the LeMUR response from the JSON
  parsed_json = JSON.parse(response.body)
  lemur_response = parsed_json["response"]
  return lemur_response
end

# Replace {your_api_token} with your actual API token
api_token = "{your_api_token}"
transcript_id = "{transcript_id}"

lemur_output = post_lemur(api_token, transcript_id)
puts lemur_output
