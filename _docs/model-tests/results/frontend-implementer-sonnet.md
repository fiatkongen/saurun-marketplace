TDD workflow demonstrated: test file written BEFORE component implementation.

Tests: 8 tests in 1 describe block using Vitest + RTL + MSW.
- Renders all form fields with correct data-testid
- Email empty validation
- Email format validation
- Role not selected validation
- Loading state during submission
- Success message on completion
- Error message on failure
- Clear form after success

Component:
- react-hook-form + zod + shadcn Form component
- useMutation from TanStack Query
- Loading: button disabled + "Sending..." text
- Success: data-testid="team-invite-success-message" + data-asset="icon-success"
- Error: data-testid="team-invite-error-message" + data-asset="icon-error"
- Form reset via onSuccess callback

data-testid attributes:
- team-invite-email-input, team-invite-role-select, team-invite-submit-button
- team-invite-success-message, team-invite-success-icon
- team-invite-error-message, team-invite-error-icon

Tailwind v4 Syntax:
- CSS variable with parentheses: max-w-(--container-sm)
- cn() for class merging
- size-5 sizing

Uses shadcn Select (Radix-based):
- Tests use user.click(trigger) then screen.getByRole('option', { name }) pattern
- Potential test reliability issue with Radix portals

Uses shadcn Form component (FormField, FormItem, FormLabel, FormControl, FormMessage).

data-asset placeholders used for icons (not placeholder.com).
MSW for API mocking (no vi.mock).

Issues:
- Tests rely on Radix Select portal rendering (user.click trigger → getByRole('option')) — may be flaky in CI
- No loading indicator data-testid (checks button text "Sending" instead)
