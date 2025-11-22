# Test Driving GitHub's Spec Kit

_Category: Case Studies_

> Learn how GitHub's Spec Kit transforms AI coding from “vibe coding” to structured spec‑driven development. Real results from a .NET NuGet package project.

## Executive summary

Spec Kit is a lightweight way to turn ambiguous prompts into precise, test‑able software work. Instead of throwing a vague request at an AI assistant and hoping for the best, you define an explicit “contract” the model can implement: scope, constraints, file plan, and acceptance tests. In this case study, we used GitHub’s Spec Kit approach to build and ship a production‑ready .NET NuGet package with Copilot as the primary coding partner. The result: tighter iteration loops, fewer re‑writes, cleaner interfaces, and a fully automated pipeline from spec to tests to packaging.

If you’ve ever felt trapped in “vibe coding” with AI—short prompts, long guesses, and endless corrections—Spec Kit is the antidote.

---

## Contents

- What is GitHub’s Spec Kit?
- Why specs matter for AI coding
- The experiment: shipping a .NET NuGet package with Spec Kit
- Writing the spec
- Driving Copilot with the spec
- Implementation highlights (C#)
- Acceptance tests (xUnit)
- CI/CD and NuGet packaging
- Results: what changed vs. vibe coding
- Lessons learned and best practices
- How to adopt Spec Kit in your team
- FAQs
- References

---

## What is GitHub’s Spec Kit?

GitHub’s Spec Kit is a simple, repo‑native pattern for writing implementation‑ready specifications that AI agents (and humans) can follow. Think of it as “prompt engineering for code,” but with all the precision of a lightweight functional spec.

A typical Spec Kit includes:

- A single source of truth for intent and constraints (spec.yaml or spec.md).
- A file plan that tells the AI which files to create or modify.
- Non‑goals and trade‑offs to minimize scope creep.
- Acceptance tests (or stubs) that capture executable expectations.
- Integration points for your tools (e.g., CI workflow names, language version, linters).

You can use Spec Kit with GitHub Copilot Chat in editors like VS Code or Visual Studio, and in PR reviews. You paste or reference your spec, ask Copilot to implement the file plan and tests, and iterate until tests pass.

Useful links:

- GitHub Copilot: https://github.com/features/copilot
- NuGet publishing: https://learn.microsoft.com/nuget/create-packages/overview-and-workflow
- .NET testing with xUnit: https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test

---

## Why specs matter for AI coding

AIs are fast at producing code; they’re not mind readers. Specs give the model guardrails, context, and a target.

| Dimension       | Vibe coding (prompt-and-pray)     | Spec-driven (Spec Kit)                          |
| --------------- | --------------------------------- | ----------------------------------------------- |
| Input           | Short, ambiguous prompt           | Clear problem statement, constraints, file plan |
| Output quality  | Variable; surprises likely        | Predictable; closer to intended architecture    |
| Iterations      | Many; fix what you didn’t ask for | Fewer; validate what you did ask for            |
| Tests           | Often added late                  | Baked into acceptance criteria                  |
| Maintainability | Incidental                        | Intentional: naming, APIs, non-goals documented |
| Time to value   | Fast start, slow finish           | Steady velocity, faster finish                  |

Top benefits:

- Reduced rework: constraints prevent the model from inventing unneeded complexity.
- Faster convergence: acceptance tests make correctness visible immediately.
- Socialization: the spec doubles as team documentation and code review context.
- Reusability: a good spec becomes a template for future tasks.

---

## The experiment: shipping a .NET NuGet package with Spec Kit

We set out to implement, test, and publish a small yet non‑trivial .NET library: a generic, thread‑safe LRU cache with metrics. The target was a single NuGet package with strong documentation, zero external dependencies, and predictable performance.

Scope:

- Name: TinyLruNet
- Purpose: O(1) get/set/contains operations with LRU eviction, metrics, and TTL.
- Targets: netstandard2.0, net8.0
- Dependencies: None beyond the BCL
- Testing: xUnit
- CI: GitHub Actions; pack on release tag; push to NuGet

Copilot was the primary implementer; we acted as the product owner and reviewer via Spec Kit.

---

## Writing the spec

We started with a spec.yaml at the root of the repo. You can prefer markdown; YAML helps tools parse sections more precisely. The key is explicitness: success criteria, non‑goals, and a file plan the agent can act on.

```yaml
# spec.yaml
title: TinyLruNet - Lightweight LRU cache for .NET
owner: @your-github-handle
status: draft
summary: >
  A generic, thread-safe LRU cache with O(1) operations, optional TTL, eviction events,
  and basic metrics. No external dependencies. Package as a NuGet library.

goals:
  - Provide LRU eviction policy with capacity bound.
  - Offer thread-safe get/set/try-get/remove/clear methods.
  - Support optional absolute TTL per entry.
  - Expose metrics: Count, Capacity, Hits, Misses, Evictions.
  - Provide IDisposable for cleanup of timers.
  - Target netstandard2.0 and net8.0; no external dependencies.

non_goals:
  - Distributed caching or multi-process coordination.
  - Persistence to disk.
  - Async I/O APIs.
  - Sliding expiration (future work).

constraints:
  api_style: "C# public API with XML docs; minimal public surface"
  performance: "O(1) average for get/set; eviction unblocks readers quickly"
  quality: "100% tests for core behaviors; static analysis passes"
  threading: "Use lock-based approach; avoid ReaderWriterLockSlim pitfalls for simplicity"

file_plan:
  - path: src/TinyLruNet/LruCache.cs
    purpose: "Core LRU cache implementation"
  - path: src/TinyLruNet/TinyLruNet.csproj
    purpose: "Library project; multi-targeting"
  - path: tests/TinyLruNet.Tests/LruCacheTests.cs
    purpose: "xUnit tests capturing acceptance criteria"
  - path: Directory.Build.props
    purpose: "Common settings; LangVersion, TreatWarningsAsErrors"
  - path: .github/workflows/ci.yml
    purpose: "Build and test on PR"
  - path: .github/workflows/release.yml
    purpose: "Pack and push to NuGet on tag"

acceptance_tests:
  - name: "Evicts least-recently-used on capacity overflow"
  - name: "Get updates recency; Set overwrites value and recency"
  - name: "TTL removes items after expiration"
  - name: "Metrics increment on hit/miss/eviction"
  - name: "Thread-safety under contention yields consistent results"

documentation:
  readme: true
  xml_docs: true

outputs:
  nuget_package_id: "TinyLruNet"
  nuget_push_on_tag: true
```

Prompting Copilot with this spec is as simple as: “Implement the file_plan from spec.yaml. Create the code and tests to satisfy acceptance_tests. Begin with src/TinyLruNet/LruCache.cs and tests.”

Tip: Keep your spec and acceptance tests in the repo to make them visible in PRs and to Copilot.

---

## Driving Copilot with the spec

Workflow we used:

1. Paste spec.yaml into Copilot Chat in VS Code and request: “Create the files in file_plan with initial implementations and tests.”
2. Ask Copilot to write the test file first based on acceptance_tests. This enforces an executable target.
3. Generate minimal implementation to make the tests compile.
4. Run tests locally. Let the failures guide the next prompts: “The test X failed; adjust eviction logic accordingly.”
5. Iterate until tests pass and code is clean. Ask Copilot to add XML docs for the public API.
6. Generate the README and package metadata.
7. Configure GitHub Actions for CI and release.

This pattern turns Copilot into an implementation engine rather than a design engine. We decide what to build; Copilot helps build it.

---

## Implementation highlights (C#)

Below is a condensed LRU cache implementation that meets the spec. The structure is intentionally straightforward: Dictionary for O(1) lookup, LinkedList for O(1) recency updates. A single lock protects invariants.

```csharp
// src/TinyLruNet/LruCache.cs
using System;
using System.Collections.Generic;
using System.Threading;

namespace TinyLruNet
{
    /// <summary>
    /// A generic, thread-safe LRU cache with optional per-item TTL.
    /// O(1) get/set operations based on Dictionary + LinkedList.
    /// </summary>
    public sealed class LruCache<TKey, TValue> : IDisposable where TKey : notnull
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<Entry>> _map;
        private readonly LinkedList<Entry> _lru;
        private readonly Timer? _ttlTimer;
        private readonly object _gate = new();

        private long _hits;
        private long _misses;
        private long _evictions;
        private bool _disposed;

        private readonly TimeSpan _sweepInterval = TimeSpan.FromSeconds(5);

        private sealed class Entry
        {
            public TKey Key { get; }
            public TValue Value { get; set; }
            public DateTimeOffset? ExpiresAt { get; set; }

            public Entry(TKey key, TValue value, DateTimeOffset? expiresAt)
            {
                Key = key;
                Value = value;
                ExpiresAt = expiresAt;
            }

            public bool IsExpired(DateTimeOffset now) =>
                ExpiresAt.HasValue && now >= ExpiresAt.Value;
        }

        public LruCache(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _map = new Dictionary<TKey, LinkedListNode<Entry>>(capacity);
            _lru = new LinkedList<Entry>();
            // Periodic TTL sweep; optional micro-leaks are acceptable for simplicity.
            _ttlTimer = new Timer(Sweep, state: null, _sweepInterval, _sweepInterval);
        }

        public int Capacity => _capacity;
        public int Count { get { lock (_gate) { return _map.Count; } } }

        public long Hits => Interlocked.Read(ref _hits);
        public long Misses => Interlocked.Read(ref _misses);
        public long Evictions => Interlocked.Read(ref _evictions);

        /// <summary>
        /// Sets a value with an optional absolute time-to-live.
        /// Overwrites existing value and updates recency.
        /// </summary>
        public void Set(TKey key, TValue value, TimeSpan? ttl = null)
        {
            var expiresAt = ttl.HasValue ? DateTimeOffset.UtcNow.Add(ttl.Value) : (DateTimeOffset?)null;
            lock (_gate)
            {
                EnsureNotDisposed();

                if (_map.TryGetValue(key, out var node))
                {
                    node.Value.Value = value;
                    node.Value.ExpiresAt = expiresAt;
                    _lru.Remove(node);
                    _lru.AddFirst(node);
                    return;
                }

                var entry = new Entry(key, value, expiresAt);
                var newNode = new LinkedListNode<Entry>(entry);
                _lru.AddFirst(newNode);
                _map[key] = newNode;

                if (_map.Count > _capacity)
                {
                    EvictLast();
                }
            }
        }

        /// <summary>
        /// Gets a value if present and not expired. Updates recency on hit.
        /// </summary>
        public bool TryGet(TKey key, out TValue? value)
        {
            lock (_gate)
            {
                EnsureNotDisposed();

                if (_map.TryGetValue(key, out var node))
                {
                    var now = DateTimeOffset.UtcNow;
                    if (node.Value.IsExpired(now))
                    {
                        RemoveNode(node);
                        Interlocked.Increment(ref _misses);
                        value = default!;
                        return false;
                    }

                    _lru.Remove(node);
                    _lru.AddFirst(node);
                    Interlocked.Increment(ref _hits);
                    value = node.Value.Value;
                    return true;
                }
            }

            Interlocked.Increment(ref _misses);
            value = default!;
            return false;
        }

        /// <summary>
        /// Removes a key if present. Returns true if removed.
        /// </summary>
        public bool Remove(TKey key)
        {
            lock (_gate)
            {
                EnsureNotDisposed();
                if (_map.TryGetValue(key, out var node))
                {
                    RemoveNode(node);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            lock (_gate)
            {
                EnsureNotDisposed();
                _map.Clear();
                _lru.Clear();
            }
        }

        private void EvictLast()
        {
            var last = _lru.Last;
            if (last is null) return;
            RemoveNode(last);
            Interlocked.Increment(ref _evictions);
        }

        private void RemoveNode(LinkedListNode<Entry> node)
        {
            _lru.Remove(node);
            _map.Remove(node.Value.Key);
        }

        private void Sweep(object? _)
        {
            // Best-effort: skip if disposed or empty to minimize contention.
            if (_disposed) return;
            var now = DateTimeOffset.UtcNow;

            lock (_gate)
            {
                var node = _lru.Last;
                while (node != null)
                {
                    var prev = node.Previous;
                    if (node.Value.IsExpired(now))
                    {
                        RemoveNode(node);
                        Interlocked.Increment(ref _evictions);
                    }
                    node = prev;
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LruCache<TKey, TValue>));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _ttlTimer?.Dispose();
            lock (_gate)
            {
                _map.Clear();
                _lru.Clear();
            }
        }
    }
}
```

Project file for multi‑targeting and NuGet metadata:

```xml
<!-- src/TinyLruNet/TinyLruNet.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;netstandard2.0</TargetFrameworks>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- NuGet metadata -->
    <PackageId>TinyLruNet</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Company>Your Company</Company>
    <PackageDescription>Thread-safe LRU cache for .NET with TTL and metrics. Zero dependencies.</PackageDescription>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/your/repo</PackageProjectUrl>
    <RepositoryUrl>https://github.com/your/repo</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    <PackageReadmeFile>README.md</PackageReadmeFile>
  </PropertyGroup>
</Project>
```

Optional shared settings:

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
    <WarningsAsErrors>CS1591</WarningsAsErrors>
  </PropertyGroup>
</Project>
```

---

## Acceptance tests (xUnit)

We asked Copilot to codify the acceptance tests from the spec. Tests are the heartbeat of Spec Kit—they pin down behavior the model must satisfy.

```csharp
// tests/TinyLruNet.Tests/LruCacheTests.cs
using System;
using System.Threading;
using TinyLruNet;
using Xunit;

namespace TinyLruNet.Tests
{
    public class LruCacheTests
    {
        [Fact]
        public void Evicts_LRU_On_Capacity()
        {
            var cache = new LruCache<string, int>(capacity: 2);
            cache.Set("a", 1);
            cache.Set("b", 2);

            // Access "a" to make it MRU; now "b" is LRU.
            Assert.True(cache.TryGet("a", out _));

            cache.Set("c", 3); // Evicts "b"

            Assert.True(cache.TryGet("a", out var va));
            Assert.Equal(1, va);

            Assert.False(cache.TryGet("b", out _));
            Assert.True(cache.TryGet("c", out var vc));
            Assert.Equal(3, vc);

            Assert.Equal(1, cache.Evictions);
        }

        [Fact]
        public void Set_Overwrites_And_Updates_Recency()
        {
            var cache = new LruCache<string, int>(2);
            cache.Set("x", 10);
            cache.Set("y", 20);

            // Overwrite x; now x becomes MRU, y is LRU.
            cache.Set("x", 11);

            cache.Set("z", 30); // Evicts y
            Assert.False(cache.TryGet("y", out _));
            Assert.True(cache.TryGet("x", out var vx));
            Assert.Equal(11, vx);
            Assert.True(cache.TryGet("z", out var vz));
            Assert.Equal(30, vz);
        }

        [Fact]
        public void Ttl_Expires_Items()
        {
            var cache = new LruCache<string, string>(2);
            cache.Set("t", "ok", ttl: TimeSpan.FromMilliseconds(60));
            Assert.True(cache.TryGet("t", out var v1));
            Assert.Equal("ok", v1);
            Thread.Sleep(120);

            // After expiration, TryGet should fail and count drop if swept.
            var present = cache.TryGet("t", out _);
            Assert.False(present);
        }

        [Fact]
        public void Metrics_Track_Hits_Misses_And_Evictions()
        {
            var cache = new LruCache<int, int>(2);
            cache.Set(1, 1);
            cache.Set(2, 2);
            Assert.True(cache.TryGet(1, out _)); // hit
            Assert.False(cache.TryGet(3, out _)); // miss
            cache.Set(3, 3); // eviction

            Assert.Equal(1, cache.Hits);
            Assert.Equal(1, cache.Misses);
            Assert.Equal(1, cache.Evictions);
        }

        [Fact]
        public void Thread_Safety_Under_Contention()
        {
            var cache = new LruCache<int, int>(64);
            var threads = new Thread[8];
            var stop = false;

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    var rnd = new Random();
                    while (!Volatile.Read(ref stop))
                    {
                        var k = rnd.Next(0, 1024);
                        cache.Set(k, k);
                        cache.TryGet(k, out _);
                    }
                });
            }

            foreach (var t in threads) t.Start();
            Thread.Sleep(300);
            stop = true;
            foreach (var t in threads) t.Join();

            // Basic integrity assertions
            Assert.InRange(cache.Count, 0, cache.Capacity);
        }
    }
}
```

Run locally:

- dotnet test
- Expect red/green cycles as behavior clarifies.

---

## CI/CD and NuGet packaging

Spec Kit encourages end‑to‑end thinking: define how code ships before you write it. We added two workflows.

Continuous integration on push/PR:

```yaml
# .github/workflows/ci.yml
name: ci
on:
    push:
        branches: [main]
    pull_request:

