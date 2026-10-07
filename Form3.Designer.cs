using System;
using System.Windows.Forms; // Upewnij się, że to jest na górze

namespace PzemDemo
// Zmień na swoją przestrzeń nazw
{
    public partial class Form3 : Form
    {
        //private int countdownSeconds = 20; // Czas blokady, np. 20 sekund
        private Form1 _parentForm;
        public DialogResult UserResult { get; private set; } // Przechowuje wynik wyboru użytkownika
        string result_message;
        bool while_flag = true;
        public Form3(string message, string caption, Form1 parentForm)
        {
            InitializeComponent();
            this.Text = caption;
            messageLabel.Text = message;         
            _parentForm = parentForm;
            result_message = _parentForm.OdczytajDaneZPortuZTimeoutem();

            while (while_flag)
            {
                if(result_message == "PASS")
                {
                    MessageBox.Show(result_message);
                    while_flag = false;
                }
                else if (result_message == "FAIL")
                {
                    MessageBox.Show(result_message);
                    while_flag = false;
                }
                else if (result_message == "TIMEOUT")
                {
                    MessageBox.Show(result_message);
                    while_flag = false;
                }
            }


        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            //UserResult = DialogResult.Yes;
            //string newe = _parentForm.read_kable();
            //this.Close(); // Zamknij formularz
            //messageLabel3.Text = _parentForm.read_kable();
            
            //if(messageLabel3.Text == "PASS") UserResult = DialogResult.Yes;
            //else UserResult = DialogResult.No;
            //for(int i=10000000; i<1; i--) { }
           // this.Close();

        }

 



        private Label messageLabel;
        private Button btnYes;
        private Timer delayTimer;
        private System.ComponentModel.IContainer components;
    }

  
}