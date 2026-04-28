# Windows Dev Engine — Agent Entry Point

## READ THIS FIRST

You are an AI agent tasked with building a Windows desktop application using this engine.
This folder is your complete operating manual. Follow it exactly. Do not improvise structure.

---

## Who This Is For

**Commander Maverick** — a Windows power user who builds tools to solve real problems.
He values:
- Applications that look incredible and feel fast
- Debugging capability over raw feature count
- Animations and visual polish that make apps feel alive
- Templates that let him spin up a new app in under an hour
- Code that he (and future AI agents) can read and extend without confusion

---

## Your Reading Order (Do This Before Writing Any Code)

| Step | File | Why |
|------|------|-----|
| 1 | `agent/00_MANIFESTO.md` | Understand what you're building and why |
| 2 | `agent/03_DEBUG_FIRST.md` | Internalize the most important principle |
| 3 | `agent/04_ARCHITECTURE.md` | Know the project structure before you create any file |
| 4 | `agent/02_DESIGN_STANDARDS.md` | Know the visual rules before you write any XAML |
| 5 | `agent/01_BUILD_RECIPE.md` | Follow the step-by-step build process |
| 6 | `agent/06_CHECKLIST.md` | Check off gates before calling the build done |

---

## Quick Reference

- **Language:** C# 12 / .NET 8
- **UI Framework:** WPF (Windows Presentation Foundation)
- **Pattern:** MVVM (Model-View-ViewModel)
- **Debug:** Serilog + DiagnosticsOverlay (always wired before any feature)
- **Templates:** Copy from `templates/` — never start from scratch
- **Samples:** Reference `samples/` for any animation, control, or UI effect

---

## The One Rule

**If it can't be debugged, it doesn't ship.**

Every feature must be observable. Every state change must be logged.
Every error must be caught, logged with context, and surfaced — never swallowed.

---

## Starting a New App

```
1. Copy templates/BaseProject/ → rename it to your app name
2. Read agent/01_BUILD_RECIPE.md step by step
3. Wire debug/DiagnosticsOverlay/ BEFORE writing any feature
4. Build features one at a time, checking off agent/06_CHECKLIST.md as you go
5. Reference samples/ for any UI component or animation you need
```

---

*Last updated: 2026-04-27 | Engine version: 1.0.0*