jobs:
    build:
        runs-on: ubuntu-latest
        steps:
            - uses: actions/checkout@v4
            - uses: actions/setup-dotnet@v4
              with:
                  dotnet-version: "8.0.x"
            - name: Restore
              run: dotnet restore
            - name: Build
              run: dotnet build --configuration Release --no-restore
            - name: Test
              run: dotnet test --configuration Release --no-build --verbosity normal
```

Release on tag to NuGet:

```yaml
# .github/workflows/release.yml
name: release
on:
    push:
        tags:
            - "v*.*.*"

jobs:
    publish:
        runs-on: ubuntu-latest
        permissions:
            contents: read
            packages: write
        steps:
            - uses: actions/checkout@v4
            - uses: actions/setup-dotnet@v4
              with:
                  dotnet-version: "8.0.x"
            - name: Restore
              run: dotnet restore
            - name: Build
              run: dotnet build -c Release
            - name: Pack
              run: dotnet pack src/TinyLruNet/TinyLruNet.csproj -c Release -o ./artifacts --no-build
            - name: Push to NuGet
              run: dotnet nuget push "./artifacts/*.nupkg" --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

With Spec Kit, these workflows are part of the file plan. Copilot generates them with the correct paths and targets.

---

## Results: what changed vs. vibe coding

