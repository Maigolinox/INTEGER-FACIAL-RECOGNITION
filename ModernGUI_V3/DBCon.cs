using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernGUI_V3
{
    public class DBCon
    {
        private OleDbConnection conn;
        public string[] Name;
        private byte[] face;
        public List<byte[]> Face = new List<byte[]>();
        public int TotalUser;
        public DBCon()
        {
            conn = new OleDbConnection("Provider = Microsoft.Jet.OLEDB.4.0; Data Source = UsersFace.mdb");
            conn.Open();
        }
        public bool GuardarImagen(string Name, string Code, byte[] abImagen)
        {
            conn.Open();
            OleDbCommand comm = new OleDbCommand("INSERT INTO UserFaces (Name,Code,Face) VALUES ('" + Name + "','" + Code + "',?)", conn);
            OleDbParameter parImagen = new OleDbParameter("@Face", OleDbType.VarBinary, abImagen.Length);
            parImagen.Value = abImagen;
            comm.Parameters.Add(parImagen);
            int iResultado = comm.ExecuteNonQuery();
            conn.Close();
            return Convert.ToBoolean(iResultado);
        }

        public DataTable ObtenerBytesImagen()
        {
            string sql = "SELECT IdImage,Name,Code,Face FROM UserFaces";
            OleDbDataAdapter adaptador = new OleDbDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            adaptador.Fill(dt);
            int cont = dt.Rows.Count;
            Name = new string[cont];

            for (int i = 0; i < cont; i++)
            {
                Name[i] = dt.Rows[i]["Name"].ToString();
                face = (byte[])dt.Rows[i]["Face"];
                Face.Add(face);
            }
            TotalUser = dt.Rows.Count;
            conn.Close();
            return dt;
        }

        public void ConvertImgToBinary(string Name, string Code, Image Img)
        {
            Bitmap bmp = new Bitmap(Img);
            MemoryStream MyStream = new MemoryStream();
            bmp.Save(MyStream, System.Drawing.Imaging.ImageFormat.Bmp);

            byte[] abImagen = MyStream.ToArray();
            GuardarImagen(Name, Code, abImagen);
        }

        public Image ConvertByteToImg(int con)
        {
            Image FetImg;
            byte[] img = Face[con];
            MemoryStream ms = new MemoryStream(img);
            FetImg = Image.FromStream(ms);
            ms.Close();
            return FetImg;

        }
    }
}
