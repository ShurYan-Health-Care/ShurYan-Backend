# Frontend Integration Prompt — ShurYan Telemedicine Video Call

> Copy everything below this line and send it directly to your frontend AI agent.

---

You are building the **Telemedicine Video Call** feature for a medical platform called **ShurYan**. The backend is fully implemented and tested. Your job is to integrate it into the frontend with a polished, production-quality UI/UX. Below is everything you need — APIs, real-time events, Agora SDK usage, states, and rules. Follow it exactly.

---

## Tech Stack Required

Install these packages:

```bash
npm install agora-rtc-sdk-ng @microsoft/signalr
```

---

## Architecture Overview

The video call flow works like this:

1. User navigates to an online appointment page.
2. User clicks **"Join Video Call"** → frontend calls `POST /api/video-sessions/{appointmentId}/join`.
3. Backend returns an **Agora App ID**, **channel name**, and a **signed RTC token**.
4. Frontend uses the Agora Web SDK to join the channel with those credentials.
5. **SignalR** is used for real-time events (session ready, session ended).
6. When either side ends the call → frontend calls `POST /api/video-sessions/{appointmentId}/end`.

---

## Authentication

All API calls require a JWT Bearer token:

```
Authorization: Bearer {accessToken}
```

All SignalR connections require the JWT as a query parameter:

```
wss://{host}/hubs/video-notify?access_token={accessToken}
```

---

## API Endpoints

### Standard Response Wrapper

Every response from the backend follows this shape:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "string",
  "data": { ... }
}
```

---

### GET `/api/video-sessions/{appointmentId}`

**Purpose:** Fetch current session state + a fresh Agora token. Use on page load to restore state.  
**Auth roles:** Doctor, Patient  
**No request body.**

**Response `data`:**

```json
{
  "id": "guid",
  "appointmentId": "guid",
  "doctorId": "guid",
  "patientId": "guid",
  "agoraChannelName": "string",
  "agoraAppId": "string",
  "agoraToken": "string",
  "status": 1,
  "doctorJoinedAt": "ISO datetime | null",
  "patientJoinedAt": "ISO datetime | null",
  "startedAt": "ISO datetime | null",
  "endedAt": "ISO datetime | null",
  "durationSeconds": "number | null",
  "endReason": "number | null"
}
```

---

### POST `/api/video-sessions/{appointmentId}/join`

**Purpose:** Join the session. This is the main action. Call it when the user clicks "Join Video Call".  
**Auth roles:** Doctor, Patient  
**No request body.**

**Response `data`:** Same shape as GET above.

**What the backend does automatically:**
- Records `doctorJoinedAt` or `patientJoinedAt` timestamp.
- When **both** have joined, sets `status = 2 (Active)` and fires a `SessionReady` SignalR event **to the patient**.

**Error codes:**

| Status | Meaning |
|--------|---------|
| `400` | Appointment is not in a joinable state, or session already ended |
| `403` | User is not a participant in this session |
| `404` | Appointment or session not found |

---

### POST `/api/video-sessions/{appointmentId}/end?reason={int}`

**Purpose:** End the call. Call it when user clicks "End Call".  
**Auth roles:** Doctor, Patient  
**No request body.**

**`reason` query parameter (required):**

| Value | Enum Name | When to use |
|-------|-----------|-------------|
| `1` | Completed | Normal end, doctor clicked End Call |
| `2` | DoctorLeft | Doctor left early |
| `3` | PatientLeft | Patient left early |
| `4` | Timeout | Session timed out |

**What the backend does automatically:**
- Records `endedAt`, `durationSeconds`, `endReason`.
- Sets `status = 3 (Ended)`.
- Fires a `SessionEnded` SignalR event **to the other participant**.

---

## Session Status Enum

| Value | Name | Description |
|-------|------|-------------|
| `1` | Waiting | One side has joined, waiting for the other |
| `2` | Active | Both participants are in the session |
| `3` | Ended | Session was ended normally |
| `4` | Abandoned | Session was abandoned |

---

## SignalR Real-Time Events

**Hub URL:** `{API_BASE}/hubs/video-notify`

### Connect on app startup (as soon as user is authenticated):

```js
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_BASE}/hubs/video-notify?access_token=${accessToken}`)
  .withAutomaticReconnect()
  .build();

await connection.start();
```

