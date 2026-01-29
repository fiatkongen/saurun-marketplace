#!/usr/bin/env python3
"""
Parse markdown task file and extract structured task information.
"""

import re
import sys
from pathlib import Path
from typing import List, Dict, Optional


def parse_task_file(file_path: str) -> Dict[str, List[Dict]]:
    """
    Parse a markdown task file with sections: Pending, In Progress, Completed, Failed.

    Returns:
        Dict with keys: pending, in_progress, completed, failed
        Each value is a list of task dicts with: text, depends_on, blocked_by
    """
    content = Path(file_path).read_text()

    sections = {
        'pending': [],
        'in_progress': [],
        'completed': [],
        'failed': []
    }

    # Split by section headers
    current_section = None
    for line in content.split('\n'):
        # Match section headers (case insensitive)
        if re.match(r'^##\s+(pending|in progress|completed|failed)', line, re.IGNORECASE):
            section_name = line.strip('#').strip().lower().replace(' ', '_')
            current_section = section_name
            continue

        # Parse task items
        if current_section and line.strip().startswith('-'):
            task_text = line.strip('- ').strip()

            # Extract dependencies
            depends_match = re.search(r'\(Depends on: ([^)]+)\)', task_text)
            depends_on = depends_match.group(1) if depends_match else None

            # Extract blockers
            blocked_match = re.search(r'\(Blocked by: ([^)]+)\)', task_text)
            blocked_by = blocked_match.group(1) if blocked_match else None

            sections[current_section].append({
                'text': task_text,
                'depends_on': depends_on,
                'blocked_by': blocked_by
            })

    return sections


def find_unblocked_tasks(sections: Dict[str, List[Dict]]) -> List[Dict]:
    """
    Find all pending tasks that are not blocked.

    A task is blocked if:
    - It has a "Depends on: X" and X is not in completed section
    - It has a "Blocked by: X" annotation
    """
    completed_tasks = {task['text'] for task in sections['completed']}
    unblocked = []

    for task in sections['pending']:
        # Check explicit dependencies
        if task['depends_on']:
            # Simple check: if dependency text appears in completed
            is_dependency_met = any(
                task['depends_on'].lower() in completed['text'].lower()
                for completed in sections['completed']
            )
            if not is_dependency_met:
                continue

        # Check for blocker annotations
        if task['blocked_by']:
            continue

        unblocked.append(task)

    return unblocked


if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("Usage: parse_tasks.py <task_file.md>")
        sys.exit(1)

    file_path = sys.argv[1]
    sections = parse_task_file(file_path)

    print("=== Task Summary ===")
    print(f"Pending: {len(sections['pending'])}")
    print(f"In Progress: {len(sections['in_progress'])}")
    print(f"Completed: {len(sections['completed'])}")
    print(f"Failed: {len(sections['failed'])}")
    print()

    unblocked = find_unblocked_tasks(sections)
    print(f"Unblocked tasks ready for execution: {len(unblocked)}")

    if unblocked:
        print("\nNext available task:")
        print(f"  - {unblocked[0]['text']}")
