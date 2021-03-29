using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace month_6_Project_and_Portfolio_I
{
    class Block
    {
        public readonly (int x, int y) coord_id;

        public readonly int block_size;
        public Cell[,] cells;
        
        public List<(int x, int y)> alive_list = new List<(int x, int y)>();
        private Dictionary<(int x, int y), int> inside_cells = new Dictionary<(int x, int y), int>();

        public Block(int x_position, int y_position, int block_size) {
            this.coord_id = (x_position, y_position);
            this.block_size = block_size;
            this.cells = new Cell[block_size, block_size];

            this.ForEach((x, y) => this.cells[x, y] = new Cell(x, y, false));
        }

        public delegate void ForEachCallback(int x, int y);

        public void ForEach(ForEachCallback callback) {
            for (int y = 0; y < this.cells.GetLength(1); y += 1) {
                for (int x = 0; x < this.cells.GetLength(0); x += 1) {
                    callback(x, y);
                }
            }
        }

        public Cell Get(int x, int y) => this.cells[x, y];
        public Cell Get((int x, int y) cell) => this.Get(cell.x, cell.y);

        public void Set(int x, int y, bool value) { this.cells[x, y].Set(value); }
        public void Set((int x, int y) cell, bool value) { this.Set(cell.x, cell.y, value); }

        public void Toggle(int x, int y) {
            this.cells[x, y].Toggle();

            if (this.cells[x, y].IsAlive) {
                this.alive_list.Add((x, y));
            } else {
                bool could_remove = this.alive_list.Remove((x, y));

                if (!could_remove) {
                    Console.WriteLine($"{x}, {y} was not alive");
                }
            }
        }
        public void Toggle((int x, int y) cell) { this.Toggle(cell.x, cell.y); }

        public bool IsOutsideCell((int x, int y) cell) => (
            cell.x < 0 || this.block_size <= cell.x ||
            cell.y < 0 || this.block_size <= cell.y
        );

        public delegate void MatrixScanAliveCellsCallback((int x, int y) cell);

        public void MatrixScanAliveCells(
            MatrixScanAliveCellsCallback callback
            //ForEachNeighbourCallback inside_cells_callback,
            //ForEachNeighbourCallback outside_cells_callback
        ) {
            this.alive_list.ForEach((cell) => {
                for (int x_offset = -1; x_offset <= 1; x_offset += 1) {
                    for (int y_offset = -1; y_offset <= 1; y_offset += 1) {
                        callback((cell.x + x_offset, cell.y + y_offset));
                    }
                }
            });
        }

        public void Draw(
            PaintEventArgs e, Brush brush, Pen pen,
            int cell_size, (int x, int y) offset
        ) {
            this.ForEach((x, y) => {
                Rectangle cellRect = new Rectangle(
                    x * cell_size + offset.x,
                    y * cell_size + offset.y,
                    cell_size, cell_size
                );

                // cell
                this.Get(x, y).Draw(e, brush, cellRect);
                this.Get(x, y).Write(e, brush, cellRect, (x, y).ToString());

                // grid
                e.Graphics.DrawRectangle(pen, cellRect);
            });
        }

        public bool Within(int x, int y, int cell_size, (int x, int y) offset) {
            int x_min = offset.x;
            int y_min = offset.y;
            int x_max = cell_size * this.block_size + offset.x;
            int y_max = cell_size * this.block_size + offset.y;

            return (
                x_min < x && x < x_max &&
                y_min < y && y < y_max
            );
        }

        public (
            Dictionary<(int x, int y), int> inside_cells,
            Dictionary<(int x, int y), int> outside_cells
        ) Next() {
            /**
             * `inside_cells` is the list of cells inside the block that have at least one neighbour,
             * and it includes info of how many neighbours they have.
             * 
             * `outside_cells` is the same but only includes cells outside the block. They are returned
             * for the main program to do something with them.
             */

            // Dictionary of Neighbour to Count
            //var inside_cells = new Dictionary<(int x, int y), int>();
            this.inside_cells.Clear();
            var outside_cells = new Dictionary<(int x, int y), int>();

            this.MatrixScanAliveCells((cell) => {
                // d -> dictionary, c -> cell
                void set_or_increment(Dictionary<(int x, int y), int> d, (int x, int y) c) {
                    if (d.ContainsKey(c))
                    { d[c] += 1; }
                    else
                    { d.Add(c, 1); }
                }

                set_or_increment(this.IsOutsideCell(cell) ? outside_cells : this.inside_cells, cell);
            });

            foreach (var cell in inside_cells) {
                bool cell_is_alive = this.Get(cell.Key).IsAlive;

                // if alive and not (2 or 3) neighbours -> dead
                bool dead_condition = (
                    cell_is_alive == true &&
                    !(cell.Value == 2 || cell.Value == 3)
                );

                // if dead and 3 neighbours -> alive
                bool alive_condition = (
                    cell_is_alive == false &&
                    cell.Value == 3
                );

                if (dead_condition) {
                    this.Set(cell.Key, false);
                } else if (alive_condition) {
                    this.Set(cell.Key, true);
                }
            }

            return (inside_cells, outside_cells);
        }
    }
}