---

### Event: `SessionReady`

**Sent to:** Patient only  
**Fired when:** Both doctor AND patient have called `/join` (session becomes Active)

```js
connection.on("SessionReady", (data) => {
  // data = { appointmentId: "guid", channelName: "string" }
  // At this point the session is Active.
  // If the patient hasn't joined Agora yet, do it now.
});
```

> **Note:** The doctor does NOT receive this event. The doctor's side is live the moment their own `/join` call succeeds.

---

### Event: `SessionEnded`

**Sent to:** The OTHER participant (not the one who called `/end`)  
**Fired when:** Either side calls `POST /end`

```js
connection.on("SessionEnded", (data) => {
  // data = { appointmentId: "guid" }
  // Leave Agora channel, stop local tracks, show ended screen.
});
```

---

## Agora RTC Integration

### Install / Import

```js
import AgoraRTC from "agora-rtc-sdk-ng";
```

### Full Join Flow

Call this after a successful `POST /join` response:

```js
async function joinAgoraChannel({ agoraAppId, agoraChannelName, agoraToken, currentUserGuid }) {
  const client = AgoraRTC.createClient({ mode: "rtc", codec: "vp8" });

  // Listen for remote user joining
  client.on("user-published", async (user, mediaType) => {
    await client.subscribe(user, mediaType);
    if (mediaType === "video") {
      user.videoTrack.play("remote-video-element-id");
    }
    if (mediaType === "audio") {
      user.audioTrack.play();
    }
  });

  client.on("user-unpublished", (user, mediaType) => {
    // Handle remote user turning off camera/mic
  });

  // UID MUST be the user's GUID with dashes removed (32-char hex string)
  const uid = currentUserGuid.replace(/-/g, "");

  await client.join(agoraAppId, agoraChannelName, agoraToken, uid);

  // Publish local camera + mic
  const [audioTrack, videoTrack] = await AgoraRTC.createMicrophoneAndCameraTracks();
  videoTrack.play("local-video-element-id");
  await client.publish([audioTrack, videoTrack]);

  return { client, audioTrack, videoTrack };
}
```

### Leave / End Flow

```js
async function leaveAgoraChannel({ client, audioTrack, videoTrack }) {
  audioTrack.stop();
  audioTrack.close();
  videoTrack.stop();
  videoTrack.close();
  await client.leave();
}
```

---

## Important Rules & Gotchas

- **UID format is critical.** The backend generates tokens using the user's GUID with dashes removed. You MUST pass this exact format: `userId.replace(/-/g, "")`. Example: `"bbb43b44a085402393e28f5555c3a5ae"`. Using any other UID will cause token validation to fail.
- **String UID warning.** Agora will log a warning about string UIDs — this is safe to ignore. The backend is built to use string UIDs.
- **Token expiry.** Tokens are valid for 3600 seconds (1 hour). For long sessions, re-call `GET /api/video-sessions/{appointmentId}` to get a fresh token and call `client.renewToken(newToken)`.
- **Connect SignalR early.** Connect to the hub as soon as the user logs in, not just when they click Join. This ensures you don't miss the `SessionReady` event.
- **`SessionReady` is for patient only.** The doctor does not receive it. Both sides call `/join` independently.
- **The side calling `/end` does NOT receive `SessionEnded`.** Handle cleanup for them in the `/end` API response handler, not via SignalR.
- **Only online appointments support video.** Guard the "Join" button to only show for appointments where `isOnline: true` and status is `Confirmed`, `CheckedIn`, or `InProgress`.

---

## UI States to Implement

### Pre-Join Screen
- Show the appointment details.
- Display a **"Join Video Call"** button (green, prominent).
- Show a brief reminder: "This is a video consultation. Make sure your camera and microphone are ready."

### Waiting Screen (After Doctor joins, before Patient joins)
- Show local camera preview (small, full-width card).
- Show animated indicator: **"Waiting for [Doctor's name / Patient's name] to join..."**
- Show "Cancel" option that calls `/end?reason=2` or `3`.
- `status === 1` (Waiting).

