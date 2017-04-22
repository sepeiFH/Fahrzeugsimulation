using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using FhMapDrawing.ServiceReference1;
using TiledSharp;

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
        Texture2D tileset;

        int tileWidth;
        int tileHeight;
        int tilesetTilesWide;
        int tilesetTilesHigh;

        float scale = 1;

        Texture2D tilesetVehicle;
        int tileWidthVehicle;
        int tileHeightVehicle;
        int tilesetTilesWideVehicle;

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

            graphics.PreferredBackBufferWidth = 1100; 
            graphics.PreferredBackBufferHeight = 500;  
            graphics.ApplyChanges();

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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        int count = 0;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            for(var layer = 0; layer < map.Layers.Count; layer++)
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
                        int tileFrame = gid - 1;
                        int column = tileFrame % tilesetTilesWide;
                        int row = (int)Math.Floor((double)tileFrame / (double)tilesetTilesWide);

                        float x = (i % map.Width) * map.TileWidth;
                        float y = (float)Math.Floor(i / (double)map.Width) * map.TileHeight;

                        Rectangle tilesetRec = new Rectangle(tileWidth * column, tileHeight * row, tileWidth, tileHeight);

                        float rotation = 0.0f;

                        if (layer > 0)
                        {
                            x += count;
                            //rotation += (float)count / 50;
                        }

                        //spriteBatch.Draw(tileset, new Rectangle((int)x, (int)y, tileWidth, tileHeight), tilesetRec, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layer);
                        //spriteBatch.Draw(tileset, new Rectangle((int) (x * scale), (int) (y*scale), tileWidth, tileHeight), tilesetRec, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layer);
                        spriteBatch.Draw(tileset, new Rectangle((int)(x * scale), (int)(y * scale), (int)(tileWidth * scale), (int)(tileHeight * scale)), tilesetRec, Color.White, rotation, Vector2.Zero, SpriteEffects.None, layer);
                    }
                }

            var items = clientSimulator.GetDynamicObjects();
            foreach (BlockObjectContract item in items)
            {
                if (item.GID < 100)
                {
                    int tileFrame = item.GID - 1;
                    int column = tileFrame % tilesetTilesWide;
                    int row = (int)Math.Floor((double)tileFrame / (double)tilesetTilesWide);
                    float rotate = (float) (item.Rotation / 180 * Math.PI);

                    int dynX = 0, dynY = 0;

                    if (item.Rotation >= 0 && item.Rotation <= 89)
                    {
                        dynY = 16;
                        dynX = -16;
                    }
                    else if(item.Rotation == 180)
                    {
                        dynY = -16;
                        dynX = +16;
                    }else if (item.Rotation == 270)
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

                    Rectangle tilesetRec = new Rectangle(tileWidth * column, tileHeight * row, tileWidth, tileHeight);

                    spriteBatch.Draw(tileset, new Rectangle(x, y, (int)(tileWidth * scale), (int)(tileHeight * scale)), tilesetRec, Color.White, rotate, new Vector2(16, 16), SpriteEffects.None, 1);
                }else if (item.GID > 999)
                {
                    int tileFrame = item.GID - 1000;
                    int column = tileFrame % tileWidthVehicle;
                    int row = (int)Math.Floor((double)tileFrame / (double)tileWidthVehicle);
                    float rotate = (float)(item.Rotation / 180 * Math.PI);

                    Rectangle tilesetRec = new Rectangle(tileWidthVehicle * column, tileHeightVehicle * row, tileWidthVehicle, tileHeightVehicle);

                    int x = (int)((item.X) * scale);
                    int y = (int)((item.Y) * scale);

                    spriteBatch.Draw(tilesetVehicle, new Rectangle(x, y, (int)(tileWidthVehicle * scale), (int)(tileHeightVehicle * scale)), tilesetRec, Color.White, rotate, new Vector2(tileWidthVehicle, tileHeightVehicle), SpriteEffects.None, 1);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
