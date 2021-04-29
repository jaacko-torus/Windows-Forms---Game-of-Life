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
    public partial class ChangePlaySpeedDialog : Form {

        public float play_speed {
            get => (float)this.numericUpDownPlaySpeed.Value;
            set => this.numericUpDownPlaySpeed.Value = (decimal)value;
        }
        public ChangePlaySpeedDialog() {
            this.InitializeComponent();
        }
    }
}
