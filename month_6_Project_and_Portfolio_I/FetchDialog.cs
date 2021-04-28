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
    public partial class FetchDialog : Form {
        public string value {
            get => this.textBox1.Text;
        }

        public FetchDialog() {
            this.InitializeComponent();
        }
    }
}
