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

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // cells should be square, find min of the two and make a block
            int cellSize = Math.Min(
                graphicsPanel1.ClientSize.Width / universe.GetLength(0),
                graphicsPanel1.ClientSize.Height / universe.GetLength(1)
            );

            Func<int, int, int, int> calc_offset = (int total_length, int cell_size, int amount) => (total_length - (cell_size * amount)) / 2;

            (int x, int y) offset = (
                calc_offset(graphicsPanel1.ClientSize.Width, cellSize, universe.GetLength(0)),
                calc_offset(graphicsPanel1.ClientSize.Height, cellSize, universe.GetLength(1))
            );

            Pen gridPen = new Pen(gridColor, 1);
            Brush cellBrush = new SolidBrush(cellColor);

            for (int y = 0; y < universe.GetLength(1); y += 1)
            {
                for (int x = 0; x < universe.GetLength(0); x += 1)
                {
                    Rectangle cellRect = new Rectangle(
                        x * cellSize + offset.x,
                        y * cellSize + offset.y,
                        cellSize, cellSize
                    );

                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    e.Graphics.DrawRectangle(gridPen, cellRect);
                }
            }

            gridPen.Dispose();
            cellBrush.Dispose();
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int cellSize = Math.Min(
                    graphicsPanel1.ClientSize.Width / universe.GetLength(0),
                    graphicsPanel1.ClientSize.Height / universe.GetLength(1)
                );

                Func<int, int, int, int> calc_offset = (int total_length, int cell_size, int amount) => (total_length - (cell_size * amount)) / 2;

                (int x, int y) offset = (
                    calc_offset(graphicsPanel1.ClientSize.Width, cellSize, universe.GetLength(0)),
                    calc_offset(graphicsPanel1.ClientSize.Height, cellSize, universe.GetLength(1))
                );

                // Calculate the cell that was clicked in
                int x = (e.X - offset.x) / cellSize;
                int y = (e.Y - offset.y) / cellSize;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }
    }
}
