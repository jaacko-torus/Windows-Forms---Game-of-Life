using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics;

namespace month_6_Project_and_Portfolio_I {
    class _Universe {
        
        public static int block_size;

        private static int _cell_size;
        public static int cell_size {
            get => _Universe._cell_size;
            set {
                _Universe._cell_size = value;
                Cell.font = new Font("Arial", _Universe._cell_size / 3f);
            }
        }
        public static Vector2 offset;

        public static int generation = 0;

        public static Dictionary<Vector2, Cell> map = new Dictionary<Vector2, Cell>();

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

        // getters

        public static int real_block_size { get => _Universe.cell_size * _Universe.block_size; }

        public static Vector2 center {
            get => _Universe.offset - new Vector2(_Universe.real_block_size) / 2;
        }

        // Initializer

        public static GraphicsPanel graphics_panel;

        public static void Start(GraphicsPanel graphics_panel) {
            _Universe.graphics_panel = graphics_panel;

            _Universe.block_size = 10;

            _Universe.cell_size = _Universe.DefaultCellSize(graphics_panel);
            //_Universe.offset = _Universe.DefaultOffset(graphics_panel);

            //UMatrix.ForEach3x3Matrix(new Vector2(0, 0), (block_coords) => {
            //    _Universe.map.Add(block_coords, new Block(block_coords, _Universe.block_size, _Universe.colors));
            //});
        }

        // methods

        // getters

        public static Vector2 ClientSize =>
            new Vector2(_Universe.graphics_panel.ClientSize.Width, _Universe.graphics_panel.ClientSize.Height);

        public static int DefaultCellSize(GraphicsPanel graphics_panel) => (int)Math.Min(
            _Universe.ClientSize.X,
            _Universe.ClientSize.Y
        ) / _Universe.block_size;

        public static Vector2 DefaultOffset(GraphicsPanel graphics_panel) =>
            (_Universe.ClientSize - new Vector2(Universe.real_block_size)) / 2;

        private static Vector2 FindClickedBlock(Vector2 mouse) => UVector2.Floor(
            (mouse - _Universe.offset) / _Universe.real_block_size
        );

        // TODO: not sure if I should be using `mod2` or `mod3` here.
        private static Vector2 FindClickedCell(Vector2 mouse) => UVector2.Floor(
            UVector2.mod3(mouse - _Universe.offset, _Universe.real_block_size) / _Universe.cell_size
        );

        // side_effects

        public static void DrawAxisLines(PaintEventArgs e, Vector2 window) {
            (PointF p1, PointF p2) get_x_line(float offset_x) => (
                new Vector2(offset_x, 0).ToPointF(),
                new Vector2(offset_x, _Universe.ClientSize.Y).ToPointF()
            );

            (PointF p1, PointF p2) get_y_line(float offset_y) => (
                new Vector2(0, offset_y).ToPointF(),
                new Vector2(_Universe.ClientSize.X, offset_y).ToPointF()
            );

            var grid_pen = new Pen(_Universe.colors["grid"], 1);

            var total_lines = window / _Universe.cell_size;
            var biggest_axis = Math.Max(total_lines.X, total_lines.Y);

            for (int n = 0; n < biggest_axis; n += 1) {
                var offset = UVector2.mod3(new Vector2(n * _Universe.cell_size) + _Universe.offset, window);

                if (offset.X < _Universe.ClientSize.X) {
                    var x_line = get_x_line(offset.X);
                    e.Graphics.DrawLine(grid_pen, x_line.p1, x_line.p2);
                }

                if (offset.Y < _Universe.ClientSize.Y) {
                    var y_line = get_y_line(offset.Y);
                    e.Graphics.DrawLine(grid_pen, y_line.p1, y_line.p2);
                }
            }

            grid_pen.Dispose();
        }

        public static void Draw(PaintEventArgs e) {
            var virtual_window =
                _Universe.ClientSize + new Vector2(_Universe.cell_size) -
                UVector2.mod2(_Universe.ClientSize, _Universe.cell_size);

            _Universe.DrawAxisLines(e, virtual_window);
        }

        // Calculate the next generation of cells
        public static void Next(ToolStripStatusLabel generation_gui) {
            // advance every block
            //_Universe.map.Values.ToList().ForEach(block => block.Next());

            _Universe.generation += 1;

            // Update generation in GUI
            generation_gui.Text = $"Generation {_Universe.generation}";
        }

        public static void Reset() {
            //_Universe.map.Keys.ToList().ForEach(block => _Universe.map[block].Reset());

            // TODO: this resets all blocks but does not remove any unneeded blocks.
        }

        public static void ToggleCellAtMousePosition(Vector2 mouse_position) {

            Console.WriteLine(mouse_position);

            return;

            //var block = _Universe.FindClickedBlock(mouse_position);
            //var cell = _Universe.FindClickedCell(mouse_position);

            ///**
            //  * NOTE: When recounting neighbours, I can't use `CountAndSetCellNeighbours`
            //  * since it's a wrapper for `cell.neighbours = <int>` with error correction.
            //  * Instead I need to find neighbours myself by matrix scanning.
            //  */

            //if (_Universe.map.ContainsKey(block)) {
            //    // toggle
            //    _Universe.map[block].Toggle(cell);
            //    // reset neighbours
            //    _Universe.map[block].ResetCellNeighbours(cell);
            //    // recount neighbours
            //    UMatrix.ForEach3x3Matrix(cell, _Universe.map[block].CountAndSetCellNeighbours);
            //};
        }
    }
}
