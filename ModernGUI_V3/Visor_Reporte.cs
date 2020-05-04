using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace ModernGUI_V3
{
    public partial class Visor_Reporte : Form
    {
        public Visor_Reporte()
        {
            InitializeComponent();
        }
        static string conexion = "SERVER=127.0.0.1;PORT=3306;DATABASE=integer5;UID=root;PASSWORD=;";
        MySqlConnection cn = new MySqlConnection(conexion);
        private void Visor_Reporte_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            CrearPDF();
            axAcroPDF1.src = "Reportefechado.pdf";
        }
        private void CrearPDF()
        {
            PdfWriter pdfWriter = new PdfWriter("Reporte.pdf");
            PdfDocument pdf = new PdfDocument(pdfWriter);
            Document document = new Document(pdf, PageSize.LETTER);
            document.SetMargins(60,20,55,20);
            //var parrafo = new Paragraph("Hola mundo");
            //document.Add(parrafo);

            PdfFont fontColumnas = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont fontContenido = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            string[] columnas = {"Numero de ingreso","Usuario","Fecha y Hora"};//Id_Bitacora,Id_Usr,HORA
            float[] tamanos = { 4, 4, 4 };
            Table tabla = new Table(UnitValue.CreatePercentArray(tamanos));
            tabla.SetWidth(UnitValue.CreatePercentValue(100));
            foreach (string columna in columnas)
            {
                tabla.AddHeaderCell(new Cell().Add(new Paragraph(columna).SetFont(fontColumnas)));
            }
            string sql = "SELECT p.Id_Bitacora,p.Id_Usuario,p.HORA FROM bitacora AS p ";
            cn.Open();
            MySqlCommand comando = new MySqlCommand(sql,cn);
            MySqlDataReader reader = comando.ExecuteReader();
            while(reader.Read()){
                //for(int x = 1; x < 100; x++) { 
                tabla.AddCell(new Cell().Add(new Paragraph(reader["Id_Bitacora"].ToString()).SetFont(fontContenido)));
                tabla.AddCell(new Cell().Add(new Paragraph(reader["Id_Usuario"].ToString()).SetFont(fontContenido)));
                tabla.AddCell(new Cell().Add(new Paragraph(reader["HORA"].ToString()).SetFont(fontContenido)));
                //}
            }
            document.Add(tabla);
            document.Close();
            var logo = new iText.Layout.Element.Image(ImageDataFactory.Create("logo.png")).SetWidth(50);
            var plogo = new Paragraph("").Add(logo);
            var titulo = new Paragraph("Reporte de ingresos al sistema");
            titulo.SetTextAlignment(TextAlignment.CENTER);
            titulo.SetFontSize(14);
            var dfecha = DateTime.Now.ToString("dd-MM-yyyy");
            var dhora = DateTime.Now.ToString("hh:mm:ss");
            var fecha = new Paragraph("Fecha: "+dfecha+" Hora: "+dhora);
            fecha.SetFontSize(8);
            PdfDocument pdfDoc = new PdfDocument(new PdfReader("Reporte.pdf"),new PdfWriter("Reportefechado.pdf"));
            Document doc = new Document(pdfDoc);
            int numeros=pdfDoc.GetNumberOfPages();
            for (int i=1;i<=numeros;i++)
            {
                PdfPage pagina = pdfDoc.GetPage(i);
                float y = (pdfDoc.GetPage(i).GetPageSize().GetTop()-15);
                doc.ShowTextAligned(plogo, 40,y,i,TextAlignment.CENTER,VerticalAlignment.TOP,0);
                doc.ShowTextAligned(titulo, 150, y, i, TextAlignment.CENTER, VerticalAlignment.TOP, 0);
                doc.ShowTextAligned(fecha, 520, y, i, TextAlignment.CENTER, VerticalAlignment.TOP, 0);
                doc.ShowTextAligned(new Paragraph(String.Format("Página {0} de {1}",i,numeros)), pdfDoc.GetPage(i).GetPageSize().GetWidth()/2, pdfDoc.GetPage(i).GetPageSize().GetBottom()+30, i, TextAlignment.CENTER, VerticalAlignment.TOP, 0);
            }
            doc.Close();
        }
    }
}