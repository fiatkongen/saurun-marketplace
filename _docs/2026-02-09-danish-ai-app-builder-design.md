# Danish AI Prototype Tool — Design Document

**Date:** 2026-02-09
**Status:** Draft (v2 — Lead-gen model)
**Author:** Brainstorm session
**BRUTAL Review:** Completed (v1 + v2 comparison included)

---

## Executive Summary

**This is NOT a standalone product.** It's a lead-generation funnel for [done-in-21.dk](https://done-in-21.dk) — an existing Danish software development service that delivers finished software in 21 working days with a money-back guarantee.

### The Strategy

```
[AI Prototype Landing Page]
         ↓
   [Conversational Q&A]
         ↓
   [AI builds PROTOTYPE]
         ↓
   [3 judges validate idea]
         ↓
   [User sees result]
         ↓
    ┌────┴────┐
    ↓         ↓
[Fee for code]    →    [Upsell to done-in-21]
(break-even)                    ↓
                        [21-day sprint]
                        [Day 7 money-back guarantee]
                        [Finished product day 21]
```

### Key Insight

The prototype IS days 1-2 of done-in-21's process (clarifying business logic + user scenarios). When a customer converts, the spec is already done — done-in-21 can start from day 3.

### Value Proposition

> "Se din idé blive til virkelighed — på 10 minutter."
>
> Vores AI hjælper dig med at forme din idé og bygger en prototype du kan se og røre. Vil du have det færdigt? Vi bygger det på 21 dage — eller du betaler ingenting.

---

## done-in-21.dk Integration

### What done-in-21 Offers

| Aspect | Details |
|--------|---------|
| **Core offer** | Finished software in 21 working days, fixed price |
| **Guarantee** | Day 7 money-back (reject prototype = pay 0 kr) |
| **Target** | SMVs and startups with simple projects |
| **Constraints** | 1-2 user roles, 0-2 integrations, describable in 5 sentences |
| **Price range** | SMV: 50-150k DKK / Startups: 150-500k DKK |
| **Cases** | Booking systems, internal dashboards, MVPs, process automation |

### Synergies

| Synergy | Why it works |
|---------|--------------|
| **AI prototype = day 1-2** | Business logic + scenarios already clarified |
| **3 judges = day 7 concept** | Customer is used to go/no-go decision points |
| **Spec is done** | done-in-21 can start from day 3, not day 1 |
| **Qualified leads** | People who complete the full flow ARE your ideal customer |
| **Risk pyramid** | Free Q&A → cheap fee → done-in-21's day 7 guarantee |

### Capability Filtering

The prototype tool should filter OUT projects that don't fit done-in-21:

| Target | Fits done-in-21? | Action |
|--------|------------------|--------|
| Landing page | ✅ Simple (21 days is overkill) | Offer lite package or code-only |
| Webshop | ⚠️ Depends on complexity | Filter if 3+ integrations |
| Booking system | ✅ Perfect match | Full upsell |
| Business tool | ✅ Perfect match | Full upsell |
| App/platform | ⚠️ Can become too complex | Filter if 3+ user roles |
| Andet | ❓ Unknown | Route to 30 min consultation |

**If AI detects 3+ user roles or 3+ integrations:** "Dette projekt kræver en konsultation først" → book 30 min.

---

## Market Opportunity

### Gap Identified

| Current Option | Problem |
|----------------|---------|
| Global AI builders (Lovable, Bolt) | English-only, assume you know what to build, no guidance |
| Danish development agencies | 100,000-1,000,000+ DKK, months of work, intimidating process |
| DIY no-code (Bubble, Webflow) | Still requires learning curve, no help with ideation |
| Business consultants | Help with ideas, but don't build anything |
| Idea validation tools (ValidatorAI) | Validate ideas, but don't build |

**The gap:** No service combines **idea development + validation + prototype** in one Danish-language experience, connected to a real development service.

### Target Market

**Primary:** Danish SMVs and startups who:
- Have app/software ideas but can't code
- Don't want to book a 30 min consultation (yet)
- Want to "try first" before committing
- Have budget for done-in-21 (50-500k DKK) if convinced

