#!/bin/bash
# Programmatic grading for react-frontend-patterns A/B test
# Usage: ./grade_programmatic.sh <outputs_dir>
# Checks assertions that can be verified via grep/regex

DIR="$1"
if [ -z "$DIR" ] || [ ! -d "$DIR" ]; then
  echo "Usage: $0 <outputs_dir>"
  exit 1
fi

echo "=== Grading: $DIR ==="
echo ""

# Combine all .ts/.tsx files recursively for analysis
ALL_CODE=$(find "$DIR" -type f \( -name "*.ts" -o -name "*.tsx" \) -exec cat {} +)

# 1. No bare useStore()
echo "--- no-bare-store ---"
BARE_STORE=$(echo "$ALL_CODE" | grep -nE '(const .+ = use\w+Store\(\s*\)|= use\w+Store\(\s*\)\s*$|{\s*\w+.*}\s*=\s*use\w+Store\(\s*\))' | grep -v '//' | grep -v 'getState' || true)
if [ -z "$BARE_STORE" ]; then
  echo "PASS: No bare useStore() found"
else
  echo "FAIL: Bare useStore() found:"
  echo "$BARE_STORE"
fi
echo ""

# 2. Server data in TanStack Query
echo "--- server-data-in-tq ---"
HAS_USEQUERY=$(echo "$ALL_CODE" | grep -c 'useQuery' || true)
HAS_USEEFFECT_FETCH=$(echo "$ALL_CODE" | grep -c 'useEffect.*fetch\|fetch.*useEffect' || true)
HAS_USESTATE_FETCH=$(echo "$ALL_CODE" | grep -cE 'useState.*\[\]|useState<.*\[\]' || true)
if [ "$HAS_USEQUERY" -gt 0 ]; then
  echo "PASS: Uses useQuery ($HAS_USEQUERY occurrences)"
else
  echo "FAIL: No useQuery found"
fi
if [ "$HAS_USEEFFECT_FETCH" -gt 0 ]; then
  echo "WARNING: Found useEffect+fetch pattern ($HAS_USEEFFECT_FETCH occurrences)"
fi
echo ""

# 3. Mutation invalidation
echo "--- mutation-invalidation ---"
HAS_USEMUTATION=$(echo "$ALL_CODE" | grep -c 'useMutation' || true)
HAS_INVALIDATE=$(echo "$ALL_CODE" | grep -c 'invalidateQueries' || true)
if [ "$HAS_USEMUTATION" -gt 0 ] && [ "$HAS_INVALIDATE" -gt 0 ]; then
  echo "PASS: useMutation with invalidateQueries found"
elif [ "$HAS_USEMUTATION" -gt 0 ]; then
  echo "FAIL: useMutation found but no invalidateQueries"
else
  echo "N/A: No mutations in this code"
fi
echo ""

# 4. Store reset method
echo "--- store-reset ---"
HAS_RESET=$(echo "$ALL_CODE" | grep -cE 'reset.*:.*\(\)|reset\s*=|reset\(\)' || true)
if [ "$HAS_RESET" -gt 0 ]; then
  echo "PASS: Store has reset method"
else
  echo "FAIL: No reset method found"
fi
echo ""

# 5. data-testid on interactive elements
echo "--- data-testid ---"
HAS_TESTID=$(echo "$ALL_CODE" | grep -c 'data-testid' || true)
if [ "$HAS_TESTID" -gt 0 ]; then
  echo "PASS: data-testid found ($HAS_TESTID occurrences)"
else
  echo "FAIL: No data-testid attributes found"
fi
echo ""

# 6. Derived state (check for cart total or count stored vs computed)
echo "--- derived-state ---"
STORED_TOTAL=$(echo "$ALL_CODE" | grep -cE 'total:\s*(number|0)|itemCount:\s*(number|0)' || true)
SELECTOR_TOTAL=$(echo "$ALL_CODE" | grep -cE 'useStore\(\(s\)|use\w+Store\(\(s\).*reduce|use\w+Store\(\(s\).*\.length' || true)
if [ "$STORED_TOTAL" -gt 0 ]; then
  echo "FAIL: Total/count appears to be stored as state ($STORED_TOTAL)"
elif [ "$SELECTOR_TOTAL" -gt 0 ]; then
  echo "PASS: Derived values computed in selectors ($SELECTOR_TOTAL)"
else
  echo "N/A: Could not determine"
fi
echo ""

echo "=== Done ==="
