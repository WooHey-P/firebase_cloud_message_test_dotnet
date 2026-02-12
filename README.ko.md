# FCM Sender (.NET 8 Web API)

Firebase Cloud Messaging(FCM) 푸시 메시지 발송을 위한 .NET 8 기반 API 서버입니다.

[English](README.md)

## 빠른 시작

### 1. 저장소 클론

```bash
git clone <repository-url>
cd firebase_cloud_message_test_for_dotnet
```

### 2. 초기화 스크립트 실행

```bash
./init.sh
```

스크립트가 다음을 안내합니다:

- 서비스 계정 JSON 파일명 입력
- `.env` 파일 자동 생성

### 3. 실행

```bash
dotnet restore
dotnet run --project src/FcmSender.Api
```

### 4. 테스트

브라우저에서 `http://localhost:5130/swagger` 접속

## 사전 준비

### Firebase 서비스 계정 키 발급

1. [Firebase Console](https://console.firebase.google.com/) 접속
2. 프로젝트 설정 > **서비스 계정** 탭
3. **새 비공개 키 생성** 클릭
4. 다운로드된 JSON 파일을 `secrets/` 폴더에 저장

## 환경 변수

| 변수 | 필수 | 설명 |
|------|------|------|
| `GOOGLE_APPLICATION_CREDENTIALS` | O | 서비스 계정 JSON 경로 (project_id는 자동 추출됨) |
| `FIREBASE_DEFAULTDEVICETOKEN` | X | 기본 FCM 토큰 |

> **참고**: `project_id`는 서비스 계정 JSON 파일에서 자동으로 추출되므로 별도로 설정할 필요가 없습니다.

## API 사용법

### 알림 발송

```
POST /api/notifications/send
Content-Type: application/json
```

### 요청 예시

```bash
curl -X POST http://localhost:5130/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "title": "알림 제목",
    "body": "알림 내용",
    "token": "디바이스_FCM_토큰"
  }'
```

### 요청 필드

| 필드 | 타입 | 설명 |
|------|------|------|
| `title` | string | 알림 제목 |
| `body` | string | 알림 내용 |
| `token` | string | 디바이스 FCM 토큰 |
| `topic` | string | 토픽명 (token 대신 사용) |
| `data` | object | 커스텀 key-value 데이터 |
| `validateOnly` | bool | true면 실제 발송 없이 검증만 수행 |

### 응답 예시

```json
{
  "messageName": "projects/my-project/messages/123456",
  "dryRun": false
}
```

## 파일 구조

```
.
├── init.sh                   # 초기화 스크립트
├── .env.example              # 환경 변수 템플릿
├── .env                      # 환경 변수 (Git 무시)
├── secrets/                  # 서비스 계정 JSON (Git 무시)
├── src/
│   ├── FcmSender.Api/        # Minimal API + Swagger
│   └── FcmSender.Core/       # FCM 메시지 전송 로직
└── tests/
    └── FcmSender.Tests/      # 단위 테스트
```

## 참고

- [Firebase Cloud Messaging HTTP v1](https://firebase.google.com/docs/cloud-messaging/send-message)
- [.NET Minimal API](https://learn.microsoft.com/aspnet/core/tutorials/min-web-api)
