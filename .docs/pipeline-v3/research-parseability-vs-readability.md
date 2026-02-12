# Research: Parseability vs Readability in Spec Formats for AI Coding Agents

**Date:** 2026-02-12
**Scope:** Tradeoffs between structured/parseable spec formats and natural language for AI coding agents

---

## 1. Structured Formats vs Prose: How LLMs Handle Input

### Key Finding: Hybrid Wins — Neither Pure Structure Nor Pure Prose Is Optimal

LLMs handle structured input and prose differently, and the optimal approach is a hybrid:

- **Markdown with headers/lists outperforms flat prose.** A 2024 study showed GPT-4 achieved 81.2% accuracy on reasoning tasks with Markdown prompts vs 73.9% with JSON-formatted prompts. Headers (`#`, `##`) provide hierarchical signals that LLMs use to understand concept relationships.
- **Semi-structured input improves reasoning and reduces hallucinations.** ThoughtWorks research confirms: "providing the model with semi-structured input prompts or forcing it to output in a structured manner can significantly improve reasoning performance and reduce hallucinations."
- **Pure YAML/JSON has diminishing returns.** JSON won for older models (GPT-3.5: 59.7% vs Markdown 50%), but newer models (GPT-4+) prefer Markdown. The trend is clear: as models improve, they extract more from natural-language-like structure.
- **Token efficiency matters.** Markdown is lighter than JSON, XML, or HTML — conveys meaning with fewer characters. More room for meaningful content, lower costs per token. Research shows Markdown improves RAG retrieval accuracy 20-35% vs HTML or plain text.

### The YAML Frontmatter + Markdown Body Pattern

This hybrid is emerging as the de facto standard for AI agent configuration:

- **YAML frontmatter** = machine-readable metadata (name, version, dependencies, flags)
- **Markdown body** = human-readable instructions, examples, behavioral descriptions
- Used by: Claude's SKILL.md, GitHub SpecKit, Anthropic's plugin system, Jekyll-derived tooling
- Jimmy Song (2025): "YAML describes the 'state and configuration' of machine behavior; Markdown describes the 'context and rules' of intelligent agent behavior."

### XML Tags for Section Demarcation

Anthropic specifically recommends XML tags (`<instructions>`, `<context>`, `<example>`) for structuring Claude prompts:

- Claude was trained to recognize XML tags as organizational markers
- No "magic" tags — any consistent tag naming works
- Benefits: clarity, accuracy, flexibility, parseability
- Best for: separating data from instructions, wrapping examples, delineating output format

**Verdict:** Use YAML frontmatter for metadata, Markdown for instructions, XML tags for section boundaries when precision matters. Avoid pure JSON/YAML for behavioral specs.

---

## 2. Prompt Engineering Research on Structure

### Headers, Bullets, and Numbered Lists

Research consensus from 2024-2025:

- **Headers create hierarchy.** When an LLM sees `# Heading`, it knows a new section starts. Hierarchical structure tells models how concepts relate — main ideas vs subpoints vs list items.
- **Bulleted lists are parsed as distinct items**, not a single paragraph. Critical for acceptance criteria, constraints, and enumerated requirements.
- **Numbered lists imply sequence/priority.** Useful for ordered steps, ranked constraints.
- **Bold/italics add semantic weight.** LLMs trained on Markdown pick up on `**critical**` as emphasis.
- **Code blocks signal executable/literal content.** Triple backticks create clear boundaries between natural language and code.

### Chain-of-Thought Style Specs

- Self-planning approaches achieve up to **25.4% improvement** in Pass@1 over direct generation
- Specs that "walk through the logic" (explaining WHY before WHAT) help agents plan before implementing
- The TDAID workflow (Plan → Red → Green → Refactor → Validate) leverages this
- Walking through design rationale in the spec reduces hallucinated requirements

### The "Curse of Instructions" Problem

Critical finding: **LLM performance degrades as instruction count increases.**

- Instruction following rate drops measurably as instructions accumulate
- Progressive instruction addition introduces tension/contradictions between rules
- **Degradation begins around 3,000 tokens** — well below context window limits
- Optimal prompt length: **150-300 words** for moderately complex tasks; diminishing returns past ~500 words
- Solution: modular specs (split by component), not monolithic documents

---

## 3. Executable Specifications

### Cucumber/Gherkin as Contracts

- Given/When/Then format provides structured, natural-language-like behavioral contracts
- ThoughtWorks recommends specs "have a clear structure, with a common style to define scenarios using Given/When/Then"
- AI tools like Gherkinizer use LLMs to auto-generate BDD tests from user stories
- **Strength:** Readable by humans AND directly executable as tests
- **Weakness:** Verbose for complex logic; can become "documentation theater" if disconnected from implementation

