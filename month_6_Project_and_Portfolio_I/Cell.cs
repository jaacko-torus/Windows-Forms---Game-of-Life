using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace month_6_Project_and_Portfolio_I {
    class Cell {
        private bool state;

        private readonly (int x, int y) cell;

        public int neighbours = 0;

        public Cell((int x, int y) cell, bool state) {
            this.state = state;
            this.cell = cell;
        }

        public (int x, int y) Values { get => this.cell; }

        public bool IsAlive { get => this.state; }

        public void Set(bool value) {
            this.state = value;
        }
        
        public void Toggle() => this.state = !this.state;

        public void Draw(PaintEventArgs e, Brush brush, Rectangle rectangle) {
            if (this.IsAlive) {
                e.Graphics.FillRectangle(brush, rectangle);
            }
        }

        public void Write(PaintEventArgs e, Brush brush, Rectangle rectangle) {
            Brush text_brush = new SolidBrush(Color.FromArgb(0xff, 0xff, 0xff));

            e.Graphics.DrawString(
                this.neighbours.ToString(),
                SystemFonts.DefaultFont,
                text_brush,
                rectangle.X, rectangle.Y
            );

            text_brush.Dispose();
        }

        public bool StateFromNeighbours(int neighbour_count) => this.IsAlive
            ? neighbour_count == 2 || neighbour_count == 3
            : neighbour_count == 3;
    }
}