**Segment we're capturing:** People who would NOT book a consultation, but WILL try a free/cheap prototype tool.

---

## Product Design

### User Journey (8 Phases)

```
[Landing page] → [Click "Start"] → [Target selection] → [Conversational Q&A]
    → [Idea Summary] → [Expert Validation (3 judges)] → [Prototype Built]
    → [View Result + Expert Review]
    → [Choice: Code fee OR done-in-21 upsell]
```

### Phase 1: Landing Page (Explain-First)

| Section | Purpose | Content |
|---------|---------|---------|
| **Hero** | Hook + CTA | "Se din idé blive til virkelighed — på 10 minutter" + [Prøv gratis] |
| **How it works** | 3 steps | 1) Fortæl din idé 2) AI bygger prototype 3) Se resultatet |
| **Examples** | Social proof | 3-4 prototype examples |
| **What you get** | Clarity | "Prototype + ekspertvurdering + mulighed for færdigt produkt" |
| **done-in-21 connection** | Upsell setup | "Vil du have det bygget rigtigt? 21 dage, pengene tilbage garanti" |
| **FAQ** | Objections | "Er det en rigtig app?" "Hvad koster det færdige produkt?" |
| **Final CTA** | Convert | [Start din prototype nu] |

### Phase 2: Target Selection (Structured)

> **Hvad vil du gerne bygge?**
> - En simpel hjemmeside (landing page)
> - En webshop
> - Et bookingsystem
> - Et værktøj til min virksomhed
> - En app eller platform
> - Noget andet

### Phase 3: Conversational Q&A (AI-Driven)

**Approach:** Intelligent conversational AI that tracks completeness internally.

**Required dimensions per target:**

| Target | Required dimensions |
|--------|---------------------|
| **Landing page** | Purpose, CTA, sections, content status, integrations, style |
| **Webshop** | Products, quantity, payment, shipping, inventory, accounts |
| **Booking** | What's booked, who books, calendar logic, payment, notifications |
| **Business tool** | Users/roles, data types, workflows, integrations, access control |
| **App/platform** | Core actions, user types, interactions, notifications, monetization |

**AI behavior:**
- Natural conversation, not interrogation
- ONE follow-up at a time
- No technical jargon
- Reference familiar apps ("Som Instagram, men for...")
- Move to confirmation when ~80% complete

### Phase 4: Idea Summary

Structured confirmation before building.

### Phase 5: Expert Validation (Pre-Build)

**The 3 judges review the IDEA before building:**

> **Vores eksperter har kigget på din idé:**
>
> 🎨 **Designeren:** "God brugeroplevelse. Overvej at forenkle bookingflowet."
>
> ⚙️ **Teknikeren:** "Teknisk realistisk. Start uden chat-funktion."
>
> 👤 **Brugeren:** "Jeg ville bruge dette! Priserne skal være synlige."

**Strategic purpose:** Teaches customer to appreciate complexity → better conversion to done-in-21.

### Phase 6: Prototype Built (Async)

- AI builds prototype (NOT production-ready)
- Email notification when complete
- No live code view

#### Future Optimization: Parallel Frontend+Backend Build

**Status:** v2 optimization — NOT for MVP. Implement when prototype flow is validated.

**Concept:** Contract-first parallel development.

```
[Spec] → [API Contract] → ┌─ [Backend implementation]
                           └─ [Frontend implementation + style + assets]
                                    ↓
                              [Integration]
```

**How it works:**
1. Generate API contract from spec (endpoints, request/response shapes)
2. Backend builds real implementation against the contract
3. Frontend builds against mock data from the same contract
4. Style exploration + asset generation happens during frontend build
5. When both done → swap mock for real API → integration test

**When to activate (threshold):**

| Contract size | Approach | Why |
|---------------|----------|-----|
| < 8 endpoints | Sequential | Overhead of contract + mocking + integration ≈ time saved |
| 8+ endpoints | Parallel | 30-40% time savings justify the coordination cost |

**Time savings analysis:**

