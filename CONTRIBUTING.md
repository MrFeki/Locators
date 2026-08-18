# Contributing Guidelines

Welcome. This repository follows strict coding and testing standards to ensure maintainability and quality.

## Standards

- Follow the rules in `.editorconfig` (indentation, naming, formatting). Changes that violate `.editorconfig` will be rejected.
- Use SOLID, DRY, KISS, and YAGNI principles.
- Prefer simple, well-tested solutions. Avoid premature abstraction.

## Project Structure

The solution must be split into layers:

- `Core` - Framework-agnostic utilities and abstractions (logging interfaces, browser factory, base classes).
- `Business` - Page objects and application-specific logic.
- `Tests` - Automated tests and TAF configuration.

Each project must target .NET 8.

## Naming

- Public types and methods: `PascalCase`.
- Private fields: `_camelCase` (leading underscore required).
- Local variables: `camelCase`.

## Tests

- Use NUnit for tests.
- Tests must derive from an abstract base class that handles setup and teardown (browser initialization, logging, screenshots on failure).
- Use the Browser Factory (Singleton/Factory pattern) for WebDriver creation.
- Keep tests independent and idempotent.

## Logging

- Use Serilog, NLog, or log4net. Configure logging to both console and file.
- Minimum log level must be configurable via `appsettings.json`.
- Take a screenshot on test failure; filename must include date and time.

## Pull Requests

- Create small, focused PRs.
- Include unit/integration tests for new functionality.
- Ensure all tests pass locally.

## Code Reviews

- Reviewers should validate compliance with `.editorconfig` and these guidelines.

## Formatting

- Run `dotnet format` before committing.

---
By contributing you agree to follow the rules above. Thank you.