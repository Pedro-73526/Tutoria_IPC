using Azure.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Radzen;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using TutoringApp.Data;
using TutoringApp.Models;

namespace TutoringApp.Services
{
    public class ExcelService : IExcelService
    {
        private ApplicationDbContext _context;
        public ExcelService(ApplicationDbContext context)
        {
            _context = context;
        }
        public byte[] CreateTemplate()
        {
            byte[] fileContents;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                // Add content to the worksheet here, such as headers and sample data
                worksheet.Cells[1, 1].Value = "Curso";
                worksheet.Cells[1, 2].Value = "Função";
                worksheet.Cells[1, 3].Value = "Username";
                worksheet.Cells[2, 9].Value = "Certifique-se que o código do curso existe. Pode confirmar na plataforma.";
                worksheet.Cells[4, 9].Value = "Em username coloque o prefixo 'al' para alunos. Exemplo: al00000";

                // Data for Examples
                List<Tuple<int, string, string>> examples = new List<Tuple<int, string, string>>()
                {
                    new Tuple<int, string, string>(5019, "Tutor", "username"),
                    new Tuple<int, string, string>(5019, "Mentor", "al00000"),
                    new Tuple<int, string, string>(5019, "Tutorando", "al00001"),
                };
                for (int i = 0; i < examples.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = examples[i].Item1;
                    worksheet.Cells[i + 2, 2].Value = examples[i].Item2;
                    worksheet.Cells[i + 2, 3].Value = examples[i].Item3;
                }

                // Add bold to cells
                for (int i = 1; i <= 3; i++)
                {
                    worksheet.Cells[1, i].Style.Font.Bold = true;
                }
                worksheet.Cells.AutoFitColumns();
                worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                fileContents = package.GetAsByteArray();
            }
            return fileContents;
        }

