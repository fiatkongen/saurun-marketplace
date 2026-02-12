# Research: AI Agent Input Contracts & Specification Formats

**Date:** 2026-02-12
**Scope:** What leading AI coding agents actually use as their specification/task input format, and what empirical evidence exists about which formats produce better code.

---

## 1. Agent-by-Agent Analysis

### 1.1 Claude Code (Anthropic)

**Input mechanisms:**
- **CLAUDE.md** — Persistent project memory file. Markdown with no mandated schema. Contains stack info, naming conventions, architecture patterns, testing standards, linting rules. Agent consults it before every decision.
- **Skills (SKILL.md)** — YAML frontmatter (`name`, `description`, `allowed-tools`, `user-invocable`, `context`, `model`, `agent`) + markdown body with instructions. Template vars (`$ARGUMENTS`, `$1`/`$2`). Dynamic context via shell injection (`` !`command` ``).
- **Agent definitions** — Single `.md` files with YAML frontmatter (`name`, `description`, `skills`, `model`) + system prompt body.
- **Plan Mode** — Read-only analysis mode (Shift+Tab x2). Forces think-before-act. No writing/execution allowed.
- **Native Tasks** — Built into Claude Code 2.1+ (Jan 2026). Tasks.md becomes living record of execution. Integrates with sub-agent spawning system.
- **Agent Teams** — Natural language task descriptions to team lead, who coordinates specialized teammates via task system and messaging.

**Key insight:** Claude Code uses a *layered* approach — persistent context (CLAUDE.md), reusable capabilities (Skills), and per-task natural language + plan mode. No rigid task spec schema.

### 1.2 Cursor

