﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace month_6_Project_and_Portfolio_I {
    public partial class Window : Form {
        // private

        // readonly

        private readonly Timer timer = new Timer();

        // constructor

        public Window() {
            this.InitializeComponent();

            Universe.Start(this.graphicsPanelMain);

            // Setup the timer
            this.timer.Interval = 200;
            this.timer.Tick += (object s, EventArgs e) => {
                // update universe
                Universe.Next(toolStripStatusLabelGenerations);
                // redraw
                this.redraw();
            };
        }

        // helpers

        public void redraw() {
            this.graphicsPanelMain.Invalidate();
        }

        public void toggleTimer() {
            if (this.timer.Enabled) {
                this.timer.Stop();
            } else {
                this.timer.Start();
            }
        }

        private bool isInScreen(int x, int y) {
            int max_x = this.graphicsPanelMain.ClientSize.Width;
            int max_y = this.graphicsPanelMain.ClientSize.Height;

            return (
                0 < x && x < max_x &&
                0 < y && y < max_y
            );
        }

        // window events

        private void keyEventHandler(object sender, KeyEventArgs e) {
            int speed = 10;

            switch (e.KeyCode) {
                case Keys.Left:  Universe.offset.x += speed; break;
                case Keys.Right: Universe.offset.x -= speed; break;
                case Keys.Up:    Universe.offset.y += speed; break;
                case Keys.Down:  Universe.offset.y -= speed; break;
                case Keys.Space: this.toggleTimer(); break;
            }

            this.redraw();
        }

        private void graphicsPanelMain_Paint(object sender, PaintEventArgs e) {
            Universe.Draw(e);
        }

        private void graphicsPanelMain_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                Universe.SetCellAtMousePosition((e.X, e.Y));
                this.redraw();
            }
        }
        
        private void toolStripButtonStart_Click(object sender, EventArgs e) {
            this.timer.Start();
        }

        private void toolStripButtonStep_Click(object sender, EventArgs e) {
            Universe.Next(toolStripStatusLabelGenerations);
            this.redraw();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e) {
            /**
             * NOTE: this should never mess with the internal logic since
             * `timer.Tick` only calls `this.Next` and there are no threads,
             * so I can feel safe only stopping the timer.
             */
            this.timer.Stop();
            this.redraw();
        }
    }
}
