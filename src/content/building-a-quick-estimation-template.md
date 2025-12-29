# Building a Quick Estimation Template When You Have Almost Nothing to Go On

> When faced with vague requirements and tight deadlines, I built a simple three-pillar framework using Innovation, Scope, and People for estimating quickly.

Category: Project Management

---

## Why You Need a Quick Estimation Template

There are moments in project management when leadership asks, “How long will this take?” and all you have is a one-liner and a deadline. Requirements are nebulous, resources are unclear, and the risks are unknown—yet you still need a number. In these situations, you don’t need a perfect plan; you need a credible, defensible, and quick estimate that communicates uncertainty honestly.

This article presents a pragmatic, repeatable approach built around a three-pillar model—Innovation, Scope, and People—to deliver fast estimates with traceable logic. It helps you move from “I have almost nothing” to “Here’s a 50/70/90 estimate with assumptions,” in under an hour.

---

## Principles of Estimating in the Fog

- Fast over perfect: Provide a bounded, defensible range within 30–60 minutes.
- Honest uncertainty: Communicate confidence levels and assumptions up front.
- Repeatable structure: Use a compact template you can refine over time.
- Calibrate as you learn: Track real vs. estimated to tighten multipliers.
- Visible trade-offs: Show how adding information reduces uncertainty.

For context, see the Cone of Uncertainty, which shows how estimates become more accurate as knowledge increases: https://en.wikipedia.org/wiki/Cone_of_Uncertainty

---

## The Three-Pillar Framework

At low fidelity, everything collapses into three drivers:

1) Innovation (I): How novel is this work?
2) Scope (S): How much work do we think is included?
3) People (P): Who’s doing it, and how are they organized?

Each pillar is scored quickly on a 1–5 scale. Scores map to multipliers that inflate or deflate the base estimate. This keeps the math simple and the reasoning explainable.

### Pillar 1: Innovation (Novelty and Uncertainty)

- 1 – Purely routine work with playbooks
- 2 – Minor variations on familiar patterns
- 3 – Some unknowns; expected trial-and-error
- 4 – High novelty; integration with unfamiliar tech
- 5 – R&D-like; unclear feasibility

What to ask:
- Is there precedent internally or externally?
- Are there new technologies, vendors, or APIs?
- Are there unknown performance/security/stability constraints?
- Will we prototype or spike to learn?

### Pillar 2: Scope (Breadth and Depth)

- 1 – Single small deliverable
- 2 – A few related deliverables with limited integration
- 3 – Moderate set of deliverables with some cross-team dependencies
- 4 – Complex feature set; multi-system integrations
- 5 – Program-level scope; many moving parts

What to ask:
- What’s in vs. out of scope (even roughly)?
- How many components, integrations, or environments?
- What are the dependencies or non-functional requirements?
- What does “done” mean?

### Pillar 3: People (Capability and Configuration)

- 1 – Expert team with proven track record in this domain
- 2 – Strong team, one or two gaps
- 3 – Capable team; limited domain experience; some context switching
- 4 – Mixed capabilities; changing priorities; partial availability
- 5 – New team; low availability; external coordination

What to ask:
- Who is available and for how many hours per week?
- Do we have domain expertise?
- Are roles covered (e.g., engineering, QA, design, PM)?
- Are decision-makers accessible?

---

## The Scoring Matrix and Multipliers

Use scores to select multipliers. These are starting points; calibrate them to your org.

| Pillar     | Score | Multiplier | Heuristic description |
|------------|-------|------------|-----------------------|
| Innovation | 1     | 0.90       | Routine, low uncertainty |
|            | 2     | 1.00       | Familiar work          |
|            | 3     | 1.20       | Some unknowns          |
|            | 4     | 1.50       | High novelty           |
|            | 5     | 2.00       | R&D/prototype          |
| Scope      | 1     | 0.80       | Very limited           |
|            | 2     | 1.00       | Small-to-medium        |
|            | 3     | 1.30       | Moderate complexity    |
|            | 4     | 1.60       | Complex integrations   |
|            | 5     | 2.20       | Program-level          |
| People     | 1     | 0.85       | Elite, stable team     |
|            | 2     | 1.00       | Strong coverage        |
|            | 3     | 1.20       | Gaps or context switching |
|            | 4     | 1.50       | Availability issues    |
|            | 5     | 1.90       | New team / external    |

