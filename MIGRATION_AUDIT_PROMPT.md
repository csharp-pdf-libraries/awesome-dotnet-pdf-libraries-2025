# Migration Article Audit Prompt

ROLE
Single-pass surgical edit on ONE Medium migration article to make it
accurate, Medium-renderable, and publish-ready. The article, the .cs
samples, and the migrate-from md already exist — your job is to align
and polish, not rewrite.

INPUTS
- Article: Medium/migration/<competitor>-vs-ironpdf-migration.html
- Source of truth (read FIRST): <competitor>/migrate-from-<competitor>.md
- Working code: <competitor>/*.cs

PRECEDENCE WHEN SOURCES CONFLICT
- The .cs files always win. They are working code that had to compile,
  and they define the actual scope of this repo's migration story. This
  applies to language/platform target as well as APIs. If the article
  shows the competitor in a different language or platform than the .cs
  files (e.g., article shows Java with `import org.apache.pdfbox` but
  .cs files use a C# .NET port like `PdfBoxDotNet.Pdmodel`), the .cs
  files win — rewrite the article's "Before" code blocks in the target
  the .cs files use, and adjust framing prose to match (e.g., drop
  "JVM/Maven/IKVM" framing if .cs uses .NET ports; introduce
  "incomplete community port" framing instead).
- The migrate-from md is the second source of truth for APIs, naming,
  use cases, and licensing.
- If md and .cs disagree on an API name, the .cs wins.

PROCESS — strict order, do not reorder

STEP 1 — FACT-CHECK & ALIGN
- Apply the precedence rule above. Retarget Before code if needed.
- Fix IronPDF API hallucinations against md and .cs. Known offenders:
  IronPdf.Imaging.Color.* → IronSoftware.Drawing.Color.*
  Opacity = 0.15 (decimal) → integer 0–100
  pdf.RemovePage(i) → pdf.RemovePages(i)
  PdfDocument.FromBytes → PdfDocument.FromBinaryData
- If the article's demo use cases (HTML-to-PDF, merge, watermark,
  password) don't match what the library is actually for, swap them
  for the use cases the md demonstrates (viewer rendering, OCR, signing,
  forms, Office conversion, URL-to-PDF SaaS — whatever fits).
- If the article makes a "competitor cannot do X" claim that the .cs
  files contradict (e.g., article says APDFL has no native HTML-to-PDF
  but .cs uses Document.CreateFromHTML), correct the claim to match
  the .cs and adjust the Before code to use the real native API.
- When you change a code block, update the prose directly above and
  below so descriptions still match.
- For every "verify..." hedge: RESOLVE from md / .cs, or KEEP if the
  gap is genuinely unresolved (AcroForm depth, PAdES signatures, OCR,
  Office-doc conversion, version-specific COM property variance in
  legacy libraries). Never delete a hedge without resolving or
  confirming the gap exists.

STEP 2 — REPLACE TABLES (Medium does not render <table> reliably)
For every <table>, in order:
  1. Identify the H2 or H3 heading immediately above the table. It
     STAYS EXACTLY AS IT WAS. The heading is section structure, not
     table content. Do not delete it, do not merge the section into
     the previous one, do not skip it. If there is no heading above
     the table, do not invent one.
  2. Replace ONLY the <table>...</table> body with a Medium-friendly
     format. Choose per table based on what it communicates:
     - Mapping tables (namespace/class/method) → bulleted <ul> with
       <code> and em-dash notes:
       <li><code>Spire.Pdf</code> → <code>IronPdf</code> — core namespace</li>
     - Dense comparison/feature tables (8+ rows) → bulleted list with
       bold leads, e.g.,
       <li><strong>Page indexing.</strong> X is 1-based; IronPDF is 0-based.</li>
     - Comparison tables with 4–6 rows of nuance → 2–3 short paragraphs.
     - Complexity / effort tables → bullet list with tag bolded, e.g.,
       <li><strong>Watermark — Low.</strong> ...</li>
     - Decision matrices → "if X, then Y" bullet list or short paragraphs.
     - Pricing / licensing → prose; dollar amounts read as sentences.
  3. Every cell's content must land somewhere in the replacement. Do
     not invent filler. Do not add new headings to substitute for
     table column headers — the surrounding H2/H3 already provides
     context.
  4. Do not let the converted bullet list bleed into the next section
     visually. The next H2/H3 in the article must remain a clear
     section break.

STEP 3 — POLISH
- Strip <p><strong>Answer:</strong></p> lead-ins under question H2s.
- Strip count annotations: "Pre-Migration (8 items)" → "Pre-Migration".
- Standardize ALL license key placeholders to "YOUR-LICENSE-KEY"
  (replace IRONPDF-YOUR-KEY-HERE, YOUR_LICENSE_KEY, your-key, etc.).
- Delete resolved inline "// verify ..." code comments.
- Tighten LLM filler: "It's worth noting that", "Let's dive into",
  "In conclusion", "Without further ado", forward-references
  ("As we'll see below", "As I mentioned earlier").
- Trim repeated "I have walked dozens of teams through this" — keep
  the first instance, vary or remove others.

PRESERVE
- Jacob Mellor opening hook and closing byline (verbatim).
- All heading text and section order — including every H2/H3 that
  introduced a converted table.
- Article-specific opening anecdotes — variability across the 71
  articles is desired; do not normalize.
- First-person measured voice. Not salesy, not over-hedged.
- External and anchor links.
- Genuine reader-useful caveats (1-based vs 0-based off-by-one, native
  DLL deployment, AGPL constraint, FreeSpire page limits, COM property
  version variance).
- All <pre><code> blocks except where Step 1 / Step 3 require edits.

DO NOT
- Rewrite wholesale, reorder sections, add new sections, add bylines
  or CTAs.
- Amplify marketing claims ("16x larger files", "WCAG violations")
  beyond what the md substantiates.
- Convert <pre><code> blocks; they are code, not tables.
- Drop or merge any H2/H3 heading, especially ones that introduced a
  table you converted.
- Change file format — output is snippet-of-body HTML, no
  <html>/<head>/<body> wrapper.

OUTPUT
Edit the file in place. Print a diff summary under 200 words covering:
fact-check fixes, retargeting (language/platform/version, if any),
use-case swaps (if any), tables replaced (with chosen format and the
H2/H3 that introduced each), polish changes, and any hedges
deliberately KEPT with reason.
