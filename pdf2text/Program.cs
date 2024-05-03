namespace pdf2text
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reflection;
    using iText.IO;
    using iText.Pdfa;
    using iText.Kernel;
    using iText.Kernel.Pdf;
    using System.IO;
    using System.Windows.Forms;
    internal class Program
    {
        static string crlf = Encoding.UTF8.GetString(new byte[2] { (byte)13, (byte)10 });
        static string tl = Encoding.UTF8.GetString(new byte[3] { (byte)226, (byte)149, (byte)148 });
        static string tr = Encoding.UTF8.GetString(new byte[3] { (byte)226, (byte)149, (byte)151 });
        static string bl = Encoding.UTF8.GetString(new byte[3] { (byte)226, (byte)149, (byte)154 });
        static string br = Encoding.UTF8.GetString(new byte[3] { (byte)226, (byte)149, (byte)157 });
        static string si = Encoding.UTF8.GetString(new byte[3] { (byte)226, (byte)149, (byte)144 });
        static string ud = Encoding.UTF8.GetString(new byte[3] { (byte)226, (byte)149, (byte)145 });
        public static Dictionary<string, string> From(string filepath, int start_page = 1, int end_page = 0)
        {
            PdfReader pdfReader = new PdfReader(filepath);
            PdfDocument pdfDoc = new PdfDocument(pdfReader);
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (end_page == 0)
            {
                end_page = pdfDoc.GetNumberOfPages();
            }
            for (int i = start_page; i <= end_page; i++)
            {
                iText.Kernel.Pdf.Canvas.Parser.Listener.ITextExtractionStrategy strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();
                string pageContent = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                string page_num = i.ToString();
                results.Add(page_num, pageContent);
            }
            pdfDoc.Close();
            pdfReader.Close();
            return results;
        }
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

        }
        [STAThread]
        static void Main(string[] args)
        {
            string source_pdf_file = String.Empty;
            if(args.Length == 0)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                DialogResult r = ofd.ShowDialog();
                if(r == DialogResult.OK)
                {
                    source_pdf_file = ofd.FileName;
                }
            } else
            {
                source_pdf_file = args[0];
            }
            if (!File.Exists(source_pdf_file))
            {
                Console.WriteLine("File not found");
                return;
            }
            FileInfo fi = new FileInfo(source_pdf_file);
            string out_text_file = fi.Directory.FullName + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fi.FullName) + ".txt";
            int to_page = new PdfDocument(new PdfReader(source_pdf_file)).GetNumberOfPages();
            Dictionary<string, string>  pdf_ = From(source_pdf_file, 1, to_page);
            List<int> lengths = new List<int>();
            pdf_.Keys.ToList().ForEach(key =>
            {
                pdf_[key].ToString().Split((Char)10).ToList().ForEach(i =>
                {
                    lengths.Add(i.Length);
                });
            });
            int max = lengths.Max();
            if((max % 2) != 0) { max++; }
            max = max + 4;
            int half = (max - 8) / 2;
            string sixty = String.Empty;
            Enumerable.Range(1, half).ToList().ForEach(n =>
            {
                sixty += si;
            });
            string one31 = String.Empty;
            Enumerable.Range(0, max).ToList().ForEach(n =>
            {
                one31 += " ";
            });
            List<string> pages = new List<string>();
            pdf_.Keys.ToList().ForEach(key =>
            {
                int page_num = Int32.Parse(key.ToString());
                string content = pdf_[key];
                List<string> formatted = new List<string>();
                List<string> all = new List<string>();
                content.Split((Char)10).ToList().ForEach(line =>
                {
                    all.Add(line);
                });
                for(int z = 0; z < all.Count; z++)
                {
                    string line = all[z];
                    line = (ud + " " + all[z]).Replace(crlf, String.Empty);
                    int fill = max - line.Length;
                    string spaces = " ";
                    Enumerable.Range(0, fill).ToList().ForEach(space =>
                    {
                        spaces += " ";
                    });
                    string new_ = line.Replace(((Char)13).ToString(), String.Empty) + (spaces + ud).Replace(((Char)13).ToString(), String.Empty);
                    formatted.Add(new_);
                }
                string fstring = String.Join(crlf, formatted);
                string page = String.Empty;
                if(page_num.ToString().Length > 1)
                {
                    page = $"{tl}{sixty.Substring(1)} page {page_num.ToString()} {sixty}{tr}{crlf}{fstring}{crlf}{bl}{sixty.Substring(1)} page {page_num.ToString()} {sixty}{br}{crlf}{one31}";
                } else
                {
                    page = $"{tl}{sixty} page {page_num.ToString()} {sixty}{tr}{crlf}{fstring}{crlf}{bl}{sixty} page {page_num.ToString()} {sixty}{br}{crlf}{one31}";
                }
                pages.Add(page);
            });
            string results = String.Join(crlf, pages);
            File.WriteAllText(out_text_file, results);
            Console.WriteLine(String.Join(crlf, pages));
        }
    }
}
