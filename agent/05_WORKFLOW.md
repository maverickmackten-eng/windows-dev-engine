# 05 — Development Workflow

## The Daily Loop

Every development session follows this cycle:

```
1. ORIENT    — Read the last git commit, check open issues
2. PLAN      — Identify the one thing that moves the app forward most
3. BUILD     — Follow the Build Recipe for that one thing
4. VERIFY    — Run app, test golden path, test failure paths
5. COMMIT    — Commit with proper message format
6. DOCUMENT  — Update any agent docs if you learned something new
```

Never work on more than one feature at a time.
Never commit broken code. A commit says "this works."

---

## Starting A New Session (Agent Instructions)

When picking up an existing project, do this before writing any code:

```bash
# 1. Check what was done last
git log --oneline -10

# 2. Check current state
git status

# 3. Build and run to verify baseline
dotnet build
dotnet run
```

Then check:
- [ ] Does the app start without errors?
- [ ] Is the DiagnosticsOverlay accessible (Ctrl+Shift+D)?
- [ ] What was the last completed feature?
- [ ] What is the next feature in the backlog?

---

## Git Workflow

### Branch Strategy (For Larger Apps)

```
main          ← Always working, always clean
├── feat/dashboard-view
├── feat/settings-persistence
├── fix/crash-on-empty-list
└── polish/animation-refinements
```

For solo/small projects: commit directly to main is acceptable.

### Commit Before Switching Context

If you stop mid-feature, commit what you have as a WIP commit:
```
git commit -m "wip: dashboard layout (incomplete)"
```

When you return, squash the WIP commits into one clean commit before merging.

---

## Visual Studio Setup Recommendations

### Must-Have Extensions

```
- ReSharper (or Rider) — code analysis and refactoring
- XAML Styler — automatic XAML formatting
- CodeMaid — code cleanup
- GitLens (if using VS Code for editing)
```

### Solution Configuration

- Always build with `Debug` config during development
- `Release` config for final testing only
- Enable all warnings: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
  during development (loosen if third-party packages warn)

### Debug Output

Configure Debug output window to show:
- Module loads
- Thread exit messages
- Exception details

---

## Testing Strategy

### What To Test

| Layer | Test Type | Priority |
|-------|-----------|----------|
| ViewModels | Unit tests (xUnit) | HIGH |
| Models | Unit tests (xUnit) | HIGH |
| Services | Integration tests | MEDIUM |
| Views | Manual visual testing | MANUAL |
| Animations | Manual visual testing | MANUAL |

### ViewModel Test Template

```csharp
public class DashboardViewModelTests
{
    private readonly DashboardViewModel _sut;
    private readonly Mock<INavigationService> _navMock;

    public DashboardViewModelTests()
    {
        _navMock = new Mock<INavigationService>();
        _sut = new DashboardViewModel(_navMock.Object);
    }

    [Fact]
    public void StatusMessage_DefaultsToReady()
    {
        Assert.Equal("Ready", _sut.StatusMessage);
    }

    [Fact]
    public void NavigateCommand_CallsNavigationService()
    {
        _sut.NavigateCommand.Execute("Settings");
        _navMock.Verify(n => n.NavigateTo("Settings"), Times.Once);
    }
}
```

---

## Performance Budget

These limits apply to every application:

| Metric | Target | Maximum |
|--------|--------|---------|
| App startup to interactive | < 2s | 4s |
| Page navigation | < 100ms | 300ms |
| UI frame rate (idle) | 60 FPS | 30 FPS |
| UI frame rate (animated) | 60 FPS | 45 FPS |
| Memory at startup | < 100 MB | 200 MB |
| Log file per day | < 10 MB | 50 MB |

If you exceed a budget, investigate before shipping.
The DiagnosticsOverlay shows FPS and memory in real time.

---

## Debugging A Problem — The Process

1. **Reproduce** — Get the exact steps that cause the problem
2. **Check logs** — Open log file (Ctrl+Shift+L), find the relevant time window
3. **Read the exception** — The full stack trace is always in the log
4. **Isolate** — Comment out surrounding code to narrow the cause
5. **Fix** — One change at a time
6. **Verify** — Reproduce the original steps — problem gone?
7. **Check for regression** — Navigate through all other features
8. **Commit** — `fix: [description of what broke and what you changed]`

---

## Release Checklist

Before calling a version done:

```
[ ] All features work on fresh Windows 11 install (no dev tools)
[ ] All features work at 100%, 125%, and 150% DPI scaling
[ ] App handles no-internet scenario gracefully (if it uses network)
[ ] Log folder created automatically on first run
[ ] No debug/test artifacts in release build (test buttons, dummy data)
[ ] App icon set (not default WPF icon)
[ ] Version number updated in .csproj
[ ] README updated with how to run/install
```
