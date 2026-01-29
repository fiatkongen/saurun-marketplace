#!/usr/bin/env python3
"""
Move a task between sections in a markdown task file.
"""

import os
import sys
from pathlib import Path
import re


def move_task(file_path: str, task_text: str, from_section: str, to_section: str, note: str = None):
    """
    Move a task from one section to another.

    Args:
        file_path: Path to the markdown task file
        task_text: The task text to move (partial match)
        from_section: Source section (pending, in_progress, completed, failed)
        to_section: Destination section
        note: Optional note to append to the task
    """
    content = Path(file_path).read_text()
    lines = content.split('\n')

    # Normalize section names
    section_map = {
        'pending': '## Pending',
        'in_progress': '## In Progress',
        'completed': '## Completed',
        'failed': '## Failed'
    }

    from_header = section_map.get(from_section.lower().replace(' ', '_'))
    to_header = section_map.get(to_section.lower().replace(' ', '_'))

    if not from_header or not to_header:
        print(f"Error: Invalid section name")
        sys.exit(1)

    # Find and extract the task
    task_line = None
    task_line_idx = None
    in_section = False

    for idx, line in enumerate(lines):
        # Track current section
        if line.startswith('## '):
            in_section = (line == from_header)
            continue

        # Find matching task in source section
        if in_section and line.strip().startswith('-') and task_text.lower() in line.lower():
            task_line = line
            task_line_idx = idx
            break

    if not task_line:
        print(f"Error: Task not found in {from_section} section")
        sys.exit(1)

    # Add note if provided
    if note:
        task_line = task_line.rstrip() + f" ({note})"

    # Remove from source section
    lines.pop(task_line_idx)

    # Find destination section and add task
    for idx, line in enumerate(lines):
        if line == to_header:
            # Insert after header (skip empty line if present)
            insert_idx = idx + 1
            if insert_idx < len(lines) and lines[insert_idx].strip() == '':
                insert_idx += 1
            lines.insert(insert_idx, task_line)
            break

    # Write back
    Path(file_path).write_text('\n'.join(lines))
    print(f"✅ Moved task to {to_section}")


if __name__ == '__main__':
    if len(sys.argv) < 4:
        print("Usage: move_task.py [file] <task_text> <from_section> <to_section> [note]")
        print("  If no file specified, uses ~/tasks.md")
        print("Example: move_task.py 'Run tests' pending in_progress")
        print("Example: move_task.py tasks.md 'Run tests' pending in_progress")
        sys.exit(1)

    # Check if first argument is a file path or task text
    # If it contains .md or is an existing file, treat as file path
    if len(sys.argv) >= 5 and ('.md' in sys.argv[1] or Path(sys.argv[1]).exists()):
        file_path = sys.argv[1]
        task_text = sys.argv[2]
        from_section = sys.argv[3]
        to_section = sys.argv[4]
        note = sys.argv[5] if len(sys.argv) > 5 else None
    else:
        # Use default path ~/tasks.md
        file_path = os.path.expanduser("~/tasks.md")
        task_text = sys.argv[1]
        from_section = sys.argv[2]
        to_section = sys.argv[3]
        note = sys.argv[4] if len(sys.argv) > 4 else None
        print(f"Using default task file: {file_path}")

    if not Path(file_path).exists():
        print(f"Error: Task file not found: {file_path}")
        sys.exit(1)

    move_task(file_path, task_text, from_section, to_section, note)
