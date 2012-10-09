using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Box2D.XNA;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using Microsoft.Phone;

namespace NLAS_game 
{
    public enum GameState { MainMenu, /* Paused, */ HowToPlay, InGame, Shop, ConfirmUpgrade, Debriefing, Failed}
    public enum Upgrade {Fastball, BallsofSteel, BiggerBins, UmpirePatience};

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private bool recoverFromForceQuit = false;

        // Game Variables
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont hudFont;
        private SpriteFont statFont; 
        private GameState currentGameState; // Current State of the Game. Note: Setting of this variable should be done through setGameState()
        private Random random;
        private String saveFileName = "USBGame_Temp_Game_Save";  //this is used if game exits prematurely due to phone call
        private bool spritebatchBegan;

        // Upgrade Constants
        private const int MAX_LEVEL = 3; // Maximum level of each showConfimationScreen
        private const string FASTBALL_DESCRIPTION = "Increase speed of balls across \nthe screen.";
        private const int FASTBALL_COST_1 = 100; // Fast ball level 1 showConfimationScreen cost 
        private const int FASTBALL_COST_2 = 200; // Fast ball level 2 showConfimationScreen cost 
        private const int FASTBALL_COST_3 = 400; // Fast ball level 3 showConfimationScreen cost 
        private const string BALLSOFSTEEL_DESCRIPTION = "Increase the density of the \nballs to hit the trash harder.";
        private const int BALLSOFSTEEL_COST_1 = 100; // Balls of steel level 1 showConfimationScreen cost 
        private const int BALLSOFSTEEL_COST_2 = 200; // Balls of steel level 2 showConfimationScreen cost 
        private const int BALLSOFSTEEL_COST_3 = 400; // Balls of steel level 3 showConfimationScreen cost 
        private const string BIGGERBIN_DESCRIPTION = "Increase the size of the bins \nto collect more cash.";
        private const int BIGGERBIN_COST_1 = 100; // Bigger bin level 1 showConfimationScreen cost 
        private const int BIGGERBIN_COST_2 = 200; // Bigger bin level 2 showConfimationScreen cost 
        private const int BIGGERBIN_COST_3 = 400; // Bigger bin level 3 showConfimationScreen cost 
        private const string UMPIREPATIENCE_DESCRIPTION = "Increase the number of hits \nthe Umpire takes before calling \nthe game.";
        private const int UMPIREPATIENCE_COST_1 = 100; // Umpire patience level 1 showConfimationScreen cost 
        private const int UMPIREPATIENCE_COST_2 = 200; // Umpire patience level 2 showConfimationScreen cost 
        private const int UMPIREPATIENCE_COST_3 = 400; // Umpire patience level 3 showConfimationScreen cost 
        // Wave Constants
        private const int WAVE_DROP_VARIANCE = 2; // Variance in object drop time
        private const float WAVE_DROP_INCREASE = 0.3f; // Increase in drop frequency per wave in seconds
        private const int WAVE_DROP_BASE = 4; // Base time of when objects are dropped 
        private const int WAVE_WAIT_TIME = 1; // Initial wait time before throwing object in a wave
        private const int WAVE_TIME_BASE = 35; // Base time of a wave in seconds
        private const int VUVUZELA_FREQUENCY_BASE = 9; // Base chance of a vuvuzela being thrown
        private const int BAT_FREQUENCY_BASE = 7; // Base chance of a bat being thrown
        private const int POPCORN_FREQUENCY_BASE = 12; // Base chance of a popcorn being thrown
        private const int POPCAN_FREQUENCY_BASE = 15; // Base chance of a popcan being thrown 
        private const int FREQUENCY_INCREMENT = 1; // Increase in chance of throwing more difficult objects each wave
        // Cash Constants
        private const int WAVE_BONUS = 50; // Cash received for passing each round
        private const int POPCAN_BONUS = 20; // Cash received for binning a popcan
        private const int HOTDOG_BONUS = 10; // Cash received for binning a hotdog
        private const int POPCORN_BONUS = 30; // Cash received for binning a popcorn container
        private const int BAT_BONUS = 50; // Cash received for binning a baseball bat
        private const int VUVUZELA_BONUS = 40; // Cash received for binning a vuvuzela        
        // Bat Constants
        private const float BAT_DENSITY = 2.5f;
        private const float BAT_HEIGHT = 24;
        private const float BAT_WIDTH = 72;
        // Baseball Constants
        private const float BASEBALL_SPEED = 3.4f;
        private const float BASEBALL_DENSITY = 1.2f;
        private const float BASEBALL_RADIUS = 16;
        // Vuvuzela Constants
        private const float VUVUZELA_DENSITY = 1;
        private const float VUVUZELA_WIDTH = 32;
        private const float VUVUZELA_HEIGHT = 72;
        // Hotdog Constants
        private const float HOTDOG_DENSITY = 1;
        private const float HOTDOG_WIDTH = 56;
        private const float HOTDOG_HEIGHT = 24;
        // Popcan Constants
        private const float POPCAN_DENSITY = 1.5f;
        private const float POPCAN_WIDTH = 48;
        private const float POPCAN_HEIGHT = 24;
        // Popcorn Constants
        private const float POPCORN_DENSITY = 1f;
        private const float POPCORN_WIDTH = 32;
        private const float POPCORN_HEIGHT = 64;
        // Umpire Constants
        private const int UMPIRE_WIDTH = 64; 
        private const int UMPIRE_HEIGHT = 64;
        private const int UMPIRE_BASE_PATIENCE = 3; // Base number of hits an umpire can take before ending the game
        private Vector2 UMPIRE_SPAWN = new Vector2(SCREEN_WIDTH / 2, SCREEN_HEIGHT - UMPIRE_HEIGHT); 
        private Umpire umpire;
        // Trashbin Constants 
        private const int TRASHBIN_WIDTH = 64;
        private const int TRASHBIN_HEIGHT = 64; 
        private Vector2 TRASHBIN_LEFT_SPAWN = new Vector2(SCREEN_WIDTH / 4, SCREEN_HEIGHT - TRASHBIN_HEIGHT);
        private Vector2 TRASHBIN_RIGHT_SPAWN = new Vector2(3*SCREEN_WIDTH/ 4, SCREEN_HEIGHT - TRASHBIN_HEIGHT);
        private Trashbin leftTrash;
        private Trashbin rightTrash;
        // Foulpole Constants
        private const int FOULPOLE_WIDTH = 100;
        private const int FOULPOLE_HEIGHT = 350;
        private Vector2 FOULPOLE_RIGHT_SPAWN = new Vector2(SCREEN_WIDTH - FOULPOLE_WIDTH, SCREEN_HEIGHT - FOULPOLE_HEIGHT/2);
        private Vector2 FOULPOLE_LEFT_SPAWN = new Vector2(0 + FOULPOLE_WIDTH, SCREEN_HEIGHT - FOULPOLE_HEIGHT/2);

        // Player Attributes
        private int fastBallLevel = 0; // Fast ball showConfimationScreen level
        private int ballsOfSteelLevel = 0; // Balls of steel showConfimationScreen level
        private int biggerBinsLevel = 0; // Bigger bins showConfimationScreen level
        private int umpirePatienceLevel = 0; // Umpire patience showConfimationScreen level
        private int currentCash = 0; // Current amount of player cash to spend on upgrades
        private int totalCash = 0; // Total cash earned in the game

        // Wave Attributes
        private int waveLevel = 0; // Current wave
        private int waveTotalTime = 0; // Total time for the wave
        private double waveTimeRemaining = 0; // Time remaining in the wave
        private int waveBinnedObjects = 0; // Number of objects that have been binned for the wave
        private int totalBinnedObj = 0;
        private int umpirePatience = 0; // Umpire patience, when it hits zero you lose the wave
        private int waveCash = 0;  // Cash accumumlated for the wave
        private double waveDropTimer = 0; // Throw timer used to determine when to throw the next object
        private List<DynamicPhysicsGameObject> activeObjects; // List of active objects in the wave 

