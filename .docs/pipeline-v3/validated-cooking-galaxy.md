# GWT vs EARS+Properties Spec Format Experiment

## Context
Pipeline-v3 bets 70% of effort on spec quality. GWT is assumed but unproven for AI agents. EARS + property descriptions may be more token-efficient and equally complete. This experiment compares both formats on 5 features of escalating complexity.

## Plan
1. Create `spec-format-experiment` skill with orchestrator + reference files
2. Define 5 test features (bookmark CRUD → recipe search)
3. Two spec writer prompts (GWT vs EARS+properties)
4. Scoring rubric: completeness, precision, tokens, implementer success, consistency, readability
5. Experiment protocol: generate specs → score → implement → measure → verdict

## Files to Create
- `plugins/saurun/skills/spec-format-experiment/SKILL.md`
- `plugins/saurun/skills/spec-format-experiment/references/gwt-spec-writer-prompt.md`
- `plugins/saurun/skills/spec-format-experiment/references/ears-spec-writer-prompt.md`
- `plugins/saurun/skills/spec-format-experiment/references/feature-descriptions.md`
- `plugins/saurun/skills/spec-format-experiment/references/scoring-rubric.md`
- `plugins/saurun/skills/spec-format-experiment/references/experiment-protocol.md`

## Verification
- Skill loads via `/spec-format-experiment`
- All reference files are accessible from skill body
- Run spec generation for Feature 1 as smoke test
