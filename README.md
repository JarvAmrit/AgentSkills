# AgentSkills

A curated library of reusable **GitHub Copilot agent skills** built to enterprise standards. Each skill is a self-contained instruction file (`.github/agents/<skill>.md`) that drives a complete, interactive workflow directly inside GitHub Copilot.

---

## 🗂️ Available Skills

### 1. `azdo-implement` — Azure DevOps Work Item Implementer

Fetches a work item from Azure DevOps and drives the full implementation lifecycle end-to-end.

**What it does:**
- Fetches User Story or Bug details from Azure DevOps via the REST API
- Creates a feature or fix branch using the correct enterprise naming convention
- Searches the codebase and implements the required code changes
- Generates and runs unit tests
- Opens a draft pull request linked to the work item
- Optionally generates functional test cases and uploads them to Azure DevOps Test Plans

**Trigger phrase (in Copilot):**
```
@workspace /azdo-implement
```

---

### 2. `azdo-create-workitem` — Azure DevOps Work Item Creator

Guides you through creating a User Story, Bug, or Task in Azure DevOps via a fully conversational, selectable-UI-driven flow.

**What it does:**
- Gathers requirements through a conversational Q&A flow
- Drafts the work item with title, description, and acceptance criteria
- Maps it to the correct Program Increment, Sprint, and Feature
- Creates the work item via the Azure DevOps REST API
- Optionally hands off directly to `azdo-implement` for immediate implementation

**Trigger phrase (in Copilot):**
```
@workspace /azdo-create-workitem
```

---

## ⚙️ Setup

All skills use `curl` with Personal Access Token (PAT) authentication to call the Azure DevOps REST API. Before first use, configure the following placeholders in each skill file:

| Placeholder    | Description                                      |
|----------------|--------------------------------------------------|
| `YOUR_ORG`     | Your Azure DevOps organization name              |
| `YOUR_PROJECT` | Your Azure DevOps project name                   |
| `YOUR_PAT`     | A PAT with Work Items (Read & Write) permissions |

---

## 📁 Repository Structure

```
.github/
  agents/
    azdo-implement.md        # Work item implementation skill
    azdo-create-workitem.md  # Work item creation skill
```

---

## 🤝 Contributing

New skills should follow the same conventions:
- One Markdown file per skill under `.github/agents/`
- Self-contained: no external scripts or dependencies beyond `curl`
- Interactive: use selectable UI prompts for key decision points
- End with a full completion summary

---

## 📄 License

[MIT](LICENSE)
