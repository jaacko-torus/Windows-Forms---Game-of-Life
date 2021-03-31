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
        public static int cell_size;
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

        // Initializer

        public static void Start(GraphicsPanel graphics_panel) {
            Universe.block_size = 10;

            Universe.cell_size = Universe.DefaultCellSize(graphics_panel);
            Universe.offset = Universe.DefaultOffset(graphics_panel);

            Universe.map.Add((0, 0), new Block(0, 0, Universe.block_size));
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

        private static (int x, int y) FindClickedCell((int x, int y) mouse) => (
            (mouse.x - Universe.offset.x) / Universe.cell_size,
            (mouse.y - Universe.offset.y) / Universe.cell_size
        );

        // side_effects

        public static void Draw(PaintEventArgs e) {
            var grid_pen = new Pen(Universe.colors["grid"], 1);
            var cell_brush = new SolidBrush(Universe.colors["cell"]);

            Universe.map.Values.ToList().ForEach((block) => {
                block.Draw(e, Universe.colors, cell_brush, grid_pen, Universe.cell_size, Universe.offset);
            });

            grid_pen.Dispose();
            cell_brush.Dispose();
        }

        // Calculate the next generation of cells
        public static void Next(ToolStripStatusLabel generation_gui) {
            Universe.map.Values.ToList().ForEach((block) => block.Next());

            Universe.generation += 1;

            // Update generation in GUI
            generation_gui.Text = $"Generation {Universe.generation}";
        }

        public static void SetCellAtMousePosition((int x, int y) mouse_position) {
            Universe.map.Values.ToList().ForEach((block) => {
                if (block.Within(mouse_position, Universe.cell_size, Universe.offset)) {
                    var clicked_cell = FindClickedCell(mouse_position);

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
