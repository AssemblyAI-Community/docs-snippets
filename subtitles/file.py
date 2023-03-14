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
        "audio_url": audio_url
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

def export_subtitles(api_token, transcript_id, subtitle_format):
    """
    Export subtitles using AssemblyAI API.

    Args:
        api_token (str): Your API token for AssemblyAI.
        transcript_id (str): The ID of the transcript you want to export as subtitles.
        subtitle_format (str): The subtitle format to export (either 'srt' or 'vtt').

    Returns:
        str: The filename of the saved subtitle file.
    """
    # Set the API endpoint for exporting subtitles
    url = f"https://api.assemblyai.com/v2/transcript/{transcript_id}/{subtitle_format}"

    # Set the headers for the request, including the API token
    headers = {
        "authorization": api_token
    }

    # Send a GET request to the API to get the subtitle data
    response = requests.get(url, headers=headers)

    # Check if the response status code is successful (200)
    if response.status_code == 200:
        # Save the subtitle data to a local file
        filename = f"{transcript_id}.{subtitle_format}"
        with open(filename, "wb") as subtitle_file:
            subtitle_file.write(response.content)
        return filename
    else:
        raise RuntimeError(f"Subtitle export failed: {response.text}")

# Call the create_transcript function with your API token and the audio URL
your_api_token = "{your_api_token}"
audio_url = "https://bit.ly/3yxKEIY"
transcript = create_transcript(your_api_token, audio_url)

# Call the export_subtitles function with your API token, the transcript ID, and the desired subtitle format
subtitle_format = "srt"  # or "vtt" if you want VTT format
subtitle_filename = export_subtitles(your_api_token, transcript['id'], subtitle_format)

# Print the subtitle filename
print(f"Subtitles saved to: {subtitle_filename}")
