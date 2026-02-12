# FCM Sender (.NET 8 Web API)

A .NET 8 based API server for sending Firebase Cloud Messaging (FCM) push notifications.

[한국어](README.ko.md)

## Quick Start

### 1. Clone the repository

```bash
git clone <repository-url>
cd firebase_cloud_message_test_for_dotnet
```

### 2. Run the initialization script

```bash
./init.sh
```

The script will guide you through:

- Specifying the service account JSON filename
- Automatically generating the `.env` file

### 3. Run the server

```bash
dotnet restore
dotnet run --project src/FcmSender.Api
```

### 4. Test

Open `http://localhost:5130/swagger` in your browser

## Prerequisites

### Get Firebase Service Account Key

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Project Settings > **Service accounts** tab
3. Click **Generate new private key**
4. Save the downloaded JSON file to `secrets/` folder

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `GOOGLE_APPLICATION_CREDENTIALS` | Yes | Path to service account JSON (project_id is auto-extracted) |
| `FIREBASE_DEFAULTDEVICETOKEN` | No | Default FCM device token |

> **Note**: The `project_id` is automatically extracted from the service account JSON file, so you don't need to set it separately.

## API Usage

### Send Notification

```
POST /api/notifications/send
Content-Type: application/json
```

### Request Example

```bash
curl -X POST http://localhost:5130/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Notification Title",
    "body": "Notification Body",
    "token": "DEVICE_FCM_TOKEN"
  }'
```

### Request Fields

| Field | Type | Description |
|-------|------|-------------|
| `title` | string | Notification title |
| `body` | string | Notification body |
| `token` | string | Device FCM token |
| `topic` | string | Topic name (use instead of token) |
| `data` | object | Custom key-value data |
| `validateOnly` | bool | If true, validates without sending |

### Response Example

```json
{
  "messageName": "projects/my-project/messages/123456",
  "dryRun": false
}
```

## Project Structure

```
.
├── init.sh                   # Initialization script
├── .env.example              # Environment variable template
├── .env                      # Environment variables (git ignored)
├── secrets/                  # Service account JSON (git ignored)
├── src/
│   ├── FcmSender.Api/        # Minimal API + Swagger
│   └── FcmSender.Core/       # FCM message sending logic
└── tests/
    └── FcmSender.Tests/      # Unit tests
```

## References

- [Firebase Cloud Messaging HTTP v1](https://firebase.google.com/docs/cloud-messaging/send-message)
- [.NET Minimal API](https://learn.microsoft.com/aspnet/core/tutorials/min-web-api)
