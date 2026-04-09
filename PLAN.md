# TextEditor — Development Plan

> **Copilot**: identify the first unchecked step below, state it, and wait for confirmation before starting.

---

## Current Goal

Build a fully-featured, cross-platform (Windows + macOS) desktop text editor in C# using Clean Architecture, with Avalonia UI as the presentation layer and a Piece Table as the core text storage structure.

---
 
## Steps

### Phase 0 — Solution & Project Scaffold

- [x] **Step 1**: Create the solution folder structure (`src/` and `tests/`). Scaffold all 8 projects using `dotnet new` — `classlib` for Domain, Application, Infrastructure; `avalonia.app` (or `classlib` initially) for Presentation; `xunit` for each of the 4 test projects. Register all 8 projects in `TextEditor.slnx`.
- [x] **Step 2**: Configure every `.csproj` with `<TargetFramework>net10.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, and `<LangVersion>latest</LangVersion>`.
- [x] **Step 3**: Add Clean Architecture project references — Application → Domain; Infrastructure → Application + Domain; Presentation → Application. Add test project references to their corresponding source projects.
- [x] **Step 4**: Add NuGet packages — xUnit + Shouldly + Moq to all test projects; `Microsoft.Extensions.DependencyInjection` to Infrastructure and Presentation; `Serilog` (or `Microsoft.Extensions.Logging`) to Infrastructure.

### Phase 1 — Piece Table Core (Domain)

- [x] **Step 5**: Create `BufferType` enum (`Original`, `Add`) and `Piece` value record (`BufferType`, `Start`, `Length`) in `TextEditor.Domain`.
- [x] **Step 6**: Create `PieceTable` class with an immutable original buffer string and a mutable add-buffer `StringBuilder`. Initialise the piece list from the original string.
- [x] **Step 7**: Implement `PieceTable.Insert(int offset, string text)` — appends text to the add-buffer, splits or extends pieces to reflect the insertion.
- [x] **Step 8**: Implement `PieceTable.Delete(int offset, int length)` — removes the character range by splitting and removing pieces.
- [ ] **Step 9**: Implement `PieceTable.GetText()` and `PieceTable.GetRange(int offset, int length)` — materialise content by walking the piece list.
- [ ] **Step 10**: Write unit tests for `PieceTable` (insert at start/middle/end, delete, get full text, get range, multi-operation sequences).

### Phase 2 — Line Model (Domain)

- [ ] **Step 11**: Implement `PieceTable.LineCount` — counts newline characters in the logical text.
- [ ] **Step 12**: Implement `PieceTable.GetLineText(int lineIndex)` — returns the text of a single logical line (excluding the newline character).
- [ ] **Step 13**: Implement `PieceTable.GetLineStartOffset(int lineIndex)` and `PieceTable.GetLineEndOffset(int lineIndex)` — character offsets for line boundaries.
- [ ] **Step 14**: Implement `PieceTable.OffsetToLineColumn(int offset)` → `(line, column)` and `PieceTable.LineColumnToOffset(int line, int column)` → `int offset` — bidirectional coordinate mapping.
- [ ] **Step 15**: Write unit tests for all line model operations (single line, multiple lines, empty lines, last line without trailing newline, boundary offsets).

### Phase 3 — Document Entity (Domain)

- [ ] **Step 16**: Create `DocumentId` value record (wraps a `Guid`).
- [ ] **Step 17**: Create `TextRange` value record (`int Offset`, `int Length`) with a `bool IsEmpty` property.
- [ ] **Step 18**: Create `Document` entity class — wraps `PieceTable`, holds `DocumentId`, `FilePath?`, and an `IsDirty` flag. Exposes `InsertText`, `DeleteText`, `GetText`, `LineCount`, `GetLineText`, `OffsetToLineColumn`, `LineColumnToOffset`.
- [ ] **Step 19**: Write unit tests for `Document` — verify dirty flag is set on mutation, cleared on construction, and that it delegates correctly to `PieceTable`.

### Phase 4 — Application Layer: Interfaces & Shared Types

- [ ] **Step 20**: Create `Result` and `Result<T>` types in `TextEditor.Application` — represent success/failure without exceptions; include `IsSuccess`, `Error` string, and factory methods `Ok()`, `Ok(value)`, `Fail(error)`.
- [ ] **Step 21**: Write unit tests for `Result<T>` — success carries value, failure carries error message, accessing `.Value` on failure throws.
- [ ] **Step 22**: Define `IDocumentRepository` interface in Application — `Task<Result<Document>> LoadAsync(string filePath, CancellationToken ct)` and `Task<Result> SaveAsync(Document document, string filePath, CancellationToken ct)`.
- [ ] **Step 23**: Define `IClipboardService` interface in Application — `Task<string?> GetTextAsync(CancellationToken ct)` and `Task SetTextAsync(string text, CancellationToken ct)`.

### Phase 5 — Application Use Cases: Document Lifecycle

- [ ] **Step 24**: Implement `CreateNewDocumentUseCase` — returns a new `Document` with an empty `PieceTable` and a generated `DocumentId`.
- [ ] **Step 25**: Implement `OpenDocumentUseCase` — calls `IDocumentRepository.LoadAsync`, returns `Result<Document>`.
- [ ] **Step 26**: Implement `SaveDocumentUseCase` — calls `IDocumentRepository.SaveAsync` with the document's current path; returns `Result`.
- [ ] **Step 27**: Implement `SaveDocumentAsUseCase` — same as save but accepts a new `filePath` parameter, updates `Document.FilePath`.
- [ ] **Step 28**: Implement `CheckUnsavedChangesUseCase` — returns whether `Document.IsDirty` is true; used before close/open to decide whether to show a warning.
- [ ] **Step 29**: Write unit tests for all document lifecycle use cases (mock `IDocumentRepository`; verify correct delegation, dirty-flag transitions, and failure propagation).

### Phase 6 — Application Use Cases: Text Editing

- [ ] **Step 30**: Implement `InsertTextUseCase` — validates offset is in range, calls `Document.InsertText`, returns `Result`.
- [ ] **Step 31**: Implement `DeleteTextUseCase` — validates range is valid, calls `Document.DeleteText`, returns `Result`.
- [ ] **Step 32**: Write unit tests for `InsertTextUseCase` and `DeleteTextUseCase` (valid input, offset out of range, zero-length delete).

### Phase 7 — Infrastructure: File I/O

- [ ] **Step 33**: Implement `FileDocumentRepository.LoadAsync` — reads file bytes, detects encoding via BOM sniffing (UTF-8 BOM, UTF-16 LE/BE BOM; default UTF-8 without BOM), decodes to string, constructs `Document`.
- [ ] **Step 34**: Implement `FileDocumentRepository.SaveAsync` — writes document text to disk using the detected/original encoding; creates parent directories if absent.
- [ ] **Step 35**: Create `InfrastructureServiceExtensions` — `IServiceCollection` extension method that registers `FileDocumentRepository` as `IDocumentRepository`.
- [ ] **Step 36**: Write Infrastructure unit tests using an `InMemoryDocumentRepository` — no real file I/O; verify load returns correct `Document`, save stores content correctly.

### Phase 8 — Avalonia UI Scaffold

- [ ] **Step 37**: Add Avalonia NuGet packages (`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`) to `TextEditor.Presentation`.
- [ ] **Step 38**: Create `App.axaml` / `App.axaml.cs` — minimal Avalonia `Application` subclass with `FluentTheme`.
- [ ] **Step 39**: Create `MainWindow.axaml` / `MainWindow.axaml.cs` — empty shell window with a `DockPanel` placeholder.
- [ ] **Step 40**: Create `Program.cs` as the composition root — build `IServiceProvider`, register all services (Infrastructure + Application use cases), launch Avalonia app.
- [ ] **Step 41**: Verify the app builds and opens a blank window on both target platforms (manual smoke test step).

### Phase 9 — Word Wrap Engine (Application)

- [ ] **Step 42**: Create `VisualLine` record in Application — `int LogicalLineIndex`, `int WrapOffsetInLine`, `string Text` (the visual segment).
- [ ] **Step 43**: Create `WordWrapEngine` in Application — given a logical line's text and a `viewportCharacterWidth`, returns `IReadOnlyList<VisualLine>`. Implements VS Code–style soft wrap: break at the last word boundary (space) within the viewport width; if no boundary exists, break mid-word.
- [ ] **Step 44**: Write unit tests for `WordWrapEngine` — line shorter than viewport (no wrap), line wraps at space, line wraps mid-word, empty line, line of exactly viewport width.

### Phase 10 — Custom Text Rendering Control (Presentation)

- [ ] **Step 45**: Create `TextEditorControl` — a custom Avalonia `Control` subclass. Override `OnRender(DrawingContext)` to iterate visual lines and draw each using `FormattedText`. Use a fixed monospace font initially.
- [ ] **Step 46**: Expose `Document` as a bindable property on `TextEditorControl`; invalidate the visual on change.
- [ ] **Step 47**: Wire `TextEditorControl` into `MainWindow` and display a hardcoded document to verify text renders correctly.

### Phase 11 — Cursor: Data Model (Domain)

- [ ] **Step 48**: Create `CursorPosition` value record (`int Line`, `int Column`, `int PreferredColumn`) — `PreferredColumn` stores the intended column for VS Code–style column memory.
- [ ] **Step 49**: Create `TextCursor` class — holds `CursorPosition`, references `Document`. Implement `MoveLeft()` and `MoveRight()` with line-boundary wrapping (moving left at column 0 goes to end of previous line; moving right at line end goes to start of next line).
- [ ] **Step 50**: Implement `TextCursor.MoveUp()` and `MoveDown()` — navigates to the adjacent logical line, restoring `PreferredColumn` if the target line is long enough (VS Code column memory behaviour).
- [ ] **Step 51**: Write unit tests for `TextCursor` — left/right wrap at line boundaries, up/down column memory (preferred column preserved across short line, restored on long line), movement clamped at document start/end.

### Phase 12 — Cursor: Rendering & Keyboard Input (Presentation)

- [ ] **Step 52**: Render a blinking caret (1 px wide, line-height tall) at the `TextCursor` position inside `TextEditorControl.OnRender`.
- [ ] **Step 53**: Implement a blink timer in `TextEditorControl` — toggles caret visibility every 500 ms using `DispatcherTimer`; resets on any keystroke.
- [ ] **Step 54**: Handle `KeyDown` in `TextEditorControl` — route `Left`, `Right`, `Up`, `Down` arrows to `TextCursor`; call `InvalidateVisual()` after each move.

### Phase 13 — Keyboard Text Input (Presentation)

- [ ] **Step 55**: Handle printable character `TextInput` event → call `InsertTextUseCase` at cursor offset; advance cursor by the inserted text length.
- [ ] **Step 56**: Handle `Backspace` → delete the character before the cursor (if cursor is not at document start); move cursor left.
- [ ] **Step 57**: Handle `Delete` → delete the character after the cursor (if cursor is not at document end); cursor position unchanged.
- [ ] **Step 58**: Handle `Enter` → insert `\n` at the cursor offset; move cursor to the start of the new line.

### Phase 14 — Scrolling (Presentation)

- [ ] **Step 59**: Implement `TextEditorControl.MeasureOverride` — calculate and return total content size (height = total visual line count × line height; width = longest visual line in pixels) so the parent scroll container knows the scroll extent.
- [ ] **Step 60**: Wrap `TextEditorControl` in an Avalonia `ScrollViewer` in `MainWindow` — enable vertical and horizontal scrolling.
- [ ] **Step 61**: Implement auto-scroll in `TextEditorControl` — after every cursor move, if the caret falls outside the current viewport bounds, scroll the minimum amount to bring it into view.

### Phase 15 — File Operations UI (Presentation)

- [ ] **Step 62**: Add a `MenuBar` to `MainWindow` with `File` menu items: `New` (Ctrl+N), `Open…` (Ctrl+O), `Save` (Ctrl+S), `Save As…` (Ctrl+Shift+S), `Exit`.
- [ ] **Step 63**: Wire `Open…` — show Avalonia `OpenFileDialog`, call `OpenDocumentUseCase`, load the result into `TextEditorControl`.
- [ ] **Step 64**: Wire `Save` (Ctrl+S) — call `SaveDocumentUseCase`; if the document has no file path yet, fall through to `SaveDocumentAsUseCase`.
- [ ] **Step 65**: Wire `Save As…` — show Avalonia `SaveFileDialog`, call `SaveDocumentAsUseCase`.
- [ ] **Step 66**: Implement unsaved-changes warning dialog — a modal with `Save`, `Don't Save`, and `Cancel` buttons. Show it before `New`, `Open`, and `Exit` when `CheckUnsavedChangesUseCase` returns true.

