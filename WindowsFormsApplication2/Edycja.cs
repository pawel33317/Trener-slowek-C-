using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace TrenerSlowek
{
    public partial class Edycja : Form
    {
        String SlowkoPL = "";
        String SlowkoEN = "";
        Glowne OknoGlowne;
        public Edycja(string slowkoPL, string slowkoEN, Glowne oknoglowne, string opisPL, string opisEN)
        {
            this.OknoGlowne = oknoglowne;
            this.SlowkoPL = slowkoPL;
            this.SlowkoEN = slowkoEN;
            InitializeComponent();
            this.textBox1.Text = slowkoPL;
            this.textBox2.Text = slowkoEN;
            this.textBox3.Text = opisPL;
            this.textBox4.Text = opisEN;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //metoda w oknie glownym aktualizuje dane w bazie
            this.OknoGlowne.aktualizujSlowka(this.textBox1.Text, this.textBox3.Text, this.textBox4.Text, this.textBox2.Text, this.SlowkoPL, this.SlowkoEN);
            this.Close();
        }

        private void Edycja_Load(object sender, EventArgs e)
        {

        }
    }
}
