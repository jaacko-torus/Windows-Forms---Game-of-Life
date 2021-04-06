using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.IO;

namespace month_6_Project_and_Portfolio_I {
    public partial class Window : Form {
        // public

        private float speed = 20;

        // private

        // readonly

        private readonly Timer nextGenTimer = new Timer();
        private readonly Timer inputTimer = new Timer();

        // constructor

        public Window() {
            this.InitializeComponent();

            Universe.Start(this.graphicsPanelMain);

            // Setup the timer
            this.nextGenTimer.Interval = 200;
            this.nextGenTimer.Tick += (object s, EventArgs e) => {
                // update universe
                Universe.Next(this.toolStripStatusLabelGenerations);
                // redraw
                this.redraw();
            };

            this.inputTimer.Interval = 1000 / 30;
            this.inputTimer.Tick += this.handleInput;
            this.inputTimer.Start();
        }

        private void reset() {

            Universe.Start(this.graphicsPanelMain);
            this.nextGenTimer.Start();
        }

        // helpers

        public void redraw() => this.graphicsPanelMain.Invalidate();

        public void toggleTimer() {
            if (this.nextGenTimer.Enabled) {
                this.nextGenTimer.Stop();
            } else {
                this.nextGenTimer.Start();
            }
        }

        // window events

        public void handleInput(object sender, EventArgs e) {
            int to_i(bool b) => b ? 1 : 0;

            var movement = UVector2.Normalized(new Vector2(
                to_i(this.input["left"]) - to_i(this.input["right"]),
                to_i(this.input["up"])   - to_i(this.input["down"])
            )) * this.speed;

            if (movement != Vector2.Zero) {
                Universe.offset += movement;
            }

            if (this.input["resize"]) {
                // idk how to resize :(

                // TODO: make sure center 
                // Universe.offset = Universe.ToClientPosition(Universe.DefaultOffset());
            }

            if (this.input.Values.Contains(true)) {
                // redraw only when needed
                this.redraw();
            }
        }

        private Dictionary<string, bool> input = new Dictionary<string, bool>() {
            { "left",   false },
            { "right",  false },
            { "up",     false },
            { "down",   false },

            { "resize", false }
        };

        private Dictionary<string, Keys[]> key_map = new Dictionary<string, Keys[]>() {
            { "left",  new Keys[] { Keys.A, Keys.Left } },
            { "right", new Keys[] { Keys.D, Keys.Right } },
            { "up",    new Keys[] { Keys.W, Keys.Up } },
            { "down",  new Keys[] { Keys.S, Keys.Down } },

            { "pause", new Keys[] { Keys.Space } }
        };

        private void keyDown(object sender, KeyEventArgs e) {
            this.key_map.ForEach((name, keys) => {
                if (keys.Contains(e.KeyCode) && this.input.ContainsKey(name)) {
                    this.input[name] = true;
                }
            });

            if (this.key_map["pause"].Contains(e.KeyCode)) {
                this.toggleTimer();
            }
        }

        private void keyUp(object sender, KeyEventArgs e) {
            this.key_map.ForEach((name, keys) => {
                if (keys.Contains(e.KeyCode)) {
                    this.input[name] = false;
                }
            });
        }

        private void graphicsPanelMain_Paint(object sender, PaintEventArgs e) {
            Universe.Draw(e);
        }

        private void graphicsPanelMain_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                Universe.ToggleAtMousePosition(new Vector2(e.X, e.Y));
                this.redraw();
            }
        }
        
        private void toolStripButtonStart_Click(object sender, EventArgs e) {
            this.nextGenTimer.Start();
        }

        private void toolStripButtonStep_Click(object sender, EventArgs e) {
            Universe.Next(toolStripStatusLabelGenerations);
            this.redraw();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e) {
            this.nextGenTimer.Stop();
            this.redraw();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) =>
            this.Close();

        private void toolStripButtonReset_Click(object sender, EventArgs e) {
            this.reset();
            Universe.Reset(this.toolStripStatusLabelGenerations);
            this.nextGenTimer.Stop();
            this.redraw();
        }

        private void Window_ResizeBegin(object sender, EventArgs e) =>
            this.input["resize"] = true;

        private void Window_ResizeEnd(object sender, EventArgs e) =>
            this.input["resize"] = false;

        private void saveToolStripButton_Click(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;
            dlg.DefaultExt = "cells";

            if (dlg.ShowDialog() == DialogResult.OK){
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // add comments if necesary
                writer.WriteLine("!This is my comment.");

                Universe.SaveStateAs(Universe.SAVE_FORMAT.CELLS).ToList()
                    .ForEach(writer.WriteLine);

                //// Iterate through the universe one row at a time.
                //for (int y = 0; y < universe Height; y++) {
                //    // Create a string to represent the current row.
                //    String currentRow = string.Empty;

                //    // Iterate through the current row one cell at a time.
                //    for (int x = 0; x < universe Width; x++) {
                //        // If the universe[x,y] is alive then append 'O' (capital O)
                //        // to the row string.

                //        // Else if the universe[x,y] is dead then append '.' (period)
                //        // to the row string.
                //    }

                //    // Once the current row has been read through and the 
                //    // string constructed then write it to the file using WriteLine.
                //}

                // After all rows and columns have been written then close the file.
                writer.Close();
            }
        }
    }
}
