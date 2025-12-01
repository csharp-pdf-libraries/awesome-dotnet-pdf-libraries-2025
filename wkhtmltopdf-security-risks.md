# Why You Must Replace wkhtmltopdf: Critical Security Vulnerabilities and the Path to Modern PDF Generation

## Executive Summary

**wkhtmltopdf is abandoned, insecure, and creates a critical attack vector on your servers.**

If your organization uses wkhtmltopdf—directly or through wrappers like DinkToPdf, Rotativa, TuesPechkin, or HaukCodeDinkToPdf—you are running software with:

- **CVE-2022-35583**: A CRITICAL (CVSS 9.8) SSRF vulnerability that will never be patched
- **Abandoned since 2020**: No security updates, no bug fixes, no support
- **Outdated WebKit engine from 2015**: Fails modern web standards
- **Known attack vectors**: Actively exploited for SSRF, LFI, and initial access in targeted attacks

This article explains the risks and provides a clear migration path to IronPDF.

---

## Table of Contents

1. [The State of wkhtmltopdf](#the-state-of-wkhtmltopdf)
2. [Critical Security Vulnerabilities](#critical-security-vulnerabilities)
3. [Attack Scenarios](#attack-scenarios)
4. [Why This Matters for Your Organization](#why-this-matters-for-your-organization)
5. [Affected Tools and Libraries](#affected-tools-and-libraries)
6. [The Solution: Migrate to IronPDF](#the-solution-migrate-to-ironpdf)
7. [Migration Guides](#migration-guides)
8. [Additional Resources](#additional-resources)

---

## The State of wkhtmltopdf

### Official Abandonment

wkhtmltopdf was officially declared abandoned by its maintainers. From the [official status page](https://wkhtmltopdf.org/status.html):

> "This project is abandoned and is now insecure."

The project's decline began when:

1. **2013**: Google forked WebKit into Blink
2. **2015**: Qt deprecated QtWebKit
3. **2016**: Qt removed QtWebKit entirely
4. **2020**: Last wkhtmltopdf release (0.12.6)
5. **2022**: Critical CVE-2022-35583 disclosed—with no fix

### Why It Was Abandoned

wkhtmltopdf depends on QtWebKit, which itself was abandoned:

- QtWebKit was "on life support for years" after being deprecated
- The JavaScript engine doesn't support ES6, breaking modern web content
- The rendering engine is based on WebKit from circa 2015—a decade behind modern browsers
- No compatibility with CSS3 features like Flexbox and Grid
- Inconsistent JavaScript execution

### The "Abandonware" Classification

wkhtmltopdf is now classified as **abandonware**—software that is no longer updated, maintained, or supported by its developers. This means:

- No security patches for newly discovered vulnerabilities
- No bug fixes
- No compatibility updates
- No one monitoring for exploits

---

## Critical Security Vulnerabilities

### CVE-2022-35583: Server-Side Request Forgery (SSRF)

| Attribute | Value |
|-----------|-------|
| **CVE ID** | CVE-2022-35583 |
| **CVSS Score** | **9.8 CRITICAL** |
| **Status** | **UNPATCHED** |
| **Affected Versions** | All versions including 0.12.6 (latest) |

#### Description

From the [GitHub Advisory Database](https://github.com/advisories/GHSA-v2fj-q75c-65mr):

> "wkhtmlTOpdf 0.12.6 is vulnerable to SSRF which allows an attacker to get initial access into the target's system by injecting iframe tag with initial asset IP address on its source. This allows the attacker to takeover the whole infrastructure by accessing their internal assets."

#### Impact

An attacker can:

1. **Access internal services**: Reach databases, APIs, and admin panels behind your firewall
2. **Exfiltrate AWS credentials**: Access `169.254.169.254` to steal IAM role credentials
3. **Read local files**: Use `file://` protocol to read `/etc/passwd`, private keys, and configuration files
4. **Map internal networks**: Discover internal services and IP addresses
5. **Pivot attacks**: Use initial access for lateral movement and privilege escalation

### Local File Inclusion (LFI)

From [Virtue Security's analysis](https://www.virtuesecurity.com/kb/wkhtmltopdf-file-inclusion-vulnerability-2/):

> "Because wkhtmltopdf renders HTML content on the server-side, it is a high risk target for both Server-side Request Forgery (SSRF) and Local File Inclusion (LFI) vulnerabilities."

An attacker can read arbitrary files:

```html
<iframe src="file:///etc/passwd" width="800" height="600"></iframe>
```

```html
<img src="x" onerror="document.write('<iframe src=file:///etc/shadow></iframe>')">
```

### Same-Origin Policy Bypass

From [Ubuntu Security Notice USN-6232-1](https://ubuntu.com/security/notices/USN-6232-1):

> "wkhtmltopdf was not properly enforcing the same-origin policy when processing certain HTML files. If a user or automated system using wkhtmltopdf were tricked into processing a specially crafted HTML file, an attacker could possibly use this issue to expose sensitive information."

---

## Attack Scenarios

### Scenario 1: AWS Credential Theft

**Target**: Any application using wkhtmltopdf on AWS EC2

**Attack Vector**:
```html
<iframe src="http://169.254.169.254/latest/meta-data/iam/security-credentials/" width="800" height="600"></iframe>
```

**Impact**:
- Attacker obtains IAM role credentials
- Access to all AWS services the role can access (S3, RDS, Lambda, etc.)
- Potential for complete cloud infrastructure compromise

### Scenario 2: Internal Network Discovery

**Target**: Corporate application with PDF generation

**Attack Vector**:
```html
<script>
  for (let i = 1; i <= 255; i++) {
    new Image().src = `http://192.168.1.${i}:8080/probe`;
  }
</script>
<iframe src="http://192.168.1.1:8080/admin" width="800" height="600"></iframe>
```

**Impact**:
- Map internal network topology
- Discover internal services (Jenkins, GitLab, databases)
- Find targets for lateral movement

### Scenario 3: Initial Access for Ransomware

**Target**: Enterprise with public-facing PDF generation

**Attack Chain**:
1. Attacker submits malicious HTML to PDF converter
2. SSRF reveals internal services and credentials
3. Attacker gains foothold on internal network
4. Lateral movement to domain controller
5. Deployment of ransomware across organization

**This is not theoretical**: SSRF vulnerabilities are a [MITRE ATT&CK technique (T1190)](https://attack.mitre.org/techniques/T1190/) used by advanced persistent threats for initial access.

### Scenario 4: Configuration File Exfiltration

**Target**: Any server with wkhtmltopdf

**Attack Vector**:
```html
<iframe src="file:///var/www/app/.env" width="800" height="600"></iframe>
<iframe src="file:///app/config/database.yml" width="800" height="600"></iframe>
<iframe src="file:///root/.ssh/id_rsa" width="800" height="600"></iframe>
```

**Impact**:
- Database credentials
- API keys
- SSH private keys
- JWT secrets

---

## Why This Matters for Your Organization

### Compliance Failures

Using abandoned, vulnerable software can violate:

- **PCI DSS**: Requirement 6.2 - Protect system components from known vulnerabilities
- **HIPAA**: Security Rule requirements for software maintenance
- **SOC 2**: CC6.1 - Security vulnerabilities are identified and addressed
- **ISO 27001**: A.12.6.1 - Management of technical vulnerabilities
- **GDPR**: Article 32 - Appropriate security measures

### Audit Red Flags

Security auditors specifically look for:

- Software with known unpatched CVEs
- Abandoned dependencies
- Internet-facing services with SSRF vulnerabilities

wkhtmltopdf checks all three boxes.

### Insurance Implications

Cyber insurance policies often exclude:

- Breaches resulting from known, unpatched vulnerabilities
- Failure to maintain security updates
- Negligent security practices

Running wkhtmltopdf could void your coverage.

### Real-World Exposure

wkhtmltopdf is widely deployed:

- Installed on millions of Linux servers
- Used by popular frameworks (Rails, Django, Laravel, .NET)
- Bundled in Docker images
- Deployed in CI/CD pipelines

Your organization likely has multiple instances.

---

## Affected Tools and Libraries

wkhtmltopdf is used directly and through numerous wrappers. If you use any of these, you're vulnerable:

### .NET / C#

| Library | Status | Migration Guide |
|---------|--------|-----------------|
| **DinkToPdf** | Abandoned (2018) | [Migrate to IronPDF](dinktopdf/migrate-from-dinktopdf.md) |
| **Rotativa** | Uses wkhtmltopdf | [Migrate to IronPDF](rotativa/migrate-from-rotativa.md) |
| **TuesPechkin** | Abandoned | [Migrate to IronPDF](tuespechkin/migrate-from-tuespechkin.md) |
| **HaukCodeDinkToPdf** | Fork of DinkToPdf | [Migrate to IronPDF](haukcodedinktopdf/migrate-from-haukcodedinktopdf.md) |
| **WickedPdf** | Discussing migration | See alternatives |

### Python

| Library | Status |
|---------|--------|
| **pdfkit** | Uses wkhtmltopdf |
| **python-pdfkit** | Uses wkhtmltopdf |
| **WeasyPrint** | Alternative (Cairo-based) |

### Ruby

| Library | Status |
|---------|--------|
| **wicked_pdf** | Uses wkhtmltopdf |
| **PDFKit** | Uses wkhtmltopdf |
| **prawn** | Alternative (pure Ruby) |

### PHP

| Library | Status |
|---------|--------|
| **snappy** | Uses wkhtmltopdf |
| **laravel-snappy** | Uses wkhtmltopdf |
| **Dompdf** | Alternative |

### Node.js

| Library | Status |
|---------|--------|
| **wkhtmltopdf** | Direct wrapper |
| **Puppeteer** | Alternative (Chromium-based) |
| **Playwright** | Alternative (multi-browser) |

---

## The Solution: Migrate to IronPDF

### Why IronPDF

| Aspect | wkhtmltopdf | IronPDF |
|--------|-------------|---------|
| **Security** | CVE-2022-35583 (CRITICAL) | No known vulnerabilities |
| **Maintenance** | Abandoned (2020) | Actively maintained |
| **Rendering Engine** | WebKit (2015) | Chromium (current) |
| **CSS Support** | No Flexbox/Grid | Full CSS3 |
| **JavaScript** | ES5 only, unreliable | Full ES6+ |
| **Native Dependencies** | Platform-specific binaries | Pure NuGet package |
| **Thread Safety** | Crashes under load | Thread-safe |
| **Support** | None | Professional support |

### Security Architecture

IronPDF addresses wkhtmltopdf's security issues by design:

1. **Sandboxed Rendering**: Chromium runs in a sandboxed process
2. **No Same-Origin Bypass**: Proper security policies enforced
3. **No Local File Access by Default**: Configurable security settings
4. **No SSRF Vulnerabilities**: Secure network access controls
5. **Regular Security Updates**: Chromium updates included in releases

### Simple Migration

**Before (wkhtmltopdf/DinkToPdf):**
```csharp
var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    Objects = {
        new ObjectSettings() { HtmlContent = html }
    }
};
byte[] pdf = converter.Convert(doc);
// Vulnerable to SSRF, LFI, crashes under load
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Secure, stable, modern
```

### Immediate Actions

1. **Inventory**: Find all wkhtmltopdf instances in your infrastructure
2. **Assess Risk**: Identify internet-facing or user-input-processing instances
3. **Prioritize**: Start with highest-risk deployments
4. **Migrate**: Follow the guides below
5. **Remove**: Completely uninstall wkhtmltopdf from servers

---

## Migration Guides

### .NET / C# Libraries

- **[DinkToPdf → IronPDF](dinktopdf/migrate-from-dinktopdf.md)**: Complete API mapping, 10 code examples
- **[Rotativa → IronPDF](rotativa/migrate-from-rotativa.md)**: ASP.NET MVC/Core migration
- **[TuesPechkin → IronPDF](tuespechkin/migrate-from-tuespechkin.md)**: Threading and synchronization migration
- **[HaukCodeDinkToPdf → IronPDF](haukcodedinktopdf/migrate-from-haukcodedinktopdf.md)**: DinkToPdf fork migration

### Find wkhtmltopdf in Your Infrastructure

```bash
# Find wkhtmltopdf binaries
find / -name "wkhtmltopdf" -o -name "wkhtmltoimage" -o -name "libwkhtmltox*" 2>/dev/null

# Check Docker images
docker images --format "{{.Repository}}" | xargs -I {} sh -c 'docker run --rm {} which wkhtmltopdf 2>/dev/null && echo "Found in: {}"'

# Search .NET projects
grep -r "DinkToPdf\|Rotativa\|TuesPechkin\|wkhtmltopdf" --include="*.csproj" --include="*.cs" .

# Search Python projects
grep -r "pdfkit\|wkhtmltopdf" --include="*.py" --include="requirements.txt" .

# Search Ruby projects
grep -r "wicked_pdf\|pdfkit\|wkhtmltopdf" --include="*.rb" --include="Gemfile" .
```

---

## Additional Resources

### Security Advisories

- [CVE-2022-35583 - NVD](https://nvd.nist.gov/vuln/detail/CVE-2022-35583)
- [GitHub Advisory GHSA-v2fj-q75c-65mr](https://github.com/advisories/GHSA-v2fj-q75c-65mr)
- [Snyk Vulnerability Database](https://security.snyk.io/vuln/SNYK-UNMANAGED-WKHTMLTOPDFWKHTMLTOPDF-2988835)
- [Ubuntu Security Notice USN-6232-1](https://ubuntu.com/security/notices/USN-6232-1)

### Analysis and Exploitation

- [Virtue Security: wkhtmltopdf File Inclusion Vulnerability](https://www.virtuesecurity.com/kb/wkhtmltopdf-file-inclusion-vulnerability-2/)
- [SSRF via PDF Generation (Medium)](https://medium.com/@parab500/part-1-uncovering-the-vulnerability-how-attackers-can-exploit-ssrf-via-pdf-generation-bf91a42b8e67)
- [AppSecEngineer: Safeguarding Against SSRF](https://www.appsecengineer.com/blog/safeguarding-against-ssrf-securing-html-to-pdf-conversion)

### Official Deprecation

- [wkhtmltopdf Status Page](https://wkhtmltopdf.org/status.html)
- [wkhtmltopdf is Abandonware (Doppio)](https://doc.doppio.sh/article/wkhtmltopdf-is-now-abandonware.html)
- [CiviCRM Advisory: wkhtmltopdf EOL](https://civicrm.org/advisory/civi-psa-2024-01-wkhtmltopdf-eol)

### IronPDF Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Support](https://ironpdf.com/support/)

---

## Conclusion

**wkhtmltopdf is a ticking time bomb in your infrastructure.**

Every day you continue running it:

- You expose your organization to CRITICAL SSRF and LFI vulnerabilities
- You risk becoming an initial access vector for advanced attacks
- You operate non-compliant systems that could void cyber insurance
- You render modern web content incorrectly

The solution is clear: **migrate to IronPDF**.

IronPDF provides:
- A modern, secure Chromium-based rendering engine
- Active maintenance and security updates
- Full CSS3 and JavaScript support
- Thread-safe, production-ready architecture
- Professional support

Don't wait for a breach. Start your migration today.

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*

---

*Last updated: 2024. For the latest security advisories, check the links in [Additional Resources](#additional-resources).*
