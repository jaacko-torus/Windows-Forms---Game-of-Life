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
                Universe.Next(toolStripStatusLabelGenerations);
                // redraw
                this.redraw();
            };

            this.inputTimer.Interval = 1000 / 30;
            //EventHandler
            this.inputTimer.Tick += this.handleInput;
            this.inputTimer.Start();
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

        private bool isWithinScreen(int x, int y) {
            int max_x = this.graphicsPanelMain.ClientSize.Width;
            int max_y = this.graphicsPanelMain.ClientSize.Height;

            return (
                0 < x && x < max_x &&
                0 < y && y < max_y
            );
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

            if (this.input["space"]) {
                this.toggleTimer();
            }

            if (this.input.Values.Contains(true)) {
                // redraw only when needed
                this.redraw();
            }
        }

        private Dictionary<string, bool> input = new Dictionary<string, bool>() {
            { "left",  false },
            { "right", false },
            { "up",    false },
            { "down",  false },
            { "space", false }
        };

        private Dictionary<string, Keys[]> key_map = new Dictionary<string, Keys[]>() {
            { "left",  new Keys[] { Keys.A, Keys.Left } },
            { "right", new Keys[] { Keys.D, Keys.Right } },
            { "up",    new Keys[] { Keys.W, Keys.Up } },
            { "down",  new Keys[] { Keys.S, Keys.Down } },
            { "space", new Keys[] { Keys.Space } }
        };

        private void keyDown(object sender, KeyEventArgs e) {
            this.key_map.ForEach((name, keys) => {
                if (keys.Contains(e.KeyCode)) {
                    this.input[name] = true;
                }
            });
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