**Input mechanisms:**
- **Rules (.cursor/rules/*.mdc)** — MDC format (Markdown + YAML frontmatter). Frontmatter fields: `description`, `globs`, `alwaysApply`. Rule types inferred: Always, Auto-Attach (by glob), Agent (description only), Manual.
- **Legacy .cursorrules** — Single file in project root (deprecated as of v2.2).
- **Task input** — Natural language prompts in chat/composer. No structured task spec.

**Format example:**
```yaml
---
description: "Standards for TypeScript code"
globs: "*.ts,*.tsx"
alwaysApply: false
---
[markdown instructions]
```

**Key insight:** Cursor's approach is *context injection by file pattern* — rules activate based on what files you're editing. Tasks themselves are unstructured natural language.

### 1.3 Windsurf

**Input mechanisms:**
- **.windsurfrules** — Single file in project root. Plain markdown with project conventions.
- **Memories** — Persistent agent memory of project-specific rules (styling guides, legacy constraints) across sessions. Introduced late 2025.
- **Task input** — Natural language in chat. No structured spec format.

**Key insight:** Simplest approach of all IDE-based agents. Relies heavily on natural language + accumulated memory.

### 1.4 Cline

**Input mechanisms:**
- **Custom Instructions** — Text field in extension settings.
- **Rules** — Toggle-able rule files, enable/disable per task.
- **Plan/Act modes** — Plan mode gathers requirements and outlines implementation in markdown. Act mode uses those plans to guide code generation. Cline pioneered this two-mode approach.

**Key insight:** Cline was first to formalize markdown-based planning documents as an explicit agent input format.

### 1.5 Devin (Cognition)

**Input mechanisms:**
- **Natural language task description** — Describe the issue, feature, or update. Devin parses and loads relevant repo context.
- **Plan approval** — Devin generates a detailed plan with repo citations. User approves or adjusts before coding starts.
- **Knowledge base** — Persistent project knowledge Devin accumulates.

**Key insight:** Devin works best with *clear, upfront, complete natural language specifications*. Mid-task requirement changes degrade performance significantly. Scoping responsibility falls on the engineer. "Like most junior engineers, Devin does best with clear requirements."

### 1.6 SWE-Agent / SWE-Bench

**Task instance format (JSON/JSONL):**
```json
{
  "instance_id": "repo__issue_123",
  "problem_statement": "Issue title and body text",
  "repo": "owner/repo-name",
  "base_commit": "abc123...",
  "hints_text": "Comments on the issue",
  "created_at": "2025-01-15T...",
  "test_patch": "diff of test files",
  "patch": "gold solution diff"
}
```

**Key fields:** `problem_statement` (natural language issue title+body), `repo` + `base_commit` (codebase snapshot), `hints_text` (optional additional context).

**SWE-Bench Pro enhancement:** Adds human-written *requirements* list grounded in unit tests. Format: list of requirement statements that map to validation tests. Simulates standard engineering practice of resolving issues following problem specification.

**Evaluation:** Binary pass/fail. Task "resolved" only if: (1) fail-to-pass tests now pass, (2) all pass-to-pass tests still pass. Turn-based interaction capped at 200 turns.

**Key insight:** The most successful benchmark format is *natural language problem statement + codebase reference + testable acceptance criteria*. SWE-Bench Pro's addition of explicit requirements improved agent performance evaluation.

### 1.7 OpenHands (formerly OpenDevin)

**Input mechanisms:**
- **Event stream abstraction** — Chronological collection of Actions and Observations forming a perception-action loop.
- **Actions** — Strongly validated via Pydantic schemas. Types: `IPythonRunCellAction`, `CmdRunAction`, `BrowserInteractiveAction`, etc.
- **Observations** — Structured, serializable environmental feedback.
- **Task input** — Natural language instructions injected as events into the stream.

**Architecture (V1 SDK, Nov 2025):** Modular SDK with event-sourced state management, type-checked tooling, multi-LLM routing, pluggable memory, and MCP integration.

**Key insight:** OpenHands has the most *formally typed* internal representation — Pydantic-validated actions/observations — but task *input* is still natural language. The structure lives in the *execution interface*, not the task description.

### 1.8 AutoCodeRover

**Input:** Takes `problem_statement` (P) + `codebase` (C). Two-phase process:
1. LLM agent navigates codebase and extracts relevant code via context retrieval APIs
2. Second LLM agent uses collected context to generate patch

**Key insight:** Minimal structured input — just a problem statement and a codebase pointer. The agent itself structures its understanding through exploration.

### 1.9 Aider

**Input mechanisms:**
- **Natural language prompts** — One thing at a time. "Keep prompts tight."
- **CONVENTIONS.md** — Project-level coding conventions that influence generation.
- **AGENTS.md** — Supported as operational guide (shared format).
- **--read parameter** — Static reference files loaded as read-only context.
- **--message parameter** — The actual changing task prompt.

**Key insight:** Aider separates *persistent context* (conventions, agent instructions) from *per-task prompts*. Advocates small, focused tasks over monolithic specs.

### 1.10 GitHub Copilot Workspace

**Specification format — "Current State / Desired State" pattern:**
1. **Current State** — Bullet-point list describing current codebase behavior
2. **Desired State** — Bullet-point list articulating success criteria (not implementation details)
3. **Plan** — File-level action list (create/modify/delete) with bullet-point instructions per file
4. **Implementation** — Generated diffs

All steps are *fully editable* by the developer before proceeding.

**Key insight:** Copilot Workspace had the most *explicitly structured* intermediate representation — it forced a current→desired state delta format. This was the closest any tool came to a formal specification language for tasks. (Technical preview sunset May 2025, but the pattern influenced later tools.)

### 1.11 OpenAI Codex CLI

**Input mechanisms:**
- **AGENTS.md** — Hierarchical discovery: `~/.codex/AGENTS.override.md` → per-directory walk. Max 32 KiB combined. No mandated schema — free-form markdown.
- **Agent Skills (SKILL.md)** — Portable across tools. Contains name, description, instructions, optional scripts/references. Same pattern as Claude Code skills.
- **Task input** — Natural language prompts.

**Key insight:** OpenAI adopted Claude Code's skill format and AGENTS.md became a cross-tool standard. Task input remains natural language.

### 1.12 Amazon Q Developer Agent

**Agent configuration (JSON):**
```json
{
  "name": "my-agent",
  "description": "What the agent does",
  "prompt": "System-level instructions (or file:// URI)",
  "model": "claude-sonnet-4",
  "tools": ["fs_read", "execute_bash", "@git"],
  "allowedTools": ["fs_read", "fs_write"],
  "toolsSettings": {
    "fs_read": { "allowedPaths": ["src/"] },
    "execute_bash": { "allowedCommands": ["npm test"] }
  },
  "mcpServers": { ... },
  "context": { "files": ["**/*.ts"], "commands": ["npm run build"] }
}
```

**Key insight:** Amazon Q has the most *granular permission model* of any agent — tool-level allow/deny paths, command whitelists, MCP server configs. Agent *configuration* is highly structured, but task *input* is still natural language.

---

## 2. Emerging Standards

### 2.1 AGENTS.md — The Cross-Tool Standard

- **Status:** Open standard under Linux Foundation (Agentic AI Foundation). Used by 60k+ repos.
- **Format:** Plain markdown. No mandated schema. "Use any headings you like."
- **Hierarchical:** Closest file to edited path takes precedence. Monorepo-friendly.
- **Supported by:** OpenAI Codex, Google Jules, Cursor, GitHub Copilot, Aider, Devin, Windsurf, Factory, Amp, Kilo Code, 20+ tools.
- **Content:** Build commands, test instructions, code style, security boundaries, deployment steps.
- **Designed for:** Agent-facing operational context. Complements README.md (human-facing).
- **Limit:** 32 KiB combined (Codex default).

**Assessment:** AGENTS.md won the "project context" format war. It is *not* a task specification format — it provides persistent environmental context, not per-task requirements.

### 2.2 Spec-Driven Development (SDD) — The Emerging Paradigm

Three tools have formalized SDD workflows:

#### Kiro (AWS)
**Three-file structure:**
- **requirements.md** — EARS notation: `WHEN [condition] THE SYSTEM SHALL [behavior]`
- **design.md** — Architecture, data models, sequence diagrams, interfaces
- **tasks.md** — Checklist of implementation tasks with descriptions, outcomes, dependencies

Workflow: Requirements → Design → Implementation. All files auto-loaded into conversation context.

#### GitHub Spec Kit
**Four-phase workflow:**
1. **Specify** — Generate detailed spec
2. **Plan** — Technical architecture
3. **Tasks** — Small, reviewable units (e.g., "Create user registration endpoint that validates email format")
4. **Implement** — Execute per task with review

Agent-agnostic (works with Copilot, Claude Code, Gemini CLI, Cursor, Windsurf).

#### Tessl
**Spec-anchored approach:**
- Specs as primary artifact (not throwaway)
- `@generate` annotation triggers code generation from spec
- `@describe` documents existing code
- `// GENERATED FROM SPEC - DO NOT EDIT` markers
- Spec Registry with 10k+ pre-built library specs
- Aspires to "spec-as-source" — humans edit specs, code is derived

