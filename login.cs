using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace PzemDemo
{
    public partial class login : Form
    {
        

        public login()
        {
            InitializeComponent();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
          
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

    
        private void button1_Click(object sender, EventArgs e)
        {          
            if ((txtLogin.Text == "KJ_01" || txtLogin.Text == "kj_01") ||
                (txtLogin.Text == "KJ_02" || txtLogin.Text == "kj_02") ||
                (txtLogin.Text == "KJ_03" || txtLogin.Text == "kj_03") ||
                (txtLogin.Text == "KJ_04" || txtLogin.Text == "kj_04") ||
                (txtLogin.Text == "KJ_05" || txtLogin.Text == "kj_05") ||
                (txtLogin.Text == "KJ_06" || txtLogin.Text == "kj_06") ||
                (txtLogin.Text == "KJ_07" || txtLogin.Text == "kj_07") ||
                (txtLogin.Text == "KJ_08" || txtLogin.Text == "kj_08") ||
                (txtLogin.Text == "KJ_09" || txtLogin.Text == "kj_09") ||
                (txtLogin.Text == "KJ_10" || txtLogin.Text == "kj_10") ||
                (txtLogin.Text == "KJ_11" || txtLogin.Text == "kj_11") ||
                (txtLogin.Text == "KJ_12" || txtLogin.Text == "kj_12") ||
                (txtLogin.Text == "KJ_13" || txtLogin.Text == "kj_13") ||
                (txtLogin.Text == "KJ_14" || txtLogin.Text == "kj_14") ||
                (txtLogin.Text == "KJ_15" || txtLogin.Text == "kj_15") ||
                (txtLogin.Text == "KJ_16" || txtLogin.Text == "kj_16") ||
                (txtLogin.Text == "KJ_17" || txtLogin.Text == "kj_17") ||
                (txtLogin.Text == "KJ_18" || txtLogin.Text == "kj_18") ||
                (txtLogin.Text == "KJ_19" || txtLogin.Text == "kj_19") ||
                (txtLogin.Text == "KJ_20" || txtLogin.Text == "kj_20") ||
                (txtLogin.Text == "KJ_21" || txtLogin.Text == "kj_21") ||
                (txtLogin.Text == "KJ_22" || txtLogin.Text == "kj_22") ||
                (txtLogin.Text == "KJ_23" || txtLogin.Text == "kj_23") ||
                (txtLogin.Text == "KJ_24" || txtLogin.Text == "kj_24") ||
                (txtLogin.Text == "KJ_25" || txtLogin.Text == "kj_25") ||
                (txtLogin.Text == "ADMIN" || txtLogin.Text == "admin"))
            {
                Form1_Method(txtLogin.Text);
                //Form1 newobjform = new Form1();
                //this.Hide();
                //newobjform.Show();
                Form2 newobjform = new Form2();
                this.Hide();
                newobjform.Show();
            }

            else
            {
                MessageBox.Show("Wprowadź poprawną nazwę użytkownika");
            }   
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        public void Form1_Method(string stringtoshare)
        {
            DataContainer.ValueToShare = stringtoshare;
        }

        private void login_Load(object sender, EventArgs e)
        {

        }
    }
}