Baseline we’ve seen in “vibe coding” sessions:

- Multiple AI “hallucinations” about thread models and APIs.
- Post‑hoc tests that forced painful refactors.
- Architecture drift across iterations.

With Spec Kit:

- First draft matched the desired public API and file structure.
- We converged in 3 iterations:
    - Iteration 1: tests generated; initial implementation produced; TTL sweep bug revealed.
    - Iteration 2: fixed expiration and recency; clarified metrics semantics.
    - Iteration 3: added XML docs, CI, and NuGet metadata; all tests green.
- The release workflow pushed a package on first tag without manual patching.

Qualitative improvements:

- Traceability: acceptance tests aligned with spec names, making code review crisp.
- Reviewability: non‑goals prevented Copilot from adding sliding expiration “for completeness.”
- Velocity: less time specifying “what we meant” after the code landed; more time validating behavior.

---

## Lessons learned and best practices

Actionable guidance for your own Spec Kit runs:

- Write tests first in the spec. Be explicit about edge cases (e.g., recency after Set).
- Include non‑goals. They act as guardrails and reduce scope creep.
- Provide a file plan. AI agents struggle less when told exactly which files to touch.
- Constrain the API surface. “Minimal public API” is a useful constraint for libraries.
- Seed reasonable defaults. E.g., a TTL sweep interval and performance expectations.
- Embrace deterministic builds. Add CI early; failing builds reveal missing package metadata and analyzer issues.
- Iterate in small loops. After each Copilot-generated change, run tests and feed failures back into the chat.
- Keep spec and tests in the repo. They become living documentation and PR context.
- Prefer simple concurrency. Unless required, a coarse lock is easier for models to get correct than complex lock‑free code.