        public async Task<(List<Enrollement> enrollementsToCreate, List<Enrollement> enrollementsDuplicated, List<int> nonExistentCourses)> ReadExcel(IBrowserFile file, int yearId)
        {
            // Explicar o que acontece neste metodo. O nonExistentCourses retorna a lista de cursos no excel que nao existem na db.
            List<Enrollement> enrollementsToCreate = new List<Enrollement>();
            List<Enrollement> enrollementsDuplicated = new List<Enrollement>();
            List<int> nonExistentCourses = new List<int>();
            int duplicated = 0, toCreate = 0, inserted = 0;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // copy data from file to memory stream
                    await file.OpenReadStream().CopyToAsync(ms);
                    // positions the cursor at the beginning of the memory stream
                    ms.Position = 0;

                    // create ExcelPackage from memory stream
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (ExcelPackage package = new ExcelPackage(ms))
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets.FirstOrDefault();
                        int rowCount = ws.Dimension.End.Row;
                        int countRows = Enumerable.Range(2, rowCount - 1).Count(r => ws.Cells[r, 1, r, 5].Any(c => c.Value != null));

                        // Scrolls through the lines where the data is, and adds according to the enrollment to be added
                        for (int row = 2; row <= countRows + 1; row++)
                        {
                            Enrollement enrollement = new Enrollement() { User = new ApplicationUser() };

                            int courseCode = Convert.ToInt32(ws.Cells[row, 1].Value);
                            // If courseCode exists in db:
                            if (_context.Courses.Any(c => c.CourseCode == courseCode))
                            {
                                // Put string like this example: mEntOR -> Mentor
                                string role = ws.Cells[row, 2].Value.ToString().ToLower();
                                role = role[0].ToString().ToUpper() + role.Substring(1);
                                role = role == "Tutorando" ? role : (role == "Tutor" || role == "Mentor") ? role : "Empty";

                                enrollement.Course = _context.Courses.FirstOrDefault(c => c.CourseCode == courseCode);
                                enrollement.Role = role;
                                enrollement.User.UserName = ws.Cells[row, 3].Value.ToString();
                                enrollement.YearId = yearId;

                                // If enrollment exist in database
                                if (_context.Enrollements.Any(e => e.User.UserName == enrollement.User.UserName && e.Course.CourseCode == enrollement.Course.CourseCode && e.Role == role && e.YearId == yearId || role=="Empty"))
                                {
                                    enrollementsDuplicated.Add(enrollement);
                                    duplicated++;
                                }
                                else
                                {
                                    enrollementsToCreate.Add(enrollement);
                                    toCreate++;
                                }
                                inserted++;
                            }
                            else
                            {
                                // if course not exist:
                                nonExistentCourses.Add(courseCode);
                            }
                        }
                    }
                }
                return (enrollementsToCreate, enrollementsDuplicated, nonExistentCourses);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public byte[] ExportEnrollmentsTable(IQueryable<Enrollement> enrollments)
        {
            byte[] fileContents;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                // Add content to the worksheet here, such as headers and sample data
                worksheet.Cells[1, 1].Value = "Username";
                worksheet.Cells[1, 2].Value = "Nome";
                worksheet.Cells[1, 3].Value = "Apelido";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Função";
                worksheet.Cells[1, 6].Value = "Curso";
                worksheet.Cells[1, 7].Value = "Número de grupos";

                int i = 2;
                foreach (Enrollement obj in enrollments)
                {
                    worksheet.Cells[i, 1].Value = obj.User.UserName;
                    worksheet.Cells[i, 2].Value = obj.User.FirstName;
                    worksheet.Cells[i, 3].Value = obj.User.LastName;
                    worksheet.Cells[i, 4].Value = obj.User.Email;
                    worksheet.Cells[i, 5].Value = obj.Role;
                    worksheet.Cells[i, 6].Value = obj.Course.CourseName;
                    worksheet.Cells[i, 7].Value = obj.Groups.Count;
                    i++;
                }

                // Add bold to cells
                for (int j = 1; j <= 7; j++)
                {
                    worksheet.Cells[1, j].Style.Font.Bold = true;
                }
                worksheet.Cells.AutoFitColumns();
                worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                fileContents = package.GetAsByteArray();
            }
            return fileContents;
        }

        public byte[] ExportGroupsTable(IQueryable<CustomGroup> data)
        {
            byte[] fileContents;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                // Value for merged cells
                worksheet.Cells["A1"].Value = "Tutor";
                worksheet.Cells["C1"].Value = "Mentor";
                worksheet.Cells["F1"].Value = "Tutorando";

                // Cells merge
                var mergedCells1 = worksheet.Cells["A1:B1"];
                mergedCells1.Merge = true;
                mergedCells1.Style.Fill.PatternType = ExcelFillStyle.Solid;
                mergedCells1.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSalmon);
                mergedCells1.Style.Fill.BackgroundColor.Tint = 0.7M;
                var mergedCells2 = worksheet.Cells["C1:E1"];
                mergedCells2.Merge = true;
                mergedCells2.Style.Fill.PatternType = ExcelFillStyle.Solid;
                mergedCells2.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                mergedCells2.Style.Fill.BackgroundColor.Tint = 0.7M;
                var mergedCells3 = worksheet.Cells["F1:I1"];
                mergedCells3.Merge = true;
                mergedCells3.Style.Fill.PatternType = ExcelFillStyle.Solid;
                mergedCells3.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
                mergedCells3.Style.Fill.BackgroundColor.Tint = 0.7M;

                // Add content to the worksheet here, such as headers and sample data
                worksheet.Cells[2, 1].Value = "Nome";
                worksheet.Cells[2, 2].Value = "Email";
                worksheet.Cells[2, 3].Value = "Nome";
                worksheet.Cells[2, 4].Value = "Email";
                worksheet.Cells[2, 5].Value = "Número";
                worksheet.Cells[2, 6].Value = "Nome";
                worksheet.Cells[2, 7].Value = "Email";
                worksheet.Cells[2, 8].Value = "Número";
                worksheet.Cells[2, 9].Value = "Curso";

                int i = 3;
                foreach (var obj in data)
                {
                    worksheet.Cells[i, 1].Value = obj.Tutor.User.FirstName + " " + obj.Tutor.User.LastName;
                    worksheet.Cells[i, 2].Value = obj.Tutor.User.Email;
                    if (obj.Mentor != null && obj.Mentor.User != null)
                    {
                        worksheet.Cells[i, 3].Value = obj.Mentor.User.FirstName + " " + obj.Mentor.User.LastName;
                        worksheet.Cells[i, 4].Value = obj.Mentor.User.Email;
                        worksheet.Cells[i, 5].Value = obj.Mentor.User.NumberMec;
                    }
                    else
                    {
                        worksheet.Cells[i, 3].Value = "Sem mentor";
                        worksheet.Cells[i, 4].Value = "Sem mentor";
                        worksheet.Cells[i, 5].Value = "Sem mentor";
                    }
                    worksheet.Cells[i, 6].Value = obj.Mentee.User.FirstName + " " + obj.Mentee.User.LastName;
                    worksheet.Cells[i, 7].Value = obj.Mentee.User.Email;
                    worksheet.Cells[i, 8].Value = obj.Mentee.User.NumberMec;
                    worksheet.Cells[i, 9].Value = obj.Course.CourseName;
                    i++;
                }

                // Add bold to cells
                for (int j = 1; j <= 9; j++)
                {
                    worksheet.Cells[1, j].Style.Font.Bold = true;
                    worksheet.Cells[2, j].Style.Font.Bold = true;
                }
                worksheet.Cells.AutoFitColumns();
                worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                fileContents = package.GetAsByteArray();
            }
            return fileContents;
        }
    }
}