### Phase 16 — Status Bar (Presentation)

- [ ] **Step 67**: Create `StatusBarControl` — a thin `DockPanel` at the bottom of `MainWindow` showing: document filename (or `Untitled`), dirty indicator (`●`), cursor line + column, and file encoding.
- [ ] **Step 68**: Bind `StatusBarControl` to a `StatusBarViewModel` that updates reactively when the document or cursor changes.

### Phase 17 — Undo / Redo: Command Pattern (Application)

- [ ] **Step 69**: Define `IEditorCommand` interface in Application — `void Execute()` and `void Undo()`.
- [ ] **Step 70**: Implement `InsertTextCommand : IEditorCommand` — stores document, offset, and text; `Execute` calls `Document.InsertText`; `Undo` calls `Document.DeleteText`.
- [ ] **Step 71**: Implement `DeleteTextCommand : IEditorCommand` — stores document, offset, length, and the deleted text (captured at execute time for undo); `Execute` calls `Document.DeleteText`; `Undo` calls `Document.InsertText`.
- [ ] **Step 72**: Implement `CommandHistory` — maintains an undo stack and a redo stack. `Execute(IEditorCommand)` runs the command, pushes to undo, clears redo. `Undo()` pops from undo, calls `command.Undo()`, pushes to redo. `Redo()` pops from redo, calls `command.Execute()`, pushes to undo.
- [ ] **Step 73**: Refactor `InsertTextUseCase` and `DeleteTextUseCase` to create the corresponding `IEditorCommand` and route through `CommandHistory` instead of calling `Document` directly.
- [ ] **Step 74**: Wire Ctrl+Z → `CommandHistory.Undo()` and Ctrl+Y / Ctrl+Shift+Z → `CommandHistory.Redo()` in `MainWindow`.
- [ ] **Step 75**: Write unit tests for `CommandHistory` — execute, undo, redo, undo after new command clears the redo stack, undo/redo with an empty stack are no-ops.

