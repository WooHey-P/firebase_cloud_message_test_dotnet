# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

.NET 8 Minimal API server that sends Firebase Cloud Messaging (FCM) push notifications via the **HTTP v1 API** (using the `Google.Apis.FirebaseCloudMessaging.v1` client, not the Firebase Admin SDK).

## Commands

```bash
# Restore dependencies
dotnet restore

# Run the API (Swagger UI at http://localhost:5130/swagger)
dotnet run --project src/FcmSender.Api

# Build the whole solution
dotnet build

# Run all tests
dotnet test

# Run a single test
dotnet test --filter "FullyQualifiedName~FcmMessageFactoryTests.Create_WithToken_SetsTokenTarget"

# First-time setup: interactively generates .env from a service account JSON in secrets/
./init.sh
```

Tests use **xUnit** (`FcmSender.Tests`). `GlobalUsings.cs` globally imports `Xunit`, so test files don't need `using Xunit;`.

## Architecture

Two projects plus tests:

- **`FcmSender.Core`** — framework-agnostic sending logic. No ASP.NET dependency.
- **`FcmSender.Api`** — Minimal API host, Swagger, HTTP contracts.

### Request flow

`POST /api/notifications/send` → `NotificationEndpoints.SendAsync` → maps the API contract `SendNotificationRequest.ToDomainRequest()` to the domain `FcmNotificationRequest` → `IFcmSender` (`FirebaseMessagingSender`) → builds a `Message` via `IFcmMessageFactory` (`FcmMessageFactory`) → sends through `FirebaseCloudMessagingService`.

Key seams (all registered in `Program.cs`):

- `IFirebaseCredentialProvider` (`FirebaseCredentialProvider`, singleton) — loads and caches a scoped `GoogleCredential`.
- `IFcmMessageFactory` (`FcmMessageFactory`, singleton) — pure `FcmNotificationRequest` → `Message` translation; the most unit-testable piece.
- `IFcmSender` (`FirebaseMessagingSender`, scoped) — orchestrates credential + factory + API call.

### Two-layer request model

There is deliberate separation between the **API contract** (`FcmSender.Api/Contracts/Requests/SendNotificationRequest.cs`) and the **domain model** (`FcmSender.Core/Models/FcmNotificationRequest.cs`). The API contract carries DataAnnotations validation and JSON shape concerns (e.g. `[JsonPropertyName("content-available")]`), then `ToDomainRequest()` flattens it into the domain record. When adding a field, update **both** models and the mapping.

The API contract accepts title/body **either** at the top level **or** in a nested `notification` object — `ToDomainRequest()` prefers the nested object.

### Message targeting

`FcmMessageFactory.ResolveTarget` picks the delivery target in strict priority order: `Token` → `Topic` → `Condition` → configured `DefaultDeviceToken`. If none is present it throws `InvalidOperationException`, which the endpoint converts into a `ValidationProblem`.

## Configuration & credentials

Config binds to the `Firebase` section (`FirebaseOptions`, section name `"Firebase"`). `Program.cs` validates that `ProjectId` is non-empty at startup.

Environment variables (loaded from `.env` via `DotNetEnv` in `Program.cs`) are mapped into the `Firebase` config section:

- `GOOGLE_APPLICATION_CREDENTIALS` — path to the service account JSON. **Required.** The `project_id` is auto-extracted from this JSON at startup (`TryExtractProjectIdFromServiceAccount`) if `FIREBASE_PROJECTID` is not set explicitly.
- `FIREBASE_DEFAULTDEVICETOKEN` — optional fallback device token.
- Credential JSON can alternatively be supplied inline via `GOOGLE_APPLICATION_CREDENTIALS_JSON` or `GOOGLE_APPLICATION_CREDENTIALS_JSON_BASE64` (see `FirebaseCredentialProvider.ResolveCredentialSource`).

The `.env` `GOOGLE_APPLICATION_CREDENTIALS` path is **relative to the API project working directory** (`src/FcmSender.Api`), which is why `init.sh` writes `../../secrets/<file>`.

`secrets/` and `.env` are git-ignored — never commit service account keys.

## Conventions

- File-scoped `namespace` declarations, with `using` directives placed **inside** the namespace in `Core` service files.
- Nullable reference types and implicit usings are enabled.
- Domain models are `sealed record`; services are `sealed class`.
- User-facing validation/error messages are written in **Korean**.
