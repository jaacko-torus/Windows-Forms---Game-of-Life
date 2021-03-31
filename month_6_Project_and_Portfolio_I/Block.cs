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

        /**
         * NOTE: inner and outer cells
         * These refer to cells that are or are neighbours of alive cells.
         * Inner are those within a block
         * Outer are those outside a block
         */
        private Dictionary<(int x, int y), int> inner_cells = new Dictionary<(int x, int y), int>();
        private Dictionary<(int x, int y), int> outer_cells = new Dictionary<(int x, int y), int>();

        // constructor

        public Block(int x_position, int y_position, int block_size) {
            this.coord_id = (x_position, y_position);
            this.block_size = block_size;
            this.cells = new Cell[block_size, block_size];

            Cell.font_string_format = new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            this.ForEach((cell) => this.cells[cell.x, cell.y] = new Cell(cell, false));
        }

        // methods and delegates

        // getters

        public Cell Get((int x, int y) cell) => this.cells[cell.x, cell.y];

        public bool IsOutsideBlock((int x, int y) cell) => (
            cell.x < 0 || this.block_size <= cell.x ||
            cell.y < 0 || this.block_size <= cell.y
        );

        public bool Within((int x, int y) mouse, int cell_size, (int x, int y) offset) {
            int x_min = offset.x;
            int y_min = offset.y;
            int x_max = cell_size * this.block_size + offset.x;
            int y_max = cell_size * this.block_size + offset.y;

            return (
                x_min < mouse.x && mouse.x < x_max &&
                y_min < mouse.y && mouse.y < y_max
            );
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

        // setters

        public void Set((int x, int y) cell, bool value) { this.cells[cell.x, cell.y].Set(value); }

        public void Toggle((int x, int y) cell) {
            this.cells[cell.x, cell.y].Toggle();

            if (this.cells[cell.x, cell.y].IsAlive) {
                this.alive_list.Add(cell);
            } else {
                this.alive_list.Remove(cell);
            }
        }

        public void ResetCellNeighbours((int x, int y) cell) {
            Block.Scan3x3Matrix(cell, (neighbour) => {
                this.inner_cells[neighbour] = 0;
            });
        }

        public void CountAndSetCellNeighbours((int x, int y) cell) {
            int neighbour_count = CountCellNeighbours(cell);

            // NOTE: I should count neighbours from cells that are outsider cells
            // so that I can return that data to the right cell.
            if (IsOutsideBlock(cell)) {
                this.outer_cells[cell] = neighbour_count;
            } else {
                this.inner_cells[cell] = neighbour_count;
                this.Get(cell).neighbours = neighbour_count;
            }
        }

        // side effects

        public delegate void ForEachCallback((int x, int y) cell);

        public void ForEach(ForEachCallback callback) {
            for (int x = 0; x < this.cells.GetLength(0); x += 1) {
                for (int y = 0; y < this.cells.GetLength(1); y += 1) {
                    callback((x, y));
                }
            }
        }

        public delegate void ScanMatrixCallback((int x, int y) offset);

        public static void Scan3x3Matrix((int x, int y) curr_cell, ScanMatrixCallback callback) {
            for (int x_offset = -1; x_offset <= 1; x_offset += 1) {
                for (int y_offset = -1; y_offset <= 1; y_offset += 1) {
                    callback((curr_cell.x + x_offset, curr_cell.y + y_offset));
                }
            }
        }

        public void ResetNeighbours() {
            this.alive_list.ToList().ForEach((alive_cell) => {
                this.ResetCellNeighbours(alive_cell);
            });
        }

        public void CountAndSetNeighbours() {
            this.inner_cells.Keys.ToList().ForEach((neighbour) => {
                this.CountAndSetCellNeighbours(neighbour);
            });
        }

        public void Draw(
            PaintEventArgs e,
            Dictionary<string, Color> colors,
            Brush brush, Pen pen,
            int cell_size, (int x, int y) offset
        ) {
            Cell.font_brush = new SolidBrush(colors["cell_text"]);
            Cell.font = new Font("Arial", cell_size / 3f);

            Cell.cell_brush = brush;

            this.ForEach((cell) => {
                Rectangle cell_rectangle = new Rectangle(
                    cell.x * cell_size + offset.x,
                    cell.y * cell_size + offset.y,
                    cell_size, cell_size
                );

                // cell
                this.Get(cell).Draw(e, cell_rectangle);
                this.Get(cell).Write(e, cell_rectangle);

                // grid
                e.Graphics.DrawRectangle(pen, cell_rectangle);
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

            this.ResetNeighbours();
            this.CountAndSetNeighbours();
        }
    }
}
