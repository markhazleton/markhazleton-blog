# Building a Quick Estimation Template When You Have Almost Nothing to Go On

## Start with What Makes You Nervous

When you're staring at a list of vague requirements and someone wants estimates by tomorrow, your first instinct might be to just make up numbers. Don't. Instead, grab a tool like Claude and start by articulating what actually drives complexity in your work.

The key is to identify the 3-4 factors that genuinely affect effort. Not 10 factors, not 20. Just the few that matter. Think about what makes you groan when you see it in a requirement. Is it something completely new to your team? Is it massive in scope? Does it require coordinating multiple teams and skills? Those are your factors.

## Build the Simplest Possible Framework

Forget sophisticated algorithms. You want something you can explain in 30 seconds and calculate in your head. Rate each factor on a 1-10 scale - everyone understands 1-10. Then either add them (for linear complexity) or multiply them (for exponential complexity).

Addition works better when factors are independent. Multiplication works when factors compound each other. Most teams find addition more intuitive and predictable. You can always adjust with a multiplier later.

Your formula becomes:
- (Innovation + Scope + People) × Multiplier = Estimate

That's it. No machine learning, no complex weightings. Just simple math that anyone can verify.

### The Three Core Factors Explained

**Innovation (Have we done this before?)**
- 1-3: We've done this many times, have templates/patterns
- 4-6: We've done similar things, need some adaptation
- 7-10: Breaking new ground, R&D required, never attempted

**Scope (How big is it?)**
- 1-3: Small change, affects one component
- 4-6: Medium feature, multiple components
- 7-10: Major system, extensive changes across platform

**People (How complex is the coordination?)**
- 1-3: One person or single team, one skill set
- 4-6: 2-3 teams, multiple skills, some coordination
- 7-10: Many teams, diverse technology skills, heavy coordination

## Use Progressive Multipliers for Better Accuracy

Here's where using an AI assistant really helps - it can quickly generate the progressive logic you need. Instead of one multiplier for everything, use different multipliers based on the sum:

- Low complexity (sum 3-9): multiply by 2-3
- Medium complexity (sum 10-15): multiply by 4
- High complexity (sum 16-20): multiply by 5
- Very high (sum 20+): multiply by 6 or flag for breakdown

This accounts for the reality that complexity doesn't scale linearly. Small tasks stay small, but complex tasks explode in unexpected ways.

## Map to Whatever Format Your Organization Uses

If you're Agile, map to Fibonacci. If you use t-shirt sizes, create bands. If you need hours, use a conversion factor (but keep it hidden). The framework doesn't care about output format - it just generates consistent relative values.

The mapping can be a simple lookup table or nested IF statements. Let Claude write these for you - it's great at generating these tedious but necessary formulas.

## Calibrate with Whatever History You Have

Got even 5-10 completed items? Perfect. Work backwards:
1. Take a completed item
2. Rate it with your Innovation, Scope, and People factors
3. Calculate what multiplier would have given you the actual result
4. Average across several items
5. That's your team's multiplier

No history? Start with a multiplier of 3 and adjust after your first few completions. The exact number matters less than having a consistent method.

## Make It Defendable, Not Perfect

The goal isn't perfect estimates - it's estimates you can explain and adjust. When someone challenges a number, you can show:
- The specific Innovation, Scope, and People ratings
- Why you rated each factor that way
- How changing any rating affects the estimate
- Which completed work validates your approach

This transparency builds trust even when estimates are wrong.

## Build in Automatic Reality Checks

Add a column for "actual effort" and calculate variance automatically. Add conditional formatting to highlight when estimates are off by more than 50%. This isn't about shame - it's about continuous calibration.

Track patterns: Are innovative tasks always underestimated? Does your team consistently underrate the People complexity? These patterns help you adjust ratings going forward, not just multipliers.

## Keep the Cognitive Load Low

If you're spending more than 2 minutes per estimate, your framework is too complex. The power isn't in sophistication - it's in consistency and speed. You should be able to estimate 50 items in under an hour.

This means:
- Clear definitions for Innovation (have we done this?), Scope (how big?), and People (how many skills/teams?)
- Reference examples for common patterns
- No second-guessing or over-analysis
- Trust the framework even when it feels wrong

## Use AI to Handle the Tedious Parts

