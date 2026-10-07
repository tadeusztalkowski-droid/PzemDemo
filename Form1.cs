using System;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using Pzem;
using System.Management;
using System.Threading;

namespace PzemDemo
{

    public partial class Form1 : Form
    {

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////  * * * * * * * * * * * * * * * * * * ZMIENNE GLOBAL * * * * * * * * * * * * * * * * * *  /////////////////// 667596linea
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        string usedprinter;
                
        string geturl;
      
        string indeks;
 
        bool admin_flag         = false;
        bool indeks_flag        = false;

        bool serial_number_flag = false;
        bool resultflagRCR      = false;
        bool resultflagAW       = false;
        bool resutlflagDIM      = false;
        bool resultflagDALI     = false;
        bool resultflagMOC      = false;
        bool resultflagTOTAL    = false;
        bool resultflagKable = false;

        bool indeks_6_digit     = false;
        bool indeksHV           = false;
        bool anyindeks          = false;
        bool pobierz_dane_flag  = false;

        bool serial_port_flag   = false;
        bool serial_port_flag2  = false;
        bool qrcode_flag        = false;
        bool linea_s_eco_flag   = false;

        bool brak_indeksu_ukladu = false;
        bool brak_numeru_zlecenia = false;

        string[] dbdata;
        string user_for_serial;
        string serial_raw;
        string errordatabase;

        volatile string global_string;
        volatile string global_CCT_COM;
        const int ReadTimeoutMilliseconds = 60000;
        Thread t;

        SerialPort serialPort2 = new SerialPort();

        bool CCT_flag = false;

        //////////////Dopuszczalne nazwy urządzeń COM <-> USB ////////////////
        string nazwaCOM1 = "Prolific USB-to-Serial Comm Port";           /////
        string nazwaCOM2 = "Silicon Labs CP210x USB to UART Bridge";     /////
        string nazwaCOM3 = "USB Serial Port";                            /////
        string nazwaCOM_KLS = "Bluetooth";                               /////
        string nazwaCOM_kable = "CH340";                                 /////
        //////////////////////////////////////////////////////////////////////

        PZEM004TV30 pzem;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        string search_COM_port()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
            string[] tempp;
            string[] tempp2;
            string COM_result = null;

            foreach (ManagementObject result in searcher.Get())
            {
                comboBox3.Items.Add(result["Caption"].ToString());                

                if (result["Caption"].ToString().Contains(nazwaCOM1) || result["Caption"].ToString().Contains(nazwaCOM2) || result["Caption"].ToString().Contains(nazwaCOM3))
                {
                    string nazwa_wybranego_portu_COM = result["Caption"].ToString();
                    tempp = nazwa_wybranego_portu_COM.Split('(');
                    tempp2 = tempp[1].Split(')');
                    serial_port_flag = true;
                    comboBox3.SelectedItem = nazwa_wybranego_portu_COM;
                    COM_result = tempp2[0];
                }
            }
            return COM_result;
        }

        
        void set_COM_port(string COM_NR)
        {
            if (serial_port_flag)
            {            
                //MessageBox.Show("Podłączono urządzenie pomiarowe na porcie: " + COM_NR, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                serialPort1.PortName = COM_NR;
                pzem = new PZEM004TV30(serialPort1);
                pzem.setAddress(0x01);
                pzem.OnUpdate += Pzem_OnUpdate;
            }
            else MessageBox.Show("Brak podłączonego urządzenia USB", "UWAGA !", MessageBoxButtons.OK, MessageBoxIcon.Error);

 
        }

        string search_COM_port_kable()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
            string[] tempp;
            string[] tempp2;
            string COM_result = null;

