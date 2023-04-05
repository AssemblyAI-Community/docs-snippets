import websocket
import json
import base64

def on_message(ws, message):
    message = json.loads(message)
    if message["message_type"] == "SessionBegins":
        session_id = message["session_id"]
        expires_at = message["expires_at"]
        print(f"Session ID: {session_id}")
        print(f"Expires at: {expires_at}")
    elif message["message_type"] == "PartialTranscript":
        print(f"Partial transcript received: {message['text']}")
    elif message['message_type'] == 'FinalTranscript':
        print(f"Final transcript received: {message['text']}")

def on_error(ws, error):
    error_message = json.loads(error)
    if error_message['error_code'] == 4000:
        print("Sample rate must be a positive integer")

def on_close(ws):
    print("WebSocket closed")

def on_open(ws):
    print("WebSocket opened")
    auth_header = {"Authorization": "{your_api_token}"}
    sample_rate = 16000
    word_boost = ["HackerNews", "Twitter"]
    ws.send(f"wss://api.assemblyai.com/v2/realtime/ws?sample_rate={sample_rate}&word_boost={json.dumps(word_boost)}", header=auth_header)

def send_audio(ws, audio_data):
    payload = {
        "audio_data": base64.b64encode(audio_data).decode("utf-8")
    }
    ws.send(json.dumps(payload))

def terminate_session(socket):
    payload = {'terminate_session': True}
    message = json.dumps(payload)
    socket.send(message)
    socket.close()

websocket.enableTrace(True)
ws = websocket.WebSocketApp("wss://api.assemblyai.com/v2/realtime/ws", on_message=on_message, on_error=on_error, on_close=on_close)
ws.on_open = on_open
ws.run_forever()