Common pitfalls:

- Over‑broad specs lead to under‑specified code. Narrow the scope to ship earlier.
- Hidden constraints (e.g., must target netstandard2.0) discovered late cause churn. Put them in the spec.
- Letting Copilot design the API can drift from your intended ergonomics. Specify the API style and naming.

---

## How to adopt Spec Kit in your team

A practical rollout plan:

1. Pick a low‑risk target
    - A small library, CLI tool, or refactor with clear boundaries.

2. Clone or create a Spec Kit template
    - Include sections: goals, non‑goals, constraints, file_plan, acceptance_tests, outputs.

3. Integrate with your editor and CI
    - Use Copilot Chat in VS Code or Visual Studio.
    - Add CI workflow names into the file plan so they’re implemented on day one.

4. Run a pilot
    - Time‑box one or two sprints. Compare iteration counts and defects vs. your usual flow.

5. Retrospective and standardization
    - Turn your best spec into an internal template.
    - Capture your preferred test patterns and code style choices.

6. Scale up
    - Introduce Spec Kit for bug fixes and new features, not only greenfield.

A minimal spec template you can reuse:

```yaml
title: <Project/Feature name>
summary: <Short description of what and why>
goals:
    - <Explicit behavior or outcome>
non_goals:
    - <Explicitly excluded items>
constraints:
    runtime: <e.g., .NET versions, Node versions>
    api_style: <e.g., public surface, patterns>
    performance: <big-O, latency, memory>
file_plan:
    - path: <path/to/file>
      purpose: <role>
acceptance_tests:
    - name: <test case name>
      notes: <edge cases>
outputs:
    docs: <readme, xml docs, site>
    release: <package/release target>
```

