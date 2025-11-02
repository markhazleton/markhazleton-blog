# Test Driving GitHub's Spec Kit

*Learn how GitHub’s Spec Kit transforms AI coding from “vibe coding” to rigorous spec-driven development. A practical case study building and shipping a .NET NuGet package with measurable results.*

**Category:** Case Studies

---

## Executive Summary

Large language models are astonishingly capable, but they’re also famously inconsistent when used without guardrails. GitHub’s Spec Kit tackles this by moving work from ad hoc prompting to a structured, spec-driven workflow that Copilot (and other LLMs) can reliably follow. In this article, I test-drive Spec Kit by building, testing, and publishing a real .NET NuGet package. You’ll see the spec format, the repository layout, the automation, and the results—what worked, what didn’t, and how to adopt this approach in your own organization.

- What you’ll learn:
  - How to write an AI-ready specification that reduces ambiguity and rework
  - How to structure a .NET library repo for reproducible AI contributions
  - How to drive code generation, tests, and packaging from the spec
  - How to iterate safely when the model gets it wrong
  - Practical CI/CD setup for NuGet publishing

- Who this is for:
  - Engineering leaders aiming to productize AI coding
  - Individual contributors trialing Copilot with higher reliability
  - Teams shipping .NET libraries or SDKs

References:
- GitHub Copilot: https://github.com/features/copilot
- Copilot Workspace: https://github.com/features/copilot-workspace
- GitHub Actions: https://docs.github.com/actions
- NuGet Publishing: https://learn.microsoft.com/nuget/create-packages/publish-a-package

---

## From Vibe Coding to Spec-Driven Development

“Vibe coding” is when you toss a few prompts at an LLM and hope for the best. It’s fast and occasionally dazzling—but brittle at scale. The missing piece is a shared artifact that translates product intent into implementable and testable work. That’s the job of a spec.

GitHub’s Spec Kit provides a lightweight, repeatable way to write specs that:
- Are consumable by humans and LLMs
- Capture scope, constraints, and acceptance criteria
- Include test scaffolding and file plans
- Tie back to CI and packaging pipelines

The payoff:
- Faster iterations with less rework
- Lower hallucination risk
- Clearer reviewer experience
- Better traceability from requirements to code to package

---

## What Is GitHub’s Spec Kit?

Spec Kit is a simple, opinionated pattern for writing AI-consumable specs alongside your code. It’s not a proprietary file format—think of it as a well-structured SPEC.md (or similar) plus a few conventions that Copilot (and teammates) can follow consistently.

A typical Spec Kit includes:
- Problem statement and goals
- Non-goals and constraints
- API surface and data contracts
- Edge cases and examples
- Acceptance tests (or a testing plan)
- File tree and naming plan
- Contribution instructions for AI and humans

Why it works: LLMs respond dramatically better to grounded, structured guidance than to unbounded requests. Spec Kit distills the minimal structure that unlocks that behavior.

---

## Case Study: Building “TextCraft” — A .NET String Utilities NuGet Package

To test Spec Kit end-to-end, I built a tiny but realistic .NET class library called TextCraft. It provides three features that often trip up AI and humans alike:

- Slugify: Convert strings to URL-safe slugs with Unicode handling
- ToKebabCase: Convert PascalCase/camelCase/space-separated strings to kebab-case
- ToTitleCase: Case conversion with culture-aware exceptions (e.g., “iPhone 12 Pro”)

We’ll walk through the spec, the AI-driven implementation, tests, packaging, and publication.

---

## Repository Layout

Here’s the final repository layout the spec dictated:

```
textcraft/
├─ SPEC.md
├─ src/
│  └─ TextCraft/
│     ├─ TextCraft.csproj
│     ├─ Slugifier.cs
│     ├─ CaseConverter.cs
│     └─ TitleCaseOptions.cs
├─ tests/
│  └─ TextCraft.Tests/
│     ├─ TextCraft.Tests.csproj
│     ├─ SlugifierTests.cs
│     ├─ CaseConverterTests.cs
│     └─ TitleCaseTests.cs
├─ .github/
│  └─ workflows/
│     ├─ ci.yml
│     └─ publish.yml
├─ Directory.Build.props
└─ README.md
```

