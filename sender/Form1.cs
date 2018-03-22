using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Net;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace sender
{
    public partial class Form1 : Form
    {
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
            //создаем поток для записи нашей речи
            input = new WaveIn();
            //определяем его формат - частота дискретизации 8000 Гц, ширина сэмпла - 16 бит, 1 канал - моно
            input.WaveFormat = new WaveFormat(8000, 16, 1);
            //добавляем код обработки нашего голоса, поступающего на микрофон
            input.DataAvailable += Voice_Input;
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
        //    in_thread = new Thread(new ThreadStart(Listening));
            //запускаем его
       //     in_thread.Start();
        }
        private void Voice_Input(object sender, WaveInEventArgs e)
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
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopReceive();
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
        private void Form1_Load(object sender, EventArgs e)
        {
            String host = System.Net.Dns.GetHostName();
            System.Net.IPAddress ip = System.Net.Dns.GetHostByName(host).AddressList[0];
            //textBox1.Text = ip.ToString();

            label3.Text = null;
            label3.Text = "Статус трансляции: Не начата";
            label4.Text = null;
            label4.Text = "Статус микрофона: Включен";

            stopReceive = false;
            rec = new Thread(new ThreadStart(Receive));
            rec.Start();
            /*
            rec2 = new Thread(new ThreadStart(Receive2));
            rec2.Start();*/

           // return 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
          /*  stopReceive = false;
            rec = new Thread(new ThreadStart(Receive));
            rec.Start();*/
        }


        Thread rec = null;
        UdpClient udp = new UdpClient(22);
        bool stopReceive = false;

        // Функция извлекающая пришедшие сообщения

        // работающая в отдельном потоке.

        //

        void Receive()
        {
            try
            {
                while (true)
                {

                    IPEndPoint ipendpoint = null;
                    byte[] message = udp.Receive(ref ipendpoint);
                    ShowMessage(Encoding.Default.GetString(message));

                    // Если дана команда остановить поток, останавливаем бесконечный цикл.
                    if (stopReceive == true) break;
                }  
            }
            catch(Exception e)
            {
                //MessageBox.Show(e.Message);
            }
        }
        

        // Функция безопасной остановки дополнительного потока
        void StopReceive()
        {
            stopReceive = true; 
            if(udp != null) udp.Close();
            if(rec != null) rec.Join();
        }


        // Блок кода предоставляющий безопасный доступ к членам класса из разных потоков
        delegate void ShowMessageCallback(string message);
        void ShowMessage(string message)
        {
            if (textBox3.InvokeRequired)
            {
                ShowMessageCallback dt = new ShowMessageCallback(ShowMessage);
                Invoke(dt, new object[] { message });
            }
            else
            {
                textBox3.Text = message;
            }
        }
        //


        private void button1_Click_1(object sender, EventArgs e)
        {
            label3.Text = null;
            label3.Text = "Статус трансляции: Начата";

            timer1.Enabled = true;
            /*
            // Отправка сообщения
            UdpClient udp = new UdpClient();

            // Указываем адрес отправки сообщения
            IPAddress ipaddress = IPAddress.Parse(textBox1.Text);
            IPEndPoint ipendpoint = new IPEndPoint(ipaddress, Convert.ToInt32(textBox2.Text));


            MemoryStream memoryStream = new MemoryStream();
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            printscreen.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] b = memoryStream.ToArray();//конвертирование в байты

             int sended2 = udp.Send(b, b.Length, ipendpoint);
            // После окончания попытки отправки закрываем UDP соединение,
            // и освобождаем занятые объектом UdpClient ресурсы.
            udp.Close();
            */


            /*
            TcpClient tcpClient = new TcpClient();
            try
            {
                Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(printscreen as Image);
                graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
              //  printscreen.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                tcpClient.Connect("192.168.0.108", 12001);
                Stream stream = tcpClient.GetStream();
                printscreen.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                //System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                tcpClient.Close();
            }*/
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(printscreen as Image);
                graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
                //  printscreen.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                //IPEndPoint remote_point = new IPEndPoint(IPAddress.Parse(textBox1.Text), 5555);
                tcpClient.Connect(IPAddress.Parse(textBox1.Text), 12001);
                Stream stream = tcpClient.GetStream();
                printscreen.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                //System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label3.Text = null;
            label3.Text = "Статус трансляции: Остановлена";
            timer1.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label4.Text = null;
            label4.Text = "Статус микрофона: Включен";
            input.StartRecording();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label4.Text = null;
            label4.Text = "Статус микрофона: Выключен";
            input.StopRecording();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox3.Text = null;
        }
    }
}