# FCM Sender (.NET 8 Web API)

Firebase Cloud Messaging(FCM)을 손쉽게 호출하기 위한 .NET 8 기반 서버 프로젝트입니다.  
서비스 계정 JSON을 안전하게 분리하고, Swagger UI를 통해 공용 API로 테스트/배포할 수 있도록 설계했습니다.

## 1. 주요 기능
1. **단일 엔드포인트** `POST /api/notifications/send` 로 기기 토큰/토픽/조건 대상 메시지 발송  
2. `.env` / `appsettings.*.json` 기반 환경 분리 및 비밀치 보호  
3. Swagger UI 자동 문서화 및 예제 요청 제공  
4. Google 공식 SDK(`Google.Apis.FirebaseCloudMessaging.v1`) 사용으로 향후 API 변경에 안정적 대응  
5. 단위 테스트(`FcmMessageFactory`)로 핵심 변환 로직 검증

## 2. 사전 준비
1. .NET SDK 8.0 이상 설치  
2. Firebase 프로젝트 생성 및 **서비스 계정 키(JSON)** 발급  
3. 최소 한 개 이상의 테스트 기기 토큰 또는 토픽 이름

## 3. 환경 구성
1. 저장소 루트에서 `.env.example`을 복사해 `.env` 생성
   ```bash
   cp .env.example .env
   ```
2. `.env` 혹은 `appsettings.Development.json`에 다음 값을 채웁니다.
   - `FIREBASE__PROJECTID`: Firebase 콘솔의 프로젝트 ID
   - `GOOGLE_APPLICATION_CREDENTIALS`: 서비스 계정 JSON 파일 경로  
     (`secrets/firebase-service-account.json` 등 Git에 커밋되지 않는 위치 권장)
   - `FIREBASE__DEFAULTDEVICETOKEN` (선택): 요청 본문에 토큰이 없을 때 사용할 기본 기기 토큰
   - 필요 시 `GOOGLE_APPLICATION_CREDENTIALS_JSON` 또는 `..._BASE64`로 직렬화 값 전달 가능

> `.gitignore`에 `secrets/`, `.env` 가 포함되어 있으니 민감정보가 저장소에 노출되지 않습니다.

## 4. 실행 방법
1. 의존성 복구
   ```bash
   dotnet restore
   ```
2. API 실행
   ```bash
   dotnet run --project src/FcmSender.Api
   ```
3. 기본 엔드포인트
   - Swagger UI: `https://localhost:5001/swagger` (개발 인증서 신뢰 필요)
   - HTTP 호출: `POST https://localhost:5001/api/notifications/send`

### 예제 요청 (curl)
```bash
curl -X POST https://localhost:5001/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
        "title": "Server notice",
        "body": "This is a push message from our new API",
        "token": "device-token",
        "data": { "screen": "dashboard" }
      }'
```

## 5. 테스트
단위 테스트 실행:
```bash
dotnet test
```
`FcmSender.Tests`는 메시지 변환 로직의 기본 동작(토큰/토픽 우선 순위, 디폴트 토큰 동작 등)을 검증합니다.

## 6. 프로젝트 구조
```
.
├── src/
│   ├── FcmSender.Api/        # Minimal API + Swagger
│   └── FcmSender.Core/       # FCM 옵션, 크리덴셜, 메시지 변환/전송 로직
├── tests/
│   └── FcmSender.Tests/      # xUnit 단위 테스트
├── .env.example              # 환경 변수 샘플
└── FcmSender.sln
```

## 7. 운영 시 고려 사항
1. **HTTPS**: 템플릿 기본 인증서를 사용 중이며, 실 서버 배포 시 정식 인증서 구성 필요  
2. **레이트 리밋**: 공용 API로 공개할 경우 별도 게이트웨이/역프록시에서 제한 권장  
3. **로깅/모니터링**: 현재 로그는 기본 수준이며, 운영 환경에서는 구조화 로깅(Serilog 등) 추가 권장  
4. **신뢰할 수 있는 호스트**: `AllowedHosts` 설정 검토  
5. **메시지 검증 모드**: `validateOnly: true` 로 Dry-Run 가능 → CI 체크용으로 활용 가능

## 8. 참고 문서
- [Firebase Cloud Messaging HTTP v1 공식 문서](https://firebase.google.com/docs/cloud-messaging/send-message?hl=ko#http_post_request)
- [.NET Minimal API + Swagger](https://learn.microsoft.com/aspnet/core/tutorials/min-web-api)

이 프로젝트를 기반으로 팀 내 FCM 발송 자동화 파이프라인이나 백오피스 연동에 활용해 보세요. 추가 개선이 필요하면 Issue/PR 환영입니다! 😄