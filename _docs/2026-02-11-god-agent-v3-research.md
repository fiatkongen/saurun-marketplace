# God-Agent v3 Research: Evidence-Based Design Foundations

**Date:** 2026-02-11
**Status:** Research complete. Design pending.
**Purpose:** Capture all research findings that should inform the next god-agent redesign. This document replaces intuition with evidence.

---

## Background: Why v2.0.0 TDD Redesign Was Questioned

The v2.0.0 redesign (context-isolated three-agent TDD loop) was based on:
- One blog post (alexop.dev) whose "84% improvement" measured **skill activation**, not code quality
- The intuition that TDD discipline improves AI-generated code
- No empirical evidence that context isolation improves output

During implementation review, we discovered research that fundamentally challenges these assumptions.

---

## Part 1: Does TDD Help AI Agents?

### Key Paper: "Rethinking the Value of Agent-Generated Tests" (Feb 2026)

**Source:** [arxiv.org/html/2602.07900v1](https://arxiv.org/html/2602.07900v1)
**Evidence strength:** HIGH (empirical, 6 SOTA models, SWE-bench Verified, controlled experiments)

**Findings:**
- GPT-5.2 writes almost NO tests (0.6% of tasks) → 71.8% resolution
- Claude Opus 4.5 writes tests in 83% of tasks → 74.4% resolution
- Only 2.6 percentage points difference
- Encouraging low-test models to write tests → costs up (+19.8% tokens), resolution unchanged
- Suppressing high-test models from writing tests → tokens down 32-49%, resolution down only 1.8-2.6%
- 83.2% of tasks maintained same outcome regardless of test-writing intervention
- Agent-written tests are mostly **observational** (print statements), not **verification** (assertions)

**Conclusion:** "Agent-written tests often behave more like a reproduced software development lifecycle routine than a dependable source of help."

**Implication for god-agent:** Making the agent write tests first does NOT improve the code it produces. The benefit of tests is for HUMANS who maintain the code later, not for the agent itself.

### Key Paper: "TDD for Code Generation" (ASE 2024)

**Source:** [arxiv.org/abs/2402.13521](https://arxiv.org/abs/2402.13521)
**Evidence strength:** HIGH (empirical, GPT-4 + Llama 3, HumanEval/MBPP/CodeChef)

**Findings:**
- Tests provided AS INPUT improve code generation by 8-18%
- GPT-4 on CodeChef: 35.72% (prompt only) → 43.44% (+tests) → 52.8% (+iterative remediation)
- Tests alongside problem statements consistently improve outcomes

**Critical distinction:** Tests as INPUT (specification) help. Tests the agent writes for ITSELF don't measurably help.

**Implication for god-agent:** Use tests/acceptance criteria as PART OF THE SPEC given to the implementer. Don't force the implementer to write tests before code.

### Key Paper: "Tests as Prompt" (WebApp1K, May 2025)

**Source:** [arxiv.org/abs/2505.09027](https://arxiv.org/abs/2505.09027)
**Evidence strength:** HIGH (empirical, 19 models, 1000 challenges, 20 domains)

**Findings:**
- Instruction following and in-context learning are MORE IMPORTANT than coding ability for TDD success
- Top models: o1-preview 95.2%, o1-mini 93.9%, DeepSeek-R1 92.7%, GPT-4o 88.5%, Claude 3.5 Sonnet 88.08%
- When task complexity doubled (single → dual feature): o1-preview dropped from 95% to 65.2%
- "Instruction loss in long prompts" — models stop attending to some requirements
- 7 consistent error types across ALL models: version mismatches, text mismatches, API call discrepancies

**Implication for god-agent:** Keep specs SHORT and FOCUSED. Doubling feature scope in one prompt causes 30% performance drop. Decompose.

### alexop.dev — The Original Inspiration

**Source:** [alexop.dev/posts/custom-tdd-workflow-claude-code-vue/](https://alexop.dev/posts/custom-tdd-workflow-claude-code-vue/)
**Evidence strength:** LOW (single practitioner, no quality metrics)

**What was actually measured:** Skill activation rates improved from ~20% to ~84% via UserPromptSubmit hooks.

**What was NOT measured:** Code quality, test coverage, bug rates, TDD compliance, output comparison. Zero quality metrics.

**Implication:** The hook-based skill activation finding is valid. The TDD quality claims are unsupported.

---

## Part 2: What Is the Optimal Spec Format for AI Agents?

### Tests + Types = Strongest Input

| Format | Improvement | Evidence | Source |
|--------|-------------|----------|--------|
| Tests alongside problem statements | +8-18% pass rate | Empirical (GPT-4, Llama 3) | [TDD for Code Gen](https://arxiv.org/abs/2402.13521) |
| Tests as sole prompt (no NL) | Top models reach 95% | Empirical (19 models) | [Tests as Prompt](https://arxiv.org/abs/2505.09027) |
| Human-refined specs | Up to 50% error reduction | Controlled studies | [Spec-Driven Dev](https://arxiv.org/html/2602.00180v1) |
| SCoT prompts (structured reasoning) | +13-17% Pass@1 | Peer-reviewed, ACM | [SCoT](https://arxiv.org/abs/2305.06599) |
| Type/schema-constrained generation | 50%+ compilation error reduction | Academic + industrial | [Type-Constrained](https://arxiv.org/pdf/2504.09246) |

### Optimal Prompt Length

| Token Range | Effect | Source |
|-------------|--------|--------|
| 500-2,000 | Optimal accuracy/speed tradeoff | [Particula Research](https://particula.tech/blog/optimal-prompt-length-ai-performance) |
| 2,000-4,000 | Response time +40-80%, marginal accuracy gain (+2-3%) | Same |
| 4,000+ | Measurable quality drops, missed details, hallucinations | Same |
| Doubling features in prompt | o1-preview: 95% → 65%; Claude 3.5: 88% → 75% | [WebApp1K](https://arxiv.org/abs/2505.09027) |
| Claude stable to ~5,500 tokens | Model-specific threshold | Particula |

**Key finding:** 1,000-3,500 tokens per task is the sweet spot.

### "Lost in the Middle" Effect

**Source:** [research.trychroma.com/context-rot](https://research.trychroma.com/context-rot), [arxiv.org/html/2403.04797v1](https://arxiv.org/html/2403.04797v1)
**Evidence strength:** HIGH (multiple independent confirmations)

- Models retrieve best from BEGINNING and END of prompts
- Performance degrades 30%+ for middle-positioned content
- Claude models showed "the most pronounced gap between focused and full prompt performance"

**Implication:** Put critical requirements (what must be true) at TOP and BOTTOM of specs. Supporting context in middle.

### Examples: Diminishing Returns

- Few-shot prompting increases accuracy by 15-40%
- 2-3 well-chosen examples are optimal
- Beyond 8, performance plateaus or DEGRADES
- Quality of examples matters more than quantity
- Source: [Prompt Engineering Guide](https://www.promptingguide.ai/techniques/fewshot), [CEDAR (ICSE 2023)](https://people.ece.ubc.ca/amesbah/resources/papers/cedar-icse23.pdf)

### Types and Interfaces

- TypeChat (Microsoft): TypeScript type definitions constrain LLM output with automatic validation
- Type-constrained decoding: 50%+ compilation error reduction, significant correctness increase
- Practical: 90% reduction in ID mix-up bugs, 3x faster convergence
- Sources: [TypeChat](https://microsoft.github.io/TypeChat/docs/techniques/), [Type-Constrained Code Gen](https://arxiv.org/pdf/2504.09246), [TypeScript for LLM Coding](https://thomaslandgraf.substack.com/p/why-i-choose-typescript-for-llmbased)

### The Recommended Spec Template (Evidence-Based)

**Tier 1: Must-Haves (strong evidence, high impact):**
- Verification criteria (tests/assertions) — single highest-leverage element
- Type signatures/interfaces — 50%+ error reduction
- Concise intent statement (1-3 sentences) — the "why"
- Explicit constraints/boundaries — prevents drift, hallucination

**Tier 2: Should-Haves (moderate evidence):**
- 2-3 concrete examples (input/output pairs)
- Structured delimiters (XML/Markdown sections)
- Given/When/Then acceptance criteria
- File paths / architecture pointers

**Tier 3: Nice-to-Haves (situational):**
- Progressive task decomposition
- Priority indicators
- Error handling expectations

### Concrete Spec Format

```
## Goal
[1-3 sentences: What and why]

## Contract
[Type signatures, interfaces, API contract]

## Acceptance Criteria
[Given/When/Then scenarios — happy path + edge cases]

## Tests
[Concrete test cases the implementation must pass]

## Constraints
[MUST / MUST NOT rules, file boundaries, patterns to follow]

## Examples
[1-2 concrete input/output pairs]
```

Total: 1,000-3,500 tokens per feature task.

### Industry Spec-Driven Development Tools

| Tool | Format | Notes |
|------|--------|-------|
| GitHub Spec Kit | Markdown (spec.md, plan.md, tasks/) | 50k+ GitHub stars; thorough but heavyweight |
| Kiro (AWS) | Markdown, 3 files | Given/When/Then → Design → Tasks |
| Tessl | Markdown with @generate/@test tags | Spec-as-source, code regenerated from specs |

All three assume feature-scoped decomposition, not layer-scoped.

Source: [Martin Fowler / Thoughtworks](https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html)

### What Anthropic Recommends

From [Claude Code Best Practices](https://code.claude.com/docs/en/best-practices):
- "Give Claude a way to verify its work. This is the single highest-leverage thing you can do."
- Keep CLAUDE.md concise: "For each line, ask: Would removing this cause mistakes? If not, cut it"
- Write spec to SPEC.md, then start fresh session to implement (clean context)

---

## Part 3: Vertical vs Horizontal Task Decomposition

### The Central Finding

**Neither pure vertical NOR pure horizontal wins. The research converges on:**

> Vertical specs → Horizontal (layer-specialized) dispatch → Integration verification per slice

### Against Pure Horizontal (Current God-Agent Approach)

- **32.3% of multi-agent failures** are inter-agent misalignment (conversation resets, ignoring other agents, information withholding)
- Source: [Why Do Multi-Agent LLM Systems Fail?](https://arxiv.org/abs/2503.13657) — 1600+ traces, 7 MAS frameworks

- Backend and frontend agents independently produce **inconsistent error handlers, logging patterns, API contracts** unless constrained by shared specifications
- Source: [Augment Code](https://www.augmentcode.com/learn/agentic-swarm-vs-spec-driven-coding)

- "Perfect agent coordination accelerates failure when agents lack architectural understanding"
- Source: Same

### Against Pure Vertical (Single Full-Stack Agent)

- **Context rot** drops models below 50% performance at 32K tokens
- Source: [Chroma Research](https://research.trychroma.com/context-rot) — 11 of 12 models below 50% at 32K

- Irrelevant context increases error rates by up to 300%
- Source: Same

- Context should be treated as "a scarce, high-value resource"
- Source: [Factory.ai](https://factory.ai/news/context-window-problem)

### For the Hybrid (Vertical Spec → Specialized Dispatch)

**Specialized agents perform better:**
- Self-Collaboration (analyst + coder + tester) surpasses single-agent by 30-47% on Pass@1
- Source: [Code in Harmony](https://openreview.net/forum?id=URUMBfrHFy)

- Anthropic's Opus 4 lead + Sonnet 4 subagents outperformed single-agent by **90.2%**
- BUT: used ~15x more tokens
- Source: [Anthropic Multi-Agent Research System](https://www.anthropic.com/engineering/multi-agent-research-system)

**BUT specialization has limits:**
- Multi-agent advantage SHRINKS as base LLM capability increases
- MetaGPT improvement: 10.7% (ChatGPT) → 3.0% (Gemini-2.0-Flash)
- ~80% of test cases showed "ties" (both single and multi-agent succeeded or both failed)
- Multi-agent consumed **4-220x more input tokens** and **2-12x more output tokens**
- Source: [Single-agent or Multi-agent? Why Not Both?](https://arxiv.org/pdf/2505.18286)

### Vertical Slice Architecture Is Most AI-Friendly

- All code for a feature in one directory → efficient context priming
- "Token efficiency" improves — agents only load one feature slice
- VSA trades code duplication for isolation and AI clarity
- Source: [Comparing Architectures for AI Tools](https://cloudurable.com/blog/a-deeper-dive-when-the-vibe-dies-comparing-codebase-architectures-for-ai-tools/)

### SWE-Bench Pro: Multi-File Complexity

- Average solution: 4.1 files, 107 lines
- Agent performance drops from 70%+ (simple) to below 25% (multi-file)
- Agents adopt "greedy" approach — focus narrowly, rarely modify more than one file
- Source: [SWE-Bench Pro](https://arxiv.org/html/2509.16941v1)

### Integration Is the Achilles' Heel

- 44.2% of MAS failures: system design issues
- 32.3% of MAS failures: inter-agent misalignment
- 23.5% of MAS failures: task verification
- Source: [Why Do Multi-Agent LLM Systems Fail?](https://arxiv.org/abs/2503.13657)

- Bugs (22%), infrastructure (14%), agent coordination (10%) most frequent issues in 42K commits
- Source: [Large-Scale Study on Multi-Agent AI Systems](https://arxiv.org/pdf/2601.07136)

### Real-World Practitioner Guidance

**Addy Osmani on agent teams:**
- Scope by file ownership: "Two teammates editing the same file leads to overwrites"
- Multi-agent excels for "parallel exploration" with "largely independent" teammates
- Single-agent superior for "heavy interdependencies" and "routine, focused problems"
- Source: [Claude Code Swarms](https://addyosmani.com/blog/claude-code-agent-teams/)

**The 80% Problem:**
- Agents sprint through 80% then stall from assumption propagation
- Fix: spend 70% of effort on problem definition, 30% on execution
- Source: [The 80% Problem](https://addyo.substack.com/p/the-80-problem-in-agentic-coding)

**Anthropic Agent SDK patterns:**
- Orchestrator-worker pattern recommended
- Subagents need: "an objective, an output format, guidance on tools, and clear task boundaries"
- Without this, agents duplicate work or leave gaps
- Source: [Claude Agent SDK](https://claude.com/blog/building-agents-with-the-claude-agent-sdk)

---

## Part 4: Design Principles for v3 (Derived from Evidence)

### Principle 1: Specs Are the Product (Not Code)

The god-agent's primary job is producing excellent specs. Code generation follows naturally from good specs. Invest 70% of pipeline effort in spec quality (Phases 0-2), 30% in execution (Phase 3+).

Evidence: 50% error reduction from human-refined specs. 8-18% from tests-as-input. The 80% problem stems from poor specs, not poor coding.

### Principle 2: Feature-First Decomposition

Decompose work by FEATURE (vertical slices), not by LAYER (horizontal). Each feature spec contains both backend and frontend requirements, sharing an API contract.

Evidence: VSA is most AI-friendly architecture. All SDD tools assume feature-scoped specs. Integration bugs are the #1 multi-agent failure mode.

### Principle 3: Specialized Dispatch Within Features

Within each feature, dispatch to layer-specialized agents (backend-implementer, frontend-implementer). Don't mix .NET and React in one context.

Evidence: 30-47% improvement from role separation. Context rot at 32K tokens. Irrelevant context adds 300% error rate.

### Principle 4: Small Context, High Signal

Each agent dispatch: 1,000-3,500 tokens of spec. Include types, acceptance criteria, constraints. Exclude irrelevant context.

Evidence: Performance drops at 4K+ tokens. Doubling features causes 30% quality drop. Lost-in-middle effect degrades middle content.

### Principle 5: Share Contracts, Not Implementations

Backend agent sees feature spec + API contract + domain models. Frontend agent sees feature spec + API contract + UI requirements. Neither sees the other's implementation.

Evidence: 32.3% of MAS failures from inter-agent misalignment. Shared contracts prevent drift.

### Principle 6: Verify Per Slice

Run integration verification after each feature, not after all features. Catch contract drift immediately.

Evidence: 23.5% of MAS failures are task verification. Late integration catches problems too late.

### Principle 7: Tests Are Output, Not Process

Don't force TDD discipline on the agent. Instead:
- Include acceptance criteria/tests in the SPEC (tests as input — research-supported)
- Verify tests exist and pass in the OUTPUT (quality gate)
- Use a dedicated test quality reviewer to catch anti-patterns

Evidence: Agent-written tests don't help the agent (2.6% difference). Tests as input help (+8-18%). The value of tests is for humans, not the agent.

### Principle 8: Scaffolding Reduces Errors

Generate type shells, interfaces, and route stubs BEFORE implementation. This gives agents concrete types to import and reduces compilation errors.

Evidence: Type-constrained generation reduces errors 50%+. Types as input = strong signal.

---

## Part 5: What to Keep from v1/v2

| Component | Keep? | Reason |
|-----------|-------|--------|
| Phase -1 Scaffold | YES | Unchanged, works well |
| Phase 0 Intake / brainstorming | YES | Spec quality is critical |
| Phase 1 Architecture doc (abstract) | REDESIGN | Replace with feature scenarios + types + acceptance criteria |
| Phase 2 Horizontal plans | REDESIGN | Replace with vertical feature specs |
| Phase 2.5 Contract Scaffolding | YES (core concept) | Type shells reduce errors 50%+. Simplify implementation. |
| Phase 3 Three-agent TDD loop | NO | Not supported by research |
| Phase 3 Specialized agents (backend/frontend) | YES | 30-47% improvement from role separation |
| Phase 3 Mechanical gates (build/test verification) | YES | Verification is always valuable |
| Phase 3 Quality reviewer | YES | But review output quality, not TDD process |
| Phase 4 Integration | REDESIGN | Move to per-feature verification, not end-of-pipeline |
| Phase 5 E2E Testing | YES | Unchanged |
| Phase 6 Design Polish | YES | Unchanged |
| Anti-pattern detection (dotnet-tdd, react-tdd) | YES | Test quality rules still apply to output |
| Bug table requirement | YES | Forces agents to justify each test |
| Token tracking | YES | Essential for cost management |
| STATE.md resume | YES | Essential for long-running pipeline |

---

## Part 6: Open Questions for v3 Design

1. **How to handle shared infrastructure?** Auth middleware, global error handling, database migrations span features. Should there be a "foundation" slice that runs first?

2. **Feature interdependencies.** Feature B may depend on Feature A's endpoints. How does the second agent know what the first produced? (Answer: shared contract from spec, verified by integration gate.)

3. **How to generate good feature scenarios?** The whole pipeline now depends on Phase 1 producing great per-feature specs. What makes Phase 1 reliable?

4. **What about the architecture doc?** Do we still need a global architecture doc, or do feature specs + conventions doc suffice?

5. **Complexity routing.** Should simple features get a lighter pipeline (fewer dispatches)? The "Why Not Both?" paper found 88.1% cost reduction from confidence-guided cascading.

6. **How do tests get written?** If not TDD, how do we ensure test coverage? Options:
   - Acceptance criteria in spec → implementer writes tests alongside code
   - Post-implementation test augmentation agent
   - Reviewer gate that checks coverage

7. **What about the v2.0.0 changes we just implemented?** The new agent files (backend-test-writer, frontend-test-writer) and SKILL.md changes — revert, keep, or adapt?

---

## Sources Index

### Empirical Research (Highest Evidence Quality)

| Paper | Year | Key Finding | URL |
|-------|------|-------------|-----|
| Rethinking Agent-Generated Tests | 2026 | Tests don't help agents; 2.6% difference | [arxiv](https://arxiv.org/html/2602.07900v1) |
| TDD for Code Generation | 2024 | Tests as input +8-18% | [arxiv](https://arxiv.org/abs/2402.13521) |
| Tests as Prompt (WebApp1K) | 2025 | Instruction following > coding ability; attention decay | [arxiv](https://arxiv.org/abs/2505.09027) |
| Spec-Driven Development | 2026 | Human-refined specs: 50% error reduction | [arxiv](https://arxiv.org/html/2602.00180v1) |
| SCoT Prompting | 2023 | Structured reasoning: +13-17% Pass@1 | [arxiv](https://arxiv.org/abs/2305.06599) |
| Type-Constrained Code Gen | 2025 | 50%+ compilation error reduction | [arxiv](https://arxiv.org/pdf/2504.09246) |
| Context Rot | 2025 | Performance below 50% at 32K tokens | [chroma](https://research.trychroma.com/context-rot) |
| Lost in the Middle | 2024 | 30%+ degradation for middle content | [arxiv](https://arxiv.org/html/2403.04797v1) |
| SWE-Bench Pro | 2025 | Agents struggle with multi-file (4.1 files avg) | [arxiv](https://arxiv.org/html/2509.16941v1) |
| Why Do MAS Fail? | 2025 | 32.3% failures from inter-agent misalignment | [arxiv](https://arxiv.org/abs/2503.13657) |
| Single or Multi-Agent? | 2025 | Hybrid routing: 2% better at 50% cost | [arxiv](https://arxiv.org/pdf/2505.18286) |
| Code in Harmony | 2025 | Role separation: +30-47% Pass@1 | [openreview](https://openreview.net/forum?id=URUMBfrHFy) |
| Large-Scale MAS Study | 2026 | Bugs 22%, coordination 10% of issues | [arxiv](https://arxiv.org/pdf/2601.07136) |
| Self-Organized Agents | 2024 | Dynamic decomposition: +5% HumanEval | [arxiv](https://arxiv.org/abs/2404.02183) |

### Industry Case Studies (Medium-High Evidence)

| Source | Key Finding | URL |
|--------|-------------|-----|
| Anthropic Multi-Agent System | 90.2% improvement, 15x token cost | [anthropic](https://www.anthropic.com/engineering/multi-agent-research-system) |
| Claude Agent SDK | Orchestrator-worker pattern recommended | [claude](https://claude.com/blog/building-agents-with-the-claude-agent-sdk) |
| Factory.ai | Context as scarce resource | [factory](https://factory.ai/news/context-window-problem) |
| Augment Code | "Perfect coordination accelerates failure without understanding" | [augment](https://www.augmentcode.com/learn/agentic-swarm-vs-spec-driven-coding) |
| SWE-bench Verified | Spec clarity is prerequisite for agent success | [openai](https://openai.com/index/introducing-swe-bench-verified/) |

### Practitioner Guidance (Medium Evidence)

| Source | Key Finding | URL |
|--------|-------------|-----|
| Addy Osmani: Good Spec | 6-section spec format; 3-tier boundaries | [osmani](https://addyosmani.com/blog/good-spec/) |
| Addy Osmani: LLM Workflow | Tests as safety nets; manual review essential | [osmani](https://addyosmani.com/blog/ai-coding-workflow/) |
| Addy Osmani: Agent Teams | Scope by file ownership; activity ≠ value | [osmani](https://addyosmani.com/blog/claude-code-agent-teams/) |
| Addy Osmani: 80% Problem | 70% spec effort, 30% execution | [osmani](https://addyo.substack.com/p/the-80-problem-in-agentic-coding) |
| Martin Fowler: SDD Tools | Spec Kit, Kiro, Tessl comparison | [fowler](https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html) |
| Thoughtworks: SDD | "Most important practice to emerge in 2025" | [thoughtworks](https://www.thoughtworks.com/en-us/insights/blog/agile-engineering-practices/spec-driven-development-unpacking-2025-new-engineering-practices) |
| Claude Code Best Practices | "Give Claude a way to verify its work" | [anthropic](https://code.claude.com/docs/en/best-practices) |
| GPT-5 Prompting Guide | Structured XML specs for adherence | [openai](https://cookbook.openai.com/examples/gpt-5/gpt-5_prompting_guide) |
| VSA for AI Tools | Vertical slices most AI-friendly | [cloudurable](https://cloudurable.com/blog/a-deeper-dive-when-the-vibe-dies-comparing-codebase-architectures-for-ai-tools/) |
| Anthropic 2026 Trends | Multi-agent + AI review = standard | [claude](https://claude.com/blog/eight-trends-defining-how-software-gets-built-in-2026) |
| TypeChat (Microsoft) | Schema engineering > prompt engineering | [microsoft](https://microsoft.github.io/TypeChat/docs/techniques/) |
| alexop.dev | Skill activation 20%→84% (NOT quality) | [alexop](https://alexop.dev/posts/custom-tdd-workflow-claude-code-vue/) |
| BDD for AI | 95% acceptance rate from Gherkin specs | [arxiv](https://arxiv.org/html/2504.07244v1) |
| JetBrains Junie | 4-artifact approach: requirements, plan, tasks, guidelines | [jetbrains](https://blog.jetbrains.com/junie/2025/10/how-to-use-a-spec-driven-approach-for-coding-with-ai/) |