Notes:
- Innovation and Scope usually inflate the estimate as they rise.
- People often inflates when less optimal (higher score = larger multiplier).
- Multipliers compound: TotalMultiplier = I × S × P.

---

## A Minimal Estimation Formula

- Start with a base scope size in abstract units (e.g., story points or T-shirt sizes converted).
- Translate into person-days using a baseline throughput.
- Apply multipliers to account for innovation, scope uncertainty, and team factors.
- Add a contingency consistent with confidence levels.

Suggested defaults:
- Baseline throughput: 1 story point ≈ 0.75 person-days (adjust per team)
- Alternatively: 1 small feature ≈ 3–5 days; 1 integration ≈ 5–10 days

Formula:
- Base person-days = ScopeUnits × Throughput
- Adjusted person-days = Base × I × S × P
- 50% estimate (P50) = Adjusted
- 70% estimate (P70) = P50 × 1.2
- 90% estimate (P90) = P50 × 1.5

These P50/P70/P90 factors are simple proxies when you can’t run full risk modeling. Replace with your own calibrated ratios over time.

---

## Quick Start: 15-Minute Estimation Interview

Ask:
- What is the primary outcome? What does “done” look like?
- What’s in/out? Name three things definitely not included.
- Who is available? Any hard capacity limits?
- Which systems are involved? Any new vendors or tech?
- What date is driving this? What is flexible?

Then:
- Assign I/S/P scores.
- Choose a rough scope unit count (e.g., 8–15 points).
- Compute P50/P70/P90.

---

## The One-Page Estimation Template (Markdown)

Copy/paste this into your ticket, doc, or email.

```
# Quick Estimate — <Project/Feature Name>
Date: <YYYY-MM-DD>
Estimator: <Name>
Confidence: P50/P70/P90

Outcome (one-liner):
- <Describe the measurable outcome or deliverable>

Assumptions:
- <List key assumptions>
- <What’s explicitly out-of-scope>

Three-Pillar Scores:
- Innovation (I): <1–5>  → Multiplier: <X.XX>
- Scope (S): <1–5>       → Multiplier: <X.XX>
- People (P): <1–5>      → Multiplier: <X.XX>

Scope Size:
- Units: <story points / features / tasks>
- Quantity: <N>
- Throughput: <units-to-days conversion>

Math:
- Base person-days = <N × throughput>
- Adjusted = Base × I × S × P
- P50 = <Adjusted>
- P70 = <Adjusted × 1.2>
- P90 = <Adjusted × 1.5>

Risks & Unknowns:
- <Top 3–5 risks>
- <Mitigations or spikes>

Dependencies:
- <Teams, vendors, approvals>

Decision/Trade-offs:
- If we drop X, we save ~Y days
- If we defer Z, risk reduces by ~R%

Next Steps (to refine estimate):
- <Spike A> (1–2 days) to validate <unknown>
- <Stakeholder review> to confirm scope
```

---

## JSON/YAML Template for Tooling

If you use scripts or dashboards, you can capture inputs like this:

```json
{
  "project": "New Analytics Dashboard",
  "date": "2025-01-15",
  "scope_units": 14,
  "throughput_days_per_unit": 0.8,
  "innovation_score": 3,
  "scope_score": 4,
  "people_score": 2,
  "multipliers": {
    "innovation": { "1": 0.9, "2": 1.0, "3": 1.2, "4": 1.5, "5": 2.0 },
    "scope":      { "1": 0.8, "2": 1.0, "3": 1.3, "4": 1.6, "5": 2.2 },
    "people":     { "1": 0.85, "2": 1.0, "3": 1.2, "4": 1.5, "5": 1.9 }
  },
  "confidence_factors": { "p70": 1.2, "p90": 1.5 },
  "assumptions": [
    "Single data warehouse source",
    "Two chart types at launch",
    "No SSO integration in v1"
  ]
}
```