        private Color CASH_GREEN = new Color(0, 255, 0); 
        // Main Menu 
        private Texture2D titlescreen;
        private Button howtoplaybutton;
        private Button startgamebutton;
        private Button exitbutton;
        private Vector2 startgamebuttonPos = new Vector2(300, 195);
        private Vector2 howtoplaybuttonPos = new Vector2(100, 340);
        private Vector2 exitbuttonPos = new Vector2(500, 340);

        // Fail Screen
        private Texture2D failscreen;
        private Vector2 failbacktomenuPos = new Vector2(430, 350);
        private Vector2 tryagainPos = new Vector2(190, 350);
        private Button backtomenubutton;
        private Button tryagainbutton;

        // Paused
        //private Button resumebutton;
        //private Vector2 resumebuttonPos = new Vector2(350, 200);
        //private Texture2D pausescreen;
        //private float pausescreenAlpha = 0.5f;
        //private GameState previousState = GameState.MainMenu;

        // Debriefing screen
        private Texture2D debriefingscreen;
        private Button shopbutton; 
        private Button nextwavebutton;
        private Vector2 shopbuttonPos = new Vector2(110, 330);
        private Vector2 nextwavebuttonPos = new Vector2(500, 330);

        // Shop Screen
        private Texture2D shopscreen;
        private Button okbutton;
        private Button fastballbutton;
        private Button ballsofsteelbutton;
        private Button biggerbinsbutton;
        private Button umpirepatiencebutton;
        private Vector2 fastballbuttonPos = new Vector2(190, 250);
        private Vector2 ballsofsteelbuttonPos = new Vector2(190, 130);
        private Vector2 biggerbinsbuttonPos = new Vector2(435, 130);
        private Vector2 umpirepatiencebuttonPos = new Vector2(435, 250);
        private Vector2 okbuttonPos = new Vector2(600, 375);

        // Confirm purchase screen
        private Upgrade confirmingUpgrade;
        private Vector2 CONFIRMATION_TOPLEFT_LOCATION = new Vector2(100, 70);
        private Vector2 CONFIRMATION_BOTTOM_LOCATION = new Vector2(315, 300);
        private Texture2D confirmscreen;
        private Button upgradebutton;
        private Button greyupgradebutton;
        private Button cancelbutton;
        private Vector2 upgradebuttonPos = new Vector2(550, 365); 
        private Vector2 cancelbuttonPos = new Vector2(315, 365);

        // In Game
        private Texture2D backgroundscreen; 
        
        // How To Screen
        private Texture2D howtoscreen;
        private Button letsplaybutton;
        private Vector2 letsplaybuttonPos = new Vector2(300, 190); 

        // Screen size
        private const int SCREEN_WIDTH = 800;
        private const int SCREEN_HEIGHT = 480; 
        private const int LEFT_LAUNCH_ZONE = 140;
        private const int RIGHT_LAUNCH_ZONE = LEFT_LAUNCH_ZONE;
        private const int LEFT_LAUNCH_EXTRA_MARGIN = 20;//Extra margin for your thumb in case you "pull" the slingshot in the wrong direction
        private const int RIGHT_LAUNCH_EXTRA_MARGIN = 20;

        //Object spawing consts
        private const float SPAWN_HEIGHT = -64;
        private const float UPPER_KILL_HEIGHT = -96;
        private const float KILL_HEIGHT = 64 + SCREEN_HEIGHT;
        private const float LEFT_HORIZONTAL_KILL_DISTANCE = -64;
        private const float RIGHT_HORIZONTAL_KILL_DISTANCE= SCREEN_WIDTH + -LEFT_HORIZONTAL_KILL_DISTANCE;
        private const int SPAWN_X_VELOCITY_RANGE = 20;
        private const int SPAWN_Y_VELOCITY_RANGE = 10;

        // Managers
        private SoundManager soundManager;

        //Physics stuff
        World physicsWorld;
        int velocityIterations = 6;
        int positionIterations = 2;

        //Variables for keeping track of thumbs and thumb positions
        Vector2 leftThumbStartPosition;
        Vector2 rightThumbStartPosition;
        Vector2 leftThumbCurrentPosition;
        Vector2 rightThumbCurrentPosition;
        bool currentlyDraggingOnLeftSide;
        bool currentlyDraggingOnRightSide;
        Texture2D ballStartTexture;
        Texture2D ballStretchTexture;
        float ballStartImageRadius = 30.0f;
        float ballStretchLineWidth = 10.0f;
        GameObject leftThumbStartObject;
        GameObject rightThumbStartObject;
        GameObject leftThumbStretchLine;
        GameObject rightThumbStretchLine;
        GameObject leftSuspendedBaseBall;
        GameObject rightSuspendedBaseBall;

        List<GameObject> allGameObjects;
        int totalNumberOfActiveBaseballs;
        private const int TOTAL_NUMBER_OF_BASEBALLS_ALLOWED_ONSCREEN = 7;

