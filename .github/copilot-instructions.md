# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Async Programming
- When methods return `IAsyncEnumerable<T>`, use the `[EnumeratorCancellation]` attribute from `System.Runtime.CompilerServices` for the `CancellationToken` parameter to properly propagate cancellation through the async enumerable chain.