using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public static class EnemySOExcelImporter
{
    private const string OutputFolder = "Assets/ScriptableObject/EnemySO/Imported";
    private const string ProjectCsvPath = "Assets/CSV/Enemys.csv";

    [MenuItem("GameCamp/Import Enemy SO From Excel")]
    public static void ImportFromExcel()
    {
        string filePath = EditorUtility.OpenFilePanel("Select enemy data file", "", "xlsx,csv");
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        try
        {
            ImportFile(filePath);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Enemy SO import failed: {exception.Message}");
        }
    }

    [MenuItem("GameCamp/Import Enemy SO From Project CSV")]
    public static void ImportProjectCsv()
    {
        string filePath = Path.GetFullPath(ProjectCsvPath);
        if (File.Exists(filePath))
        {
            ImportFile(filePath);
        }
    }

    private static void ImportFile(string filePath)
    {
        try
        {
            List<List<string>> rows = Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase)
                ? ReadCsv(filePath)
                : ReadXlsx(filePath);
            ImportRows(rows);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Enemy SO import failed: {exception.Message}");
        }
    }

    private static void ImportRows(List<List<string>> rows)
    {
        if (rows.Count < 2)
        {
            throw new InvalidOperationException("The file must include a header row and at least one enemy row.");
        }

        Dictionary<string, int> headerIndexes = new();
        for (int columnIndex = 0; columnIndex < rows[0].Count; columnIndex++)
        {
            string header = NormalizeHeader(rows[0][columnIndex]);
            if (!string.IsNullOrEmpty(header) && !headerIndexes.ContainsKey(header))
            {
                headerIndexes.Add(header, columnIndex);
            }
        }

        int nameIndex = FindColumn(headerIndexes, "name", "enemyname", "이름", "적이름");
        int healthIndex = FindColumn(headerIndexes, "health", "hp", "체력");
        int damageIndex = FindColumn(headerIndexes, "damage", "attackdamage", "데미지", "공격력");
        int attackSpeedIndex = FindColumn(headerIndexes, "attackspeed", "attackspersecond", "공격속도");
        int speedIndex = FindColumn(headerIndexes, "speed", "movespeed", "이동속도", "스피드");

        EnsureFolder(OutputFolder);
        int importedCount = 0;
        for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            List<string> row = rows[rowIndex];
            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            string enemyName = GetValue(row, nameIndex);
            if (string.IsNullOrWhiteSpace(enemyName))
            {
                Debug.LogWarning($"Row {rowIndex + 1} was skipped because the enemy name is empty.");
                continue;
            }

            float health = ParseFloat(GetValue(row, healthIndex), "health", rowIndex + 1);
            float damage = ParseFloat(GetValue(row, damageIndex), "damage", rowIndex + 1);
            float attackSpeed = ParseFloat(GetValue(row, attackSpeedIndex), "attack speed", rowIndex + 1);
            float speed = ParseFloat(GetValue(row, speedIndex), "speed", rowIndex + 1);
            string assetPath = $"{OutputFolder}/{SanitizeFileName(enemyName)}.asset";
            EnemySO enemyData = AssetDatabase.LoadAssetAtPath<EnemySO>(assetPath);

            if (enemyData == null)
            {
                enemyData = ScriptableObject.CreateInstance<EnemySO>();
                AssetDatabase.CreateAsset(enemyData, assetPath);
            }

            enemyData.ConfigureStats(enemyName, health, damage, attackSpeed, speed);
            EditorUtility.SetDirty(enemyData);
            importedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Imported {importedCount} EnemySO assets into {OutputFolder}.");
    }

    private static List<List<string>> ReadCsv(string filePath)
    {
        return File.ReadAllLines(filePath, Encoding.UTF8)
            .Select(ParseCsvLine)
            .ToList();
    }

    private static List<List<string>> ReadXlsx(string filePath)
    {
        using ZipArchive archive = ZipFile.OpenRead(filePath);
        List<string> sharedStrings = ReadSharedStrings(archive);
        ZipArchiveEntry sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheetEntry == null)
        {
            throw new InvalidOperationException("The first worksheet could not be found.");
        }

        using Stream sheetStream = sheetEntry.Open();
        XDocument sheetDocument = XDocument.Load(sheetStream);
        XNamespace spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        List<List<string>> rows = new();

        foreach (XElement sheetRow in sheetDocument.Descendants(spreadsheet + "row"))
        {
            List<string> row = new();
            foreach (XElement cell in sheetRow.Elements(spreadsheet + "c"))
            {
                int columnIndex = GetColumnIndex((string)cell.Attribute("r"));
                while (row.Count <= columnIndex)
                {
                    row.Add(string.Empty);
                }

                row[columnIndex] = GetCellValue(cell, sharedStrings, spreadsheet);
            }

            rows.Add(row);
        }

        return rows;
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        ZipArchiveEntry sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (sharedStringsEntry == null)
        {
            return new List<string>();
        }

        using Stream stream = sharedStringsEntry.Open();
        XDocument document = XDocument.Load(stream);
        XNamespace spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        return document.Descendants(spreadsheet + "si")
            .Select(item => string.Concat(item.Descendants(spreadsheet + "t").Select(text => text.Value)))
            .ToList();
    }

    private static string GetCellValue(XElement cell, List<string> sharedStrings, XNamespace spreadsheet)
    {
        string cellType = (string)cell.Attribute("t");
        string value = cell.Element(spreadsheet + "v")?.Value ?? string.Empty;
        if (cellType == "s" && int.TryParse(value, out int sharedStringIndex))
        {
            return sharedStringIndex < sharedStrings.Count ? sharedStrings[sharedStringIndex] : string.Empty;
        }

        return cellType == "inlineStr"
            ? string.Concat(cell.Descendants(spreadsheet + "t").Select(text => text.Value))
            : value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        List<string> cells = new();
        StringBuilder cell = new();
        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (line[i] == ',' && !insideQuotes)
            {
                cells.Add(cell.ToString().Trim());
                cell.Clear();
            }
            else
            {
                cell.Append(line[i]);
            }
        }

        cells.Add(cell.ToString().Trim());
        return cells;
    }

    private static int FindColumn(Dictionary<string, int> headerIndexes, params string[] acceptedHeaders)
    {
        foreach (string header in acceptedHeaders)
        {
            if (headerIndexes.TryGetValue(NormalizeHeader(header), out int index))
            {
                return index;
            }
        }

        throw new InvalidOperationException($"Missing required column: {acceptedHeaders[0]}");
    }

    private static string NormalizeHeader(string header)
    {
        return header.Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
    }

    private static string GetValue(List<string> row, int index)
    {
        return index < row.Count ? row[index] : string.Empty;
    }

    private static float ParseFloat(string value, string fieldName, int rowNumber)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ||
            float.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        throw new InvalidOperationException($"Row {rowNumber} has an invalid {fieldName} value: {value}");
    }

    private static int GetColumnIndex(string cellReference)
    {
        int columnIndex = 0;
        foreach (char character in cellReference.TakeWhile(char.IsLetter))
        {
            columnIndex = columnIndex * 26 + (char.ToUpperInvariant(character) - 'A' + 1);
        }

        return columnIndex - 1;
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidCharacter, '_');
        }

        return fileName;
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] segments = folderPath.Split('/');
        string currentPath = segments[0];
        for (int i = 1; i < segments.Length; i++)
        {
            string nextPath = $"{currentPath}/{segments[i]}";
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, segments[i]);
            }

            currentPath = nextPath;
        }
    }
}
