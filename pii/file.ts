import fs from 'fs';
import axios from 'axios';

const API_TOKEN = 'your_api_token';

// Function to upload a local file to the AssemblyAI API
async function upload_file(api_token: string, path: string): Promise<string | null> {
  const data = fs.readFileSync(path);
  const url = 'https://api.assemblyai.com/v2/upload';

  try {
    const response = await axios.post(url, data, {
      headers: {
        'Content-Type': 'application/octet-stream',
        'Authorization': api_token,
      },
    });

    if (response.status === 200) {
      return response.data['upload_url'];
    } else {
      console.error(`Error: ${response.status} - ${response.statusText}`);
      return null;
    }
  } catch (error) {
    console.error(`Error: ${error}`);
    return null;
  }
}

// Async function that sends a request to the AssemblyAI transcription API and retrieves the transcript
async function transcribeAudio(api_token: string, audio_url: string) {
  const headers = {
    authorization: api_token,
    'content-type': 'application/json',
  };

  // Send a POST request to the transcription API with the audio URL in the request body
  const response = await axios.post('https://api.assemblyai.com/v2/transcript', {
    audio_url,
    redact_pii: true,
    redact_pii_policies: ['us_social_security_number', 'credit_card_number']  
  }, { headers });

  // Retrieve the ID of the transcript from the response data
  const transcriptId = response.data.id;

  // Construct the polling endpoint URL using the transcript ID
  const pollingEndpoint = `https://api.assemblyai.com/v2/transcript/${transcriptId}`;

  // Poll the transcription API until the transcript is ready
  while (true) {
    // Send a GET request to the polling endpoint to retrieve the status of the transcript
    const pollingResponse = await axios.get(pollingEndpoint, { headers });

    // Retrieve the transcription result from the response data
    const transcriptionResult = pollingResponse.data;

    // If the transcription is complete, return the transcript object
    if (transcriptionResult.status === 'completed') {
      return transcriptionResult;
    }
    // If the transcription has failed, throw an error with the error message
    else if (transcriptionResult.status === 'error') {
      throw new Error(`Transcription failed: ${transcriptionResult.error}`);
    }
    // If the transcription is still in progress, wait for a few seconds before polling again
    else {
      await new Promise((resolve) => setTimeout(resolve, 3000));
    }
  }
}

async function main() {
  // Call the transcribeAudio function to start the transcription process
  const path = '/path/to/foo.wav';
  const uploadUrl = await upload_file(API_TOKEN, path);

  if (!uploadUrl) {
    console.error(new Error('Upload failed'));
    return
  }

  const transcript = await transcribeAudio(API_TOKEN, uploadUrl);
  console.log('Transcript:', transcript);
}

main();