```yaml
project: New Analytics Dashboard
date: 2025-01-15
scope_units: 14
throughput_days_per_unit: 0.8
innovation_score: 3
scope_score: 4
people_score: 2
confidence_factors:
  p70: 1.2
  p90: 1.5
assumptions:
  - Single data warehouse source
  - Two chart types at launch
  - No SSO integration in v1
```

---

## Example Walkthrough

Scenario:
- Outcome: MVP of a customer-facing dashboard with filtering and export.
- Constraints: Demo in 6 weeks.
- Team: One senior engineer (70%), one mid-level (50%), shared designer (25%).
- Risks: Unknown export format standard; new BI library.

Scores:
- Innovation (I) = 3 (some unknowns with BI library)
- Scope (S) = 4 (integrations with auth, data, export)
- People (P) = 3 (partial availability, mixed levels)

Scope and throughput:
- 16 story points at 0.75 days/point
- Base = 16 × 0.75 = 12 person-days

Multipliers:
- I=1.2, S=1.6, P=1.2 → Total = 1.2 × 1.6 × 1.2 = 2.304

Adjusted:
- P50 = 12 × 2.304 = 27.648 ≈ 28 person-days
- P70 = 28 × 1.2 = 33.6 ≈ 34 person-days
- P90 = 28 × 1.5 = 42 person-days

Calendar implications:
- With ~1.45 FTE (0.7 + 0.5 + 0.25×0.5 for design), say 7 person-days/week
- P50 timeline ≈ 4 weeks, P70 ≈ 5 weeks, P90 ≈ 6 weeks
- Conclusion: Demo feasible, but reserve scope cuts if risks materialize.

Trade-offs:
- Drop export v1 → save ~4–6 days; reduce risk
- Replace BI library with plain charts → save ~2–3 days learning curve

---

## Monte Carlo Option (When You Have 10 More Minutes)

Use a simple simulation to convert multipliers and scope variance into confidence ranges.

```python
import json, random, statistics as stats

config = {
    "scope_units": 16,
    "throughput_days_per_unit": 0.75,
    "multipliers": {"I": 1.2, "S": 1.6, "P": 1.2},
    "scope_variation": 0.25,   # ±25%
    "mult_variation": 0.10,    # ±10% each multiplier
    "trials": 5000
}

def sample_uniform(center, spread):
    return random.uniform(center*(1-spread), center*(1+spread))

def simulate(cfg):
    base = cfg["scope_units"] * cfg["throughput_days_per_unit"]
    samples = []
    for _ in range(cfg["trials"]):
        scope = sample_uniform(cfg["scope_units"], cfg["scope_variation"])
        i = sample_uniform(cfg["multipliers"]["I"], cfg["mult_variation"])
        s = sample_uniform(cfg["multipliers"]["S"], cfg["mult_variation"])
        p = sample_uniform(cfg["multipliers"]["P"], cfg["mult_variation"])
        samples.append((scope * cfg["throughput_days_per_unit"]) * i * s * p)
    samples.sort()
    def percentile(pct): return samples[int(len(samples)*pct)]
    return {
        "p50": percentile(0.50),
        "p70": percentile(0.70),
        "p90": percentile(0.90),
        "mean": stats.mean(samples),
        "stddev": stats.pstdev(samples)
    }

print(simulate(config))
```

This is not over-engineering; it provides a quick sanity check on your P70/P90.

---

## Spreadsheet-Friendly Formulas

- Base person-days:
  - = ScopeUnits × Throughput
- Total multiplier:
  - = I_Mult × S_Mult × P_Mult
- P50:
  - = Base × TotalMult
- P70:
  - = P50 × 1.2
- P90:
  - = P50 × 1.5