---

## The Spec: SPEC.md (AI-Consumable)

Below is the actual SPEC.md used to drive Copilot. It’s intentionally explicit and action-oriented.

```markdown
# Spec: TextCraft .NET String Utilities Library

## Summary
Implement a .NET class library "TextCraft" with three functions:
1) Slugifier.Slugify(string input, SlugOptions? options = null)
2) CaseConverter.ToKebabCase(string input)
3) CaseConverter.ToTitleCase(string input, TitleCaseOptions? options = null)

Target frameworks: net8.0

## Goals
- Culture-aware, Unicode-resilient string normalization.
- Minimal public API with XML docs.
- Deterministic behavior with testable edge cases.
- Ship to NuGet with semantic versioning and proper metadata.

## Non-Goals
- No dependency on heavyweight globalization libs.
- No web or CLI tool; library only.
- No runtime configuration outside of the provided options.

## Constraints
- Pure C# (no unsafe code).
- Avoid allocations where reasonable; use StringBuilder when useful.
- Cross-platform behavior must be consistent (Windows/Linux/Mac).
- Use dotnet test with xUnit.

## Public API
```csharp
namespace TextCraft;

public sealed record SlugOptions(
    char Separator = '-',
    bool Lowercase = true,
    bool RemoveDiacritics = true,
    int MaxLength = 80);

public static class Slugifier
{
    /// <summary>Create a URL-safe slug from input text.</summary>
    public static string Slugify(string input, SlugOptions? options = null);
}

public static class CaseConverter
{
    /// <summary>Convert to kebab-case from mixed forms (PascalCase, camelCase, spaced, underscored).</summary>
    public static string ToKebabCase(string input);

    /// <summary>Convert to title case with a minimal set of exceptions (e.g., "and", "or", "of", "the").</summary>
    public static string ToTitleCase(string input, TitleCaseOptions? options = null);
}

public sealed record TitleCaseOptions(
    string[] LowercaseWords = new[] { "and", "or", "of", "the", "a", "an", "in", "on", "for", "to" },
    bool PreserveKnownBrands = true);
```

## Behavior & Examples
- Slugify:
  - "Hello, World!" -> "hello-world"
  - "Café con Leche" -> "cafe-con-leche" (diacritics removed)
  - "      Multiple   Spaces     " -> "multiple-spaces"
  - MaxLength truncates without trailing separators.

- ToKebabCase:
  - "HelloWorld" -> "hello-world"
  - "helloWorld" -> "hello-world"
  - "HTTPServer" -> "http-server"
  - "user_id" -> "user-id"
  - " Already Kebab " -> "already-kebab"

- ToTitleCase:
  - "war and peace" -> "War and Peace"
  - "the lord of the rings" -> "The Lord of the Rings"
  - "iphone 12 pro" -> "iPhone 12 Pro" (brand preserved if enabled)
  - "api design in c#" -> "API Design in C#"

## Edge Cases
- Empty or whitespace-only input returns empty string.
- Slugify collapses non-alphanumerics to single separators.
- Unicode normalization: NFKD; remove diacritics if enabled.
- Preserve ASCII digits.
- For TitleCase, recognized initialisms: API, HTTP, URL, C#, .NET.

## File Plan
- src/TextCraft/Slugifier.cs
- src/TextCraft/CaseConverter.cs
- src/TextCraft/TitleCaseOptions.cs
- tests/TextCraft.Tests/SlugifierTests.cs
- tests/TextCraft.Tests/CaseConverterTests.cs
- tests/TextCraft.Tests/TitleCaseTests.cs

## Acceptance Tests (xUnit)
- Provide tests for each example and edge case above.
- 95%+ line coverage across src/ via coverlet (not a hard gate but report coverage).
- dotnet test must pass in CI.

## Done Definition
- All acceptance tests pass locally and in CI.
- README with examples.
- Package metadata filled; GitHub Actions publish on tag v*.*.*.
- No TODOs in public API surface.
```

This format was easy for a human to read and trivial for Copilot to follow. The clarity around API surface, examples, and file plan was especially important.

---

## Driving Implementation with the Spec

