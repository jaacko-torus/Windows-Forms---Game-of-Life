using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics;

namespace month_6_Project_and_Portfolio_I {
    static class Universe {
        private static GraphicsPanel graphics_panel;

        private static Vector2 client_size =>
            new Vector2(Universe.graphics_panel.ClientSize.Width, Universe.graphics_panel.ClientSize.Height);

        private static float zoom = 10;
        
        private static float _cell_size;
        private static float cell_size {
            get => Universe._cell_size;
            set {
                Universe._cell_size = value;
                Cell.font = new Font("Arial", Universe._cell_size / 3f);
            }
        
        }

        public static Vector2 offset;

        public static int generation = 0;

        // only alive cells
        private static HashSet<Vector2> alive = new HashSet<Vector2>();
        // all cells that need to be kept track of, alive + alive neighbors
        private static Dictionary<Vector2, Cell> map = new Dictionary<Vector2, Cell>();

        // Drawing colors, Pallette https://www.nordtheme.com/docs/colors-and-palettes
        public static Dictionary<string, Color> colors = new Dictionary<string, Color>() {
            // #3B4252
            { "grid",                Color.FromArgb(0x3B, 0x42, 0x52) },
            // #D0707F a darker `colors["cell"]` not in pallette
            { "cell_adjecent_color", Color.FromArgb(0xD0, 0x70, 0x7F) },
            // #BF616A
            { "cell",                Color.FromArgb(0xBF, 0x61, 0x6A) },
            // #ECEFF4
            { "cell_text",           Color.FromArgb(0xEC, 0xEF, 0xF4) }
        };

        public static void Start(GraphicsPanel graphics_panel) {
            Universe.graphics_panel = graphics_panel;

            Universe.cell_size = Universe.DefaultCellSize(graphics_panel);

            Universe.offset = Universe.DefaultOffset(graphics_panel);

            Cell.font_brush = new SolidBrush(Universe.colors["cell_text"]);
            Cell.cell_brush = new SolidBrush(Universe.colors["cell"]);

            Cell.font_string_format = new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
        }

        // default values

        private static float DefaultCellSize(GraphicsPanel graphics_panel) =>
            Math.Min(Universe.client_size.X, Universe.client_size.Y) / Universe.zoom;

        private static Vector2 DefaultOffset(GraphicsPanel graphics_panel) => Universe.client_size / 2;


        // cell actions


        private static void ApplyRules() {
            Universe.map.ForEach((cell_id, cell) => {
                // set cell to its next state
                cell.Next();

                if (cell.is_alive) {
                    // spawn any cells that are needed
                    Universe.SpawnCellNeighbors(cell_id);
                    // add to alive list
                    Universe.alive.Add(cell_id);
                } else {
                    // remove from alive list
                    Universe.alive.Remove(cell_id);
                }
            });
        }

        private static void SpawnCell(Vector2 cell) {
            Universe.map[cell] = new Cell(true);
            Universe.alive.Add(cell);
        }

        private static void SpawnCellNeighbors(Vector2 cell) {
            UMatrix.ForEach3x3Matrix(cell, (neighbor) => {
                if (cell != neighbor && !Universe.map.ContainsKey(neighbor)) {
                    Universe.map[neighbor] = new Cell(false);
                }
            });
        }

        private static void RemoveIslandCell(Vector2 cell) {
            if (Universe.map.ContainsKey(cell) && !Universe.map[cell].is_alive && !Universe.map[cell].has_neighbors) {
                Universe.map.Remove(cell);
            }
        }

        private static void RemoveIslandCellNeighbors(Vector2 cell) =>
            UMatrix.ForEach3x3Matrix(cell, Universe.RemoveIslandCell);


        // cell counting


        private static int CountCellNeighbours(Vector2 cell) {
            // NOTE: using `return` for clarity
            return UMatrix.Reduce3x3Matrix(cell, (total, curr_neighbour) => {
                // count if not self, I'm keeping track of it, and is alive. Short circuiting `isAlive`.
                return curr_neighbour != cell && Universe.map.ContainsKey(curr_neighbour) && Universe.map[curr_neighbour].is_alive
                    ? total + 1
                    : total;
            }, 0);
        }

