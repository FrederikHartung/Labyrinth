using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth
{
    public class Editor
    {
        public Editor(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            Value_Typ = PropertyInfo.PropertyType;
            Label = new Label();
            Label.Text = propertyInfo.Name;
            ErstelleValueHolder();
        }

        public PropertyInfo PropertyInfo { get; set; }
        public Label Label { get; set; }
        public object ValueHolder { get; set; }
        public Type ValueHolder_Typ { get; set; }
        public Type Value_Typ { get; set; }

        private void ErstelleValueHolder()
        {
            if (Value_Typ == typeof(string))
            {
                ValueHolder = new TextBox();
                ValueHolder_Typ = typeof(TextBox);
            }
            else if (Value_Typ == typeof(int))
            {
                NumericUpDown holder = new NumericUpDown();
                holder.Increment = 1;
                ValueHolder = holder;
                ValueHolder_Typ = typeof(NumericUpDown);
            }
            else if (Value_Typ == typeof(double) || Value_Typ == typeof(float) || Value_Typ == typeof(decimal))
            {
                NumericUpDown holder = new NumericUpDown();
                ValueHolder = holder;
                ValueHolder_Typ = typeof(NumericUpDown);
            }
            else if (Value_Typ == typeof(DateTime))
            {
                DateTimePicker holder = new DateTimePicker();
                ValueHolder = holder;
                ValueHolder_Typ = typeof(DateTimePicker);
            }
            else if (Value_Typ == typeof(bool))
            {
                CheckBox holder = new CheckBox();
                ValueHolder = holder;
                ValueHolder_Typ = typeof(bool);
            }
            else
            {
                MessageBox.Show($"Der Typ {ValueHolder_Typ} wird noch nicht von der \"ErstelleValueHolder\" der Klasse {this} unterstützt.");
            }

        }
    }
}
