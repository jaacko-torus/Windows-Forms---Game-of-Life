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
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace month_6_Project_and_Portfolio_I {
    public partial class Window : Form {
        // public

        public float camera_speed = 20;
        public float next_gen_speed = 200;
        public float refresh_speed = 1000 / 30;

        // private

        private int random_seed;
        private Vector2 raw_mouse_position;
        private Vector2 mouse_position;

        // readonly

        private readonly Timer nextGenTimer = new Timer();
        private readonly Timer inputTimer = new Timer();

        // constructor

        public Window() {
            this.InitializeComponent();

            this.graphicsPanelMain.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.graphicsPanelMain_MouseWheel);

            Universe.Initialize(this.graphicsPanelMain);
            Universe.camera.zoom_speed = (float)(1.0 / 240.0);
            this.toolStripStatusLabelZoom.Text = $"Zoom = {Universe.camera.zoom}";

            // Setup the timer
            this.nextGenTimer.Interval = (int)this.next_gen_speed;
            this.nextGenTimer.Tick += (object s, EventArgs e) => {
                // update universe
                Universe.Next(this.toolStripStatusLabelGenerations);
                // redraw
                this.redraw();
            };

            this.inputTimer.Interval = (int)this.refresh_speed;
            this.inputTimer.Tick += this.handleInput;
            this.inputTimer.Start();
        }

        private void reset() {
            Universe.Initialize(this.graphicsPanelMain);
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
            )) * this.camera_speed;

            if (movement != Vector2.Zero) {
                Universe.camera.position += (10 / Universe.camera.zoom) * movement;
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
            { "down",   false }
        };

        private readonly Dictionary<string, Keys[]> key_map = new Dictionary<string, Keys[]>() {
            { "left",  new Keys[] { Keys.A, Keys.Left } },
            { "right", new Keys[] { Keys.D, Keys.Right } },
            { "up",    new Keys[] { Keys.W, Keys.Up } },
            { "down",  new Keys[] { Keys.S, Keys.Down } },

            { "pause", new Keys[] { Keys.Space } }
        };

        private void keyDown(object sender, KeyEventArgs e) {
            if (this.key_map["pause"].Contains(e.KeyCode)) {
                this.toggleTimer();
                return;
            }

            this.key_map.ForEach((name, keys) => {
                if (keys.Contains(e.KeyCode) && this.input.ContainsKey(name)) {
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

            this.toolStripStatusLabelMousePosition.Text = $"Mouse Position = ({this.mouse_position.X}, {this.mouse_position.Y})";

            RectangleF cell_rect(Vector2 v) => new RectangleF(
                Universe.camera.WorldToScreen(v * Universe.cell_size).ToPointF(),
                new Vector2(Universe.cell_size).ToSizeF()
            );

            var hover_brush = new SolidBrush(Color.FromArgb(0x66, (Cell.cell_brush as SolidBrush).Color));

            e.Graphics.FillRectangle(hover_brush, cell_rect(this.mouse_position));
        }

        private bool is_paused = false;
        private void graphicsPanelMain_MouseDown(object sender, MouseEventArgs e) {
            if (this.nextGenTimer.Enabled) {
                this.is_paused = true;
                this.nextGenTimer.Stop();
            }
        }

        private void graphicsPanelMain_MouseUp(object sender, MouseEventArgs e) {
            if (this.is_paused) {
                this.is_paused = false;
                this.nextGenTimer.Start();
            }
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

        private void saveToolStripButton_Click(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells|RLE|*.rle";
            dlg.FilterIndex = 2;
            dlg.DefaultExt = "cells";

            if (dlg.ShowDialog() == DialogResult.OK) {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                var cells_format_matches = new Regex(@"\.cells$", RegexOptions.Multiline).IsMatch(dlg.FileName);
                var rle_format_matches = new Regex(@"\.rle$", RegexOptions.Multiline).IsMatch(dlg.FileName);

                if (rle_format_matches) {
                    Universe.SaveStateAs(Universe.SAVE_FORMAT.RLE).ToList()
                        .ForEach(writer.WriteLine);

                    Console.WriteLine("Feature currently not implemented.");
                } else if (cells_format_matches) {
                    // NOTE: cells is the default format

                    // add comments if necesary
                    writer.WriteLine("!This is my comment.");

                    Universe.SaveStateAs(Universe.SAVE_FORMAT.CELLS).ToList()
                        .ForEach(writer.WriteLine);
                } else {
                    Console.WriteLine("This should never happen.");
                }
                
                writer.Close();
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells|RLE|*.rle";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog()) {
                var file_name = dlg.FileName;
                StreamReader reader = new StreamReader(file_name);

                dlg.Dispose();

                var state = reader.ReadToEnd().Split(Environment.NewLine.ToCharArray())
                    .Where(line => line.Length > 0 && line[0] != '!').ToList();

                reader.Close();

                bool parsing_failed = false;

                var cells_format_matches = new Regex(@"\.cells$", RegexOptions.Multiline).IsMatch(file_name);
                var rle_format_matches = new Regex(@"\.rle$", RegexOptions.Multiline).IsMatch(file_name);

                if (state.Count > 0) {
                    if (cells_format_matches || rle_format_matches) {
                        Regex formating;

                        bool state_has_no_formatting_errors = false;

                        if (cells_format_matches) {
                            formating = new Regex(@"^[O.]+$", RegexOptions.Multiline);
                        
                            var first_line_length = state[0].Length;

                            state_has_no_formatting_errors = state
                                .All(line => formating.IsMatch(line) && line.Length == first_line_length);

                            Console.WriteLine(state.Aggregate((p, c) => p + "\n" + c));
                        }

                        if (rle_format_matches) {
                            Console.WriteLine("Feature currently not implemented.");
                        }

                        if (state_has_no_formatting_errors) {
                            this.reset();
                            Universe.Reset(this.toolStripStatusLabelGenerations);
                            this.nextGenTimer.Stop();

                            Universe.OpenStateAs(Universe.SAVE_FORMAT.CELLS, state.ToArray());

                            this.redraw();
                        } else {
                            parsing_failed = true;
                        }
                    } else {
                        parsing_failed = true;
                    }
                }

                if (parsing_failed) {
                    DialogResult result = MessageBox.Show(
                        $"Make sure the `{ new Regex(@"\.[\w]+$").Match(file_name) }` file you are using is formated correctly.\nClick retry when you're ready to select a file again.",
                        "Invalid format",
                        MessageBoxButtons.RetryCancel
                    );

                    if (result == DialogResult.Retry) {
                        openToolStripButton_Click(sender, e);
                    }
                }
            }
        }

        private void toolStripButtonRandom_Click(object sender, EventArgs e) {
            this.reset();
            Universe.Reset(this.toolStripStatusLabelGenerations);
            Universe.Random(0);
            this.nextGenTimer.Stop();
            this.redraw();
        }

        private void graphicsPanelMain_MouseMove(object sender, MouseEventArgs e) {
            this.raw_mouse_position = new Vector2(e.X, e.Y);
            this.mouse_position = Universe.FindClickedCell(raw_mouse_position);
            this.redraw();
        }

        private void graphicsPanelMain_MouseEnter(object sender, EventArgs e) {
            this.graphicsPanelMain.Focus();
        }

        private void graphicsPanelMain_MouseLeave(object sender, EventArgs e) {
            this.ActiveControl = null;
        }

        private void graphicsPanelMain_MouseWheel(object sender, MouseEventArgs e) {
            bool no_scroll = e.Delta == 0;
            if (no_scroll) { return; }

            Universe.camera.ZoomBy(e.Delta);
            Universe.camera.Clamp();


            //Universe.camera.position += -Universe.camera.position * e.Delta;


            Universe.UpdateDefaultCellSize();


            this.toolStripStatusLabelZoom.Text = $"Zoom = {Universe.camera.zoom}";
            this.redraw();
        }

        private void toolStripButtonChangeRandomSeed_Click(object sender, EventArgs e) {
            ChangeRandomSeedDialog change_random_seed_dialog = new ChangeRandomSeedDialog();

            change_random_seed_dialog.seed = this.random_seed;

            if (change_random_seed_dialog.ShowDialog() == DialogResult.OK) {
                this.random_seed = change_random_seed_dialog.seed;
            }
        }

        private string fetch(string value) {
            const string download_path = "https://conwaylife.com/ref/lexicon/";

            bool is_letter = new Regex(@"[a-z]").IsMatch(value[0].ToString());
            bool is_number = new Regex(@"[0-9]").IsMatch(value[0].ToString());

            char page_index_url =
                is_number ? '1' :
                is_letter ? value[0] :
                '\0';

            if (is_letter || is_number) {
                string download_file = $"lex_{page_index_url}.htm";
                string fetched_html = new WebClient().DownloadString(download_path + download_file);

                Match lexicon_section = new Regex($@"<p><a name=\w+>:<\/a><b>{value}[\S\s]+?<\/pre>").Match(fetched_html);

                if (lexicon_section.Success) {
                    var lexicon = new Regex(@"(?<=<pre>[\n\r]+)[\t\n\r.O]+(?=<\/pre>)").Match(lexicon_section.Value);
                    var lexicon_name = new Regex(@"(?<=<a name=)[\w\-]+(?=>:<\/a>)").Match(lexicon_section.Value);
                    var lexicon_section_minus_lexicon = new Regex(@"[\S\s]+(?=<pre>)").Match(lexicon_section.Value);

                    string remove_html(string s) => new Regex(@"<[\S\s]+?>").Replace(s, "");
                    string replace_newline_w_space(string s) => new Regex(@"[\n\r]+").Replace(s, " ");
                    string clean(string s) => replace_newline_w_space(remove_html(s));

                    if (lexicon.Success) {
                        Console.WriteLine();
                        Console.WriteLine(clean(lexicon_section_minus_lexicon.Value));

                        return lexicon.Value;
                    } else if (lexicon_name.Success) {
                        Console.WriteLine($"Lexicon \"{value}\" was not found. Last reference found was \"{download_path + download_file}#{lexicon_name}\".");
                        Console.WriteLine();
                        Console.WriteLine(clean(lexicon_section.Value));

                        return "\0";
                    } else {
                        Console.WriteLine($"Lexicon value of \"{value}\" not found");
                        return "\0";
                    }
                } else {
                    Console.WriteLine($"Lexicon \"{value}\" not found");
                    return "\0";
                }
            } else {
                Console.WriteLine($"Fetched lexicon \"{value}\" is invalid.");
                return "\0";
            }
        }

        private void toolStripButtonFetch_Click(object sender, EventArgs e) {
            FetchDialog fetch_dialog = new FetchDialog();

            if (fetch_dialog.ShowDialog() == DialogResult.OK && fetch_dialog.value.Length > 0) {
                string found_lexicon = this.fetch(fetch_dialog.value);
                Console.WriteLine();

                if (!string.IsNullOrWhiteSpace(found_lexicon)) {
                    var state = Regex.Split(found_lexicon, @"([\n\r]+|\t)+").Where(row => !string.IsNullOrWhiteSpace(row));
                    
                    this.reset();

                    Universe.Reset(this.toolStripStatusLabelGenerations);
                    this.nextGenTimer.Stop();

                    Universe.OpenStateAs(Universe.SAVE_FORMAT.CELLS, state.ToArray());

                    this.redraw();
                }
            }
        }

        private void toolStripButtonChangePlaySpeed_Click(object sender, EventArgs e) {
            ChangePlaySpeedDialog change_play_speed_dialog = new ChangePlaySpeedDialog();

            change_play_speed_dialog.play_speed = this.next_gen_speed;

            if (change_play_speed_dialog.ShowDialog() == DialogResult.OK) {
                this.next_gen_speed = change_play_speed_dialog.play_speed;
                this.nextGenTimer.Interval = (int)this.next_gen_speed;
            }
        }
    }
}
