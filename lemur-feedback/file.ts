import axios from 'axios';

const API_TOKEN = 'your_api_token';
const TRANSCRIPT_ID = 'transcript_id';

// Function that sends a request to the Lemur API
async function lemurPost(api_token: string, transcriptId: string) {
  const headers = {
    authorization: api_token,
    'content-type': 'application/json',
  };

  // Send a POST request to the LeMUR API with the transcript ID in the request body
  const response = await axios.post('https://api.staging.assemblyai-labs.com/beta/generate/summary', {
    "transcript_ids": [
      transcriptId
    ],
    "context": "this is a sales call",
    "answer_format": "short summary"
  }, { headers });

  // Retrieve the LeMUR API response
  return response.data["response"];
}

async function main() {
  const lemurResponse = await lemurPost(API_TOKEN, TRANSCRIPT_ID);
  console.log(lemurResponse);
}

main();
