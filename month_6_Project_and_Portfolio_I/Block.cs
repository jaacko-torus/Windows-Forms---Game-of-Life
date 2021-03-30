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
        private Dictionary<(int x, int y), int> inner_cells = new Dictionary<(int x, int y), int>();

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

        public void Scan3x3Matrix(ScanMatrixCallback callback) {
            for (int x_offset = -1; x_offset <= 1; x_offset += 1) {
                for (int y_offset = -1; y_offset <= 1; y_offset += 1) {
                    if (x_offset == 0 && y_offset == 0) { continue; }

                    callback((x_offset, y_offset));
                }
            }
        }

        public int CountNeighbours((int x, int y) cell) {
            int count = 0;

            Scan3x3Matrix((offset) => {
                var curr_neighbour = (cell.x + offset.x, cell.y + offset.y);

                if (IsOutsideBlock(curr_neighbour)) {
                    // if outside of block, fetch the right block
                    // and get state of cell
                    // otherwise assume dead
                } else if (this.Get(curr_neighbour).IsAlive) {
                    count += 1;
                }
            });

            return count;
        }

        public void CountAllNeighbours() {
            this.alive_list.Distinct().ToList().ForEach((alive_cell) => {
                Scan3x3Matrix((offset) => {
                    var neighbour = (alive_cell.x + offset.x, alive_cell.y + offset.y);
                    this.inner_cells[neighbour] = 0;
                });
            });

            this.inner_cells.ToList().ForEach((neighbour) => {
                if (IsOutsideBlock(neighbour.Key)) {
                    // nothing for now
                } else {
                    this.inner_cells[neighbour.Key] = CountNeighbours(neighbour.Key);
                    this.Get(neighbour.Key).neighbours = this.inner_cells[neighbour.Key];
                }
            });
        }

        public void Next() {
            Console.Clear();

            CountAllNeighbours();

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

            CountAllNeighbours();
        }
    }
}
