using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoBorderForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Librairies.formCustomInit(this, Color.White, Color.Green, Color.Red, Color.Magenta, true, this.panel1, Librairies.formCustomButton.Alls);

        }
    }
}