With SPEC.md in place, Copilot had a clear target. Here are representative code snippets generated and refined during the process.

### C# Implementation: Slugifier.cs

```csharp
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TextCraft;

public sealed record SlugOptions(
    char Separator = '-',
    bool Lowercase = true,
    bool RemoveDiacritics = true,
    int MaxLength = 80);

public static class Slugifier
{
    private static readonly Regex NonAlphanumeric = new(@"[^a-zA-Z0-9]+", RegexOptions.Compiled);
    private static readonly Regex MultiSeparator = new(@"[-_]+", RegexOptions.Compiled);

    public static string Slugify(string input, SlugOptions? options = null)
    {
        options ??= new SlugOptions();

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string text = input.Trim();

        // Normalize to NFKD to separate diacritics
        text = text.Normalize(NormalizationForm.FormKD);

        if (options.RemoveDiacritics)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            text = sb.ToString().Normalize(NormalizationForm.FormKC);
        }

        // Replace non-alphanumerics with hyphen first
        text = NonAlphanumeric.Replace(text, "-");

        // Lowercase optionally
        if (options.Lowercase)
            text = text.ToLowerInvariant();

        // Collapse multiple separators
        text = MultiSeparator.Replace(text, "-");

        // Trim separators at ends
        text = text.Trim('-');

        // Apply max length without cutting through a separator cluster
        if (options.MaxLength > 0 && text.Length > options.MaxLength)
        {
            text = text[..options.MaxLength].Trim('-');
        }

        // Replace default hyphen with configured separator if needed
        if (options.Separator != '-')
            text = text.Replace('-', options.Separator);

        return text;
    }
}
```

### C# Implementation: CaseConverter.cs and TitleCaseOptions.cs

```csharp
using System.Text;
using System.Text.RegularExpressions;

namespace TextCraft;

public sealed record TitleCaseOptions(
    string[] LowercaseWords = new[] { "and", "or", "of", "the", "a", "an", "in", "on", "for", "to" },
    bool PreserveKnownBrands = true);

public static class CaseConverter
{
    private static readonly Regex WordBoundary = new(@"([A-Z]+(?=[A-Z][a-z])|[A-Z]?[a-z]+|\d+)", RegexOptions.Compiled);
    private static readonly HashSet<string> KnownInitialisms = new(StringComparer.OrdinalIgnoreCase)
        { "API", "HTTP", "URL", "C#", ".NET" };

    public static string ToKebabCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var fragments = new List<string>();
        foreach (Match m in WordBoundary.Matches(input.Trim().Replace('_', ' ').Replace('-', ' ')))
        {
            var token = m.Value;
            if (!string.IsNullOrWhiteSpace(token))
                fragments.Add(token.ToLowerInvariant());
        }
        return string.Join("-", fragments);
    }

    public static string ToTitleCase(string input, TitleCaseOptions? options = null)
    {
        options ??= new TitleCaseOptions();
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var words = Tokenize(input);
        if (words.Count == 0) return string.Empty;

        for (int i = 0; i < words.Count; i++)
        {
            var w = words[i];

            // Preserve symbols like C# / .NET
            if (KnownInitialisms.Contains(w))
            {
                words[i] = NormalizeInitialism(w);
                continue;
            }

            if (options.PreserveKnownBrands && IsKnownBrandLike(w))
            {
                words[i] = PreserveBrandCasing(w);
                continue;
            }

            bool shouldLower = i > 0 && i < words.Count - 1 &&
                               options.LowercaseWords.Contains(w, StringComparer.OrdinalIgnoreCase);

            words[i] = shouldLower ? w.ToLowerInvariant() : ToWordTitleCase(w);
        }

        // Always capitalize first and last words
        words[0] = ToWordTitleCase(words[0]);
        words[^1] = ToWordTitleCase(words[^1]);

        return string.Join(' ', words);
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        foreach (Match m in WordBoundary.Matches(input))
        {
            var token = m.Value.Trim();
            if (token.Length > 0) tokens.Add(token);
        }
        return tokens;
    }

    private static string ToWordTitleCase(string w)
    {
        if (w.Length == 0) return w;
        if (w.Length == 1) return w.ToUpperInvariant();

        // Keep internal punctuation like iPhone's 'i' behavior handled by PreserveKnownBrands
        return char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant();
    }

    private static bool IsKnownBrandLike(string w)
    {
        // Primitive heuristic for demo purposes
        return w.Equals("iPhone", StringComparison.OrdinalIgnoreCase)
               || w.Equals("iOS", StringComparison.OrdinalIgnoreCase)
               || w.Equals("GitHub", StringComparison.OrdinalIgnoreCase);
    }

    private static string PreserveBrandCasing(string w)
    {
        if (w.Equals("iphone", StringComparison.OrdinalIgnoreCase)) return "iPhone";
        if (w.Equals("ios", StringComparison.OrdinalIgnoreCase)) return "iOS";
        if (w.Equals("github", StringComparison.OrdinalIgnoreCase)) return "GitHub";
        return w;
    }

    private static string NormalizeInitialism(string w)
    {
        if (w.Equals("c#", StringComparison.OrdinalIgnoreCase)) return "C#";
        if (w.Equals(".net", StringComparison.OrdinalIgnoreCase) || w.Equals("dotnet", StringComparison.OrdinalIgnoreCase)) return ".NET";
        return w.ToUpperInvariant();
    }
}
```

