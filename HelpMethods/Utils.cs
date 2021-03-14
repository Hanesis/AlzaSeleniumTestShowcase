﻿using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;

namespace AlzaSeleniumTest.HelpMethods
{
    public static class Utils
    {
        public static string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
        public static string GetPdfText(string path)
        {
            var reader = new PdfReader(path);
            var text = PdfTextExtractor.GetTextFromPage(reader, 1);
            reader.Close();
            return text;
        }
    }
}
