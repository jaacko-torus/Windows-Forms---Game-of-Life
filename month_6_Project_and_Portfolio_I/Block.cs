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
        
        public HashSet<(int x, int y)> alive_list = new HashSet<(int x, int y)>();

        private Dictionary<(int x, int y), int> inner_cells = new Dictionary<(int x, int y), int>();
        private Dictionary<(int x, int y), int> outer_cells = new Dictionary<(int x, int y), int>();

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

        public Cell Get((int x, int y) cell) => this.cells[cell.x, cell.y];
        public void Set((int x, int y) cell, bool value) { this.cells[cell.x, cell.y].Set(value); }

        public void Toggle((int x, int y) cell) {
            this.cells[cell.x, cell.y].Toggle();

            if (this.cells[cell.x, cell.y].IsAlive) {
                this.alive_list.Add(cell);
            } else {
                this.alive_list.Remove(cell);
            }
        }

        public bool IsOutsideBlock((int x, int y) cell) => (
            cell.x < 0 || this.block_size <= cell.x ||
            cell.y < 0 || this.block_size <= cell.y
        );

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
                this.Get((x, y)).Draw(e, brush, cellRect);
                this.Get((x, y)).Write(e, brush, cellRect);

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

        public delegate void ScanMatrixCallback((int x, int y) offset);

        public static void Scan3x3Matrix((int x, int y) curr_cell, ScanMatrixCallback callback) {
            for (int x_offset = -1; x_offset <= 1; x_offset += 1) {
                for (int y_offset = -1; y_offset <= 1; y_offset += 1) {
                    callback((curr_cell.x + x_offset, curr_cell.y + y_offset));
                }
            }
        }

        public int CountCellNeighbours((int x, int y) cell) {
            int count = 0;

            Block.Scan3x3Matrix(cell, (curr_neighbour) => {
                // NOTE: below line only works because equality for Tuples works as expected
                if (curr_neighbour == cell) { return; }
                
                bool is_alive_inner_cell = !IsOutsideBlock(curr_neighbour) && this.Get(curr_neighbour).IsAlive;

                if (is_alive_inner_cell) { count += 1; }
            });

            return count;
        }

        public void ResetNeighbours() {
            this.alive_list.ToList().ForEach((alive_cell) => {
                Block.Scan3x3Matrix(alive_cell, (neighbour) => {
                    this.inner_cells[neighbour] = 0;
                });
            });
        }

        // CountAndSetNeighbours
        public void CountAndSetNeighbours() {
            this.inner_cells.Keys.ToList().ForEach((neighbour) => {
                int neighbour_count = CountCellNeighbours(neighbour);

                if (IsOutsideBlock(neighbour)) {
                    // NOTE: I should count neighbours from cells that are outsider cells
                    // so that I can return that data to the right cell.
                    this.outer_cells[neighbour] = neighbour_count;
                } else {
                    this.inner_cells[neighbour] = neighbour_count;
                    this.Get(neighbour).neighbours = neighbour_count;
                }
            });
        }

        public void Next() {
            this.inner_cells.Keys.ToList().ForEach((neighbour) => {
                if (IsOutsideBlock(neighbour)) {
                    // nothing for now
                } else {
                    int neighbour_count = this.inner_cells[neighbour];
                    bool next_state = this.Get(neighbour).StateFromNeighbours(neighbour_count);
                    this.Get(neighbour).Set(next_state);

                    if (next_state == true) {
                        alive_list.Add(neighbour);
                    } else {
                        alive_list.Remove(neighbour);
                    }
                }
            });

            this.inner_cells.ToList().ForEach((neighbour) => {
                if (IsOutsideBlock(neighbour.Key)) {
                    // nothing for now
                } else {
                    bool next_state = this.Get(neighbour.Key).StateFromNeighbours(neighbour.Value);
                    this.Get(neighbour.Key).Set(next_state);

                    if (next_state == true) {
                        alive_list.Add(neighbour.Key);
                    } else {
                        alive_list.Remove(neighbour.Key);
                    }
                }
            });

            this.ResetNeighbours();
            this.CountAndSetNeighbours();
        }
    }
}