### Phase 18 — Text Selection (Domain + Application + Presentation)

- [ ] **Step 76**: Create `TextSelection` value record in Domain — `int AnchorOffset`, `int ActiveOffset`; computed property `NormalisedRange` returns a `TextRange` (low-to-high); `bool IsEmpty` when both offsets are equal.
- [ ] **Step 77**: Add selection state to `TextCursor`. On the first Shift+Arrow, set anchor at the current offset; extend active on subsequent Shift+Arrow presses. Any unshifted move collapses the selection.
- [ ] **Step 78**: Implement `TextCursor.SelectAll(Document)` — sets anchor to 0 and active to document length.
- [ ] **Step 79**: Render selection highlight in `TextEditorControl.OnRender` — draw a semi-transparent rectangle behind the selected characters on each visual line that intersects the selection range.
- [ ] **Step 80**: Update keyboard input handlers — if a selection is active when `Backspace`, `Delete`, or a printable key is pressed, delete the selected range first (typed character replaces the selection).
- [ ] **Step 81**: Wire Ctrl+A → `TextCursor.SelectAll`.
- [ ] **Step 82**: Write unit tests for `TextSelection` — empty selection, normalised range when anchor > active, `IsEmpty`, extending selection across line boundaries.

### Phase 19 — Copy / Paste (Application + Presentation)

