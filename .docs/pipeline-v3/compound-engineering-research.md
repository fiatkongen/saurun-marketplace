# Compound Engineering Research

**Date:** 2026-02-12
**Source:** Every Inc (EveryInc/compound-engineering-plugin)
**Authors:** Dan Shipper (CEO) & Kieran Klaassen (Engineer)
**Plugin version at time of research:** 2.31.1 (29 agents, 24 commands, 18 skills, 1 MCP server)
**GitHub:** 8.4k stars, 660 forks, 179 commits, 26 contributors

---

## 1. Philosophy / Core Thesis

**Central claim:** "Each unit of engineering work should make subsequent units easier -- not harder."

Traditional software engineering accumulates technical debt: more code = more edge cases, interdependencies, and friction. Compound engineering inverts this by ensuring that every feature, bug fix, and review cycle teaches the system something reusable. The codebase gets *easier* to work with over time, not harder.

**The inversion:** Developer role shifts from "code author" to "orchestrator and quality arbiter." Primary skills become requirements specification, plan review, architectural judgment, and teaching the AI system. Writing code is explicitly *not* the developer's job.

**The 80/20 split:** ~80% of effort goes into planning and review. ~20% into execution and knowledge capture. This is the opposite of traditional dev where most time is spent writing/debugging code.

**The 50/50 rule (from their guide):** Allocate equal time to building features vs. improving the systems that enable faster future work.

**Key beliefs to adopt (from their definitive guide):**
- Extract taste into systems (CLAUDE.md, specialized agents) rather than relying on manual review
- Trust through safeguards (tests, automated review) rather than manual gatekeeping
- Plans are primary artifacts -- more valuable than individual code implementations
- Embrace imperfect results that scale over perfect results that don't
- Measure output by problems solved, not keystrokes logged

**Beliefs to unlearn:**
- Code must be manually written
- Every line requires manual review
- Solutions originate from the engineer alone
- Writing code is the core job function
- Typing equals learning

---

## 2. Work Decomposition: The Four-Phase Loop

### Plan -> Work -> Review -> Compound -> Repeat

The loop is the central workflow. Each phase has dedicated slash commands.

### Phase 1: Plan (`/workflows:plan`) -- 40% of time

**What it does:**
1. Checks for recent brainstorm docs in `docs/brainstorms/` (within 14 days)
2. Collaborative questioning to clarify scope, constraints, success criteria
3. Parallel research agents run:
   - `repo-research-analyst` -- codebase structure, conventions, patterns
   - `learnings-researcher` -- searches `docs/solutions/` for institutional knowledge
   - `best-practices-researcher` -- external docs, framework guidance, deprecation checks
   - `git-history-analyzer` -- commit history patterns
   - `framework-docs-researcher` -- framework-specific docs via Context7 MCP
4. SpecFlow analysis via `spec-flow-analyzer` agent -- validates completeness, identifies gaps and edge cases
5. Detail level selection: MINIMAL (quick issues), MORE (standard), A LOT (comprehensive)
6. Generates plan file: `docs/plans/YYYY-MM-DD-<type>-<descriptive-name>-plan.md`
7. Optional: creates GitHub/Linear issue automatically

**Key insight:** Planning is research-driven. Agents query the codebase, commit history, internet best practices, AND the organization's own documented solutions before producing a plan. The plan is a collaborative artifact between human judgment and agent research.

### Phase 2: Work (`/workflows:work`) -- 10% of time

**What it does:**
1. Read and clarify the plan document
2. Set up environment (branch or git worktree for isolation)
3. Create TodoWrite task list from plan
4. Execute tasks in priority order, marking progress
5. Update the original plan document by checking off completed items
6. Make incremental commits at logical boundaries
7. Run tests continuously (not end-of-cycle)
8. Create PR with summary, testing notes, screenshots

**Optional swarm mode:** For complex plans with 5+ independent tasks, launches parallel agents to maximize throughput.

**Key insight:** Work is the smallest phase. The plan is already so detailed that execution is largely mechanical. The agent follows instructions rather than making architectural decisions.

### Phase 3: Review (`/workflows:review`) -- 30% of time

