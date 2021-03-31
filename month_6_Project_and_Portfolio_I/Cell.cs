using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace month_6_Project_and_Portfolio_I {
    class Cell {
        // public

        // static
        public static Brush font_brush;
        public static Font font;
        public static StringFormat font_string_format;
        public static Brush cell_brush;

        // getters
        public (int x, int y) Values { get => this.cell; }
        public bool IsAlive { get => this.state; }

        // properties
        public int neighbours = 0;

        // private

        // readonly
        private readonly (int x, int y) cell;

        // properties
        private bool state;

        // constructor
        public Cell((int x, int y) cell, bool state) {
            this.state = state;
            this.cell = cell;
        }

        // methods

        // setters

        public void Set(bool value) {
            this.state = value;
        }
        
        public void Toggle() => this.state = !this.state;

        // getters

        public bool StateFromNeighbours(int neighbour_count) => this.IsAlive
            ? neighbour_count == 2 || neighbour_count == 3
            : neighbour_count == 3;

        // side effects

        public void Draw(PaintEventArgs e, Rectangle rectangle) {
            if (this.IsAlive) {
                e.Graphics.FillRectangle(Cell.cell_brush, rectangle);
            }
        }

        public void Write(PaintEventArgs e, Rectangle rectangle) {
            e.Graphics.DrawString(
                this.neighbours.ToString(),
                Cell.font,
                Cell.font_brush,
                rectangle,
                Cell.font_string_format
            );
        }
    }
}