| Project type | Sequential | Parallel | Saving |
|--------------|-----------|----------|--------|
| Simple (3-5 endpoints) | ~40 min | ~35 min | ~5 min (not worth it) |
| Medium (8-12 endpoints) | ~90 min | ~60 min | ~30 min (worth it) |
| Complex (15+ endpoints) | ~120 min | ~85 min | ~35 min (clearly worth it) |

**Risks to mitigate:**
- **Contract drift:** Backend discovers entity-relation issue → contract changes → frontend rework. Mitigation: keep contracts simple, validate before parallel dispatch.
- **AI contract quality:** AI-generated contracts may have inconsistencies. Mitigation: add contract validation step.

**Style exploration during build:**
When parallel mode is active, use frontend build time to ask user about style preferences:
- "Mens vi bygger, lad os tale om udseendet..."
- Show 3-4 style directions
- Generate assets in parallel
- Turns "dead wait time" into "engaged customization time"

**For MVP:** Build sequentially. Ask style questions during Q&A phase (Phase 3) as one more dimension.

### Phase 7: View Result + Expert Review (Post-Build)

**The 3 judges review the RESULT:**

Same judges, now reviewing the actual prototype. Points out strengths AND limitations.

### Phase 8: Conversion Point

Two options presented:

| Option | Price | What they get |
|--------|-------|---------------|
| **Code package** | ~break-even (2-5k DKK) | Near-production code, documentation, but explicit gaps |
| **done-in-21 full service** | [Book konsultation] | Finished product in 21 days, day 7 guarantee |

**Upsell messaging:** "Du har allerede prototypen og specifikationen. Vi kan bygge det færdigt på 21 dage — eller du betaler ingenting."

---

## Key Differentiators

### Primary: Risk Reversal Stack

```
[Free Q&A] → [Cheap prototype fee] → [done-in-21 day 7 guarantee]
     ↓              ↓                         ↓
  Zero risk    Low commitment          Full money-back
```

Each step reduces perceived risk.

### Secondary Differentiators

| Differentiator | vs. Competitors |
|----------------|-----------------|
| **Danish-first** | Lovable/Bolt are English-only |
| **Helps you figure out WHAT** | Competitors expect you to know |
| **Expert validation at both ends** | No competitor offers this |
| **Connected to real dev service** | Not a dead end — path to finished product |
| **Prototype, not promise** | See something tangible before committing |

---

## BRUTAL Review Summary

### Rating Comparison: v1 vs. v2

| Metric | v1 (Standalone) | v2 (Lead-gen) | Change |
|--------|-----------------|---------------|--------|
| **GR Average Score** | 4.0/10 | 5.2/10 | **+1.2** |
| **Fatal Flaws** | 4 | 1-2 | **Improved** |
| **Survival Chance (RT)** | "Microscopic" | 8-12% | **Improved** |

### v2 Plate Scores (Gordon Ramsay)

| Dimension | Score | Notes |
|-----------|-------|-------|
| Problem Clarity | 7/10 | Clear problem, need to quantify pain |
| Customer Specificity | 6/10 | Need named personas with budgets |
| Revenue Logic | 4/10 | Break-even model needs unit economics |
| Competitive Moat | 3/10 | "Danish-first" is 6-month moat max |
| Execution Realism | 5/10 | Need capability matrix for AI |
| Scalability | 6/10 | Flywheel potential if data collected |

### Problems Solved by v2

| v1 Problem | v2 Solution |
|------------|-------------|
| AI code must work in production | Prototype only — explicit limitations |
| Unlimited support burden | done-in-21 takes over |
| CAC > LTV uncertainty | CAC paid by done-in-21 LTV |
| Entire business at risk | One marketing channel at risk |
| Customer stranded after delivery | done-in-21 includes support |

### Remaining Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| **Lead quality (tire-kickers)** | High | Minimum viable fee to filter |
| **Prototype quality paradox** | High | Define explicit capability limits |
| **Competitor copies concept** | Medium | Build data flywheel for moat |
| **Cannibalization of existing sales** | Medium | Test with real customers first |

### Kill Shots (Red Team)

