using FhMapDrawing.Helper;
using FhMapDrawing.ServiceReference1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TiledSharp;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FhMapDrawing
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        TmxMap map;
        float scale = 1f;
        private List<TilesInfo> tiles;
        private ServiceReference1.SimulatorServiceMapClient clientSimulator;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //Fullscreen
            /*graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();*/

            graphics.PreferredBackBufferWidth = (int)(1800 * scale);
            graphics.PreferredBackBufferHeight = (int)(1250 * scale);
            graphics.ApplyChanges();
            IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            clientSimulator = new ServiceReference1.SimulatorServiceMapClient();

            //map = new TmxMap("Content/FH2.tmx");
            map = new TmxMap(clientSimulator.GetMap(), true);

            tiles = new List<TilesInfo>(map.Tilesets.Count);

            foreach (var tile in map.Tilesets)
            {
                FileStream fileStream = new FileStream("Content/" + tile.Image.Source.ToString(), FileMode.Open);
                Texture2D tileset = Texture2D.FromStream(GraphicsDevice, fileStream);
                fileStream.Dispose();
                tiles.Add(new TilesInfo() { Tileset = tileset, TileWidth = tile.TileWidth, TileHeight = tile.TileHeight, TilesetTilesWide = tileset.Width / tile.TileWidth, TilesetTilesHigh = tileset.Height / tile.TileHeight, Name = tile.Image.Source });
            }

            /*
            //Map
            FileStream fileStream = new FileStream("Content/" + map.Tilesets[0].Image.Source.ToString(), FileMode.Open);
            tileset = Texture2D.FromStream(GraphicsDevice, fileStream);
            fileStream.Dispose();

            //tileset = Content.Load<Texture2D>(map.Tilesets[0].Name.ToString());

            tileWidth = map.Tilesets[0].TileWidth;
            tileHeight = map.Tilesets[0].TileHeight;

            tilesetTilesWide = tileset.Width / tileWidth;
            tilesetTilesHigh = tileset.Height / tileHeight;

            //Vehicle
            fileStream = new FileStream("Content/" + map.Tilesets[1].Image.Source.ToString(), FileMode.Open);
            tilesetVehicle = Texture2D.FromStream(GraphicsDevice, fileStream);
            fileStream.Dispose();

            tileWidthVehicle = map.Tilesets[1].TileWidth;
            tileHeightVehicle = map.Tilesets[1].TileHeight;

            tilesetTilesWideVehicle = tilesetVehicle.Width / tileWidthVehicle;
            */

            //calculate scale
            //scale = (float)Decimal.Round((decimal)graphics.PreferredBackBufferWidth / (map.Width * tileWidth), 1);


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private MouseState oldState;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.F11))
            {
                graphics.IsFullScreen = !graphics.IsFullScreen;
                graphics.ApplyChanges();
            }


            // TODO: Add your update logic here

            MouseState newState = Mouse.GetState();

            if (newState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                Point pos = new Point(newState.X, newState.Y);
                double smallestDistance = 100;
                BlockObjectContract tempItem = null;
                foreach (BlockObjectContract item in currentItems.Where(x => x.GID > 999).ToList())
                {
                    double currentDistance = Utils.GetDistance(pos, new Point((int)item.X, (int)item.Y));
                    if (currentDistance < smallestDistance)
                    {
                        tempItem = item;
                        smallestDistance = currentDistance;
                    }
                }
                if (tempItem != null)
                {
                    clientSimulator.ToggleBrokenItem(tempItem);
                }
            }

            oldState = newState;

            base.Update(gameTime);
        }

        int count = 0;

        private List<BlockObjectContract> currentItems;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            for (var layer = 0; layer < map.Layers.Count; layer++)
                for (var i = 0; i < map.Layers[layer].Tiles.Count; i++)
                {
                    int gid = map.Layers[layer].Tiles[i].Gid;



                    //Ampel Test Start
                    /*int increment = 50;

                    if (gid == 3 && ++count == increment * 4)
                    {
                        gid = 4;
                        TmxLayerTile layerC = map.Layers[layer].Tiles[i];
                        map.Layers[layer].Tiles[i] = new TmxLayerTile((uint)gid, layerC.X, layerC.Y);
                    }

                    if (gid == 4 && ++count == increment * 5)
                    {
                        gid = 5;
                        TmxLayerTile layerC = map.Layers[layer].Tiles[i];
                        map.Layers[layer].Tiles[i] = new TmxLayerTile((uint)gid, layerC.X, layerC.Y);
                    }

                    if (gid == 5 && ++count == increment * 7)
                    {
                        gid = 3;
                        count = 0;
                        TmxLayerTile layerC = map.Layers[layer].Tiles[i];
                        map.Layers[layer].Tiles[i] = new TmxLayerTile((uint)gid, layerC.X, layerC.Y);
                    }*/
                    //Ampel Test Ende




                    // Empty tile, do nothing
                    if (gid == 0)
                    {

                    }
                    else
                    {
                        TilesInfo temp = tiles.Find(z => z.Name.Contains("2104"));
                        int tileFrame = gid - 1;
                        int column = tileFrame % temp.TilesetTilesWide;
                        int row = (int)Math.Floor((double)tileFrame / (double)temp.TilesetTilesWide);

                        float x = (i % map.Width) * map.TileWidth;
                        float y = (float)Math.Floor(i / (double)map.Width) * map.TileHeight;

                        Rectangle tilesetRec = new Rectangle(temp.TileWidth * column, temp.TileHeight * row, temp.TileWidth, temp.TileHeight);

                        float rotation = 0.0f;

                        if (layer > 0)
                        {
                            x += count;
                            //rotation += (float)count / 50;
                        }

                        //spriteBatch.Draw(tileset, new Rectangle((int)x, (int)y, tileWidth, tileHeight), tilesetRec, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layer);
                        //spriteBatch.Draw(tileset, new Rectangle((int) (x * scale), (int) (y*scale), tileWidth, tileHeight), tilesetRec, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layer);
                        spriteBatch.Draw(temp.Tileset, new Rectangle((int)(x * scale), (int)(y * scale), (int)(temp.TileWidth * scale), (int)(temp.TileHeight * scale)), tilesetRec, Color.White, rotation, Vector2.Zero, SpriteEffects.None, layer);
                    }
                }

            //Houses etc
            foreach (var item in map.ObjectGroups)
            {
                if (item.Name.Contains("Umgebung"))
                {
                    foreach (var obj in item.Objects)
                    {
                        TilesInfo temp = tiles.Find(z => z.Name.Contains("Umgebung"));
                        int tileFrame = obj.Tile.Gid - 162;
                        int column = tileFrame % temp.TileWidth;
                        int row = (int)Math.Floor((double)tileFrame / (double)temp.TileWidth);
                        float rotate = (float)(obj.Rotation / 180 * Math.PI);

                        Rectangle tilesetRec = new Rectangle(temp.TileWidth * column, temp.TileHeight * row, temp.TileWidth, temp.TileHeight);

                        int x = (int)((obj.X) * scale) + temp.TileWidth / 2;
                        int y = (int)((obj.Y) * scale) - temp.TileHeight / 2;

                        //spriteBatch.Draw(temp.Tileset, new Rectangle(x, y, (int)(temp.TileWidth * scale), (int)(temp.TileHeight * scale)), tilesetRec, Color.White, rotate, new Vector2(temp.TileWidth * scale / 2, temp.TileHeight * scale / 2), SpriteEffects.None, 1);
                    }
                }
            }
            try
            {
                currentItems = clientSimulator.GetDynamicObjects().ToList();
                foreach (BlockObjectContract item in currentItems)
                {
                    if (item.GID < 100)
                    {
                        TilesInfo temp = tiles.Find(z => z.Name.Contains("2104"));
                        int tileFrame = item.GID - 1;
                        int column = tileFrame % temp.TilesetTilesWide;
                        int row = (int)Math.Floor((double)tileFrame / (double)temp.TilesetTilesWide);
                        float rotate = (float)(item.Rotation / 180 * Math.PI);

                        int dynX = 0, dynY = 0;

                        if (item.Rotation >= 0 && item.Rotation <= 89)
                        {
                            dynY = 16;
                            dynX = -16;
                        }
                        else if (item.Rotation == 180)
                        {
                            dynY = -16;
                            dynX = +16;
                        }
                        else if (item.Rotation == 270)
                        {
                            dynX = 16;
                            dynY = 16;
                        }
                        else if (item.Rotation == 90)
                        {
                            dynX = -16;
                            dynY = -16;
                        }

                        int x = (int)((item.X - dynX) * scale);
                        int y = (int)((item.Y - dynY) * scale);

                        Rectangle tilesetRec = new Rectangle(temp.TileWidth * column, temp.TileHeight * row, temp.TileWidth, temp.TileHeight);

                        spriteBatch.Draw(temp.Tileset, new Rectangle(x, y, (int)(temp.TileWidth * scale), (int)(temp.TileHeight * scale)), tilesetRec, Color.White, rotate, new Vector2(16, 16), SpriteEffects.None, 1);
                    }
                    else if (item.GID > 100 && item.GID < 200)
                    {

                    }
                    else if (item.GID > 999)
                    {
                        TilesInfo temp = tiles.Find(z => z.Name.Contains("TileCars"));
                        int tileFrame = item.GID - 1000;
                        int column = tileFrame % temp.TileWidth;
                        int row = (int)Math.Floor((double)tileFrame / (double)temp.TileWidth);
                        float rotate = (float)(item.Rotation / 180 * Math.PI);

                        Rectangle tilesetRec = new Rectangle(temp.TileWidth * column, temp.TileHeight * row, temp.TileWidth, temp.TileHeight);

                        int x = (int)((item.X) * scale);
                        int y = (int)((item.Y) * scale);

                        spriteBatch.Draw(temp.Tileset, new Rectangle(x, y, (int)(temp.TileWidth * scale), (int)(temp.TileHeight * scale)), tilesetRec, Color.White, rotate, new Vector2(temp.TileWidth / 2, temp.TileHeight / 2), SpriteEffects.None, 1);
                    }
                }
            }
            catch
            {
                //TODO: print Disconnected message
                //spriteBatch.DrawString(new SpriteFont(), "Disconnected", new Vector2(0,0), Color.Red)
                Bitmap bitmap = new Bitmap(400, 100, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawString("Disconnected", new Font("Tahoma", 14), Brushes.Red, new PointF(0, 0));
                }

                spriteBatch.Draw(GetTexture(GraphicsDevice, bitmap), Vector2.Zero);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private Texture2D GetTexture(GraphicsDevice dev, System.Drawing.Bitmap bmp)
        {
            int[] imgData = new int[bmp.Width * bmp.Height];
            Texture2D texture = new Texture2D(dev, bmp.Width, bmp.Height);

            unsafe
            {
                // lock bitmap
                System.Drawing.Imaging.BitmapData origdata =
                    bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                uint* byteData = (uint*)origdata.Scan0;

                // Switch bgra -> rgba
                for (int i = 0; i < imgData.Length; i++)
                {
                    byteData[i] = (byteData[i] & 0x000000ff) << 16 | (byteData[i] & 0x0000FF00) | (byteData[i] & 0x00FF0000) >> 16 | (byteData[i] & 0xFF000000);
                }

                // copy data
                System.Runtime.InteropServices.Marshal.Copy(origdata.Scan0, imgData, 0, bmp.Width * bmp.Height);

                byteData = null;

                // unlock bitmap
                bmp.UnlockBits(origdata);
            }

            texture.SetData(imgData);

            return texture;
        }

    }
}
