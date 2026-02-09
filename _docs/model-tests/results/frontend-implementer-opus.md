TDD workflow demonstrated: test file written BEFORE component implementation.

Tests: 9 tests in 1 describe block using Vitest + RTL + MSW.
- Renders all form fields
- Email required validation
- Email format validation
- Role required validation
- Loading state during submission
- Success message on completion
- Error message on failure
- Form reset after success
- Correct POST payload verification

Component:
- react-hook-form + zod for validation
- useMutation from TanStack Query
- Loading: button disabled + spinner with data-testid="invite-loading-indicator"
- Success: data-testid="invite-success-message" + data-asset="icon-success"
- Error: data-testid="invite-error-message" + data-asset="icon-error"
- Form reset via onSuccess callback

data-testid attributes:
- invite-email-input, invite-role-select, invite-submit-button
- invite-loading-indicator, invite-email-error, invite-role-error
- invite-success-message, invite-success-icon
- invite-error-message, invite-error-icon

Tailwind v4 Syntax:
- CSS variables with parentheses: border-(--border), bg-(--card), text-(--card-foreground), bg-(--background), ring-(--ring)
- cn() for class merging (all classes)
- size-4, size-8 sizing

Key Decision: Native <select> instead of shadcn Select for test reliability (Radix portals make userEvent.selectOptions unreliable). Styled with Tailwind to match shadcn aesthetics.

data-asset placeholders used for icons (not placeholder.com).
MSW for API mocking (no vi.mock).
Separate schema file (schema.ts) with zod validation.
