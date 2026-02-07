# Skill & Agent Model Assignments

Updated: 2026-02-07 | Based on A/B test results in `_docs/model-tests/results/master-report.md`

## Applied Changes

| Skill/Agent | Model | Test Result | Category |
|-------------|:-----:|:-----------:|----------|
| codex-bridge | **haiku** | 6/6 GREEN | Routing decision |
| writing-plans (router) | **haiku** | 6/6 GREEN | Routing decision |
| test-deploy | **haiku** | 3/3 GREEN | No-op confirmation |
| react-tdd | **sonnet** | 8/8 GREEN | Discipline enforcement |
| deploy-to-marketplace | **sonnet** | 9/9 GREEN | Multi-step workflow |

## Proposed but Not Applied

These passed A/B testing but were kept on Opus by decision.

| Skill/Agent | Proposed | Test Result | Category | Reason |
|-------------|:--------:|:-----------:|----------|--------|
| react-implementer-prompt | haiku | 9/9 GREEN | Template filling | Dispatches subagents — model would inherit |
| dotnet-implementer-prompt | haiku | 9/9 GREEN | Template filling | Dispatches subagents — model would inherit |
| dotnet-code-quality-reviewer-prompt | haiku | 10/10 GREEN | Template filling | Dispatches code-reviewer — model would inherit |
| react-code-quality-reviewer-prompt | haiku | 11/11 GREEN | Template filling | Dispatches code-reviewer — model would inherit |
| react-tailwind-v4-components | haiku | 9/9 GREEN | Pattern application | User decision |
| dotnet-tactical-ddd | sonnet | 8/8 GREEN | Pattern application | User decision |
| react-frontend-patterns | sonnet | 8/8 GREEN | Pattern application | User decision |
| dotnet-tdd | sonnet | 8/8 GREEN | Discipline enforcement | User decision |
| react-writing-plans | sonnet | 7/7 GREEN | Planning | User decision |
| product-manager | sonnet | 8/8 GREEN | Standalone agent | User decision |
| codex-validator | sonnet | 6/6 GREEN | Standalone agent | User decision |
| backend-implementer | sonnet | 9/9 GREEN | Agent + 2 skills | User decision |
| frontend-implementer | sonnet | 10/10 GREEN | Agent + 4 skills | User decision |

## Failed A/B Test (Stays on Opus)

| Skill/Agent | Proposed | Test Result | Reason |
|-------------|:--------:|:-----------:|--------|
| dotnet-writing-plans | sonnet | 7/8 GREEN, 2 RED | Repeats validation rules inline instead of referencing architecture |

## Exempted (Not Tested)

| Skill/Agent | Reason |
|-------------|--------|
| requirement-verifier | Numeric mutation detection too demanding for Haiku |
| dansk-qa | Danish language quality requires native-level judgment |
| autonomous-task-executor | Complex multi-step orchestration; not suitable for A/B format |
