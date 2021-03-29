using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace month_6_Project_and_Portfolio_I
{
    class Block
    {
        public readonly (int x, int y) coord_id;

        public readonly int size;
        public bool[,] cells;
        
        public List<(int x, int y)> alive = new List<(int x, int y)>();

        public Block(int x_position, int y_position, uint size)
        {
            this.coord_id = (x_position, y_position);
            this.size = (int)size;
            this.cells = new bool[size, size];
        }

        public delegate void ForEachCallback(uint x, uint y);

        public void ForEach(ForEachCallback callback)
        {
            for (uint y = 0; y < this.cells.GetLength(1); y += 1)
            {
                for (uint x = 0; x < this.cells.GetLength(0); x += 1)
                {
                    callback(x, y);
                }
            }
        }

        public bool Get(uint x, uint y) => this.cells[x, y];

        public void Set(uint x, uint y, bool value)
        {
            this.cells[x, y] = value;
        }

        public bool Is(uint x, uint y, bool value) => this.cells[x, y] == value;

        public void Toggle(uint x, uint y)
        {
            this.cells[x, y] = !this.cells[x, y];
        }
    }
}
