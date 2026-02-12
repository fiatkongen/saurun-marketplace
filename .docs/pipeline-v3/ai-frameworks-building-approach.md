# AI Agent-Based Software Engineering Frameworks: Research Findings

> Research date: 2026-02-11 | Covers frameworks and tools active as of early 2026

---

## Table of Contents

1. [Taxonomy & Overview](#taxonomy--overview)
2. [Spec-Driven Frameworks](#spec-driven-frameworks)
   - [GitHub Spec Kit](#github-spec-kit)
   - [Amazon Kiro](#amazon-kiro)
   - [Tessl](#tessl)
   - [JetBrains Junie](#jetbrains-junie)
   - [BMAD Method](#bmad-method)
3. [AI-First IDEs](#ai-first-ides)
   - [Cursor](#cursor)
   - [Windsurf (Codeium)](#windsurf-codeium)
4. [Terminal / CLI Agents](#terminal--cli-agents)
   - [Claude Code (Anthropic)](#claude-code-anthropic)
   - [OpenAI Codex](#openai-codex)
   - [Aider](#aider)
   - [Cline](#cline)
   - [Roo Code](#roo-code)
   - [Goose (Block)](#goose-block)
   - [Amp (Sourcegraph)](#amp-sourcegraph)
   - [Codebuff](#codebuff)
5. [Autonomous Agents / Cloud Agents](#autonomous-agents--cloud-agents)
   - [Devin (Cognition)](#devin-cognition)
   - [Factory.ai Droids](#factoryai-droids)
   - [OpenHands (formerly OpenDevin)](#openhands-formerly-opendevin)
   - [SWE-Agent](#swe-agent)
   - [Google Jules](#google-jules)
   - [GitHub Copilot Coding Agent](#github-copilot-coding-agent)
6. [Platform / Enterprise Agents](#platform--enterprise-agents)
   - [Augment Code](#augment-code)
   - [Qodo (formerly CodiumAI)](#qodo-formerly-codiumai)
   - [Amazon Q Developer](#amazon-q-developer)
   - [Tabnine](#tabnine)
7. [Generative App Builders](#generative-app-builders)
   - [Bolt.new (StackBlitz)](#boltnew-stackblitz)
   - [Lovable](#lovable)
   - [v0 (Vercel)](#v0-vercel)
   - [Replit](#replit)
8. [Cross-Cutting Analysis](#cross-cutting-analysis)
9. [Key Themes & Trends](#key-themes--trends)

---

## Taxonomy & Overview

Frameworks fall into distinct categories based on how they approach building software:

| Category | Philosophy | Human Role | Examples |
|----------|-----------|------------|----------|
| **Spec-Driven** | Write spec first, AI implements | Spec author, plan reviewer | Spec Kit, Kiro, Tessl, Junie, BMAD |
| **AI-First IDE** | AI deeply integrated into editor | Interactive co-pilot | Cursor, Windsurf |
| **Terminal/CLI Agent** | Agent in your terminal, edits your files | Prompt engineer, reviewer | Claude Code, Codex, Aider, Cline, Goose |
| **Autonomous Cloud Agent** | Full sandbox, async execution | Issue assigner, PR reviewer | Devin, Factory, Jules, SWE-Agent |
| **Enterprise Platform** | Org-wide context, multi-stage pipeline | Workflow designer | Augment, Qodo, Amazon Q, Tabnine |
| **Generative App Builder** | Prompt to full app, browser-based | Prompt describer | Bolt, Lovable, v0, Replit |

Martin Fowler's SDD taxonomy defines three levels that apply across all spec-driven tools:
- **Spec-First**: Spec written for a task, may be discarded after.
- **Spec-Anchored**: Spec is living documentation, changes start from spec.
- **Spec-as-Source**: Spec is the only artifact humans edit; code is compiled output.

---

## Spec-Driven Frameworks

### GitHub Spec Kit

**What it is**: Open-source toolkit (released Sep 2025) for Spec-Driven Development. Agent-agnostic -- works with Copilot, Claude Code, Gemini CLI, Cursor, Windsurf.

**Work Decomposition**: Three-phase sequential pipeline:
1. **Specify** (`spec.md`): Goals, constraints, user needs. The "what and why."
2. **Plan** (`plan.md`): Architecture, data flow, libraries, dependencies. The "how."
3. **Tasks** (`tasks/`): Self-contained units of work with exact file paths. Parallel tasks marked `[P]`. Tests ordered before implementation.

**Spec/Requirements Handling**: Spec-first (per Fowler's taxonomy). Specs live in `.specify/` directory. The Specify CLI scaffolds templates. Specs can evolve via discussion commands but are not auto-synced with code.

**Agent Model**: Single agent. Framework is agent-agnostic -- generates prompt files for whichever agent you use. Slash commands (`/specify`, `/plan`, `/tasks`) drive the workflow.

**Context Management**: Selective. The `.specify/` directory provides structured context. Agent-specific prompt files under `.github/` guide the agent to read specs before acting.

**Human-in-the-Loop**: Human writes/approves spec, reviews plan, then hands tasks to AI. High control at spec/plan level, delegated execution.

**Evidence**: Early adoption. GitHub's recommended approach for teams moving beyond "vibe coding." No published benchmarks yet.

---

### Amazon Kiro

**What it is**: VS Code fork (preview Jul 2025) with built-in spec-driven workflows. Part of AWS ecosystem.

**Work Decomposition**: Three-phase structured pipeline:
1. **User Stories + Acceptance Criteria**: Given/When/Then format. System generates structured stories from natural language requirements.
2. **Design Doc**: Technical design with diagrams, schemas, data models.
3. **Tasks**: Sequenced, trackable implementation tasks generated from the design.

**Spec/Requirements Handling**: Spec-anchored. Kiro specs stay synced with the evolving codebase -- developers can update code and ask Kiro to refresh specs, or update specs to regenerate tasks. Living documentation model.

**Agent Model**: Single integrated agent within the IDE. The agent handles all three phases and can execute tasks sequentially.

**Context Management**: Full project context via IDE integration. The agent reads the codebase to inform design decisions and task generation.

**Human-in-the-Loop**: Developer describes requirements in natural language, reviews generated stories/design/tasks at each phase, then approves execution. Iterative refinement at each stage.

**Evidence**: Positioned as antidote to vibe coding chaos. Enterprise-oriented with AWS integration. No public benchmark scores.

---

### Tessl

**What it is**: Agent enablement platform with a Spec Framework (closed beta) and Spec Registry (open beta, 10,000+ specs). Founded by former CircleCI leadership.

**Work Decomposition**: Spec-centric with annotations:
- `@generate`: Tells agent to generate code from spec
- `@describe`: Spec documents existing code (reverse engineering)
- `@test`: Capability described in natural language with linked test
- Spec structure: Description + Capabilities (with tests) + API

**Spec/Requirements Handling**: **Spec-as-source** (most aggressive SDD level). The spec is the primary artifact. Generated code is marked `// GENERATED FROM SPEC - DO NOT EDIT`. Humans edit specs, not code. The Tessl Framework alters agent behavior to avoid writing files directly, instead generating them from specifications.

**Agent Model**: Agent-agnostic enablement layer. Works with Claude Code, Cursor, etc. The framework modifies agent behavior rather than being an agent itself.

**Context Management**: The Spec Registry provides 10,000+ "Usage Specs" with version-accurate APIs and examples for external libraries, preventing API hallucinations. Specs are preprocessed and optimized for agent readability.

**Human-in-the-Loop**: Human maintains the spec. Code is a derived artifact. Tests validate generated code against spec.

**Evidence**: Boldest vision (spec-as-source). The registry addresses a real pain point (library hallucination). Martin Fowler's analysis identifies Tessl as the only tool exploring spec-as-source level.

---

### JetBrains Junie

**What it is**: AI coding agent integrated into JetBrains IDEs (IntelliJ, WebStorm, etc.). Leverages deep IDE integration.

**Work Decomposition**: Structured planning pipeline:
1. **Requirements** (`requirements.md`): Human-authored feature requirements
2. **Plan** (`plan.md`): Agent generates implementation plan with approach, sequence, risks
3. **Tasks**: Broken down from plan into executable units
4. **Guidelines** (`.junie/guidelines.md`): Persistent coding style, best practices, preferences

**Spec/Requirements Handling**: Spec-first. Human writes requirements, Junie generates plan and tasks. Two-column interface shows high-level direction alongside specific steps. Developer can stop, add hints, and redirect.

**Agent Model**: Single agent with deep IDE integration. Leverages JetBrains' code intelligence (inspections, refactorings, debugger). Can perform tasks independently or alongside developer.

**Context Management**: Full IDE context -- project structure, dependencies, build system, inspections, type information. Junie-guidelines repository provides technology-specific guideline catalogs.

**Human-in-the-Loop**: Plan review interface with two-column view. Developer can intervene mid-execution. Guidelines provide persistent behavioral constraints.

**Evidence**: 30% faster task completion claimed (without quality compromise). Deep IDE integration is a differentiator vs. terminal agents.

---

### BMAD Method

**What it is**: "Breakthrough Method for Agile AI-Driven Development." Open-source multi-agent framework with 21 specialized agents and 50+ guided workflows. Agent-agnostic (works with any LLM).

**Work Decomposition**: Four-phase lifecycle:
1. **Analysis**: Analyst agent refines raw ideas into Project Brief
2. **Planning**: Product Manager creates Functional Requirements and User Stories
3. **Solutioning**: Architect designs technical/visual blueprint
4. **Implementation**: Developer agent executes story-driven development; QA Agent verifies

**Spec/Requirements Handling**: Heavily spec-driven with role-based artifacts. Each phase produces formal documents that feed the next phase. Scale-adaptive (v6 Alpha) -- automatically adjusts planning depth.

**Agent Model**: **Multi-agent with 21 specialized roles** including Analyst, Product Manager, Architect, Developer, QA Agent, and others. Built on CORE (Collaboration Optimized Reflection Engine). BMad Builder allows creating custom agents.

**Context Management**: Artifacts from each phase serve as context for the next. Knowledge accumulates through the pipeline. Agents have distinct roles and outputs.

**Human-in-the-Loop**: Human guides at phase transitions. Each agent produces artifacts for human review before the next phase begins.

**Evidence**: Growing community adoption. The v6 Alpha with scale-adaptive planning suggests active iteration. More prescriptive than other frameworks.

---

## AI-First IDEs

### Cursor

**What it is**: VS Code fork rebuilt for AI-first development. Agent-first architecture since 2.0 (late 2025). Proprietary Composer model trained via RL.

**Work Decomposition**: Multi-modal:
- **Agent Mode**: Describe goal, agent plans and executes across multiple files. Autonomous subtask identification.
- **Composer**: Mixture-of-experts model for multi-file editing with aggregated diff view.
- **Inline Edit**: Targeted single-file changes.
- **Parallel Agents**: Up to 8 agents in parallel via git worktrees, each on isolated branch.

**Spec/Requirements Handling**: Prompt-driven, not spec-driven. Rules files (`.cursor/rules/*.mdc`) provide persistent project-level instructions. No formal spec pipeline -- developer prompts directly.

**Agent Model**: **Multi-agent capable**. Up to 8 parallel agents via git worktrees. Each agent operates in isolated copy of codebase. Composer 2.0 model trained specifically for agentic tool use (semantic search, file editing, terminal commands).

**Context Management**: Sophisticated RAG pipeline:
- **AST-based chunking** via tree-sitter for semantically coherent code units
- **Vector storage** on AWS S3 with Turbopuffer similarity search
- **Extended context** up to 272k tokens
- **Automatic augmentation**: current file, recent files, semantic search results, linter errors, edit history
- **Manual @-references** for targeted context injection
- **File path obfuscation** for privacy

**Human-in-the-Loop**: Interactive. Agent proposes changes as reviewable diffs. YOLO mode available for full autonomy. Browser tool for testing web apps. Developer reviews aggregated edit sets.

**Evidence**: Dominant market position in AI IDEs. No public SWE-bench scores, but Composer model trained on real codebases via RL. Used by a very large developer population.

---

### Windsurf (Codeium)

**What it is**: AI-native IDE, formerly Codeium. Acquired by Cognition (Devin) in Jul 2025. Built around Cascade AI engine. Gartner Magic Quadrant Leader for AI Coding Assistants (2025).

**Work Decomposition**: Cascade-driven with three modes:
- **Write Mode**: Direct code changes across project
- **Chat Mode**: Contextual help without altering code
- **Turbo Mode**: Fully autonomous task execution

**Spec/Requirements Handling**: Prompt-driven with `.windsurfrules` files for workflow rules. No formal spec pipeline. Auto-iterates until code works.

**Agent Model**: Single agent (Cascade) with multi-file reasoning and repository-scale comprehension. Multi-step task execution. Beginner-friendly auto-iteration loop.

**Context Management**: Deep context engine with repository-scale comprehension. Enterprise-grade context indexing. Cascade understands project structure across files.

**Human-in-the-Loop**: Write mode shows changes for review. Chat mode is advisory. Turbo mode is hands-off. Rules files constrain behavior.

**Evidence**: Gartner Leader 2025. Acquired by Cognition to combine with Devin (editor + agent vertical). Active development community.

---

## Terminal / CLI Agents

### Claude Code (Anthropic)

**What it is**: Agentic coding tool in the terminal. Runs Opus 4.6, Sonnet 4.5, Haiku 4.5 models. Plugin marketplace (Dec 2025). Skills specification adopted by VS Code, Copilot, Codex CLI, Cursor, and others.

**Work Decomposition**: Flexible -- developer-directed or autonomous:
- Single prompts for targeted changes
- Multi-step plans for complex features
- **Subagents**: Quick focused workers within a session
- **Agent Teams** (experimental): Multiple Claude Code instances with a lead coordinating teammates working independently

**Spec/Requirements Handling**: Prompt-driven with structured context:
- `CLAUDE.md`: Project-level instructions, conventions, preferences
- **Skills**: Reusable markdown-defined capabilities with frontmatter (`SKILL.md`)
- **Agents**: Named sub-agents defined in markdown with preloaded skills
- Supports integration with spec-driven workflows via skills/commands

**Agent Model**: **Single agent with subagent spawning + experimental multi-agent teams**. Subagents run within a session, report back to main agent. Agent Teams have independent context windows, can communicate peer-to-peer. Lead delegates tasks, teammates work in parallel.

**Context Management**:
- `CLAUDE.md` files at global/project/directory levels
- Skills inject metadata and prompts into conversation history
- MCP (Model Context Protocol) for external tool/data integration
- Teammates inherit `CLAUDE.md`, MCP servers, and skills but NOT conversation history
- Dynamic context via shell command injection in skills

**Human-in-the-Loop**: Interactive by default. Approve file edits, terminal commands. Configurable approval policies. Agent Teams shift to task-assignment model where lead coordinates.

**Evidence**: Skills spec became industry standard (adopted by Microsoft, OpenAI, Cursor, etc.). Plugin marketplace with 36+ curated plugins. Agent Teams shipped with Opus 4.6.

---

### OpenAI Codex

**What it is**: Cloud-based and CLI coding agent. Codex CLI (open-source, terminal) + Codex App (macOS, cloud sandboxes). Powered by codex-1 (o3-optimized for code) and later GPT-5.2-Codex.

**Work Decomposition**: Agent loop architecture:
1. Accept user input, construct prompt
2. Model generates response or tool call request
3. If tool call: execute, append output, query again
4. Loop until model produces final answer
- Codex App: Multiple parallel tasks, each in isolated cloud sandbox, up to 30 min autonomous execution

**Spec/Requirements Handling**: Prompt-driven with AGENTS.md support for repo-level instructions. MCP for extending with third-party tools. Can be orchestrated via Agents SDK (CLI as MCP server).

**Agent Model**: **Multi-agent capable**. Codex App runs multiple parallel agents. Each task gets its own cloud sandbox preloaded with repository. CLI is single-agent.

**Context Management**: Sandbox preloaded with repo. Default sandboxing limits file edits to working folder/branch. Cached web search available. AGENTS.md provides repo-level context.

**Human-in-the-Loop**: CLI: interactive with permission levels. App: delegate tasks, review PRs when done. Cloud sandbox provides isolation.

**Evidence**: codex-1 trained via RL on real coding challenges. SWE-bench competitive. Revenue-generating product. macOS app described as "command center for agents."

---

### Aider

**What it is**: Open-source terminal-based AI pair programming tool. Git-native with commit-per-change workflow. Model-agnostic (works with Claude, GPT, DeepSeek, local models).

**Work Decomposition**: Single-pass pair programming model:
- **Code mode**: Direct editing via search/replace blocks
- **Architect mode**: Two-step -- architect model reasons about changes in plain text, editor model applies syntactically correct diffs
- Edit formats: whole, diff, unified diff, editor-diff, editor-whole

**Spec/Requirements Handling**: Prompt-only. No formal spec pipeline. Human describes changes conversationally. Git history serves as implicit documentation.

**Agent Model**: **Single agent** (currently). Community proposals for multi-agent support exist but aren't implemented. Architect mode separates reasoning from editing but uses a single session.

**Context Management**: **Repo map via tree-sitter**:
- AST parsing extracts symbol definitions across codebase
- Dependency graph built (files as nodes, references as edges)
- **PageRank algorithm** ranks files by relevance
- Only most relevant portions sent to LLM
- 4.3-6.5% context utilization (most efficient among peers)
- Configurable `map-tokens` setting

**Human-in-the-Loop**: Highly interactive. Conversational back-and-forth. Every change committed to git with descriptive message. Human reviews diffs in real time.

**Evidence**: Consistently top-ranked on SWE-bench Lite among open-source tools. Described as most reliable for structured refactors. Pioneered the repo-map-via-tree-sitter approach now studied in academic research.

---

### Cline

**What it is**: Autonomous AI coding agent for VS Code. Open-source, model-agnostic. 5M+ developers. Human-in-the-loop by design.

**Work Decomposition**: Plan/Act pipeline:
- **Plan mode**: Agent devises sequence of steps
- **Act mode**: Executes steps one-by-one
- Supports file editing, terminal commands, browser automation, MCP tools

**Spec/Requirements Handling**: Prompt-driven with `.clinerules` for project-level configuration (directory permissions, tool restrictions, approval requirements).

**Agent Model**: Single agent with rich tool access. Can be extended via MCP. No native multi-agent but extensible.

**Context Management**: Full project context via VS Code workspace. Model-agnostic (OpenAI, Anthropic, Google, Ollama). All actions logged with timestamps for auditability.

**Human-in-the-Loop**: **Central design principle**. Agent proposes, human approves. File edits and terminal commands appear as diffs/actions before execution. Auto-approval rules available for trusted operations. Enterprise `.clinerules` for granular permission control.

**Evidence**: 5M+ developers. Trusted in production repos due to traceability. Consistently recommended in 2025-2026 developer forums.

---

### Roo Code

**What it is**: Open-source VS Code extension. Fork/evolution from Cline lineage. Multi-mode architecture with "sticky models."

**Work Decomposition**: Mode-based:
- **Architect mode**: High-reasoning model for planning
- **Code mode**: Faster model for implementation
- **Debug mode**: Error analysis and fixing
- **Ask mode**: Q&A about codebase
- **Custom modes**: User-defined configurations

**Spec/Requirements Handling**: Prompt-driven. Each mode preserves model preferences ("sticky models"). No formal spec pipeline.

**Agent Model**: Single agent with mode switching. Model-agnostic. Supports local and external AI backends.

**Context Management**: VS Code workspace context. Shell commands, builds, browser-based validation for runtime feedback loop.

**Human-in-the-Loop**: Nothing runs without approval unless explicitly enabled with auto-approval rules. More reliable on large multi-file changes (at cost of speed).

**Evidence**: Strong reputation for reliability on large changes. 27k+ GitHub stars for the broader Cline/Roo ecosystem.

---

### Goose (Block)

**What it is**: Open-source (Apache 2.0) AI agent from Block (Square/Cash App). Contributed to Linux Foundation Agentic AI Foundation (Dec 2025). 27k+ GitHub stars.

**Work Decomposition**: Autonomous project-level execution. Can build entire projects, write/execute code, debug, orchestrate workflows, interact with external APIs.

**Spec/Requirements Handling**: Prompt-driven. AGENTS.md support. MCP-native architecture for tool integration.

**Agent Model**: Single agent, heavily MCP-based. Extensible via any MCP server. Model-agnostic (any LLM, multi-model configuration).

**Context Management**: MCP-based context aggregation from external tools and data sources. Desktop app + CLI.

**Human-in-the-Loop**: Interactive with approval flow. Open and modular design prioritizes developer control.

**Evidence**: 27k stars, 350+ contributors, 100+ releases in one year. Linux Foundation backing. MCP-native design is forward-looking.

---

### Amp (Sourcegraph)

**What it is**: AI coding agent built by Sourcegraph. Leverages Sourcegraph's code graph and search infrastructure. IDE-agnostic (VS Code, JetBrains, Neovim, terminal).

**Work Decomposition**: Prompt-driven with deep code intelligence. Agent bridges between developer requests and codebase understanding via Sourcegraph's global code graph.

**Spec/Requirements Handling**: Prompt-driven. Shared threads, context, and workflows across team for reuse.

**Agent Model**: Single agent backed by enterprise code graph. Context aggregation (symbols, references, dependency trees), graph querying, dynamic updates as code evolves.

**Context Management**: **Sourcegraph's code graph** -- semantic mapping of relationships across repositories. Not just token/grep matching. Dynamic refresh as code changes. Enterprise-scale codebase support.

**Human-in-the-Loop**: Interactive. Team-oriented with shared workflows and context.

**Evidence**: Built on Sourcegraph's proven code intelligence platform. Enterprise-grade. Trusted by large engineering organizations using Sourcegraph.

---

### Codebuff

**What it is**: Open-source CLI coding agent. Model-agnostic via OpenRouter. Multi-agent architecture internally.

**Work Decomposition**: Multi-agent internal pipeline:
- File Explorer Agent scans codebase architecture
- Planner Agent plans file changes and ordering
- Coordinated execution

**Spec/Requirements Handling**: Prompt-driven. Custom agent workflows via TypeScript generators.

**Agent Model**: **Internal multi-agent** with specialized sub-agents (explorer, planner). Can spawn sub-agents with conditional logic.

**Context Management**: Deep codebase parsing. Understands structure, dependencies, patterns.

**Human-in-the-Loop**: Interactive terminal workflow.

**Evidence**: Claims 61% vs Claude Code's 53% on 175+ real-world coding task evals. Open-source, growing community.

---

## Autonomous Agents / Cloud Agents

### Devin (Cognition)

**What it is**: First fully autonomous AI software engineer (Mar 2024). $10.2B valuation (late 2025). Acquired Windsurf (Jul 2025). $155M+ ARR.

**Work Decomposition**: Agentic loop:
1. Decompose goal into steps
2. Search/read documentation
3. Edit code, run commands/tests
4. Analyze failures, iterate
5. Repeat until stopping condition
- Multi-agent: dispatch tasks to other Devin instances
- Self-assessed confidence evaluation -- asks for clarification when uncertain

**Spec/Requirements Handling**: Prompt-driven with structured knowledge:
- **Knowledge system**: Tips, instructions, org context that Devin recalls automatically
- **Playbooks**: Reusable prompt templates for recurring scenarios (steps + success criteria + guardrails)
- **Enterprise Knowledge**: Org-wide shared context
- **DeepWiki**: Auto-generated documentation for GitHub repos

**Agent Model**: **Multi-agent**. Can dispatch tasks to other Devin instances. Each instance gets own cloud IDE (shell, browser, editor). Parallel execution across multiple Devin sessions.

**Context Management**: Own command line, browser, code editor. Knowledge management system with automatic recall. Enterprise-wide knowledge sharing. DeepWiki for repo understanding.

**Human-in-the-Loop**: Async. Human assigns task, Devin works autonomously. Can intervene/steer mid-session. Confidence-based clarification requests. Advanced Mode with session analysis.

**Evidence**: Deployed at Goldman Sachs, Santander, Nubank. $1M to $155M ARR in 18 months. Working in thousands of companies. SWE-bench competitive.

---

### Factory.ai Droids

**What it is**: Agent-native software development platform. Specialized Droids for different SDLC stages. $50M+ funding.

**Work Decomposition**: **Model Predictive Control (MPC) + subtask decomposition**:
1. Receive high-level problem
2. Decompose into subtasks (frontend, backend, tests, rollout)
3. Translate subtasks into action space
4. Reason about optimal trajectories
5. Continuously update plans based on environmental feedback
6. Predefined templates for known task types (e.g., "this will result in a PR")

**Spec/Requirements Handling**: Task-driven. Integrates with Linear, Notion, Jira for requirements. Predefined templates optimize known workflow patterns.

**Agent Model**: **Single orchestrated agent** with sophisticated planning. Droids for feature development, migrations, code review, testing, incident response. Not multi-agent in the peer sense, but specialized configurations.

**Context Management**: Environmental grounding via AI-computer interfaces that interact with dev tools, observability systems, knowledge bases. Integrates with GitHub, Slack, Linear, Notion, Sentry.

**Human-in-the-Loop**: Delegate complete tasks. Works in any IDE/terminal. Results delivered as PRs. TDD-based verification.

**Evidence**: #1 on Terminal-Bench (58.75%). Used by MongoDB, Ernst & Young, Zapier, Bayer. 200% QoQ growth through 2025.

---

### OpenHands (formerly OpenDevin)

**What it is**: Open-source platform for AI software developers as generalist agents. 64k+ GitHub stars. Academic origin with broad community.

**Work Decomposition**: Event-stream based:
- Agent receives task
- Plans and executes via tools (bash shell, web browser, IPython, code editor)
- Actions and observations tracked in event stream
- Hierarchical agent structures with delegation primitives

**Spec/Requirements Handling**: Task/issue driven. No formal spec pipeline. CodeAct architecture for code-based actions.

**Agent Model**: **Multi-agent with hierarchical delegation**. Agent hub with 10+ agent implementations. Primary: CodeAct-based generalist with web browsing and code editing specialists. Standardized vocabulary for agent roles. V1 SDK refactors into modular packages.

**Context Management**: Docker-sandboxed runtime with bash, browser, IPython. Event stream tracks full history. V1 introduces opt-in sandboxing and reusable tool/workspace packages.

**Human-in-the-Loop**: Configurable. Can be fully autonomous or interactive. Web UI for monitoring.

**Evidence**: 15+ benchmark support (SWE-bench, HumanEvalFix, ML-Bench, WebArena, MiniWoB++). 64k stars, hundreds of contributors. Academic papers at top venues.

---

### SWE-Agent

**What it is**: Academic project from Princeton/Stanford (NeurIPS 2024). Pioneered Agent-Computer Interface (ACI) concept. Open-source research tool.

**Work Decomposition**: Single-pass issue resolution:
1. Receive GitHub issue
2. Navigate repository using custom ACI commands
3. Locate relevant code
4. Edit and test
5. Submit patch

**Spec/Requirements Handling**: Issue-driven. Takes a GitHub issue as input, produces a patch.

**Agent Model**: **Single agent** with custom ACI. LM-centric commands for browsing repos, viewing/editing/executing code. SWE-agent 1.0 uses SWE-ReX for deployment (local Docker or remote AWS/Modal).

**Context Management**: ACI design is the key innovation -- custom commands and feedback formats optimized for LLM interaction. Not raw bash, but structured interfaces.

**Human-in-the-Loop**: Minimal. Fully autonomous from issue to patch. Research-oriented.

**Evidence**: 12.29% on SWE-bench (full test set) when introduced. Spawned extensive research (SWE-Search with MCTS, moatless-tools). Foundational to the field.

---

### Google Jules

**What it is**: Autonomous coding agent by Google. Runs in cloud VMs. Available as standalone (jules.google) and as Gemini CLI extension.

**Work Decomposition**: Plan-then-execute:
1. User describes task
2. Jules generates plan
3. User approves plan
4. Jules executes autonomously in VM (clones code, installs deps, modifies files)
- Designed for "scoped tasks" -- less interactive than Gemini CLI by design

**Spec/Requirements Handling**: Prompt-driven with plan approval. Jules Tools for scoped tasks. Gemini CLI for more iterative collaboration.

**Agent Model**: Single autonomous agent per task. VM-isolated. Now powered by Gemini 3 Pro.

**Context Management**: Full repository clone in VM. Installs dependencies. Can run tests. Isolated execution environment.

**Human-in-the-Loop**: Approve plan, then hands-off. Less interactive by design. Review results when done.

**Evidence**: Gemini 3 Pro integration (Feb 2026) claimed improved agentic reliability. Part of broader Google Antigravity agent-first platform vision.

---

### GitHub Copilot Coding Agent

**What it is**: Autonomous coding agent integrated into GitHub platform. GA for all paid Copilot subscribers. Evolved from Copilot Workspace and Project Padawan.

**Work Decomposition**: Issue-to-PR pipeline:
1. Assign GitHub issue to Copilot (or prompt via Chat/VS Code)
2. Agent works in GitHub Actions environment
3. Identifies subtasks, executes across files
4. Creates PR with results
- Agent HQ: Multi-agent dashboard with Claude Code + Codex alongside Copilot

**Spec/Requirements Handling**: Issue-driven. GitHub Issues as input. Custom instructions storable in editor. AGENTS.md support.

**Agent Model**: Single agent per task, but **Agent HQ enables multi-agent orchestration** across Copilot, Claude Code, and Codex. Each agent can work asynchronously.

**Context Management**: Full repo in GitHub Actions environment. MCP support for external tools. Copilot SDK for embedding agent capabilities.

**Human-in-the-Loop**: Async delegation model. Assign issue, come back to review PR. Can work in VS Code for more interactive mode.

**Evidence**: GA status. Millions of Copilot users. Part of GitHub's complete platform play. Copilot SDK enables building custom agents.

---

## Platform / Enterprise Agents

### Augment Code

**What it is**: Enterprise AI coding platform with deep context engine. 200k-token codebase-wide context window. Recently opened semantic coding capability as API for any agent.

**Work Decomposition**: Four-phase methodology:
1. **Specify**: Define requirements
2. **Plan**: Technical approach
3. **Tasks**: Break down into units
4. **Implement**: Agent executes

**Spec/Requirements Handling**: Supports spec-driven workflows. Four-phase methodology resembles Spec Kit.

**Agent Model**: Single autonomous agent. Recent API opening allows other agents to use Augment's semantic coding engine.

**Context Management**: **Semantic context engine** -- not token/grep matching but semantic relationship mapping across project. 200k-token window. Largest context among competitors.

**Human-in-the-Loop**: Interactive with autonomous workflows. Enterprise controls.

**Evidence**: Claims 56% programming time reduction, 30-40% faster time-to-market. Enterprise customers report 60% cycle time reduction.

---

### Qodo (formerly CodiumAI)

**What it is**: Agentic code integrity platform. Five specialized agents across the SDLC. Enterprise-focused. Gartner-recognized.

**Work Decomposition**: **Five specialized agents**:
1. **Qodo Gen**: Test generation and agentic coding (IDE)
2. **Qodo Merge**: PR code review
3. **Qodo Cover**: Coverage analysis
4. **Qodo Aware**: Deep research / codebase understanding
5. **Qodo Command**: Workflow automation

**Spec/Requirements Handling**: Quality-driven rather than spec-driven. Focus on code integrity -- tests, reviews, coverage as specifications of correctness.

**Agent Model**: **Multi-agent with role specialization**. Each agent handles a different SDLC stage. Agents dynamically gather context and execute multi-step problem-solving.

**Context Management**: System-aware agents that understand contracts, dependencies, production impact. IDE integration (JetBrains, VS Code) + CI/CD pipeline integration.

**Human-in-the-Loop**: Varies by agent. Gen is interactive in IDE. Merge is automated with PR integration. Enterprise controls.

**Evidence**: Gartner recognized as top-funded startup in domain-specialized agentic AI for code review. Established user base from CodiumAI days.

---

### Amazon Q Developer

**What it is**: AWS generative AI assistant for software development. Covers entire SDLC with agentic capabilities. Deep AWS integration.

**Work Decomposition**: Issue-to-PR with multi-step planning:
1. Developer describes feature
2. Agent analyzes existing codebase
3. Maps out step-by-step implementation plan
4. Upon approval, executes code changes and tests

**Spec/Requirements Handling**: Task-driven via natural language, GitHub Issues, or direct prompts. CLI supports MCP for additional context sources.

**Agent Model**: Single agent with broad tool access. Can be assigned GitHub issues for autonomous implementation.

**Context Management**: Full codebase analysis. MCP support in CLI for external data sources. Broad language support (15+ languages including Terraform, CloudFormation).

**Human-in-the-Loop**: Plan approval before execution. PR-based delivery for review. Automated code review on PRs.

**Evidence**: SWE-bench Verified 66% (top ranking). SWTBench Verified 49% (state-of-the-art at time of measurement). 12x adoption surge at Epsilon in 2025.

---

### Tabnine

**What it is**: Enterprise AI coding platform with privacy-first approach. Org-Native Agents. Air-gapped deployment options.

**Work Decomposition**: Multi-agent across SDLC stages -- planning, code creation, testing, documentation. Agents handle refactoring, debugging, documentation workflows.

**Spec/Requirements Handling**: Task-driven with organizational context. Enterprise Knowledge for org-wide standards.

**Agent Model**: **Org-Native Agents** with enterprise context grounding. Combines vector, graph, and agentic retrieval techniques.

**Context Management**: **Enterprise Context Engine** -- learns organizational architecture, frameworks, coding standards. Live organizational context (not static training data). Auto-adapts to new codebases without retraining.

**Human-in-the-Loop**: Enterprise controls. Flexible deployment (SaaS, VPC, on-premises, air-gapped).

**Evidence**: Dell partnership for air-gapped GPU-accelerated deployment. Finance, defense, healthcare customers. Zero data retention, no training on customer code.

---

## Generative App Builders

### Bolt.new (StackBlitz)

**What it is**: Browser-based development environment using WebContainer technology. Creates full projects from chat. No local setup required.

**Work Decomposition**: **Single-pass full-project generation**:
- Creates project structure
- Installs dependencies
- Writes backend logic
- Configures database
- Deploys
- All from chat in browser

**Spec/Requirements Handling**: Prompt-only. Describe what you want, get a running app. Iterative refinement via conversation.

**Agent Model**: Single agent. Powered primarily by Claude Sonnet.

**Context Management**: Full project context within WebContainer. Live preview.

**Human-in-the-Loop**: Prompt and iterate. Live preview for validation. Deploy when satisfied.

**Evidence**: Popular for hackathons, demos, quick prototypes. WebContainer technology is unique differentiator (full dev environment in browser).

---

### Lovable

**What it is**: Full-stack web application generator. Founded by GPT Engineer creators. $20M ARR in 2 months (fastest European startup growth).

**Work Decomposition**: Single-pass full-stack generation:
- UI, backend, database schema, auth, deployment from natural language
- Single browser tab experience

**Spec/Requirements Handling**: Prompt-only. Natural language to full-stack app. Iterative refinement.

**Agent Model**: Single agent. Focus on speed to working app.

**Context Management**: Full generated project context. Built-in deployment.

**Human-in-the-Loop**: Describe app, review generated result, iterate.

**Evidence**: $20M ARR in 2 months. Fastest-growing European startup. Fastest full-stack MVP delivery.

---

### v0 (Vercel)

**What it is**: Vercel's generative UI platform. Opinionated toward React/Next.js/shadcn/ui ecosystem. Evolved from component generation to full backend support.

**Work Decomposition**: Component-oriented generation:
- UI component generation via natural language
- Backend services, database integration, API routes (newer capability)
- Tight Next.js ecosystem integration

**Spec/Requirements Handling**: Prompt-only. Opinionated tech stack.

**Agent Model**: Single agent optimized for React ecosystem.

**Context Management**: Within generated project. Leverages shadcn/ui component library.

**Human-in-the-Loop**: Prompt, review, iterate. Deploy to Vercel.

**Evidence**: Dominant in React component generation. Vercel ecosystem lock-in is both strength and limitation.

---

### Replit

**What it is**: Cloud IDE with integrated AI (Ghostwriter + Agent 3). Entire development lifecycle in browser.

**Work Decomposition**: Agent 3 (Sep 2025):
- Builds entire apps from natural language
- Writes tests, debugs own code
- Works autonomously for up to 200 minutes
- **Can spawn new agents** for sub-workflows
- 2-3x speed improvements through 2025

**Spec/Requirements Handling**: Prompt-only. Describe desired workflow; Agent 3 generates specialized agent to handle it.

**Agent Model**: **Agent-spawning**. Agent 3 can create other specialized agents that integrate with Slack, email, Telegram. Recursive agent creation.

**Context Management**: Full Replit environment. Instant dev environments. Design Mode (Nov 2025) for visual feedback.

**Human-in-the-Loop**: Describe and delegate. Agent works autonomously for extended periods. Return to review.

**Evidence**: Massive scale (millions of users). Agent 3 represents significant autonomous capability. 200-minute autonomous sessions.

---

## Cross-Cutting Analysis

### Work Decomposition Patterns

| Pattern | Description | Used By |
|---------|------------|---------|
| **Spec-Plan-Tasks** | Formal 3-phase pipeline | Spec Kit, Kiro, Junie, Augment |
| **Plan-Act Loop** | Agent plans then executes | Cline, Roo Code, Jules |
| **Agent Loop** | Tool-call loop until done | Codex, Claude Code, Devin |
| **MPC + Subtask** | Continuous plan updating | Factory Droids |
| **Mode Switching** | Different modes for different work | Cursor, Roo Code, Windsurf |
| **Single-Pass Generation** | Prompt to running app | Bolt, Lovable, v0 |
| **Multi-Agent Pipeline** | Specialized agents per phase | BMAD, Qodo, OpenHands |
| **Spec-as-Source** | Code compiled from spec | Tessl |

### Context Management Strategies

| Strategy | Description | Used By |
|----------|------------|---------|
| **RAG + Vector Search** | Embed/chunk code, similarity search | Cursor, Augment |
| **Repo Map + PageRank** | AST parsing + graph ranking | Aider |
| **Code Graph** | Semantic relationship mapping | Amp (Sourcegraph) |
| **Enterprise Context Engine** | Org-wide learning | Tabnine, Augment |
| **Sandbox + Full Repo** | Clone repo into isolated env | Codex, Jules, Devin |
| **MCP Integration** | External tools via protocol | Claude Code, Goose, Codex |
| **Spec Registry** | Pre-processed library specs | Tessl |
| **Event Stream** | Action/observation history | OpenHands |

### Human-in-the-Loop Models

| Model | Control Level | Used By |
|-------|--------------|---------|
| **Spec Author** | High (define what to build) | Spec Kit, Kiro, Tessl |
| **Plan Reviewer** | Medium-High (approve approach) | Junie, Jules, Factory |
| **Interactive Co-Pilot** | Medium (approve each action) | Cline, Aider, Claude Code |
| **Async Delegator** | Low (assign, review later) | Devin, Copilot Agent, Codex App |
| **Prompt-and-Iterate** | Low (describe, refine result) | Bolt, Lovable, v0, Replit |

### Multi-Agent Approaches

| Approach | Description | Used By |
|----------|------------|---------|
| **No Multi-Agent** | Single agent, single loop | Aider, Cline, Windsurf |
| **Internal Sub-Agents** | Specialized workers within one session | Codebuff, Claude Code (subagents) |
| **Parallel Isolated Agents** | Multiple agents on git worktrees | Cursor (8 parallel) |
| **Peer Agent Teams** | Independent agents that communicate | Claude Code (Agent Teams) |
| **Role-Based Agents** | Different agents for different roles | BMAD (21 agents), Qodo (5 agents) |
| **Hierarchical Delegation** | Parent agents delegate to children | OpenHands, Devin |
| **Agent Spawning** | Agents that create other agents | Replit Agent 3 |

---

## Key Themes & Trends

### 1. The Spec-Driven Movement is Real but Fragmented
Three distinct levels (spec-first, spec-anchored, spec-as-source) with Tessl pushing the boundary. Most tools are spec-first at best. The industry hasn't converged on a standard spec format.

### 2. Context Engineering is the Differentiator
The best tools don't just have smarter models -- they provide better context. Aider's PageRank repo map, Augment's semantic engine, Sourcegraph's code graph, and Tabnine's enterprise context engine represent fundamentally different approaches to the same problem.

### 3. Multi-Agent is Evolving from Hype to Practice
Early multi-agent (BMAD's 21 agents) is prescriptive and role-based. Newer approaches (Claude Code Agent Teams, Cursor parallel agents) are more fluid. The trend is toward agent teams that coordinate rather than rigid pipelines.

### 4. Async Delegation is the Enterprise Model
The shift from interactive co-pilot to async delegation (Devin, Copilot Agent, Codex App, Jules) represents the most significant architectural change. Developers become managers of AI agents.

### 5. The IDE vs Terminal vs Cloud Debate is Collapsing
Cognition's acquisition of Windsurf, GitHub's Agent HQ (Copilot + Claude Code + Codex), and Cursor's parallel agents all point toward convergence. The future is multi-surface: IDE for interactive work, terminal for power users, cloud for async delegation.

### 6. MCP is Becoming the Universal Connector
Model Context Protocol (MCP) adoption across Claude Code, Goose, Codex, Amazon Q, and others is creating a standard for tool integration. MCP and AGENTS.md both contributed to Linux Foundation's Agentic AI Foundation (Dec 2025).

### 7. Generative App Builders are a Separate Category
Bolt/Lovable/v0/Replit solve a different problem (rapid prototyping) than enterprise coding agents. Their single-pass generation model doesn't scale to large codebases, but excels for greenfield projects and MVPs.

### 8. Benchmarks are Proliferating but Incomplete
SWE-bench remains dominant but is being supplemented by Terminal-Bench, SWE-Bench Pro, and others. Amazon Q (66% SWE-bench Verified), Factory Droids (58.75% Terminal-Bench), and SWE-agent foundational work drive the field. Real-world effectiveness remains hard to measure.

### 9. The Skills/Plugin Ecosystem is Emerging
Claude Code's Skills spec became an industry standard adopted by Microsoft, OpenAI, Cursor, and others. Tessl's Spec Registry, GitHub's AGENTS.md, and Cursor's rules files all represent different approaches to making agents customizable and shareable.

### 10. Privacy and Enterprise Control are Non-Negotiable
Tabnine's air-gapped deployment, Cursor's path obfuscation, and enterprise features across all major platforms reflect that enterprises demand control over where code goes. This is not just a feature -- it's table stakes.
