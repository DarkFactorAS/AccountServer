---
toolRestrictions: []
maxParallelToolCalls: 20
---

# Project Agent Configuration: Account Server

## Purpose

This agent.md file defines the agent's role, coding standards, and conventions
for the Account Server. It serves as a guide for automated agents and
contributors working in this repository.

## Goals for this app

- This app is a back-end server to handle all account logins from other applications. 
  The AccountClient is an interface that the external application can implement by importing the nuget packet with it
- The api's requires the external server to authenticate with OAUTH2 server-to-server before handling
  any client account login
- All data received from the client is done through the controllers.
- The instructions should be comprehensive and clear, guiding GitHub Copilot
  to best practices for all developers.
- The gameserver requires authentication for most of its API functions
- Instructions should be adaptable to different repositories, scenarios, and
  user needs.
- You have the flexibility to adjust the content based on the repository in
  context and the user's requirements.

---

## Agent Role

- Maintain and extend the codebase, focusing on reliability,
  maintainability, and security.
- Automate repetitive tasks such as builds, tests, and deployments.
- Enforce code quality and project conventions.

## Coding Standards

- Language: C# (dotnet)
- Follow .NET best practices for structure, naming, and error handling.
- Use async/await for I/O-bound operations.
- Prefer dependency injection for service management.
- Write clear, concise, and well-documented code.

## Project Conventions

- All configuration files are in the config folder.
- Controllers, Providers, and Repositories are separated by folder.
- Use tasks defined in tasks.json for build and run operations.
- Automated tests are in DFWeb.FR.Tests and DFWeb.BE.Tests

## Build & Run

- Use the provided build tasks (e.g., build, build_account, publish, watch)
  for compiling and running the project.
- Docker support is available via docker-up.sh and docker-down.sh scripts.

## Agent Instructions

- When modifying code, ensure all related tests pass.
- Update documentation and configuration as needed for new features or
  changes.
- Maintain compatibility with existing deployment scripts and Docker
  configurations.

## Git Workflow Requirements

- At the start of every new session, create and switch to a new Git branch
  before making changes.
- Use a descriptive branch name that reflects the session scope (for example:
  `session/feature-name` or `session/bugfix-name`).
- If the `jira-task-ingest` skill is involved when starting a new session,
  name the branch after the Jira task and include the Jira key (for example:
  `session/DF-123-short-description`).
- After each resolved action, commit the completed code to the current session
  branch with a clear commit message.
- Continue committing incrementally after each resolved action rather than
  batching multiple completed actions into a single commit.
- Before starting a Pull Request make sure all unittests and docker containers build and run


## Related Repositories

- [Bots Client](https://github.com/DarkFactorAS/Bots.git)

## Contact

For questions or contributions, refer to the repository maintainers or open an issue.
