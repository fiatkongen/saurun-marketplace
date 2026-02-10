PLAN COVERAGE REPORT

Spec: test-fixtures/test-spec.md
Architecture: test-fixtures/test-architecture.md
Plan: test-fixtures/test-plan.md

SPEC REQUIREMENTS:
S1: "Users can upload CSV files up to 10MB" → MUTATED
    Task 2 says "5MB", spec says "10MB"
S2: "Retry failed uploads 3 times with exponential backoff (1s, 2s, 4s)" → MISSING
    No task implements retry logic
S3: "Return HTTP 413 if file exceeds size limit" → MUTATED
    Task 3 says "HTTP 400", spec says "HTTP 413"
S4: "Admin users can upload up to 50MB" → MISSING
    No task differentiates admin upload limits
S5: "Rate limit: max 100 uploads per hour per user" → MISSING
    No task implements rate limiting

USER FLOWS:
UF1: "User selects CSV file" → COVERED (Task 3 - upload endpoint)
UF2: "System validates file size and format" → PARTIAL (Task 2 validates size but not format)
UF3: "System uploads and processes file" → COVERED (Task 2 Process behavior + Task 3)
UF4: "System shows success/error message" → PARTIAL (Task 4 shows list but no explicit error state)

ARCHITECTURE CONTRACTS:
EP1: POST /uploads → COVERED (Task 3)
EP2: GET /uploads/{id} → MISSING
EP3: DELETE /uploads/{id} → MISSING
E1: Upload entity → COVERED (Task 2)
E2: UploadError entity → MISSING
C1: UploadPage → COVERED (Task 4)
C2: UploadForm → PARTIAL (referenced in Task 4 behavior, no dedicated task)
C3: UploadList → PARTIAL (referenced in Task 4 behavior, no dedicated task)
C4: UploadStatus → MISSING

Note: Task 1 ("Shared test helpers, N/A - infrastructure") is exempt from orphan checking.

ORPHAN TASKS:
Task 5: "PATCH /uploads/{id}/notify" → NOT IN ARCHITECTURE
    No matching endpoint in Architecture §API Contract

FIXES NEEDED:
1. MUTATED: Task 2, behavior 2: Change "5MB" to "10MB"
2. MUTATED: Task 3, behavior 2: Change "HTTP 400" to "HTTP 413"
3. ADD BEHAVIOR to Task 2: "Retries failed upload 3 times with exponential backoff (1s, 2s, 4s)" (or add as new task if retry is a separate concern)
4. ADD TASK: Admin upload limits — Validate admin users can upload up to 50MB
5. ADD TASK: Rate limiting — max 100 uploads/hour/user
6. ADD TASK: GET /uploads/{id} endpoint → UploadDto (200)
7. ADD TASK: DELETE /uploads/{id} endpoint → 204
8. ADD TASK: UploadError entity
9. ADD TASK: UploadForm component
10. ADD TASK: UploadList component
11. ADD TASK: UploadStatus component
12. ORPHAN WARNING: Task 5 implements "PATCH /uploads/{id}/notify" which doesn't exist in architecture. Remove task or add endpoint to architecture.

SUMMARY:
Spec: 0/5 fully covered (0%) — 0 COVERED, 0 PARTIAL, 3 MISSING, 2 MUTATED
  (Note: MUTATED counts as "not fully covered" — the requirement exists but has wrong values)
User Flows: 2/4 fully covered (50%) — 2 COVERED, 2 PARTIAL
Architecture: 3/9 fully covered (33%) — 3 COVERED, 2 PARTIAL, 4 MISSING
Mutations: 2
Orphan tasks: 1