Claude or similar tools excel at:
- Generating Excel formulas for your Innovation + Scope + People logic
- Creating validation rules
- Building reference tables
- Writing calibration calculations
- Producing multiple format outputs from the same data

Don't waste time on formula syntax or Excel functions. Describe what you want and let AI write the implementation.

## Embrace Continuous Refinement

Your first version will be wrong. That's fine. The framework gives you something to be wrong WITH, which is infinitely better than being wrong without any structure.

After each sprint/milestone/project:
1. Compare estimates to actuals
2. Look for systematic bias in Innovation, Scope, or People ratings
3. Adjust either ratings definitions or multipliers
4. Document what you learned

Within 3-4 cycles, your accuracy will improve dramatically.

## The Psychology Matters

Having a framework - any framework - changes the conversation from "how did you guess that?" to "let's discuss these ratings." It moves you from defending random numbers to discussing specific complexity factors.

This is huge. Stakeholders can engage with "I rated People complexity as 7 because we need frontend, backend, DBA, and DevOps coordination" in a way they can't with "my gut says 3 weeks."

## Don't Hide the Simplicity

Be upfront that this is a rapid estimation tool based on simple math. Don't pretend it's more sophisticated than it is. The transparency is a feature, not a bug.

When presenting estimates:
- Show the Innovation + Scope + People framework openly
- Explain it takes 30 seconds per item
- Acknowledge the uncertainty
- Focus on relative sizing, not absolute precision

## LLM Prompts to Get You Started

Getting started with an AI assistant to build your framework? Here are proven prompts that will generate exactly what you need:

### For Creating Your Initial Framework:
```
"I need to estimate 50 IT tasks with minimal information. Create an Excel formula that:
- Takes 3 factors rated 1-10 (Innovation, Scope, People)
- Innovation = have we done this before? (1=many times, 10=never)
- Scope = how big is it? (1=tiny, 10=massive)
- People = coordination complexity, teams, skills needed (1=single person, 10=many teams)
- Adds them together
- Applies progressive multipliers based on the sum
- Maps the result to Fibonacci numbers (1,2,3,5,8,13,21,34,55,89)"
```

### For Building the Spreadsheet:
```
"Create a Python script using openpyxl that generates an Excel estimation template with:
- Headers for task name, category, Innovation (have we done this?), Scope (how big?), People (teams/skills needed)
- All three factors rated 1-10
- Data validation limiting ratings to 1-10
- Automatic calculation: (Innovation + Scope + People) × Progressive Multiplier
- Conditional formatting for variance over 50%
- A calibration sheet that calculates suggested multipliers from historical data"
```

### For Generating Reference Tables:
```
"Create a reference table showing typical Innovation, Scope, and People ratings for common IT tasks:
- Simple bug fix (done many times, small scope, one developer)
- New API endpoint (done before but needs adaptation, medium scope, backend team)
- Database migration (somewhat new, large scope, multiple teams)
- Full microservice (never done, large scope, many teams and skills)
- UI dashboard (done similar, medium scope, designer + frontend)
Include suggested ratings for each factor"
```

### For Calibration Logic:
```
"Write an Excel formula that:
- Takes completed story points in column A
- Takes Innovation rating in column B
- Takes Scope rating in column C  
- Takes People rating in column D
- Calculates the sum in column E
- Calculates the implied multiplier in column F
- Provides an average multiplier recommendation at the bottom"
```

### For Creating Validation Rules:
```
"Generate Excel data validation rules and formulas that will:
- Ensure Innovation, Scope, and People are between 1-10
- Flag any item where Innovation + Scope + People > 25 as 'must split'
- Highlight variance between estimate and actual over 50% in red
- Show green when estimates are within 20% of actuals
- Create dropdown lists for confidence levels based on how well we know the Innovation/Scope/People factors"
```

### For Format Conversions:
```
"Create nested IF statements that convert (Innovation + Scope + People) × Multiplier estimates to:
1. Story points in Fibonacci sequence
2. T-shirt sizes (XS, S, M, L, XL, XXL)
3. Time ranges (days, weeks, months)
4. Team capacity (what percentage of a sprint)
Make it work with Innovation+Scope+People sums from 3 to 30"
```

