using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

namespace PzemDemo
{

    public partial class Form2 : Form
    {
        volatile string global_CCT_COM;
        void search_COM_port_Bluetooth()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
            string[] tempp;
            string[] tempp2;

            foreach (ManagementObject result in searcher.Get())
            {
                comboBox4.Items.Add(result["Caption"].ToString());

                if (result["Caption"].ToString().Contains("Bluetooth"))
                {
                    string nazwa_wybranego_portu_COM = result["Caption"].ToString();
                    tempp = nazwa_wybranego_portu_COM.Split('(');
                    tempp2 = tempp[1].Split(')');                    
                    global_CCT_COM = tempp2[0];
                    comboBox4.SelectedItem = nazwa_wybranego_portu_COM;
                }
            }

        }
        public Form2()
        {
            InitializeComponent();
            search_COM_port_Bluetooth();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string nazwa_wybranego_portu_COM = comboBox4.SelectedItem.ToString();           
            string[] tempp;
            string[] tempp2;
            tempp = nazwa_wybranego_portu_COM.Split('(');
            tempp2 = tempp[1].Split(')');
            global_CCT_COM = tempp2[0];           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataContainer.ValueToShare += ("," + global_CCT_COM);

            Form1 newobjform = new Form1();
            this.Hide();
            newobjform.Show();
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataContainer.ValueToShare += ("," + "brak");

            Form1 newobjform = new Form1();
            this.Hide();
            newobjform.Show();
        }
    }
}
