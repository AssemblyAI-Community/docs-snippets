import requests
import json
import time

def create_transcript(api_token, audio_url):
    """
    Create a transcript using AssemblyAI API.

    Args:
        api_token (str): Your API token for AssemblyAI.
        audio_url (str): URL of the audio file to be transcribed.

    Returns:
        dict: Completed transcript object.
    """
    # Set the API endpoint for creating a new transcript
    url = "https://api.assemblyai.com/v2/transcript"

    # Set the headers for the request, including the API token and content type
    headers = {
        "authorization": api_token,
        "content-type": "application/json"
    }

    # Set the data for the request, including the URL of the audio file to be transcribed
    data = {
        "audio_url": audio_url,
        "speaker_labels": True
    }

    # Send a POST request to the API to create a new transcript, passing in the headers and data
    response = requests.post(url, json=data, headers=headers)

    # Get the transcript ID from the response JSON data
    transcript_id = response.json()['id']

    # Set the polling endpoint URL by appending the transcript ID to the API endpoint
    polling_endpoint = f"https://api.assemblyai.com/v2/transcript/{transcript_id}"

    # Keep polling the API until the transcription is complete
    while True:
        # Send a GET request to the polling endpoint, passing in the headers
        transcription_result = requests.get(polling_endpoint, headers=headers).json()

        # If the status of the transcription is 'completed', exit the loop
        if transcription_result['status'] == 'completed':
            break

        # If the status of the transcription is 'error', raise a runtime error with the error message
        elif transcription_result['status'] == 'error':
            raise RuntimeError(f"Transcription failed: {transcription_result['error']}")

        # If the status of the transcription is not 'completed' or 'error', wait for 3 seconds and poll again
        else:
            time.sleep(3)

    return transcription_result

# Call the create_transcript function with your API token and the audio URL
your_api_token = "{your_api_token}"
audio_url = "https://bit.ly/3yxKEIY"
transcript = create_transcript(your_api_token, audio_url)

# Print the transcript text
print("Transcript:\n", transcript['text'])

# Iterate through the utterances and print speaker information
print("\nSpeakers' Information:")
for utterance in transcript['utterances']:
    speaker = utterance['speaker']
    text = utterance['text']
    confidence = utterance['confidence']

    print(f"Speaker {speaker}: {text}\nConfidence: {confidence}\n")