For Google Sheets with dropdowns and a lookup table:
- Suppose A2=InnovationScore, B2=ScopeScore, C2=PeopleScore, and you have a lookup table in H2:J6 for multipliers. Then:
  - I_Mult: =INDEX($H$2:$H$6, A2)
  - S_Mult: =INDEX($I$2:$I$6, B2)
  - P_Mult: =INDEX($J$2:$J$6, C2)
  - Total: =PRODUCT(I_Mult, S_Mult, P_Mult)

---

## Risk and Contingency: Aligning with Confidence

When nothing is certain, ranges are more honest than single numbers. Map risk appetite to buffers:

| Confidence | Factor | Use when… |
|------------|--------|-----------|
| P50        | 1.0×   | Internal planning, low penalty for slippage |
| P70        | 1.2×   | Stakeholder commitments with moderate risk |
| P90        | 1.5×   | External commitments, penalties, or launches |

Tip:
- Quote “P70: 5 weeks (range 4–6)” rather than “5 weeks.”
- Pair with key assumptions; commit to updating within 3–5 business days as unknowns clarify.

---

## Communication Template (Email/Slack)

```
Here’s a quick estimate for <Project> based on limited info:

P50: ~28 person-days
P70: ~34 person-days
P90: ~42 person-days

Assumptions:
- Single data source; no SSO
- Two visualizations at launch
- Partial team availability

Drivers (multipliers):
- Innovation=1.2 (new BI lib)
- Scope=1.6 (multiple integrations)
- People=1.2 (partial availability)

Top risks:
- Export format ambiguity
- Data quality variance

Next steps to reduce uncertainty (within 3 days):
- 1-day spike to validate export format
- Stakeholder review to confirm must-have charts

If we drop export in v1: save ~4–6 days.
```

---

## Calibrating Over Time

Your first multipliers are guesses. Make them better:

- Track: planned (P50) vs. actuals at a task/feature level.
- Categorize: routine vs. novel, integration-heavy vs. UI-heavy.
- Regress: adjust multipliers and throughput quarterly.
- Watch variance: if your P90 misses often, increase buffers.
- Codify: publish a 1-pager of “current org multipliers.”

Calibration practice:
- Compute Actual/Base for completed items.
- Compare by pillar scoring to see bias (e.g., people=4 often underestimates by 30%).
- Update multiplier table accordingly.

---

## Context Variations

- Software Delivery
  - Throughput via historical velocity: 1 point ≈ team-days/velocity.
  - Innovation spike tickets to de-risk libraries, API quotas, infra.
- Data Projects
  - Treat data quality/availability as innovation risk.
  - Scope includes pipelines, transformations, lineage, validation.
- Design/Research
  - Throughput in artifacts/week; innovation includes new user segments.
- Operations/Infrastructure
  - People multiplier more sensitive to change windows and approvals.
  - Scope includes environments, runbooks, rollback plans.

---

## Common Pitfalls and How to Avoid Them

- Pitfall: Anchoring on a single number.
  - Fix: Always present P50/P70/P90 with assumptions.
- Pitfall: Ignoring availability and context switching.
  - Fix: Use People multiplier and explicit FTE assumptions.
- Pitfall: Hidden scope in non-functional requirements.
  - Fix: Include deployment, security, observability in scope checklist.
- Pitfall: “Unknown unknowns” hand-waving.
  - Fix: Include a time-boxed spike to turn unknowns into knowns.
- Pitfall: No follow-up refinement.
  - Fix: Set a refinement checkpoint date in the estimate.

---

## A Lightweight Risk Checklist

- Integrations: new vendor, auth, rate limits?
- Data: quality, volume, latency, privacy?
- Compliance: approvals, audit, change windows?
- Performance: SLAs, load profiles?
- Environments: dev/test/stage/prod parity, infra readiness?
- People: key person risk, onboarding time?
- External: dependencies on other teams’ backlogs?

---

## Quick Reference: T-shirt Sizing Conversion

Use when tasks are non-technical or mixed-discipline.

| Size | Person-days (baseline) |
|------|------------------------|
| XS   | 0.5–1                  |
| S    | 1–3                    |
| M    | 3–5                    |
| L    | 5–8                    |
| XL   | 8–13                   |

