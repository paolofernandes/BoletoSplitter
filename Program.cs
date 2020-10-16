using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace BrasilinoAgilizator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string pdfFilePath = @"C:\Users\paolo.fernandes\Downloads\BOLETOS MENSALIDADES OUTUBRO 2020.pdf";
            string outputPath = @"C:\Users\paolo.fernandes\Downloads\BoletosDoSeuBrasilino";

            int interval = 1;
            int pageNameSuffix = 0;

            // Intialize a new PdfReader instance with the contents of the source Pdf file:
            PdfReader reader = new PdfReader(pdfFilePath);

            FileInfo file = new FileInfo(pdfFilePath);
            string pdfFileName = file.Name.Substring(0, file.Name.LastIndexOf(".")) + "-";

            Program obj = new Program();

            for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber += interval)
            {
                Console.WriteLine("DESMEMBRANDO " + pageNumber);
                pageNameSuffix++;
                string newPdfFileName = string.Format(pdfFileName + "{0}", pageNameSuffix);
                obj.SplitAndSaveInterval(pdfFilePath, outputPath, pageNumber, interval, newPdfFileName);
            }

            string[] fileEntries = Directory.GetFiles(outputPath);
            foreach (string fileName in fileEntries)
            {
                var fileInfo = new FileInfo(fileName);
                var sacado = obj.getSacado(fileName).Split(" ");

                string id = sacado[^2];
                var name = String.Join(" ", sacado[0..^2]);

                //[ID] - [NOME] - "BOLETOS" [MÊS POR EXTENSO] [ANO]
                var nomeArquivo = fileInfo.Directory.FullName + "\\" + id + " - " + name + " - BOLETOS SETEMBRO 2020.pdf";
                Console.WriteLine("Criando arquivo: " + nomeArquivo);
                try
                {
                    fileInfo.MoveTo(nomeArquivo, true);
                }
                catch
                {
                    System.Threading.Thread.Sleep(6000);
                    fileInfo.MoveTo(nomeArquivo, true);
                    continue;
                }
            }

            Console.WriteLine("TERMINADO!");
        }

        private void SplitAndSaveInterval(string pdfFilePath, string outputPath, int startPage, int interval, string pdfFileName)
        {
            using (PdfReader reader = new PdfReader(pdfFilePath))
            {
                Document document = new Document();
                PdfCopy copy = new PdfCopy(document, new FileStream(outputPath + "\\" + pdfFileName + ".pdf", FileMode.Create));
                document.Open();

                for (int pagenumber = startPage; pagenumber < (startPage + interval); pagenumber++)
                {
                    if (reader.NumberOfPages >= pagenumber)
                    {
                        copy.AddPage(copy.GetImportedPage(reader, pagenumber));
                    }
                    else
                    {
                        break;
                    }
                }

                document.Close();
            }
        }

        private string getSacado(string filePath)
        {
            PdfReader reader = new PdfReader(filePath);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);

            SimpleTextExtractionStrategy strategy = parser.ProcessContent(1, new SimpleTextExtractionStrategy());

            var text = strategy.GetResultantText();

            string[] linhas = text.Split("\n");

            bool getNome = false;
            string sacado = string.Empty;

            foreach (string linha in linhas)
            {
                if (linha.Trim() == "(-) Descontos / Abatimentos")
                {
                    getNome = true;
                }
                else if (getNome)
                {
                    sacado = linha;
                    getNome = false;
                    break;
                }
            }

            reader.Close();

            return sacado;
        }
    }
}