- [ ] **Step 83**: Implement `CopySelectionUseCase` — reads `TextSelection.NormalisedRange` from the cursor, calls `Document.GetRange`, calls `IClipboardService.SetTextAsync`.
- [ ] **Step 84**: Implement `CutSelectionUseCase` — copies selection to clipboard, then deletes it via `DeleteTextCommand` through `CommandHistory`.
- [ ] **Step 85**: Implement `PasteTextUseCase` — calls `IClipboardService.GetTextAsync`; if text is non-empty, deletes any active selection, then inserts the text at the cursor offset via `InsertTextCommand`.
- [ ] **Step 86**: Implement `AvaloniaClipboardService : IClipboardService` in Presentation — delegates to `TopLevel.GetTopLevel(control)!.Clipboard`.
- [ ] **Step 87**: Register `AvaloniaClipboardService` in the composition root. Wire Ctrl+C, Ctrl+X, Ctrl+V in `MainWindow`.
- [ ] **Step 88**: Write unit tests for `CopySelectionUseCase`, `CutSelectionUseCase`, and `PasteTextUseCase` — mock `IClipboardService`; use `It.Is<T>` to verify the exact text written to and read from the clipboard.

### Phase 20 — Line Number Gutter (Presentation)

- [ ] **Step 89**: Create `LineNumberGutterControl` — a custom Avalonia `Control` that renders right-aligned line numbers in a fixed-width column using the same line height as `TextEditorControl`.
- [ ] **Step 90**: Lay out `LineNumberGutterControl` and `TextEditorControl` side-by-side inside a `Grid` within the `ScrollViewer`, ensuring the gutter scrolls vertically in sync with the editor but remains fixed horizontally.
- [ ] **Step 91**: Highlight the current line number (bold or accent colour) to match the cursor's current logical line.

