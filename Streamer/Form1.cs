using EventDrivenCapture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Streamer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        EventDrivenCapture.CaptureEventHandler handler;
        List<EventDrivenCapture.Capture> args = new List<Capture>();
        EventDrivenCapture.Capture defaultCaptue = new EventDrivenCapture.Capture(new EventDrivenCapture.CaptureSetting(100, 100, 100, 100));
        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBoxX.Text = defaultCaptue.CaptureSetting.StartX.ToString();
            this.textBoxY.Text = defaultCaptue.CaptureSetting.StartY.ToString();
            this.textBoxW.Text = defaultCaptue.CaptureSetting.Width.ToString();
            this.textBoxH.Text = defaultCaptue.CaptureSetting.Height.ToString();
        }
        int current = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (current > 1)
                {
                    current = 0;
                    args.Clear();
                }
                var x = int.Parse(this.textBoxX.Text);
                var y = int.Parse(this.textBoxY.Text);
                var w = int.Parse(this.textBoxW.Text);
                var h = int.Parse(this.textBoxH.Text);
                var setting = new EventDrivenCapture.CaptureSetting(x, y, w, h);
                setting.ID = current;
                args.Add(new EventDrivenCapture.Capture(setting));
                
                    var bitmap = new Bitmap(w, h);
                    Graphics captureGraphics = Graphics.FromImage(bitmap);
                    captureGraphics.CopyFromScreen(x, y, 0, 0, new Size(w, h));

                switch (current)
                {
                    case 0:
                        this.pictureBox1.Image = bitmap;
                        break;
                    case 1:
                        this.pictureBox2.Image = bitmap;
                        break;
                    default:
                        break;
                }
                current++;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        bool started = false;
        private void button2_Click(object sender, EventArgs e)
        {
            if (!this.args.Any())
            {
                MessageBox.Show("Please set up a capture area first");
            }
            else
            {
                if(!started)
                {
                    if (handler == null)
                        handler = new EventDrivenCapture.CaptureEventHandler(args.ToArray());
                    handler.CapturedEventHandler += captured;
                    handler.ExceptionEventHandler += exception;
                    handler.CanceledEventHandler += canceled;
                    handler.Start();
                    started = true;
                }
            }
        }

        private void canceled(object sender, CaptureEventArgs[] e)
        {
            MessageBox.Show("job is canceled");
        }

        private void exception(object sender, CaptureEventArgs[] e)
        {
            MessageBox.Show($"expect 2 capture are, only found {e.Length}. Stop capturing...");
            handler.Stop();
            setDefault();
        }
        
        private void captured(object sender, CaptureEventArgs[] e) // this could throw exception, for testing ExceptionEventHandler
        {
            this.pictureBox1.Image = e[0]?.Capture?.CapturedImage;
            this.pictureBox2.Image = e[1]?.Capture?.CapturedImage;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            handler.Pause();
        }

        private void buttonC_Click(object sender, EventArgs e)
        {
            handler.Continue();
        }

        private void buttonS_Click(object sender, EventArgs e)
        {
            handler.Stop();
            setDefault();
        }

        private void setDefault()
        {
            current = 0;
            this.textBoxX.Text = defaultCaptue.CaptureSetting.StartX.ToString();
            this.textBoxY.Text = defaultCaptue.CaptureSetting.StartY.ToString();
            this.textBoxW.Text = defaultCaptue.CaptureSetting.Width.ToString();
            this.textBoxH.Text = defaultCaptue.CaptureSetting.Height.ToString();
            args.Clear();
            handler = null; 
            started = false;
            this.pictureBox1.Image = null;
            this.pictureBox2.Image = null;
        }
    }
    
}