### For Documentation:
```
"Write a one-page guide explaining this estimation framework to stakeholders. Include:
- Innovation: Have we done this before? (1=yes many times, 10=never)
- Scope: How big is this? (1=tiny change, 10=massive undertaking)
- People: How complex is coordination? (1=one person, 10=many teams/skills)
- How we add these three factors and apply a multiplier
- Why this gives us consistent, defendable estimates
Keep it non-technical and focus on the business value"
```

## Common Mistakes to Avoid

### Overcomplicating the Factors
Don't split Innovation into "technical innovation" and "business innovation." Don't divide People into "internal teams" and "external vendors." Keep it simple: Have we done this? How big? How many people/skills involved?

### Confusing Scope with People
Scope is about the size of the work itself. People is about coordination complexity. A large data migration (Scope=8) might only need one DBA (People=2). Keep them separate.

### Rating Innovation Based on Industry Standards
Innovation means "have WE done this before?" - not whether it exists in the world. If your team has never built a REST API, that's high Innovation for you, even though it's standard in the industry.

### Ignoring Hidden People Complexity
Remember to account for all the people involved: developers, testers, designers, product owners, stakeholders, deployment teams. If you're coordinating across all these groups, that's high People complexity.

### Using Raw Multiplication
Multiplying Innovation × Scope × People gives you a range of 1-1000. That's too wide. Addition (Innovation + Scope + People) gives you 3-30, which is much more manageable.