### Failing Tests as Specs (TDD-First)

The TDD approach is experiencing a renaissance with AI agents:

- **Tests as precise targets.** "By writing the failing test first, you give the AI a precise target. The test acts as a clear specification and an immediate correctness checker."
- **Fast feedback loops.** Pass/fail results provide instant learning about effectiveness
- **TDAID workflow:** Plan → Red → Green → Refactor → Validate (extends classic TDD with explicit planning and validation phases)
- **Key benefit:** Tests ARE the spec — no drift between documentation and behavior
- **Key limitation:** Tests specify WHAT, not WHY. Design rationale is lost. Tests alone don't communicate architectural intent.

### Property-Based Specification

Research from 2025 (agentic property-based testing across Python ecosystem):

- AI agents successfully wrote Hypothesis property-based tests across 100 packages, 933 modules
- **56% of 984 bug reports validated as genuine bugs**; 32% deemed worthy of reporting
- Properties (invariants, round-trips, mathematical properties, metamorphic relations) are a natural specification format
- **Cost:** ~$9.93 per valid bug discovered, ~$5.87 per module analyzed
- Kiro (2025): "Property tests are a great match for specification-driven development because specification requirements are oftentimes directly expressing properties"
- **Critical constraint:** Agent must "only test properties that the code is explicitly claiming" — inventing assumptions leads to false positives

### Comparison: Document Specs vs Executable Specs

| Dimension | Document-Based Specs | Executable Specs (Tests/Properties) |
|-----------|---------------------|--------------------------------------|
| Ambiguity | Higher — natural language is inherently ambiguous | Lower — pass/fail is unambiguous |
| Design intent | Rich — can explain WHY | Poor — only captures WHAT |
| Drift risk | High — docs go stale | Low — tests break when behavior changes |
| Completeness | Aspirational — hard to verify | Measurable — coverage metrics exist |
| Agent guidance | Good for planning phase | Good for implementation phase |
| Feedback speed | None (read-only) | Immediate (run and check) |

**Verdict:** Use document specs for planning/design + executable specs for implementation/validation. Neither alone is sufficient.

---

## 4. Specification by Example

### Concrete Examples as Primary Spec

Research findings on examples vs descriptions:

- **"One concrete code snippet demonstrating style outperforms three paragraphs of description"** (Addy Osmani)
- Few-shot input/output examples disambiguate natural language descriptions — they are NOT redundant with prose
- Different examples contribute differently to LLM coding capabilities; example SELECTION matters
- Model-free (CodeExemplar-Free) and model-based (CodeExamplar-Base) algorithms exist for optimal example selection

### How Well Do AI Agents Generalize from Examples?

- Instruction-tuned LLMs demonstrate **zero-shot generalization** — no fine-tuning needed
- RLEF (ICML 2025): agents improve via execution feedback on examples — generate, execute, learn, regenerate
- ToolAlpaca: agents generalize across unseen tools by adapting calling strategy from example patterns
- **Critical insight:** Agents generalize BETTER from concrete examples than from abstract rules, but examples must be representative of edge cases, not just happy paths

### Input/Output Examples vs Behavioral Descriptions

| Aspect | I/O Examples | Behavioral Descriptions |
|--------|-------------|------------------------|
| Precision | High — exact expected behavior | Medium — depends on language clarity |
| Edge cases | Must be explicitly enumerated | Can state general rules |
| Agent comprehension | Direct pattern matching | Requires interpretation |
| Maintenance | Brittle — changes require example updates | Flexible — rules adapt |
| Completeness | Combinatorial explosion for complex domains | Compact general statements |

**Verdict:** Use examples as primary disambiguation mechanism alongside behavioral descriptions. Put examples IN acceptance criteria sections, not in separate appendices. 3-5 well-chosen examples > 20 exhaustive ones.

---

## 5. Ambiguity Analysis

### What Makes Specs Ambiguous to AI Agents?

Root causes identified across research:

1. **Vague qualifiers.** "Secure authentication" → What OAuth flow? What token validation? What session management?
2. **Implicit knowledge.** Specs that assume domain context the agent lacks
3. **Missing error scenarios.** Happy path only → agent invents error handling (often poorly)
4. **Conflicting constraints.** Instructions that contradict each other as they accumulate
5. **Scope gaps.** What's NOT mentioned gets filled by the agent's training distribution — not your intent
6. **Abstract vs concrete.** "Use best practices" is meaningless; "Use bcrypt with cost factor 12" is actionable

### Common Failure Modes

