using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;

namespace month_6_Project_and_Portfolio_I {
    class Block {
        public static Pen grid_pen;

        public readonly Vector2 coord_id;
        public readonly int block_size;

        public Cell[,] cells;        
        public HashSet<Vector2> alive_list = new HashSet<Vector2>();

        /**
         * NOTE: inner and outer cells
         * These refer to cells that are or are neighbours of alive cells.
         * Inner are those within a block
         * Outer are those outside a block
         */
        private Dictionary<Vector2, int> inner_cells = new Dictionary<Vector2, int>();
        private Dictionary<Vector2, int> outer_cells = new Dictionary<Vector2, int>();

        // constructor

        public Block(Vector2 position, int block_size, Dictionary<string, Color> colors) {
            this.coord_id = position;
            this.block_size = block_size;
            this.cells = new Cell[block_size, block_size];

            Block.grid_pen = new Pen(Universe.colors["grid"], 1);

            Cell.font_brush = new SolidBrush(Universe.colors["cell_text"]);
            Cell.cell_brush = new SolidBrush(Universe.colors["cell"]);

            Cell.font_string_format = new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            this.ForEach((cell) => this.cells[(int)cell.X, (int)cell.Y] = new Cell(cell, false));
        }

        // methods and delegates

        // getters

        public Cell Get(Vector2 cell) => this.cells[(int)cell.X, (int)cell.Y];

        public bool IsOutsideBlock(Vector2 cell) => (
            cell.X < 0 || this.block_size <= cell.X ||
            cell.Y < 0 || this.block_size <= cell.Y
        );

        public bool Within(Vector2 mouse, int real_block_size, Vector2 offset) {
            var grid_offset = this.coord_id * real_block_size;
            var total_offset = offset + grid_offset;

            var min = total_offset;
            var max = new Vector2(real_block_size) + total_offset;

            return (
                min.X < mouse.X && mouse.X < max.X &&
                min.Y < mouse.Y && mouse.Y < max.Y
            );
        }

        public int CountCellNeighbours(Vector2 cell) {
            int count = 0;

            Block.Scan3x3Matrix(cell, (curr_neighbour) => {
                // NOTE: below line only works because equality for Tuples works as expected
                if (curr_neighbour == cell) { return; }
                
                bool is_alive_inner_cell = !this.IsOutsideBlock(curr_neighbour) && this.Get(curr_neighbour).IsAlive;

                if (is_alive_inner_cell) { count += 1; }
            });

            return count;
        }

        // setters

        public void Set(Vector2 cell, bool value) {
            this.cells[(int)cell.X, (int)cell.Y].Set(value);
        }

        public void Toggle(Vector2 cell) {
            this.Get(cell).Toggle();

            if (this.Get(cell).IsAlive) {
                this.alive_list.Add(cell);
            } else {
                this.alive_list.Remove(cell);
            }
        }

        public void ResetCellNeighbours(Vector2 cell) {
            Block.Scan3x3Matrix(cell, (neighbour) => {
                this.inner_cells[neighbour] = 0;
            });
        }

        public void CountAndSetCellNeighbours(Vector2 cell) {
            int neighbour_count = this.CountCellNeighbours(cell);

            // NOTE: I should count neighbours from cells that are outsider cells
            // so that I can return that data to the right cell.
            if (this.IsOutsideBlock(cell)) {
                this.outer_cells[cell] = neighbour_count;
            } else {
                this.inner_cells[cell] = neighbour_count;
                this.Get(cell).neighbours = neighbour_count;
            }
        }

        // side effects

        public delegate void ForEachCallback(Vector2 cell);

        public void ForEach(ForEachCallback callback) {
            for (int x = 0; x < this.cells.GetLength(0); x += 1) {
                for (int y = 0; y < this.cells.GetLength(1); y += 1) {
                    callback(new Vector2(x, y));
                }
            }
        }

        public delegate void ScanMatrixCallback(Vector2 offset);

        public static void Scan3x3Matrix(Vector2 curr_cell, ScanMatrixCallback callback) {
            for (int x_offset = -1; x_offset <= 1; x_offset += 1) {
                for (int y_offset = -1; y_offset <= 1; y_offset += 1) {
                    callback(curr_cell + new Vector2(x_offset, y_offset));
                }
            }
        }

        public void ResetNeighbours() {
            this.alive_list.ToList().ForEach(this.ResetCellNeighbours);
        }

        public void Reset() {
            this.ResetNeighbours();

            this.alive_list.Clear();
            this.inner_cells.Clear();
            this.outer_cells.Clear();
        }

        public void CountAndSetNeighbours() {
            this.inner_cells.Keys.ToList().ForEach(this.CountAndSetCellNeighbours);
        }

        public void Draw(
            PaintEventArgs e,
            int cell_size, Vector2 offset
        ) {
            this.ForEach((cell) => {
                var position = cell * cell_size + offset;

                var cell_rectangle = new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    cell_size, cell_size
                );

                // cell
                this.Get(cell).Draw(e, cell_rectangle);
                this.Get(cell).Write(e, cell_rectangle);

                // grid
                e.Graphics.DrawRectangle(Block.grid_pen, cell_rectangle);
            });
        }

        public void Next() {
            this.inner_cells.Keys.ToList().ForEach((neighbour) => {
                if (this.IsOutsideBlock(neighbour)) {
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
