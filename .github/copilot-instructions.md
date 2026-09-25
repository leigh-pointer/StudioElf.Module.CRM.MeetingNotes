# Copilot Instructions

## AI Instruction Entry Point (Copilot)

This file is the entry point for GitHub Copilot in this repository.

It does not define governance rules.

This repository is the Meeting Notes extension for StudioElf CRM. It is governed by two
file-backed sets, and neither one may be worked around:

1. The Oqtane AI Playbook, reached at
   `..\..\..\oqtane-ai-playbook\module-playbook-example\docs\governance\027-rules-index.md`
   with its prompts under `..\..\..\oqtane-ai-playbook\module-playbook-example\docs\prompts\`
2. The CRMX extension rule set, indexed at
   `..\..\docs\governance-crm\CRMX-000-index.md` (a synced copy; the canonical set lives in
   the CRM core repository, and no file in the copy is ever edited here)

Before generating or modifying code, you MUST read and apply:

- `..\..\docs\governance-crm\CRMX-000-index.md` and every rule it indexes that the task touches
- `.github\module-instructions.md`
- the Playbook documents the task requires

## Copilot Memories

When Copilot detects a Memory ("Memory Detected") it should add it to the
`module-instructions.md` file in the `/.github` folder.

## Governance Declaration (Mandatory)

This repository is governed by the Oqtane AI Playbook and the CRMX extension rule set.

Before generating, modifying, or refactoring code, the AI must:

- Acknowledge that it is operating under Oqtane AI Playbook and CRMX governance
- This is an Oqtane module extension for StudioElf CRM.
- You are NOT allowed to modify Oqtane.Server, Oqtane.Client, Oqtane.Shared, any Oqtane
  framework project, or any file in the CRM core repository. The only tree this repository
  may write is its own project folder. Full rule: `CRMX-013`.
- The module must be fully self-contained.
- Apply all referenced governance rules
- Refuse any request that would violate a governed rule
- Ask for clarification if governance context is unclear

If governance cannot be applied, the AI must stop and explain why.
