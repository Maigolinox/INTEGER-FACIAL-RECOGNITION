using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Data.OleDb;
using System.Speech.Synthesis;
using System.Media;
using System.Runtime.InteropServices;

namespace ModernGUI_V3
{
    public partial class Reconocimiento : Form
    {
        //#region Dlls para poder hacer el movimiento del Form
        //[DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        //private extern static void ReleaseCapture();

        //[DllImport("user32.DLL", EntryPoint = "SendMessage")]
        //private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        //Rectangle sizeGripRectangle;
        //bool inSizeDrag = false;
        //const int GRIP_SIZE = 15;

        //int w = 0;
        //int h = 0;
        //#endregion

        public int heigth, width;
        public string[] Labels;
        DBCon dbc = new DBCon();
        int con = 0;
        SoundPlayer media = new SoundPlayer();
        SpeechSynthesizer vos = new SpeechSynthesizer();
        //DECLARANDO TODAS LAS VARIABLES, vectores y  haarcascades
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.4d, 0.4d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, Labelsinfo, names = null;

        public Reconocimiento()
        {
            InitializeComponent();
            //heigth = this.Height; width = this.Width;
            //GARGAMOS LA DETECCION DE LAS CARAS POR  haarcascades 
            try { serialPort2.Open(); } catch (Exception msg) { MessageBox.Show(msg.ToString()); }

            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                dbc.ObtenerBytesImagen();
                //carga de caras y etiquetas para cada imagen               
                string[] Labels = dbc.Name;
                NumLabels = dbc.TotalUser;
                ContTrain = NumLabels;
                string LoadFaces;
                for (int tf = 0; tf < NumLabels; tf++)
                {
                    con = tf;
                    Bitmap bmp = new Bitmap(dbc.ConvertByteToImg(con));
                    //LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(bmp));//cargo la foto con ese nombre
                    labels.Add(Labels[tf]);//cargo el nombre que se encuentre en la posicion del tf
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e + "No hay ningun rosto registrado).", "Cargar rostros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Reconocer()
        {
            try
            {
                //Iniciar el dispositivo de captura
                grabber = new Capture();
                grabber.QueryFrame();
                //Iniciar el evento FrameGraber
                Application.Idle += new EventHandler(FrameGrabber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FrameGrabber(object sender, EventArgs e)
        {
            lblNumeroDetect.Text = "0";
            NamePersons.Add("");
            try
            {
                currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //Convertir a escala de grises
                gray = currentFrame.Convert<Gray, Byte>();

                //Detector de Rostros
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

                //Accion para cada elemento detectado
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                    //Dibujar el cuadro para el rostro
                    currentFrame.Draw(f.rect, new Bgr(Color.LightGreen), 1);

                    if (trainingImages.ToArray().Length != 0)
                    {
                        //Clase para reconocimiento con el nùmero de imagenes
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                        //Clase Eigen para reconocimiento de rostro
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), ref termCrit);
                        var fa = new Image<Gray, byte>[trainingImages.Count]; //currentFrame.Convert<Bitmap>();

                        name = recognizer.Recognize(result);
                        //Dibujar el nombre para cada rostro detectado y reconocido
                        currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.YellowGreen));
                    }

                    NamePersons[t - 1] = name;
                    NamePersons.Add("");
                    //Establecer el nùmero de rostros detectados
                    lblNumeroDetect.Text = facesDetected[0].Length.ToString();
                    lblNadie.Text = name;




                //    serialPort2.Write("3");//AL ENCENDER SE ENVIA UN 1
                    Task.Delay(2000).Wait();
                    serialPort2.Write("4");

                
















                }
                t = 0;

                //Nombres concatenados de todos los rostros reconocidos
                for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                {
                    names = names + NamePersons[nnn] + ", ";
                }

                //Mostrar los rostros procesados y reconocidos
                imageBoxFrameGrabber.Image = currentFrame;
                name = "";
                serialPort2.Write("3");
                Task.Delay(2000).Wait();
                serialPort2.Write("4");
                //Borrar la lista de nombres            
                NamePersons.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Reconocimiento_Load(object sender, EventArgs e)
        {
            //ControlBox = false;
            //#region[Metodo deredimension de formulario sin borde]

            //SetGripRectangle();
            //this.Paint += (o, ea) => { ControlPaint.DrawSizeGrip(ea.Graphics, this.BackColor, sizeGripRectangle); };

            //this.MouseUp += delegate { inSizeDrag = false; };
            //this.MouseDown += (o, ea) =>
            //{
            //    if (IsInSizeGrip(ea.Location))
            //        inSizeDrag = true;
            //};
            //this.MouseMove += (o, ea) =>
            //{
            //    if (inSizeDrag)
            //    {
            //        this.Width = ea.Location.X + GRIP_SIZE / 2;
            //        this.Height = ea.Location.Y + GRIP_SIZE / 2;
            //        SetGripRectangle();
            //        this.Invalidate();
            //    }
            //};
            //#endregion
            Reconocer();
            media.SoundLocation = "sounds/2.wav";
            media.Play();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            switch (button1.Text)
            {
                case "Conectar":
                    Reconocer();
                    button4.Text = "Desconectar";
                    break;
                case "Desconectar":
                    Desconectar();
                    break;
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Desconectar();
            Registrar r = new Registrar();
            r.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormPrincipal formprincipal = new FormPrincipal();
            formprincipal.Show();
            
            //this.Hide();
        }

        private void lblNadie_Click(object sender, EventArgs e)
        {

        }

        private void imageBoxFrameGrabber_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Desconectar();
            Registrar r = new Registrar();
            r.ShowDialog();
        }




        //private void StateWin()
        //{

        //    if (this.btn_maximize.Text == "1")
        //    {
        //        this.btn_maximize.Text = "2";
        //        this.Location = new Point(0, 0);
        //        this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
        //    }
        //    else if (this.btn_maximize.Text == "2")
        //    {
        //        this.btn_maximize.Text = "1";
        //        this.Size = new Size(width, heigth);
        //        this.StartPosition = FormStartPosition.CenterScreen;
        //    }
        //}


        private void button4_Click(object sender, EventArgs e)
        {
            switch (button1.Text)
            {
                case "Conectar":
                    Reconocer();
                    button4.Text = "Desconectar";
                    break;
                case "Desconectar":
                    Desconectar();
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Desconectar();
            Registrar r = new Registrar();
            r.ShowDialog();
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            Reconocer();
        }

        private void Desconectar()
        {
            Application.Idle -= new EventHandler(FrameGrabber);
            grabber.Dispose();
            imageBoxFrameGrabber.ImageLocation = "img/1.png";
            lblNadie.Text = string.Empty;
            lblNumeroDetect.Text = string.Empty;
            button4.Text = "Conectar";
        }
    }
}