        public Game1()
        {
            //Magic, do not touch, handles recovery from phone calls
            Microsoft.Phone.Shell.PhoneApplicationService service = Microsoft.Phone.Shell.PhoneApplicationService.Current;
            service.Activated += new EventHandler<Microsoft.Phone.Shell.ActivatedEventArgs>(service_Activated);

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Register the screen size
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            // Start the game in the main menu
            setGameState(GameState.MainMenu);

            // initialize managers
            soundManager = new SoundManager(this.Content);

            // initialize game object lists
            this.allGameObjects = new List<GameObject>();
            this.activeObjects = new List<DynamicPhysicsGameObject>();

            this.currentlyDraggingOnLeftSide = false;
            this.currentlyDraggingOnRightSide = false;
            this.leftThumbStartPosition = new Vector2(LEFT_LAUNCH_ZONE,SCREEN_HEIGHT/2);
            this.leftThumbCurrentPosition = new Vector2(LEFT_LAUNCH_ZONE/2, SCREEN_HEIGHT / 2);
            this.rightThumbStartPosition = new Vector2(SCREEN_WIDTH-RIGHT_LAUNCH_ZONE, SCREEN_HEIGHT / 2);
            this.rightThumbCurrentPosition = new Vector2(SCREEN_WIDTH - RIGHT_LAUNCH_ZONE/2 , SCREEN_HEIGHT / 2);

        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            random = new Random();
            Vector2 gravity = new Vector2(0, 2.0f);
            this.physicsWorld = new World(gravity, false);

            if (recoverFromForceQuit)
            {

                IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();

                // open isolated storage, and write the savefile.
                if (savegameStorage.FileExists(saveFileName))
                {
                    IsolatedStorageFileStream fs = null;
                    try
                    {
                        fs = savegameStorage.OpenFile(saveFileName, System.IO.FileMode.Open);
                    }
                    catch (IsolatedStorageException)
                    {
                        // The file couldn't be opened, even though it's there.
                        // You can use this knowledge to display an error message
                        // for the user (beyond the scope of this example).
                    }

                    StreamReader reader = new StreamReader(fs);

                    if (reader != null)
                    {
                        try
                        {
                            //line 1 = gamestate, wait until end of load to change gamestate
                            String gameState = reader.ReadLine();

                            fastBallLevel = Int32.Parse(reader.ReadLine());
                            ballsOfSteelLevel = Int32.Parse(reader.ReadLine());
                            biggerBinsLevel = Int32.Parse(reader.ReadLine());
                            umpirePatienceLevel = Int32.Parse(reader.ReadLine());
                            currentCash = Int32.Parse(reader.ReadLine());

                            waveLevel = Int32.Parse(reader.ReadLine());

                            //finally determine gamestate and load it
                            setGameState(GameState.Debriefing);

                        } 
                        catch(IOException)
                        {          
                            Debug.WriteLine("Load Failed");
                        }
                    }

                    reader.Close();
                    fs.Close();

                }

            }

            soundManager.initializeSound();
            this.totalNumberOfActiveBaseballs = 0;

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

            //load sprite font in
            hudFont = Content.Load<SpriteFont>("SpriteFont/HUDFont");
            statFont = Content.Load<SpriteFont>("SpriteFont/StatFont");

            // Main Menu
            titlescreen = Content.Load<Texture2D>("Textures/titlescreen");
            howtoplaybutton = new Button(Content, howtoplaybuttonPos, "Textures/Buttons/StartMenu/howtoplaybutton", howToPlay);
            startgamebutton = new Button(Content, startgamebuttonPos, "Textures/Buttons/StartMenu/startgamebutton", startGame);
            exitbutton = new Button(Content, exitbuttonPos, "Textures/Buttons/StartMenu/exitbutton", exitGame);

            // Fail screen
            failscreen = Content.Load<Texture2D>("Textures/gameoverscreen");
            backtomenubutton = new Button(Content, failbacktomenuPos, "Textures/Buttons/StartMenu/exitbutton", backToMainMenu);
            tryagainbutton = new Button(Content, tryagainPos, "Textures/tryagainbutton", startGame);
            
            // Pause screen
            //pausescreen = Content.Load<Texture2D>("Textures/pausescreen"); 
            //resumebutton = new Button(Content, resumebuttonPos, "Textures/Buttons/PauseMenu/resumebutton", resumeGame);

            // Debriefing screen
            debriefingscreen = Content.Load<Texture2D>("Textures/debriefingscreen");
            shopbutton = new Button(Content, shopbuttonPos, "Textures/Buttons/DebriefingMenu/shopbutton", showShop);
            nextwavebutton = new Button(Content, nextwavebuttonPos, "Textures/Buttons/DebriefingMenu/nextbutton", nextWave);

            // Shop Screen
            shopscreen = Content.Load<Texture2D>("Textures/shopscreen");
            okbutton = new Button(Content, okbuttonPos, "Textures/Buttons/ShopMenu/okaybutton", exitShop);
            fastballbutton = new Button(Content, fastballbuttonPos, "Textures/Buttons/ShopMenu/fastballpowerup", fastballpu);
            ballsofsteelbutton = new Button(Content, ballsofsteelbuttonPos, "Textures/Buttons/ShopMenu/ballsofsteelpowerup", ballsofsteelpu);
            biggerbinsbutton = new Button(Content, biggerbinsbuttonPos, "Textures/Buttons/ShopMenu/biggerbinspowerup", biggerbinpu);
            umpirepatiencebutton = new Button(Content, umpirepatiencebuttonPos, "Textures/Buttons/ShopMenu/umpirepatiencepowerup", umppatientpu);

            //confirm purchase screen
            confirmscreen = Content.Load<Texture2D>("Textures/confirmationscreen");
            upgradebutton = new Button(Content, upgradebuttonPos, "Textures/Buttons/ShopMenu/upgradebutton", confirmUpgrade);
            greyupgradebutton = new Button(Content, upgradebuttonPos, "Textures/Buttons/ShopMenu/upgradedonebutton", confirmUpgrade);
            cancelbutton = new Button(Content, cancelbuttonPos, "Textures/Buttons/ShopMenu/cancelbutton", cancelUpgrade);

            // In Game
            backgroundscreen = Content.Load<Texture2D>("Textures/backgroundscreen");

            // How To Screen
            howtoscreen = Content.Load<Texture2D>("Textures/howtoscreen");
            letsplaybutton = new Button(Content, letsplaybuttonPos, "Textures/Buttons/HowToPlayMenu/letsplaybutton", startGame);

            // Game Objects 
            Baseball.baseballTexture = Content.Load<Texture2D>("Textures/baseball");
            Vuvuzela.vuvuzelaTexture = Content.Load<Texture2D>("Textures/vuvuzela");
            Bat.batTexture = Content.Load<Texture2D>("Textures/baseballbat");
            Popcorn.popcornTexture = Content.Load<Texture2D>("Textures/popcorn");
            Popcan.popcanTexture = Content.Load<Texture2D>("Textures/popcan"); 
            Hotdog.hotdogTexture = Content.Load<Texture2D>("Textures/hotdog");
            Umpire.umpireTexture = Content.Load<Texture2D>("Textures/umpire"); 
            Trashbin.trashbinTexture = Content.Load<Texture2D>("Textures/trashcan");
            Foulpole.foulpoleTexture = Content.Load<Texture2D>("Textures/foulpoles");
            UmpireHitEffect.hitSprite = this.Content.Load<Texture2D>("Textures/hitstar");
            CashRewardEffect.hitSprite = this.Content.Load<Texture2D>("Textures/dollarsign");

            //Stuff for drawing the ball slinging
            this.ballStartTexture = this.Content.Load<Texture2D>("Textures/ballStartSprite");
            this.ballStretchTexture = this.Content.Load<Texture2D>("Textures/lineSegmentTexture");
            
            //
            this.leftThumbStartObject = new GameObject(this.ballStartTexture);
            this.leftThumbStartObject.scaleToFitTheseDimensions(this.ballStartImageRadius * 2.0f, this.ballStartImageRadius * 2.0f);
            this.leftThumbStretchLine = new GameObject(this.ballStretchTexture);
            this.leftThumbStretchLine.scaleToFitTheseDimensions(10.0f, this.ballStretchLineWidth);
            this.leftSuspendedBaseBall = new GameObject(Baseball.baseballTexture);
            this.leftSuspendedBaseBall.scaleToFitTheseDimensions(120.0f, 120.0f);

            this.rightThumbStartObject = new GameObject(this.ballStartTexture);
            this.rightThumbStartObject.scaleToFitTheseDimensions(this.ballStartImageRadius * 2.0f, this.ballStartImageRadius * 2.0f);
            this.rightThumbStretchLine = new GameObject(this.ballStretchTexture);
            this.rightThumbStretchLine.scaleToFitTheseDimensions(10.0f, this.ballStretchLineWidth);
            this.rightSuspendedBaseBall = new GameObject(Baseball.baseballTexture);
            this.rightSuspendedBaseBall.scaleToFitTheseDimensions(120.0f, 120.0f);

            soundManager.initializeSound(); 

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

            soundManager.unloadSoundContent();

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit or escape to a logical screen
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                switch(currentGameState) {
                    case GameState.InGame:
                        //setGameState(GameState.Paused);
                        setGameState(GameState.MainMenu);
                        break;
                        /*
                    case GameState.Paused:
                        setGameState(GameState.InGame);
                        break;
                         */
                    case GameState.Failed:
                        setGameState(GameState.MainMenu);
                        break;
                    case GameState.MainMenu:
                        this.exitGame();
                        break;
                    case GameState.HowToPlay:
                        setGameState(GameState.MainMenu);
                        break;
                    case GameState.Shop:
                        setGameState(GameState.Debriefing);
                        break;
                    case GameState.ConfirmUpgrade:
                        setGameState(GameState.Shop);
                        break;
                    default:
                        break;
                }
            }

            // Get the user input
            TouchCollection touches = TouchPanel.GetState();

            // Which game state are we updating?
            switch (currentGameState)
            {
                case GameState.Failed:
                    foreach (TouchLocation location in touches)
                    {
                        backtomenubutton.Update(location);
                        tryagainbutton.Update(location);
                    }
                    break;
                    /*
                case GameState.Paused:
                    foreach (TouchLocation location in touches)
                    {
                        resumebutton.Update(location);
                    }
                    break;
                     */
                // Update Logic for the Main Menu
                case GameState.MainMenu:
                    foreach (TouchLocation location in touches)
                    {
                        howtoplaybutton.Update(location);
                        startgamebutton.Update(location);
                        exitbutton.Update(location);
                    }
                    break;
                // Update Logic for the How To Play Screen
                case GameState.HowToPlay:
                    foreach (TouchLocation location in touches)
                    {
                        letsplaybutton.Update(location);
                    }
                    break;
                // Update Logic for the Debriefing Screen
                case GameState.Debriefing:
                    foreach (TouchLocation location in touches)
                    {
                        shopbutton.Update(location);
                        nextwavebutton.Update(location);
                    }
                    break;
                // Update Logic for the Shop Screen
                case GameState.Shop:
                    foreach (TouchLocation location in touches)
                    {
                        fastballbutton.Update(location);
                        biggerbinsbutton.Update(location);
                        ballsofsteelbutton.Update(location);
                        umpirepatiencebutton.Update(location); 
                        okbutton.Update(location);
                    }
                    break;
                case GameState.ConfirmUpgrade:
                    foreach (TouchLocation location in touches)
                    {
                        if (isUpgradePossible(confirmingUpgrade))
                            upgradebutton.Update(location);
                        cancelbutton.Update(location);
                    }
                    break;
                // Update Logic for in Game
                case GameState.InGame:
                    UpdateInGame(touches, gameTime);
                    break;
            }

