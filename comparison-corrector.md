# Medium Comparison Article Corrector

Single-pass prompt for editing one Medium comparison article at a time. Replace `<competitor>` with the matching subdirectory name (e.g., `asposepdf`, `syncfusion-pdf-framework`, `itext-itextsharp`). The article's slug in the filename may differ from the subdirectory name — pick the subdirectory whose `migrate-from-*.md` describes the same product.

---

ROLE
Single-pass surgical edit on ONE Medium comparison article to make it
accurate, legally defensible, Medium-renderable, and publish-ready.
The article, the .cs samples, and the migrate-from md already exist —
your job is to align facts, hedge liability, restructure tables, and
polish. Not rewrite.

INPUTS
- Article: Medium/comparison/<competitor>-vs-ironpdf-comparison.html
- Source of truth (read FIRST): <competitor>/migrate-from-<competitor>.md
- Working code: <competitor>/*.cs

PRECEDENCE WHEN SOURCES CONFLICT
- The .cs files always win. They are working code that had to compile,
  and they define the actual scope of this repo's content. This applies
  to language, platform, version, and integration model as well as APIs.
  If the article shows the competitor in a different version, platform,
  or integration model than the .cs files (e.g., article is "iTextSharp
  5.x" but .cs uses iText 7; or article shows a fabricated .NET SDK but
  .cs uses HttpClient against a documented REST endpoint), either
  retarget the article's code blocks to match the .cs files OR scope
  the article's title and framing explicitly to the legacy version.
  Do not leave it ambiguous.
- The migrate-from md is the second source of truth for APIs, naming,
  use cases, and licensing.
- If md and .cs disagree on an API name, the .cs wins.

PROCESS — strict order, do not reorder

STEP 1 — FACT-CHECK & API ALIGN
- Apply the precedence rule above. Retarget code if needed.
- Fix IronPDF API hallucinations against md and .cs. Known offenders:
  pdf.Password = "..." → pdf.SecuritySettings.UserPassword = "..."
  pdf.OwnerPassword = "..." → pdf.SecuritySettings.OwnerPassword = "..."
  pdf.RemovePage(i) → pdf.RemovePages(i)
  Opacity = 0.15 (decimal) → integer 0–100
  IronPdf.Imaging.Color.* → IronSoftware.Drawing.Color.*
  PdfDocument.FromBytes → PdfDocument.FromBinaryData
- Verify version-specific factual claims (CVE numbers, "last release"
  dates, ".NET version support" ranges, vendor pricing). If you cannot
  confirm against the md or vendor docs, remove the specific number/
  date and use a general phrasing ("a recent security patch", "the
  deprecated 5.x line").
- Add the standard IronPDF license line to any IronPDF code sample
  that lacks it: IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
- Verify NuGet package names against the .cs files (typically in a
  "// NuGet: Install-Package <name>" comment at the top) and the
  migrate-from md. If the article references a stale or rebranded
  package name (e.g., PDFTron-era name when the current package is
  Apryse-branded), update to match the .cs/md source of truth.
- For every "verify..." hedge: RESOLVE from md / .cs, or KEEP only if
  the gap is genuinely unresolved (AcroForm depth, PAdES signatures,
  OCR, Office-doc conversion, version-specific COM property variance).
  Never delete a hedge without resolving or confirming the gap exists.

STEP 2 — LEGAL HEDGING (comparison-specific)
The article compares IronPDF to a named competitor product. Vendors
in this space (especially Aspose) have a track record of legal action
against unsourced negative comparison content. Goal: keep the
technical substance, remove the prosecutorial framing.

- Strip editorial comments inside competitor code blocks. Comments
  like "// CSS parsing failure (modern CSS3 features)",
  "// Synchronous blocking conversion - no async support",
  "// External CSS often fails to load silently" — replace with
  neutral comments describing what the code does, or delete. Code
  comments must describe behavior, not characterize the vendor.
- Delete "expected failure" prediction blocks. Patterns like
  Console.WriteLine("Expected issues: - Grid layout likely collapsed")
  must be removed entirely. Predicting competitor failure modes
  inside compiled code is the highest-risk pattern.
- Hedge unsourced negative bullet claims. Lines like "CSS Grid
  completely unsupported" become "CSS Grid support is limited in
  this engine — verify against your version" or carry a citation.
  Acceptable qualifiers: "typically", "in common configurations",
  "as of v[N.N]", "documented limitation per [link]", "may", "can".
  Avoid "in our testing" / "we have tested" unless first-party
  testing was actually performed — otherwise the hedge itself is
  an unsupported claim.
  If the migrate-from md now cites a specific vendor source for the
  claim, you may state it with normal confidence rather than hedging
  it — over-hedging a claim source-corrector already verified dilutes
  the article unnecessarily.
- Soften one-word negative ratings in feature tables. Cells like
  "Mixed quality", "Often cryptic", "Frequent failures",
  "Workarounds needed" become factual descriptors of architecture
  ("WebKit-based engine", "synchronous API", "custom HTML parser")
  or get dropped. Compare architectures, do not grade vendors.
- Reframe opening anecdotes as composite scenarios. Lines like
  "the application crashes on Linux servers" or "performance
  optimization turns into an architecture regression" become
  "teams I have worked with have hit this pattern". Composite, not
  specific incidents.
- Reframe characterizations of the vendor's company, support, or
  business decisions ("Acquisition uncertainty", "Mixed quality
  forum", "Response quality varies", "smaller community than...")
  as factual descriptors or remove. Stick to product-and-API
  characterization, not vendor characterization.

STEP 3 — TABLES & POLISH
For every <table>, in order:
  1. Identify the H2 or H3 heading immediately above the table. It
     STAYS EXACTLY AS IT WAS. Do not delete, merge, or skip it.
  2. Replace ONLY the <table>...</table> body with a Medium-friendly
     format. Choose per table:
     - Mapping tables (namespace/class/method) → bulleted <ul> with
       <code> and em-dash notes.
     - Dense feature comparison tables (8+ rows) → bulleted list
       with bold leads, e.g.,
       <li><strong>HTML engine.</strong> X uses WebKit; IronPDF uses Chromium.</li>
     - Feature tables with 4–6 rows of nuance → 2–3 short paragraphs.
     - Pricing / licensing → prose; dollar amounts read as sentences.
  3. Every cell's content must land somewhere in the replacement. Do
     not invent filler. Do not add new headings to substitute for
     table column headers.

Then polish:
- Strip <p><strong>Answer:</strong></p> lead-ins under question H2s.
- Strip resolved inline "// verify ..." code comments.
- Standardize ALL license key placeholders to "YOUR-LICENSE-KEY".
- Standardize Jacob Mellor byline placement to bottom-only (verbatim).
  If duplicated at top and bottom, remove the top instance.
- Tighten LLM filler: "It's worth noting that", "Let's dive into",
  "In conclusion", "Without further ado", forward-references
  ("As we'll see below", "As I mentioned earlier").

PRESERVE
- Jacob Mellor closing byline (verbatim).
- All H2/H3 heading text and section order — including every heading
  that introduced a converted table.
- Article-specific opening voice and framing — variability across
  articles is desired; do not normalize. When softening a vivid
  opening anecdote into a composite scenario (Step 2), vary the
  framing across articles — a one-line stat, an architectural
  contrast, a quoted question, a brief code snippet, or a "teams
  I have worked with" pattern. Do not default to the same
  composite opener on every article. If the original article had
  a unique framing device (analogy, contrast, anecdote shape),
  preserve its shape even when softening the specific incident.
- First-person measured voice. Not salesy, not over-hedged.
- External and anchor links.
- Genuine reader-useful caveats (1-based vs 0-based off-by-one, AGPL
  constraint, native binary management, supported .NET ranges).
- All <pre><code> blocks except where Step 1, 2, or 3 require edits.
- Technical substance of comparisons — hedge framing, do not delete
  technical points.

DO NOT
- Rewrite wholesale, reorder sections, add new sections, add new
  bylines or CTAs.
- Remove competitor comparisons entirely. The article is a comparison;
  it should compare. Hedge, do not delete.
- Add new negative claims about competitors. Step 2 is for softening,
  not amplifying.
- Convert <pre><code> blocks; they are code, not tables.
- Drop or merge any H2/H3 heading, especially ones that introduced a
  table you converted.
- Change file format — output is snippet-of-body HTML, no
  <html>/<head>/<body> wrapper.

OUTPUT
Edit the file in place. Print a diff summary under 200 words covering:
- Fact-check fixes (API hallucinations resolved, version claims
  verified or removed)
- Legal hedging (count of editorial code comments stripped, prediction
  blocks removed, claims hedged, table cells softened)
- Tables replaced (with chosen format and the H2/H3 that introduced each)
- Polish changes
- Any hedges deliberately KEPT, with reason