- **Scope creep / "eager agent" problem.** Agents add features not requested when specs leave gaps. Solution: explicit "out of scope" sections and boundary constraints.
- **Hallucinated requirements.** When "the expression of goal information carries semantic vagueness, it can easily lead to erroneous parsing of user intention." Agents fill ambiguity with probabilistic associations from training data.
- **Confident guessing.** Training objectives reward plausible-looking output over accurate uncertainty expression. Models almost never say "I'm not sure what you mean."
- **Spec drift.** Living specs not updated → agent works against stale context.

### How Different Formats Handle Edge Cases

- **Gherkin:** Forces explicit scenario enumeration — good for edge cases but verbose
- **Property-based specs:** State invariants that must hold for ALL inputs — naturally handle edge cases
- **Markdown prose:** Can describe edge case categories but leaves specific behavior ambiguous
- **I/O examples:** Only cover enumerated cases; miss everything not shown
- **Tiered boundaries (Always/Ask/Never):** Addy Osmani's pattern — good for preventing forbidden behaviors

### Preventing Hallucinated Requirements

Best practices from industry (2024-2025):

1. **Explicit boundaries.** Define what's in AND out of scope
2. **Rigid behavioral contracts.** "These aren't just personality prompts; they are rigid behavioural contracts that define the agent's specific identity and boundaries."
3. **Single-step implementation.** One task at a time prevents agents from "biting off more than they can chew"
4. **Conformance suites.** Language-independent tests (often YAML-based) that any implementation must pass — acts as an executable contract
5. **LLM-as-a-Judge.** Second agent reviews first agent's output against spec for subjective criteria

---

## 6. Layered Specifications

### High-Level Intent + Detailed Acceptance Criteria

The spec-driven development community converges on layered approaches:

- **Inspirational specs** (initial intent) → **Canonical specs** (validated specifications with measurable success conditions)
- Business requirement specs SEPARATE from technical specifications (though "defining the boundary between the two is often unclear")
- Use "domain-oriented ubiquitous language to describe business intent rather than specific tech-bound implementations"

### Spec Hierarchies

Practical patterns emerging:

- **Hierarchical summarization.** Extended Table of Contents with summaries stays in prompt; detailed sections referenced separately
- **Modular division.** `SPEC_backend.md`, `SPEC_frontend.md` — focused agent work without monolithic context
- **Parent/child specs.** Parent coordinates; children contain granular details
- **Three-tier framework** (Guy Podjarny / Tessl):
  - Spec-assisted: baseline guidance (CLAUDE.md, API definitions)
  - Spec-driven: specs modified before code changes
  - Spec-centric: code is regenerable from detailed specs + tests

### Information Overload: How Much Context Is Too Much?

This is one of the most critical findings for pipeline design:

- **Context rot is real.** Chroma research (2024) showed LLM performance degrades as input grows, even on simple tasks. Models that handle something reliably at 100 tokens fail at 10,000 tokens.
- **Performance drops even with perfect retrieval.** A study showed 24.2% accuracy drop even when models could perfectly retrieve all evidence. Having information available ≠ being able to use it.
- **Lost in the middle effect.** LLMs recall information at the beginning and end of contexts better than the middle. Critical information buried in the middle may be "effectively invisible."
- **Quantified degradation:** Llama-3.1-8B: 59-85% accuracy loss at 30K tokens. Mistral-7B: 30% drop with whitespace distractors. General range: 13.9-85% degradation.
- **Advertised vs actual context windows.** A model claiming 200K tokens becomes unreliable around 130K, with sudden (not gradual) performance drops.
- **Instruction degradation starts at ~3,000 tokens** — well below context limits.

**Practical implications for spec design:**

1. Shorter, focused specs > monolithic documents
2. Critical information at the TOP, not buried in the middle
3. Modular specs loaded on-demand > one giant spec always in context
4. "Retrieve then reason" — have agent recite relevant spec sections before implementing
5. Each spec module should include ONLY globally-relevant constraints + domain-specific details
6. Start fresh context sessions after each implementation step; update specs with progress

---

## 7. Synthesis: Format Effectiveness Ranking

Based on all research reviewed, formats ranked by effectiveness for AI coding agents:

### Tier 1: Most Effective
1. **YAML frontmatter + Markdown body** — metadata parseable, instructions readable, examples inline
2. **Failing tests + design rationale doc** — unambiguous targets + architectural intent
3. **Markdown with XML section tags** — clear demarcation, Claude-optimized

### Tier 2: Effective
4. **Gherkin/BDD scenarios** — natural language + executable, but verbose
5. **Property-based specifications** — powerful for invariants, but require testing expertise
6. **Markdown with headers/bullets only** — good baseline, lacks metadata parseability

