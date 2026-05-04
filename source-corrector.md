# Subdirectory Source-of-Truth Corrector

Single-pass prompt for verifying and correcting ONE competitor's subdirectory content (`.cs` files + migrate-from md). Run this BEFORE `medium-corrector.md` or `comparison-corrector.md` — it fixes the upstream source-of-truth so downstream prompts inherit correct facts. Replace `<competitor>` with the subdirectory name (e.g., `pdfbolt`, `apryse`, `apache-pdfbox`, `asposepdf`).

---

ROLE
Single-pass surgical edit on ONE competitor's subdirectory to make the
source-of-truth content factually accurate. The .cs files and the
migrate-from md drive every downstream article in this repo (migration,
comparison, README). Your job is to verify each factual claim against
authoritative sources and correct fabrications, while keeping the
comparison thrust, structure, and tone intact.

INPUTS
- Competitor subdirectory: <competitor>/
- Files to verify and edit:
  - <competitor>/migrate-from-<competitor>.md
  - <competitor>/*.cs (both competitor-side AND IronPDF-side samples)
  - <competitor>/README.md (read for context; edit only if a fact is wrong)
- Web access REQUIRED. Use WebSearch and WebFetch against vendor docs,
  nuget.org, GitHub, and current pricing pages. Trust vendor primary
  sources over third-party reviews.

PROCESS — strict order

STEP 1 — CATEGORIZE THE COMPETITOR
Determine the integration model first; this dictates what "correct"
.cs code should look like:
- A. NuGet-installed .NET library (Aspose.PDF, Spire.PDF, PDFsharp,
     iText 7, Syncfusion, etc.): verify package name on nuget.org
     and class/method names against vendor docs.
- B. REST API SaaS (PDFBolt, api2pdf, pdfmyurl, grabzit, craftmypdf,
     pdforge, pdfreactor cloud, etc.): typically NO official .NET
     SDK — integration is HttpClient + JSON POST to documented
     endpoints. .cs files must reflect this, not a fabricated SDK.
- C. Java library with .NET ports (Apache PDFBox, etc.): verify which
     community port the .cs files use and confirm the namespace exists.
- D. Wrapper around external binary (DinkToPdf / TuesPechkin around
     wkhtmltopdf, etc.): verify the wrapper's NuGet package and the
     binary it depends on.
- E. Reporting platform (Crystal Reports, Telerik Reporting, SSRS,
     FastReport): integration is template files + runtime engine,
     not a simple library API.
- F. Discontinued / EOL product (iTextSharp 5.x, wkhtmltopdf, etc.):
     verify EOL status, last release date, and named successor.
- G. Acquired / rebranded product (PDFTron → Apryse, ActivePDF under
     Foxit, etc.): use the current vendor name and current packages.

STEP 2 — VERIFY KEY FACTS VIA WEB SEARCH
Verify each of these against authoritative sources before trusting the
existing content. Note source URLs for the diff summary.
- NuGet package name and current version (search nuget.org directly).
- Real namespace and class names (vendor docs, GitHub repo if open
  source).
- For REST APIs: actual endpoint URLs, HTTP method, request body
  shape, auth header name.
- Pricing and free-tier numbers (vendor pricing page only — third-
  party review sites are often stale).
- Last release date, EOL status, version-support claims, last CVE.
- Cross-platform support (Linux, Docker, .NET Framework / Core / 6+).
- License terms (AGPL, MIT, commercial, perpetual vs subscription).
- Feature claims — both "supports X" and "does NOT support X". Check
  HTML-to-PDF, PDF/A vs PDF/X (these are different standards),
  digital signatures, forms, OCR, Office conversion, async support,
  watermark, merge, text extraction. Don't conflate adjacent standards.
- Acquisition / rebrand status. Use the current name in the docs and
  acknowledge the previous name once.
- Vendor product family. If the vendor sells multiple SKUs (e.g.,
  Aspose.PDF vs Aspose.Words; Spire.PDF vs FreeSpire.PDF), confirm
  which one this subdirectory is about — do not conflate.

STEP 3 — EDIT FILES TO MATCH VERIFIED FACTS

For competitor-side .cs files:
- If the file shows a fabricated SDK that does not exist (e.g., a
  class invented for a REST-only service), REWRITE to use the real
  integration pattern. For REST APIs, use HttpClient with the
  documented endpoint, auth header, and JSON body.
- If the namespace or class is wrong (renamed, deprecated, never
  existed), update to the verified current API.
- Keep the spirit of the example (HTML-to-PDF, merge, watermark,
  etc.). Don't change which operation is demonstrated, only how
  it's done.
- The top-of-file comment should accurately state how to install
  (`// NuGet: Install-Package <real-package-name>`) or note REST-
  only (`// REST API — no official .NET SDK; see https://vendor.com/docs`).

For IronPDF-side .cs files:
- Verify against current IronPDF public surface. Known hallucinations
  to fix:
  pdf.Password / pdf.OwnerPassword → pdf.SecuritySettings.UserPassword / OwnerPassword
  pdf.RemovePage(i) → pdf.RemovePages(i)
  IronPdf.Imaging.Color.* → IronSoftware.Drawing.Color.*
  Opacity = 0.15 (decimal) → integer 0-100
  PdfDocument.FromBytes → PdfDocument.FromBinaryData
- Add `IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";` if missing.

For migrate-from md:
- Update tables and prose where the .cs file changed; the md should
  cite the same APIs the .cs files now demonstrate.
- Fix specific factual claims: pricing, free-tier numbers, package
  names, version numbers, EOL status, vendor name.
- Where a "competitor cannot do X" claim was wrong (the competitor
  actually does support X), correct it. Where it was right but
  imprecise (e.g., conflating PDF/A with PDF/X), tighten it.
- Keep the migration thrust, the comparison framing, the section
  structure, and the tone. Do not rewrite for style.

STEP 4 — FLAG WHAT YOU CANNOT VERIFY
If a claim cannot be confirmed via web search (vendor docs behind
login, product too obscure, vendor's site down), do NOT silently
leave it. Either:
- Mark with a hedge in the md ("verify at <vendor>.com — could not
  confirm during this pass"), or
- Note explicitly in the OUTPUT summary as "unverified".
Never fabricate to fill a gap.

PRESERVE
- The comparison thrust (this is competitor X vs IronPDF; IronPDF is
  the migration target).
- Migrate-from md section structure, headings, and table-of-contents.
- The voice — measured, technical, first-person, not salesy.
- README.md unless a fact in it is provably wrong.
- File names and locations. Don't rename, don't add new .cs files,
  don't delete examples.
- Existing accurate code samples — don't rewrite working code for style.
- Existing accurate prose — don't paraphrase for the sake of it.

DO NOT
- Touch any file outside <competitor>/ — Medium/ articles are out
  of scope for this prompt; they are fixed by separate downstream
  prompts after this pass.
- Rewrite code or prose for style; only for factual accuracy.
- Add or remove .cs example files; edit existing ones in place.
- Drop the IronPDF-favoring framing; make it factually defensible
  instead by softening unsupported claims into verified ones.
- Invent "facts" to replace ones you couldn't verify — flag them.
- Web-search exhaustively when a fact is already on a page you have
  open; be efficient.
- Change file format (markdown stays markdown, .cs stays .cs).

OUTPUT
Edit files in place. Print a diff summary under 300 words covering:
- Competitor category (A–G from Step 1) and a one-line justification.
- Facts verified, with source URLs as evidence.
- Facts changed: old value → new value, with citation per change.
- Files edited (each .cs and the md), one-line change summary each.
- Facts flagged as unverified, with reason and a recommended next step
  (e.g., "vendor pricing page requires login — confirm with sales").