Then apply I/S/P multipliers just as you would for points.

---

## Putting It All Together: A Worked Micro-Example

- Feature: “Add email passwordless login.”
- Assumptions: Use existing auth provider; mobile and web; no SSO v1.
- Scores: I=2, S=3, P=2 → Multipliers: 1.0 × 1.3 × 1.0 = 1.3
- Scope: 10 points; throughput 0.8 days/point → Base = 8 days
- P50: 8 × 1.3 = 10.4 ≈ 10.5 days
- P70: 12.6 days; P90: 15.8 days
- Communication: “P70: ~2.5 weeks for a 1-FTE engineer; risk reduced if we reuse existing session flows.”

---

## Implementation Snippet: CLI Estimator

For quick terminal usage:

```python
#!/usr/bin/env python3
import argparse

I_MULT = {1:0.9, 2:1.0, 3:1.2, 4:1.5, 5:2.0}
S_MULT = {1:0.8, 2:1.0, 3:1.3, 4:1.6, 5:2.2}
P_MULT = {1:0.85,2:1.0, 3:1.2, 4:1.5, 5:1.9}

def estimate(units, days_per_unit, i, s, p, p70=1.2, p90=1.5):
    base = units * days_per_unit
    total_mult = I_MULT[i] * S_MULT[s] * P_MULT[p]
    p50 = base * total_mult
    return p50, p50*p70, p50*p90

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--units", type=float, required=True)
    ap.add_argument("--days_per_unit", type=float, default=0.75)
    ap.add_argument("--i", type=int, required=True)
    ap.add_argument("--s", type=int, required=True)
    ap.add_argument("--p", type=int, required=True)
    args = ap.parse_args()
    p50, p70, p90 = estimate(args.units, args.days_per_unit, args.i, args.s, args.p)
    print(f"P50: {p50:.1f} days | P70: {p70:.1f} | P90: {p90:.1f}")
```

Usage:
- ./estimate.py --units 16 --days_per_unit 0.75 --i 3 --s 4 --p 2

---

## How to Present to Stakeholders

- Lead with outcomes and ranges:
  - “To deliver X, we estimate P70: 5 weeks (range 4–6), assuming Y.”
- Highlight top 2–3 uncertainties and the plan to reduce them.
- Offer scope levers: “If we drop A, we save B days.”
- Time-box learning: “We’ll run a 2-day spike and report back by Friday.”
- Ask for decisions: “We need sign-off on C to hold P70.”

---

## FAQ

- Why not just use story points?
  - Points are helpful, but in early stages you still need a conversion and a way to express uncertainty. The three-pillar multipliers make your assumptions explicit.
- Isn’t multiplying multipliers risky?
  - Yes, compounding can inflate quickly. That’s intentional—uncertainties multiply in real life. Calibrate to your context.
- What about fixed-price contracts?
  - Use P90 or higher and enumerate assumptions in the SOW. Price change orders for scope additions or assumption violations.
- Can this work outside software?
  - Yes. Substitute scope units with appropriate measures (deliverables, interviews, pages designed, servers configured).

---

## Further Reading

- Cone of Uncertainty: https://en.wikipedia.org/wiki/Cone_of_Uncertainty
- Brooks’s Law (team scaling risks): https://en.wikipedia.org/wiki/Brooks%27s_law
- Relative estimation and Story Points: https://www.scrum.org/resources/blog/what-are-story-points
- Monte Carlo in project management: https://en.wikipedia.org/wiki/Monte_Carlo_method

---

## Final Thoughts

When you have almost nothing to go on, the goal isn’t precision—it’s clarity. A quick, transparent estimate with Innovation, Scope, and People puts structure around ambiguity, communicates risk honestly, and creates a path to reduce uncertainty. Use this template to get to a credible P50/P70/P90 in under an hour, then iterate as you learn.

Ship the estimate, time-box the unknowns, and refine. That’s how you deliver under uncertainty.