            foreach (ManagementObject result in searcher.Get())
            {
                comboBox8.Items.Add(result["Caption"].ToString());

                if (result["Caption"].ToString().Contains(nazwaCOM_kable))// || result["Caption"].ToString().Contains(nazwaCOM2) || result["Caption"].ToString().Contains(nazwaCOM3))
                {
                    string nazwa_wybranego_portu_COM = result["Caption"].ToString();
                    tempp = nazwa_wybranego_portu_COM.Split('(');
                    tempp2 = tempp[1].Split(')');
                    serial_port_flag2 = true;
                    comboBox8.SelectedItem = nazwa_wybranego_portu_COM;
                    COM_result = tempp2[0];
                }
            }
            return COM_result;
        }

        void set_COM_port_kable(string COM_NR)
        {
            if (serial_port_flag2)
            {
                //MessageBox.Show("Podłączono urządzenie pomiarowe na porcie: " + COM_NR, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                serialPort3.Close();
                serialPort3.BaudRate = 9600;
                serialPort3.PortName = COM_NR;
                serialPort3.Open();
            }
            else MessageBox.Show("Brak podłączonego urządzenia USB kable", "UWAGA !", MessageBoxButtons.OK, MessageBoxIcon.Error);


        }
        private void ReadThread(object context)
        {
            SerialPort serialPort = context as SerialPort;

            while (serialPort.IsOpen)
            {
                string inData = serialPort.ReadLine();               
                global_string = inData;               
            }
            
        }

        void start_serial_CCT(string COMname)
        {
            serialPort2.Close();
            serialPort2.BaudRate = 11520;
            serialPort2.PortName = COMname;
            serialPort2.Open();
            t = new Thread(ReadThread);
            t.Start(serialPort2);
        }

        void list_init()
        {
            for (int i = 1; i <= 10; i++) comboBox4.Items.Add(i);
            comboBox4.SelectedItem = 4;

            for (int i = 1; i <= 10; i++) comboBox5.Items.Add(i);
            comboBox5.SelectedItem = 1;

            for (int i = 1; i <= 50; i++) comboBox6.Items.Add(i);
            comboBox6.SelectedItem = 1;

            for (int i = 1; i <= 9; i++) comboBox7.Items.Add(i);
            comboBox7.SelectedItem = 2;

        }

        public string OdczytajDaneZPortuZTimeoutem()
        {
            if (serialPort3 == null || !serialPort3.IsOpen)
            {
                MessageBox.Show("Port szeregowy testera kabli nie jest dostępny.");
                return "FAIL";
            }

            // Zapisz aktualny ReadTimeout, aby przywrócić go później
            int originalReadTimeout = serialPort3.ReadTimeout;
            // Ustaw ReadTimeout na 1 minutę dla tej operacji
            serialPort3.ReadTimeout = ReadTimeoutMilliseconds;

            string receivedData = null;
            try
            {
                MessageBox.Show($"WYKRYTO KONIECZNOŚĆ TESTU CIĄGŁOŚCI KABLI ! \n\n Lampa{txtNazwa.Text} \n\nWykorzystaj tester ciągłości kabli do przeprowadzenia testu \n\nOczekiwanie na dane z portu przez maksymalnie {ReadTimeoutMilliseconds / 1000} sekund... \n\n Wykonaj test i kliknij 'OK'","TEST CIĄGŁOŚCI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Ta linia będzie blokować wykonanie programu do momentu otrzymania danych
                // lub upłynięcia ReadTimeout.
                receivedData = serialPort3.ReadExisting();
                if(receivedData.Contains("PASS")) MessageBox.Show($"Wynik testu : {receivedData}", "Wynik Testu ciągłości kabli", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else MessageBox.Show($"Wynik testu : FAIL", "Wynik Testu ciągłości kabli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (TimeoutException)
            {
                MessageBox.Show($"Upłynął czas oczekiwania ({ReadTimeoutMilliseconds / 1000}s) na dane z portu.", "Wynik Testu ciągłości kabli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                receivedData = "TIMEOUT"; // Możesz zwrócić specjalny string lub null
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas odczytu danych z portu: {ex.Message}", "Wynik Testu ciągłości kabli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                receivedData = "ERROR"; // Możesz zwrócić specjalny string lub null
            }
            finally
            {
                // Przywróć oryginalny ReadTimeout
                serialPort3.ReadTimeout = originalReadTimeout;
            }
            return receivedData;
        }


        public Form1()
        {
            InitializeComponent();
            
            Form2_Method();            

            PrintDocument prtdoc = new PrintDocument();
            string strDefaultPrinter = prtdoc.PrinterSettings.PrinterName;

            foreach (String strPrinter in PrinterSettings.InstalledPrinters)
            {
                comboBox1.Items.Add(strPrinter);
                if (strPrinter == strDefaultPrinter)
                {
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(strPrinter);
                }
            }

            list_init();
            set_COM_port(search_COM_port());

            set_COM_port_kable(search_COM_port_kable());

            if (CCT_flag) start_serial_CCT(global_CCT_COM);
            else MessageBox.Show("Nie podłączono miernika CCT");
           

            if (admin_flag)
            {
                comboBox3.Enabled   = true;

                textBox5.Enabled    = true;
                button5.Enabled     = true;

                textBox6.Enabled    = true;
                button6.Enabled     = true;

                label79.Enabled     = true;
                label80.Enabled     = true;

                comboBox4.Enabled   = true;
                comboBox5.Enabled   = true;

                label81.Visible     = false;
                label82.Visible     = false;
            }
            else
            {
                comboBox3.Enabled   = false;

                textBox5.Enabled    = false;
                button5.Enabled     = false;            

                textBox6.Enabled    = false;
                button6.Enabled     = false;

                label79.Enabled     = false;
                label80.Enabled     = false;

                comboBox4.Enabled   = false;
                comboBox5.Enabled   = false;

                label81.Visible     = true;
                label82.Visible     = true;
            }



            comboBox2.Items.Add("Baza danych Lena Lighting");
            comboBox2.Items.Add("Baza danych test");
            comboBox2.SelectedIndex     = 0;
            checkBox2.Checked           = true;
            checkBox9.Checked           = true;

        }

        private void Pzem_OnUpdate(object sender, PzemEvent e)
        {
            voltage_label.Text  = e.Values.Voltage      + " V";
            freq_label.Text     = e.Values.Frequency    + " Hz";
            power_label.Text    = e.Values.Power        + " W";
            PF_label.Text       = e.Values.PF           + "";
            
            textBox3.Text = global_string;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void CLICK_Click(object sender, EventArgs e)
        {

            if(serial_port_flag) pzem.resetPZ(serialPort1);
            string input = textBoxWpiszIndeks.Text;
            string nr_zamowienia = textBox1.Text;
            string indeks_ukladu = textBox2.Text;

         

            if (anyindeks)
            {
                indeks              = input;
                pobierz_dane_flag   = true;
            }
            else if (indeks_6_digit)
            {
                if (input.Length == 6)
                {
                    if ((input.Substring(0, 1) == "0")) indeks = input.Substring(1, 5);
                    else indeks = input;

                    if (indeksHV) indeks = indeks + "/HV";

                    pobierz_dane_flag = true;
                }
                else
                {
                    MessageBox.Show("Niepoprawna długość kodu", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    pobierz_dane_flag = false;
                }
            }
            else
            {
                if (input.Length == 13)
                {
                    if (input.Substring(0, 7) == "5905963")
                    {
                        if ((input.Substring(7, 6)).Substring(0, 1) == "0") indeks = input.Substring(8, 5);
                        else indeks = input.Substring(7, 6);

                        if (indeksHV) indeks = indeks + "/HV";

                        pobierz_dane_flag = true;
                    }
                    else
                    {
                        MessageBox.Show("To nie kod z Lena Lighting", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        pobierz_dane_flag = false;
                    }
                }
                else
                {
                    MessageBox.Show("Niepoprawna długość kodu", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    pobierz_dane_flag = false;
                }
            }

            if (pobierz_dane_flag)
            {
                pobierz_dane_flag = false;
                string wybrana_baza = (string)comboBox2.SelectedItem ;
                if (wybrana_baza == "Baza danych Lena Lighting") geturl = "http://192.168.0.14/qtester/dane";
                else if (wybrana_baza == "Baza danych test") geturl = "http://localhost:8080/dane";

                string wsad = geturl + "/parametry_indeks_numer_zlecenia?indeks="   + indeks; 
                if (brak_indeksu_ukladu == false) wsad = wsad + "&indeksUkladu="    + indeks_ukladu;
                if (brak_numeru_zlecenia == false) wsad = wsad + "&numerZlecenia="  + nr_zamowienia;


                //MessageBox.Show(wsad);

                string content = webGetMethod(wsad);                
                //MessageBox.Show(content);

                int found = content.IndexOf(",");

                if (found >= 0) errordatabase = content.Substring(found + 1, 2);
                //MessageBox.Show(errordatabase);

                if (errordatabase == "e1") MessageBox.Show("Brak Indeksu w bazie danych", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (errordatabase == "e2") MessageBox.Show("Błąd bazy danych", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (errordatabase == "e3") MessageBox.Show("Upłynął czas komunikacji z bazą danych", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (errordatabase == "e4") MessageBox.Show("Problem z połączeniem z bazą danych", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (errordatabase == "e5") MessageBox.Show("Błędny numer układu", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (errordatabase == "e6") MessageBox.Show("Błędny numer zlecenia", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    dbdata = content.Split(',');

                    txtIndeks.Text  = dbdata[0];
                    txtNazwa.Text   = dbdata[1];
                    txtRCR.Text     = dbdata[2];
                    txtAW.Text      = dbdata[3];
                    txtDIM.Text     = dbdata[4];
                    txtDALI.Text    = dbdata[5];
                    txtMOC.Text     = dbdata[6];// + " W";
                    txtTOL.Text     = dbdata[7];// + " %";

                    if (dbdata[2] == "1")
                    {
                        txtRCR.Text = "tak";
                        txtRCR.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        txtRCR.Text = "nie";
                        txtRCR.ForeColor = Color.Salmon;
                    }


                    if (dbdata[3] == "1")
                    {
                        txtAW.Text = "tak";
                        txtAW.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        txtAW.Text = "nie";
                        txtAW.ForeColor = Color.Salmon;
                    }


                    if (dbdata[4] == "1")
                    {
                        txtDIM.Text = "tak";
                        txtDIM.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        txtDIM.Text = "nie";
                        txtDIM.ForeColor = Color.Salmon;
                    }


                    if (dbdata[5] == "1")
                    {
                        txtDALI.Text = "tak";
                        txtDALI.ForeColor = Color.DarkGreen;

                    }
                    else
                    {
                        txtDALI.Text = "nie";
                        txtDALI.ForeColor = Color.Salmon;
                    }
                    txtNrzam.Text = nr_zamowienia;
                    txtIndeksukladu.Text = indeks_ukladu;


                    indeks_flag = true;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (anyindeks == true)
            {
                if (textBoxWpiszIndeks.Text.Length == 0) textBoxWpiszIndeks.BackColor = Color.WhiteSmoke;
                else textBoxWpiszIndeks.BackColor = Color.PaleGreen;             
            }
            else if (indeks_6_digit == false)
            {
                if (textBoxWpiszIndeks.Text.Length == 0) textBoxWpiszIndeks.BackColor = Color.WhiteSmoke;
                else if (textBoxWpiszIndeks.Text.Length == 13) textBoxWpiszIndeks.BackColor = Color.PaleGreen;
                else textBoxWpiszIndeks.BackColor = Color.Salmon;
            }
            else if (indeks_6_digit == true)
            {
                if (textBoxWpiszIndeks.Text.Length == 0) textBoxWpiszIndeks.BackColor = Color.WhiteSmoke;
                else if (textBoxWpiszIndeks.Text.Length == 6) textBoxWpiszIndeks.BackColor = Color.PaleGreen;
                else textBoxWpiszIndeks.BackColor = Color.Salmon;
            }
        }    
        
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        public string webPostMethod(string postData, string URL)
        {
            string responseFromServer = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)request).UserAgent =
                              "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
            request.Accept = "/";
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }

        public string webGetMethod(string URL)
        {
            try
            {
                string jsonString = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Timeout = 3000;
                request.Method = "GET";
                request.Credentials = CredentialCache.DefaultCredentials;
                ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
                request.Accept = "text/plain";
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                request.ContentType = "application/x-www-form-urlencoded";

                WebResponse response = request.GetResponse();

                StreamReader sr = new StreamReader(response.GetResponseStream());
                jsonString = sr.ReadToEnd();
                sr.Close();
                return jsonString;
                
            }
            catch (WebException exceptionStatus)
            {
                //if (exceptionStatus.Status == WebExceptionStatus.Timeout) return ",e3";
                //else return ",e4";
                return exceptionStatus.Message;
            }
        }

        
        public class RawPrinterHelper
        {
            // Structure and API declarions:
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public class DOCINFOA
            {
                [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
                [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
                [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
            }
            [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

            [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool ClosePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

            [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool EndDocPrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool StartPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool EndPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

            // SendBytesToPrinter()
            // When the function is given a printer name and an unmanaged array
            // of bytes, the function sends those bytes to the print queue.
            // Returns true on success, false on failure.
            public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
            {
                Int32 dwError = 0, dwWritten = 0;
                IntPtr hPrinter = new IntPtr(0);
                DOCINFOA di = new DOCINFOA();
                bool bSuccess = false; // Assume failure unless you specifically succeed.
                di.pDocName = "My C#.NET RAW Document";
                di.pDataType = "RAW";

                // Open the printer.
                if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    // Start a document.
                    if (StartDocPrinter(hPrinter, 1, di))
                    {
                        // Start a page.
                        if (StartPagePrinter(hPrinter))
                        {
                            // Write your bytes.
                            bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                            EndPagePrinter(hPrinter);
                        }
                        EndDocPrinter(hPrinter);
                    }
                    ClosePrinter(hPrinter);
                }
                // If you did not succeed, GetLastError may give more information
                // about why not.
                if (bSuccess == false)
                {
                    dwError = Marshal.GetLastWin32Error();
                }
                return bSuccess;
            }

            public static bool SendFileToPrinter(string szPrinterName, string szFileName)
            {
                // Open the file.
                FileStream fs = new FileStream(szFileName, FileMode.Open);
                // Create a BinaryReader on the file.
                BinaryReader br = new BinaryReader(fs);
                // Dim an array of bytes big enough to hold the file's contents.
                Byte[] bytes = new Byte[fs.Length];
                bool bSuccess = false;
                // Your unmanaged pointer.
                IntPtr pUnmanagedBytes = new IntPtr(0);
                int nLength;

                nLength = Convert.ToInt32(fs.Length);
                // Read the contents of the file into the array.
                bytes = br.ReadBytes(nLength);
                // Allocate some unmanaged memory for those bytes.
                pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                // Copy the managed byte array into the unmanaged array.
                Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
                // Send the unmanaged bytes to the printer.
                bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
                // Free the unmanaged memory that you allocated earlier.
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
                return bSuccess;
            }

            public static bool SendStringToPrinter(string szPrinterName, string szString)
            {
                IntPtr pBytes;
                Int32 dwCount;

                // How many characters are in the string?
                // Fix from Nicholas Piasecki:
                // dwCount = szString.Length;
                dwCount = (szString.Length + 1) * Marshal.SystemMaxDBCSCharSize;

                // Assume that the printer is expecting ANSI text, and then convert
                // the string to ANSI text.
                pBytes = Marshal.StringToCoTaskMemAnsi(szString);
                // Send the converted ANSI string to the printer.
                SendBytesToPrinter(szPrinterName, pBytes, dwCount);
                Marshal.FreeCoTaskMem(pBytes);
                return true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            usedprinter = (string)comboBox1.SelectedItem;
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
            t.Abort();           
        }
        public void Form2_Method()
        {
            string[] tempp;            
            tempp = DataContainer.ValueToShare.Split(',');

            label12.Text = tempp[0];
            if ((label12.Text == "admin") || (label12.Text == "ADMIN")) admin_flag = true;
            
            label60.Text = tempp[1];
            global_CCT_COM = tempp[1];

            if (global_CCT_COM == "brak")
            {
                CCT_flag = false;
                label60.ForeColor = Color.Red;
            }
            else
            {
                CCT_flag = true;
                label60.ForeColor = Color.Green;
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
          
            if (serial_port_flag)
            {
                pzem.resetPZ(serialPort1);
            
                if (!indeks_flag) MessageBox.Show("Najpierw wprowadź indeks !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    resultflagRCR   = false;
                    resultflagAW    = false;
                    resutlflagDIM   = false;
                    resultflagDALI  = false;
                    resultflagMOC   = false;
                    resultflagTOTAL = false;

                    if (txtRCR.Text == "tak")
                    {
                        string messageRCR = "Czy RCR działa poprawnie ?";
                        string captionRCR = "RCR";
                        if ((MessageBox.Show(messageRCR, captionRCR, MessageBoxButtons.YesNo, MessageBoxIcon.Question)).ToString() == "No") resultflagRCR = true;
                        else resultflagRCR = false;
                    }
                    if (txtAW.Text == "tak")
                    {
                        string messageAW = "Czy AW działa poprawnie ?";
                        string captionAW = "AW";
                        if ((MessageBox.Show(messageAW, captionAW, MessageBoxButtons.YesNo, MessageBoxIcon.Question)).ToString() == "No") resultflagAW = true;
                        else resultflagAW = false;
                    }
                    if (txtDIM.Text == "tak")
                    {
                        string messageDIM = "Czy DIM działa poprawnie ?";
                        string captionDIM = "DIM";
                        if ((MessageBox.Show(messageDIM, captionDIM, MessageBoxButtons.YesNo, MessageBoxIcon.Question)).ToString() == "No") resutlflagDIM = true;
                        else resutlflagDIM = false;
                    }
                    if (txtDALI.Text == "tak")
                    {
                        string messageDALI = "Czy DALI działa poprawnie ?";
                        string captionDALI = "DALI";
                        if ((MessageBox.Show(messageDALI, captionDALI, MessageBoxButtons.YesNo, MessageBoxIcon.Question)).ToString() == "No") resultflagDALI = true;
                        else resultflagDALI = false;
                    }


                    if (dbdata[1].Contains("LINEA S LED ECO") || dbdata[1].Contains("linea s led eco") || dbdata[1].Contains("linea s led single") || dbdata[1].Contains("LINEA S LED SINGLE"))
                    {
                        // string messageRCR = "Czy została sprwadzona prawidłowa kolejność wiązek kablowych ?";
                        // string captionRCR = "LINEA S LED ECO";
                        // if ((MessageBox.Show(messageRCR, captionRCR, MessageBoxButtons.YesNo, MessageBoxIcon.Question)).ToString() == "No") linea_s_eco_flag = true;
                        // else linea_s_eco_flag = false;

                        /*string messageLinea = "Proszę wykonać test ciągłości przewodów w ciągu 60 sekund";
                        string captionLinea = "LINEA S LED ECO";

                        using (Form3 timedForm = new Form3(messageLinea, captionLinea, this))
                        {
                            timedForm.ShowDialog();

                            if (timedForm.UserResult == DialogResult.No)
                            {
                                linea_s_eco_flag = true;
                            }
                            else
                            {
                                linea_s_eco_flag = false;
                            }
                        }       */


                        if (OdczytajDaneZPortuZTimeoutem().Contains("PASS")) resultflagKable = false;
                        else resultflagKable = true;
                    }


                    float tol_int;
                    float.TryParse(txtTOL.Text, out tol_int);

                    float moc_int;
                    float.TryParse(txtMOC.Text, out moc_int);

                    float temp      = moc_int * (tol_int / 100);

                    float moc_up    = moc_int + temp;
                    float moc_down  = moc_int - temp;

                    MessageBox.Show("Upewnij się, że lampa jest włączona na 100%","Rozpoczęcie testu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MessageBox.Show("Zmierz temperaturę barwową","CCT", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    float current_power     = pzem.Values.Power;
                    float current_voltage   = pzem.Values.Voltage;
                    float current_frequency = pzem.Values.Frequency;
                    float current_PF        = pzem.Values.PF;
                    float current_current   = pzem.Values.Current;

                    if (current_power >= moc_down && current_power <= moc_up) resultflagMOC = false;
                    else resultflagMOC = true;

                    string final_message = null;

                    if (resultflagRCR || resultflagAW || resutlflagDIM || resultflagDALI || resultflagMOC || resultflagKable)
                    {
                        resultflagTOTAL = true;
                        final_message = "FAIL";
                        if (resultflagRCR)  final_message += " RCR";
                        if (resultflagAW)   final_message += " AW";
                        if (resutlflagDIM)  final_message += " DIM";
                        if (resultflagDALI) final_message += " DALI";
                        if (resultflagMOC)  final_message += " MOC";
                        if (resultflagKable) final_message += " KABLE LINEA";
                    }
                    else
                    {
                        resultflagTOTAL = false;
                        final_message   = "PASS";
                    }
                    string moc_info = "";
                    if (resultflagMOC) moc_info = " " + current_power.ToString() + " W";

                    if (resultflagTOTAL) MessageBox.Show(final_message + moc_info, "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else MessageBox.Show(final_message, "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                    if (label12.Text == "ADMIN" || label12.Text == "admin") user_for_serial = "_AD";
                    else if (label12.Text == "KJ_01" || label12.Text == "kj_01") user_for_serial = "_01";
                    else if (label12.Text == "KJ_02" || label12.Text == "kj_02") user_for_serial = "_02";
                    else if (label12.Text == "KJ_03" || label12.Text == "kj_03") user_for_serial = "_03";
                    else if (label12.Text == "KJ_04" || label12.Text == "kj_04") user_for_serial = "_04";
                    else if (label12.Text == "KJ_05" || label12.Text == "kj_05") user_for_serial = "_05";
                    else if (label12.Text == "KJ_06" || label12.Text == "kj_06") user_for_serial = "_06";
                    else if (label12.Text == "KJ_07" || label12.Text == "kj_07") user_for_serial = "_07";
                    else if (label12.Text == "KJ_08" || label12.Text == "kj_08") user_for_serial = "_08";
                    else if (label12.Text == "KJ_09" || label12.Text == "kj_09") user_for_serial = "_09";
                    else if (label12.Text == "KJ_10" || label12.Text == "kj_10") user_for_serial = "_10";
                    else if (label12.Text == "KJ_11" || label12.Text == "kj_11") user_for_serial = "_11";
                    else if (label12.Text == "KJ_12" || label12.Text == "kj_12") user_for_serial = "_12";
                    else if (label12.Text == "KJ_13" || label12.Text == "kj_13") user_for_serial = "_13";
                    else if (label12.Text == "KJ_14" || label12.Text == "kj_14") user_for_serial = "_14";
                    else if (label12.Text == "KJ_15" || label12.Text == "kj_15") user_for_serial = "_15";
                    else if (label12.Text == "KJ_16" || label12.Text == "kj_16") user_for_serial = "_16";
                    else if (label12.Text == "KJ_17" || label12.Text == "kj_17") user_for_serial = "_17";
                    else if (label12.Text == "KJ_18" || label12.Text == "kj_18") user_for_serial = "_18";
                    else if (label12.Text == "KJ_19" || label12.Text == "kj_19") user_for_serial = "_19";
                    else if (label12.Text == "KJ_20" || label12.Text == "kj_20") user_for_serial = "_20";
                    else if (label12.Text == "KJ_21" || label12.Text == "kj_21") user_for_serial = "_21";
                    else if (label12.Text == "KJ_22" || label12.Text == "kj_22") user_for_serial = "_22";
                    else if (label12.Text == "KJ_23" || label12.Text == "kj_23") user_for_serial = "_23";
                    else if (label12.Text == "KJ_24" || label12.Text == "kj_24") user_for_serial = "_24";
                    else if (label12.Text == "KJ_25" || label12.Text == "kj_25") user_for_serial = "_25";

                
                    
                    string api              = "api_key=tPmAT5Ab3j7F9&";
                    string data             = "data=" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "&";
                    string godzina          = "godzina=" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "&";
                    string indeks           = "indeks=" + dbdata[0] + "&";
                    string nazwa_indeksu    = "nazwa_indeksu=" + dbdata[1] + "&";
                    ////
                    serial_raw = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                    ////
                    string numer_seryjny    = "numer_seryjny=" + serial_raw + user_for_serial + "&";
                    string napiecie         = "napiecie=" + current_voltage + "&";
                    string prad             = "prad=" + current_current + "&";
                    string moc              = "moc=" + current_power + "&";
                    string pf               = "pf=" + current_PF + "&";
                    string wynik            = "wynik=" + final_message + "&";
                    string symbol_testera   = "symbol_testera=TESTER_KJ&";
                    string uzytkownik       = "nr_uzytkownika=" + label12.Text + "&";
                    string numer_zlecenia   = "numer_zlecenia=" + txtNrzam.Text + "&";
                    string indeks_ukladu    = "indeks_ukladu=" + textBox2.Text + "&";
                    string CCT              = "cct=" + global_string + "&";
                    string Waga_netto       = "waga_netto=" + textBox4.Text;

                    string wybrana_baza2    = (string)comboBox2.SelectedItem;

                    if (wybrana_baza2 == "Baza danych Lena Lighting") geturl = "http://192.168.0.14/qtester/dane";
                    else if (wybrana_baza2 == "Baza danych test") geturl = "http://localhost:8080/dane";

                    string datatosend       = api + data + godzina + indeks + nazwa_indeksu + numer_seryjny + napiecie + prad + moc + pf + wynik + symbol_testera + uzytkownik + numer_zlecenia + indeks_ukladu + CCT + Waga_netto;
                    string urlpost1         = geturl + "/wyniki";                    
                    string response         = webPostMethod(datatosend, urlpost1);
                    
                    serial_number_flag = true;                    

                    qrcode_flag = ( dbdata[1].Contains("Astra")     ||
                                    dbdata[1].Contains("ASTRA")     ||

                                    dbdata[1].Contains("Tiara")     ||
                                    dbdata[1].Contains("TIARA")     ||

                                    dbdata[1].Contains("strado")    ||
                                    dbdata[1].Contains("STRADO")    ||

                                    dbdata[1].Contains("corona")    ||
                                    dbdata[1].Contains("CORONA")    ||

                                    dbdata[1].Contains("skver")     ||
                                    dbdata[1].Contains("SKVER")     ||

                                    dbdata[1].Contains("aldea")     ||
                                    dbdata[1].Contains("ALDEA"));


                    // jeśli testowana jest lampa uliczna i test jest OK to drukuje 4 etykiety z QR                
                    if (!resultflagTOTAL && checkBox9.Checked && qrcode_flag) for (int i = 0; i < 4; i++) RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FO10,5^BQN,2,2,H,7^FDQA,https://street.lenalighting.pl/api/lamps/" + serial_raw + user_for_serial + "^FS^FWR^FO95,35^BY1^BCN,35,Y,N,N^FD" + serial_raw + user_for_serial + "^FS^XZ");

                    // jeśli testowana jest lampa uliczna i test jest FAIL ale zaznaczony jest checkbox że ma drukować pomimo to - drukuje 4 etykiety z QR 
                    if (resultflagTOTAL && checkBox8.Checked && qrcode_flag)  for (int i = 0; i < 4; i++) RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FO10,5^BQN,2,2,H,7^FDQA,https://street.lenalighting.pl/api/lamps/" + serial_raw + user_for_serial + "^FS^FWR^FO95,35^BY1^BCN,35,Y,N,N^FD" + serial_raw + user_for_serial + "^FS^XZ");

                    // jeśli testowana jest lampa nie uliczna i test jest OK to drukuje 1 etykietę bez QR (tylko kod kreskowy) 
                    if (!resultflagTOTAL && checkBox2.Checked && qrcode_flag == false) RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FWR^FO60,40^BY1^BCN,35,Y,N,N^FD" + serial_raw + user_for_serial + "^FS^XZ");
                    
                    // jeśli testowana jest lampa nie uliczna i test jest FAIL ale zaznaczony jest checkbox że ma drukować pomimo to - drukuje 1 etykietę bez QR (tylko kod kreskowy) 
                    if ( resultflagTOTAL && checkBox3.Checked && qrcode_flag == false) RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FWR^FO60,40^BY1^BCN,35,Y,N,N^FD" + serial_raw + user_for_serial + "^FS^XZ");
                
                
                
                }
            }
            else MessageBox.Show("Brak urządzenia pomiarowego USB", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void textBoxIndeks_TextChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void txtDIM_Click(object sender, EventArgs e)
        {

        }

        private void txtMOC_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtIndeks_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (serial_number_flag)
            {
                RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FWR^FO60,40^BY1^BCN,35,Y,N,N^FD" + serial_raw + user_for_serial +  "^FS^XZ");
            }
            else MessageBox.Show("Brak numeru seryjnego", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FO10,50^GFA,1219,1219,23,00OF8I0FFJ07JF81F8J03CJ03C,07PFI0FFJ07JF81FCJ07CJ07C,1QF800FFJ07JF81FEJ07CJ07E,3QFC00FFJ07JF81FFJ07CJ0FE,3QFE00FFJ07JF81FF8I07CJ0FE,7QFE00FFJ07FCI01FFCI07CI01FF,7RF00FFJ07FCI01FFEI07CI01FF,SF00FFJ07FCI01IFI07CI01FF8,SF00FFJ07FCI01IFI07CI03FF8,SF00FFJ07FCI01IFC007CI03FFC,SF00FFJ07FCI01IFC007CI07BFC,SF00FFJ07FCI01F7FE007CI079FC,SF00FFJ07FCI01F3FF007CI0F9FE,SF00FFJ07FCI01F1FF807CI0F0FE,SF00FFJ07JF81F0FFC07C001F0FF,SF00FFJ07JF81F07FE07C001E07F,SF00FFJ07JF81F03FF07C001E07F8,SF00FFJ07JF81F01FF87C003C07F8,SF00FFJ07FCI01F00FFC7C003C03F8,SF00FFJ07FCI01F007FC7C007803FC,SF00FFJ07FCI01F003FE7C007801FC,SF00FFJ07FCI01F001FF3C00KFE,SF00FFJ07FCI01FI0IFC00KFE,SF00FFJ07FCI01FI07FFC00F5EDFF,SF00FFJ07FCI01FI03FFC01FI0FF,SF00FFJ07FCI01FI01FFC01EI0FF,SF00FFJ07FCI01FJ0FFC03EI07F8,SF00FFJ07FCI01FJ07FC03EI07F8,SF00FFJ07FCI01FJ07FC03CI03FC,SF00FF9F607FCFF81FJ03FC07CI03FC,SF00KF07JF81FJ01FC078I03FE,SF00KF07JF81FK0FC0F8I01FE,SF00KF07JF81FK07C0F8I01FF,SF,FF83OF,FF00OF,FC007NF,FC003NF,F8003NF00C006003FC00C0307FC181C01001FE,F8001NF00C00600E0700C030040181E01007038,F8001NF00C0060180380C030040181E0100C00C,F8001NF00C00603I080C030040181B01018006,F8001NF00C00606K0C030040181981018,F8001NF00C00606K0C03004018188103,FC003NF00C00606K0C0300401818C103,7C007NF00C00606K0IF004018186103,7E007NF00C006060FFC0C0300401818310307FF,7F01NFE00C00606I0C0C03004018181101I02,3QFE00800603I0C0C030040181819018006,1QFC00C0060300180C03004018180D00C00C,0QF800800601C0300C030040181807006018,07OFEI0FF0600F9E00C030040181803003CF,007NFJ08202001FN0401J01I0FC,^FS^XZ");

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void label32_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox5.Checked = false;
                anyindeks = false;

                indeks_6_digit = true;
            }
            else indeks_6_digit = false;

            string tempx = textBoxWpiszIndeks.Text;
            textBoxWpiszIndeks.Text = " ";
            textBoxWpiszIndeks.Text = tempx;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (serial_port_flag) pzem.resetPZ(serialPort1);
            else MessageBox.Show("Brak urządzenia pomiarowego USB", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {      

        }

        private void label40_Click(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                checkBox5.Checked = false;
                anyindeks = false;

                indeksHV = true;
            }
            else indeksHV = false;

            string tempx = textBoxWpiszIndeks.Text;
            textBoxWpiszIndeks.Text = " ";
            textBoxWpiszIndeks.Text = tempx;
        }

        private void label41_Click(object sender, EventArgs e)
        {

        }

        private void label39_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0) textBox1.BackColor = Color.WhiteSmoke;
            else if (textBox1.Text.Length > 0) textBox1.BackColor = Color.PaleGreen;
        }

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void txtNazwa_Click(object sender, EventArgs e)
        {

        }

        private void label44_Click(object sender, EventArgs e)
        {

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                checkBox1.Checked   = false;
                indeks_6_digit      = false;
                checkBox4.Checked   = false;
                indeksHV            = false;

                anyindeks = true;
            }           
            else anyindeks = false;

            string tempx = textBoxWpiszIndeks.Text;
            textBoxWpiszIndeks.Text = " ";
            textBoxWpiszIndeks.Text = tempx;

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {

        }

        private void tabPage5_Click(object sender, EventArgs e)
        {

        }

        private void label56_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0) textBox2.BackColor = Color.WhiteSmoke;
            else if (textBox2.Text.Length > 0) textBox2.BackColor = Color.PaleGreen;
        }

        private void PF_label_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                brak_indeksu_ukladu = true;
                //MessageBox.Show("Zaznaczono brak indeksu układu","UWAGA", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else brak_indeksu_ukladu = false;

        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                brak_numeru_zlecenia = true;
                //MessageBox.Show("Zaznaczono brak numeru zlecenia", "UWAGA", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

            else brak_numeru_zlecenia = false;
        }

        private void label51_Click(object sender, EventArgs e)
        {

        }

        private void label51_Click_1(object sender, EventArgs e)
        {

        }

        private void label57_Click(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
    
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label51_Click_2(object sender, EventArgs e)
        {

        }

        private void voltage_label_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void button3_Click_2(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text.Length == 0) textBox4.BackColor = Color.WhiteSmoke;
            else if (textBox4.Text.Length > 0) textBox4.BackColor = Color.PaleGreen;
        }

        private void button3_Click_3(object sender, EventArgs e)
        {
            
           RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FO10,5^BQN,2,2,H,7^FDQA,https://street.lenalighting.pl/api/lamps/" + serial_raw + user_for_serial + "^FS^FWR^FO95,35^BY1^BCN,35,Y,N,N^FD" + serial_raw + user_for_serial + "^FS^XZ");
           //RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FO5,0^BQN,2,2,H,7^FDQA,https://street.lenalighting.pl/api/lamps/" + serial_raw + user_for_serial + "^FS^XZ");
        }
        
        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string nowy_kod_kreskowy = textBox5.Text;
            if(nowy_kod_kreskowy.Length == 0) MessageBox.Show("Brak numeru seryjnego");
            else
            {
                int lim = int.Parse(comboBox5.SelectedItem.ToString());
                for (int i = 0; i < lim; i++) RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FWR^FO60,40^BY1^BCN,35,Y,N,N^FD" + nowy_kod_kreskowy + "^FS^XZ");
            }
           
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string nowy_kod_qr = textBox6.Text;
            if (nowy_kod_qr.Length == 0) MessageBox.Show("Brak numeru seryjnego");
            else
            {
                int lim = int.Parse(comboBox4.SelectedItem.ToString());
                for (int i = 0; i < lim ; i++) RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FO10,5^BQN,2,2,H,7^FDQA,https://street.lenalighting.pl/api/lamps/" + nowy_kod_qr + "^FS^FWR^FO95,35^BY1^BCN,35,Y,N,N^FD" + nowy_kod_qr + "^FS^XZ");
            }

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void comboBox4_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            string nowy_kod_qr = textBox7.Text;
            if (nowy_kod_qr.Length == 0) MessageBox.Show("Brak numeru seryjnego");
            else
            {
                int lim = int.Parse(comboBox6.SelectedItem.ToString());
                int qrsize = int.Parse(comboBox7.SelectedItem.ToString());

                for (int i = 0; i < lim; i++) RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FO10,5^BQN,2,"+qrsize+",H,7^FDQA," + nowy_kod_qr +"^FS^XZ");
            }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (serial_number_flag)
            {
                RawPrinterHelper.SendStringToPrinter(usedprinter, "^XA^FWR^FO60,40^BY1^BCN,35,Y,N,N^FD" + serial_raw + user_for_serial + "^FS^XZ");
            }
            else MessageBox.Show("Brak numeru seryjnego", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

} 