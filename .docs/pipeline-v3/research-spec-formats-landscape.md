# Specification Formats Landscape Research

> Research date: 2026-02-12
> Purpose: Evaluate all major specification/contract formats as potential inputs to an AI coding agent pipeline.

---

## Table of Contents

1. [Given-When-Then (GWT / BDD)](#1-given-when-then-gwt--bdd)
2. [EARS (Easy Approach to Requirements Syntax)](#2-ears-easy-approach-to-requirements-syntax)
3. [Plain Natural Language Specs](#3-plain-natural-language-specs)
4. [User Stories](#4-user-stories)
5. [RFC-Style Specs](#5-rfc-style-specs)
6. [Architecture Decision Records (ADR)](#6-architecture-decision-records-adr)
7. [Job Stories](#7-job-stories)
8. [Structured PRDs](#8-structured-product-requirements-documents-prd)
9. [Gherkin / Cucumber Feature Files](#9-gherkin--cucumber-feature-files)
10. [Design Docs (Google-style)](#10-design-docs-google-style)
11. [Emerging: Spec-Driven Development (SDD) Formats](#11-emerging-spec-driven-development-sdd-formats)
12. [Comparative Analysis](#12-comparative-analysis)
13. [Communication Matrix](#13-communication-matrix)
14. [Conclusions for AI Agent Input](#14-conclusions-for-ai-agent-input)

---

## 1. Given-When-Then (GWT / BDD)

### Structure

```
Feature: User login

  Scenario: Successful login with valid credentials
    Given a registered user with email "user@example.com"
    And the user is on the login page
    When the user enters email "user@example.com" and password "correctpass"
    And clicks the "Sign In" button
    Then the user should be redirected to the dashboard
    And a session cookie should be set
```

### Strengths
- **Behavior-focused**: Describes what the system does, not how it's built.
- **Testable by definition**: Each scenario maps directly to an automated test.
- **Shared language**: Business and tech teams can both read/write these.
- **Precise preconditions**: The Given clause forces explicit state setup.
- **Composable**: Scenarios can be combined into feature suites.

### Weaknesses
- **Verbose at scale**: Complex features generate dozens of scenarios; hard to see the forest for the trees.
- **Misses "why"**: No inherent place for business motivation or strategic context.
- **Implementation-leaky**: Teams often write scenarios that mirror UI steps rather than behaviors.
- **No architectural guidance**: Says nothing about data models, API contracts, or system boundaries.
- **Combinatorial explosion**: Edge cases multiply quickly; hard to know when you've covered enough.

### Best Suited For
- Acceptance criteria for individual features.
- QA-driven development workflows.
- Features with clear input/output boundaries (forms, APIs, workflows).

### AI-Agent Friendliness: **HIGH**
GWT is highly structured, unambiguous, and maps directly to testable assertions. An AI agent can parse Given (setup), When (action), Then (verification) with near-zero interpretation effort. Excellent for generating test code and verifying implementation correctness. Weak as a standalone spec because it lacks architectural context.

---

## 2. EARS (Easy Approach to Requirements Syntax)

### Structure

Five template patterns using keywords:

| Pattern | Template | Example |
|---------|----------|---------|
| **Ubiquitous** | The `<system>` shall `<response>` | The system shall encrypt all data at rest. |
| **State-driven** | While `<state>`, the `<system>` shall `<response>` | While the user is authenticated, the system shall display the dashboard. |
| **Event-driven** | When `<trigger>`, the `<system>` shall `<response>` | When a payment fails, the system shall retry up to 3 times. |
| **Optional feature** | Where `<feature>`, the `<system>` shall `<response>` | Where two-factor auth is enabled, the system shall require a TOTP code. |
| **Unwanted behavior** | If `<condition>`, then the `<system>` shall `<response>` | If the session expires, then the system shall redirect to login. |

Complex requirements combine multiple keywords:

```
While the user is on the checkout page,
  When the user clicks "Place Order",
    If the payment method is expired,
      then the system shall display an error and prevent order submission.
```

### Strengths
- **Eliminates ambiguity**: Constrained templates force precise, single-interpretation requirements.
- **Lightweight adoption**: No special tools needed; just follow the templates.
- **Proven in safety-critical domains**: Developed at Rolls-Royce for jet engine software.
- **Addresses 8 common requirement defects**: Ambiguity, vagueness, complexity, omission, duplication, wordiness, inappropriate implementation, and untestability.
- **Quantifiably better**: Case studies show measurable quality improvement vs. freeform requirements.

### Weaknesses
- **No "why"**: Pure requirements with no business context or motivation.
- **Long sentences**: More than 3 preconditions create unwieldy single-sentence requirements.
- **Individual requirements, not stories**: Doesn't capture user flows or end-to-end behavior.
- **Adoption resistance**: Teams used to prose specs may find templates constraining.
- **No architecture**: Says nothing about system design, data models, or integration patterns.

### Best Suited For
- Safety-critical and regulated systems.
- Embedded systems, firmware, hardware-adjacent software.
- Supplementing other formats with precise constraint definitions.

### AI-Agent Friendliness: **HIGH**
Highly parseable due to fixed keyword patterns. An AI agent can extract trigger, state, condition, and expected response programmatically. Excellent for generating validation logic and constraint checks. However, insufficient as a sole input — needs architectural context and intent layered on top.

---

## 3. Plain Natural Language Specs

### Structure

```
The system should allow users to create an account using their email address.
After registration, a confirmation email should be sent. The user cannot
access protected resources until they verify their email by clicking the
link in the confirmation email. Passwords must be at least 8 characters
and include a number and special character.
```

### Strengths
- **Zero learning curve**: Everyone can write and read prose.
- **Flexible**: Can express anything — motivation, constraints, edge cases, context.
- **Fast to write**: No template overhead; just describe what you want.
- **Captures nuance**: Supports qualitative descriptions, trade-off discussions, and soft requirements.

### Weaknesses
- **Inherently ambiguous**: "The system should provide a fast response" means different things to different people.
- **Root cause of defects**: Studies show 56% of software defects originate in requirements analysis; ~50% of those from poorly written natural language requirements.
- **No structure for machines**: An AI agent must infer structure, intent, boundaries, and acceptance criteria from unstructured text.
- **Inconsistency**: Different authors describe similar requirements differently; no enforced consistency.
- **Incomplete by nature**: Easy to omit edge cases, error handling, and non-functional requirements.

### Best Suited For
- Early brainstorming and ideation.
- Communicating high-level vision to stakeholders.
- Supplementary context alongside structured formats.

### AI-Agent Friendliness: **LOW-MEDIUM**
LLMs can process natural language, but ambiguity leads to hallucinated interpretations. The agent must make assumptions about unstated requirements, which increases error rate. Larger context doesn't help — research shows providing larger context to LLMs actually drops quality. Natural language works as supplementary context, not as a primary contract.

---

## 4. User Stories

### Structure

```
As a [role], I want [capability], so that [benefit].

Example:
As a marketplace seller, I want to set custom shipping rates per region,
so that I can offer competitive prices without losing margin on shipping.

Acceptance Criteria:
- Seller can define shipping zones by country/region
- Each zone has an independent rate
- Default rate applies to undefined zones
- Rates display at checkout before payment
```

### Strengths
- **User-centric**: Forces thinking about who benefits and why.
- **Prioritizable**: The "so that" clause enables value-based prioritization.
- **Small and composable**: Each story is a self-contained unit of work.
- **Widely understood**: Industry-standard format; teams know how to estimate and deliver against stories.
- **Domain learning**: Teams that routinely write user stories develop stronger domain understanding.

### Weaknesses
- **Training wheels**: The format is intentionally transitional — meant to teach user-centric thinking, not serve as a permanent spec format.
- **Forced fit**: "I want to enter a strong password" — the user doesn't actually want this. The format misrepresents system constraints as user desires.
- **Missing technical detail**: No place for data models, API contracts, performance requirements, or system constraints.
- **Shallow acceptance criteria**: The standard format doesn't enforce completeness; teams often write vague or incomplete criteria.
- **"So that" overhead**: For obvious features (e.g., login), the benefit clause adds noise without insight.
- **No architecture**: Describes behavior from outside the system; says nothing about internal design.

### Best Suited For
- Product backlog management.
- Communicating user needs to development teams.
- Sprint planning and estimation.

### AI-Agent Friendliness: **MEDIUM**
The role/want/benefit structure is parseable and helps an AI understand intent. Acceptance criteria (when present and well-written) provide testable assertions. However, user stories lack the technical specificity an AI agent needs to make design decisions. An agent given only user stories must infer architecture, data models, error handling, and integration patterns — leading to wide variance in output quality.

---

## 5. RFC-Style Specs

### Structure

Typical sections (varies by org — examples from Uber, HashiCorp, Sourcegraph):

```markdown
# RFC: Implement Rate Limiting for Public API

## Status: Proposed
## Author: Jane Smith
## Reviewers: [list]

## Summary
One-paragraph description of the proposed change.

## Background
Why this matters now. What has changed. What the current state is.

## Problem
Specific problem being solved, with data/evidence.

## Proposal
Detailed technical proposal including:
- Architecture changes
- API design
- Data model changes
- Migration strategy

## Alternatives Considered
Other approaches and why they were rejected.

## Security / Performance / Operational Concerns
Cross-cutting concerns.

## Rollout Plan
How to ship incrementally and safely.

## Open Questions
Unresolved decisions.
```

### Strengths
- **Comprehensive**: Covers motivation, technical design, alternatives, and operational concerns in one document.
- **Decision-forcing**: The alternatives section forces explicit trade-off analysis.
- **Review-oriented**: Built for asynchronous feedback; catches issues early "when changes are still cheap."
- **Organizational memory**: Creates a durable record of why decisions were made.
- **Proven at scale**: Used by Google, Uber, Amazon, Stripe, HashiCorp, and most large engineering orgs.

### Weaknesses
- **Heavy**: 10-20 pages for larger projects; overhead not justified for simple changes.
- **No standardized format**: Every org customizes; no universal RFC template.
- **Point-in-time**: Tends to drift from reality post-implementation.
- **Narrative, not executable**: Prose descriptions don't map directly to tests or code.
- **Review bottleneck**: Requires multiple reviewers; can slow down iteration.

### Best Suited For
- System-level architectural changes.
- Cross-team features requiring coordination.
- Changes with significant trade-offs or risk.

### AI-Agent Friendliness: **MEDIUM-HIGH**
Rich in context, motivation, and technical detail — exactly what an AI agent needs to understand "why" and make informed design choices. However, the narrative format requires natural language understanding to extract actionable requirements. The lack of standardized structure means the agent must adapt to each org's RFC format. Best used as context alongside more structured acceptance criteria.

---

## 6. Architecture Decision Records (ADR)

### Structure (Nygard template)

```markdown
# ADR 4: Use PostgreSQL for Order Storage

## Status
Accepted

## Context
We need persistent storage for order data. The system processes ~10k
orders/day with complex queries for reporting. The team has strong
PostgreSQL experience. We evaluated DynamoDB but the query patterns
don't fit a key-value model well.

## Decision
We will use PostgreSQL 16 with read replicas for the order storage layer.

## Consequences
- Positive: Team familiarity reduces ramp-up; rich query support.
- Negative: Requires connection pooling; vertical scaling limits.
- Neutral: Standard backup/restore procedures apply.
```

### Strengths
- **Decision-focused**: Explicitly captures the "why" behind architectural choices.
- **Lightweight**: 1-2 pages; low overhead to write and maintain.
- **Immutable history**: Numbered sequentially; decisions are superseded, not deleted.
- **Onboarding accelerator**: New team members understand past choices and their rationale.
- **Version-controlled**: Lives alongside code in the repo.

### Weaknesses
- **Decision-only**: Captures what was decided and why, but not what needs to be built.
- **No requirements**: Doesn't describe features, user needs, or acceptance criteria.
- **Retrospective bias**: Often written after the decision, losing nuance of the deliberation.
- **Accessibility**: Living in version control makes them less visible to non-dev stakeholders.
- **No implementation guidance**: Says "we chose X" but not "here's how to implement X."

### Best Suited For
- Recording architectural decisions for posterity.
- Providing context for future changes ("why did we choose this?").
- Supplementing other spec formats with decision rationale.

### AI-Agent Friendliness: **MEDIUM**
Useful as context input — an AI agent given ADRs understands existing constraints, technology choices, and team preferences. However, ADRs don't describe what to build, only past decisions. An agent can't derive implementation tasks from ADRs alone. Valuable as supplementary context alongside a primary spec format.

---

## 7. Job Stories

### Structure

```
When [situation/context],
I want to [motivation/action],
so I can [expected outcome].

Example:
When I'm reviewing my monthly spending on the dashboard,
I want to filter transactions by category,
so I can identify where to cut costs.
```

### Strengths
- **Context-driven**: The "When" clause captures the situation that triggers the need, not the user role.
- **Motivation-focused**: Describes why the user acts, not just what they do.
- **Research-rooted**: Originated from Jobs-to-be-Done (JTBD) framework; grounded in user research.
- **Avoids role-forcing**: Doesn't require naming a persona; focuses on the circumstance.
- **Better for complex products**: When the same user has different needs in different contexts, job stories capture this naturally.

### Weaknesses
- **Less widespread**: Fewer teams are familiar with the format compared to user stories.
- **No technical detail**: Same gap as user stories — no architecture, data models, or API contracts.
- **Acceptance criteria not built-in**: Must be added separately; the template doesn't enforce it.
- **Situation ambiguity**: "When I'm reviewing my spending" is context-dependent and may not be specific enough for implementation.
- **Weaker prioritization**: Without a clear role, it's harder to prioritize by user segment.

### Best Suited For
- Products with context-dependent user needs.
- UX-driven design processes.
- Early product discovery and research-informed development.

### AI-Agent Friendliness: **MEDIUM**
Similar to user stories in parseability. The situation/motivation/outcome structure gives the AI useful context about when and why a feature matters. However, the same technical gaps apply — an AI agent needs more than motivation to produce correct code. The "When" clause can help the agent understand triggering conditions, which is more actionable than a role declaration.

---

## 8. Structured Product Requirements Documents (PRD)

### Structure

```markdown
# PRD: Marketplace Seller Analytics Dashboard

## Overview
Status: Draft | Owner: Product Manager | Target: Q2 2026

## Objective
Enable sellers to self-serve analytics to reduce support tickets by 40%.
Aligns with OKR: "Improve seller retention by 15%."

## User Personas
- **Power Seller**: 100+ listings, needs bulk analytics
- **Casual Seller**: <10 listings, needs simple overview

## User Stories / Requirements
[List of user stories or requirements]

## Functional Requirements
- Dashboard loads in <2s for up to 10k data points
- Supports date range filtering (last 7d, 30d, 90d, custom)
- Export to CSV

## Non-Functional Requirements
- Mobile-responsive (iOS Safari, Chrome Android)
- WCAG 2.1 AA compliant
- 99.9% uptime SLA

## Success Metrics
- 60% seller adoption within 3 months
- Support ticket reduction: 40%

## Out of Scope
- Real-time streaming analytics
- Custom report builder

## Timeline / Milestones
- Phase 1: Core dashboard (4 weeks)
- Phase 2: Export + advanced filters (2 weeks)

## Open Questions
- Should we support multi-currency display?
```

### Strengths
- **Comprehensive**: Covers motivation, personas, requirements, constraints, success metrics, and scope boundaries.
- **Strategic alignment**: Links features to business objectives and OKRs.
- **Scope management**: Explicit "Out of Scope" section prevents scope creep.
- **Cross-functional**: Readable by product, design, engineering, and business stakeholders.
- **Success-oriented**: Defines what "done" and "successful" mean in measurable terms.

### Weaknesses
- **Heavy upfront investment**: Time-consuming to write well; risks becoming stale during development.
- **Waterfall origins**: Traditional PRDs try to capture everything upfront; conflicts with iterative development.
- **Variable quality**: No enforced structure — PRDs range from excellent to useless depending on the author.
- **Not executable**: Narrative format doesn't map directly to tests or implementation tasks.
- **Implementation gap**: Describes "what" comprehensively but often leaves "how" underspecified.

### Best Suited For
- Medium-to-large features requiring cross-team alignment.
- Product-driven organizations with PM-led spec processes.
- Features needing business justification and success metrics.

### AI-Agent Friendliness: **MEDIUM-HIGH**
A well-written PRD gives an AI agent rich context: business motivation, user personas, functional requirements, constraints, and scope boundaries. The "Out of Scope" section is particularly valuable — it tells the agent what NOT to build. Weakness: the narrative format requires NLU to extract actionable items, and quality varies wildly. Best when combined with structured acceptance criteria.

---

## 9. Gherkin / Cucumber Feature Files

### Structure

```gherkin
Feature: Shopping cart checkout
  As a customer with items in my cart
  I want to complete a purchase
  So that I receive my ordered products

  Background:
    Given I am logged in as "customer@example.com"
    And I have the following items in my cart:
      | Product      | Quantity | Price  |
      | Widget Pro   | 2        | $29.99 |
      | Cable Pack   | 1        | $9.99  |

  Scenario: Successful checkout with valid payment
    When I proceed to checkout
    And I enter valid shipping address
    And I enter valid credit card details
    And I click "Place Order"
    Then I should see "Order confirmed"
    And I should receive a confirmation email
    And my cart should be empty

  Scenario Outline: Payment validation
    When I proceed to checkout
    And I enter card number "<card>"
    Then I should see "<message>"

    Examples:
      | card             | message              |
      | 4111111111111111 | Payment accepted     |
      | 0000000000000000 | Invalid card number  |
      | expired_card     | Card expired         |
```

### Strengths
- **Executable specifications**: Feature files run directly as automated tests via Cucumber/SpecFlow.
- **Living documentation**: Tests and specs are the same artifact; they can't drift apart.
- **Data-driven**: Scenario Outlines with Examples tables enable combinatorial testing concisely.
- **Structured but readable**: Keywords (Feature, Scenario, Given/When/Then) provide machine-parseable structure while remaining human-readable.
- **Background reuse**: Common setup steps are shared across scenarios.

### Weaknesses
- **Same as GWT, amplified**: All GWT weaknesses apply, plus the overhead of maintaining step definitions.
- **Step definition maintenance**: The glue code connecting Gherkin to implementation adds significant maintenance burden.
- **Anti-pattern magnet**: Teams write imperative UI-step scenarios instead of declarative behavior scenarios.
- **Tooling dependency**: Requires Cucumber/SpecFlow/Behave runtime; adds CI complexity.
- **No architecture**: Zero information about system design, data models, or technical constraints.

### Best Suited For
- QA-heavy organizations with established BDD practices.
- Features with clear, testable user-facing behavior.
- Regulated industries requiring traceability from requirements to tests.

### AI-Agent Friendliness: **HIGH**
Gherkin is the most machine-friendly format in this list. Fixed keyword grammar, tabular data, and scenario structure make it trivially parseable. AI agents can generate step definitions, identify missing scenarios, and use feature files as both spec and verification. The main limitation: it says nothing about how to build the system, only how it should behave from the outside.

---

## 10. Design Docs (Google-style)

### Structure

```markdown
# Design Doc: Migrate User Auth to OAuth 2.0

## Context and Scope
Currently using custom session-based auth. Growing user base (2M+ users)
requires SSO support for enterprise customers. This doc covers the auth
service migration; frontend changes are out of scope.

## Goals and Non-Goals
Goals:
- Support Google and Microsoft SSO
- Maintain backward compatibility during migration
- Zero-downtime cutover

Non-Goals:
- Custom SAML provider support (future work)
- Mobile app auth changes (separate doc)

## Design

### System-Context Diagram
[Diagram showing auth service, identity providers, API gateway, user DB]

### API Changes
POST /auth/oauth/callback
  - Receives OAuth code
  - Exchanges for token
  - Creates/links user account
  - Returns session

### Data Model Changes
users table: add `oauth_provider` (enum), `oauth_id` (string, nullable)
New table: oauth_tokens (user_id, provider, access_token, refresh_token, expires_at)

### Migration Strategy
1. Deploy new OAuth endpoints alongside existing auth
2. Add OAuth login option to UI
3. Run 2 weeks dual-mode
4. Deprecate password-only login for enterprise accounts

## Alternatives Considered
- **Auth0 SaaS**: Rejected — vendor lock-in, cost at scale, data residency concerns.
- **Keycloak self-hosted**: Rejected — operational overhead, team unfamiliar.

## Cross-Cutting Concerns
- Security: Token rotation every 1h; refresh tokens encrypted at rest.
- Observability: Auth events to event bus for audit logging.
```

### Strengths
- **Comprehensive technical context**: Covers architecture, data models, APIs, migration, and trade-offs.
- **Decision rationale**: Alternatives section documents why rejected paths were rejected.
- **Scope clarity**: Goals/Non-Goals section explicitly bounds the work.
- **Diagrams encouraged**: Visual system context reduces ambiguity.
- **Organizational consensus**: Review process catches issues early.

### Weaknesses
- **Heavy**: 10-20 pages for large projects; expensive to write.
- **Drifts from reality**: Post-implementation, design docs rarely get updated.
- **No acceptance criteria**: Describes the design but not how to verify correctness.
- **Narrative format**: Not machine-parseable; requires NLU to extract actionable items.
- **Premature detail**: Risk of over-specifying implementation before learning from prototyping.

### Best Suited For
- Major architectural changes or new system components.
- Cross-team projects requiring shared understanding.
- Decisions with significant trade-offs or risk.

### AI-Agent Friendliness: **HIGH**
Design docs are arguably the richest input for an AI coding agent. They provide exactly what the agent needs: system context, data models, API contracts, migration strategy, constraints, and trade-off rationale. An agent given a good design doc can make informed architectural decisions rather than guessing. The weakness is the narrative format — the agent must extract structure from prose. A design doc + structured acceptance criteria is the strongest possible combination.

---

## 11. Emerging: Spec-Driven Development (SDD) Formats

### Overview

SDD is a 2025-2026 methodology specifically designed for AI agent workflows. Three major tools have emerged:

### Kiro (AWS)

Three-file structure: `requirements.md` -> `design.md` -> `tasks.md`

```
# requirements.md
## Requirement 1: User Registration
As a new user, I want to create an account, so that I can access the platform.

### Acceptance Criteria
- GIVEN a valid email, WHEN submitting registration, THEN account is created
- GIVEN an existing email, WHEN submitting registration, THEN error is shown

# design.md
## Components
- RegistrationForm (React component)
- AuthService (backend service)
## Data Model
users: {id, email, password_hash, created_at, verified}
## API
POST /api/auth/register {email, password} -> {user_id, token}

# tasks.md
- [ ] Create users table migration
- [ ] Implement AuthService.register()
- [ ] Create RegistrationForm component
- [ ] Add email validation
- [ ] Write integration tests
```

**Strength**: Intuitive three-phase model. **Weakness**: Verbose for small changes; "sledgehammer to crack a nut."

### GitHub Spec-Kit

Four-phase workflow: Specify -> Plan -> Tasks -> Implement. Uses `.specify/` folder with constitution, spec, plan, and task files.

**Strength**: Comprehensive checklists track quality. **Weakness**: Creates numerous repetitive markdown files; overwhelming review burden.

### Tessl

Single-file spec-to-code mapping with `@generate` and `@test` annotations.

**Strength**: Low abstraction; specs map directly to code files. **Weakness**: Non-deterministic code generation even from identical specs.

### AI-Agent Friendliness: **HIGHEST**
These formats are purpose-built for AI agent consumption. They combine user stories, acceptance criteria (GWT), technical design, and implementation tasks in a single structured workflow. The trade-off: they're heavier than traditional formats and risk replicating waterfall-era over-specification. The key insight from Thoughtworks: specifications should be "version-controlled, human-readable super prompts" — focused enough to guide the agent without overwhelming it.

---

## 12. Comparative Analysis

### Format Comparison Table

| Format | Structure Level | Intent | Constraints | Acceptance Criteria | Technical Context | AI Friendliness |
|--------|----------------|--------|-------------|--------------------|--------------------|-----------------|
| GWT/BDD | High | Low | Low | **High** | None | High |
| EARS | High | None | **High** | High | None | High |
| Natural Language | None | Medium | Low | Low | Low | Low |
| User Stories | Medium | **High** | Low | Medium | None | Medium |
| RFC | Medium | **High** | High | Low | **High** | Medium-High |
| ADR | Medium | **High** | Medium | None | Medium | Medium |
| Job Stories | Medium | **High** | Low | Low | None | Medium |
| PRD | Medium | **High** | High | Medium | Medium | Medium-High |
| Gherkin | **High** | Low | Low | **High** | None | **High** |
| Design Docs | Medium | High | High | Low | **High** | High |
| SDD (Kiro etc.) | **High** | High | High | **High** | **High** | **Highest** |

### Dimensions Explained

- **Structure Level**: How parseable/machine-readable is the format?
- **Intent**: Does it capture WHY we're building this?
- **Constraints**: Does it define boundaries, non-goals, and limitations?
- **Acceptance Criteria**: Does it define HOW WE KNOW it's done?
- **Technical Context**: Does it describe WHAT EXISTS already?
- **AI Friendliness**: How well can an AI agent use this as primary input?

---

## 13. Communication Matrix

What each format communicates well (and doesn't):

### INTENT (Why are we building this?)

| Best | Adequate | Poor |
|------|----------|------|
| User Stories, Job Stories, PRD, RFC | ADR, Design Docs | GWT, EARS, Gherkin, Natural Language |

User stories and job stories were designed to capture motivation. PRDs and RFCs include background/objective sections. GWT and EARS describe behavior/constraints without explaining why.

### CONSTRAINTS (What are the boundaries?)

| Best | Adequate | Poor |
|------|----------|------|
| EARS, Design Docs (Non-Goals), PRD (Out of Scope) | RFC, ADR | User Stories, Job Stories, GWT, Natural Language |

EARS is purpose-built for constraint expression. Design docs' non-goals section and PRDs' out-of-scope section explicitly bound the work. User stories and job stories focus on what to build, not what not to build.

### ACCEPTANCE CRITERIA (How do we know it's done?)

| Best | Adequate | Poor |
|------|----------|------|
| GWT, Gherkin, EARS | User Stories (with AC), SDD formats | RFC, ADR, Design Docs, Natural Language, Job Stories |

GWT and Gherkin are literally acceptance criteria. EARS requirements are inherently testable ("shall" statements). RFCs and design docs describe design, not verification.

### TECHNICAL CONTEXT (What exists already?)

| Best | Adequate | Poor |
|------|----------|------|
| Design Docs, RFC | ADR, PRD, SDD formats | User Stories, Job Stories, GWT, EARS, Natural Language |

Design docs include system-context diagrams, data models, and API contracts. RFCs cover architecture changes and dependencies. User-facing formats (stories, GWT) intentionally ignore internals.

---

## 14. Conclusions for AI Agent Input

### Key Findings

1. **No single format is sufficient.** Every format excels at one or two dimensions but fails at others. An effective AI agent input must combine multiple formats or create a hybrid.

2. **The strongest combination is: Design Doc + GWT acceptance criteria.** Design docs provide the richest technical context and intent. GWT provides testable, unambiguous acceptance criteria. Together they cover all four dimensions.

3. **SDD (Spec-Driven Development) formats are purpose-built for this problem** but are immature and risk over-specification. Kiro's three-file model (requirements + design + tasks) is the most promising emerging approach.

4. **Conciseness matters more than completeness.** Research shows that "providing larger context to LLMs actually drops quality." Specs should be focused, not exhaustive. A specification should be "just enough information to be effective without being overwhelming."

5. **Semi-structured > pure prose > pure formal.** The sweet spot for AI agents is semi-structured formats (markdown with headers, templates, keyword patterns) that balance human readability with machine parseability.

6. **Explicit constraints are undervalued.** The highest-impact addition to any spec format is explicit non-goals/out-of-scope/constraints. These prevent the AI agent from gold-plating and scope-creeping.

7. **EARS is underutilized.** Its template patterns are trivially machine-parseable and eliminate the ambiguity that causes AI hallucinations. Combining EARS-style constraints with user-story-style intent creates a powerful hybrid.

### Recommended Characteristics for an AI Agent Spec Format

A spec format optimized for AI agent input should include:

- **Intent section**: 2-3 sentences on WHY (borrowed from User Stories / PRD)
- **Technical context**: What exists, what's changing, key constraints (borrowed from Design Docs / RFC)
- **Behavioral requirements**: GWT-style acceptance criteria or EARS-style constraint statements
- **Explicit boundaries**: What is NOT in scope (borrowed from Design Docs non-goals / PRD out-of-scope)
- **Structured, not narrative**: Headers, templates, keyword patterns — not prose paragraphs
- **Concise**: Optimized for focused context, not exhaustive documentation

### Format Anti-Patterns for AI Agents

- Pure natural language prose (too ambiguous)
- Exhaustive PRDs trying to specify everything upfront (too much context, quality drops)
- GWT/Gherkin alone without technical context (agent must guess architecture)
- ADRs alone (decisions without requirements)
- User stories without acceptance criteria (too vague to implement)

---

## Sources

- [Thoughtworks: Spec-Driven Development](https://www.thoughtworks.com/en-us/insights/blog/agile-engineering-practices/spec-driven-development-unpacking-2025-new-engineering-practices)
- [Martin Fowler: Understanding SDD — Kiro, Spec-Kit, Tessl](https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html)
- [Aviator: Spec-Driven Development for Scalable AI Agents](https://www.aviator.co/blog/spec-driven-development-the-key-to-scalable-ai-agents/)
- [Google Design Docs at Google](https://www.industrialempathy.com/posts/design-docs-at-google/)
- [Pragmatic Engineer: RFC and Design Doc Examples](https://newsletter.pragmaticengineer.com/p/software-engineering-rfc-and-design)
- [Michael Nygard: Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
- [EARS: The Easy Approach to Requirements Syntax](https://alistairmavin.com/ears/)
- [QRA: EARS Definitive Guide](https://qracorp.com/guides_checklists/the-easy-approach-to-requirements-syntax-ears/)
- [Mountain Goat Software: Job Stories](https://www.mountaingoatsoftware.com/blog/job-stories-offer-a-viable-alternative-to-user-stories)
- [Kiro: Spec-Driven Development](https://kiro.dev/blog/kiro-and-the-future-of-software-development/)
- [BDD & Cucumber Reality Check 2025](https://www.303software.com/insights/behavior-driven-development-cucumber-testing-2025-reality)
- [Developair: Natural Language Requirements](https://www.developair.tech/double-edged-sword-natural-language-requirements/)
- [Medium: Specification Driven Development (SDD)](https://medium.com/ai-pace/specification-driven-development-sdd-ai-first-coding-practice-e8f4cc3c2fc4)
