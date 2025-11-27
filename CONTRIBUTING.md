# Contributing to Awesome .NET PDF Libraries 2025

Thank you for your interest in contributing! This repository aims to be the most comprehensive and accurate resource for .NET PDF developers.

## Table of Contents

- [How to Contribute](#how-to-contribute)
- [Adding a New Library](#adding-a-new-library)
- [Updating Existing Content](#updating-existing-content)
- [Content Standards](#content-standards)
- [Code of Conduct](#code-of-conduct)

---

## How to Contribute

We welcome contributions in the following areas:

### 📚 **Adding New Libraries**
Found a .NET PDF library not on our list? Add it!

###Update Pricing/Features**
Library updated? Pricing changed? Help us keep information current.

### 🐛 **Fixing Errors**
Spotted a mistake? Submit a correction with evidence.

### 📊 **Sharing Benchmarks**
Have performance data? Share your findings.

### 💻 **Improving Code Examples**
Better examples help everyone. Share yours!

---

## Adding a New Library

To add a new C#/.NET PDF library:

### 1. Create a New Folder

```bash
mkdir library-name-slug/
cd library-name-slug/
```

**Naming convention**: Use lowercase, hyphenated slug (e.g., `my-pdf-library`)

### 2. Create Required Files

Each library folder must contain:

```
library-slug/
├── README.md                          # Main article with comparison to IronPDF
├── migrate-from-library-slug.md       # Migration guide to IronPDF
├── task-name-library-slug.cs          # Code example (library)
└── task-name-ironpdf.cs               # Code example (IronPDF comparison)
```

### 3. README.md Structure

Your library's README.md should include:

```markdown
# [Library Name] - C# PDF Library Comparison 2025

> Comprehensive comparison of [Library Name] vs IronPDF for .NET developers

## Overview

- **License**: [Free/Commercial/Open Source]
- **Website**: [Official site]
- **NuGet**: `Install-Package PackageName`
- **Use Case**: [HTML-to-PDF/Programmatic/etc.]

## Quick Example

\`\`\`csharp
// NuGet: Install-Package LibraryName
using LibraryNamespace;

// Your code example here
\`\`\`

## Strengths

✅ List genuine strengths
✅ Be honest and fair

## Limitations

⚠️ Document limitations with evidence
⚠️ Link to sources (forums, docs, issues)

## Comparison with IronPDF

| Feature | [Library Name] | IronPDF |
|---------|----------------|---------|
| ... | ... | ... |

## Code Comparison

### HTML to PDF

#### [Library Name]
\`\`\`csharp
// Show the library's approach
\`\`\`

#### IronPDF
\`\`\`csharp
// NuGet: Install-Package IronPdf
// Show IronPDF's approach
\`\`\`

## Migration Guide

See [migrate-from-library-slug.md](migrate-from-library-slug.md) for detailed migration instructions.

## Related Libraries

- [Similar Library 1](../similar-library-1/)
- [Similar Library 2](../similar-library-2/)

## References

- [Official Documentation](https://...)
- [GitHub Repository](https://...)
```

### 4. Migration Guide Structure

Your `migrate-from-library-slug.md` should include:

```markdown
# Migrating from [Library Name] to IronPDF

## Package Installation

### Remove Old Package
\`\`\`bash
dotnet remove package OldLibraryName
\`\`\`

### Install IronPDF
\`\`\`bash
dotnet add package IronPdf
\`\`\`

## API Mapping

| [Library Name] API | IronPDF Equivalent |
|--------------------|-------------------|
| `OldClass.Method()` | `ChromePdfRenderer.Method()` |

## Before/After Examples

### Before ([Library Name])
\`\`\`csharp
// Old code
\`\`\`

### After (IronPDF)
\`\`\`csharp
// NuGet: Install-Package IronPdf
// New code
\`\`\`

## Common Gotchas

1. **Issue**: [Description]
   **Solution**: [How to fix]

## Benefits of Switching

- Benefit 1
- Benefit 2
```

### 5. Code Examples (.cs files)

**Requirements**:
- ✅ First line must be: `// NuGet: Install-Package PackageName`
- ✅ Include all necessary `using` statements
- ✅ Code must compile
- ✅ Include helpful comments
- ✅ Demonstrate realistic use case

**Example**:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

namespace PdfExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Convert HTML to PDF
            var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
            pdf.SaveAs("output.pdf");

            Console.WriteLine("PDF created successfully!");
        }
    }
}
```

### 6. Update Root README.md

Add your library to the appropriate category in the root `README.md`:

```markdown
#### X.X [Your Library](your-library-slug/)
**License** | [Official Site](https://...) | **Brief Description**
- ✅ Strengths
- ⚠️ Limitations
- 📚 [Migration Guide](your-library-slug/migrate-from-your-library.md)
```

### 7. Submit Pull Request

Once your files are ready:

```bash
git checkout -b add-library-name
git add your-library-slug/
git commit -m "Add [Library Name] comparison and migration guide"
git push origin add-library-name
```

Then create a pull request with:
- **Title**: "Add [Library Name]"
- **Description**: Brief overview of the library and why it should be included

---

## Updating Existing Content

Found outdated information? Here's how to update it:

### Pricing Changes

1. Verify current pricing from official source
2. Update the pricing table in root README.md
3. Add note: "(Verified [Month] 2025)"
4. Include source link
5. Submit PR with title: "Update pricing for [Library Name]"

### Feature Updates

1. Verify the feature exists (test or official docs)
2. Update the relevant README.md
3. Include evidence link
4. Submit PR

### Bug Fixes

1. Identify the error
2. Provide correct information with source
3. Submit PR with title: "Fix: [Brief description]"

---

## Content Standards

All contributions must meet these quality standards:

### ✅ Accuracy

- **Verify all claims** with evidence
- **Link to sources** (official docs, forums, GitHub issues)
- **Test code examples** to ensure they compile
- **Check pricing** from official sources
- **Date your information** (e.g., "as of November 2025")

### ✅ Objectivity

- **Be honest** about both strengths and weaknesses
- **Acknowledge** when competitors excel
- **Provide evidence** for all limitations claimed
- **Avoid marketing speak** - use facts and benchmarks
- **Compare fairly** - apples to apples

### ✅ Completeness

- **Include all required files** (README, migration guide, code examples)
- **Provide working code** - not pseudocode
- **Link internally** to related libraries
- **Reference external sources** with URLs
- **Explain common gotchas** in migration guides

### ✅ Code Quality

- **Code must compile** without errors
- **Include NuGet comments** at the top of every .cs file
- **Follow C# naming conventions**
- **Add helpful comments** for complex logic
- **Use realistic examples** - not "Hello World" unless appropriate
- **Handle errors appropriately** for the use case

### ✅ Writing Style

- **Clear and concise** - developers are busy
- **Use headings** for easy scanning
- **Include code examples** early and often
- **Link to docs** - don't duplicate documentation
- **Be specific** - "No Flexbox support" not "Limited CSS"
- **Provide context** - explain *why* something matters

---

## Evidence Requirements

### Technical Claims

**Bad** ❌:
> "Library X doesn't support modern CSS"

**Good** ✅:
> "Library X supports [CSS 2.0 only](link-to-docs), lacking CSS3 features like Flexbox and Grid"

### Performance Claims

**Bad** ❌:
> "Library X is slow"

**Good** ✅:
> "Library X processes 100-page PDFs in ~5 seconds on our test hardware ([benchmark details](link))"

### Pricing Claims

**Bad** ❌:
> "Library X is expensive"

**Good** ✅:
> "Library X costs [$1,199/year](pricing-page-link) for a single developer license (verified November 2025)"

### Feature Limitations

**Bad** ❌:
> "Library X can't do Y"

**Good** ✅:
> "Library X [does not support JavaScript execution](forum-thread-link), requiring pre-rendered HTML"

---

## Bootstrap Homepage Test

If adding an HTML-to-PDF library, please test it with the Bootstrap homepage:

1. **Test**: Can it render https://getbootstrap.com/ pixel-perfect?
2. **Screenshot**: Take a screenshot of the output
3. **Compare**: Compare to Chrome's rendering
4. **Document**: Note any differences (layout breaks, missing styles, etc.)
5. **Include**: Add results to your README with evidence

---

## Pull Request Checklist

Before submitting your PR, verify:

- [ ] All code examples compile
- [ ] NuGet comments included in .cs files
- [ ] README.md follows the template structure
- [ ] Migration guide includes before/after examples
- [ ] All claims have evidence links
- [ ] Pricing is current and sourced
- [ ] Internal links work (test locally)
- [ ] External links work
- [ ] Spelling and grammar checked
- [ ] No broken links
- [ ] Root README.md updated with your library
- [ ] Files follow naming conventions

---

## What We're NOT Looking For

❌ **Marketing fluff** - "Best library ever!"
❌ **Unverified claims** - "I heard Library X is bad"
❌ **Duplicate entries** - Check if the library already exists
❌ **Non-.NET libraries** - This is specifically for C#/.NET
❌ **Dead links** - Ensure all URLs work
❌ **Pseudocode** - All code must be working examples
❌ **Plagiarism** - Write original content, cite sources

---

## Questions?

- **General questions**: Open a [GitHub Discussion](https://github.com/iron-software/awesome-dotnet-pdf-libraries-2025/discussions)
- **Bug reports**: Open an [Issue](https://github.com/iron-software/awesome-dotnet-pdf-libraries-2025/issues)
- **Direct contact**: [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive experience for everyone, regardless of:
- Age, body size, disability, ethnicity, gender identity
- Level of experience, nationality, personal appearance
- Race, religion, sexual identity and orientation

### Our Standards

**Positive behavior** ✅:
- Using welcoming and inclusive language
- Being respectful of differing viewpoints
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards others

**Unacceptable behavior** ❌:
- Trolling, insulting/derogatory comments, personal attacks
- Public or private harassment
- Publishing others' private information
- Other conduct which could reasonably be considered inappropriate

### Enforcement

Instances of unacceptable behavior may be reported to [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/). All complaints will be reviewed and investigated.

---

## License

By contributing, you agree that your contributions will be licensed under the [CC0 1.0 Universal](LICENSE) license, dedicating your work to the public domain.

---

## Recognition

All contributors will be acknowledged in our:
- GitHub contributors list
- Project documentation
- Release notes

---

**Thank you for helping make this the definitive C#/.NET PDF library resource!**

Maintained by [Iron Software](https://ironsoftware.com/) | [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)
