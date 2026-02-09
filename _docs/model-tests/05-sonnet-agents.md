# Model Downgrade Tests: Agents (Opus → Sonnet)

## Critical Context Note

Agents have **preloaded skills** specified in their frontmatter. These skills are loaded into the model's context before the task is given. The model reads and applies the skill patterns — it does NOT need domain knowledge from training.

**Both model runs MUST have the agent's preloaded skills injected as context.** Here's what each agent needs:

| Agent | Preloaded Skills |
|-------|-----------------|
| product-manager | None (standalone instructions) |
| codex-validator | None (standalone instructions) |
| backend-implementer | `saurun:dotnet-tactical-ddd`, `saurun:dotnet-tdd` |
| frontend-implementer | `saurun:react-frontend-patterns`, `saurun:react-tailwind-v4-components`, `frontend-design:frontend-design`, `saurun:react-tdd` |

## Test Methodology

1. Load the agent's .md content + all preloaded skill SKILL.md content
2. Run with test input on Opus — capture output
3. Run with identical context + input on Sonnet — capture output
4. Compare against criteria below
5. GREEN = Sonnet sufficient, RED present (but not in Opus) = needs Opus

---

### product-manager (Opus → Sonnet)

**What it does:** Transforms raw ideas into structured product plans with user stories, acceptance criteria, and feature specifications.

**Test input:**
```
Create a product plan for a feature that lets SaaS users invite team members via email. Users should be able to set roles (Admin, Editor, Viewer) and see pending invitations. We need this to be simple but secure.
```

**GREEN criteria (pass — Sonnet is sufficient):**
- Includes Executive Summary with all required sections (Elevator Pitch, Problem Statement, Target Audience, USP, Success Metrics)
- Provides at least 3 user stories in correct format ("As a [persona], I want to [action], so that I can [benefit]")
- Each feature has acceptance criteria in Given/When/Then format
- Includes functional requirements (user flows, state management, validation, integration points)
- Includes non-functional requirements (performance, security, scalability, accessibility)
- Asks clarifying questions about gaps (checklist item about GAPS)
- Output is saved to `project-documentation/product-manager-output.md`
- Priorities justified (P0/P1/P2)

**RED indicators (fail — needs bigger model):**
- Missing any of the 5 Executive Summary components
- User stories lack persona/action/benefit structure
- Acceptance criteria missing or not in Given/When/Then format
- Skips non-functional requirements section entirely
- No clarifying questions asked
- Output not saved to specified location
- Generic/vague criteria that aren't testable
- Fails to address security in acceptance criteria (critical for invite feature)

---

### codex-validator (Opus → Sonnet)

**What it does:** Validates whether gaps found by Codex are genuine issues vs conversion losses or false positives.

**Test input:**
```
Validate this Codex finding:

PLAN.md path: /tmp/test-plan/PLAN.md
Source plan path: /tmp/test-plan/SOURCE.md
Gap: "Missing error handling specification for failed team member invitations"
Location: Phase 2 - Implementation Details, Team Invite Feature

PLAN.md excerpt:
"""
## Phase 2: Team Invite Feature
- Implement email invitation flow
- Add role selection (Admin, Editor, Viewer)
- Show pending invitations list
"""

SOURCE.md excerpt:
"""
Team member invitations should be sent via email with a secure token.
Users can assign roles (Admin, Editor, Viewer).
Show a list of pending invitations with status.
If the invitation email fails to send, show a clear error message and log the failure for debugging.
"""
```

**GREEN criteria (pass — Sonnet is sufficient):**
- Correctly identifies this as **CONVERSION_LOSS** (present in source, missing in PLAN.md)
- Validation output includes exact format: `VALIDATION: [VALID/INVALID]` and `GAP_TYPE: [type]`
- Source Check section quotes both files accurately
- Provides 2-3 sentence reasoning that references both source and PLAN.md
- Recommends a fix in structured format (for VALID findings)
- Concludes with `---\nEND_OF_VALIDATION`

**RED indicators (fail — needs bigger model):**
- Misclassifies the gap type (should be CONVERSION_LOSS, not GENUINE_GAP or FALSE_POSITIVE)
- Missing required output format sections (VALIDATION, GAP_TYPE, Source Check, Reasoning)
- Doesn't quote from both files
- Reasoning is generic or doesn't reference actual file content
- Accepts a gap that's actually present in PLAN.md (false positive)
- Rejects a gap that's clearly missing and important (false negative)
- No END_OF_VALIDATION marker

---

### backend-implementer (Opus → Sonnet)

**What it does:** Implements .NET backend features following DDD patterns with Result<T>, rich domain models, and TDD workflow.