| # | Vulnerability | Likelihood | Impact |
|---|---------------|------------|--------|
| 1 | Lead quality is illusion | High | Fatal |
| 2 | Prototype too good OR too bad | High | Severe |
| 3 | Competitor copies + outspends | Medium-High | Fatal |

### Critics' Consensus

> Worth testing — but not worth 6 months of building. Ship v0.1 in 2-3 weeks, measure conversion, iterate or kill.

---

## Steelman Alternatives (from BRUTAL Review)

The Devil's Advocate proposed 4 alternatives worth considering:

### Alternative 1: Spec-as-a-Service (No Code)

Drop AI building entirely. Keep conversation + judges. Output: 3-page spec document.

**Pros:** Zero cannibalization risk, zero code quality concerns, pure lead-gen.

### Alternative 2: Inverted Funnel (Judges First)

Judges review IDEA first. Only build prototype if idea is promising.

**Pros:** Filters waste-of-time ideas before compute spend.

### Alternative 3: Premium Prototype (10k DKK)

Prototype costs 10k DKK. 100% refunded if they buy done-in-21.

**Pros:** Only serious leads pay. Eliminates tire-kickers.

### Alternative 4: AI as Co-Pilot (Not Self-Service)

Keep 30 min consultation. AI generates prototype LIVE in the meeting.

**Pros:** Human touch preserved. More impressive demo. Higher conversion.

---

## Open Questions

1. **Prototype pricing:** Break-even at what level? 2k? 5k? 10k?
2. **Cannibalization:** Will this reduce 30 min consultation bookings?
3. **Capability matrix:** Exactly what can/can't the AI build?
4. **Data flywheel:** How do we structure learning from every prototype?
5. **Conversion target:** What's acceptable? 5%? 10%? 20%?
6. **Kill switch:** At what metrics do we shut this down?

---

## Next Steps (Prioritized)

### Week 1-2: Prototype of the Prototype

1. [ ] Build simplest version: chat → generate ONE type (booking system) → show result
2. [ ] Test with 20 real SMVs from network
3. [ ] Measure: completion rate, feedback, conversion to consultation
4. [ ] **Success metric:** 30%+ completion, 15%+ book meeting

### Week 3-4: Define Limits Brutally

1. [ ] List 20 project types AI CAN build
2. [ ] List 10 things it CANNOT
3. [ ] Build capability matrix
4. [ ] Communicate clearly on landing page

### Week 5+: Build Flywheel

1. [ ] Structure data collection from every prototype
2. [ ] After 100 prototypes: analyze what converts
3. [ ] Use learnings to improve AI + targeting

### Kill Switch Criteria

If after month 3:
- Conversion to consultation < 8%
- Completion rate < 25%
- NPS < 6

→ Shut down and redirect to Alternative 4 (AI as co-pilot in consultations).

---

## Appendix A: Competitive Analysis Summary

- **Lovable:** $25/mo, best for non-tech, English-only
- **Bolt.new:** $20/mo, token-based, English-only
- **Danish agencies:** 100K-1M+ DKK, weeks/months
- **Gap:** No Danish-language prototype tool connected to real dev service

---

## Appendix B: done-in-21 Service Details

**Website:** [done-in-21.dk](https://done-in-21.dk)

**Process:**
- Day 1-7: Running prototype with core concept
- Day 8-14: Feature-complete product ready for testing
- Day 15-21: Production-ready, polished, documented

**Guarantees:**
- Day 7: Reject prototype = pay 0 kr
- Fixed price, fixed deadline
- 100% code ownership from day 1
- 7 days post-launch support

**Ideal projects:**
- Booking systems
- Internal dashboards
- MVPs for validation
- Process automation

**NOT for:**
- Legacy modernization
- Complex multi-system integrations
- 4+ integrations
- Advanced permissions architecture

---

## Appendix C: Full BRUTAL Review Transcripts

Available on request. Key findings integrated into main document.

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| v1 | 2026-02-09 | Initial design (standalone product) |
| v2 | 2026-02-09 | Pivoted to lead-gen for done-in-21 |
| v2.1 | 2026-02-09 | Added BRUTAL Review v1 + v2 comparison |
