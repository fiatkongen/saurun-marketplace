# Phase 2: Transform

## Purpose
Convert tasks.md task groups into executable PLAN.md files.

## Inputs (from Phase 1)
- `spec_path`: Path to spec folder
- `state_path`: Path to .long-run/ directory
- `is_resume`: Boolean indicating resume state
- `resume_from`: Plan number to resume from (if applicable)

## Workflow

### Step 1: Parse tasks.md

```
Read {spec_path}/tasks.md
Parse structure:
  - Task groups (### headers)
  - Dependencies (Dependencies: lines)
  - Subtasks (- [ ] items)
  - Acceptance criteria

Output: Array of task_group objects
```

### Step 2: Determine Plan Boundaries

```
FOR each task_group:
  IF subtask count > 5:
    Split into multiple plans (NN-a, NN-b, etc.)
  ELSE:
    One plan per task group

Update STATE.md with total plan count
```

### Step 3: Read Context

```
Read available context files:
  - {spec_path}/spec.md
  - {spec_path}/planning/requirements.md
  - {spec_path}/planning/visuals/ (if exists)

Extract:
  - Technical approach
  - File path patterns
  - Constraints and "do NOT" rules
```

### Step 4: Transform Each Task Group

```
FOR each task_group (starting from resume_from if resuming):

  # Skip if already transformed
  IF {state_path}/plans/{NN}-PLAN.md exists:
    CONTINUE

  # Transform using workflow
  result = {{workflows/long-run/transform-task-group}}
    task_group: task_group
    spec_path: spec_path
    context: parsed_context

  IF result.status == "needs_clarification":
    # Handle clarification
    Use {{workflows/long-run/clarification-protocol}}

    IF user provides clarification:
      Retry transformation
    ELSE IF user chooses skip:
      Create skipped SUMMARY.md
      CONTINUE
    ELSE IF user chooses exit:
      Save state and STOP

  # Validate enrichment
  validation = {{workflows/long-run/validate-enrichment}}
    tasks: result.tasks

  IF validation has critical errors:
    Trigger clarification protocol
```

### Step 5: Validate Dependencies

```
Build dependency graph from all plans
Check for circular dependencies

IF cycle detected:
  Present error: "Circular dependency: {cycle_path}"
  Offer options to resolve
  STOP
```

### Step 6: Update State

```
Update STATE.md:
  - Status: Transforming → Executing
  - Plan count: {total_plans}
  - Progress bar

Output to user:
  "✅ Transformation complete

   Plans created: {count}
   Location: {state_path}/plans/

   Proceeding to execution phase..."
```

### Step 7: Output

```
Pass to Phase 3:
  - plans: Array of plan paths
  - dependencies: Dependency graph
  - spec_path
  - state_path
```

## Task Group Parsing

### Input Format (tasks.md)
```markdown
#### Task Group 2: API Endpoints
**Dependencies:** Task Group 1

- [ ] 2.0 Complete API layer
  - [ ] 2.1 Write 2-8 focused tests
  - [ ] 2.2 Create controller
  - [ ] 2.3 Implement auth

**Acceptance Criteria:**
- All CRUD operations work
- Proper authorization enforced
```

### Parsed Object
```javascript
{
  number: "02",
  name: "API Endpoints",
  dependencies: ["01"],
  subtasks: [
    { id: "2.0", text: "Complete API layer", children: [...] },
    { id: "2.1", text: "Write 2-8 focused tests" },
    { id: "2.2", text: "Create controller" },
    { id: "2.3", text: "Implement auth" }
  ],
  acceptance_criteria: [
    "All CRUD operations work",
    "Proper authorization enforced"
  ]
}
```

## Error Handling

```
ON transformation error:
  Log error details
  IF recoverable: Retry
  IF not recoverable:
    Mark task group as needs-clarification
    Trigger clarification protocol
```
