#!/bin/bash

echo "==================================="
echo "  FCM Sender 초기화 스크립트"
echo "==================================="
echo ""

# .env 파일 존재 여부 확인
if [ -f ".env" ]; then
    read -p ".env 파일이 이미 존재합니다. 덮어쓰시겠습니까? (y/N): " overwrite
    if [[ ! "$overwrite" =~ ^[Yy]$ ]]; then
        echo "초기화를 취소합니다."
        exit 0
    fi
fi

# 서비스 계정 JSON 파일명 입력
echo ""
echo "서비스 계정 JSON 파일명을 입력하세요."
echo "(secrets/ 폴더에 미리 저장해두세요)"
echo "(예: my-project-firebase-adminsdk.json)"
read -p "파일명: " service_account_filename

if [ -z "$service_account_filename" ]; then
    echo "오류: 서비스 계정 JSON 파일명은 필수입니다."
    exit 1
fi

# secrets 폴더 생성
mkdir -p secrets

# 파일 존재 여부 확인
if [ ! -f "secrets/$service_account_filename" ]; then
    echo ""
    echo "secrets/$service_account_filename 파일이 없습니다."
    echo "Firebase Console에서 다운로드한 서비스 계정 JSON 파일을"
    echo "secrets/ 폴더에 복사한 후 다시 실행하세요."
    exit 1
fi

# 기본 FCM 토큰 입력 (선택)
echo ""
read -p "기본 FCM 디바이스 토큰 (선택, Enter로 건너뛰기): " default_token

# .env 파일 생성
cat > .env << EOF
GOOGLE_APPLICATION_CREDENTIALS=../../secrets/$service_account_filename
FIREBASE_DEFAULTDEVICETOKEN=$default_token
EOF

echo ""
echo "==================================="
echo "  초기화 완료!"
echo "==================================="
echo ""
echo "다음 명령어로 서버를 실행하세요:"
echo ""
echo "  dotnet restore"
echo "  dotnet run --project src/FcmSender.Api"
echo ""
echo "Swagger UI: http://localhost:5130/swagger"
echo ""
