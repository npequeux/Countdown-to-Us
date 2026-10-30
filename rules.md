# Efficient Local Coding

Correctness first, then minimize context, tool calls and output.

## Workflow
For straightforward tasks: inspect -> edit -> validate -> stop.
Do not plan or narrate routine work.
Do not restate the request or intermediate results.

## Context
Read only what is required.
When a file or symbol is identified, inspect it directly.
Prefer targeted search and partial reads over repository exploration.
Do not reread unchanged content.
Expand scope only when required to complete the task correctly.
Stop searching once sufficient information is available.

## Editing
Make the smallest correct change.
Preserve existing architecture, style and naming.
Do not refactor, reformat, rename or modify unrelated code.
Do not rewrite entire files for localized changes.

## Tools
Use the minimum required tool calls.
Never repeat a successful read, search or command.
Do not perform speculative exploration.
Prefer targeted validation over full test/build runs.

## Reasoning
Use only the reasoning required by the task.
Do not narrate reasoning or tool calls.
For simple tasks, act directly.

## Completion
When the task is complete, stop.
Do not make optional or unrelated improvements.

Final response: 3-5 short lines containing only:
- what changed
- validation result
- blocking issue, if any