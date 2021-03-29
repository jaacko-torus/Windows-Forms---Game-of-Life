using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace month_6_Project_and_Portfolio_I
{
    public partial class Window : Form
    {
        // Drawing colors, Pallette https://www.nordtheme.com/docs/colors-and-palettes
        private readonly Color grid_color      = Color.FromArgb(0x3B, 0x42, 0x52); // #3B4252
        private readonly Color cell_grid_color = Color.FromArgb(0xD0, 0x70, 0x7F); // #D0707F Using relative to cell_color
        private readonly Color cell_color      = Color.FromArgb(0xBF, 0x61, 0x6A); // #BF616A

        public int block_size;
        public int cell_size;
        public (int x, int y) offset;
        public (int x, int y) center;

        private Dictionary<(int x, int y), Block> map = new Dictionary<(int x, int y), Block>();

        private readonly Timer timer = new Timer();

        int generation = 0;

        public Window() {
            this.InitializeComponent();

            this.block_size = 10;

            DefaultSingleBlockStyle();

            this.map.Add((0, 0), new Block(0, 0, this.block_size));

            // Setup the timer
            this.timer.Interval = 500;
            this.timer.Tick += (object s, EventArgs e) => this.Next();

            // keyboard
            this.KeyDown += this.KeyEventHandler;
        }

        public void Redraw() {
            this.graphicsPanelMain.Invalidate();
        }

        private void KeyEventHandler(object sender, KeyEventArgs e) {
            int speed = 10;

            switch (e.KeyCode) {
                case Keys.Left:  this.offset.x -= speed; break;
                case Keys.Right: this.offset.x += speed; break;
                case Keys.Up:    this.offset.y -= speed; break;
                case Keys.Down:  this.offset.y += speed; break;
            }

            this.Redraw();
        }

        private int DefaultCellSize(GraphicsPanel panel) =>
                Math.Min(panel.ClientSize.Width / this.block_size, panel.ClientSize.Height / this.block_size);
        
        private (int x, int y) DefaultOffset(GraphicsPanel panel) => (
            (panel.ClientSize.Width - (this.cell_size * this.block_size)) / 2,
            (panel.ClientSize.Height - (this.cell_size * this.block_size)) / 2
        );

        private void DefaultSingleBlockStyle() {
            this.cell_size = DefaultCellSize(this.graphicsPanelMain);
            this.offset = DefaultOffset(this.graphicsPanelMain);
        }

        // Calculate the next generation of cells
        private void Next() {

            this.map[(0, 0)].Next();

            this.generation += 1;

            // Update status strip generations
            this.toolStripStatusLabelGenerations.Text = $"Generation {this.generation}";
        }

        private void graphicsPanelMain_Paint(object sender, PaintEventArgs e) {
            Pen grid_pen = new Pen(this.grid_color, 1);
            Brush cell_brush = new SolidBrush(this.cell_color);

            this.map[(0, 0)].Draw(e, cell_brush, grid_pen, this.cell_size, offset);

            grid_pen.Dispose();
            cell_brush.Dispose();
        }

        private void graphicsPanelMain_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                if (this.map[(0, 0)].Within(e.X, e.Y, this.cell_size, offset)) {
                    (int x, int y) cell_clicked = (
                        (e.X - this.offset.x) / this.cell_size,
                        (e.Y - this.offset.y) / this.cell_size
                    );

                    this.map[(0, 0)].Toggle(cell_clicked);

                    this.Redraw();
                }
            }
        }

        private void toolStripStart_Click(object sender, EventArgs e) {
            this.timer.Start();
        }
    }
}
