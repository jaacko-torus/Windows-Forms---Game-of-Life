using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;

namespace month_6_Project_and_Portfolio_I {
    class Cell {
        // public

        // static
        public static Brush font_brush;
        public static Font font;
        public static StringFormat font_string_format;
        public static Brush cell_brush;

        public bool is_alive { get => this.state; }

        // properties
        public int neighbors = 0;
        public bool has_neighbors => neighbors != 0;

        private bool state;

        public bool next_state => this.is_alive
            ? this.neighbors == 2 || this.neighbors == 3
            : this.neighbors == 3;

        public Cell(bool state) {
            this.state = state;
        }

        public void Toggle() => this.state = !this.state;
        public void Set(bool value) => this.state = value;
        public void Next() => this.state = this.next_state;
        
        public void Paint(PaintEventArgs e, RectangleF rectangle) {
            if (this.is_alive) {
                e.Graphics.FillRectangle(Cell.cell_brush, rectangle);
            }
        }

        public void Write(PaintEventArgs e, RectangleF rectangle) {
            if (Universe.camera.zoom < Universe.camera.zoom_remove_cell_neighbors) {
                e.Graphics.DrawString(
                    this.neighbors.ToString(),
                    Cell.font,
                    Cell.font_brush,
                    rectangle,
                    Cell.font_string_format
                );
            }
        }
    }
}
