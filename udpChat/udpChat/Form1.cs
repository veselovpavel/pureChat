using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace udpChat
{
    public partial class Form1 : Form
    {
        private bool initialization;

        const string ipFileName = "addr.pch";
        const string userFileName = "user.pch";
        enum Code : byte
        {
            findInformerCode = 100,
            findInformerCode_AFromInformer,
            findInformerCode_AFromWaiting,
            createIPListRequest,
            ipListRequestCode,
            ipListRequestCode_A,
            userIPRequestCode,
            userIPRequestCode_A,
            changeIPNotification,
            relayingMessage,
            firstByteInMessage
        };

        private UdpClient udpclient;
        private List<IPEndPoint> ipList;
        private IPEndPoint informer;
        private IPEndPoint answerFromWaiting;
        private Queue<Package> packagesQueue;
        private IPEndPoint userIP;
        private Package answer;

        private RSACryptoServiceProvider mainRSA;
        private bool isKeysLoaded;

        private List<string> openKeys;

        delegate void SendMsg(string Text, RichTextBox Rtb);

        SendMsg AcceptDelegate = (string Text, RichTextBox Rtb) => { Rtb.AppendText(Text + "\n"); };

        public Form1()
        {
            InitializeComponent();
            Thread InitThread = new Thread(Initialization);
            InitThread.IsBackground = true;
            InitThread.Start();
        }

        private void Initialization()
        {
            Log("начинаем инициализацию");
            initialization = false;
            ipList = new List<IPEndPoint>();
            packagesQueue = new Queue<Package>();
            mainRSA = new RSACryptoServiceProvider(2048);
            isKeysLoaded = false;
            openKeys = new List<string>();

            //ищем файл с ip-адресами
            Log("ищем файл с ip-адресами");
            if (!File.Exists(ipFileName))//если файл отсутствует, выходим из приложения
            {
                MessageBox.Show("Отсутвует файл с адресами!\nФайл с адресами \"" + ipFileName + "\"\nдолжен находиться в одной директории\nс исполняемым файлом приложения!");
                Environment.Exit(0);
            }

            //считываем адреса из файла
            Log("считываем адреса из файла");
            using (BinaryReader reader = new BinaryReader(File.Open(ipFileName, FileMode.Open)))
            {
                while (true)
                {
                    var ip = reader.ReadBytes(4);
                    var port = reader.ReadBytes(2);
                    if (port.Length != 2) break; //файл закончился
                    byte[] tmp = new byte[4];
                    Array.Copy(port, tmp, 2);
                    port = tmp;
                    ipList.Add(new IPEndPoint(new IPAddress(ip), BitConverter.ToInt32(port, 0)));
                    Log("добавили ip: " + ipList.Last().ToString());
                }
            }

            if (ipList.Count == 0)
            {
                MessageBox.Show("В файле с адресами нет ни одного адреса!\nНевозможно подключиться к сети!");
                Environment.Exit(0);
            }



            //ищем файл с пользовательскими настройками
            Log("ищем файл с пользовательскими настройками");
            using (BinaryReader reader = new BinaryReader(File.Open(userFileName, FileMode.OpenOrCreate)))
            {
                var ip = reader.ReadBytes(4);
                var port = reader.ReadBytes(2);
                if (port.Length == 2)
                {
                    byte[] tmp = new byte[4];
                    Array.Copy(port, tmp, 2);
                    port = tmp;
                    userIP = new IPEndPoint(new IPAddress(ip), BitConverter.ToInt32(port, 0));
                }
            }



            Random random = new Random();
            //ищем свободный порт и подключаемся к нему
            Log("ищем свободный порт и подключаемся к нему");
            while (true)
            {
                try
                {
                    int port = random.Next(49152, 65536);
                    udpclient = new UdpClient(port);
                    Log("подключились к порту " + port);
                    break;
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse) continue; //если порт уже занят, пробуем другой
                    else
                    {
                        MessageBox.Show("Неизвестная ошибка!\nКод ошибки: " + ex.NativeErrorCode + "\nОбратитесь к разработчику приложения.");
                        Environment.Exit(0);
                    }
                }
            }

            //запускаем слушатель и обработчик пакетов
            Log("запускаем слушатель и обработчик пакетов");
            Thread ListenThread = new Thread(Listen);
            ListenThread.IsBackground = true;
            ListenThread.Start();

            Thread PackagesHandlerThread = new Thread(PackagesHandler);
            PackagesHandlerThread.IsBackground = true;
            PackagesHandlerThread.Start();

            //переходим в ждущий режим
            WaitingMode();

            GetInformationFromIP();






            //запускаем поток пробивающий NAT
            Log("запускаем поток пробивающий NAT");
            Thread SenderThread = new Thread(Sender);
            SenderThread.IsBackground = true;
            SenderThread.Start();
        }

        private void GetInformationFromIP()//выводим список ip в лог
        {
            foreach (var ip in ipList)
            {
                Log(ip.ToString());
            }

            Log("мой ip: " + userIP.ToString());
        }

        private bool GetIPListFromInformer()
        {
            //просим информатора дать список ip
            if (informer == null) return false;
            Log("просим информатора дать список ip");
            List<IPEndPoint> tmpList = new List<IPEndPoint>();
            try
            {
                udpclient.Send(new byte[] { (byte)Code.ipListRequestCode }, 1, informer); //ipListRequestCode - код запроса списка ip
                CancellationTokenSource cts = new CancellationTokenSource();
                var task = ReceivePackageAsync(informer, (byte)Code.ipListRequestCode_A, cts.Token);
                if (!task.Wait(3000))
                {
                    cts.Cancel();
                    Log("не ответил");
                    return false;
                }
                Log("ответил, записываем ip");
                tmpList = BytesToIPList(task.Result);
                if (tmpList.Count == 0)
                {
                    Log("по неизвестной причине, список оказался пуст");
                    return false;
                }
                ipList = tmpList;
                SaveIPListToFile();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке подключиться к сети!\n" + ex.ToString());
                Environment.Exit(0);
                return false;
            }
        }

        private bool GetUserIPFromInformer()
        {
            //узнаем свой ip через информатора
            if (informer == null) return false;
            Log("узнаем свой ip через информатора");
            try
            {
                udpclient.Send(new byte[] { (byte)Code.userIPRequestCode }, 1, informer); //userIPRequestCode - код запроса на свой ip
                CancellationTokenSource cts = new CancellationTokenSource();
                var task = ReceivePackageAsync(informer, (byte)Code.userIPRequestCode_A, cts.Token);
                if (!task.Wait(3000))
                {
                    cts.Cancel();
                    Log("не ответил");
                    return false;
                }
                if (task.Result.Length != 6)
                {
                    Log("пришел пакет, но там нет ip либо он не полный");
                    return false;
                }
                if (userIP == null)
                {
                    Log("IP пользователя был неизвестен - записываем");
                    userIP = BytesToIP(task.Result);
                    //оповещаем всех о смене ip
                    byte[] buffer = new byte[7];
                    buffer[0] = (byte)Code.changeIPNotification;
                    Array.Copy(userIP.Address.GetAddressBytes(), 0, buffer, 1, 4);
                    Array.Copy(BitConverter.GetBytes(userIP.Port), 0, buffer, 5, 2);
                    List<IPEndPoint> tmpList = new List<IPEndPoint>(ipList);
                    if (userIP != null)
                    {
                        tmpList.Remove(userIP);
                    }
                    foreach (var ip in tmpList)
                    {
                        udpclient.Send(buffer, buffer.Length, ip);
                    }
                }
                else
                {
                    Log("IP пользователя был известен");
                    if (!BytesToIP(task.Result).Address.Equals(userIP.Address) || BytesToIP(task.Result).Port != userIP.Port)
                    {
                        //ищем старый ip в списке, если находим - удаляем
                        for (int i = 0; i < ipList.Count; ++i)
                        {
                            if (ipList[i].Address.Equals(userIP.Address) && ipList[i].Port == userIP.Port)
                            {
                                ipList.RemoveAt(i);
                                break;
                            }
                        }

                        //оповещаем всех о смене ip
                        byte[] buffer = new byte[7];
                        buffer[0] = (byte)Code.changeIPNotification;
                        Array.Copy(userIP.Address.GetAddressBytes(), 0, buffer, 1, 4);
                        Array.Copy(BitConverter.GetBytes(userIP.Port), 0, buffer, 5, 2);
                        List<IPEndPoint> tmpList = new List<IPEndPoint>(ipList);
                        if (userIP != null)
                        {
                            tmpList.Remove(userIP);
                        }
                        foreach (var ip in tmpList)
                        {
                            udpclient.Send(buffer, buffer.Length, ip);
                        }
                        Log("всем сообщили о новом ip");
                    }
                    userIP = BytesToIP(task.Result);
                    ipList.Add(userIP);
                    ipList = ipList.Distinct().ToList();
                }

                SaveUserInformationToFile();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке узнать свой IP!\n" + ex.ToString());
                Environment.Exit(0);
                return false;
            }
        }

        private void SaveIPListToFile()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(ipFileName)))
            {
                foreach (IPEndPoint ip in ipList)
                {
                    writer.Write(ip.Address.GetAddressBytes());
                    byte[] port = new byte[2];
                    Array.Copy(BitConverter.GetBytes(ip.Port), port, 2);
                    writer.Write(port);
                }
            }
        }

        private void SaveUserInformationToFile()
        {
            if (userIP == null) return;
            using (BinaryWriter writer = new BinaryWriter(File.Create(userFileName)))
            {
                writer.Write(userIP.Address.GetAddressBytes());
                byte[] port = new byte[2];
                Array.Copy(BitConverter.GetBytes(userIP.Port), port, 2);
                writer.Write(port);
            }
        }

        private void SendIPList(IPEndPoint address, byte code)
        {
            byte[] buffer = new byte[ipList.Count * 6 + 1];
            buffer[0] = code;
            int byteNumber = 1;
            foreach (IPEndPoint ip in ipList)
            {
                Array.Copy(ip.Address.GetAddressBytes(), 0, buffer, byteNumber, 4);
                byteNumber += 4;
                Array.Copy(BitConverter.GetBytes(ip.Port), 0, buffer, byteNumber, 2);
                byteNumber += 2;
            }
            udpclient.Send(buffer, buffer.Length, address);
        }

        private void SendUserIP(IPEndPoint address)
        {
            byte[] buffer = new byte[7];
            buffer[0] = (byte)Code.userIPRequestCode_A;
            Array.Copy(address.Address.GetAddressBytes(), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(address.Port), 0, buffer, 5, 2);
            udpclient.Send(buffer, buffer.Length, address);
        }

        private void CreateIPList(byte[] buffer)
        {
            Log("CreateIPList");
            byte[] tmp = new byte[buffer.Length - 1];
            Array.Copy(buffer, 1, tmp, 0, tmp.Length);
            buffer = tmp;
            foreach (var ip in BytesToIPList(buffer))
            {
                ipList.Add(ip);
            }
            ipList = ipList.Distinct().ToList();
            SaveIPListToFile();
            userIP = BytesToIPList(buffer).Last();
            SaveUserInformationToFile();
            initialization = true;
            label1.Text = "Подключение к сети выполнено";
        }

        private void ChangeIP(Package package)
        {
            byte[] buffer = new byte[package.buffer.Length - 1];
            Array.Copy(package.buffer, 1, buffer, 0, buffer.Length);
            IPEndPoint oldIP = BytesToIP(buffer);
            ipList.Remove(oldIP);
            ipList.Add(package.sender);
        }

        private List<IPEndPoint> BytesToIPList(byte[] bytes)
        {
            List<IPEndPoint> ipList = new List<IPEndPoint>();
            byte[] ip, port, tmp;
            int byteNumber = 0;
            while (byteNumber + 6 <= bytes.Length)
            {
                ip = new byte[4];
                port = new byte[4];
                tmp = new byte[2];
                Array.Copy(bytes, byteNumber, ip, 0, 4);
                byteNumber += 4;
                Array.Copy(bytes, byteNumber, tmp, 0, 2);
                byteNumber += 2;
                Array.Copy(tmp, port, 2);
                ipList.Add(new IPEndPoint(new IPAddress(ip), BitConverter.ToInt32(port, 0)));
            }
            return ipList;
        }

        private IPEndPoint BytesToIP(byte[] bytes)
        {
            byte[] ip = new byte[4];
            byte[] port = new byte[4];
            byte[] tmp = new byte[2];
            Array.Copy(bytes, ip, 4);
            Array.Copy(bytes, 4, tmp, 0, 2);
            Array.Copy(tmp, port, 2);
            return new IPEndPoint(new IPAddress(ip), BitConverter.ToInt32(port, 0));
        }

        private Task<byte[]> ReceivePackageAsync(IPEndPoint ip, byte code, CancellationToken token)
        {
            return Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (answer == null) continue;
                    if (!answer.sender.Address.Equals(ip.Address) || answer.sender.Port != ip.Port)
                    {
                        answer = null;
                        continue;
                    }
                    if (answer.buffer[0] != code)
                    {
                        answer = null;
                        continue;
                    }
                    byte[] buffer = new byte[answer.buffer.Length - 1];
                    Array.Copy(answer.buffer, 1, buffer, 0, buffer.Length);
                    answer = null;
                    return buffer;
                }
                return null;
            });
        }

        public void SendMessage(byte[] data)
        {
            byte[] buffer = new byte[data.Length + 1];
            buffer[0] = (byte)Code.relayingMessage;
            Array.Copy(data, 0, buffer, 1, data.Length);
            List<IPEndPoint> tmpList = new List<IPEndPoint>(ipList);
            if (userIP != null)
            {
                tmpList.Remove(userIP);
            }
            foreach (var ip in tmpList)
            {
                udpclient.Send(buffer, buffer.Length, ip);
            }
        }

        private void RelayingMessage(Package package)
        {
            try
            {
                byte[] buffer = new byte[package.buffer.Length - 1];
                Array.Copy(package.buffer, 1, buffer, 0, buffer.Length);
                List<IPEndPoint> tmpList = new List<IPEndPoint>(ipList);
                tmpList.Remove(package.sender);
                if (userIP != null)
                {
                    tmpList.Remove(userIP);
                }
                foreach (var ip in tmpList)
                {
                    udpclient.Send(buffer, buffer.Length, ip);
                }
                buffer = mainRSA.Decrypt(buffer, false);
                if (buffer[0] != (byte)Code.firstByteInMessage) return;//сообщение не нам
                byte[] data = new byte[buffer.Length - 1];
                Array.Copy(buffer, 1, data, 0, data.Length);
                string returnData = Encoding.UTF8.GetString(data);
                richTextBox1.BeginInvoke(AcceptDelegate, new object[] { "<Собеседник>: " + returnData, richTextBox1 });
            }
            catch (CryptographicException ex)
            {
                return;//если не получилось расшифровать то сообщение точно не нам
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в RelayingMessage!\n" + ex.ToString());
                Environment.Exit(0);
            }
        }

        private bool FindInformerClient()
        {
            Random random = new Random();
            List<IPEndPoint> tmpList = new List<IPEndPoint>(ipList);
            if (userIP != null)
            {
                tmpList.Remove(userIP);
            }
            foreach (IPEndPoint ip in tmpList.OrderBy(x => random.Next()).ToList())
            {
                udpclient.Send(new byte[] { (byte)Code.findInformerCode }, 1, ip); //findInformerCode - код поиска "информатора"
            }
            Log("отослали всем запрос на поиск информатора");
            bool cancelTask = false;
            var task = Task.Run(() =>
            {
                while (!cancelTask)
                {
                    if (answer == null) continue;
                    if (answer.buffer[0] == (byte)Code.findInformerCode_AFromWaiting)
                    {
                        answerFromWaiting = answer.sender;
                    }
                    if (answer.buffer[0] != (byte)Code.findInformerCode_AFromInformer)
                    {
                        answer = null;
                        continue;
                    }
                    IPEndPoint ip = answer.sender;
                    answer = null;
                    return ip;
                }
                return null;
            });
            Log("ждем ответа 5 секунд");
            if (task.Wait(5000))
            {
                informer = task.Result;
                Log("нашли информатора!");
                return true;
            }
            else
            {
                cancelTask = true;
                Log("информатор не найден");
                return false;
            }
        }

        private void Log(string data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open("log", FileMode.Append)))
            {
                writer.Write(data);
            }
        }

        public void Listen()
        {

            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                while (true)
                {
                    byte[] buffer = udpclient.Receive(ref RemoteIpEndPoint); //ожидание пакета
                    packagesQueue.Enqueue(new Package(buffer, RemoteIpEndPoint));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в потоке, принимающем сообщения!\n" + ex.ToString());
                Environment.Exit(0);
            }

        }

        private void Sender()
        {
            int iterations = 0;
            while (true)
            {
                try
                {
                    udpclient.Send(new byte[0], 0, informer);
                    ++iterations;
                    Thread.Sleep(2000);

                    if (iterations >= 5)//проверяем не изменился ли у нас ip
                    {
                        iterations = 0;
                        //если информатор не ответил - ищем нового
                        while (!GetUserIPFromInformer())
                        {
                            FindInformerClient();
                        }
                        GetInformationFromIP();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка в потоке, поддерживающем соединение!\n" + ex.ToString());
                    Environment.Exit(0);
                }
            }
        }

        private void PackagesHandler()
        {
            try
            {
                while (true)
                {
                    if (packagesQueue.Count == 0) continue;

                    Package package = packagesQueue.Dequeue();

                    if (package.buffer.Length == 0)
                    {
                        //пришел пустой пакет
                        Log("пришел пустой пакет");
                        continue;
                    }

                    if (package.buffer.Length == 1)
                    {
                        Log("пришел пакет размером 1 байт");

                        if (package.buffer[0] == (byte)Code.findInformerCode) //кто-то ищет информатора
                        {
                            Log("это запрос на информатора");
                            if (initialization)//если мы инициализированы, становимся информатором
                            {
                                Log("отправили пакет, что мы информатор");
                                udpclient.Send(new byte[] { (byte)Code.findInformerCode_AFromInformer }, 1, package.sender);
                                continue;
                            }
                            else//если мы в ждущем режиме, то вместе с приславшим клиентом формируем общую базу ip
                            {
                                Log("отправили пакет, что мы в ждущем режиме и предлагаем сформировать список");
                                udpclient.Send(new byte[] { (byte)Code.findInformerCode_AFromWaiting }, 1, package.sender);
                                continue;
                            }
                        }

                        if (package.buffer[0] == (byte)Code.findInformerCode_AFromInformer)//информатор ответил
                        {
                            Log("информатор ответил");
                            answer = new Package(package.buffer, package.sender);
                            continue;
                        }

                        if (package.buffer[0] == (byte)Code.findInformerCode_AFromWaiting)//ответил клиент, который в режиме ожидания
                        {
                            Log("ответил клиент, который в режиме ожидания");
                            answer = new Package(package.buffer, package.sender);
                            continue;
                        }

                        if (package.buffer[0] == (byte)Code.ipListRequestCode) //посылаем обратно список ip
                        {
                            if (!initialization) continue;
                            Log("это запрос на список ip");
                            SendIPList(package.sender, (byte)Code.ipListRequestCode_A);
                            continue;
                        }

                        if (package.buffer[0] == (byte)Code.userIPRequestCode) //посылаем обратно ip отправителя
                        {
                            if (!initialization) continue;
                            Log("это запрос на свой ip");
                            SendUserIP(package.sender);
                            continue;
                        }
                    }


                    //здесь обрабатываем пакеты различного объема
                    if (package.buffer[0] == (byte)Code.createIPListRequest && (package.buffer.Length - 1) % 6 == 0)//прислали список ip для формирования общего
                    {
                        if (initialization) continue;
                        Log("прислали список ip для формирования общего");
                        CreateIPList(package.buffer);
                        informer = package.sender;
                        continue;
                    }

                    if (package.buffer[0] == (byte)Code.ipListRequestCode_A && (package.buffer.Length - 1) % 6 == 0) //если это список ip
                    {
                        if (initialization) continue;
                        Log("пришел список ip");
                        answer = new Package(package.buffer, package.sender);
                        continue;
                    }
                    if (package.buffer[0] == (byte)Code.userIPRequestCode_A && package.buffer.Length == 7) //если это ip пользователя
                    {
                        Log("пришел ip пользователя");
                        answer = new Package(package.buffer, package.sender);
                        continue;
                    }
                    if (package.buffer[0] == (byte)Code.changeIPNotification && package.buffer.Length == 7) //если это оповещение о смене ip
                    {
                        if (!initialization) continue;
                        Log("пришло оповещение о смене ip");
                        ChangeIP(package);
                        continue;
                    }
                    if (package.buffer[0] == (byte)Code.relayingMessage)//нужно переслать сообщение
                    {
                        if (!initialization) continue;
                        RelayingMessage(package);
                        continue;
                    }

                    Log("пришло простое сообщение");
                    byte[] buffer;
                    try
                    {
                        buffer = mainRSA.Decrypt(package.buffer, false);
                    }
                    catch (CryptographicException ex)
                    {
                        return;//если не получилось расшифровать то сообщение точно не нам
                    }
                    if (buffer[0] != (byte)Code.firstByteInMessage) continue;//сообщение не нам
                    byte[] data = new byte[buffer.Length - 1];
                    Array.Copy(buffer, 1, data, 0, data.Length);
                    string returnData = Encoding.UTF8.GetString(data);
                    richTextBox1.BeginInvoke(AcceptDelegate, new object[] { "<Собеседник>: " + returnData, richTextBox1 });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в PackagesHandler!\n" + ex.ToString());
                Environment.Exit(0);
            }
        }

        private void WaitingMode()
        {
            Log("перешли в ждущий режим");
            while (!initialization)
            {
                if (FindInformerClient())
                {
                    Log("ответил информатор - пытаемся получить у него ip");
                    if (!GetIPListFromInformer()) continue;
                    Log("теперь пытаемся узнать свой ip");
                    if (!GetUserIPFromInformer()) continue;
                    initialization = true;
                    label1.Text = "Подключение к сети выполнено";
                    continue;
                }
                if (answerFromWaiting != null)
                {
                    Log("посылаем запрос на формирование списка ip");
                    SendIPList(answerFromWaiting, (byte)Code.createIPListRequest);
                    ipList.Remove(answerFromWaiting);
                    ipList.Add(answerFromWaiting);
                    ipList = ipList.Distinct().ToList();
                    answerFromWaiting = null;
                }
            }

            Log("вышли из ждущего режима, так как мы инициализиованы");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!initialization) return;
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите получателя!");
                return;
            }
            byte[] data = Encoding.UTF8.GetBytes(richTextBox2.Text);
            byte[] buffer = new byte[data.Length + 1];
            Array.Copy(data, 0, buffer, 1, data.Length);
            buffer[0] = (byte)Code.firstByteInMessage;
            SendMessage(MyEncrypt(openKeys[comboBox1.SelectedIndex], buffer));
            richTextBox1.AppendText("<Вы>: " + richTextBox2.Text + "\n");
            richTextBox2.Text = "";
        }

        private void создатьПаруКлючейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Assembly.GetExecutingAssembly().Location;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
                using (BinaryWriter writer = new BinaryWriter(File.Create(saveFileDialog.FileName)))
                {
                    writer.Write(rsa.ToXmlString(true));
                }
            }
        }

        private void открытьФайлСКлючамиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Assembly.GetExecutingAssembly().Location;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(openFileDialog.FileName)))
                {
                    try
                    {
                        mainRSA.FromXmlString(reader.ReadString());
                        isKeysLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при открытии файла с ключами\n" + ex.ToString());
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void получитьСвойОткрытыйКлючToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isKeysLoaded)
            {
                MessageBox.Show("Для начала откройте файл с ключами!");
                return;
            }

            XDocument xmld = XDocument.Parse(mainRSA.ToXmlString(false));
            getOpenKeyForm gokform = new getOpenKeyForm(xmld.Root.Elements().First().Value);
            gokform.ShowDialog();
        }

        private void добавитьОткрытыйКлючToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addOpenKeyForm aoform = new addOpenKeyForm();
            aoform.ShowDialog();
            if (aoform.contactName == null || aoform.openKey == null) return;
            comboBox1.Items.Add(aoform.contactName);
            openKeys.Add(aoform.openKey);
        }

        private byte[] MyEncrypt(string openKey, byte[] data)
        {
            string xmlString = string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>", openKey);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(xmlString);
            return rsa.Encrypt(data, false);
        }
    }

    class Package
    {
        public byte[] buffer;
        public IPEndPoint sender;

        public Package(byte[] buffer, IPEndPoint sender)
        {
            this.buffer = buffer;
            this.sender = sender;
        }
    }
}
