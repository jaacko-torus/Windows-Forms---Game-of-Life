using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace month_6_Project_and_Portfolio_I {
    class Cell {
        public bool state;

        private readonly int x;
        private readonly int y;

        public Cell(int x, int y, bool state) {
            this.state = state;

            this.x = x;
            this.y = y;
        }

        public (int x, int y) Values { get => (this.x, this.y); }

        public bool IsAlive { get => this.state; }

        public void Set(bool value) { this.state = value; }
        
        public void Toggle() => this.state = !this.state;

        public void Draw(PaintEventArgs e, Brush brush, Rectangle rectangle) {
            if (this.IsAlive) {
                e.Graphics.FillRectangle(brush, rectangle);
            }
        }

        public void Write(PaintEventArgs e, Brush brush, Rectangle rectangle, string text) {
            Brush text_brush = new SolidBrush(Color.FromArgb(0xff, 0xff, 0xff));
            e.Graphics.DrawString("0", SystemFonts.DefaultFont, text_brush, rectangle.X, rectangle.Y);
            text_brush.Dispose();
        }
    }
}
