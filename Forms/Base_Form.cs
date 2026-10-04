
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth
{
    public partial class Base_Form : Form
    {
        public Base_Form()
        {
            ErstelleLayout();
            InitializeComponent();
        }

        private void ErstelleLayout()
        {
            //Type type = this.GetType();
            //PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Where(x => x.GetCustomAttribute(typeof(Custom_Ignore)) == null).ToArray();
            //bool isStatic = false;

            //if (!type.IsInstanceOfType(type))
            //{
            //    isStatic = true;
            //}

            //int zuerstellendeZeilen = properties.Count() + 1; // +1 für die Buttons am Ende

            //int sizeY = this.Height / zuerstellendeZeilen;
            //int sizeX = this.Width / 2;
            //Size size = new Size((int)(sizeX * 0.9), ((int)(sizeY * 0.9)));
            //int counter = 0;

            //foreach (var prop in properties)
            //{
            //    var label = new Label();
            //    label.Text = prop.Name;
            //    label.Location = new Point(0, counter * sizeY);
            //    label.Size = size;
            //    this.Controls.Add(label);

            //    if()
            //    var textbox = new TextBox();
            //    textbox.Name = prop.Name;
            //    textbox.Text = prop.GetValue(typeof(App_Einstellungen)).ToString();
            //    textbox.Location = new Point(sizeX, counter * sizeY);
            //    textbox.Size = size;
            //    this.Controls.Add(textbox);

            //    counter++;
            //}

            //Button button = new Button();
            //button.Name = "button_ok";
            //button.Click += Button_Click;
            //button.Text = "OK";
            //button.Location = new Point(0, counter * sizeY);
            //button.Size = size;
            //this.Controls.Add(button);

            //Button button2 = new Button();
            //button2.Name = "button_speichern";
            //button2.Click += button_speichereEinstellungen_Click;
            //button2.Text = "Speichern";
            //button2.Location = new Point(sizeX, counter * sizeY);
            //button2.Size = size;
            //this.Controls.Add(button2);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Uebernehme_Aenderungen();

            Helper.Mainform.Enabled = true;
            this.Close();
        }

        private void Uebernehme_Aenderungen()
        {
            foreach (var box in this.Controls)
            {
                if (box is TextBox textBox)
                {
                    int result;
                    if (int.TryParse(textBox.Text, out result))
                    {
                        typeof(App_Einstellungen).GetProperty(textBox.Name).SetValue(typeof(App_Einstellungen), result);
                    }
                    else
                    {
                        textBox.Text = "";
                        MessageBox.Show("Bitte nur ganze Zahlen als Margin eintragen!");
                        return;
                    }
                }
            }
        }

        private void button_speichereEinstellungen_Click(object sender, EventArgs e)
        {
            Uebernehme_Aenderungen();

            if (string.IsNullOrEmpty(App_Einstellungen.Speicherort_Einstellungen))
            {
                string speicherort = ExportImportHelper.Besorge_Default_Speicherort(true);
                ExportImportHelper.Erstelle_OrdnerStruktur_wennNoetig(speicherort);
                App_Einstellungen.Speicherort_Einstellungen = speicherort;
            }

            PropertyInfo[] propertiesArray = typeof(App_Einstellungen).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.GetCustomAttribute(typeof(Custom_Ignore)) == null).ToArray();

            string ausgabe = ExportImportHelper.Exportiere_Static_Objekt(App_Einstellungen.Speicherort_Einstellungen, "Einstellungen", propertiesArray, typeof(App_Einstellungen)).hinweis;
            MessageBox.Show(ausgabe);
        }
    }
}