### Phase 21 — Auto-Indent (Application + Presentation)

- [ ] **Step 92**: Define `IIndentationStrategy` interface in Application — `string GetIndentForNewLine(string currentLineText)`.
- [ ] **Step 93**: Implement `CopyPreviousLineIndentStrategy : IIndentationStrategy` — extracts the leading whitespace (spaces or tabs) from `currentLineText` and returns it as the indent prefix for the new line.
- [ ] **Step 94**: Update the `Enter` key handler in `TextEditorControl` — after inserting `\n`, call `IIndentationStrategy.GetIndentForNewLine` with the current line's text and insert the returned indent string; position the cursor after the indent.
- [ ] **Step 95**: Write unit tests for `CopyPreviousLineIndentStrategy` — no indent (plain text line), spaces only, tabs only, mixed leading whitespace, empty line.

---

## Notes / Decisions

- **UI Framework**: Avalonia UI v11+ (cross-platform Windows + macOS + Linux, Skia-based rendering, WPF-like XAML). Only `TextEditor.Presentation` may reference Avalonia packages. Chosen over WPF (Windows-only) and MAUI (mobile-first); presentation layer is isolated so the framework can be swapped without touching Application or Domain.
- **Text Storage**: Piece Table — efficient for insert/delete without copying the whole buffer; original buffer is immutable, add-buffer is append-only.
- **Word Wrap**: Soft wrap only (VS Code style) — no newline characters are inserted into the document; visual lines are computed by `WordWrapEngine`. Wrap prefers breaking at word (space) boundaries; falls back to mid-word when no boundary exists within the viewport width.
- **Cursor Column Memory**: VS Code style — `PreferredColumn` is set on horizontal moves and preserved through vertical moves. If the target line is shorter than `PreferredColumn`, the cursor lands at the line end; when a subsequent line is long enough, the cursor snaps back to `PreferredColumn`.
- **File Encoding**: Detected on load via BOM sniffing (UTF-8 BOM, UTF-16 LE/BE BOM); defaults to UTF-8 without BOM. The detected encoding is preserved on save.
- **Error Handling**: Expected failures (file not found, invalid offset) use `Result<T>` / `Result`. Exceptions are reserved for unexpected/unrecoverable situations and must always be logged with context.
- **Undo/Redo**: Command pattern — `IEditorCommand` with `Execute` / `Undo`. `CommandHistory` owns both stacks. All text mutations (insert/delete) are routed through `CommandHistory`.
- **Reference Editor**: VS Code — cursor behaviour, word wrap, column memory, selection, and keyboard shortcuts all follow VS Code conventions.
- **Target Framework**: `net10.0` (.NET 10) for all projects.
