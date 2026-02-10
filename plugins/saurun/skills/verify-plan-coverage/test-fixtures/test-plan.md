# Upload Feature Implementation Plan

> **For Claude:** **REQUIRED SUB-SKILL:** Use `saurun:dotnet-tdd` to implement this plan task-by-task with TDD.

**Goal:** Implement CSV file upload with processing pipeline

**Architecture:** `_docs/specs/2026-02-10-upload-architecture.md`

**Tech Stack:** .NET 9, ASP.NET Core, EF Core 9, SQLite, xUnit, NSubstitute

---

### Task 1: Test infrastructure setup

**Implements:** Shared test helpers (N/A - infrastructure)

**Files:**
- Create: `Tests/CustomWebApplicationFactory.cs`
- Create: `Tests/TestHelpers.cs`

**Behaviors:**
- CustomWebApplicationFactory configured with SQLite in-memory
- TestHelpers provides authenticated HttpClient creation

### Task 2: Upload aggregate

**Implements:** Upload entity (Architecture §Entity Model)

**Files:**
- Create: `Domain/Aggregates/Upload.cs`
- Test: `Tests/Domain/UploadTests.cs`

**Behaviors:**
- Creating with valid fileName and fileSize initializes status as Pending
- Validate rejects files over 5MB
- Process transitions status to Completed
- MarkFailed increments retry count

### Task 3: Upload endpoint

**Implements:** POST /uploads (Architecture §API Contract)

**Files:**
- Create: `Api/Endpoints/UploadEndpoints.cs`
- Test: `Tests/Api/UploadEndpointsTests.cs`

**Behaviors:**
- Valid CSV upload → 201 with UploadDto
- File too large → returns HTTP 400

### Task 4: UploadPage

**Implements:** UploadPage (Architecture §Component Tree)

**Files:**
- Create: `src/pages/UploadPage.tsx`
- Test: `src/pages/__tests__/UploadPage.test.tsx`

**Behaviors:**
- Renders UploadForm component
- Shows UploadList after successful upload

### Task 5: Notification service

**Implements:** PATCH /uploads/{id}/notify (Architecture §API Contract)

**Files:**
- Create: `Api/Endpoints/NotificationEndpoints.cs`
- Test: `Tests/Api/NotificationEndpointsTests.cs`

**Behaviors:**
- Sends notification after upload completes
- Returns 200 on success