### Over-Precision in Ratings
The difference between Innovation=6 and Innovation=7 doesn't matter. Use rough bands:
- 1-3: Low (we've done this often)
- 4-6: Medium (we've done similar)
- 7-9: High (this is new to us)
- 10: Extreme (complete unknown)

### Not Considering All Skills in People Rating
People complexity isn't just about headcount. Three developers with the same skills = low People complexity. Frontend + backend + DBA + DevOps = high People complexity, even if it's still just four people.

### Forgetting About Estimation Fatigue
After about 20 estimates, accuracy drops. Take breaks. Or better yet, estimate in small batches across multiple sessions.

### Using the Same Multiplier for Everything
A multiplier that works for innovative work might be wrong for routine tasks. Consider different multipliers for different sum ranges (progressive multipliers).

### Not Tracking Confidence
Add a confidence column based on how certain you are about Innovation, Scope, and People. Low confidence items need buffer.

### Expecting Perfection Too Soon
Your first 50 estimates will be wrong. That's fine. By your third iteration (150 items estimated and completed), you'll be within 20% accuracy on most items.

## Real Calculation Examples

Let's walk through exactly how the math works with concrete numbers:

### Example 1: Simple Bug Fix
- **Innovation:** 2 (we fix similar bugs weekly)
- **Scope:** 2 (single component affected)
- **People:** 1 (one developer, no coordination)
- **Sum:** 5
- **Progressive Multiplier:** ×2 (for low complexity)
- **Raw Score:** 10
- **Fibonacci Mapping:** → 8 points

### Example 2: New Customer Portal Feature
- **Innovation:** 5 (we've done similar portals, but this has new elements)
- **Scope:** 6 (multiple screens, database changes, API updates)
- **People:** 4 (frontend, backend, UX designer, product owner)
- **Sum:** 15
- **Progressive Multiplier:** ×4 (for medium-high complexity)
- **Raw Score:** 60
- **Fibonacci Mapping:** → 55 points

### Example 3: First Machine Learning Integration
- **Innovation:** 8 (we've never done ML before)
- **Scope:** 7 (new infrastructure, training pipeline, API integration)
- **People:** 7 (data scientists, backend, DevOps, multiple vendors)
- **Sum:** 22
- **Progressive Multiplier:** ×5 (for high complexity)
- **Raw Score:** 110
- **Fibonacci Mapping:** → 89 points (consider splitting)

### Example 4: OAuth Implementation (Your Real Example)
- **Innovation:** 8 (new OAuth pattern for us)
- **Scope:** 5 (moderate - touches authentication everywhere)
- **People:** 6 (frontend, backend, security team, possibly PM/design)
- **Sum:** 19
- **Progressive Multiplier:** ×4.7 (calibrated from actuals)
- **Raw Score:** 89.3
- **Actual:** 89 points ✓

### Calibration Example
You estimated a task:
- Innovation: 4 (we've done similar)
- Scope: 5 (medium-sized feature)
- People: 3 (two developers coordinating)
- Sum: 12, Multiplier: 3, Estimate: 36 → 34 points

It actually took 55 points.
Implied multiplier: 55÷12 = 4.6

This tells you to increase your multiplier for similar complexity tasks from 3 to about 4.5.

## Frequently Asked Questions

### Q: How do I handle tasks that don't fit the Innovation-Scope-People framework?
Spike them. If you can't rate how new it is, how big it is, or who's involved, you don't know enough to estimate. Time-box an investigation, then estimate.

### Q: What if Innovation is low but People complexity is high?
That's perfectly valid. Routine work (Innovation=2) that requires coordinating five teams (People=8) is still complex overall. The addition model handles this well.

### Q: Should I rate People based on headcount or skill diversity?
Skill diversity. Five backend developers working together = People:3. Frontend + backend + DBA + DevOps + QA = People:7, even though it's still five people.

### Q: How do I rate Innovation for upgrades or migrations?
Consider how familiar your team is with both the old and new systems. Migrating from tech you know well to tech you've never used = high Innovation.

### Q: What if Scope keeps changing during development?
Re-estimate when scope changes significantly. Track both original and revised estimates to improve your Scope rating accuracy over time.

### Q: Can this work for non-technical estimates?
Absolutely. Innovation becomes "Have we done this type of project?", Scope remains size, and People covers coordination across departments/skills/stakeholders.

### Q: How do I handle dependencies between tasks?
Don't. Estimate each task's Innovation, Scope, and People independently. Add buffer at the project level for integration.

### Q: What's the minimum historical data needed to calibrate?
Five completed items give you a starting point. Ten give you confidence. Twenty give you reliability. But even one is better than none.

### Q: Should People complexity include the customer/end users?
Only if they're actively involved during development (user testing, feedback sessions). Passive end users don't add to People complexity.

### Q: How often should I recalibrate?
After every major delivery or every 20-30 completed items. As your team gains experience, Innovation ratings for similar work should decrease.

### Q: What if we've literally never done something before (Innovation=10)?
Consider splitting it into a spike/proof-of-concept first. True Innovation=10 items are too uncertain for accurate estimation.

### Q: Should the People rating include management stakeholders?
Yes, if they're actively involved in decisions during development. A stakeholder who just receives status updates doesn't add complexity. One who reviews every screen does.

## Template Download Section

Ready to implement this framework? Here's what you'll get:

### Basic Estimation Template (Excel)
- Pre-built formulas for Innovation + Scope + People estimation
- Clear column headers with descriptions
- Progressive multipliers configured
- Fibonacci mapping included
- Calibration worksheet
- Variance tracking

### Advanced Features Available:
- Multiple category support with different multipliers
- Squad planning calculations for parallel execution
- Velocity tracking across sprints
- Confidence level based on certainty of Innovation/Scope/People ratings
- Hidden hours conversion (for those who need it)

### Implementation Checklist:
- [ ] Define Innovation scale for your team (what's a 3 vs 7?)
- [ ] Define Scope scale with examples
- [ ] Define People complexity with skill/team examples
- [ ] Set initial multipliers (start with 3)
- [ ] Estimate first batch (10-20 items)
- [ ] Track actuals religiously
- [ ] Calibrate after first completion
- [ ] Document lessons learned
- [ ] Share framework with team
- [ ] Iterate and improve

## Final Thoughts

The perfect estimation framework doesn't exist. But a simple, transparent, adjustable framework beats guessing every time. Use AI to build it quickly, implement it immediately, and refine it continuously.

The key insight: you don't need much information to start estimating systematically. You just need a consistent way to capture the little information you have. Three factors - Innovation (have we done this?), Scope (how big?), and People (who's involved?) - plus simple math and regular calibration will get you surprisingly far.

And when someone asks how you estimated 50 items so quickly? Show them the spreadsheet, explain that you rate how new it is to your team, how big it is, and how many people/skills are involved, then watch them nod along. That's when you know you've built something useful.

Remember: The goal isn't to predict the future perfectly. It's to be consistently wrong in a way you can measure and correct. That's infinitely more valuable than random guessing, and it gets better every single time you use it.