import requests
import json
import sys

def post_lemur(api_token, transcript_id):
    # Set the API endpoint for creating a new LeMUR response
    url = "https://api.staging.assemblyai-labs.com/beta/generate/summary"

    # Set the headers for the request, including the API token and content type
    headers = {
        "authorization": api_token,
        "content-type": "application/json"
    }

    # Set the data for the request, including the ID of the transcript to be analyzed
    data = {
        "transcript_ids": [
            transcript_id
        ],
        "context": "this is a sales call",
        "answer_format": "short summary"
    }

    # Send a POST request to the API to create a new transcript, passing in the headers and data
    response = requests.post(url, json=data, headers=headers)

    # Get the LeMUR response from the JSON data
    lemur_response = response.json()['response']

    return lemur_response

your_api_token = "{your_api_token}"
transcript_id = sys.argv[1]

# Get output from LeMUR
lemur_output = post_lemur(your_api_token, transcript_id)

# Print the summary
print(lemur_output)
