using ClosedXML.Excel;

namespace MyPageOnGitHub.Code;

public static class ExcelService
{
    public static byte[] GenerateExcelWorkbook<T>(List<T> list)
    {
        using (var workbook = new XLWorkbook())
        {
            var propertyNames = typeof(T).GetProperties().Select(p => p.Name).ToList();

            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Write header row
            for (int i = 0; i < propertyNames.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = propertyNames[i];
            }

            // Write data rows
            var rowData = list;
            for (int rowIndex = 0; rowIndex < rowData.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < propertyNames.Count; colIndex++)
                {
                    var propertyName = propertyNames[colIndex];
                    var propertyValue = typeof(T).GetProperty(propertyName)?.GetValue(rowData[rowIndex])?.ToString();
                    worksheet.Cell(rowIndex + 2, colIndex + 1).Value = propertyValue;
                }
            }

            // Create Table
            worksheet.RangeUsed().CreateTable();

            // Save the workbook to a memory stream
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }

        }
    }
}