### 2.3 Anthropic Job Spec Format (2026 Agentic Coding Trends Report)

**Minimal agent job spec (Appendix C):**
```yaml
jobId: J-2026-000123
repo: org/service-a
baseRef: main
objective: "Add rate limiting to /v1/search"
constraints:
  - "No new paid dependencies"
  - "p95 latency must not regress > 2%"
checkpoints:
  - "Spec approved"
  - "Security review passed"
budgets:
  max_wall_clock_minutes: 240
  max_ci_minutes: 120
  max_model_cost_usd: 25
permissions:
  git_write: true
  prod_deploy: false
```

**State machine:** INTENT → SPEC → PLAN → IMPLEMENT → VERIFY → DOCS → REVIEW → RELEASE → MONITOR

**Key insight:** This is the most *production-oriented* task format — includes budgets, permissions, checkpoints, and governance. Designed for durable, long-running agent jobs.

---

## 3. Empirical Evidence

### 3.1 Prompt Specificity and Code Quality

**Study:** "More Than a Score: Probing the Impact of Prompt Specificity on LLM Code Generation" (2025)

**Results:**
- HumanEval pass@1: 0.280 (minimal) → 0.860 (moderate, ~100 words) → 0.921 (full detail)
- ParEval-Serial: 0.800 → 0.983 (+18.3% with max detail)
- ParEval-OMP: 0.667 → 0.967 (+30% with max detail)

**Most impactful specificity dimensions:**
1. Explicit I/O specifications
2. Edge case handling
3. Implementation sketches / pseudocode
4. Core behavior descriptions
5. Constraint clarification

**Takeaway:** More structured, detailed prompts produce dramatically better code. The jump from minimal to moderate detail is the biggest gain.

### 3.2 Prompt Guidelines for Code Generation

**Study:** "Guidelines to Prompt Large Language Models for Code Generation" (Jan 2026, 4 LLMs, 1,678 tasks)

**Most impactful guidelines (by frequency of enabling previously-failing tasks):**
1. **Algorithmic details** (57%) — Provide essential algorithms, formulas
2. **I/O format** (44%) — Clarify data types, shapes, structures, edge cases
3. **More examples** (24%) — Concrete doctests showing expected behavior
4. **Post-conditions** (23%) — Define output guarantees (e.g., "return value in [0,1]")
5. **Requirements** (19%) — Explicitly state dependencies with explanations
6. **Exceptions** (12%) — Specify which exceptions under what conditions
7. **Assertive language** (9%) — "Must" instead of "should"

