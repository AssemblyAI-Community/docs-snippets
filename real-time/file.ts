import WebSocket from 'ws';

const socket = new WebSocket('wss://api.assemblyai.com/v2/realtime/ws');

socket.on('open', () => {
  const authHeader = {
    Authorization: '{your_api_token}',
  };

  const sampleRate = 16000;
  const wordBoost = ['HackerNews', 'Twitter'];

  socket.send(
    `wss://api.assemblyai.com/v2/realtime/ws?sample_rate=${sampleRate}&word_boost=${JSON.stringify(
      wordBoost
    )}`,
    { headers: authHeader }
  );
});

socket.on('message', (data: string) => {
  const message = JSON.parse(data);
  if (message.message_type === 'SessionBegins') {
    const sessionId = message.session_id;
    const expiresAt = message.expires_at;
    console.log(`Session ID: ${sessionId}`);
    console.log(`Expires at: ${expiresAt}`);
  } else if (message.message_type === 'PartialTranscript') {
    console.log(`Partial transcript received: ${message.text}`);
  } else if (message.message_type === 'FinalTranscript') {
    console.log(`Final transcript received: ${message.text}`);
  }
});

socket.on('error', (error) => {
  const errorObject = JSON.parse(error);
  if (errorObject.error_code === 4002) {
    console.log('Insufficient Funds');
  }
});

socket.on('close', () => {
  console.log('WebSocket closed');
});

function sendAudio(ws: WebSocket, audioData: ArrayBuffer) {
  const payload = {
    audio_data: Buffer.from(audioData).toString('base64'),
  };
  ws.send(JSON.stringify(payload));
}

function terminateSession(socket: WebSocket) {
  const payload = { terminate_session: true };
  socket.send(JSON.stringify(payload));
  socket.close();
}
