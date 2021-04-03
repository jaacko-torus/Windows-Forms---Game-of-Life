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

        public bool Bounding(Vector2 cell) => (
            0 <= cell.X && cell.X < this.block_size &&
            0 <= cell.Y && cell.Y < this.block_size
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



        public void Toggle(Vector2 cell) {
            this.Get(cell).Toggle();

            if (this.Get(cell).IsAlive) {
                this.alive_list.Add(cell);
            } else {
                this.alive_list.Remove(cell);
            }
        }



        public delegate void ForEachCallback(Vector2 cell);

        public void ForEach(ForEachCallback callback) {
            for (int x = 0; x < this.cells.GetLength(0); x += 1) {
                for (int y = 0; y < this.cells.GetLength(1); y += 1) {
                    callback(new Vector2(x, y));
                }
            }
        }

        


        public int CountCellNeighbours(Vector2 cell) {
            // NOTE: using `return` for clarity
            return UMatrix.Reduce3x3Matrix(cell, (total, curr_neighbour) => {
                // Don't count if self, not in block, or dead. Short circuiting works as early return.
                return curr_neighbour == cell || !this.Bounding(curr_neighbour) || !this.Get(curr_neighbour).IsAlive
                    ? total
                    : total + 1;
            }, 0);
        }

        public void CountAndSetCellNeighbours(Vector2 cell) {
            int neighbour_count = this.CountCellNeighbours(cell);

            // NOTE: I should count neighbours from cells that are outsider cells
            // so that I can return that data to the right cell.
            if (this.Bounding(cell)) {
                this.inner_cells[cell] = neighbour_count;
                this.Get(cell).neighbours = neighbour_count;
            } else {
                this.outer_cells[cell] = neighbour_count;
            }
        }

        public void CountAndSetInnerNeighbours(List<Vector2> cells_to_be_counted_and_set) {
            this.inner_cells.Keys.ToList()
                .Intersect(cells_to_be_counted_and_set).ToList()
                .ForEach(this.CountAndSetCellNeighbours);
        }

        public void CountAndSetAllInnerNeighbours() {
            this.inner_cells.Keys.ToList()
                .ForEach(this.CountAndSetCellNeighbours);
        }



        public void ResetCellNeighbours(Vector2 cell) {
            UMatrix.ForEach3x3Matrix(cell, (neighbour) => {
                this.inner_cells[neighbour] = 0;
            });
        }

        public void ResetAliveNeighbours(List<Vector2> cells_to_be_reset) {
            this.alive_list.ToList()
                .Intersect(cells_to_be_reset).ToList()
                .ForEach(this.ResetCellNeighbours);
        }

        public void ResetAllAliveNeighbours() {
            this.alive_list.ToList()
                .ForEach(this.ResetCellNeighbours);
        }
        


        public void Reset() {
            this.ResetAllAliveNeighbours();

            this.alive_list.Clear();
            this.inner_cells.Clear();
            this.outer_cells.Clear();
        }



        public void SetNextState(Dictionary<Vector2, int> cells_to_be_set) {
            cells_to_be_set.ForEach((neighbour) => {
                if (this.Bounding(neighbour)) {
                    bool next_state = this.Get(neighbour).NextState(cells_to_be_set[neighbour]);

                    this.Get(neighbour).Set(next_state);

                    if (next_state == true) {
                        alive_list.Add(neighbour);
                    } else {
                        alive_list.Remove(neighbour);
                    }
                } else {
                    // nothing for now
                }
            });
        }

        public void SetInnerNextState() => this.SetNextState(this.inner_cells);

        public void ReCountCellList(Dictionary<Vector2, int> recounted_cell_list) {
            this.SetNextState(recounted_cell_list);
        }

        

        public void Draw(PaintEventArgs e, int cell_size, Vector2 offset) {
            /**
             * TODO: optimize
             * at the moment I'm drawing everything once a frame,
             * including parts of the screen which cannot be seen in the window
             * and I'm drawing rectangles instead of lines
             * I'm also redrawing anything even if it didnt change
             * 
             * - [ ] only draw within viewport
             * - [ ] draw lines spaning screen instead of separate rectangles
             * - [ ] redraw grid only after camera move
             * - [ ] redraw rectangle fills only after a the cell's state changes
             * 
             * This should significantly improve performance,
             * even tho performance is perfectly fine as it is right now :P
             */
            this.ForEach((cell) => {
                var position = cell * cell_size + offset;

                var cell_rectangle = new Rectangle(
                    (int)position.X, (int)position.Y,
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
            // set all cells according to their last neighbour state and add/remove from `this.alive_list`
            this.SetInnerNextState();
            // set all cell neighbours to 0
            this.ResetAllAliveNeighbours();
            // count up all inner neighbours
            this.CountAndSetAllInnerNeighbours();
            // clean up `block.inner_cells` to make sure we don't iterate over too much
            this.inner_cells = this.inner_cells.ToList()
                .Where(cell_kv => cell_kv.Value != 0).ToDictionary();
        }
    }
}
