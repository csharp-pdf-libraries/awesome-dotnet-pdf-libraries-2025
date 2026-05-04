# Subdirectory Source-of-Truth Cleanup (Pass 2)

Sequel to `source-corrector.md`. Run AFTER source-corrector has finished a competitor's subdirectory. Goal: resolve any remaining `verify...` hedges and `unverified — could not confirm` markers via a second focused web-search pass, then remove the markers. The subdirectory should leave this pass ready to publish, with zero leftover flags. Replace `<competitor>` with the subdirectory name.

---

ROLE
Cleanup pass on ONE competitor's subdirectory. Pass 1 may have left
flags where it could not verify a claim. Your job is to take a second
look, confirm or correct each flag, and exit with no leftover markers.

INPUTS
- Competitor subdirectory: <competitor>/
- Files: <competitor>/migrate-from-<competitor>.md and <competitor>/*.cs
- Web access REQUIRED.

PROCESS

STEP 1 — INVENTORY THE FLAGS
Scan the md and .cs files for:
- Prose hedges: "verify against <vendor>.com", "verify in your version's
  docs", "verify against the current docs", "confirm before using".
- Pass-1 explicit markers: "could not confirm during this pass",
  "unverified", "verify against vendor source".
- Inline code comments: `// verify enum name`, `// verify property name`,
  `// verify method name`, `// verify package name`.

If no markers are found, output "No remaining flags — already clean"
and exit. Do not invent work.

STEP 2 — VERIFY VIA WEB SEARCH
For each flag, do a focused web search against the vendor's primary
sources: official docs, nuget.org, vendor pricing page, the GitHub
repo if open source. Trust vendor primary sources over third-party
review sites.

STEP 3 — RESOLVE EACH FLAG (one of three outcomes)
- A. CONFIRMED: the claim is correct as written. Remove the marker,
     leave surrounding prose intact, cite the source URL in the diff.
- B. CORRECTED: the claim was wrong or imprecise. Update to the
     verified value (number, name, version, capability), remove the
     marker, cite the source.
- C. STILL UNVERIFIABLE: vendor docs behind a login, product genuinely
     obscure, or the claim is not directly addressable online. Do NOT
     leave a marker for a Pass 3. Choose the smaller risk:
     - If the claim is decorative (a side detail), DELETE the sentence.
     - If the claim is load-bearing (a key comparison point), SOFTEN
       the prose so the unverified specific is no longer load-bearing
       (replace a specific number with a general phrase, replace a
       definitive capability claim with an architectural observation).
       Then remove the marker.

STEP 4 — DO NOT GENERATE NEW MARKERS
This pass is cleanup-only. Do not introduce new "verify..." hedges,
"could not confirm" notes, or new inline `// verify ...` comments.
Every flag from Pass 1 must exit in state A, B, or C.

PRESERVE
- The comparison thrust, voice, and section structure from Pass 1.
- Genuine reader-useful caveats unrelated to markers (1-based vs
  0-based off-by-one, AGPL constraint, native binary management,
  EOL warnings). Only touch the markers Pass 1 left.
- Existing accurate code and prose — don't rewrite anything not
  tied to a flag.

DO NOT
- Touch files outside <competitor>/.
- Re-litigate decisions Pass 1 already verified.
- Introduce new examples, sections, or markers.
- Soften or delete anything that wasn't flagged.
- Run web searches for facts that are not flagged — be efficient.

OUTPUT
Edit files in place. Print a diff summary under 200 words covering:
- Total flags found in Pass 1.
- Per flag: file, original claim, resolution (A/B/C), source URL
  where applicable.
- Final state: 0 markers remaining (success condition).
- Any state-C softenings, with one-line reasoning each so a human
  can spot-check whether the softening was acceptable.