---

## FAQs

- Is Spec Kit a heavy process?
    - No. It’s a single markdown/YAML file plus tests. Aim for 1–2 pages.

- Do I need Copilot Workspace or a special tool?
    - No. Spec Kit works with Copilot Chat in your editor and standard GitHub Actions. Any AI assistant that can read files and follow instructions benefits from it.

- What if my project is bigger than a page of spec?
    - Split the work. Create one spec per feature or milestone, each with its own acceptance tests and file plan.

- Can I use BDD tools like Gherkin?
    - Yes. The principle is the same: executable behavior descriptions. Use what your team is comfortable with.

- How do I keep the spec and code in sync?
    - Treat the spec like code: version it, review it in PRs, and update acceptance tests when behavior changes.

---

## References

- GitHub Copilot overview: https://github.com/features/copilot
- .NET multi-targeting: https://learn.microsoft.com/dotnet/standard/library-guidance/cross-platform-targeting
- NuGet publishing guide: https://learn.microsoft.com/nuget/quickstart/create-and-publish-a-package-using-the-dotnet-cli
- xUnit for .NET: https://xunit.net/

---

## Closing thoughts

Spec Kit doesn’t make AI magically “understand you.” It makes you understandable—to humans and machines. By turning intent into an explicit contract, you transform AI from a creative guesser into a reliable implementer. Our .NET NuGet case study showed fewer iterations, predictable architecture, and a clean release pipeline—all because the spec did the heavy lifting up front.

If you’re serious about shipping with AI, stop vibe coding. Start spec‑driven development.
