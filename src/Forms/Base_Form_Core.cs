using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth
{
    public partial class Base_Form_Core : Form
    {
        public Base_Form_Core(Button button)
        {
            InitializeComponent();
            button.Enabled = false;
            Button_form_opener = button;
            this.FormClosed += Base_Form_Core_FormClosed;
            if (Helper.Aktive_Forms.Contains(this))
            {
                MessageBox.Show($"Interner Fehler: Die Klasse{this.GetType().Name} ist bereits in der" +
                    $" statischen Form Referenz Liste");
            }
            Helper.Aktive_Forms.Add(this);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        public Base_Form_Core()
        {
        }

        public Button Button_form_opener { get; set; }

        private void Base_Form_Core_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.FormClosed -= Base_Form_Core_FormClosed;
            Button_form_opener.Enabled = true;
            Helper.Aktive_Forms.Remove(this);
        }
    }
}
