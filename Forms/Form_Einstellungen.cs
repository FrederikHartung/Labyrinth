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
    public partial class Form_Einstellungen : Base_Form_Core
    {
        public Form_Einstellungen(Button button) : base(button)
        {
            ErstelleLayout();
            this.Name = "Einstellungen";
        }

        public int LayoutMargin { get; set; } = 25;

        private void ErstelleLayout()
        {
            PropertyInfo[] properties  = typeof(App_Einstellungen).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.GetCustomAttribute(typeof(Custom_Ignore)) == null).ToArray();
            int zuerstellendeZeilen = properties.Count() + 1; // +1 um die Knöpfe ok/speichern hinzuzufügen 

            int sizeY = (this.Height - 3 * LayoutMargin) / zuerstellendeZeilen;
            int sizeX = this.Width / 2;
            Size size = new Size((int)(sizeX * 0.90), ((int)(sizeY * 0.90)));
            int counter = 0;

            foreach (var prop in properties)
            {
                var label = new Label();
                label.Text = prop.Name;
                label.Location = new Point(0, counter * sizeY + LayoutMargin);
                label.Size = size;
                this.Controls.Add(label);

                var textbox = new TextBox();
                textbox.Name = prop.Name;
                textbox.Text = prop.GetValue(typeof(App_Einstellungen)).ToString();
                textbox.Location = new Point(sizeX, counter * sizeY + LayoutMargin);
                textbox.Size = size;
                this.Controls.Add(textbox);

                counter++;
            }

            Button button2 = new Button();
            button2.Name = "button_speichern";
            button2.Click += button_speichereEinstellungen_Click;
            button2.Text = "Speichern";
            button2.Location = new Point(0, counter * sizeY + LayoutMargin);
            button2.Size = new Size((int)(size.Width * 2 / 3), size.Height); ;
            this.Controls.Add(button2);

            Button button = new Button();
            button.Name = "button_ok";
            button.Click += Button_Click;
            button.Text = "OK";
            button.Location = new Point((int)(sizeX * 2 / 3), counter * sizeY + LayoutMargin);
            button.Size = new Size((int)(size.Width * 2 / 3), size.Height); ;
            this.Controls.Add(button);

            Button button3 = new Button();
            button3.Name = "button_abbrechen";
            button3.Click += Button3_Click; ;
            button3.Text = "Abbrechen";
            button3.Location = new Point(2* (int)(sizeX * 2 / 3), counter * sizeY + LayoutMargin);
            button3.Size = new Size((int)(size.Width * 2 / 3), size.Height); ;
            this.Controls.Add(button3);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Helper.Mainform.Enabled = true;
            this.Close();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Uebernehme_Aenderungen();

            Helper.Mainform.Enabled = true;
            this.Close();
        }

        private void Einstellungen_FormClosed(object sender, FormClosedEventArgs e)
        {
            Helper.Mainform.Enabled = true;
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

            if (Helper.Mainform._Canvas != null)
            {
                Helper.Mainform._Canvas.Resize_Update_Properties();
            }
            //Helper.Mainform.Enabled = true;
            //Helper.Mainform.Resize_Frame();
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
