using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class CsvParser
{
    public static List<Dictionary<string, string>> ReadRows(TextAsset source)
    {
        if (source == null)
        {
            throw new Exception("CSV 파일이 연결되지 않았습니다.");
        }

        string text = source.text.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] lines = text.Split('\n');
        int headerIndex = FindNextLine(lines, 0);

        if (headerIndex < 0)
        {
            throw new FormatException("CSV에 제목 행이 없습니다.");
        }

        List<string> headers = ParseLine(lines[headerIndex]);
        if (headers.Count == 0)
        {
            throw new FormatException("CSV 제목 행이 비어 있습니다.");
        }

        List<Dictionary<string, string>> rows =
            new List<Dictionary<string, string>>();

        for (int lineIndex = headerIndex + 1; lineIndex < lines.Length; lineIndex++)
        {
            if (string.IsNullOrWhiteSpace(lines[lineIndex]))
            {
                continue;
            }

            List<string> values = ParseLine(lines[lineIndex]);
            if (values.Count != headers.Count)
            {
                throw new FormatException(
                    "CSV " + (lineIndex + 1) + "번째 줄의 열 개수가 맞지 않습니다.");
            }

            Dictionary<string, string> row =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int fieldIndex = 0; fieldIndex < headers.Count; fieldIndex++)
            {
                if (row.ContainsKey(headers[fieldIndex]))
                {
                    throw new FormatException(
                        "CSV에 같은 제목이 두 번 있습니다: " + headers[fieldIndex]);
                }

                row.Add(headers[fieldIndex], values[fieldIndex]);
            }

            rows.Add(row);
        }

        return rows;
    }

    private static int FindNextLine(string[] lines, int startIndex)
    {
        for (int index = startIndex; index < lines.Length; index++)
        {
            if (!string.IsNullOrWhiteSpace(lines[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private static List<string> ParseLine(string line)
    {
        List<string> fields = new List<string>();
        StringBuilder field = new StringBuilder();
        bool insideQuotes = false;

        for (int index = 0; index < line.Length; index++)
        {
            char current = line[index];

            if (current == '"')
            {
                if (insideQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (current == ',' && !insideQuotes)
            {
                fields.Add(field.ToString().Trim());
                field.Clear();
            }
            else
            {
                field.Append(current);
            }
        }

        if (insideQuotes)
        {
            throw new FormatException("CSV에 닫히지 않은 따옴표가 있습니다.");
        }

        fields.Add(field.ToString().Trim());
        return fields;
    }
}
