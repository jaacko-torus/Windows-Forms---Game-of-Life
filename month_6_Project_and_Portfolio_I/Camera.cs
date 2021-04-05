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

        public Vector2 position;
        public Vector2 size;

        public AnchorType anchor_type = AnchorType.CENTER;
        private Vector2 _anchor;
        public Vector2 anchor {
            get =>
                this.anchor_type == AnchorType.TOP_LEFT ? this.position :
                this.anchor_type == AnchorType.CENTER   ? (this.size - this.position) / 2 + this.position :
                this.anchor_type == AnchorType.CUSTOM   ? this._anchor :
                new Vector2(float.NaN, float.NaN);
            
            set {
                if (this.anchor_type == AnchorType.CUSTOM) {
                    this._anchor = value;
                }
            }
        }
    }
}