**What it does:**
1. Identifies review target (PR, branch, file path)
2. Offers git worktree isolation for review
3. Launches 13+ specialized review agents **in parallel**:
   - `security-sentinel` -- OWASP compliance, injection, auth, secrets
   - `performance-oracle` -- algorithmic complexity, N+1 queries, memory, caching
   - `architecture-strategist` -- pattern compliance, SOLID, coupling
   - `code-simplicity-reviewer` -- YAGNI, over-engineering, redundancy
   - `data-integrity-guardian` -- data consistency, migration safety
   - `pattern-recognition-specialist` -- pattern drift, inconsistency
   - `dhh-rails-reviewer` -- Rails-specific (DHH style)
   - `kieran-rails-reviewer` -- Rails-specific (Kieran's preferences)
   - `kieran-typescript-reviewer` -- TypeScript-specific
   - `kieran-python-reviewer` -- Python-specific
   - Plus conditional agents for migrations, backfills, etc.
4. Deep thinking phases: stakeholder perspectives (dev, ops, user, security, business) + scenario exploration (edge cases, failures, scale, attacks)
5. Synthesizes findings into prioritized todos:
   - **P1 (Critical):** Blocks merge -- security vulns, data corruption, breaking changes
   - **P2 (Important):** Should fix -- performance, architecture
   - **P3 (Nice-to-have):** Enhancements, cleanup
6. Generates file-based todos in `todos/` directory
7. Optional browser testing and Xcode testing post-review

**Key insight:** The massive parallelization of specialized agents is the signature move. Each agent has deep domain knowledge encoded in its prompt. This is how they encode "taste" into systems rather than relying on human reviewers remembering everything.

### Phase 4: Compound (`/workflows:compound`) -- 20% of time

**What it does:**
1. Five subagents analyze in parallel (return text only, no file creation):
   - Context Analyzer -- problem identification
   - Solution Extractor -- root cause and fix
   - Related Docs Finder -- cross-references
   - Prevention Strategist -- best practices to prevent recurrence
   - Category Classifier -- file organization
2. Orchestrator assembles a single markdown doc with YAML frontmatter
3. Writes to `docs/solutions/[category]/[filename].md`
4. Optional: specialized review agents run based on problem type

**Categories:** build-errors, test-failures, runtime-errors, performance-issues, database-issues, security-issues, ui-bugs, integration-issues, logic-errors.

**The compounding math:**
- First occurrence: 30 min research
- Documentation: 5 min
- All subsequent occurrences: 2 min (agent reads the doc)

**Key insight:** This is the "money step." Without it, you're just using AI to code. With it, each problem solved teaches the entire system. Future planning agents read `docs/solutions/` and incorporate past learnings automatically.

---

## 3. How They Handle Specs / Requirements

**Three-stage approach:**

1. **Brainstorm (`/workflows:brainstorm`)** -- Explores WHAT to build
   - Phase 0: Assess if brainstorming is even needed
   - Phase 1: Sequential Q&A (one question at a time, never batched)
   - Phase 2: 2-3 concrete approaches with pros/cons, YAGNI-biased
   - Phase 3: Document to `docs/brainstorms/YYYY-MM-DD-<topic>-brainstorm.md`
   - Phase 4: Handoff to planning or pause
   - Key constraint: "NEVER CODE!" during brainstorm

2. **Plan (`/workflows:plan`)** -- Converts WHAT into HOW
   - Research-grounded specification with acceptance criteria
   - Validated by `spec-flow-analyzer` for completeness and edge cases
   - Three detail levels based on complexity
   - Output is the primary artifact for execution

3. **Deepen (`/deepen-plan`)** -- Enriches plan with parallel research
   - Parses plan sections
   - Matches ALL available skills to plan content
   - Spawns 10-30+ parallel research agents per section
   - Launches ALL review agents (20-40+) for feedback
   - Synthesizes into "Research Insights" subsections
   - Additive only -- original content preserved

4. **Document Review (`document-review` skill)** -- Refines before handoff
   - Scores clarity, completeness, specificity, YAGNI
   - Identifies single most impactful improvement
   - Auto-fixes minor issues, requests approval for substantive changes
   - Recommends finalization after two iterations

**Key insight:** Specs are living documents that get progressively enriched. The `deepen-plan` command is particularly aggressive -- it will spawn 40+ agents to review a single plan. This is where the "plans as primary artifacts" philosophy manifests concretely.

---

## 4. Testing Approach

Testing is woven throughout rather than being a separate phase:

- **During Work:** Tests run continuously, not end-of-cycle. Agent writes tests alongside implementation.
- **During Review:** Each specialized agent checks testability. The `performance-oracle` enforces benchmarks. `security-sentinel` validates security test coverage.
- **Browser Testing:** `/test-browser` command uses agent-browser CLI (Vercel's headless browser) for E2E testing via accessibility snapshots.
- **Xcode Testing:** `/test-xcode` for iOS simulator testing.
- **Bug Reproduction:** `/reproduce-bug` command follows a 4-phase process: log investigation -> visual reproduction (Playwright) -> documentation -> GitHub issue creation.
- **Bug Validation:** `bug-reproduction-validator` agent systematically attempts reproduction with skepticism, classifies as Confirmed/Cannot Reproduce/Not a Bug/Environmental/Data Issue/User Error.

**Key insight from practitioners:** The testing pyramid inverts in compound engineering. E2E tests become central because they validate that the AI understood requirements holistically. Unit tests verify components; E2E tests verify comprehension.

---

## 5. Single Agent vs Multi-Agent: Orchestration Pattern

**Heavily multi-agent.** This is the most agent-dense system I've seen in the Claude Code ecosystem.

### Agent Inventory (29 total)

**Review agents (15):**
- security-sentinel, performance-oracle, architecture-strategist
- code-simplicity-reviewer, data-integrity-guardian, data-migration-expert
- deployment-verification-agent, dhh-rails-reviewer, julik-frontend-races-reviewer
- kieran-python-reviewer, kieran-rails-reviewer, kieran-typescript-reviewer
- pattern-recognition-specialist, schema-drift-detector, agent-native-reviewer

**Research agents (5):**
- best-practices-researcher, framework-docs-researcher
- git-history-analyzer, learnings-researcher, repo-research-analyst

**Workflow agents (5):**
- bug-reproduction-validator, every-style-editor, lint
- pr-comment-resolver, spec-flow-analyzer

**Design agents (3):**
- design-implementation-reviewer, design-iterator, figma-design-sync

**Docs agents (1):**
- ankane-readme-writer

### Orchestration Patterns

1. **Parallel specialists** -- Review phase launches 13+ agents simultaneously. Each examines the same code from a different perspective.

2. **Sequential pipeline** -- Brainstorm -> Plan -> Deepen -> Work -> Review -> Compound. Each phase must complete before the next begins.

3. **Self-organizing swarms** -- `orchestrating-swarms` skill documents TeammateTool-based coordination. Workers independently claim tasks from shared task pools.

4. **Research-then-implement** -- Planning agents gather info before execution agents run.

5. **Plan approval workflows** -- Gated implementation requiring human approval.

6. **Coordinated refactoring** -- Multi-file changes with dependency tracking via TaskCreate/TaskList/TaskUpdate.

### Autonomous Workflows

- **`/lfg`** -- "Let's F***ing Go" -- full autonomous pipeline: plan -> deepen -> work -> review -> resolve -> browser test -> feature video. 9 sequential steps, no human intervention between them.
- **`/slfg`** -- Same but with swarm mode for parallelism in work and review/testing phases.

### Spawn Backends

Three execution environments for multi-agent:
- **in-process** -- Same Node.js process, fastest, invisible
- **tmux** -- Separate terminal panes, visible, persistent
- **iterm2** -- iTerm2 split panes (macOS only)

---

## 6. Context Management

### Knowledge Storage Locations

| Location | Purpose | Read by |
|----------|---------|---------|
| `CLAUDE.md` | Agent instructions, preferences, patterns | All agents, every session |
| `docs/solutions/[category]/` | Documented problem resolutions with YAML frontmatter | Planning agents via `learnings-researcher` |
| `docs/plans/` | Planning outputs | Work agents, review agents |
| `docs/brainstorms/` | Feature exploration docs | Planning agents (14-day lookback) |
| `todos/` | Prioritized file-based work items | Work agents, review resolution |
| `docs/solutions/patterns/critical-patterns.md` | Must-know patterns across all work | Always read by `learnings-researcher` |

### Context Injection Strategies

1. **Just-in-time loading:** Skills are loaded only when relevant, not all upfront. Reduces token waste.
2. **YAML frontmatter on solutions:** Enables grep-based pre-filtering before reading full docs. Tags: title, category, module, symptom, root_cause, component.
3. **Grep-first, read-second:** `learnings-researcher` uses parallel grep to pre-filter candidates before reading any files. Critical for scaling to 100+ solution docs.
4. **Context7 MCP server:** Provides framework documentation lookup for 100+ frameworks without loading everything into context.
5. **Progressive disclosure in skills:** Main SKILL.md kept under 500 lines. Detailed content in `references/` subdirectories loaded on demand.

### Token Optimization

Changelog mentions "major context optimization reducing token consumption by 79%" in v2.31. They actively measure and reduce context overhead.

---

## 7. Human-in-the-Loop Model

### During Planning
- Human provides feature description or bug report
- Agent asks clarifying questions (one at a time, never batched)
- Human approves plan before execution
- Optional: human reviews deepened plan

### During Work
- Largely autonomous
- Agent asks clarifying questions if ambiguous
- Human can intervene but doesn't need to

### During Review
- Agent produces prioritized findings (P1/P2/P3)
- **P1 findings block merge** -- human must address critical issues
- Human decides priority of P2/P3 items
- Three review questions to ask agent output:
  1. "What was the hardest decision here?"
  2. "What alternatives did you reject, and why?"
  3. "What are you least confident about?"

### During Compound
- Largely automatic (agents document solutions)
- Human chooses post-documentation action (continue, promote to critical patterns, link related issues, create new skill)

### The Trust Escalation (Five Adoption Stages)
1. **Stage 0:** Manual development, no AI
2. **Stage 1:** Chat-based assistance (copy-paste snippets)
3. **Stage 2:** Agentic tools with line-by-line review
4. **Stage 3:** Plan-first, PR-only review (compound engineering begins)
5. **Stage 4:** Idea to PR automation
6. **Stage 5:** Parallel cloud execution

**Key insight:** The `/lfg` command represents Stage 4-5 -- fully autonomous from feature description to PR with video demo. The human reviews the PR, not individual lines of code.

---

## 8. Comparison to Other Frameworks

### vs Spec-Driven Development (Kiro, GitHub Spec Kit)

| Dimension | Compound Engineering | Spec-Driven Development |
|-----------|---------------------|------------------------|
| **Core artifact** | Plan docs + accumulated solutions | Formal specifications |
| **Feedback loop** | Compound step captures learnings | Spec remains static after implementation |
| **Agent count** | 29 specialized agents | Single agent (Kiro) or 1 per task (Spec Kit) |
| **Review** | Parallel multi-agent review | Manual or single-agent review |
| **Knowledge accumulation** | Explicit (docs/solutions/) | Implicit (in codebase only) |
| **Automation level** | Full pipeline (/lfg) | Plan-to-implementation |
| **Opinionated?** | Very (DHH style, Rails conventions) | Framework-agnostic |

Kiro enforces requirements -> design -> tasks (3 files). Spec Kit creates more markdown. Compound engineering creates a full learning loop that feeds back into future iterations.

### vs Traditional AI-Assisted Development (Cursor, Copilot)

| Dimension | Compound Engineering | Traditional AI-Assist |
|-----------|---------------------|----------------------|
| **Developer role** | Orchestrator | Code author with AI assist |
| **Knowledge persistence** | Explicit documentation | Session-only |
| **Planning** | Mandatory, research-grounded | Optional, ad-hoc |
| **Review** | Parallel multi-agent | Manual human review |
| **Learning loop** | Systematic compound step | None (start fresh each time) |

### vs Saurun (Our Plugin)

| Dimension | Compound Engineering | Saurun |
|-----------|---------------------|--------|
| **Workflow** | Rigid 4-phase loop | Flexible skills + commands |
| **Agent count** | 29 | Varies |
| **Opinionation** | High (Rails-centric, Every's conventions) | Lower (general-purpose) |
| **Knowledge system** | Built-in (docs/solutions/) | Not yet formalized |
| **Autonomous pipeline** | /lfg, /slfg | Not yet |

---

## 9. Unique / Novel Ideas

### 1. The Compound Step as First-Class Phase
Most frameworks stop at plan-work-review. The explicit "compound" phase that documents solutions with searchable YAML frontmatter, then feeds them back into future planning, is the key differentiator. Without it, you're doing AI-assisted development. With it, you're doing compound engineering.

### 2. Taste Encoded as Specialized Agents
Rather than a single reviewer, they have 15 review agents each encoding different perspectives and preferences. `dhh-rails-reviewer` encodes DHH's Rails opinions. `kieran-rails-reviewer` encodes Kieran's preferences. This is "taste in systems" made literal.

### 3. Plans as Primary Artifacts
The plan document is more important than the code. Code is disposable and regenerable. The plan represents the thinking and decisions. This is a genuine philosophical shift.

### 4. The Deepen Command
Spawning 40+ agents to enrich a single plan is aggressive. It's treating planning as the high-leverage activity and throwing compute at it.

### 5. Protected Compound Artifacts
Files in `docs/plans/` and `docs/solutions/` are explicitly protected during review. The `code-simplicity-reviewer` is instructed to never flag them for deletion. This prevents the system from consuming its own learning infrastructure.

### 6. Agent-Native Architecture Skill
The `agent-native-architecture` skill is a meta-skill -- it teaches how to build applications where agents are first-class citizens. Five principles: Parity, Granularity, Composability, Emergent Capability, Improvement Over Time. This is them generalizing their experience into architectural guidance.

### 7. File-Based Todos as Review Artifacts
Instead of TodoWrite (in-memory, session-scoped), they use file-based todos in `todos/` with structured naming: `{id}-{status}-{priority}-{description}.md`. These persist across sessions and track work logs, dependencies, and acceptance criteria.

### 8. Baby Apps for Design Iteration
Create throwaway prototypes ("baby apps") to iterate on design freely before transferring patterns to production. Separate exploration from commitment.

### 9. Cross-Platform Plugin Conversion
The `src/` directory contains a Bun/TypeScript CLI that converts Claude Code plugins to OpenCode, Codex, and Factory Droid formats. They're treating the plugin as a portable methodology, not a tool-locked workflow.

### 10. Git Worktree Integration
Using worktrees for isolated review and parallel work, with automated env file copying and cleanup. Enables true parallel development without repository duplication.

---

## 10. Evidence of Effectiveness

### Claims from Every Inc

- **5 products, 1 person each:** Every runs five software products in-house, each primarily built and run by a single person, serving thousands of daily active users.
- **7-figure revenue:** The company generates seven-figure annual revenue with a 15-person team.
- **100% AI-written code:** Engineers write virtually zero code manually.
- **1 engineer = 5 previous engineers:** A single developer does the work that previously required five.
- **300-700% velocity increase** vs traditional development (claimed by practitioners).

### External Validation

- **Will Larson (Irrational Exuberance):** Called it "an extremely effective way to convert intuited best-practices into something specific, concrete, and largely automatic." Set up in ~1 hour. Predicted the patterns will be absorbed into mainstream tools within months.
- **Soumitra Shukla (practitioner):** "Basically my go-to plan mode in CC now, I rarely use the regular plan mode these days."
- **Kieran Klaassen on Opus 4.6:** "Been running agent swarms for a few weeks now. Slower model, but the output quality unlocks" complex features in unexpected ways.
- **GitHub adoption:** 8.4k stars, 660 forks suggests significant community interest.

### Limitations / Criticisms

- **Identity threat:** Developers subconsciously see AI-assisted dev as an attack on their identity. Letting go of code-as-craft is psychologically difficult.
- **Upfront investment:** "You have to teach your tools before they can teach themselves." The compound step requires discipline.
- **Rails-centric:** Many agents are Rails-specific (DHH style, Kieran's Rails preferences). Less immediately applicable to other stacks without customization.
- **Token cost:** 40+ parallel agents per deepened plan = significant API spend. They've optimized (79% reduction), but it's still compute-intensive.
- **Will Larson's implicit critique:** The patterns aren't conceptually novel -- they formalize what good engineers already do intuitively. The value is in the systematization, not the ideas themselves.
- **Small company context:** Every is 15 people. Unclear how well the single-person-per-product model scales to larger teams or more complex systems.

---

## 11. Key Takeaways for Saurun

### What to steal

1. **The Compound Step.** We need a systematic knowledge capture mechanism. `docs/solutions/` with YAML frontmatter + grep-based retrieval is simple and effective.

2. **Specialized Review Agents.** Encoding domain-specific review criteria in dedicated agents. Not one big reviewer -- many focused reviewers running in parallel.

3. **File-Based Todos.** Persistent, structured todo files that survive across sessions. Better than in-memory TodoWrite for complex workflows.

4. **Plans as Primary Artifacts.** The plan document should be the thing we invest in. Code follows.

5. **Protected Knowledge Artifacts.** Explicitly protecting `docs/plans/` and `docs/solutions/` from deletion during review.

6. **The Deepen Pattern.** Throwing many agents at a plan to enrich it is high-leverage. Planning is the bottleneck, not execution.

7. **Progressive Context Loading.** Skills load references on demand, not all upfront. Grep-first, read-second for solution lookup.

### What to adapt, not copy

1. **The rigid 4-phase loop.** Their pipeline is very prescribed. We might want more flexibility in when/how phases execute.

2. **Rails-specific agents.** We need to generalize for .NET/TypeScript/multi-stack instead of encoding Rails opinions.

3. **The /lfg full-auto command.** Ambitious but risky. We should build toward this incrementally rather than shipping it as the first thing.

### What to avoid

1. **40+ agent spawns per plan.** Impressive but likely overkill for most use cases. Start with 3-5 targeted agents per phase.

2. **Over-opinionation.** Their style agents (DHH, Kieran) work for a small team with shared taste. We need more generic defaults.

---

## 12. Architecture Reference

```
plugins/compound-engineering/
  .claude-plugin/plugin.json          # v2.31.1, 29 agents, 24 commands, 18 skills, 1 MCP
  CLAUDE.md                           # Plugin development guidelines
  CHANGELOG.md                        # Version history
  README.md                           # Component catalog

  agents/
    design/                           # 3 agents: design-implementation-reviewer, design-iterator, figma-design-sync
    docs/                             # 1 agent: ankane-readme-writer
    research/                         # 5 agents: best-practices, framework-docs, git-history, learnings, repo-research
    review/                           # 15 agents: security, performance, architecture, simplicity, data, etc.
    workflow/                         # 5 agents: bug-reproduction, style-editor, lint, pr-comment, spec-flow

  commands/
    workflows/                        # Core 5: brainstorm, plan, work, review, compound
    lfg.md                            # Full autonomous pipeline
    slfg.md                           # Swarm-mode autonomous pipeline
    deepen-plan.md                    # Plan enrichment (40+ agents)
    technical_review.md               # Parallel architecture review
    resolve_parallel.md               # Parallel TODO resolution
    resolve_todo_parallel.md          # Parallel file-todo resolution
    reproduce-bug.md                  # Bug reproduction workflow
    report-bug.md                     # Bug reporting
    triage.md                         # Issue triage
    test-browser.md                   # Browser E2E testing
    test-xcode.md                     # Xcode testing
    feature-video.md                  # Feature demo video
    create-agent-skill.md             # Meta: create new skills
    heal-skill.md                     # Meta: fix broken skills
    changelog.md                      # Generate changelog
    deploy-docs.md                    # Deploy documentation
    release-docs.md                   # Release documentation
    agent-native-audit.md             # Architecture audit
    generate_command.md               # Meta: generate new commands

  skills/
    agent-browser/                    # Vercel headless browser CLI
    agent-native-architecture/        # Architecture principles (14 reference docs)
    andrew-kane-gem-writer/           # Ruby gem creation (5 reference docs)
    brainstorming/                    # Feature exploration
    compound-docs/                    # Knowledge capture (YAML schema, templates)
    create-agent-skills/              # Skill creation (14 references, 11 workflows, 2 templates)
    dhh-rails-style/                  # Rails conventions (6 reference docs)
    document-review/                  # Plan/brainstorm refinement
    dspy-ruby/                        # DSPy Ruby framework
    every-style-editor/               # Writing style guide
    file-todos/                       # File-based todo system
    frontend-design/                  # UI/design guidance
    gemini-imagegen/                  # Image generation (5 Python scripts)
    git-worktree/                     # Worktree management (1 shell script)
    orchestrating-swarms/             # Multi-agent coordination
    resolve-pr-parallel/              # PR comment resolution (2 scripts)
    rclone/                           # Remote file sync
    skill-creator/                    # Skill packaging (3 Python scripts)
```

---

## Sources

- https://github.com/EveryInc/compound-engineering-plugin
- https://every.to/chain-of-thought/compound-engineering-how-every-codes-with-agents
- https://every.to/source-code/compound-engineering-the-definitive-guide
- https://every.to/guides/compound-engineering
- https://lethain.com/everyinc-compound-engineering/
- https://creatoreconomy.so/p/how-to-make-claude-code-better-every-time-kieran-klaassen
- https://www.lennysnewsletter.com/p/inside-every-dan-shipper
- https://www.vincirufus.com/posts/compound-engineering/
- https://ai-assisted-software-development.com/compounding-engineering-loop/