### Active Call Screen
- **Remote video**: large/fullscreen.
- **Local video**: picture-in-picture (bottom right corner, draggable optional).
- **Controls bar** (centered, bottom):
  - Toggle microphone (mute/unmute).
  - Toggle camera (on/off).
  - **End Call** button (red, prominent).
  - Optional: elapsed call timer (calculate from `startedAt`).
- `status === 2` (Active).

### Call Ended Screen
- Show: **"Call Ended"**
- Show duration: format `durationSeconds` as `mm:ss` or `Xm Xs`.
- Show end reason as human-readable text.
- CTA button: **"Back to Appointments"** / **"View Appointment Summary"**.
- `status === 3` (Ended).

### Error States
- Camera/mic permission denied → show instructions to enable permissions.
- Network error joining → show retry button.
- `400` from `/join` → show message from `response.message`.
- `403` → "You are not a participant in this session."

---

## Recommended Component Structure

```
VideoCallPage/
├── VideoCallPage.tsx         ← Main page, handles state machine
├── PreJoinScreen.tsx         ← Before joining
├── WaitingScreen.tsx         ← One side joined
├── ActiveCallScreen.tsx      ← Both in call
├── CallEndedScreen.tsx       ← After session ends
├── VideoPlayer.tsx           ← Reusable Agora track player
├── CallControls.tsx          ← Mute / Camera / End Call buttons
└── useVideoSession.ts        ← Custom hook: API calls, Agora, SignalR
```

---

## Example: Custom Hook Skeleton

```ts
// useVideoSession.ts
export function useVideoSession(appointmentId: string) {
  const [sessionState, setSessionState] = useState<"pre-join" | "waiting" | "active" | "ended">("pre-join");
  const [agoraClient, setAgoraClient] = useState(null);
  const [localTracks, setLocalTracks] = useState({ audio: null, video: null });
  const signalRRef = useRef(null);

  // 1. Connect SignalR on mount
  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl(`${API_BASE}/hubs/video-notify?access_token=${getToken()}`)
      .withAutomaticReconnect()
      .build();

    conn.on("SessionReady", handleSessionReady);
    conn.on("SessionEnded", handleSessionEnded);
    conn.start();
    signalRRef.current = conn;

    return () => conn.stop();
  }, []);

  // 2. Join session
  async function join() {
    const res = await api.post(`/video-sessions/${appointmentId}/join`);
    const { agoraAppId, agoraChannelName, agoraToken } = res.data.data;
    const { client, audioTrack, videoTrack } = await joinAgoraChannel({
      agoraAppId, agoraChannelName, agoraToken,
      currentUserGuid: getCurrentUserId(),
    });
    setAgoraClient(client);
    setLocalTracks({ audio: audioTrack, video: videoTrack });
    setSessionState("waiting");
  }

  // 3. Handle both joined
  function handleSessionReady() {
    setSessionState("active");
  }

  // 4. End session
  async function end(reason = 1) {
    await leaveAgoraChannel({ client: agoraClient, ...localTracks });
    await api.post(`/video-sessions/${appointmentId}/end?reason=${reason}`);
    setSessionState("ended");
  }

  // 5. Other side ended
  function handleSessionEnded() {
    leaveAgoraChannel({ client: agoraClient, ...localTracks });
    setSessionState("ended");
  }

  return { sessionState, join, end, localTracks };
}
```

---

## Summary Checklist

- [ ] SignalR connected on auth, listening for `SessionReady` and `SessionEnded`
- [ ] `POST /join` called on button click, Agora joined with credentials from response
- [ ] UID passed to Agora as GUID with dashes removed
- [ ] Local video previewed, remote video subscribed and played
- [ ] Mute / camera toggle controls working
- [ ] `POST /end` called with correct reason on End Call click
- [ ] `SessionEnded` SignalR event handled — leave Agora + show ended screen
- [ ] Token renewal for sessions > 1 hour
- [ ] Error handling for all API failure states
- [ ] Guard: only show Join button for `isOnline: true` appointments with valid status
