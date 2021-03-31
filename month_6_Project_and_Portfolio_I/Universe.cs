using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

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
        public static (int x, int y) offset;

        public static int generation = 0;

        public static Dictionary<(int x, int y), Block> map {
            get;
            private set;
        } = new Dictionary<(int x, int y), Block>();

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

        public static (int x, int y) center {
            get => (
                Universe.offset.x - (Universe.cell_size * Universe.block_size) / 2,
                Universe.offset.y - (Universe.cell_size * Universe.block_size) / 2
            );
        }

        // private

        private static int _cell_size;

        // Initializer

        public static void Start(GraphicsPanel graphics_panel) {
            Universe.block_size = 10;

            Universe.cell_size = Universe.DefaultCellSize(graphics_panel);
            Universe.offset = Universe.DefaultOffset(graphics_panel);

            Block.Scan3x3Matrix((0, 0), (block_coords) => {
                Universe.map.Add(block_coords, new Block(block_coords, Universe.block_size, Universe.colors));
            });
        }

        // helper

        // getters

        public static int DefaultCellSize(GraphicsPanel graphics_panel) => Math.Min(
            graphics_panel.ClientSize.Width / Universe.block_size,
            graphics_panel.ClientSize.Height / Universe.block_size
        );

        public static (int x, int y) DefaultOffset(GraphicsPanel graphics_panel) => (
            (graphics_panel.ClientSize.Width - (Universe.cell_size * Universe.block_size)) / 2,
            (graphics_panel.ClientSize.Height - (Universe.cell_size * Universe.block_size)) / 2
        );

        // https://en.wikipedia.org/wiki/Modulo_operation
        public static int mod1(int a, int b) => a - b * (a / b);
        public static int mod2(int a, int b) => a - b * (int)Math.Floor((double)a / (double)b);
        public static int mod3(int a, int b) => a - (int)Math.Abs(b) * (int)Math.Floor((double)a / (double)Math.Abs(b));
        public static int block_real_size { get => Universe.cell_size * Universe.block_size; }

        private static (int x, int y) FindClickedCell((int x, int y) mouse) {
            return (
                mod3((mouse.x - Universe.offset.x), Universe.block_real_size) / Universe.cell_size,
                mod3((mouse.y - Universe.offset.y), Universe.block_real_size) / Universe.cell_size
            );
        }

        private static (int x, int y) Offset((int x, int y) position) => (
            position.x + Universe.offset.x,
            position.y + Universe.offset.y
        );

        private static (int x, int y) BlockGridOffset((int x, int y) block) => (
            block.x * Universe.block_size * Universe.cell_size,
            block.y * Universe.block_size * Universe.cell_size
        );

        // side_effects

        public static void Draw(PaintEventArgs e) {
            Universe.map.Values.ToList().ForEach((block) => {
                block.Draw(e,
                    Universe.cell_size,
                    Universe.Offset(Universe.BlockGridOffset(block.coord_id))
                );
            });
        }

        // Calculate the next generation of cells
        public static void Next(ToolStripStatusLabel generation_gui) {
            Universe.map.Values.ToList().ForEach((block) => block.Next());

            Universe.generation += 1;

            // Update generation in GUI
            generation_gui.Text = $"Generation {Universe.generation}";
        }

        public static void ToggleCellAtMousePosition((int x, int y) mouse_position) {
            Universe.map.Values.ToList().ForEach((block) => {
                if (block.Within(mouse_position, Universe.cell_size, Universe.offset)) {
                    var clicked_cell = Universe.FindClickedCell(mouse_position);

                    block.Toggle(clicked_cell);

                    block.ResetCellNeighbours(clicked_cell);

                    /**
                     * NOTE: can't use `CountAndSetCellNeighbours` since it's a wrapper for
                     * `cell.neighbours = <int>` with error correction.
                     * Instead I need to find neighbours myself by matrix scanning myself.
                     */
                    Block.Scan3x3Matrix(clicked_cell, (clicked_cell_neighbour) =>
                        block.CountAndSetCellNeighbours(clicked_cell_neighbour));
                }
            });
        }
    }
}
