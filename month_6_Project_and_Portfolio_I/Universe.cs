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
        public static int block_size;

        public static int cell_size {
            get => Universe._cell_size;
            set {
                Universe._cell_size = value;
                Cell.font = new Font("Arial", Universe.cell_size / 3f);
            }
        }
        public static Vector2 offset;

        public static int generation = 0;

        public static Dictionary<Vector2, Cell> _map = new Dictionary<Vector2, Cell>();

        public static Dictionary<Vector2, Block> map {
            get;
            private set;
        } = new Dictionary<Vector2, Block>();

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

        public static int real_block_size { get => Universe.cell_size * Universe.block_size; }

        public static Vector2 center {
            get => Universe.offset - new Vector2(Universe.real_block_size) / 2;
        }

        // private

        private static int _cell_size;

        // Initializer

        public static void Start(GraphicsPanel graphics_panel) {
            Universe.block_size = 10;

            Universe.cell_size = Universe.DefaultCellSize(graphics_panel);
            Universe.offset = Universe.DefaultOffset(graphics_panel);

            UMatrix.ForEach3x3Matrix(new Vector2(0, 0), (block_coords) => {
                Universe.map.Add(block_coords, new Block(block_coords, Universe.block_size, Universe.colors));
            });
        }

        // methods

        // getters

        public static Vector2 ClientSize(GraphicsPanel graphics_panel) =>
            new Vector2(graphics_panel.ClientSize.Width, graphics_panel.ClientSize.Height);

        public static int DefaultCellSize(GraphicsPanel graphics_panel) => (int)Math.Min(
            Universe.ClientSize(graphics_panel).X,
            Universe.ClientSize(graphics_panel).Y
        ) / Universe.block_size;

        public static Vector2 DefaultOffset(GraphicsPanel graphics_panel) =>
            (Universe.ClientSize(graphics_panel) - new Vector2(Universe.real_block_size)) / 2;

        private static Vector2 FindClickedBlock(Vector2 mouse) => UVector2.Floor(
            (mouse - Universe.offset) / Universe.real_block_size
        );

        // TODO: not sure if I should be using `mod2` or `mod3` here.
        private static Vector2 FindClickedCell(Vector2 mouse) => UVector2.Floor(
            UVector2.mod3(mouse - Universe.offset, Universe.real_block_size) / Universe.cell_size
        );

        // side_effects

        public static void Draw(PaintEventArgs e) {
            Universe.map.Values.ToList().ForEach((block) => {
                block.Draw(e, Universe.cell_size, block.coord_id * Universe.real_block_size + Universe.offset);
            });
        }

        // Calculate the next generation of cells
        public static void Next(ToolStripStatusLabel generation_gui) {
            // advance every block
            Universe.map.Values.ToList().ForEach(block => block.Next());

            Universe.generation += 1;

            // Update generation in GUI
            generation_gui.Text = $"Generation {Universe.generation}";
        }

        public static void Reset() {
            Universe.map.Keys.ToList().ForEach(block => Universe.map[block].Reset());

            // TODO: this resets all blocks but does not remove any unneeded blocks.
        }

        public static void ToggleCellAtMousePosition(Vector2 mouse_position) {
            var block = Universe.FindClickedBlock(mouse_position);
            var cell = Universe.FindClickedCell(mouse_position);

            /**
              * NOTE: When recounting neighbours, I can't use `CountAndSetCellNeighbours`
              * since it's a wrapper for `cell.neighbours = <int>` with error correction.
              * Instead I need to find neighbours myself by matrix scanning.
              */

            if (Universe.map.ContainsKey(block)) {
                // toggle
                Universe.map[block].Toggle(cell);
                // reset neighbours
                Universe.map[block].ResetCellNeighbours(cell);
                // recount neighbours
                UMatrix.ForEach3x3Matrix(cell, Universe.map[block].CountAndSetCellNeighbours);
            };
        }
    }
}