---

## Tests as Acceptance Criteria

Writing tests from the spec gave Copilot unambiguous targets. The model generated most of these; I curated edge cases to harden behavior.

```csharp
using TextCraft;
using Xunit;

namespace TextCraft.Tests;

public class SlugifierTests
{
    [Theory]
    [InlineData("Hello, World!", "hello-world")]
    [InlineData("Café con Leche", "cafe-con-leche")]
    [InlineData("      Multiple   Spaces     ", "multiple-spaces")]
    [InlineData("API Design in C#", "api-design-in-c")]
    public void Slugify_Basics(string input, string expected)
    {
        Assert.Equal(expected, Slugifier.Slugify(input));
    }

    [Fact]
    public void Slugify_MaxLength_Truncates_Cleanly()
    {
        var options = new SlugOptions(MaxLength: 10);
        Assert.Equal("hello-worl", Slugifier.Slugify("Hello, World!", options));
    }

    [Fact]
    public void Slugify_Empty_Returns_Empty()
    {
        Assert.Equal(string.Empty, Slugifier.Slugify("   "));
    }
}

public class CaseConverterTests
{
    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("helloWorld", "hello-world")]
    [InlineData("HTTPServer", "http-server")]
    [InlineData("user_id", "user-id")]
    [InlineData(" Already Kebab ", "already-kebab")]
    public void ToKebabCase_Works(string input, string expected)
    {
        Assert.Equal(expected, CaseConverter.ToKebabCase(input));
    }
}

public class TitleCaseTests
{
    [Theory]
    [InlineData("war and peace", "War and Peace")]
    [InlineData("the lord of the rings", "The Lord of the Rings")]
    [InlineData("iphone 12 pro", "iPhone 12 Pro")]
    [InlineData("api design in c#", "API Design in C#")]
    public void ToTitleCase_Works(string input, string expected)
    {
        Assert.Equal(expected, CaseConverter.ToTitleCase(input));
    }

    [Fact]
    public void ToTitleCase_Empty_Returns_Empty()
    {
        Assert.Equal(string.Empty, CaseConverter.ToTitleCase("   "));
    }
}
```

Tip: your spec should say exactly which examples become tests. This lets the AI wire everything without guesswork.

---

## Packaging for NuGet

Metadata and packaging were part of “Done Definition.” Here’s the csproj and Actions config.

### TextCraft.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- Package -->
    <PackageId>TextCraft</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Company>Your Org</Company>
    <Product>TextCraft</Product>
    <Description>Culture-aware string utilities: slugify, kebab-case, and title-case for .NET.</Description>
    <RepositoryUrl>https://github.com/your-org/textcraft</RepositoryUrl>
    <PackageProjectUrl>https://github.com/your-org/textcraft</PackageProjectUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageTags>string;slugify;kebab-case;title-case;dotnet;library</PackageTags>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
  </PropertyGroup>
