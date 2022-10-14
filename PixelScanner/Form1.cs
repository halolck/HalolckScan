using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HalolckScan
{
    //Update予定
    //真ん中に点で常に検出したと同じ部分を作る(クロスヘアが枠と当たりあった際に物体が2つに別れてしまう対策)
    public partial class Form1 : Form
    {
        bool showcheck = true;
        string configname = "Default";
        int Fpscount = 0;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        int w = Screen.PrimaryScreen.Bounds.Width;
        int h = Screen.PrimaryScreen.Bounds.Height;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //マウスのキャプチャを解除
                ReleaseCapture();
                //タイトルバーでマウスの左ボタンが押されたことにする
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hopeForm1_Click(object sender, EventArgs e)
        {

        }

        private void foreverButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



        #region CaptureColor
        Color Previous = Color.Black;
        Color Rear = Color.Black;
        private void slider1_LValueChanged(object sender, CustomSlider.LEventArgs e)
        {
            if (crownComboBox1.SelectedIndex == 0)
            {
                Previous = Color.FromArgb(slider1.L_Value, Previous.G, Previous.B);
                pictureBox4.BackColor = Previous;
                capima.SetPrevious(Previous);
            }
            else
            {
                Rear = Color.FromArgb(slider1.L_Value, Rear.G, Rear.B);
                pictureBox5.BackColor = Rear;
                capima.SetRear(Rear);
            }
        }
        private void slider2_LValueChanged(object sender, CustomSlider.LEventArgs e)
        {
            if (crownComboBox1.SelectedIndex == 0)
            {
                Previous = Color.FromArgb(Previous.R, slider2.L_Value, Previous.B);
                pictureBox4.BackColor = Previous; 
                capima.SetPrevious(Previous);
            }
            else
            {
                Rear = Color.FromArgb(Rear.R, slider2.L_Value, Rear.B);
                pictureBox5.BackColor = Rear;
                capima.SetRear(Rear);
            }
        }

        private void slider3_LValueChanged(object sender, CustomSlider.LEventArgs e)
        {
            if (crownComboBox1.SelectedIndex == 0)
            {
                Previous = Color.FromArgb(Previous.R, Previous.G, slider3.L_Value);
                pictureBox4.BackColor = Previous;
                capima.SetPrevious(Previous);
            }
            else
            {
                Rear = Color.FromArgb(Rear.R, Rear.G, slider3.L_Value);
                pictureBox5.BackColor = Rear;
                capima.SetRear(Rear);
            }
        }
        #endregion

        private void crownComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (crownComboBox1.SelectedIndex == 0)
            {
                pictureBox4.BackColor = Previous;
            }
            else
            {
                pictureBox5.BackColor = Rear;
            }

            slider1.L_Value = crownComboBox1.SelectedIndex == 0 ? Previous.R : Rear.R;
            slider2.L_Value = crownComboBox1.SelectedIndex == 0 ? Previous.G : Rear.G;
            slider3.L_Value = crownComboBox1.SelectedIndex == 0 ? Previous.B : Rear.B;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReadSetting();
            ControlInit();
            Thread thread = new Thread(new ThreadStart(() =>
            {
                FrameLoop();
            }));
            thread.Start();
        }
        private void FrameLoop()
        {
            while (state)
            {
                try
                {
                    bit = capima.DisplayScan();
                    if (showcheck)
                        this.Invoke(new Action(this.UpdateImage));
                    else
                        this.Invoke(new Action(this.NullImage));
                    Fpscount++;
                }
                catch
                {

                }
                
            }
                
        }

        private void UpdateImage()
        {
            if (hopeTabPage1.SelectedIndex == 2)
            {
                this.pictureBox3.Image = bit;
            }
            else
            {
                this.pictureBox2.Image = bit;
            }
            
        }
        private void NullImage()
        {
            if (hopeTabPage1.SelectedIndex == 2)
            {
                this.pictureBox3.Image = bit;
            }
            else
            {
                this.pictureBox2.Image = bit;
            }
        }

        bool state = true;
        Bitmap bit;
        CaptureImage capima = new CaptureImage();
        private void ControlInit()
        {
            crownComboBox1.SelectedIndex = 0;
            pictureBox4.BackColor = Previous;
            pictureBox5.BackColor = Rear;
            RandomGeneretor rg = new RandomGeneretor();
            SetWindowText(this.Handle, rg.GeneratePassword(12));

            //Capture Set
            
            capima.AIO(true, capmodeindex, slider4.L_Value, slider7.L_Value, slider6.L_Value, Previous, Rear, (int)(trackBarEdit1.Value * w * 0.01), (int)(trackBarEdit2.Value * h * 0.01));
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            crownComboBox1.SelectedIndex = 0;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            crownComboBox1.SelectedIndex = 1;
        }

        private void trackBarEdit1_ValueChanged()
        {
            moonLabel3.Text = trackBarEdit1.Value.ToString()+"%";
            moonLabel4.Text = trackBarEdit2.Value.ToString()+"%";
            capima.Rerect((int)(trackBarEdit1.Value*w*0.01), (int)(trackBarEdit2.Value*h*0.01));
        }

        private void hopeButton2_Click(object sender, EventArgs e)//Save
        {
            SaveSetting();
        }
        private void SaveSetting()
        {

            //capture設定をセーブ
            CaptureInfo capinfo = new CaptureInfo();
            capinfo.ShowCapture = hopeCheckBox1.Checked;
            capinfo.ScanWidth = trackBarEdit1.Value;
            capinfo.ScanHeight = trackBarEdit2.Value;
            capinfo.Previous = Previous;
            capinfo.RearColor = Rear;
            //capinfo.TopMost = hopeCheckBox3.Checked;
            capinfo.MinArea = slider4.L_Value;
            capinfo.MinVertex = slider7.L_Value;
            capinfo.MaxVertex = slider6.L_Value;
            capinfo.CaptureImageMode = capmodeindex;

            var jsonData = JsonConvert.SerializeObject(capinfo, Formatting.Indented);
            using (var sw = new StreamWriter(@"Setting", false, System.Text.Encoding.UTF8))
            {
                sw.Write(jsonData);
            }
        }

        private void ReadSetting()
        {
            if (!File.Exists(@"Setting"))
                return;
            CaptureInfo capinfo = new CaptureInfo();
            using (var sr = new StreamReader(@"Setting", System.Text.Encoding.UTF8))
            {
                // 変数 jsonData にファイルの内容を代入 
                var jsonData = sr.ReadToEnd();
                // デシリアライズして person にセット
                capinfo = JsonConvert.DeserializeObject<CaptureInfo>(jsonData);
            }
            hopeCheckBox1.Checked = capinfo.ShowCapture;
            trackBarEdit1.Value = capinfo.ScanWidth;
            trackBarEdit2.Value = capinfo.ScanHeight;
            Previous = capinfo.Previous;
            Rear = capinfo.RearColor;
            //hopeCheckBox3.Checked = capinfo.TopMost;
            slider4.L_Value = capinfo.MinArea;
            slider7.L_Value = capinfo.MinVertex;
            slider6.L_Value = capinfo.MaxVertex;
            capmodeindex = capinfo.CaptureImageMode;
            switch (capmodeindex)
            {
                case 0:
                    hopeRadioButton1.Checked = true;
                    break;
                case 1:
                    hopeRadioButton2.Checked = true;
                    break;
                        
            }
        }

        #region CaptureMode
        int capmodeindex = 0;
        private void hopeRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            capmodeindex = 0;
            capima.SetImageMode(capmodeindex);
        }

        private void hopeRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            capmodeindex = 1;
            capima.SetImageMode(capmodeindex);
        }


        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            state = false;
            capima.CapStop();
            
        }

        private void trackBarEdit3_ValueChanged()
        {
            moonLabel5.Text = trackBarEdit3.Value.ToString();
            capima.SetFov(trackBarEdit3.Value*2);
        }

        private void hopeCheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            //this.TopMost = hopeCheckBox3.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = $"HalolckScaner -Config: {configname}- [fps:{Fpscount}]";
            Fpscount = 0;
        }

        private void hopeCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            showcheck = hopeCheckBox1.Checked;
        }

        private void slider4_LValueChanged(object sender, CustomSlider.LEventArgs e)
        {
            capima.SetMinArena(slider4.L_Value);
        }


        private void slider7_LValueChanged(object sender, CustomSlider.LEventArgs e)
        {
            capima.SetMinVertex(slider7.L_Value);

        }

        private void slider6_LValueChanged(object sender, CustomSlider.LEventArgs e)
        {
            capima.SetMaxVertex(slider6.L_Value);

        }

        bool mousedown = false;
        private void parrotColorPicker1_MouseDown(object sender, MouseEventArgs e)
        {
            mousedown = true;
        }

        private void parrotColorPicker1_MouseUp(object sender, MouseEventArgs e)
        {
            mousedown = false;
        }

        private void parrotColorPicker1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousedown)
            {
                pictureBox6.BackColor = parrotColorPicker1.SelectedColor;
                slider10.L_Value = parrotColorPicker1.SelectedColor.R;
                slider9.L_Value = parrotColorPicker1.SelectedColor.G;
                slider8.L_Value = parrotColorPicker1.SelectedColor.B;
            }
        }

        private void hopeCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = hopeCheckBox2.Checked;
        }
    }

    #region beta
    public class EnDeCrpty
    {
        private const string AES_IV = @"pf69DL6GrWFyZcMK";
        private const string AES_Key = @"9Fix4L4HB4PKeKWY";
        public static string Encrypt(string text, string iv = AES_IV, string key = AES_Key)
        {

            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                rijndael.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);

                byte[] encrypted;
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream ctStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(ctStream))
                        {
                            sw.Write(text);
                        }
                        encrypted = mStream.ToArray();
                    }
                }
                return (System.Convert.ToBase64String(encrypted));
            }
        }


        public static string Decrypt(string cipher, string iv = AES_IV, string key = AES_Key)
        {
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                rijndael.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);

                string plain = string.Empty;
                using (MemoryStream mStream = new MemoryStream(System.Convert.FromBase64String(cipher)))
                {
                    using (CryptoStream ctStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(ctStream))
                        {
                            plain = sr.ReadLine();
                        }
                    }
                }
                return plain;
            }
        }
    }
    #endregion

    [JsonObject("SetttingInfo")]
    public class SetttingInfo
    {
        public CaptureInfo CapInfo { get; set; } = new CaptureInfo();
    }
    [JsonObject("CaptureInfo")]
    public class CaptureInfo
    {
        public bool ShowCapture { get; set; }
        [JsonProperty("ScanWidth")]
        public int ScanWidth { get; set; }
        [JsonProperty("ScanHeight")]
        public int ScanHeight { get; set; }
        [JsonProperty("Previous")]
        public Color Previous { get; set; }
        [JsonProperty("RearColor")]
        public Color RearColor { get; set; }
        [JsonProperty("TopMost")]
        public bool TopMost { get; set; }
        [JsonProperty("MinArea")]
        public int MinArea { get; set; }
        [JsonProperty("MinVertex")]
        public int MinVertex { get; set; }
        [JsonProperty("MaxVertex")]
        public int MaxVertex { get; set; }
        [JsonProperty("CaptureImageMode")]
        public int CaptureImageMode { get; set; }
    }
}
