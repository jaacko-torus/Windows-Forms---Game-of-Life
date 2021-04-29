using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace month_6_Project_and_Portfolio_I {
    class Camera {
        public enum AnchorType {
            TOP_LEFT,
            CENTER,
            CUSTOM
        }

        public enum SizeType {
            CLIENT,
            CUSTOM
        }

        public GraphicsPanel graphics_panel;

        public Vector2 position;
        public float zoom_speed = 1;
        public float zoom;
        public float zoom_in_limit = 2;
        public float zoom_out_limit = 200;
        public float zoom_remove_cell_neighbors = 22;
        public float zoom_remove_universe_grid = 45;
        public Vector2 world_position => this.ScreenToWorld(this.position);

        public SizeType size_type = SizeType.CLIENT;
        private Vector2 _size;
        public Vector2 size {
            get =>
                this.size_type == SizeType.CLIENT ? this.graphics_panel.ClientSize.ToVector2() :
                this._size; // this.size_type == SizeType.CUSTOM

            set {
                if (this.size_type == SizeType.CUSTOM) {
                    this._size = value;
                }
            }
        }

        public AnchorType anchor_type = AnchorType.CENTER;
        private Vector2 _anchor;
        public Vector2 anchor {
            get =>
                this.anchor_type == AnchorType.TOP_LEFT ? this.position :
                this.anchor_type == AnchorType.CENTER ? this.position + this.size / 2 :
                this._anchor; // this.anchor_type == AnchorType.CUSTOM

            set {
                if (this.anchor_type == AnchorType.CUSTOM) {
                    this._anchor = value;
                }
            }
        }

        public Vector2 top_left => this.position - this.anchor;
        public Vector2 bottom_right => this.position + (this.size - this.anchor);

        public float left => this.top_left.X;
        public float right => this.bottom_right.X;
        public float top => this.top_left.Y;
        public float bottom => this.bottom_right.Y;

        public Camera(
            GraphicsPanel graphics_panel, Vector2 position,
            SizeType size_type, Vector2 size,
            AnchorType anchor_type, Vector2 anchor
        ) {
            this.graphics_panel = graphics_panel;

            this.position = position;

            this.zoom = 10;

            this.anchor_type = anchor_type;

            if (size_type == SizeType.CUSTOM) {
                this.size = size;
            }

            this.anchor_type = anchor_type;

            if (anchor_type == AnchorType.CUSTOM) {
                this.anchor = anchor;
            }
        }

        public Camera(GraphicsPanel graphics_panel, Vector2 position, Vector2 size, Vector2 anchor) : this(
            graphics_panel, position,
            SizeType.CLIENT, size,
            AnchorType.CENTER, anchor
        ) { }

        public Camera(
            GraphicsPanel graphics_panel,
            Vector2 position, Vector2 size
        ) : this(graphics_panel, position, size, Vector2.Zero) { }

        public void ZoomBy(float amount) {
            this.zoom += this.zoom_speed * amount;
        }

        public void Clamp() =>
            this.zoom = UMath.Clamp(this.zoom, this.zoom_in_limit, this.zoom_out_limit);

        public Vector2 ScreenToWorld(Vector2 vector2) =>
            vector2 - Universe.camera.position - Universe.camera.anchor;

        public Vector2 WorldToScreen(Vector2 vector2) =>
            Universe.camera.anchor + Universe.camera.position + vector2;
    }
}