**Practitioner survey:** 88% rated I/O format specification as "useful."

### 3.3 SANER 2026 Registered Report (Pre-Results)

**Tool:** CURRANTE — three-phase TDD-inspired workflow using TOML specs.
- Phase 1: Structured specification in TOML
- Phase 2: LLM generates tests, user refines (explain/regenerate/delete)
- Phase 3: LLM generates code against curated test suite

**Status:** Protocol peer-reviewed and accepted. Empirical results pending.

### 3.4 Developer Practices Survey (July 2025)

**Finding from 18 practitioners:** "Requirements, as typically documented, are too abstract for direct input into LLMs."

Developers compensate by:
- Adding concrete examples to abstract requirements
- Converting user stories to more specific technical descriptions
- Including code snippets and expected outputs
- Iterating with the LLM to refine understanding

### 3.5 Spec-Driven Development Claims

**Thoughtworks (2025):** "Every hour spent [on planning] saves 10 hours of rework." No controlled study cited.

**Red Hat Developer (2025):** Claims "95% or higher accuracy in implementing specs on the first go" — aspirational, not empirically demonstrated in controlled conditions.

**Augment Code:** Reports "56% programming time reduction and 30-40% faster time-to-market" with specification-driven workflows across enterprise deployments.

**Martin Fowler analysis:** Identifies three SDD maturity levels:
1. **Spec-first** — Write spec before task, discard after
2. **Spec-anchored** — Keep spec for evolution and maintenance
3. **Spec-as-source** — Spec is primary artifact, code is derived

**Honest assessment:** Strong directional evidence that structured specs improve AI code output, but rigorous controlled experiments with statistical significance are largely absent. Most evidence is observational/anecdotal from tool builders.

### 3.6 Model-Specific Sensitivity

**Finding:** Optimized prompts are model-specific. GPT-4o achieved 116.1s with its own template but degraded 23.5% with Claude's template and 29% with Gemini's. Generic "improved" templates can hurt: extraction pass rate dropped from 100% → 90% for Llama 3.

**Implication:** There is no universally optimal specification format. Format effectiveness is model-dependent.

---

## 4. Addy Osmani's Spec Recommendations (Based on 2,500+ Agent Config Analysis)

**Six essential components of agent specs:**
1. **Commands** — Full executable commands with flags
2. **Testing** — Frameworks, locations, coverage expectations
3. **Project structure** — Directory organization
4. **Code style** — Examples of preferred conventions
5. **Git workflow** — Branch naming, commit formats, PR requirements
6. **Boundaries** — What agents must never touch

**Three-tier boundary system:**
- **Always do** — No approval needed ("Always run tests before commits")
- **Ask first** — Human review required ("Ask before modifying DB schemas")
- **Never do** — Hard stops ("Never commit secrets")

**"Never commit secrets"** was the single most frequently helpful constraint across 2,500+ configs.

---

## 5. Cross-Cutting Patterns

### What All Agents Share
1. **Natural language task input** — Every single agent accepts natural language. None requires a structured DSL for task description.
2. **Markdown as the universal format** — For context, instructions, plans, and specs. Not JSON, not YAML, not XML.
3. **Separation of persistent context from per-task input** — CLAUDE.md / AGENTS.md / .cursorrules (persistent) vs. chat prompts (per-task).
4. **Plan-before-execute pattern** — Nearly every agent now supports some form of planning step before code generation.

### Where They Diverge
1. **Degree of structure in task input** — Ranges from pure chat (Windsurf) to YAML job specs (Anthropic trends report).
2. **Permission/constraint modeling** — Ranges from none (Aider) to granular tool-level allow/deny (Amazon Q).
3. **Spec lifecycle** — Ranges from throwaway (most) to spec-as-source (Tessl).
4. **Intermediate representations** — Current/desired state (Copilot Workspace), requirements/design/tasks (Kiro), event streams (OpenHands).

### The Emerging Consensus
1. **Semi-structured markdown** is the sweet spot — not rigid schemas, not pure prose.
2. **Testable acceptance criteria** improve output quality (SWE-Bench Pro, Kiro EARS).
3. **Examples beat descriptions** — Concrete I/O examples, code snippets, and doctests are the single most impactful addition to specs.
4. **Constraints and boundaries** are as important as requirements — "never do X" is often more valuable than "do Y."
5. **Layered context** works better than monolithic specs — persistent project context + per-task instructions + runtime constraints.

