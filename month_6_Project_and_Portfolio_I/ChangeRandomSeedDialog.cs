using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace month_6_Project_and_Portfolio_I {
    public partial class ChangeRandomSeedDialog : Form {
        private Random random = new Random();
        public int seed {
            get => (int)this.numericUpDownSeed.Value;
            set => this.numericUpDownSeed.Value = value;
        }

        public ChangeRandomSeedDialog() {
            this.InitializeComponent();
        }

        private void buttonRandomize_Click(object sender, EventArgs e) {
            this.seed = random.Next();
        }
    }
}
