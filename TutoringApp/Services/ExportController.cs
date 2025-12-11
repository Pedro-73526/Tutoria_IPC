using System.Linq.Dynamic.Core;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Globalization;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using OfficeOpenXml;
using System.Drawing;
using TutoringApp.Models;

namespace TutoringApp.Services
{
    public partial class ExportController
    {
        private void ExportToExcel(IEnumerable<Enrollement> data)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");

                // Write header
                worksheet.Cells[1, 1].Value = "Column1";
                worksheet.Cells[1, 2].Value = "Column2";
                // ...

                // Write data
                int rowIndex = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowIndex, 1].Value = item.User.FirstName;
                    worksheet.Cells[rowIndex, 2].Value = item.User.LastName;
                    // ...

                    rowIndex++;
                }

                byte[] fileBytes = excelPackage.GetAsByteArray();

                // Save the file
                File.WriteAllBytes("download.xlsx", fileBytes);
            }
        }
    }
}
