# Solution Patterns

## Pattern
- Name: NtSuspendProcess P/Invoke (Pause/Resume)
- Use when: Pausing a running robocopy.exe process without killing it
- Inputs: Process ID (int)
- Steps:
  1. `[DllImport("ntdll.dll")] static extern int NtSuspendProcess(IntPtr processHandle);`
  2. `[DllImport("ntdll.dll")] static extern int NtResumeProcess(IntPtr processHandle);`
  3. Pass `process.Handle` directly — do NOT use OpenProcess (handle already open)
  4. Button text toggles: "⏸ Pause" ↔ "▶ Resume"
  5. On Cancel: call NtResumeProcess BEFORE Kill() to avoid zombie process
- Verification: Process.Responding == false after suspend; stdout queue drains normally after resume
- Related lessons: NtSuspendProcess returns NTSTATUS — 0 = success; non-zero = log warning

## Pattern
- Name: ConcurrentQueue stdout pump
- Use when: Reading robocopy stdout without blocking the WPF UI thread
- Inputs: Process object, ConcurrentQueue<string>
- Steps:
  1. Redirect StandardOutput and StandardError
  2. Subscribe OutputDataReceived / ErrorDataReceived → enqueue with `"O:"` / `"E:"` prefix
  3. DispatcherTimer at 500ms interval → dequeue all items → fire LineReceived event on UI thread
  4. Stop timer when process exits AND queue is empty
- Verification: UI never freezes; all lines processed in order
- Related lessons: Do not call `process.WaitForExit()` on UI thread

## Pattern
- Name: Graceful corrupt-JSON settings fallback
- Use when: Loading settings.json that may be malformed
- Inputs: settings.json file path
- Steps:
  1. Try `File.ReadAllText` → `JsonSerializer.Deserialize<AppSettings>`
  2. Catch `JsonException` and `IOException` → return `new AppSettings()` (defaults)
  3. Log warning to auto-log: `"[WARN] settings.json corrupt — using defaults"`
- Verification: App starts normally with missing or empty settings.json
- Related lessons: Always save with `new JsonSerializerOptions { WriteIndented = true }`

## Pattern
- Name: robocopy exit code interpretation
- Use when: Determining success/failure after robocopy exits
- Inputs: Process.ExitCode (int)
- Steps:
  - 0 = no files copied (already in sync) → treat as success
  - 1 = files copied successfully → success  
  - 2 = extra files found → success
  - 4 = mismatched files → success
  - 7 = combination of 1+2+4 → success
  - 8+ = at least one failure → error
  - 16 = fatal error → error
- Verification: Summary header color: green for 0-7, red for 8+
- Related lessons: Exit code is a bitmask — use `(exitCode & 8) != 0` to detect errors
