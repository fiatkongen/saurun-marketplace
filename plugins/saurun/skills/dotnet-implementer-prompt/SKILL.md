---
name: dotnet-implementer-prompt
description: Use when breaking a .NET implementation plan into tasks for subagent execution
---

# .NET Implementer Subagent Prompt Template

## Overview

A fill-in prompt template for dispatching .NET implementer subagents. Ensures every subagent gets complete context, follows TDD, self-reviews, and reports consistently.

**Core principle:** The subagent should never need to read the plan file or guess context — everything it needs is in the prompt.

## When to Use

- Dispatching a subagent for a .NET backend implementation task from a plan
- Breaking an implementation plan into parallelizable units of work
- Any task requiring TDD, self-review, and structured reporting

## When NOT to Use

- Frontend-only tasks (use a frontend implementer template)
- One-off questions or investigations (just ask directly)
- Tasks with no implementation (e.g., pure documentation, plan writing)

## Common Mistakes

- **Not pasting full task text** — writing "see task 3 in plan" instead of pasting the actual spec. The subagent cannot read plan files.
- **Vague context** — saying "you know the project" instead of specifying entity names, existing patterns, and architectural layer.
- **Forgetting working directory** — subagent starts in wrong directory, wastes cycles figuring out project structure.
- **Not specifying expected test failures** — if you know what the RED step should look like, tell the subagent so it can verify.
- **Omitting dependencies between tasks** — not mentioning that Task 2 depends on types introduced in Task 1.

## Template

```
Task tool (your backend implementer agent, e.g. saurun:backend-implementer, or general-purpose):
  description: "Implement Task N: [task name]"
  prompt: |
    You are implementing Task N: [task name]

    ## Task Description

    [FULL TEXT of task from plan - paste it here, don't make subagent read file]

    ## Context

    [Scene-setting: where this fits, dependencies, architectural context]

    ## Before You Begin

    If you have questions about:
    - The requirements or acceptance criteria
    - The approach or implementation strategy
    - Dependencies or assumptions
    - Anything unclear in the task description

    **Ask them now.** Raise any concerns before starting work.

    ## Your Job

    Once you're clear on requirements:
    1. Implement exactly what the task specifies
    2. **REQUIRED SUB-SKILL:** Follow saurun:dotnet-tdd strictly. No exceptions.
    3. Verify implementation works: `dotnet test`
    4. Commit your work
    5. Self-review (see below)
    6. Report back

    Work from: [directory]

    **While you work:** If you encounter something unexpected or unclear, **ask questions**.
    It's always OK to pause and clarify. Don't guess or make assumptions.

    ## TDD Workflow

    **REQUIRED SUB-SKILL:** Follow saurun:dotnet-tdd for the full Red-Green-Refactor cycle. No shortcuts, no skipping steps.

    ## Before Reporting Back: Self-Review

    Review your work with fresh eyes. Ask yourself:

    **Completeness:**
    - Did I fully implement everything in the spec?
    - Did I miss any requirements?
    - Are there edge cases I didn't handle?

    **Quality:**
    - Is this my best work?
    - Are names clear and accurate (match what things do, not how they work)?
    - Is the code clean and maintainable?

    **Discipline:**
    - Did I avoid overbuilding (YAGNI)?
    - Did I only build what was requested?
    - Did I follow existing patterns in the codebase?

    **Testing:**
    - Did I follow TDD? (test first, watch fail, minimal code, watch pass, refactor)
    - Would each test catch a real bug? If not, delete it.
    - **REFERENCE:** See saurun:dotnet-tdd verification checklist for complete test quality criteria.

    If you find issues during self-review, fix them now before reporting.

    ## Report Format

    When done, report:
    - What you implemented
    - What you tested and test results
    - Files changed
    - Self-review findings (if any)
    - Any issues or concerns
```