---

## 6. Synthesis: What Actually Produces Better Code

### High-Confidence Findings (Empirically Supported)
- Adding I/O examples improves pass@1 by 20-30% (prompt specificity study)
- Algorithmic details in prompts enable 57% of previously-failing tasks (guidelines study)
- Edge case specification significantly improves correctness
- Post-conditions ("output must be in range [0,1]") provide clear verification targets
- Pre-conditions reduce ambiguity and hallucination

### Medium-Confidence Findings (Observational/Directional)
- Spec-first workflows reduce rework (10x claim from Thoughtworks, unverified)
- EARS-style structured requirements improve requirement clarity and testability
- Separation of requirements/design/tasks produces more maintainable output
- Current-state/desired-state delta format helps agents understand scope

### Low-Confidence Findings (Anecdotal/Aspirational)
- 95%+ first-pass accuracy with good specs (Red Hat claim, no controlled study)
- 56% programming time reduction (Augment Code claim)
- Spec-as-source will replace code-as-source (Tessl vision, very early)

### What Definitely Doesn't Work
- Vague, ambiguous prompts ("build me something cool")
- Mid-task requirement changes (Devin performance data)
- Massive context dumps without summarization
- Generic prompt templates applied across different models (23-29% degradation)
- Abstract requirements without concrete examples

---

## 7. Format Comparison Table

| Agent/Tool | Task Input Format | Persistent Context | Structured? | Permission Model |
|---|---|---|---|---|
| Claude Code | Natural language + plan mode | CLAUDE.md, Skills | Semi (YAML frontmatter + MD) | Tool allowlists in skills |
| Cursor | Natural language | .cursor/rules/*.mdc | Semi (YAML frontmatter + MD) | None |
| Windsurf | Natural language | .windsurfrules + Memories | Minimal | None |
| Cline | Natural language + plan/act | Custom instructions, rules | Minimal | None |
| Devin | Natural language + plan approval | Knowledge base | Minimal | None |
| SWE-Bench | JSON (problem_statement + repo) | None | Structured (JSON fields) | N/A (benchmark) |
| OpenHands | Natural language (as events) | Event stream history | Typed actions (Pydantic) | SDK-level policies |
| AutoCodeRover | Problem statement + codebase | None | Minimal | N/A |
| Aider | Natural language | CONVENTIONS.md, AGENTS.md | Minimal | None |
| Copilot Workspace | Current/Desired state bullets | None | Semi (structured bullets) | N/A (sunset) |
| Codex CLI | Natural language | AGENTS.md, SKILL.md | Semi (YAML frontmatter + MD) | AGENTS.md conventions |
| Amazon Q | Natural language | Agent JSON config | Structured (JSON config) | Granular tool permissions |
| Kiro | EARS requirements + design + tasks | Spec files in context | Structured (3-file SDD) | None |
| Spec Kit | Specify/Plan/Tasks/Implement | Spec files | Semi (4-phase markdown) | None |
| Tessl | Annotated specs (@generate) | Spec Registry | Structured (spec-anchored) | None |
| Anthropic Job Spec | YAML (objective + constraints + budgets) | CLAUDE.md | Structured (YAML schema) | Explicit permissions field |

---

## 8. Implications for Our Pipeline Design

1. **Natural language is table stakes** — any spec format must be readable as natural language, not just by parsers.
2. **Semi-structured markdown is the proven sweet spot** — YAML frontmatter + markdown body is the dominant pattern across tools.
3. **Testable acceptance criteria are the highest-ROI addition** — whether EARS, Given/When/Then, or concrete examples.
4. **Layered context beats monolithic specs** — separate persistent project context, reusable skill definitions, and per-task specifications.
5. **Constraints/boundaries are first-class** — "never do X" patterns are as valuable as positive requirements.
6. **The Anthropic Job Spec format is the most forward-looking** — includes budgets, permissions, checkpoints, and state machine lifecycle. Worth studying for production pipeline design.
7. **Don't over-structure** — every successful format allows natural language within its structure. Pure schemas (JSON-only, YAML-only) are used for *configuration*, not *task description*.
8. **Examples > descriptions** — empirically, concrete I/O examples improve code quality more than any other single intervention.