</Project>
```

### CI: .github/workflows/ci.yml

```yaml
name: CI
on:
  push:
    branches: [ main ]
  pull_request:

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --configuration Release --no-restore
      - name: Test
        run: dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage"
```

### Publish: .github/workflows/publish.yml

```yaml
name: Publish
on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  publish:
    runs-on: ubuntu-latest
    permissions:
      contents: read
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Build and Pack
        run: |
          dotnet restore
          dotnet build -c Release
          dotnet pack src/TextCraft/TextCraft.csproj -c Release -o ./artifacts
      - name: Push to NuGet
        env:
          NUGET_API_KEY: ${{ secrets.NUGET_API_KEY }}
        run: |
          dotnet nuget push ./artifacts/*.nupkg --api-key "$NUGET_API_KEY" --source https://api.nuget.org/v3/index.json --skip-duplicate
```

With this in place, tagging a release (e.g., v1.0.1) triggers packaging and publication.

---

## Results: What Changed with Spec Kit

Over the course of this small project, the Spec Kit approach yielded measurable improvements:

- Fewer loops: 2 review cycles to green vs. 5–6 typical for ad hoc prompting on similar utilities
- Stable outputs: No regressions after spec-locked acceptance tests were added
- Faster onboarding: New contributors could see the API, tests, and file plan at a glance
- CI-first culture: “Done” meant shippable to NuGet; no ambiguity

Quantitatively (single-repo observation):
- Time to first passing build: ~45 minutes (including writing the spec)
- Time to first NuGet publish: ~1 hour 30 minutes total
- Tokens spent in Copilot Chat: noticeably lower after the spec was finalized (fewer back-and-forths)

Caveat: Your results will vary by domain complexity. The tighter your spec and examples, the better the model performs.

---

## Where the AI Struggled (and How the Spec Helped)

- Unicode diacritics removal: Initial code used FormD but didn’t recompose properly. The spec’s “NFKD and remove NonSpacingMark” note guided a correct fix.
- Title casing rules: The model over-capitalized “and/of/the” until the spec and tests pinned down exceptions and first/last word rules.
- Acronyms and brands: Without the “KnownInitialisms” requirement, “API” became “Api.” Tests made this non-negotiable.

The pattern is consistent: when behavior is encoded into the spec and tests, Copilot converges quickly. When left vague, it improvises.

---

## How to Write a Great AI-Ready Spec

Follow these guidelines to make your Spec Kit documents consistently effective:

- Start with a tight API contract
  - Namespaces, classes, methods, signatures
  - Options types with defaults
  - XML documentation comments required

- Specify examples and turn them into tests
  - Each example in the spec should be an assertion
  - Include edge cases and “gotchas”

- Declare constraints and non-goals
  - Enforce frameworks, performance constraints, or style rules
  - Explicitly say what not to build

- Provide a file plan and naming scheme
  - Directory structure and filenames matter to AI
  - Avoid “mystery file” generation

- Define “Done”
  - Tests green in CI
  - Packaging/publishing steps complete
  - Documentation complete, zero TODOs in public API

- Add iteration instructions
  - “If tests fail, fix code; if requirements change, update SPEC.md and tests first”

---

## Spec Kit Components vs. Value

| Spec Component       | Purpose                                   | Value Delivered                                 |
|----------------------|--------------------------------------------|-------------------------------------------------|
| Summary + Goals      | Business/context framing                   | Shared understanding, scoping                   |
| Non-Goals            | Boundaries                                 | Prevents scope creep                            |
| API Surface          | Contract                                   | Lowers ambiguity for AI and reviewers           |
| Examples & Edge Cases| Behavioral truth table                     | Faster convergence, fewer hallucinations        |
| Acceptance Tests     | Executable spec                            | Automatic verification                          |
| File Plan            | Deterministic outputs                      | Repeatable generation and easier code review    |
| Done Definition      | Shipping checklist                         | Aligns code, CI, and packaging                  |

---

## Anti-Patterns to Avoid

- Hand-wavy prompts: “Build a slugifier” without constraints or tests invites improvisation.
- Oversized specs: If it’s longer than the codebase, nobody reads it. Keep it target-specific.
- Moving targets: Changing requirements without updating the spec breaks trust.
- Hidden decisions: Tacit rules (e.g., casing exceptions) must be explicit or encoded in tests.
- Unpinned versions: Don’t let frameworks drift; lock down the target framework and CI matrix.

---

## Adopting Spec Kit in Your Team

- Start small: Pilot on a utility or internal SDK rather than your core product.
- Standardize the template: Adopt a SPEC.md structure that fits your domain.
- Make acceptance tests mandatory: PRs are only “ready” when tests from the spec are in place.
- Teach the loop:
  1) Update SPEC.md
  2) Generate/modify code with Copilot
  3) Run tests locally
  4) Fix code or refine the spec
  5) Repeat until green, then ship
- Measure: Track cycles-to-green and review comments. Share wins.

---

## Practical Tips for .NET Libraries

- Normalize Unicode explicitly; do not rely on platform defaults.
- Keep the API minimal and immutable-friendly (records for options).
- Treat tests as product assets; place examples alongside spec text.
- Use GitHub Actions to enforce consistency across environments.
- Fill out NuGet metadata meticulously; it inspires trust and discoverability.

---

## Extending the Spec: Linting, Benchmarks, and Docs

Once the basics are working, Spec Kit can drive additional quality gates:

- Linting: Add analyzers and include “no warnings in Release” in Done Definition.
- Benchmarks: For perf-sensitive libraries, use BenchmarkDotNet and pin a baseline.
- Docs: Generate XML docs and publish a README with code snippets—both specified and tested.

Example Done Definition upgrades:
- “dotnet build -c Release yields zero warnings”
- “Benchmarks show <2% variance across runs”
- “README examples are mirrored in tests”

---

## Frequently Asked Questions

- Does this only work with GitHub Copilot?
  - No. The pattern is model-agnostic. Any LLM benefits from structured specs and tests.

- Isn’t this just test-driven development?
  - It’s complementary. Spec Kit codifies requirements and examples up front, then TDD validates them. The twist is that you’re writing for humans and an AI partner simultaneously.

- What if my problem is too open-ended for a spec?
  - Break it into spec-able slices. Use research spikes to learn, then spec the actionable parts.

- What if Copilot still gets it wrong?
  - Tighten the spec, add failing tests for the misbehavior, and iterate. Avoid changing code and spec in opposite directions.

---

## What I’d Do Differently Next Time

- Expand the “KnownInitialisms” and “brand casing” lists via a small JSON data file referenced in the spec.
- Add property-based tests for the slugifier to catch regressions on random Unicode inputs.
- Include performance budgets (e.g., max allocations per 1,000 chars).

These are easy to express in Spec Kit and give the AI a firmer target.

---

## Conclusion

GitHub’s Spec Kit shifts AI coding from novelty to engineering discipline. By grounding Copilot in clear API contracts, acceptance tests, file plans, and shipping criteria, you get predictable outputs without slowing down. In this .NET NuGet case study, the spec-driven approach cut review cycles, stabilized behavior, and made CI/CD part of the “definition of done.”

If you’ve been “vibe coding” with AI, Spec Kit is the fastest way to level up. Start with a SPEC.md, encode examples as tests, and let your AI partner execute the plan.

---

## Appendix: Reusable Spec Template

Copy this into your repo and tailor it to your project.

```markdown
# Spec: <Project / Feature Name>

## Summary
<One paragraph on what, why, for whom.>

## Goals
- <Goal 1>
- <Goal 2>

## Non-Goals
- <Non-Goal 1>

## Constraints
- <Language/Framework>
- <Performance/Security/Compliance>

## Public API (or CLI / Protocol)
```csharp
// Signatures here
```

## Behavior & Examples
- <Example input> -> <Expected output>
- Edge cases: <List them>

## File Plan
- src/<...>
- tests/<...>

## Acceptance Tests
- <List of test cases to implement verbatim>

## Done Definition
- Tests pass locally and in CI
- Docs updated
- Package/publish steps complete
```

---

Ready to try it? Create SPEC.md, paste the template, add your first examples as tests, and ask Copilot to implement to spec. Then ship with confidence.