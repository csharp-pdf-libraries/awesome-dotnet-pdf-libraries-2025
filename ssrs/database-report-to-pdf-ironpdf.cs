// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

class IronPdfDatabaseReport
{
    static void Main()
    {
        // Create database connection and fetch data
        string connString = "Server=localhost;Database=SalesDB;Integrated Security=true;";
        var dataTable = new DataTable();
        
        using (var connection = new SqlConnection(connString))
        {
            var adapter = new SqlDataAdapter("SELECT * FROM Sales", connection);
            adapter.Fill(dataTable);
        }
        
        // Build HTML table from data
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<h1>Sales Report</h1><table border='1'><tr>");
        
        foreach (DataColumn column in dataTable.Columns)
            htmlBuilder.Append($"<th>{column.ColumnName}</th>");
        htmlBuilder.Append("</tr>");
        
        foreach (DataRow row in dataTable.Rows)
        {
            htmlBuilder.Append("<tr>");
            foreach (var item in row.ItemArray)
                htmlBuilder.Append($"<td>{item}</td>");
            htmlBuilder.Append("</tr>");
        }
        htmlBuilder.Append("</table>");
        
        // Convert to PDF
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlBuilder.ToString());
        pdf.SaveAs("sales-report.pdf");
    }
}