**Preloaded context:** This agent has `dotnet-tactical-ddd` (value objects, Result<T>, factories, DTOs) and `dotnet-tdd` (Red-Green-Refactor) loaded. The test verifies Sonnet can follow these explicit instructions, not recall DDD from training.

**Test input:**
```
Implement a TeamInvitation entity and CreateInvitation command handler.

Requirements:
- TeamInvitation entity with: InvitationId (GUID), Email (EmailAddress value object), Role (enum: Admin/Editor/Viewer), Status (enum: Pending/Accepted/Expired), ExpiresAt (DateTimeOffset)
- CreateInvitationCommand with handler that:
  - Validates email format
  - Checks if user is already a team member
  - Creates invitation with 7-day expiration
  - Returns Result<TeamInvitationId>
- Use rich domain model (no anemic entities)
- Follow TDD: write failing test first

Assume boilerplate already exists (DbContext, Result<T> type, base Entity class).
```

**GREEN criteria (pass — Sonnet is sufficient):**
- Creates test file BEFORE implementation (TDD workflow from `dotnet-tdd` skill)
- TeamInvitation entity has domain logic methods (not anemic)
- Uses EmailAddress value object (not raw string)
- Enums properly defined (TeamRole, InvitationStatus)
- Command handler returns `Result<TeamInvitationId>` (not throwing exceptions)
- Validation failures return error Results (e.g., `Result.Failure("User already exists")`)
- Entity guards against invalid state (e.g., private setter, factory method)
- Test covers happy path + validation failures
- No DTOs in domain layer (DTOs are API boundary concern per `dotnet-tactical-ddd`)

**RED indicators (fail — needs bigger model):**
- Writes implementation before tests (violates TDD)
- Anemic entity (public setters, no domain logic)
- Uses raw strings instead of value objects
- Throws exceptions instead of returning Result<T>
- Missing validation in domain logic
- DTOs mixed with domain entities
- Test is integration test instead of unit test
- No factory method or domain invariant enforcement
- Skips ExpiresAt validation (should set to 7 days from now)

---

### frontend-implementer (Opus → Sonnet)

**What it does:** Builds React components using Tailwind v4, shadcn/ui, Zustand, and TanStack Query with TDD workflow.

**Preloaded context:** This agent has 4 skills loaded: `react-frontend-patterns` (Zustand selectors, TanStack Query), `react-tailwind-v4-components` (v4 syntax), `frontend-design` (visual design), and `react-tdd` (test-first). The test verifies Sonnet can follow all 4 skill instruction sets simultaneously.

**Test input:**
```
Implement a TeamInviteForm component.

Requirements:
- Form fields: email (input), role (select: Admin/Editor/Viewer), submit button
- Validation: email required and valid format, role required
- On submit: call useMutation hook to POST /api/invitations
- Show loading state during submission
- Show success message on completion
- Show error message on failure
- Use Tailwind v4 syntax
- Follow TDD: write test first
- Add data-testid attributes for E2E tests
- Use placeholder for success/error icons (data-asset)

Assume: useMutation hook and API client exist, shadcn/ui components installed.
```

**GREEN criteria (pass — Sonnet is sufficient):**
- Creates test file BEFORE component (TDD workflow from `react-tdd` skill)
- Test uses Vitest + RTL + MSW (per `react-tdd`)
- All interactive elements have `data-testid` in format `{component}-{element}-{action}`
- Uses Tailwind v4 syntax (@theme, theme() function per `react-tailwind-v4-components`)
- Form uses controlled inputs (state colocation per `react-frontend-patterns`)
- Mutation hook from TanStack Query (useMutation)
- Loading/error/success states handled
- Validation before submit (email format, required fields)
- Placeholders use `data-asset` attribute (not placeholder.com)
- Uses shadcn/ui components (e.g., Button, Input, Select from shadcn)

**RED indicators (fail — needs bigger model):**
- Writes component before test (violates TDD)
- Missing `data-testid` on any interactive element
- Wrong testid format (e.g., `submit-button` instead of `team-invite-form-submit`)
- Uses Tailwind v3 syntax (e.g., colors-3 instead of @theme)
- No validation before submit
- Doesn't use TanStack Query (uses fetch directly)
- Missing loading/error states
- Uses external placeholder service instead of data-asset
- Test uses Jest instead of Vitest
- Test doesn't mock API with MSW
- Uncontrolled form inputs (no state management)

---

## Key Observation

The implementer agents are the **strongest Sonnet candidates** in this test suite. Their preloaded skills provide explicit, detailed instructions for every pattern the model needs to apply. The model's job is instruction-following, not creative reasoning. If Sonnet fails these tests, it indicates an instruction-following deficit, not a knowledge gap.

The product-manager and codex-validator agents are riskier because they have NO preloaded skills — they rely on the model's own reasoning from their system prompt alone.