### Tier 3: Diminishing Returns
7. **Pure YAML/JSON specs** — machine-parseable but poor for behavioral description
8. **Flat prose documents** — human-readable but ambiguous for agents
9. **Monolithic PRDs** — too long, context rot, lost-in-the-middle effect

### The Optimal Spec Architecture

```
spec/
  SPEC.md                    # YAML frontmatter (metadata) + Markdown (intent, architecture, constraints)
  acceptance-criteria.md     # Given/When/Then scenarios OR I/O examples
  boundaries.md              # Always/Ask/Never constraints; out-of-scope
  tests/
    *.test.ts               # Failing tests as executable specs
    *.property.ts           # Property-based invariants
```

Key design principles:
- **Modular** — each file under ~500 words
- **Layered** — high-level intent separate from detailed criteria
- **Executable** — tests/properties as ground truth, docs as design rationale
- **Critical info first** — most important constraints at top of each file
- **Examples > descriptions** — concrete I/O examples in acceptance criteria
- **Explicit boundaries** — what's in scope, what's out, what to ask about

---

## Sources

### Spec-Driven Development & AI Agents
- [Addy Osmani — How to write a good spec for AI agents](https://addyosmani.com/blog/good-spec/)
- [ThoughtWorks — Spec-driven development: Unpacking 2025's key new practice](https://www.thoughtworks.com/en-us/insights/blog/agile-engineering-practices/spec-driven-development-unpacking-2025-new-engineering-practices)
- [Tessl — Taming agents with specifications: what the experts say](https://tessl.io/blog/taming-agents-with-specifications-what-the-experts-say)
- [Arguing with Algorithms — How to keep your AI coding agent from going rogue](https://www.arguingwithalgorithms.com/posts/technical-design-spec-pattern.html)
- [AGENTS.md — Open format for guiding AI coding agents](https://agents.md/)

### Context & Performance Research
- [Chroma Research — Context Rot](https://research.trychroma.com/context-rot)
- [Context Length Alone Hurts LLM Performance Despite Perfect Retrieval (arXiv)](https://arxiv.org/html/2510.05381v1)
- [How Many Instructions Can LLMs Follow at Once? (arXiv)](https://arxiv.org/pdf/2507.11538)

### Prompt Engineering & Structure
- [Anthropic — Use XML tags to structure your prompts](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/use-xml-tags)
- [A Systematic Survey of Prompt Engineering in LLMs (arXiv)](https://arxiv.org/html/2402.07927v2)
- [Markdown Formatting Influences LLM Responses](https://www.neuralbuddies.com/p/marking-up-the-prompt-how-markdown-formatting-influences-llm-responses)

### Executable Specifications & TDD
- [Qodo — How AI Code Assistants Are Revolutionizing TDD](https://www.qodo.ai/blog/ai-code-assistants-test-driven-development/)
- [Builder.io — Test-Driven Development with AI](https://www.builder.io/blog/test-driven-development-ai)
- [Awesome Testing — Test-Driven AI Development (TDAID)](https://www.awesome-testing.com/2025/10/test-driven-ai-development-tdaid)
- [Kiro — Property-Based Testing and Specs](https://kiro.dev/blog/property-based-testing/)
- [Agentic Property-Based Testing (arXiv)](https://arxiv.org/html/2510.09907v1)
- [Use Property-Based Testing to Bridge LLM Code Generation and Validation (arXiv)](https://arxiv.org/abs/2506.18315)

### Few-Shot Learning & Examples
- [Does Few-Shot Learning Help LLM Performance in Code Synthesis? (arXiv)](https://arxiv.org/html/2412.02906v1)
- [Self-Planning Code Generation with LLMs (ACM)](https://dl.acm.org/doi/10.1145/3672456)

### Hallucination & Ambiguity
- [Lakera — LLM Hallucinations Guide](https://www.lakera.ai/blog/guide-to-hallucinations-in-large-language-models)
- [Survey of Hallucinations in LLMs (Frontiers)](https://www.frontiersin.org/journals/artificial-intelligence/articles/10.3389/frai.2025.1622292/full)
- [LLM-based Agents Suffer from Hallucinations: A Survey (arXiv)](https://arxiv.org/html/2509.18970v1)

### Format Comparisons
- [TOON vs JSON: Why AI Agents Need Token-Optimized Data Formats](https://jduncan.io/blog/2025-11-11-toon-vs-json-agent-optimized-data/)
- [YAML to Markdown: Specification Driven Development and AI-Native Paradigms](https://jimmysong.io/blog/from-yaml-to-markdown-devops-vs-collabops/)
- [Markdown, JSON, YML, XML — Best content format for human and AI](https://blog.tech4teaching.net/markdown-json-yml-and-xml-what-is-the-best-content-format-for-both-human-and-ai/)
