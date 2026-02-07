# Interview Protocol

Runs in Step 6. Adaptive AskUserQuestion sessions in main context.

## Core Principles

- **Runs in main context** -- AskUserQuestion only works there
- **One question per message** -- don't overwhelm
- **Multiple choice preferred** -- use AskUserQuestion options (easier to answer)
- **Informed by research** -- reference findings when asking about technical decisions
- **Senior architect mindset** -- ask about failure modes, scale, edge cases, not just happy path
- **Don't ask the obvious** -- skip questions already answered in the input/spec
- **Dig deeper** -- when answers reveal complexity, follow up

## Adaptive Depth

Based on completeness score from Step 3 (see `completeness-dimensions.md`):

### FULL Interview (score 0-2): 5-8 rounds
- Cover ALL uncovered dimensions
- Plus: edge cases, failure modes, scale considerations
- Plus: technical preferences beyond stack defaults
- Plus: business constraints, timeline, priorities
- Explore each dimension thoroughly before moving to next

### LIGHT Interview (score 3-5): 2-3 rounds
- Focus ONLY on uncovered dimensions
- Clarify ambiguities in covered dimensions
- Don't re-ask what's already clear
- Batch related questions when possible

### VALIDATION Interview (score 6-7): 1 round
- Present understanding: "Here's my understanding -- anything wrong?"
- List key assumptions for confirmation
- Single AskUserQuestion with "Looks correct" as first option

## Question Design

Structure each AskUserQuestion with:
- Clear, specific question text
- 2-4 concrete options (not vague)
- Options ordered by likelihood/recommendation
- "Other" is automatically provided by AskUserQuestion

### Example Questions by Dimension

**Problem:** "What specific pain point does this solve?"
- Options based on common patterns for the domain

**Target Users:** "Who are the primary user types?"
- Options: named personas derived from input context

**Core Features:** "Which features are must-have for MVP?"
- Multi-select from features implied by input

**Entities:** "What are the key domain objects?"
- Options: entities implied by features + common domain patterns

**User Flows:** "What's the primary user journey?"
- Options: 2-3 flow patterns common for this type of app

**Constraints:** "Any specific constraints?"
- Multi-select: mobile-first, offline support, GDPR, i18n, accessibility, performance SLA

**Visual/UX:** "What visual style fits this product?"
- Options: 3-4 style directions that match the domain

## Stop Criteria

Stop the interview when:
1. All 7 dimensions covered with actionable detail, OR
2. User predominantly answers "I don't know" or "Up to you" (can't extract more), OR
3. Maximum rounds reached for the depth level

## Coverage Test

For each dimension, ask: **"Could an implementer make a specific decision from this detail without asking follow-ups?"**
- YES -> dimension is covered
- NO -> keep probing

## Output

Write Q&A to `.god-agent/brainstorm-qa.md` using God-Agent's Q&A template (from `god-agent/references/qa-template.md`).

Each answer's **Source** field uses values from God-Agent's qa-template:
- `Injected context` -- was in the original input or user provided during interview
- `Project context` -- derived from existing codebase analysis (extension mode)
- `Stack defaults` -- standard stack choice (.NET 9, React 19, etc.)
- `Assumption` -- inferred, not confirmed (mark clearly)

Note: `User interview` answers map to `Injected context` (user-provided info).
Research findings that inform an answer should be noted in the answer text,
with the Source set to whichever category best fits the final decision.
