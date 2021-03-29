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
        private readonly Color gridColor           = Color.FromArgb(0x3B, 0x42, 0x52); // #3B4252
        private readonly Color gridNextToCellColor = Color.FromArgb(0xD0, 0x70, 0x7F); // #D0707F Using relative to cellColor
        private readonly Color cellColor           = Color.FromArgb(0xBF, 0x61, 0x6A); // #BF616A

        private Dictionary<(int x, int y), Block> Map = new Dictionary<(int x, int y), Block>() {
            { (0,0), new Block(0, 0, 10) }
        };

        private readonly Timer timer = new Timer();

        int generation = 0;

        public Window() {
            this.InitializeComponent();

            // Setup the timer
            this.timer.Interval = 500;
            //timer.Tick += Timer_Tick;
            this.timer.Tick += (object s, EventArgs e) => this.NextGeneration();
        }

        // Calculate the next generation of cells
        private void NextGeneration() {

            //this.Map

            this.generation += 1;

            // Update status strip generations
            this.toolStripStatusLabelGenerations.Text = $"Generation {this.generation}";
        }

        private int CellSize(GraphicsPanel panel, int size) => Math.Min(panel.ClientSize.Width / size, panel.ClientSize.Height / size);
        private int CalcSingleOffset(int total_length, int cell_size, int amount) => (total_length - (cell_size * amount)) / 2;
        private (int x, int y) CalcOffset(GraphicsPanel panel, int cell_size, int amount) => (
            CalcSingleOffset(panel.ClientSize.Width, cell_size, amount),
            CalcSingleOffset(panel.ClientSize.Height, cell_size, amount)
        );

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e) {
            int cell_size = CellSize(this.graphicsPanelMain, this.Map[(0, 0)].size);

            var offset = CalcOffset(this.graphicsPanelMain, cell_size, this.Map[(0, 0)].size);

            Pen gridPen = new Pen(this.gridColor, 1);
            Brush cellBrush = new SolidBrush(this.cellColor);

            this.Map[(0, 0)].ForEach((x, y) => {
                Rectangle cellRect = new Rectangle(
                    x * cell_size + offset.x,
                    y * cell_size + offset.y,
                    cell_size, cell_size
                );

                if (this.Map[(0, 0)].Get(x, y).IsAlive) {
                    e.Graphics.FillRectangle(cellBrush, cellRect);
                }

                e.Graphics.DrawRectangle(gridPen, cellRect);
            });

            gridPen.Dispose();
            cellBrush.Dispose();
        }

        private void graphicsPanelMain_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                int cell_size = CellSize(this.graphicsPanelMain, this.Map[(0, 0)].size);

                var offset = CalcOffset(this.graphicsPanelMain, cell_size, this.Map[(0, 0)].size);

                (int x, int y) cell_clicked = (
                    (e.X - offset.x) / cell_size,
                    (e.Y - offset.y) / cell_size
                );

                this.Map[(0, 0)].Toggle(cell_clicked);

                // Tell Windows you need to repaint
                this.graphicsPanelMain.Invalidate();
            }
        }

        private void toolStripStart_Click(object sender, EventArgs e) {
            this.timer.Start();
        }
    }
}
