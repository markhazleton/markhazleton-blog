---
description: "Task list for Mobile-First Website Optimization feature implementation"
---

# Tasks: Mobile-First Website Optimization

**Feature**: 001-mobile-first  
**Branch**: `001-mobile-first`  
**Input**: Design documents from `/specs/001-mobile-first/`  
**Prerequisites**: plan.md (architecture), spec.md (user stories), research.md (decisions), data-model.md (configuration), contracts/frontend-contracts.md (patterns)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic responsive framework structure

- [ ] T001 Verify existing project structure and build system in tools/build/build.js
- [ ] T002 Create responsive SCSS module structure: src/scss/_variables.scss (typography scale)
- [ ] T003 [P] Create src/scss/_typography.scss (mobile-first typography system)
- [ ] T004 [P] Create src/scss/_navigation.scss (bottom nav styles)
- [ ] T005 [P] Create src/scss/_responsive.scss (breakpoint utilities)
- [ ] T006 Update src/scss/styles.scss to import new responsive modules
- [ ] T007 [P] Update src/scss/modern-styles.scss to import new responsive modules
- [ ] T008 Verify viewport meta tag in src/pug/layouts/modern-layout.pug
- [ ] T009 Create PUG navigation module in src/pug/modules/navigation.pug with bottomNav mixin
- [ ] T010 [P] Add responsiveImage mixin to src/pug/modules/article-mixins.pug

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core responsive infrastructure that MUST be complete before ANY user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T011 Implement CSS custom properties for typography scale (1.200 ratio) in src/scss/_variables.scss
- [ ] T012 Implement breakpoint-responsive base font size adjustments (16px→17px→18px) in src/scss/_variables.scss
- [ ] T013 Implement mobile-first typography system with line-height and spacing in src/scss/_typography.scss
- [ ] T014 Implement line-length control with ch units (60ch mobile, 75ch tablet+) in src/scss/_typography.scss
- [ ] T015 Implement touch target mixin with 44x44px minimum in src/scss/_responsive.scss
- [ ] T016 Apply touch target mixin to Bootstrap buttons and form controls in src/scss/_responsive.scss
- [ ] T017 Build SCSS files with npm run build:scss to verify compilation
- [ ] T018 Test responsive typography at all breakpoints (375px, 768px, 1024px, 1200px) in browser devtools

**Checkpoint**: Foundation ready - responsive typography and utilities available for all user stories

---

## Phase 3: User Story 1 - Mobile Device Primary Experience (Priority: P1) 🎯 MVP

**Goal**: Visitors on mobile devices can comfortably read content, navigate, and interact with touch gestures without zooming or horizontal scrolling

**Independent Test**: Load homepage on smartphone at 375px width → verify no horizontal scroll, 16px readable text, 44x44px touch targets, bottom navigation accessible

### Implementation for User Story 1