            soundManager.update(currentGameState);

            this.physicsWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds, this.velocityIterations, this.positionIterations);
            for (int i = 0; i < this.allGameObjects.Count; i++)
            {
                this.allGameObjects[i].update(gameTime); 
            }
                        
            base.Update(gameTime);
        }

        /// <summary>
        /// Update logic for game loop
        /// </summary>
        /// <param name="touches"></param> 
        private void UpdateInGame(TouchCollection touches, GameTime gameTime)
        {
            this.totalNumberOfActiveBaseballs = 0;
            for (int i = 0; i < this.activeObjects.Count(); i++)
            {
                if (this.activeObjects[i] is Baseball)
                {
                    this.totalNumberOfActiveBaseballs++;
                }
            }

            // check if the round is over or failsafe after 10 seconds past waveTimeRemaining
            if (waveTimeRemaining <= -10 || (waveTimeRemaining <= 0 && activeObjects.Count() == 0))
            {
                // passed
                currentCash += WAVE_BONUS + waveCash;
                totalCash += WAVE_BONUS + waveCash;
                setGameState(GameState.Debriefing);
            }
            else if (umpirePatience <= 0)
            {
                // failed
                setGameState(GameState.Failed);
            }

            // Delete old objects
            for (int i = 0; i < this.allGameObjects.Count; i++)
            {
                if (this.allGameObjects[i].deleteMePlease)
                {
                    this.allGameObjects.Remove(this.allGameObjects[i]);
                    i--;
                }
            }
            for (int i = 0; i < this.activeObjects.Count; i++)
            {
                if (this.activeObjects[i].position.Y > KILL_HEIGHT 
                    || this.activeObjects[i].position.Y < UPPER_KILL_HEIGHT
                    || this.activeObjects[i].position.X < LEFT_HORIZONTAL_KILL_DISTANCE
                    || this.activeObjects[i].position.X > RIGHT_HORIZONTAL_KILL_DISTANCE 
                    || this.activeObjects[i].deleteMePlease)
                {
                    this.activeObjects[i].deleteFromPhysicsSimulation(this.physicsWorld);
                    this.activeObjects.Remove(this.activeObjects[i]);
                    i--;
                }
            }

            // check for collisions
            foreach (GameObject gameObject in activeObjects)
            {
                if (checkCollision(umpire, gameObject))
                {
                    if (!(gameObject is Baseball))
                    {
                        this.allGameObjects.Add(new UmpireHitEffect(gameObject.position,gameTime));
                        soundManager.playSound(soundManager.umpireHit);
                        soundManager.playSound(soundManager.crowdBoo);
                        gameObject.deleteMePlease = true;
                        umpirePatience--;
                    }
                }
                else if (checkCollision(leftTrash, gameObject) || checkCollision(rightTrash, gameObject))
                {
                    if (!(gameObject is Baseball))
                    {
                        waveBinnedObjects++;
                        totalBinnedObj++;
                        soundManager.playSound(soundManager.landInTrash);
                        soundManager.playSound(soundManager.crowdCheer);
                        this.allGameObjects.Add(new CashRewardEffect(gameObject.position, gameTime));

                        if (gameObject is Hotdog)
                            waveCash += HOTDOG_BONUS;
                        else if (gameObject is Popcan)
                            waveCash += POPCAN_BONUS;
                        else if (gameObject is Popcorn)
                            waveCash += POPCORN_BONUS;
                        else if (gameObject is Bat)
                            waveCash += BAT_BONUS;
                        else if (gameObject is Vuvuzela)
                            waveCash += VUVUZELA_BONUS;
                        //trashInPounds += gameObject.doSomeCrazyWeightShitHere();
                        gameObject.deleteMePlease = true;
                    }
                }
            }
            // handle user input
            foreach (TouchLocation location in touches)
            {
                Vector2 touchPosition = location.Position;

                switch (location.State)
                {
                    case TouchLocationState.Pressed:
                        if (touchPosition.X <= LEFT_LAUNCH_ZONE)
                        {
                            this.thumbDownOnLeftSide(touchPosition);
                        }
                        else if (touchPosition.X >= SCREEN_WIDTH - RIGHT_LAUNCH_ZONE)
                        {
                            this.thumbDownOnRightSide(touchPosition);
                        }
                        break;

                    case TouchLocationState.Released:
                        if (touchPosition.X <= LEFT_LAUNCH_ZONE)
                        {
                            this.thumbUpOnLeftSide(touchPosition);
                        }
                        else if (touchPosition.X >= SCREEN_WIDTH - RIGHT_LAUNCH_ZONE)
                        {
                            this.thumbUpOnRightSide(touchPosition);
                        }
                        break;

                    case TouchLocationState.Moved:
                        if (touchPosition.X < SCREEN_WIDTH / 2)
                        {
                            this.draggingOnLeftSide(touchPosition);
                        }
                        else if (touchPosition.X >= SCREEN_WIDTH / 2)
                        {
                            this.draggingOnRightSide(touchPosition);
                        }
                        //if (touchPosition.X < LEFT_LAUNCH_ZONE+LEFT_LAUNCH_EXTRA_MARGIN)
                        //{
                        //    this.draggingOnLeftSide(touchPosition);
                        //}
                        //else if (touchPosition.X >= SCREEN_WIDTH - RIGHT_LAUNCH_ZONE - RIGHT_LAUNCH_EXTRA_MARGIN)
                        //{
                        //    this.draggingOnRightSide(touchPosition);
                        //}
                        break;
                }
            }

            // throw a new object on a timer
            if (waveDropTimer <= 0 && waveTimeRemaining > 0)
            {
                dropObject(); 
                // reset the waveDropTimer
                waveDropTimer = Math.Max(((double)(random.NextDouble() * WAVE_DROP_VARIANCE)) + WAVE_DROP_BASE - (WAVE_DROP_INCREASE*waveLevel), 0.2f);
            }

            // increment the timers
            waveDropTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            waveTimeRemaining -= gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// Checks if the given gameObject collide with the given gameObject
        /// </summary>
        /// <returns></returns>
        private bool checkCollision(GameObject gameObject1, GameObject gameObject2)
        {
            Rectangle objectBounds = gameObject1.getBounds();
            Rectangle otherBounds = gameObject2.getBounds(); 
            if (otherBounds.Intersects(objectBounds))
            {
                return true;
            }
            return false;
        } 

        public void showConfimationScreen()
        {
            setGameState(GameState.ConfirmUpgrade);
        }

        public void ballsofsteelpu()
        {
            confirmingUpgrade = Upgrade.BallsofSteel;
            showConfimationScreen();
        }

        public void biggerbinpu()
        {
            confirmingUpgrade = Upgrade.BiggerBins;
            showConfimationScreen();
        }

        public void umppatientpu()
        {
            confirmingUpgrade = Upgrade.UmpirePatience;
            showConfimationScreen();
        }

        public void fastballpu()
        {
            confirmingUpgrade = Upgrade.Fastball;
            showConfimationScreen();
        }

        public void cancelUpgrade()
        {
            setGameState(GameState.Shop); 
        }

        /// <summary>
        /// Confirms the confirmingUpgrade by calling the appropriate upgrade screen
        /// </summary>
        public void confirmUpgrade() 
        { 
            switch (confirmingUpgrade)
            {
                case Upgrade.Fastball:
                    upgradeFastBall();
                    break;
                case Upgrade.BiggerBins:
                    upgradeBiggerBins();
                    break;
                case Upgrade.BallsofSteel:
                    upgradeBallsOfSteel();
                    break;
                case Upgrade.UmpirePatience:
                    upgradeUmpirePatience();
                    break;
            } 
        }

        public void thumbDownOnLeftSide(Vector2 location)
        {
            if (this.currentlyDraggingOnLeftSide)
            {
                this.fireLeftBaseBall();
            }
            this.setThumbDownOnLeftSide(location);
        }

        public void setThumbDownOnLeftSide(Vector2 thumbDownLocation)
        {
            this.currentlyDraggingOnLeftSide = true;
            this.leftThumbStartPosition = thumbDownLocation;
            this.leftThumbCurrentPosition = thumbDownLocation;
        }

        public void thumbDownOnRightSide(Vector2 location)
        {
            if (this.currentlyDraggingOnRightSide)
            {
                this.fireRightBaseBall();
            }
            this.setThumbDownOnRightSide(location);
        }

        public void setThumbDownOnRightSide(Vector2 thumbDownLocation)
        {
            this.currentlyDraggingOnRightSide = true;
            this.rightThumbStartPosition = thumbDownLocation;
            this.rightThumbCurrentPosition = thumbDownLocation;
        }

        public void thumbUpOnLeftSide(Vector2 location)
        {
            if (!this.currentlyDraggingOnLeftSide)
            {
                return;
            }
            this.leftThumbCurrentPosition = location;
            this.fireLeftBaseBall();
        }

        public void thumbUpOnRightSide(Vector2 location)
        {
            if (!this.currentlyDraggingOnRightSide)
            {
                return;
            }
            this.rightThumbCurrentPosition = location;
            this.fireRightBaseBall();
        }

        public void draggingOnLeftSide(Vector2 location)
        {
            if (this.currentlyDraggingOnLeftSide)
            {
                if (location.X <= LEFT_LAUNCH_ZONE + LEFT_LAUNCH_EXTRA_MARGIN)
                {
                    this.leftThumbCurrentPosition = location;
                }
                else
                {
                    this.fireLeftBaseBall();
                }
            }
            else
            {
                if (location.X < LEFT_LAUNCH_ZONE)
                {
                    this.setThumbDownOnLeftSide(location);
                    this.leftThumbCurrentPosition = location;
                }
            } 
        }

        public void draggingOnRightSide(Vector2 location)
        {
            if (this.currentlyDraggingOnRightSide)
            {
                if (location.X >= SCREEN_WIDTH - RIGHT_LAUNCH_ZONE - RIGHT_LAUNCH_EXTRA_MARGIN)
                {
                    this.rightThumbCurrentPosition = location;
                }
                else
                {
                    this.fireRightBaseBall();
                }
            }
            else
            {
                if (location.X > SCREEN_WIDTH - RIGHT_LAUNCH_ZONE)
                {
                    this.setThumbDownOnRightSide(location);
                    this.rightThumbCurrentPosition = location;
                }
            }
        }

        public void fireLeftBaseBall()
        {
            this.fireBaseBall(this.leftThumbStartPosition, this.leftThumbCurrentPosition);
            this.currentlyDraggingOnLeftSide = false;
        }

        public void fireRightBaseBall()
        {
            this.fireBaseBall(this.rightThumbStartPosition, this.rightThumbCurrentPosition);
            this.currentlyDraggingOnRightSide = false;
        }

        public void fireBaseBall(Vector2 anchorPosition, Vector2 pulledPosition)
        {
            if (anchorPosition == null || pulledPosition == null || this.totalNumberOfActiveBaseballs >= TOTAL_NUMBER_OF_BASEBALLS_ALLOWED_ONSCREEN)
            {
                return;
            }
            Vector2 fireVector = anchorPosition - pulledPosition;
            float magnitude = fireVector.Length();
            if (magnitude > (float)LEFT_LAUNCH_ZONE)
            {
                magnitude = (float)LEFT_LAUNCH_ZONE;
            }
            fireVector.Normalize();
            if (float.IsNaN(fireVector.X) || float.IsNaN(fireVector.Y))
            {
                fireVector = new Vector2(0, 0);
            }
            fireVector *= magnitude;

            Baseball newBaseBall = new Baseball(this.physicsWorld,BASEBALL_RADIUS,BASEBALL_DENSITY*((ballsOfSteelLevel*0.7f)+1),
                pulledPosition.X, pulledPosition.Y,fireVector*this.calculateBaseBallSpeedMultiplier());
            this.activeObjects.Add(newBaseBall);
            this.allGameObjects.Add(newBaseBall);
        }

        public float calculateBaseBallSpeedMultiplier() 
        {
            return BASEBALL_SPEED*((fastBallLevel*0.3f)+1);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the game based on the current game state
            DrawGameState(currentGameState, gameTime);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the screen based on the given gameState
        /// </summary>
        /// <param name="gameState"></param>
        private void DrawGameState(GameState gameState, GameTime gameTime)
        {
            if (!spritebatchBegan) {
            spriteBatch.Begin();
            spritebatchBegan = true;
            }

            // Which game state are we drawing? 
            switch (gameState)
            {
                // Draw Logic for the Main Menu
                case GameState.MainMenu:
                    spriteBatch.Draw(titlescreen, new Vector2(0, 0), new Color(255, 255, 255, 255));
                    howtoplaybutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    startgamebutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    exitbutton.Draw(spriteBatch, new Color(255, 255, 255, 255)); 
                    break;
                case GameState.Failed:
                    spriteBatch.Draw(failscreen, new Vector2(0, 0), new Color(255, 255, 255, 255));
                    backtomenubutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    tryagainbutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    drawText("Waves Completed: " + (waveLevel - 1),
                        new Vector2(debriefingscreen.Width / 2 - statFont.MeasureString("Wave Completed:  ").X / 2, 210), Color.Red);
                    drawText("Cash Earned: $" + (totalCash + waveCash),
                        new Vector2(debriefingscreen.Width / 2 - statFont.MeasureString("Cash Earned:  ").X / 2, 240), Color.Red);
                    drawText("Objects Binned: " + waveLevel,
                        new Vector2(debriefingscreen.Width / 2 - statFont.MeasureString("Objects Binned:  ").X / 2, 270), Color.Red);
                    break;
                    /*
                case GameState.Paused:
                    // Draw the previous game state screen first
                    if (previousState != GameState.Paused)
                        DrawGameState(previousState, gameTime);
                    if (!spritebatchBegan)
                    {
                        spriteBatch.Begin();
                        spritebatchBegan = true;
                    }
                    // Draw the transparent pause screen over top
                    spriteBatch.Draw(pausescreen, new Vector2(0, 0), new Color(255, 255, 255, pausescreenAlpha));
                    resumebutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    break;
                     */
                // Draw Logic for the How To Play Screen
                case GameState.HowToPlay:
                    spriteBatch.Draw(howtoscreen, new Vector2(0, 0), new Color(255, 255, 255, 255));
                    letsplaybutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    break;
                // Draw Logic for the Debriefing Screen
                case GameState.Debriefing:
                    spriteBatch.Draw(debriefingscreen, new Vector2(0, 0), new Color(255, 255, 255, 255));
                    shopbutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    nextwavebutton.Draw(spriteBatch, new Color(255, 255, 255, 255));

                    drawText("Wave Completed: " + waveLevel, 
                        new Vector2(debriefingscreen.Width / 2 - statFont.MeasureString("Wave Completed:  ").X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y + 30), Color.Red);
                    drawText("Wave Cash: $" + waveCash, 
                        new Vector2(debriefingscreen.Width / 2 - statFont.MeasureString("Wave Cash:  ").X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y + 90), new Color(255, 255, 255, 1));
                    drawText("Wave Bonus: $" + WAVE_BONUS,
                        new Vector2(debriefingscreen.Width / 2 - statFont.MeasureString("Wave Cash:  ").X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y + 120), new Color(255, 255, 255, 1));
                    drawText("Current Cash: $" + currentCash, 
                        new Vector2(debriefingscreen.Width / 2 - statFont.MeasureString("Total Cash:  ").X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y + 150), new Color(255, 255, 255, 1));
                    drawText("Objects Trashed: " + waveBinnedObjects, 
                        new Vector2((debriefingscreen.Width / 2 - statFont.MeasureString("Objects Trashed:  ").X / 2) + 30, CONFIRMATION_TOPLEFT_LOCATION.Y + 180), new Color(255, 255, 255, 1));
                    break;

                // Draw Logic for the Shop Screen
                case GameState.Shop:
                    spriteBatch.Draw(shopscreen, new Vector2(0, 0), new Color(255, 255, 255, 255));
                    umpirepatiencebutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    biggerbinsbutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    fastballbutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    ballsofsteelbutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    okbutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    break;
                case GameState.ConfirmUpgrade:
                    spriteBatch.Draw(confirmscreen, new Vector2(0, 0), new Color(255, 255, 255, 255));
                    if (isUpgradePossible(confirmingUpgrade))
                        upgradebutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    else 
                        greyupgradebutton.Draw(spriteBatch, new Color(255, 255, 255, 255));
                    cancelbutton.Draw(spriteBatch, new Color(255, 255, 255, 255));

                    string cashMessage = "Current Cash: $" + currentCash; 
                    spriteBatch.DrawString(statFont, cashMessage, new Vector2(confirmscreen.Width/2 - statFont.MeasureString(cashMessage).X/2, 10), CASH_GREEN); 
                    switch (confirmingUpgrade) 
                    {
                        case Upgrade.Fastball:
                            spriteBatch.DrawString(statFont, FASTBALL_DESCRIPTION, new Vector2(confirmscreen.Width/2 - statFont.MeasureString(FASTBALL_DESCRIPTION).X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y), Color.White);
                            switch (fastBallLevel) 
                            { 
                                case 0:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + FASTBALL_COST_1, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break; 
                                case 1:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + FASTBALL_COST_2, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 2:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + FASTBALL_COST_3, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 3:
                                    string message = "Max Level";
                                    spriteBatch.DrawString(statFont, message, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                            }
                            break;
                        case Upgrade.BiggerBins:
                            spriteBatch.DrawString(statFont, BIGGERBIN_DESCRIPTION, new Vector2(confirmscreen.Width / 2 - statFont.MeasureString(BIGGERBIN_DESCRIPTION).X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y), Color.White);
                            switch (biggerBinsLevel)
                            {
                                case 0:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + BIGGERBIN_COST_1, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 1:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + BIGGERBIN_COST_2, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 2:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + BIGGERBIN_COST_3, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 3:
                                    spriteBatch.DrawString(statFont, "Max Level", CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                            }
                            break;
                        case Upgrade.BallsofSteel:
                            spriteBatch.DrawString(statFont, BALLSOFSTEEL_DESCRIPTION, new Vector2(confirmscreen.Width / 2 - statFont.MeasureString(BALLSOFSTEEL_DESCRIPTION).X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y), Color.White);
                            switch (ballsOfSteelLevel)
                            {
                                case 0:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + BALLSOFSTEEL_COST_1, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 1:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + BALLSOFSTEEL_COST_2, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 2:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + BALLSOFSTEEL_COST_3, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 3:
                                    spriteBatch.DrawString(statFont, "Max Level", CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                            }
                            break;
                        case Upgrade.UmpirePatience:
                            spriteBatch.DrawString(statFont, UMPIREPATIENCE_DESCRIPTION, new Vector2(confirmscreen.Width / 2 - statFont.MeasureString(UMPIREPATIENCE_DESCRIPTION).X / 2, CONFIRMATION_TOPLEFT_LOCATION.Y), Color.White);
                            switch (umpirePatienceLevel)
                            {
                                case 0:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + UMPIREPATIENCE_COST_1, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 1:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + UMPIREPATIENCE_COST_2, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 2:
                                    spriteBatch.DrawString(statFont, "Next Level: $" + UMPIREPATIENCE_COST_3, CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                                case 3:
                                    spriteBatch.DrawString(statFont, "Max Level", CONFIRMATION_BOTTOM_LOCATION, Color.White);
                                    break;
                            }
                            break;
                    }
                    
                    break;
                // Draw Logic for in Game
                case GameState.InGame:
                    DrawInGame(gameTime);
                    break;
            }

            spritebatchBegan = false;
            spriteBatch.End();
        } 

        private void drawText(String text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(hudFont, text, position, color);
        }

        /// <summary>
        /// Drawing logic for game loop
        /// </summary>
        private void DrawInGame(GameTime gameTime)
        {
            spriteBatch.Draw(backgroundscreen, new Vector2(0, 0), new Color(255, 255, 255, 1.0f));

            for (int i = 0; i < this.allGameObjects.Count; i++)
            {
                this.allGameObjects[i].draw(spriteBatch, gameTime);
            }

            //draw hud
            int umpHealth = (Int32)(((float)umpirePatience / (float)(UMPIRE_BASE_PATIENCE + umpirePatienceLevel)) * 100);

            drawText("Umpire Patience: " + umpHealth + "%", new Vector2(10, 5), Color.Maroon);
            //drawText("Wave Cash: " + waveCash, new Vector2(10, 28));
            //drawText("Total Cash: " + currentCash, new Vector2(10, 51));
            drawText("Wave: " + waveLevel, new Vector2(10, 30), Color.Maroon);


            Color guideColor = Color.White;
            if (this.totalNumberOfActiveBaseballs >= TOTAL_NUMBER_OF_BASEBALLS_ALLOWED_ONSCREEN)
            {
                guideColor = Color.Red;
            }
            //Draws the slingshot guide on the left side
            if (this.currentlyDraggingOnLeftSide)
            {
                this.leftThumbStartObject.position = this.leftThumbStartPosition;
                this.leftSuspendedBaseBall.position = this.leftThumbCurrentPosition;
                Vector2 vectorFromStartToCurrent = (this.leftThumbCurrentPosition - this.leftThumbStartPosition);
                this.leftThumbStretchLine.position = this.leftThumbStartPosition + vectorFromStartToCurrent / 2.0f;
                this.leftThumbStretchLine.rotation = (float)Math.Atan2(vectorFromStartToCurrent.Y, vectorFromStartToCurrent.X);
                this.leftThumbStretchLine.scaleToFitTheseDimensions(vectorFromStartToCurrent.Length(), this.ballStretchLineWidth);

                this.leftThumbStretchLine.draw(this.spriteBatch, gameTime, guideColor);
                this.leftThumbStartObject.draw(this.spriteBatch, gameTime, guideColor);
                this.leftSuspendedBaseBall.draw(this.spriteBatch, gameTime, guideColor);
            }

            //Draws the slingshot guide on the right side
            if (this.currentlyDraggingOnRightSide)
            {
                this.rightThumbStartObject.position = this.rightThumbStartPosition;
                this.rightSuspendedBaseBall.position = this.rightThumbCurrentPosition;
                Vector2 vectorFromStartToCurrent = (this.rightThumbCurrentPosition - this.rightThumbStartPosition);
                this.rightThumbStretchLine.position = this.rightThumbStartPosition + vectorFromStartToCurrent / 2.0f;
                this.rightThumbStretchLine.rotation = (float)Math.Atan2(vectorFromStartToCurrent.Y, vectorFromStartToCurrent.X);
                this.rightThumbStretchLine.scaleToFitTheseDimensions(vectorFromStartToCurrent.Length(), this.ballStretchLineWidth);

                this.rightThumbStartObject.draw(this.spriteBatch, gameTime, guideColor);
                this.rightThumbStretchLine.draw(this.spriteBatch, gameTime, guideColor);
                this.rightSuspendedBaseBall.draw(this.spriteBatch, gameTime, guideColor);
            }


        }

        /// <summary>
        /// Resets the game and returns to the main menu so the player can try again!
        /// </summary>
        private void backToMainMenu()
        { 
            // back to main menu
            setGameState(GameState.MainMenu);
        }

        /// <summary>
        /// Exit's the game
        /// </summary>
        private void exitGame()
        {
            // Clean up the content
            UnloadContent();

            // Exit the game
            Exit();
        }

        /// <summary>
        /// Start game
        /// </summary>
        private void startGame()
        {
            // reset upgrades, cash and wave 
            waveLevel = 0;
            currentCash = 0;
            totalCash = 0;
            fastBallLevel = 0;
            ballsOfSteelLevel = 0;
            biggerBinsLevel = 0;
            umpirePatienceLevel = 0; 
            waveLevel = 1;

            startWave();
        }

        /// <summary>
        /// Explains how to play the game
        /// </summary>
        private void howToPlay()
        {
            setGameState(GameState.HowToPlay);
        }

        /// <summary>
        /// Shows the shop where the player can purchuse upgrades
        /// </summary>
        private void showShop()
        {
            setGameState(GameState.Shop);
        }

        /// <summary>
        /// Shows the confirmation screen for the selected showConfimationScreen
        /// </summary>
        private void showConfirmUpgrade()
        {
            setGameState(GameState.ConfirmUpgrade);
        }

        /// <summary>
        /// Advances the game to the next wave
        /// </summary>
        private void nextWave()
        {
            waveLevel++;
            startWave();
        }

        /// <summary>
        /// Leaves the shop and returns to the debriefing screen
        /// </summary>
        private void exitShop()
        {
            setGameState(GameState.Debriefing);
        }


        /// <summary>
        /// Checks if the given upgrade is possible to do
        /// </summary>
        /// <param name="upgrade"></param>
        /// <returns></returns>
        private bool isUpgradePossible(Upgrade upgrade)
        {
            switch (upgrade)
            {
                case Upgrade.BallsofSteel:
                    switch (ballsOfSteelLevel)
                    {
                        case 0:
                            return (currentCash >= BALLSOFSTEEL_COST_1);
                        case 1:
                            return (currentCash >= BALLSOFSTEEL_COST_2);
                        case 2:
                            return (currentCash >= BALLSOFSTEEL_COST_3);
                        default:
                            return false;
                    }
                case Upgrade.BiggerBins:
                    switch (biggerBinsLevel)
                    {
                        case 0:
                            return (currentCash >= BIGGERBIN_COST_1);
                        case 1:
                            return (currentCash >= BIGGERBIN_COST_2);
                        case 2:
                            return (currentCash >= BIGGERBIN_COST_3);
                        default:
                            return false;
                    } 
                case Upgrade.Fastball:
                    switch (fastBallLevel)
                    {
                        case 0:
                           return (currentCash >= FASTBALL_COST_1);
                        case 1:
                            return (currentCash >= FASTBALL_COST_2);
                        case 2:
                            return (currentCash >= FASTBALL_COST_3);
                        default:
                            return false; 
                    }
                case Upgrade.UmpirePatience:
                    switch (umpirePatienceLevel)
                    {
                        case 0:
                            return (currentCash >= UMPIREPATIENCE_COST_1);   
                        case 1:
                            return (currentCash >= UMPIREPATIENCE_COST_2);
                        case 2:
                            return (currentCash >= UMPIREPATIENCE_COST_3);
                        default:
                            return false;
                    }
            }
            return false;
        } 

        /// <summary>
        /// Upgrade the player's ball speed to the next level
        /// </summary>
        private void upgradeFastBall()
        {
            switch (fastBallLevel)
            {
                case 0:
                    if (currentCash >= FASTBALL_COST_1)
                    {
                        currentCash -= FASTBALL_COST_1;
                        fastBallLevel = 1;
                        Debug.WriteLine("Fastball upgraded to level 1");
                    }
                    break; 
                case 1:
                    if (currentCash >= FASTBALL_COST_2)
                    {
                        currentCash -= FASTBALL_COST_2;
                        fastBallLevel = 2;
                        Debug.WriteLine("Fastball upgraded to level 2");
                    }
                    break;
                case 2:
                    if (currentCash >= FASTBALL_COST_3)
                    {
                        currentCash -= FASTBALL_COST_3;
                        fastBallLevel = 3; 
                        Debug.WriteLine("Fastball upgraded to level 3");
                    }
                    break;
            }
        }

        /// <summary>
        /// Upgrade the player's ball size to the next level
        /// </summary>
        private void upgradeBallsOfSteel()
        {
            switch (ballsOfSteelLevel)
            {
                case 0:
                    if (currentCash >= BALLSOFSTEEL_COST_1)
                    {
                        currentCash -= BALLSOFSTEEL_COST_1;
                        ballsOfSteelLevel = 1;
                        Debug.WriteLine("Balls of Steel upgraded to level 1");
                    }
                    break;
                case 1:
                    if (currentCash >= BALLSOFSTEEL_COST_2)
                    {
                        currentCash -= BALLSOFSTEEL_COST_2;
                        ballsOfSteelLevel = 2;
                        Debug.WriteLine("Balls of Steel upgraded to level 2");
                    }
                    break;
                case 2:
                    if (currentCash >= BALLSOFSTEEL_COST_3)
                    {
                        currentCash -= BALLSOFSTEEL_COST_3;
                        ballsOfSteelLevel = 3;
                        Debug.WriteLine("Balls of Steel upgraded to level 3");
                    }
                    break;
            }
        }

        /// <summary>
        /// Upgrade the player's trash bin size to the next level
        /// </summary>
        private void upgradeBiggerBins()
        {
            switch (biggerBinsLevel)
            {
                case 0:
                    if (currentCash >= BIGGERBIN_COST_1)
                    {
                        currentCash -= BIGGERBIN_COST_1;
                        biggerBinsLevel = 1;
                        Debug.WriteLine("Bigger Bins upgraded to level 1");
                    }
                    break;
                case 1:
                    if (currentCash >= BIGGERBIN_COST_2)
                    {
                        currentCash -= BIGGERBIN_COST_2;
                        biggerBinsLevel = 2;
                        Debug.WriteLine("Bigger Bins upgraded to level 2");
                    }
                    break;
                case 2:
                    if (currentCash >= BIGGERBIN_COST_3)
                    {
                        currentCash -= BIGGERBIN_COST_3;
                        biggerBinsLevel = 3;
                        Debug.WriteLine("Bigger Bins upgraded to level 3");
                    }
                    break;
            } 
        }

        /// <summary>
        /// Upgrade the umpire's patience to the next level
        /// </summary>
        private void upgradeUmpirePatience()
        {
            switch (umpirePatienceLevel)
            {
                case 0:
                    if (currentCash >= UMPIREPATIENCE_COST_1)
                    {
                        currentCash -= UMPIREPATIENCE_COST_1;
                        umpirePatienceLevel = 1; 
                        Debug.WriteLine("Umpire Patience upgraded to level 1");
                    }
                    break;
                case 1:
                    if (currentCash >= UMPIREPATIENCE_COST_2)
                    {
                        currentCash -= UMPIREPATIENCE_COST_2;
                        umpirePatienceLevel = 2;
                        Debug.WriteLine("Umpire Patience upgraded to level 2");
                    }
                    break;
                case 2:
                    if (currentCash >= UMPIREPATIENCE_COST_3)
                    {
                        currentCash -= UMPIREPATIENCE_COST_3;
                        umpirePatienceLevel = 3;
                        Debug.WriteLine("Umpire Patience upgraded to level 3");
                    }
                    break;
            }
        }

        /// <summary>
        /// Exits the How to Screen and returns to the Main Menu
        /// </summary>
        private void goToStartGame()
        {
            setGameState(GameState.InGame);
        }

        /*
        /// <summary>
        /// Resumes the game back to it's previous state before pausing
        /// </summary>
        private void resumeGame()
        {
            setGameState(previousState);
        }
         */

        /// <summary>
        /// Sets the currentGameState to the given gameState and updates the previousGameState appropriately. Note: you should call this method instead of directly setting currentGameState.
        /// </summary>
        /// <param name="gameState"></param>
        private void setGameState(GameState gameState)
        {
            this.currentGameState = gameState;
            /*
            if (this.currentGameState != GameState.Paused)
                this.previousState = this.currentGameState;
             */

            Debug.WriteLine("GameState changed to: " + gameState);

        }

        /// <summary>
        /// Configures and starts the wave based on the current state of the game
        /// </summary>
        private void startWave()
        {
            // Reset the Umpire's Patience for the wave
            umpirePatience = UMPIRE_BASE_PATIENCE + umpirePatienceLevel; 

            // Calculate the length of the wave
            waveTotalTime = WAVE_TIME_BASE;
            waveTimeRemaining = waveTotalTime;

            // Clear the number of binned objects
            waveBinnedObjects = 0;
            waveCash = 0;

            // Wait before starting throwing objects
            waveDropTimer = WAVE_WAIT_TIME;

            // Reset active objects
            for (int i = 0; i < this.activeObjects.Count; i++)
            {
                this.activeObjects[i].deleteFromPhysicsSimulation(this.physicsWorld);
            } 
            this.activeObjects = new List<DynamicPhysicsGameObject>(); 

            // Add the umpire, trash bins, and foul poles
            this.umpire = new Umpire(UMPIRE_SPAWN, UMPIRE_WIDTH, UMPIRE_HEIGHT);
            this.allGameObjects.Add(umpire); 
            this.leftTrash = new Trashbin(TRASHBIN_LEFT_SPAWN, TRASHBIN_WIDTH * (float)((biggerBinsLevel * 0.15f + 1)), TRASHBIN_HEIGHT);
            this.allGameObjects.Add(leftTrash);
            this.rightTrash = new Trashbin(TRASHBIN_RIGHT_SPAWN, TRASHBIN_WIDTH * (float)((biggerBinsLevel * 0.15f + 1)), TRASHBIN_HEIGHT);
            this.allGameObjects.Add(rightTrash);
            this.allGameObjects.Add(new Foulpole(FOULPOLE_LEFT_SPAWN, FOULPOLE_WIDTH, FOULPOLE_HEIGHT, true));
            this.allGameObjects.Add(new Foulpole(FOULPOLE_RIGHT_SPAWN, FOULPOLE_WIDTH, FOULPOLE_HEIGHT, false));

            //Make sure the ball guide doesn't stick around from last frame
            this.currentlyDraggingOnLeftSide = false;
            this.currentlyDraggingOnRightSide = false;

            // Start the wave
            setGameState(GameState.InGame); 

        }

        /// <summary>
        /// Throws a random object based on the current state of the game
        /// </summary>
        private void dropObject()
        {
            Debug.WriteLine("Dropping Object");
            float positionX = this.random.Next(0+LEFT_LAUNCH_ZONE, SCREEN_WIDTH-RIGHT_LAUNCH_ZONE);

            float positionY = SPAWN_HEIGHT; 

            float rotation = (float)(this.random.NextDouble() * 2.0f * Math.PI);

            int xdiff = (SCREEN_WIDTH / 2) - (int)positionX;
            //if (xdiff > 0)
                //xdiff = this.random.Next(xdiff, xdiff*SPAWN_X_VELOCITY_RANGE);
            //else
                //xdiff = this.random.Next(xdiff*SPAWN_X_VELOCITY_RANGE, xdiff);
            
            int ydiff = this.random.Next(0, SPAWN_Y_VELOCITY_RANGE);
            Vector2 randomStartVelocity = new Vector2(xdiff/10, ydiff); 

            // Get a number between 0-100
            int number;
            // Object to spawn
            DynamicPhysicsGameObject newlySpawnedObject;
            
                // bat 
            if ((number = random.Next(0, 100)) <= BAT_FREQUENCY_BASE + (FREQUENCY_INCREMENT * waveLevel))
            {
                newlySpawnedObject = (new Bat(physicsWorld, BAT_WIDTH, BAT_HEIGHT, positionX, positionY, rotation, BAT_DENSITY));
            }        
                // vuvuzela
            else if ((number = random.Next(0, 100)) <= VUVUZELA_FREQUENCY_BASE + (FREQUENCY_INCREMENT * waveLevel))
            {
                newlySpawnedObject= (new Vuvuzela(physicsWorld, VUVUZELA_WIDTH, VUVUZELA_HEIGHT, positionX, positionY, rotation, VUVUZELA_DENSITY));
            }
                // popcorn
            else if ((number = random.Next(0, 100)) <= POPCORN_FREQUENCY_BASE + (FREQUENCY_INCREMENT * waveLevel))
            {
                newlySpawnedObject = (new Popcorn(physicsWorld, POPCORN_WIDTH, POPCORN_HEIGHT, positionX, positionY, rotation, POPCORN_DENSITY));
            }
                // popcan
            else if ((number = random.Next(0, 100)) <= POPCAN_FREQUENCY_BASE + (FREQUENCY_INCREMENT * waveLevel))
            {
                newlySpawnedObject = (new Popcan(physicsWorld, POPCAN_WIDTH, POPCAN_HEIGHT, positionX, positionY, rotation, POPCAN_DENSITY));
            }
                // hot dog 
            else
            {
                newlySpawnedObject = (new Hotdog(physicsWorld, HOTDOG_WIDTH, HOTDOG_HEIGHT, positionX, positionY, rotation, HOTDOG_DENSITY));
            }

            // set the velocity
            newlySpawnedObject.setVelocity(randomStartVelocity);

            this.activeObjects.Add(newlySpawnedObject);
            this.allGameObjects.Add(newlySpawnedObject);
        }



        /////////////////////////////////////////////////////////////////////////////
        ////
        ////        PHONE INTERUPT HANDLER METHODS
        ////
        ////////////////////////////////////////////////////////////////////////////

        //sets
        void service_Activated(object sender, Microsoft.Phone.Shell.ActivatedEventArgs e)
        {
            recoverFromForceQuit = true;
        }

        /// <summary>
        /// This code handles unexpected game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnExiting(object sender, EventArgs args)
        {
            if (currentGameState == GameState.InGame /* || currentGameState == GameState.Paused */
                    || currentGameState == GameState.Shop)
            {
                IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();

                // open isolated storage, and write the savefile.
                IsolatedStorageFileStream fs = null;
                fs = savegameStorage.OpenFile(saveFileName, System.IO.FileMode.Create);

                StreamWriter writer = new StreamWriter(fs);

                if (writer != null)
                {
                    // just overwrite the existing info for this example.
                    if (currentGameState == GameState.InGame /*|| currentGameState == GameState.Paused */)
                    {
                        writer.WriteLine("paused"); //write current game state to file
                    } 
                    else
                        writer.WriteLine("shop"); //write current game state to file

                    //write player stats to file
                    writer.WriteLine(fastBallLevel); // Fast ball showConfimationScreen level
                    writer.WriteLine(ballsOfSteelLevel); // Balls of steel showConfimationScreen level
                    writer.WriteLine(biggerBinsLevel); // Bigger bins showConfimationScreen level
                    writer.WriteLine(umpirePatienceLevel); // Umpire patience showConfimationScreen level
                    writer.WriteLine(currentCash); // Total amount of player cash to spend on upgrades

                    //save level stats
                    writer.WriteLine(waveLevel);

                    //close writer pipe
                    writer.Close();

                }

                //close file pipe
                fs.Close();

            }

            base.OnExiting(sender, args);
        }
    }
}
