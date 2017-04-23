using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace FhMapDrawing.Helper
{
    class TilesInfo
    {
        Texture2D tileset;

        private int tileWidth;
        private int tileHeight;
        private int tilesetTilesWide;
        private int tilesetTilesHigh;
        private string name;

        public Texture2D Tileset
        {
            get { return tileset; }
            set { tileset = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int TileWidth
        {
            get { return tileWidth; }
            set { tileWidth = value; }
        }

        public int TileHeight
        {
            get { return tileHeight; }
            set { tileHeight = value; }
        }

        public int TilesetTilesWide
        {
            get { return tilesetTilesWide; }
            set { tilesetTilesWide = value; }
        }

        public int TilesetTilesHigh
        {
            get { return tilesetTilesHigh; }
            set {  tilesetTilesHigh = value; }
        }
    }
}
