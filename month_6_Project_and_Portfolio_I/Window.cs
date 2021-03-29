using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Pallette https://www.nordtheme.com/docs/colors-and-palettes

namespace month_6_Project_and_Portfolio_I
{
    public partial class Window : Form
    {
        // The universe array
        bool[,] universe = new bool[5, 5];

        // Drawing colors
        Color gridColor           = Color.FromArgb(0x3B, 0x42, 0x52); // #3B4252
        Color gridNextToCellColor = Color.FromArgb(0xD0, 0x70, 0x7F); // #D0707F Using relative to cellColor
        Color cellColor           = Color.FromArgb(0xBF, 0x61, 0x6A); // #BF616A

        Dictionary<(int x, int y), Block> Map = new Dictionary<(int x, int y), Block>() {
            { (0,0), new Block(0, 0, 10) }
        };

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        public Window()
        {
            InitializeComponent();

            // Setup the timer
            timer.Interval = 500;
            timer.Tick += Timer_Tick;
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {


            // Increment generation count
            generations += 1;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
        }
        
        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private int CellSize(GraphicsPanel panel, int size) => Math.Min(panel.ClientSize.Width / size, panel.ClientSize.Height / size);
        private int CalcSingleOffset(int total_length, int cell_size, int amount) => (total_length - (cell_size * amount)) / 2;
        private (int x, int y) CalcOffset(GraphicsPanel panel, int cell_size, int amount) => (
            CalcSingleOffset(panel.ClientSize.Width, cell_size, amount),
            CalcSingleOffset(panel.ClientSize.Height, cell_size, amount)
        );

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            int cell_size = CellSize(graphicsPanel1, Map[(0, 0)].size);

            (int x, int y) offset = CalcOffset(graphicsPanel1, cell_size, Map[(0, 0)].size);

            Pen gridPen = new Pen(gridColor, 1);
            Brush cellBrush = new SolidBrush(cellColor);

            Map[(0, 0)].ForEach((x, y) => {
                Rectangle cellRect = new Rectangle(
                    (int)x * cell_size + offset.x,
                    (int)y * cell_size + offset.y,
                    cell_size, cell_size
                );

                if (Map[(0, 0)].CellIs(x, y, true))
                {
                    e.Graphics.FillRectangle(cellBrush, cellRect);
                }

                e.Graphics.DrawRectangle(gridPen, cellRect);
            });

            gridPen.Dispose();
            cellBrush.Dispose();
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int cell_size = CellSize(graphicsPanel1, Map[(0, 0)].size);

                (int x, int y) offset = CalcOffset(graphicsPanel1, cell_size, Map[(0, 0)].size);

                (uint x, uint y) cell_clicked = (
                    (uint)((e.X - offset.x) / cell_size),
                    (uint)((e.Y - offset.y) / cell_size)
                );

                // Toggle the cell's state
                Map[(0, 0)].InvertCell(cell_clicked.x, cell_clicked.y);

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        private void toolStripStart_Click(object sender, EventArgs e)
        {
            this.timer.Start();
        }
    }
}