- [ ] T019 [P] [US1] Implement bottom sticky navigation styles (fixed position, z-index 1030) in src/scss/_navigation.scss
- [ ] T020 [P] [US1] Add safe-area-inset-bottom for iPhone notch support in src/scss/_navigation.scss
- [ ] T021 [P] [US1] Add 60px bottom padding to body element on mobile in src/scss/_navigation.scss
- [ ] T022 [US1] Implement bottom navigation tablet/desktop behavior (static position) in src/scss/_navigation.scss
- [ ] T023 [US1] Create bottomNav PUG mixin with icon+label structure in src/pug/modules/navigation.pug
- [ ] T024 [US1] Integrate bottomNav mixin into modern-layout.pug with 5 primary nav items
- [ ] T025 [US1] Apply responsive image pattern to article templates with loading="lazy" for below-fold images in src/pug/articles/*.pug
- [ ] T026 [US1] Update homepage index.pug to use responsive image patterns with proper loading attributes
- [ ] T027 [US1] Build site with npm run build and test mobile navigation at 375px width
- [ ] T028 [US1] Verify all interactive elements meet 44x44px touch target requirement using browser devtools
- [ ] T029 [US1] Test homepage content loads without horizontal scroll on mobile (320px-767px)
- [ ] T030 [US1] Test bottom navigation functionality: tap targets respond accurately, active states work
- [ ] T031 [US1] Validate mobile performance with npm run audit:perf (target FCP < 1.5s)

**Checkpoint**: Mobile experience fully functional - readable text, touch-friendly navigation, no horizontal scroll

---

## Phase 4: User Story 4 - Cross-Device Typography Consistency (Priority: P1)

**Goal**: All visitors experience consistent, readable typography meeting WCAG 2.1 AA standards across all devices

**Independent Test**: Check typography at 375px, 768px, 1200px → verify 16px+ body text, 1.5-1.6 line height, 4.5:1 contrast ratio, text zoom to 200% works

### Implementation for User Story 4

- [ ] T032 [P] [US4] Apply font-size CSS custom properties to all heading elements (h1-h6) in src/scss/_typography.scss
- [ ] T033 [P] [US4] Set body text line-height to 1.6 and heading line-height to 1.3 in src/scss/_typography.scss
- [ ] T034 [P] [US4] Apply consistent margin/spacing to typography elements in src/scss/_typography.scss
- [ ] T035 [US4] Create .article-content class with line-length control and typography rules in src/scss/_typography.scss
- [ ] T036 [US4] Update article PUG templates to wrap content in .article-content container in src/pug/articles/*.pug
- [ ] T037 [US4] Test typography scale at mobile (375px), tablet (768px), desktop (1200px) breakpoints
- [ ] T038 [US4] Verify 16px minimum body text size across all pages at all breakpoints
- [ ] T039 [US4] Test browser text zoom to 200% - verify no horizontal scroll and maintained readability
- [ ] T040 [US4] Validate text contrast ratios with npm run audit:a11y (target 4.5:1 for body text)
- [ ] T041 [US4] Test line length: mobile 35-60 characters, tablet/desktop 45-75 characters per line

**Checkpoint**: Typography system complete and validated across all devices and zoom levels

---

## Phase 5: User Story 2 - Tablet and Medium Device Experience (Priority: P2)

**Goal**: Tablet visitors experience optimized multi-column layouts with appropriate content density while maintaining touch-friendly interactions

**Independent Test**: Load site on iPad at 768px-1024px → verify 2-3 column layouts, static header navigation, 45-75 character line length, touch targets adequate

### Implementation for User Story 2

- [ ] T042 [P] [US2] Implement tablet navigation transition (bottom→header static) in src/scss/_navigation.scss
- [ ] T043 [P] [US2] Create tablet grid layouts for article listings (2-3 columns) in src/pug/articles.pug
- [ ] T044 [P] [US2] Create tablet grid layouts for project pages (2-3 columns) in src/pug/projects.pug
- [ ] T045 [US2] Adjust homepage content blocks for tablet layout (identity full-width, featured 60%, CTA 40%) in src/pug/index.pug
- [ ] T046 [US2] Verify touch targets remain 44x44px on tablet (768px-991px) in src/scss/_responsive.scss
- [ ] T047 [US2] Test tablet breakpoint (768px-1024px) for proper column layouts and spacing
- [ ] T048 [US2] Verify static navigation appears correctly on tablet (no fixed bottom bar)
- [ ] T049 [US2] Test content density: verify effective use of screen space without excessive whitespace
- [ ] T050 [US2] Validate line length at tablet breakpoint: 45-75 characters per line

**Checkpoint**: Tablet experience optimized with appropriate layouts and maintained touch-friendliness

---

## Phase 6: User Story 3 - Desktop Progressive Enhancement (Priority: P3)

**Goal**: Desktop visitors experience enhanced layouts with optimal content presentation and refined typography taking advantage of larger screens

**Independent Test**: Load site on desktop at 1200px+ → verify 3-4 column layouts, hover states work, line length constrained optimally, keyboard navigation supported

### Implementation for User Story 3

- [ ] T051 [P] [US3] Implement desktop grid layouts for article listings (3-4 columns) in src/pug/articles.pug
- [ ] T052 [P] [US3] Implement desktop grid layouts for project pages (3-4 columns) in src/pug/projects.pug
- [ ] T053 [P] [US3] Add hover states to desktop navigation links in src/scss/_navigation.scss
- [ ] T054 [P] [US3] Implement desktop button hover states and transitions in src/scss/_responsive.scss
- [ ] T055 [US3] Adjust desktop content containers to prevent excessive line length (max 75ch) in src/scss/_typography.scss
- [ ] T056 [US3] Test desktop layouts at 1200px, 1440px, 1920px widths for appropriate column counts
- [ ] T057 [US3] Verify hover states provide visual feedback on navigation and interactive elements
- [ ] T058 [US3] Test keyboard navigation: Tab, Enter, Space keys work properly
- [ ] T059 [US3] Verify line length remains optimal on ultra-wide displays (2560px+)
- [ ] T060 [US3] Test desktop content doesn't stretch excessively - appropriate margins and max-widths

**Checkpoint**: Desktop experience enhanced with multi-column layouts and refined interactions

---

## Phase 7: Content Hierarchy & Performance Optimization

**Purpose**: Implement above-the-fold content optimization and image lazy loading for mobile performance

- [ ] T061 [P] Implement Flexbox-based homepage hero layout with order properties in src/pug/index.pug
- [ ] T062 [P] Create .homepage-hero component styles in src/scss/_responsive.scss
- [ ] T063 Create homepage identity block (profile, name, title, description) in src/pug/index.pug
- [ ] T064 [P] Create homepage featured article block with card component in src/pug/index.pug
- [ ] T065 [P] Create homepage CTA block with primary action button in src/pug/index.pug
- [ ] T066 Set homepage hero/featured images to loading="eager" in src/pug/index.pug
- [ ] T067 Audit all article pages and set below-fold images to loading="lazy" in src/pug/articles/*.pug
- [ ] T068 Add width/height attributes to all images to prevent layout shift in src/pug/articles/*.pug
- [ ] T069 Test homepage above-the-fold content on mobile (600px viewport height): verify identity, featured, CTA all visible
- [ ] T070 Validate image lazy loading with browser Network panel: verify eager vs lazy behavior
- [ ] T071 Run Lighthouse performance audit: verify FCP < 1.5s, LCP < 2.5s, CLS < 0.1

**Checkpoint**: Homepage optimized for mobile, lazy loading implemented, performance targets achieved

---

## Phase 8: Cross-Browser & Device Testing

**Purpose**: Comprehensive validation across browsers, devices, and orientations

- [ ] T072 [P] Test on iPhone SE (375px width) Safari: verify layout, navigation, readability
- [ ] T073 [P] Test on iPhone 12/13 (390px width) Safari: verify layout, navigation, readability
- [ ] T074 [P] Test on Android device (various widths) Chrome: verify layout, navigation, readability
- [ ] T075 [P] Test on iPad (768px-1024px) Safari: verify tablet layouts and touch targets
- [ ] T076 Test portrait and landscape orientations on mobile and tablet devices
- [ ] T077 [P] Test on Chrome desktop (Windows): verify hover states, keyboard navigation
- [ ] T078 [P] Test on Firefox desktop: verify compatibility and responsive behavior
- [ ] T079 [P] Test on Edge desktop: verify compatibility and responsive behavior
- [ ] T080 [P] Test on Safari desktop (macOS): verify compatibility and responsive behavior
- [ ] T081 Test Samsung Internet browser on Android device
- [ ] T082 Verify no horizontal scrolling at any tested width (320px-2560px)

**Checkpoint**: Cross-browser and cross-device compatibility validated

---

## Phase 9: Accessibility & Standards Compliance

**Purpose**: Validate WCAG 2.1 AA compliance and responsive design standards

- [ ] T083 Run pa11y-ci accessibility audit with npm run audit:a11y (target: zero failures)
- [ ] T084 Verify all touch targets are minimum 44x44px with 8px spacing using pa11y output
- [ ] T085 Verify text contrast ratios meet 4.5:1 standard for body text, 3:1 for large text
- [ ] T086 Test text resize to 200% browser zoom: verify no horizontal scroll, maintained functionality
- [ ] T087 Test with screen reader (NVDA/JAWS on Windows, VoiceOver on Mac/iOS): verify navigation announcements
- [ ] T088 Validate keyboard navigation: all interactive elements reachable via Tab, activated via Enter/Space
- [ ] T089 Test with high contrast mode: verify content remains visible and usable
- [ ] T090 Run Google Mobile-Friendly Test: verify 100% passing score
- [ ] T091 Validate heading hierarchy: proper h1-h6 structure without skipping levels
- [ ] T092 Verify all images have meaningful alt text attributes

**Checkpoint**: WCAG 2.1 AA compliance validated, accessibility standards met

---

## Phase 10: Performance Validation & Optimization

**Purpose**: Final performance testing and optimization to meet success criteria

- [ ] T093 Run Lighthouse CI performance audit: verify Mobile Performance score 90+
- [ ] T094 Verify First Contentful Paint < 1.5s on 4G connection simulation
- [ ] T095 Verify Largest Contentful Paint < 2.5s on mobile
- [ ] T096 Verify Cumulative Layout Shift < 0.1 (images have width/height attributes)
- [ ] T097 Test page load on throttled 3G connection: verify critical content renders within 3 seconds
- [ ] T098 Verify font loading performance: fonts load within 2 seconds on 3G
- [ ] T099 Audit CSS bundle size: identify and remove unused styles if needed
- [ ] T100 Test lazy loading effectiveness: verify below-fold images don't load until scrolled into view
- [ ] T101 Run full audit suite with npm run audit:all and document baseline metrics
- [ ] T102 Compare performance metrics with pre-optimization baseline (if available)

**Checkpoint**: Performance targets achieved, optimization validated

---

## Phase 11: Documentation & Polish

**Purpose**: Final documentation, code cleanup, and validation

- [ ] T103 [P] Update quickstart.md with any implementation changes or lessons learned
- [ ] T104 [P] Document any deviations from original plan in research.md
- [ ] T105 [P] Create before/after screenshot comparison for documentation
- [ ] T106 Review all SCSS files for code quality and consistency
- [ ] T107 Review all PUG template files for proper indentation and structure
- [ ] T108 Remove any debug code, console.logs, or temporary comments
- [ ] T109 Validate all tasks in this tasks.md are complete and checked off
- [ ] T110 Run final build with npm run build and verify no errors or warnings
- [ ] T111 Test deployed site on staging environment (if available)
- [ ] T112 Create pull request with comprehensive description of changes

**Checkpoint**: Implementation complete, documented, and ready for review

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) - MVP critical path
- **User Story 4 (Phase 4)**: Depends on Foundational (Phase 2) - Can run parallel with US1 if staffed
- **User Story 2 (Phase 5)**: Depends on Foundational + US1 + US4 completion
- **User Story 3 (Phase 6)**: Depends on Foundational + US1 + US4 completion
- **Content Hierarchy (Phase 7)**: Depends on US1 completion
- **Testing Phases (8-10)**: Depend on all user story implementations
- **Documentation (Phase 11)**: Final phase after all testing

### User Story Dependencies

- **User Story 1 (P1)**: Mobile experience - Independent after Foundational
- **User Story 4 (P1)**: Typography - Independent after Foundational
- **User Story 2 (P2)**: Tablet - Builds on US1 and US4 foundations
- **User Story 3 (P3)**: Desktop - Builds on US1, US4, and US2 foundations

### Critical Path (MVP)

For minimum viable product, complete in this order:
1. Phase 1: Setup (T001-T010)
2. Phase 2: Foundational (T011-T018)
3. Phase 3: User Story 1 (T019-T031)
4. Phase 4: User Story 4 (T032-T041)
5. Phase 7: Performance (T061-T071)
6. Phase 9: Accessibility validation (T083-T092)
7. Phase 10: Performance validation (T093-T102)

**MVP Deliverable**: Mobile-first responsive site with readable typography, touch-friendly navigation, and WCAG compliance.

### Parallel Opportunities

**Within Setup Phase**:
- T003, T004, T005 (different SCSS files)
- T007, T010 (different file updates)

**Within Foundational Phase**:
- All tasks are sequential (each builds on previous typography/responsive setup)

**Within User Story 1**:
- T019, T020, T021 (same file, but independent style rules)
- T025, T026 (different template files)

**Within User Story 4**:
- T032, T033, T034 (same file, different typography rules)

**Cross-Story Parallelization**:
- US1 (Phase 3) and US4 (Phase 4) can be worked in parallel after Foundational complete
- Testing phases (8, 9, 10) can overlap if multiple testers available

---

## Parallel Execution Examples

### Scenario 1: Single Developer (Sequential)

```
Week 1:
- Day 1-2: Phase 1 Setup + Phase 2 Foundational
- Day 3-4: Phase 3 User Story 1 (Mobile)
- Day 5: Phase 4 User Story 4 (Typography)

Week 2:
- Day 1: Phase 7 Content Hierarchy & Performance
- Day 2-3: Phase 5 User Story 2 (Tablet) + Phase 6 User Story 3 (Desktop)
- Day 4-5: Phase 8-10 Testing & Validation
- Day 5: Phase 11 Documentation
```

### Scenario 2: Two Developers (Parallel)

```
Developer A:
- Day 1: Phase 1 Setup + Phase 2 Foundational
- Day 2-3: Phase 3 User Story 1 (Mobile navigation + touch targets)
- Day 4: Phase 7 Content Hierarchy & Performance

Developer B:
- Day 1: Wait for Foundational completion
- Day 2-3: Phase 4 User Story 4 (Typography system)
- Day 4: Phase 5 User Story 2 (Tablet layouts)
- Day 5: Phase 6 User Story 3 (Desktop enhancements)

Both:
- Day 5-6: Phase 8-10 Testing & Validation
- Day 7: Phase 11 Documentation
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 4 Only)

1. **Setup + Foundation** (T001-T018) → 4-6 hours
2. **User Story 1: Mobile** (T019-T031) → 6-8 hours
3. **User Story 4: Typography** (T032-T041) → 4-6 hours
4. **Performance** (T061-T071) → 3-4 hours
5. **Validation** (T083-T102) → 4-6 hours
6. **STOP and VALIDATE**: Full mobile experience functional
7. Deploy/demo if ready

**Total MVP Estimate**: 21-30 hours (3-4 work days)

### Incremental Delivery

1. Complete Setup + Foundational → **Foundation ready**
2. Add User Story 1 + 4 → Test independently → **MVP deployed** ✅
3. Add User Story 2 → Test independently → **Tablet optimization deployed**
4. Add User Story 3 → Test independently → **Full responsive deployed**
5. Each phase adds value without breaking previous work

---

## Success Validation Checklist

After completing all tasks, verify these success criteria from spec.md:

- [ ] SC-001: Google Mobile-Friendly Test passing score 100%
- [ ] SC-002: Mobile Lighthouse Performance score 90+
- [ ] SC-003: Mobile Lighthouse Accessibility score 95+
- [ ] SC-004: Content readable without pinch-zoom on mobile devices
- [ ] SC-005: 100% of interactive elements meet 44x44px touch target requirement
- [ ] SC-006: 100% text contrast compliance with WCAG 2.1 AA (4.5:1)
- [ ] SC-007: First Contentful Paint < 1.5s on 4G
- [ ] SC-011: Text remains readable at 200% browser zoom
- [ ] SC-012: Passes responsive design validation across all breakpoints

---

## Notes

- **[P] tasks**: Different files or independent style rules with no dependencies
- **[US#] labels**: Map tasks to specific user stories for traceability
- **MVP Path**: Focus on US1 + US4 first for fastest value delivery
- **Testing**: Real device testing critical - emulators don't catch all issues
- **Commit strategy**: Commit after each phase completion for rollback safety
- **Build frequently**: Run `npm run build:scss` and `npm run build:pug` after each major change
- **Validation**: Run audits frequently, not just at end

**Total Task Count**: 112 tasks  
**Task Count by Phase**:
- Setup: 10 tasks
- Foundational: 8 tasks (BLOCKING)
- User Story 1 (P1 - Mobile): 13 tasks
- User Story 4 (P1 - Typography): 10 tasks
- User Story 2 (P2 - Tablet): 9 tasks
- User Story 3 (P3 - Desktop): 10 tasks
- Content Hierarchy: 11 tasks
- Cross-Browser Testing: 11 tasks
- Accessibility: 10 tasks
- Performance: 10 tasks
- Documentation: 10 tasks

**Estimated Implementation Time**: 40-60 hours (5-7.5 work days for single developer)