        private static void CountAndSetCellNeighbours(Vector2 cell) =>
            Universe.map[cell].neighbors = Universe.CountCellNeighbours(cell);

        private static void CountAndSetAllCells() =>
            Universe.map.ForEach(Universe.CountAndSetCellNeighbours);


        // drawing


        private static void DrawGrid(PaintEventArgs e) {
            (PointF p1, PointF p2) get_x_line(float offset_x) => (
                new Vector2(offset_x, 0).ToPointF(),
                new Vector2(offset_x, Universe.client_size.Y).ToPointF()
            );

            (PointF p1, PointF p2) get_y_line(float offset_y) => (
                new Vector2(0, offset_y).ToPointF(),
                new Vector2(Universe.client_size.X, offset_y).ToPointF()
            );

            var grid_pen = new Pen(Universe.colors["grid"], 1);

            var virtual_window =
                Universe.client_size + new Vector2(Universe.cell_size) -
                UVector2.mod2(Universe.client_size, Universe.cell_size);

            var total_lines = virtual_window / Universe.cell_size;
            var biggest_axis = Math.Max(total_lines.X, total_lines.Y);

            for (int n = 0; n < biggest_axis; n += 1) {
                var offset = UVector2.mod3(new Vector2(n * Universe.cell_size) + Universe.offset, virtual_window);

                if (offset.X < Universe.client_size.X) {
                    var x_line = get_x_line(offset.X);
                    e.Graphics.DrawLine(grid_pen, x_line.p1, x_line.p2);
                }

                if (offset.Y < Universe.client_size.Y) {
                    var y_line = get_y_line(offset.Y);
                    e.Graphics.DrawLine(grid_pen, y_line.p1, y_line.p2);
                }
            }

            grid_pen.Dispose();
        }


        // interaction


        private static Vector2 FindClickedCell(Vector2 mouse) =>
            UVector2.Floor((mouse - Universe.offset) / Universe.cell_size);

        private static void Toggle(Vector2 cell) {
            Universe.map[cell].Toggle();

            if (Universe.map[cell].is_alive) {
                Universe.alive.Add(cell);
            } else {
                Universe.alive.Remove(cell);
            }
        }


        // window interface


        public static void Reset() {
            Universe.map.Clear();
            Universe.alive.Clear();
        }

        public static void Next(ToolStripStatusLabel generation_gui) {
            Universe.ApplyRules();
            Universe.CountAndSetAllCells();
            Universe.map.ForEach(Universe.RemoveIslandCell);

            Universe.generation += 1;

            // Update generation in GUI
            generation_gui.Text = $"Generation {Universe.generation}";
        }

        public static void Draw(PaintEventArgs e) {
            // TODO: only paint and write cells that are visible
            Universe.alive.ForEach((cell) => {
                Universe.map[cell].Paint(e, new RectangleF(
                    (cell * Universe.cell_size + Universe.offset).ToPointF(),
                    new Vector2(Universe.cell_size).ToSizeF()
                ));
            });

            Universe.map.ForEach((cell) => {
                Universe.map[cell].Write(e, new RectangleF(
                    (cell * Universe.cell_size + Universe.offset).ToPointF(),
                    new Vector2(Universe.cell_size).ToSizeF()
                ));
            });

            Universe.DrawGrid(e);
        }

        public static void ToggleAtMousePosition(Vector2 mouse_position) {
            var cell = FindClickedCell(mouse_position);

            if (Universe.map.ContainsKey(cell)) {
                // toggle
                Universe.Toggle(cell);
            } else {
                // make
                Universe.SpawnCell(cell);
            }

            // spawn any neighbors if they're not there
            Universe.SpawnCellNeighbors(cell);

            // recount neighbours
            UMatrix.ForEach3x3Matrix(cell, Universe.CountAndSetCellNeighbours);

            // cleanup any dead cells with no neighbours
            Universe.RemoveIslandCellNeighbors(cell);
        }
    }
}
