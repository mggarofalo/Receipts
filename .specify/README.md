# SpecKit Configuration

This directory contains the [GitHub Spec Kit](https://github.com/github/spec-kit) configuration for specification-driven development.

## Quick Start

### 1. Creating a New Feature Specification

```bash
# Create feature directory
mkdir -p .specify/specs/REC-XXX

# Copy templates
cp .specify/templates/spec-template.md .specify/specs/REC-XXX/spec.md
cp .specify/templates/plan-template.md .specify/specs/REC-XXX/plan.md
cp .specify/templates/tasks-template.md .specify/specs/REC-XXX/tasks.md
```

### 2. Writing the Specification

1. Open `.specify/specs/REC-XXX/spec.md`
2. Define user stories with acceptance criteria
3. Document business rules and data requirements
4. Mark any unclear items with `[NEEDS CLARIFICATION]`

### 3. Creating the Technical Plan

1. Open `.specify/specs/REC-XXX/plan.md`
2. Verify constitution compliance
3. Define changes for each layer (Domain, Application, Infrastructure, Presentation)
4. Document API contracts and test strategy

### 4. Breaking Down into Tasks

1. Open `.specify/specs/REC-XXX/tasks.md`
2. Create ordered, executable tasks
3. Mark parallelizable tasks with `[P]`
4. Note dependencies between tasks

### 5. Implementation

Follow the task breakdown, ensuring:
- Tests are written before implementation
- Constitution principles are followed
- Each task is completed before moving to the next

## Directory Structure

```
.specify/
├── README.md                # This file
├── memory/
│   └── constitution.md      # Project principles (READ FIRST)
├── specs/
│   └── [FEATURE]/           # Feature specifications
│       ├── spec.md
│       ├── plan.md
│       └── tasks.md
└── templates/
    ├── spec-template.md     # Specification template
    ├── plan-template.md     # Technical plan template
    ├── tasks-template.md    # Task breakdown template
    └── CLAUDE-template.md   # AI assistant context
```

## Constitution

The constitution (`.specify/memory/constitution.md`) defines non-negotiable principles for this project. All specifications and implementations must comply with these articles.

Key principles:
- Clean Architecture with strict layer separation
- CQRS pattern using MediatR
- Test-first development
- Domain, Entity, and ViewModel separation
- Repository pattern for data access
- Conventional Commits for version control

## Resources

- [GitHub Spec Kit Repository](https://github.com/github/spec-kit)
- [Spec-Driven Development Guide](https://github.com/github/spec-kit/blob/main/spec-driven.md)
- [Project AGENTS.md](../AGENTS.md)
