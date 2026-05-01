// NuGet: Install-Package RawPrint  (frogmorecs/RawPrint, v0.5.0 — package is unlisted/legacy on nuget.org)
// Repo: https://github.com/frogmorecs/RawPrint
// RawPrint hands bytes to the Windows spooler with the RAW datatype. Any "formatting" has to
// be expressed in the printer's own command language (PCL, PostScript, ESC/POS for receipt
// printers, ZPL/EPL for Zebra label printers). RawPrint itself has no concept of fonts,
// margins, or layout — that is the printer firmware's job.
using System;
using System.IO;
using System.Text;
using RawPrint;

class Program
{
    static void Main()
    {
        // Example: a tiny PCL5 stream that selects portrait orientation and a 16.66cpi font,
        // then prints a line of text. Real-world usage is usually ESC/POS for receipt printers
        // or ZPL for Zebra label printers — RawPrint is agnostic, it just ships the bytes.
        string pclCommands = "\x1B&l0O\x1B(s0p16.66h8.5v0s0b3T";
        string text = "Plain text document - formatting must be expressed in printer command language";
        byte[] data = Encoding.ASCII.GetBytes(pclCommands + text);

        IPrinter printer = new Printer();
        using (var stream = new MemoryStream(data))
        {
            printer.PrintRawStream("HP LaserJet", stream, "Raw Document", false);
        }
    }
}
