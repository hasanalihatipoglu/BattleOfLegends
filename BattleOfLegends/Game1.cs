using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using BoLLogic;
using System.IO;

namespace BattleOfLegends;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Grid _grid;
    private Board _board;
    private GameState _gameState;
    private Texture2D _pixelTexture;
    private SpriteFont _font;
    private Dictionary<string, Texture2D> _unitTextures;
    private Dictionary<string, Texture2D> _tileTextures;
    private Dictionary<string, Texture2D> _cardTextures;
    private Texture2D _backgroundTexture;
    private Texture2D _cardBackTexture;
    private Dictionary<string, SoundEffect> _soundEffects;

    // Cached card lists to avoid LINQ allocations every frame
    private List<Card> _romeCardsCache;
    private List<Card> _carthageCardsCache;
    private bool _cardCacheDirty = true;

    // Animation time tracking
    private double _totalElapsedSeconds = 0;

    // Game constants
    private const int BOARD_ROWS = 9;
    private const int BOARD_COLS = 13;
    private const float HEX_HEIGHT = 80f;

    private MouseState _previousMouseState;
    private int _selectedRow = -1;
    private int _selectedCol = -1;

    // Camera/viewport controls
    private Vector2 _cameraPosition = Vector2.Zero;
    private float _zoomLevel = 1.0f;
    private const float MIN_ZOOM = 0.3f;
    private const float MAX_ZOOM = 3.0f;
    private const float ZOOM_SPEED = 0.1f;
    private bool _isPanning = false;
    private Vector2 _panStartPosition;

    // Golden ratio for unit display
    private const float GOLDEN_RATIO = 1.8f;
    private const float UNIT_BASE_SIZE = 30f;

    // Card display constants
    private const float CARD_WIDTH = 80f;
    private const float CARD_HEIGHT = 120f;
    private const float CARD_SPACING = 10f;
    private const float CARD_HAND_Y = 20f; // Distance from bottom of screen

    // Card panel constants
    private const float PANEL_COLLAPSED_HEIGHT = 40f; // Height when collapsed (showing arrow button)
    private const float PANEL_EXPANDED_HEIGHT = 200f; // Height when expanded (showing all cards)
    private const float ARROW_BUTTON_SIZE = 30f;
    private const float CARD_PANEL_OFFSET = 50f; // Distance cards move out of panel when selected for hand

    // Panel state tracking
    private bool _romePanelExpanded = false;
    private bool _carthagePanelExpanded = false;

    // Message box state
    private string _currentMessage = null;
    private double _messageDisplayTime = 0;
    private const double MESSAGE_DISPLAY_DURATION = 1.0; // seconds
    private const float MESSAGE_BOX_WIDTH = 500f;
    private const float MESSAGE_BOX_HEIGHT = 150f;
    private const float MESSAGE_BOX_PADDING = 20f;

    // Phase tracker constants (vertical layout)
    private const float PHASE_BOX_WIDTH = 140f;
    private const float PHASE_BOX_HEIGHT = 50f;
    private const float PHASE_BUTTON_SIZE = 35f;
    private const float PHASE_TRACKER_LEFT_X = 10f; // Distance from left edge
    private const float PHASE_TRACKER_RIGHT_MARGIN = 10f; // Distance from right edge
    private const float PHASE_SPACING = 5f;

    // Roll button constants
    private const float ROLL_BUTTON_WIDTH = 120f;
    private const float ROLL_BUTTON_HEIGHT = 60f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window size
        _graphics.PreferredBackBufferWidth = 1400;
        _graphics.PreferredBackBufferHeight = 900;
    }

    protected override void Initialize()
    {
        // Initialize grid
        _grid = new Grid(BOARD_ROWS, BOARD_COLS, HEX_HEIGHT);

        // Load scenario from JSON file
        string scenarioPath = System.IO.Path.Combine(Content.RootDirectory, "ticinus.json");
        GameManager.Instance.LoadGame(scenarioPath);

        // Initialize board
        System.Diagnostics.Debug.WriteLine("========== GAME1: CREATING BOARD ==========");
        _board = new Board();
        _board.Initialize();
        System.Diagnostics.Debug.WriteLine($"========== GAME1: BOARD INITIALIZED WITH {_board.Cards.Count} CARDS ==========");
        GameManager.Instance.CurrentBoard = _board;
        PathFinder.Instance.CurrentBoard = _board;

        // Subscribe to message events
        MessageController.Instance.Message += OnMessage;

        // Subscribe to sound events
        SoundController.Instance.Play += OnPlaySound;

        // Subscribe to card state changes to update cache
        System.Diagnostics.Debug.WriteLine($"========== GAME1: SUBSCRIBING TO {_board.Cards.Count} CARD STATE CHANGES ==========");
        foreach (var card in _board.Cards)
        {
            card.ChangeState += OnCardStateChanged;
            System.Diagnostics.Debug.WriteLine($"  Subscribed to {card.Type} ({card.Faction}) state changes");
        }

        // CRITICAL: Ensure all cards are subscribed to turn phase AND player changes
        System.Diagnostics.Debug.WriteLine($"========== GAME1: RE-SUBSCRIBING CARDS TO TURN EVENTS ==========");
        foreach (var card in _board.Cards)
        {
            // Unsubscribe first to avoid duplicates, then resubscribe
            TurnManager.Instance.ChangeTurnPhase -= card.On_Update;
            TurnManager.Instance.ChangeTurnPhase += card.On_Update;
            TurnManager.Instance.ChangePlayer -= card.On_Update;
            TurnManager.Instance.ChangePlayer += card.On_Update;
            System.Diagnostics.Debug.WriteLine($"  Resubscribed {card.Type} ({card.Faction}) to turn phase and player change events");
        }

        // Trigger initial card state update
        System.Diagnostics.Debug.WriteLine($"========== GAME1: TRIGGERING INITIAL TURN PHASE UPDATE ==========");
        TurnManager.Instance.AdvanceTurnPhase();

        // Initialize game state
        _gameState = new GameState(_board);

        // Initialize card cache
        UpdateCardCache();

        // Center camera on the board
        CenterCameraOnBoard();

        base.Initialize();
    }

    private void UpdateCardCache()
    {
        if (_board?.Cards == null)
        {
            _romeCardsCache = new List<Card>();
            _carthageCardsCache = new List<Card>();
            return;
        }

        System.Diagnostics.Debug.WriteLine("========== UPDATING CARD CACHE ==========");
        System.Diagnostics.Debug.WriteLine($"Total cards: {_board.Cards.Count}");
        foreach (var card in _board.Cards)
        {
            System.Diagnostics.Debug.WriteLine($"  {card.Type} ({card.Faction}) - State: {card.State}");
        }

        _romeCardsCache = _board.Cards.Where(c => c.Faction == PlayerType.Rome &&
                                                   (c.State == CardState.InDeck ||
                                                    c.State == CardState.InHand ||
                                                    c.State == CardState.ReadyToPlay)).ToList();
        _carthageCardsCache = _board.Cards.Where(c => c.Faction == PlayerType.Carthage &&
                                                       (c.State == CardState.InDeck ||
                                                        c.State == CardState.InHand ||
                                                        c.State == CardState.ReadyToPlay)).ToList();

        System.Diagnostics.Debug.WriteLine($"Rome cards in cache: {_romeCardsCache.Count}");
        System.Diagnostics.Debug.WriteLine($"Carthage cards in cache: {_carthageCardsCache.Count}");
        _cardCacheDirty = false;
    }

    private void CenterCameraOnBoard()
    {
        // Calculate the center hex position (middle of the board)
        int centerRow = BOARD_ROWS / 2;
        int centerCol = BOARD_COLS / 2;

        // Get the world position of the center hex
        var centerPoints = _grid.HexToPoints(centerRow, centerCol);
        float centerX = (centerPoints[0].X + centerPoints[3].X) / 2;
        float centerY = (centerPoints[1].Y + centerPoints[4].Y) / 2;

        // Set camera position to center the board in the viewport
        _cameraPosition = new Vector2(
            centerX - (_graphics.PreferredBackBufferWidth / 2f / _zoomLevel),
            centerY - (_graphics.PreferredBackBufferHeight / 2f / _zoomLevel)
        );
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture for drawing lines
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Load textures
        _unitTextures = new Dictionary<string, Texture2D>();
        _tileTextures = new Dictionary<string, Texture2D>();
        _cardTextures = new Dictionary<string, Texture2D>();

        // Load unit textures
        try
        {
            _unitTextures["King-Blue"] = Content.Load<Texture2D>("King - Blue");
            _unitTextures["King-Red"] = Content.Load<Texture2D>("King - Red");
            _unitTextures["Cavalry-Blue"] = Content.Load<Texture2D>("Cavalry - Horse Infantry - Blue");
            _unitTextures["Cavalry-Red"] = Content.Load<Texture2D>("Cavalry - Horse Infantry - Red");
            _unitTextures["Horse-Blue"] = Content.Load<Texture2D>("Cavalry - Horse Light - Blue");
            _unitTextures["Horse-Red"] = Content.Load<Texture2D>("Cavalry - Horse Light - Red");
            _unitTextures["Infantry-Blue"] = Content.Load<Texture2D>("Melee - Infrantry - Blue");
            _unitTextures["Infantry-Red"] = Content.Load<Texture2D>("Melee - Infrantry - Red");
            _unitTextures["Pikes-Blue"] = Content.Load<Texture2D>("Melee - Pikes - Blue");
            _unitTextures["Pikes-Red"] = Content.Load<Texture2D>("Melee - Pikes - Red");
            _unitTextures["Archer-Blue"] = Content.Load<Texture2D>("Missile - Archers - Blue");
            _unitTextures["Archer-Red"] = Content.Load<Texture2D>("Missile - Archers - Red");
            _unitTextures["Spear-Blue"] = Content.Load<Texture2D>("Missile - Spear - Blue");
            _unitTextures["Spear-Red"] = Content.Load<Texture2D>("Missile - Spear - Red");

            // Load tile textures
            _tileTextures["water"] = Content.Load<Texture2D>("water");
            _tileTextures["hill"] = Content.Load<Texture2D>("hill");

            // Load background
            _backgroundTexture = Content.Load<Texture2D>("paperboard-yellow-texture");

            // Try to load card textures (optional - will use fallback rendering if missing)
            try
            {
                _cardTextures["Withdraw"] = Content.Load<Texture2D>("Withdraw");
                _cardTextures["CavalryCharge"] = Content.Load<Texture2D>("CavalryCharge");
                _cardTextures["CavalryCounter"] = Content.Load<Texture2D>("CavalryCounter");
                _cardTextures["CavalryPursue"] = Content.Load<Texture2D>("CavalryPursue");
                _cardTextures["Flanking"] = Content.Load<Texture2D>("Flanking");
                _cardTextures["FirstStrike"] = Content.Load<Texture2D>("FirstStrike");
                _cardTextures["Envelopment"] = Content.Load<Texture2D>("Envelopment");
                _cardTextures["HitAndRun"] = Content.Load<Texture2D>("HitAndRun");
                _cardTextures["Skirmish"] = Content.Load<Texture2D>("Skirmish");
                _cardTextures["Advance"] = Content.Load<Texture2D>("Advance");
                _cardTextures["Leadership"] = Content.Load<Texture2D>("Leadership");
                _cardTextures["MixedOrder"] = Content.Load<Texture2D>("MixedOrder");
                _cardBackTexture = Content.Load<Texture2D>("CardBack");
                System.Diagnostics.Debug.WriteLine("Card textures loaded successfully");
            }
            catch (Exception cardEx)
            {
                System.Diagnostics.Debug.WriteLine($"Card textures not found, using fallback rendering: {cardEx.Message}");
                _cardBackTexture = null;
            }

            // Try to load font (if it fails, we'll use bitmap font fallback)
            try
            {
                _font = Content.Load<SpriteFont>("ANTIQUE");
                System.Diagnostics.Debug.WriteLine("SpriteFont loaded successfully");
            }
            catch (Exception fontEx)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load SpriteFont, using bitmap fallback: {fontEx.Message}");
                _font = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading textures: {ex.Message}");
        }

        // Load sound effects (outside try-catch to see any errors clearly)
        try
        {
            _soundEffects = new Dictionary<string, SoundEffect>();
            _soundEffects["click_button"] = Content.Load<SoundEffect>("click_button");
            _soundEffects["flip_card"] = Content.Load<SoundEffect>("flip_card");
            _soundEffects["shake_dice"] = Content.Load<SoundEffect>("shake_dice");
            _soundEffects["break_glass"] = Content.Load<SoundEffect>("break_glass");
            System.Diagnostics.Debug.WriteLine("Sound effects loaded successfully");
        }
        catch (Exception soundEx)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading sound effects: {soundEx.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {soundEx.StackTrace}");
            _soundEffects = new Dictionary<string, SoundEffect>(); // Initialize empty to prevent null reference
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Update message display timer
        if (_currentMessage != null)
        {
            _messageDisplayTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_messageDisplayTime >= MESSAGE_DISPLAY_DURATION)
            {
                _currentMessage = null;
                _messageDisplayTime = 0;
            }
        }

        // Update card cache if needed (card states may have changed)
        if (_cardCacheDirty)
        {
            UpdateCardCache();
        }

        // Handle mouse input
        MouseState mouseState = Mouse.GetState();

        // Handle zoom with mouse wheel
        if (mouseState.ScrollWheelValue != _previousMouseState.ScrollWheelValue)
        {
            float zoomDelta = (mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue) * ZOOM_SPEED * 0.001f;
            float oldZoom = _zoomLevel;
            _zoomLevel = Math.Clamp(_zoomLevel + zoomDelta, MIN_ZOOM, MAX_ZOOM);

            // Zoom towards mouse position
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 worldPosBeforeZoom = ScreenToWorld(mousePos, oldZoom);
            Vector2 worldPosAfterZoom = ScreenToWorld(mousePos, _zoomLevel);
            _cameraPosition += worldPosBeforeZoom - worldPosAfterZoom;
        }

        // Handle panning with middle mouse button
        if (mouseState.MiddleButton == ButtonState.Pressed)
        {
            if (!_isPanning)
            {
                _isPanning = true;
                _panStartPosition = new Vector2(mouseState.X, mouseState.Y);
            }
            else
            {
                Vector2 currentPos = new Vector2(mouseState.X, mouseState.Y);
                Vector2 delta = currentPos - _panStartPosition;
                _cameraPosition -= delta / _zoomLevel;
                _panStartPosition = currentPos;
            }
        }
        else
        {
            _isPanning = false;
        }

        // Handle hover for path preview
        Vector2 hoverWorldPos = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y), _zoomLevel);
        _grid.PointToHex(hoverWorldPos.X, hoverWorldPos.Y, out int hoverRow, out int hoverCol);

        if (hoverRow >= 0 && hoverRow < BOARD_ROWS && hoverCol >= 0 && hoverCol < BOARD_COLS)
        {
            var hoverTile = _board[new Position(hoverRow, hoverCol)];
            if (hoverTile != null)
            {
                hoverTile.OnHover(_board);
            }
        }

        // Handle left click
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            System.Diagnostics.Debug.WriteLine($"Left click detected at ({mouseState.X}, {mouseState.Y})");

            // Check if clicking on Roll button first (screen space)
            if (HandleRollButtonClick(mouseState.X, mouseState.Y))
            {
                // Roll button was clicked, don't process other clicks
                System.Diagnostics.Debug.WriteLine("Roll button handled the click");
            }
            // Check if clicking on current player tracker (screen space)
            else if (HandleCurrentPlayerTrackerClick(mouseState.X, mouseState.Y))
            {
                // Current player tracker was clicked, don't process other clicks
            }
            // Check if clicking on game phase tracker (screen space)
            else if (HandleGamePhaseTrackerClick(mouseState.X, mouseState.Y))
            {
                // Game phase tracker was clicked, don't process other clicks
            }
            // Check if clicking on turn phase tracker (screen space)
            else if (HandlePhaseTrackerClick(mouseState.X, mouseState.Y))
            {
                // Phase tracker was clicked, don't process other clicks
            }
            // Check if clicking on arrow buttons to expand/collapse card panels
            else if (HandleArrowButtonClick(mouseState.X, mouseState.Y))
            {
                // Arrow button was clicked, don't process other clicks
            }
            // Check if clicking on a card (screen space, not world space)
            else if (HandleCardClick(mouseState.X, mouseState.Y))
            {
                // Card was clicked, don't process hex click
            }
            else
            {
                // Convert screen position to world position for hex selection
                Vector2 worldPos = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y), _zoomLevel);

                // Convert mouse position to hex coordinates
                _grid.PointToHex(worldPos.X, worldPos.Y, out int row, out int col);

                if (row >= 0 && row < BOARD_ROWS && col >= 0 && col < BOARD_COLS)
                {
                    _selectedRow = row;
                    _selectedCol = col;

                    // Get the tile at this position
                    var tile = _board[new Position(row, col)];
                    if (tile != null)
                    {
                        // Call the tile's OnClick method which will handle unit clicks internally
                        tile.OnClick(_board);

                        // Display information in window title
                        if (tile.Unit != null)
                        {
                            var unit = tile.Unit;
                            string unitInfo = $"Battle of Legends - {unit.GetType().Name} ({unit.Faction}) at ({row},{col}) | Health: {unit.Health.GetHealth()}/{unit.Strength} | State: {unit.State}";
                            Window.Title = unitInfo;
                            System.Diagnostics.Debug.WriteLine(unitInfo);
                        }
                        else
                        {
                            // Show tile information
                            string tileInfo = $"Battle of Legends - {tile.GetType().Name} at ({row}, {col})";
                            Window.Title = tileInfo;
                        }
                    }
                }
            }
        }

        _previousMouseState = mouseState;

        base.Update(gameTime);
    }

    private bool HandlePhaseTrackerClick(int mouseX, int mouseY)
    {
        // Get all turn phases except None
        var phases = System.Enum.GetValues(typeof(TurnPhase)).Cast<TurnPhase>().Where(p => p != TurnPhase.None).ToArray();

        // Calculate vertical tracker position (right side of screen)
        float startY = 200;
        float trackerX = _graphics.PreferredBackBufferWidth - PHASE_BOX_WIDTH - PHASE_TRACKER_RIGHT_MARGIN;

        // Check up button (at top)
        Rectangle upButton = new Rectangle(
            (int)trackerX,
            (int)startY,
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);

        if (upButton.Contains(mouseX, mouseY))
        {
            // Go to previous phase
            int currentIndex = Array.IndexOf(phases, TurnManager.Instance.CurrentTurnPhase);
            if (currentIndex > 0)
            {
                TurnManager.Instance.CurrentTurnPhase = phases[currentIndex - 1];
            }
            else
            {
                TurnManager.Instance.CurrentTurnPhase = phases[phases.Length - 1];
            }
            return true;
        }

        // Check each phase box for direct selection
        float phaseStartY = startY + PHASE_BUTTON_SIZE + PHASE_SPACING;
        for (int i = 0; i < phases.Length; i++)
        {
            Rectangle phaseBox = new Rectangle(
                (int)trackerX,
                (int)(phaseStartY + i * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
                (int)PHASE_BOX_WIDTH,
                (int)PHASE_BOX_HEIGHT);

            if (phaseBox.Contains(mouseX, mouseY))
            {
                TurnManager.Instance.CurrentTurnPhase = phases[i];
                return true;
            }
        }

        // Check down button (at bottom)
        Rectangle downButton = new Rectangle(
            (int)trackerX,
            (int)(phaseStartY + phases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);

        if (downButton.Contains(mouseX, mouseY))
        {
            // Go to next phase
            TurnManager.Instance.AdvanceTurnPhase(1);
            return true;
        }

        return false;
    }

    private bool HandleGamePhaseTrackerClick(int mouseX, int mouseY)
    {
        // Get all game phases
        var phases = System.Enum.GetValues(typeof(GamePhase)).Cast<GamePhase>().ToArray();

        // Calculate vertical tracker position (left side of screen)
        float startY = 200;
        float totalHeight = PHASE_BUTTON_SIZE + PHASE_SPACING + (phases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)) + PHASE_BUTTON_SIZE + 20;

        // Check up button (at top)
        Rectangle upButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)startY,
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);

        if (upButton.Contains(mouseX, mouseY))
        {
            // Go to previous phase
            int currentIndex = Array.IndexOf(phases, TurnManager.Instance.CurrentGamePhase);
            if (currentIndex > 0)
            {
                TurnManager.Instance.CurrentGamePhase = phases[currentIndex - 1];
            }
            else
            {
                TurnManager.Instance.CurrentGamePhase = phases[phases.Length - 1];
            }
            return true;
        }

        // Check each phase box for direct selection
        float phaseStartY = startY + PHASE_BUTTON_SIZE + PHASE_SPACING;
        for (int i = 0; i < phases.Length; i++)
        {
            Rectangle phaseBox = new Rectangle(
                (int)PHASE_TRACKER_LEFT_X,
                (int)(phaseStartY + i * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
                (int)PHASE_BOX_WIDTH,
                (int)PHASE_BOX_HEIGHT);

            if (phaseBox.Contains(mouseX, mouseY))
            {
                TurnManager.Instance.CurrentGamePhase = phases[i];
                return true;
            }
        }

        // Check down button (at bottom)
        Rectangle downButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)(phaseStartY + phases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);

        if (downButton.Contains(mouseX, mouseY))
        {
            // Go to next phase
            int currentIndex = Array.IndexOf(phases, TurnManager.Instance.CurrentGamePhase);
            if (currentIndex < phases.Length - 1)
            {
                TurnManager.Instance.CurrentGamePhase = phases[currentIndex + 1];
            }
            else
            {
                TurnManager.Instance.CurrentGamePhase = phases[0];
            }
            return true;
        }

        return false;
    }

    private bool HandleCurrentPlayerTrackerClick(int mouseX, int mouseY)
    {
        System.Diagnostics.Debug.WriteLine($"HandleCurrentPlayerTrackerClick called with mouse position: ({mouseX}, {mouseY})");

        // Get all turn phases to calculate where TurnPhase tracker ends
        var turnPhases = System.Enum.GetValues(typeof(TurnPhase)).Cast<TurnPhase>().Where(p => p != TurnPhase.None).ToArray();
        var gamePhases = System.Enum.GetValues(typeof(GamePhase)).Cast<GamePhase>().ToArray();

        // Calculate vertical position (left side, below TurnPhase tracker)
        float gamePhaseHeight = 200 + PHASE_BUTTON_SIZE + PHASE_SPACING + (gamePhases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)) + PHASE_BUTTON_SIZE + 20;
        float turnPhaseHeight = PHASE_BUTTON_SIZE + PHASE_SPACING + (turnPhases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)) + PHASE_BUTTON_SIZE + 20;
        float startY = gamePhaseHeight + 50;

        // Check Rome button
        Rectangle romeButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)startY,
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BOX_HEIGHT);

        System.Diagnostics.Debug.WriteLine($"Rome button bounds: {romeButton}");

        if (romeButton.Contains(mouseX, mouseY))
        {
            System.Diagnostics.Debug.WriteLine($"Rome button clicked. Current player: {TurnManager.Instance.CurrentPlayer}");
            TurnManager.Instance.SetCurrentPlayer(PlayerType.Rome);
            System.Diagnostics.Debug.WriteLine($"After SetCurrentPlayer. Current player: {TurnManager.Instance.CurrentPlayer}");
            return true;
        }

        // Check Carthage button
        Rectangle carthageButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)(startY + PHASE_BOX_HEIGHT + PHASE_SPACING),
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BOX_HEIGHT);

        System.Diagnostics.Debug.WriteLine($"Carthage button bounds: {carthageButton}");

        if (carthageButton.Contains(mouseX, mouseY))
        {
            System.Diagnostics.Debug.WriteLine($"Carthage button clicked. Current player: {TurnManager.Instance.CurrentPlayer}");
            TurnManager.Instance.SetCurrentPlayer(PlayerType.Carthage);
            System.Diagnostics.Debug.WriteLine($"After SetCurrentPlayer. Current player: {TurnManager.Instance.CurrentPlayer}");
            return true;
        }

        return false;
    }

    private bool HandleRollButtonClick(int mouseX, int mouseY)
    {
        // Calculate Roll button position (center bottom of screen, above Rome cards)
        float buttonX = (_graphics.PreferredBackBufferWidth - ROLL_BUTTON_WIDTH) / 2;
        float buttonY = _graphics.PreferredBackBufferHeight - CARD_HEIGHT - CARD_HAND_Y - ROLL_BUTTON_HEIGHT - 20;

        Rectangle rollButton = new Rectangle(
            (int)buttonX,
            (int)buttonY,
            (int)ROLL_BUTTON_WIDTH,
            (int)ROLL_BUTTON_HEIGHT);

        if (rollButton.Contains(mouseX, mouseY))
        {
            // Check if there's a combat to resolve
            var combatManager = CombatManager.Instance;
            if (combatManager.Attacker != null && combatManager.Target != null)
            {
                // Resolve combat
                combatManager.Combat(combatManager.Type);
            }
            return true;
        }

        return false;
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        // Display message in game UI
        _currentMessage = e.Message;
        _messageDisplayTime = 0;
        System.Diagnostics.Debug.WriteLine($"Message: {e.Message}");
    }

    private void OnCardStateChanged(object sender, EventArgs e)
    {
        // Mark card cache as dirty when any card changes state
        _cardCacheDirty = true;

        // Also log for debugging
        if (sender is Card card)
        {
            System.Diagnostics.Debug.WriteLine($"Card state changed: {card.Type} ({card.Faction}) -> {card.State}");
        }
    }

    private void OnPlaySound(object sender, SoundEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnPlaySound called for: {e.Sound}");

        // Play the requested sound effect
        if (_soundEffects != null && _soundEffects.ContainsKey(e.Sound))
        {
            try
            {
                _soundEffects[e.Sound].Play();
                System.Diagnostics.Debug.WriteLine($"Successfully played: {e.Sound}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound {e.Sound}: {ex.Message}");
            }
        }
        else
        {
            var availableSounds = _soundEffects != null ? string.Join(", ", _soundEffects.Keys) : "none";
            System.Diagnostics.Debug.WriteLine($"Sound not found: {e.Sound}. Available sounds: {availableSounds}");
        }
    }

    private bool HandleArrowButtonClick(int mouseX, int mouseY)
    {
        // Rome panel arrow button (bottom of screen)
        float romeArrowY = _graphics.PreferredBackBufferHeight - PANEL_COLLAPSED_HEIGHT / 2 - ARROW_BUTTON_SIZE / 2;
        float arrowX = (_graphics.PreferredBackBufferWidth - ARROW_BUTTON_SIZE) / 2;

        Rectangle romeArrowButton = new Rectangle(
            (int)arrowX,
            (int)romeArrowY,
            (int)ARROW_BUTTON_SIZE,
            (int)ARROW_BUTTON_SIZE);

        if (romeArrowButton.Contains(mouseX, mouseY))
        {
            _romePanelExpanded = !_romePanelExpanded;
            System.Diagnostics.Debug.WriteLine($"Rome panel toggled: {(_romePanelExpanded ? "expanded" : "collapsed")}");
            return true;
        }

        // Carthage panel arrow button (top of screen)
        float carthageArrowY = PANEL_COLLAPSED_HEIGHT / 2 - ARROW_BUTTON_SIZE / 2;

        Rectangle carthageArrowButton = new Rectangle(
            (int)arrowX,
            (int)carthageArrowY,
            (int)ARROW_BUTTON_SIZE,
            (int)ARROW_BUTTON_SIZE);

        if (carthageArrowButton.Contains(mouseX, mouseY))
        {
            _carthagePanelExpanded = !_carthagePanelExpanded;
            System.Diagnostics.Debug.WriteLine($"Carthage panel toggled: {(_carthagePanelExpanded ? "expanded" : "collapsed")}");
            return true;
        }

        return false;
    }

    private bool HandleCardClick(int mouseX, int mouseY)
    {
        if (_board?.Cards == null) return false;

        // Calculate card positions based on panel state
        float romeY = GetRomeCardYPosition();
        float carthageY = GetCarthageCardYPosition();

        if (CheckCardHandClick(_romeCardsCache, mouseX, mouseY, romeY, PlayerType.Rome))
        {
            _cardCacheDirty = true; // Mark cache as dirty after card interaction
            return true;
        }

        if (CheckCardHandClick(_carthageCardsCache, mouseX, mouseY, carthageY, PlayerType.Carthage))
        {
            _cardCacheDirty = true; // Mark cache as dirty after card interaction
            return true;
        }

        return false;
    }

    private float GetRomeCardYPosition()
    {
        // Rome cards are at bottom
        if (_romePanelExpanded)
        {
            // Panel is expanded - cards are inside the panel
            return _graphics.PreferredBackBufferHeight - PANEL_EXPANDED_HEIGHT + 20;
        }
        else
        {
            // Panel is collapsed - just below the collapsed panel
            return _graphics.PreferredBackBufferHeight - PANEL_COLLAPSED_HEIGHT - CARD_HEIGHT - 10;
        }
    }

    private float GetCarthageCardYPosition()
    {
        // Carthage cards are at top
        if (_carthagePanelExpanded)
        {
            // Panel is expanded - cards are inside the panel
            return PANEL_EXPANDED_HEIGHT - CARD_HEIGHT - 20;
        }
        else
        {
            // Panel is collapsed - just above the collapsed panel
            return PANEL_COLLAPSED_HEIGHT + 10;
        }
    }

    private bool CheckCardHandClick(List<Card> cards, int mouseX, int mouseY, float yPosition, PlayerType faction)
    {
        if (cards.Count == 0) return false;

        // Separate cards by state: InDeck vs InHand
        var deckCards = cards.Where(c => c.State == CardState.InDeck).ToList();
        var handCards = cards.Where(c => c.State == CardState.InHand).ToList();

        // Determine if panel is expanded
        bool panelExpanded = (faction == PlayerType.Rome) ? _romePanelExpanded : _carthagePanelExpanded;

        // Check deck cards (only visible when panel is expanded)
        if (panelExpanded && deckCards.Count > 0)
        {
            if (CheckCardGroupClick(deckCards, mouseX, mouseY, yPosition))
            {
                return true;
            }
        }

        // Check hand cards (always visible, positioned based on panel state)
        if (handCards.Count > 0)
        {
            float handY = yPosition;
            if (panelExpanded)
            {
                // Hand cards move out of panel by CARD_PANEL_OFFSET
                handY = (faction == PlayerType.Rome)
                    ? yPosition - CARD_PANEL_OFFSET
                    : yPosition + CARD_PANEL_OFFSET;
            }

            if (CheckCardGroupClick(handCards, mouseX, mouseY, handY))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckCardGroupClick(List<Card> cards, int mouseX, int mouseY, float yPosition)
    {
        if (cards.Count == 0) return false;

        // Group cards by type for display
        var groupedCards = cards.GroupBy(c => c.Type)
                                .Select(g => new { CardType = g.Key, Cards = g.ToList() })
                                .ToList();

        // Calculate card positions
        float totalWidth = (groupedCards.Count * CARD_WIDTH) + ((groupedCards.Count - 1) * CARD_SPACING);
        float startX = (_graphics.PreferredBackBufferWidth - totalWidth) / 2;

        for (int i = 0; i < groupedCards.Count; i++)
        {
            float xPosition = startX + (i * (CARD_WIDTH + CARD_SPACING));
            Rectangle cardRect = new Rectangle((int)xPosition, (int)yPosition, (int)CARD_WIDTH, (int)CARD_HEIGHT);

            if (cardRect.Contains(mouseX, mouseY))
            {
                // Click the first card in this group
                groupedCards[i].Cards[0].OnClick();
                return true;
            }
        }

        return false;
    }

    private Vector2 ScreenToWorld(Vector2 screenPosition, float zoom)
    {
        return (screenPosition / zoom) + _cameraPosition;
    }

    protected override void Draw(GameTime gameTime)
    {
        // Update elapsed time for animations
        _totalElapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;

        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Create transformation matrix for camera
        Matrix transformMatrix = Matrix.CreateTranslation(new Vector3(-_cameraPosition, 0)) *
                                 Matrix.CreateScale(_zoomLevel) *
                                 Matrix.CreateTranslation(new Vector3(0, 0, 0));

        _spriteBatch.Begin(transformMatrix: transformMatrix);

        // Draw background (fixed size, doesn't zoom)
        if (_backgroundTexture != null)
        {
            // Calculate background size to cover viewport with some padding
            float bgWidth = _graphics.PreferredBackBufferWidth / _zoomLevel + 500;
            float bgHeight = _graphics.PreferredBackBufferHeight / _zoomLevel + 500;
            var bgRect = new Rectangle(
                (int)(_cameraPosition.X - 250),
                (int)(_cameraPosition.Y - 250),
                (int)bgWidth,
                (int)bgHeight);
            _spriteBatch.Draw(_backgroundTexture, bgRect, Color.White);
        }

        // Draw hex grid
        float viewWidth = _graphics.PreferredBackBufferWidth / _zoomLevel;
        float viewHeight = _graphics.PreferredBackBufferHeight / _zoomLevel;
        _grid.DrawHexGrid(_spriteBatch, _pixelTexture, Color.Black,
            _cameraPosition.X, _cameraPosition.X + viewWidth,
            _cameraPosition.Y, _cameraPosition.Y + viewHeight);

        // Draw tiles
        for (int row = 0; row < BOARD_ROWS; row++)
        {
            for (int col = 0; col < BOARD_COLS; col++)
            {
                // Skip last column in odd rows
                if (row % 2 != 0 && col == BOARD_COLS - 1) continue;

                var position = new Position(row, col);
                var tile = _board[position];

                if (tile != null)
                {
                    var hexCenter = _grid.HexToPoint(row, col);
                    var points = _grid.HexToPoints(row, col);

                    // Calculate center of hexagon
                    float centerX = (points[0].X + points[3].X) / 2;
                    float centerY = (points[1].Y + points[4].Y) / 2;

                    // Draw tile texture if it's water or hill
                    if (tile is Water && _tileTextures.ContainsKey("water"))
                    {
                        var texture = _tileTextures["water"];
                        var destRect = new Rectangle((int)(centerX - 35), (int)(centerY - 35), 70, 70);
                        _spriteBatch.Draw(texture, destRect, Color.White);
                    }
                    else if (tile is Hill && _tileTextures.ContainsKey("hill"))
                    {
                        var texture = _tileTextures["hill"];
                        var destRect = new Rectangle((int)(centerX - 35), (int)(centerY - 35), 70, 70);
                        _spriteBatch.Draw(texture, destRect, Color.White);
                    }

                    // Draw unit if present with golden ratio dimensions
                    if (tile.Unit != null)
                    {
                        string textureKey = GetUnitTextureKey(tile.Unit);
                        if (_unitTextures.ContainsKey(textureKey))
                        {
                            var texture = _unitTextures[textureKey];

                            // Apply golden ratio: width = height * golden_ratio
                            float unitHeight = UNIT_BASE_SIZE;
                            float unitWidth = unitHeight * GOLDEN_RATIO;

                            // Stack counters based on health value
                            int health = tile.Unit.Health.GetHealth();
                            const float stackOffset = 3f; // Offset for each stacked counter

                            // Draw multiple counters stacked (health times)
                            for (int i = 0; i < health; i++)
                            {
                                // Offset each counter slightly to create stack effect
                                float offsetX = i * stackOffset;
                                float offsetY = -i * stackOffset; // Stack upward and to the right

                                var destRect = new Rectangle(
                                    (int)(centerX - unitWidth / 2 + offsetX),
                                    (int)(centerY - unitHeight / 2 + offsetY),
                                    (int)unitWidth,
                                    (int)unitHeight);
                                _spriteBatch.Draw(texture, destRect, Color.White);
                            }
                        }
                    }
                }
            }
        }

        // Draw PathFinder visualizations
        var pathFinder = PathFinder.Instance;

        // Draw CurrentSpaces (valid movement tiles) in green
        foreach (var tile in pathFinder.CurrentSpaces.Keys)
        {
            if (tile?.Position != null)
            {
                var points = _grid.HexToPoints(tile.Position.Row, tile.Position.Column);
                DrawPolygon(_spriteBatch, _pixelTexture, points, Color.LightGreen, 2f);
            }
        }

        // Draw CurrentTargets (valid attack targets) in red
        foreach (var tile in pathFinder.CurrentTargets.Keys)
        {
            if (tile?.Position != null)
            {
                var points = _grid.HexToPoints(tile.Position.Row, tile.Position.Column);
                DrawPolygon(_spriteBatch, _pixelTexture, points, Color.Red, 2f);
            }
        }

        // Draw CurrentPath (movement path preview) as a line through hex centers
        if (pathFinder.CurrentPath?.TilesInPath != null && pathFinder.CurrentPath.TilesInPath.Count > 1)
        {
            for (int i = 0; i < pathFinder.CurrentPath.TilesInPath.Count - 1; i++)
            {
                var currentTile = pathFinder.CurrentPath.TilesInPath[i];
                var nextTile = pathFinder.CurrentPath.TilesInPath[i + 1];

                if (currentTile?.Position != null && nextTile?.Position != null)
                {
                    // Get the center of each hex using NodeToPoint
                    var currentCenter = _grid.NodeToPoint(currentTile.Position.Row, currentTile.Position.Column);
                    var nextCenter = _grid.NodeToPoint(nextTile.Position.Row, nextTile.Position.Column);

                    // Draw line between centers
                    DrawLine(_spriteBatch, _pixelTexture, new Vector2(currentCenter.X, currentCenter.Y),
                             new Vector2(nextCenter.X, nextCenter.Y), Color.Blue, 4f);
                }
            }
        }

        // Draw CurrentAttackPath (attack path preview) as a line through hex centers
        if (pathFinder.CurrentAttackPath?.TilesInPath != null && pathFinder.CurrentAttackPath.TilesInPath.Count > 1)
        {
            for (int i = 0; i < pathFinder.CurrentAttackPath.TilesInPath.Count - 1; i++)
            {
                var currentTile = pathFinder.CurrentAttackPath.TilesInPath[i];
                var nextTile = pathFinder.CurrentAttackPath.TilesInPath[i + 1];

                if (currentTile?.Position != null && nextTile?.Position != null)
                {
                    // Get the center of each hex using NodeToPoint
                    var currentCenter = _grid.NodeToPoint(currentTile.Position.Row, currentTile.Position.Column);
                    var nextCenter = _grid.NodeToPoint(nextTile.Position.Row, nextTile.Position.Column);

                    // Draw line between centers
                    DrawLine(_spriteBatch, _pixelTexture, new Vector2(currentCenter.X, currentCenter.Y),
                             new Vector2(nextCenter.X, nextCenter.Y), Color.Orange, 4f);
                }
            }
        }

        // Highlight selected hex
        if (_selectedRow >= 0 && _selectedCol >= 0)
        {
            var points = _grid.HexToPoints(_selectedRow, _selectedCol);
            DrawPolygon(_spriteBatch, _pixelTexture, points, Color.Yellow, 3f);
        }

        _spriteBatch.End();

        // Draw UI elements in screen space (not affected by camera transform)
        _spriteBatch.Begin();
        DrawGamePhaseTracker();
        DrawPhaseTracker();
        DrawCurrentPlayerTracker();
        DrawRollButton(_totalElapsedSeconds);
        DrawCards();

        // Draw message box on top of everything if there's a message
        if (_currentMessage != null)
        {
            DrawMessageBox(_currentMessage);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private string GetUnitTextureKey(Unit unit)
    {
        string unitType = unit.GetType().Name;
        string playerColor = unit.Faction == PlayerType.Rome ? "Red" : "Blue";

        if (unit is Leader)
            return $"King-{playerColor}";
        else if (unit is Cavalry)
            return $"Cavalry-{playerColor}";
        else if (unit is Infantry)
            return $"Infantry-{playerColor}";
        else if (unit is Pikes)
            return $"Pikes-{playerColor}";
        else if (unit is Archer)
            return $"Archer-{playerColor}";
        else if (unit is Spear)
            return $"Spear-{playerColor}";
        else if (unit is Numidians)
            return $"Horse-{playerColor}";
        else if (unit is Equites)
            return $"Cavalry-{playerColor}";
        else if (unit is Velites)
            return $"Spear-{playerColor}";
        else if (unit is Phoenicians)
            return $"Cavalry-{playerColor}";


        return $"Infantry-{playerColor}"; // Default
    }

    private void DrawPolygon(SpriteBatch spriteBatch, Texture2D pixel, Vector2[] points, Color color, float thickness = 1f)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 point1 = points[i];
            Vector2 point2 = points[(i + 1) % points.Length];
            DrawLine(spriteBatch, pixel, point1, point2, color, thickness);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float thickness = 1f)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        spriteBatch.Draw(pixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);
    }

    private void DrawCircle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float radius, Color color)
    {
        // Draw a filled circle by drawing multiple rectangles in a circle pattern
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)(2 * Math.PI * i / segments);
            float angle2 = (float)(2 * Math.PI * (i + 1) / segments);

            Vector2 p1 = center + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
            Vector2 p2 = center + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

            DrawLine(spriteBatch, pixel, p1, p2, color, radius * 0.8f);
        }
    }

    private void DrawCards()
    {
        if (_board?.Cards == null) return;

        // Draw Rome panel and cards (bottom)
        DrawCardPanel(PlayerType.Rome, _romePanelExpanded, _romeCardsCache);

        // Draw Carthage panel and cards (top)
        DrawCardPanel(PlayerType.Carthage, _carthagePanelExpanded, _carthageCardsCache);
    }

    private void DrawCardPanel(PlayerType faction, bool isExpanded, List<Card> cards)
    {
        bool isRome = (faction == PlayerType.Rome);
        float panelHeight = isExpanded ? PANEL_EXPANDED_HEIGHT : PANEL_COLLAPSED_HEIGHT;

        // Calculate panel position
        float panelY = isRome
            ? _graphics.PreferredBackBufferHeight - panelHeight
            : 0;

        // Draw panel background
        Rectangle panelRect = new Rectangle(0, (int)panelY, _graphics.PreferredBackBufferWidth, (int)panelHeight);
        Color panelColor = isRome ? new Color(150, 100, 100, 200) : new Color(100, 100, 150, 200);
        _spriteBatch.Draw(_pixelTexture, panelRect, panelColor);
        DrawRectangle(_spriteBatch, _pixelTexture, panelRect, Color.Gray, 2f);

        // Draw arrow button
        DrawArrowButton(faction, isExpanded);

        // Separate cards by state
        var deckCards = cards.Where(c => c.State == CardState.InDeck).ToList();
        var handCards = cards.Where(c => c.State == CardState.InHand).ToList();

        // Draw deck cards (only when expanded)
        if (isExpanded && deckCards.Count > 0)
        {
            float deckY = isRome
                ? _graphics.PreferredBackBufferHeight - PANEL_EXPANDED_HEIGHT + 20
                : PANEL_EXPANDED_HEIGHT - CARD_HEIGHT - 20;
            DrawCardGroup(deckCards, deckY, faction);
        }

        // Draw hand cards (always visible, move out when panel expanded)
        if (handCards.Count > 0)
        {
            float handY;
            if (isExpanded)
            {
                // Hand cards move out of panel
                handY = isRome
                    ? panelY - CARD_PANEL_OFFSET
                    : panelY + panelHeight + CARD_PANEL_OFFSET;
            }
            else
            {
                // Hand cards positioned near collapsed panel
                handY = isRome
                    ? _graphics.PreferredBackBufferHeight - PANEL_COLLAPSED_HEIGHT - CARD_HEIGHT - 10
                    : PANEL_COLLAPSED_HEIGHT + 10;
            }
            DrawCardGroup(handCards, handY, faction);
        }
    }

    private void DrawArrowButton(PlayerType faction, bool isExpanded)
    {
        bool isRome = (faction == PlayerType.Rome);

        // Calculate arrow button position
        float arrowX = (_graphics.PreferredBackBufferWidth - ARROW_BUTTON_SIZE) / 2;
        float arrowY = isRome
            ? _graphics.PreferredBackBufferHeight - PANEL_COLLAPSED_HEIGHT / 2 - ARROW_BUTTON_SIZE / 2
            : PANEL_COLLAPSED_HEIGHT / 2 - ARROW_BUTTON_SIZE / 2;

        Rectangle arrowRect = new Rectangle((int)arrowX, (int)arrowY, (int)ARROW_BUTTON_SIZE, (int)ARROW_BUTTON_SIZE);

        // Draw button background
        _spriteBatch.Draw(_pixelTexture, arrowRect, Color.DarkGray);
        DrawRectangle(_spriteBatch, _pixelTexture, arrowRect, Color.White, 2f);

        // Draw arrow symbol (triangle pointing up/down)
        DrawArrowSymbol(arrowRect, isExpanded, isRome);
    }

    private void DrawArrowSymbol(Rectangle buttonRect, bool isExpanded, bool isRome)
    {
        // Determine arrow direction: Rome expanded = up arrow, collapsed = down arrow
        // Carthage expanded = down arrow, collapsed = up arrow
        bool pointUp = isRome ? isExpanded : !isExpanded;

        float centerX = buttonRect.X + buttonRect.Width / 2;
        float centerY = buttonRect.Y + buttonRect.Height / 2;
        float arrowSize = 8f;

        Vector2 p1, p2, p3;
        if (pointUp)
        {
            p1 = new Vector2(centerX, centerY - arrowSize);
            p2 = new Vector2(centerX - arrowSize, centerY + arrowSize);
            p3 = new Vector2(centerX + arrowSize, centerY + arrowSize);
        }
        else
        {
            p1 = new Vector2(centerX, centerY + arrowSize);
            p2 = new Vector2(centerX - arrowSize, centerY - arrowSize);
            p3 = new Vector2(centerX + arrowSize, centerY - arrowSize);
        }

        // Draw triangle
        DrawLine(_spriteBatch, _pixelTexture, p1, p2, Color.White, 2f);
        DrawLine(_spriteBatch, _pixelTexture, p2, p3, Color.White, 2f);
        DrawLine(_spriteBatch, _pixelTexture, p3, p1, Color.White, 2f);
    }

    private void DrawCardGroup(List<Card> cards, float yPosition, PlayerType faction)
    {
        if (cards.Count == 0) return;

        // Group cards by type to stack identical cards
        var groupedCards = cards.GroupBy(c => c.Type)
                                .Select(g => new { CardType = g.Key, Cards = g.ToList(), Count = g.Count() })
                                .ToList();

        // Calculate total width and starting X position to center the cards
        float totalWidth = (groupedCards.Count * CARD_WIDTH) + ((groupedCards.Count - 1) * CARD_SPACING);
        float startX = (_graphics.PreferredBackBufferWidth - totalWidth) / 2;

        const float stackOffset = 3f;

        for (int i = 0; i < groupedCards.Count; i++)
        {
            var group = groupedCards[i];
            float xPosition = startX + (i * (CARD_WIDTH + CARD_SPACING));

            // Determine card appearance
            Color cardColor = Color.White;
            bool isHighlighted = false;
            bool isTimingMatch = false;
            Color borderColor = Color.Gold;
            float borderThickness = 3f;

            // Check if card is in ReadyToPlay state (green border)
            var firstCard = group.Cards[0];
            if (firstCard.State == CardState.ReadyToPlay && TurnManager.Instance.CurrentGamePhase != GamePhase.Select)
            {
                isHighlighted = true;
                isTimingMatch = true;
                borderColor = Color.LimeGreen;
                borderThickness = 5f;
            }

            if (TurnManager.Instance.SelectedCard != null && group.Cards.Contains(TurnManager.Instance.SelectedCard))
            {
                cardColor = Color.Yellow;
                isHighlighted = true;
                if (!isTimingMatch)
                {
                    borderColor = Color.Gold;
                    borderThickness = 3f;
                }
            }

            // Draw stacked cards
            for (int stackIndex = 0; stackIndex < group.Count; stackIndex++)
            {
                float offsetX = stackIndex * stackOffset;
                float offsetY = -stackIndex * stackOffset;

                Rectangle cardRect = new Rectangle(
                    (int)(xPosition + offsetX),
                    (int)(yPosition + offsetY),
                    (int)CARD_WIDTH,
                    (int)CARD_HEIGHT);

                // Draw card texture or placeholder
                string cardKey = group.CardType.ToString();
                if (_cardTextures.ContainsKey(cardKey))
                {
                    _spriteBatch.Draw(_cardTextures[cardKey], cardRect, cardColor);
                }
                else if (_cardBackTexture != null)
                {
                    _spriteBatch.Draw(_cardBackTexture, cardRect, cardColor);
                }
                else
                {
                    _spriteBatch.Draw(_pixelTexture, cardRect, GetCardColor(faction));
                }

                // Draw border if highlighted (only on top card)
                if (isHighlighted && stackIndex == group.Count - 1)
                {
                    DrawRectangle(_spriteBatch, _pixelTexture, cardRect, borderColor, borderThickness);
                }
            }

            // Draw card name and count on top card
            float topOffsetX = (group.Count - 1) * stackOffset;
            float topOffsetY = -(group.Count - 1) * stackOffset;

            // Draw semi-transparent background for text
            Rectangle textBgRect = new Rectangle(
                (int)(xPosition + topOffsetX),
                (int)(yPosition + topOffsetY + CARD_HEIGHT - 25),
                (int)CARD_WIDTH,
                25);
            _spriteBatch.Draw(_pixelTexture, textBgRect, new Color(0, 0, 0, 180));

            // Draw card name
            string cardText = group.CardType.ToString();
            cardText = System.Text.RegularExpressions.Regex.Replace(cardText, "([a-z])([A-Z])", "$1 $2");

            Vector2 textPosition = new Vector2(xPosition + topOffsetX + 5, yPosition + topOffsetY + CARD_HEIGHT - 20);

            if (_font != null)
            {
                DrawCardText(cardText, textPosition);
            }
            else
            {
                DrawSimpleText(_spriteBatch, cardText, textPosition, Color.White, 1.0f, (int)CARD_WIDTH - 10);
            }

            // Draw card state for debugging
            string stateText = firstCard.State.ToString();
            Vector2 statePosition = new Vector2(xPosition + topOffsetX + 5, yPosition + topOffsetY + 5);
            Color stateColor = firstCard.State == CardState.ReadyToPlay ? Color.LimeGreen : Color.Yellow;
            if (_font != null)
            {
                _spriteBatch.DrawString(_font, stateText, statePosition, stateColor, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
            }

            // Draw count if more than 1
            if (group.Count > 1)
            {
                DrawCardCount(group.Count, xPosition + topOffsetX, yPosition + topOffsetY);
            }
        }
    }

    private void DrawCardText(string cardText, Vector2 textPosition)
    {
        string displayText = cardText;
        float maxWidth = CARD_WIDTH - 10;
        float scale = 0.35f;

        Vector2 textSize = _font.MeasureString(displayText) * scale;

        if (textSize.X > maxWidth)
        {
            scale = maxWidth / _font.MeasureString(displayText).X;
            if (scale < 0.25f)
            {
                scale = 0.25f;
            }
        }

        _spriteBatch.DrawString(_font, displayText, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawCardCount(int count, float x, float y)
    {
        string countText = $"x{count}";

        Vector2 countCenter = new Vector2(x + CARD_WIDTH - 18, y + 15);
        DrawCircle(_spriteBatch, _pixelTexture, countCenter, 12f, new Color(0, 0, 0, 200));

        Vector2 countPosition = new Vector2(countCenter.X - countText.Length * 3, countCenter.Y - 5);

        if (_font != null)
        {
            _spriteBatch.DrawString(_font, countText, countPosition, Color.White, 0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0f);
        }
        else
        {
            DrawSimpleText(_spriteBatch, countText, countPosition, Color.White, 1.0f, 30);
        }
    }

    private void DrawMessageBox(string message)
    {
        // Calculate message box position (center of screen)
        float boxX = (_graphics.PreferredBackBufferWidth - MESSAGE_BOX_WIDTH) / 2;
        float boxY = (_graphics.PreferredBackBufferHeight - MESSAGE_BOX_HEIGHT) / 2;

        // Draw semi-transparent overlay
        Rectangle overlayRect = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _spriteBatch.Draw(_pixelTexture, overlayRect, new Color(0, 0, 0, 150));

        // Draw message box background
        Rectangle boxRect = new Rectangle((int)boxX, (int)boxY, (int)MESSAGE_BOX_WIDTH, (int)MESSAGE_BOX_HEIGHT);
        _spriteBatch.Draw(_pixelTexture, boxRect, new Color(40, 40, 60, 255));
        DrawRectangle(_spriteBatch, _pixelTexture, boxRect, Color.Gold, 4f);

        // Draw title bar
        Rectangle titleRect = new Rectangle((int)boxX, (int)boxY, (int)MESSAGE_BOX_WIDTH, 35);
        _spriteBatch.Draw(_pixelTexture, titleRect, new Color(60, 60, 100, 255));
        DrawRectangle(_spriteBatch, _pixelTexture, titleRect, Color.Gold, 2f);

        // Draw title text
        if (_font != null)
        {
            string title = "Battle of Legends";
            float titleScale = 0.4f;
            Vector2 titleSize = _font.MeasureString(title) * titleScale;
            Vector2 titlePosition = new Vector2(boxX + (MESSAGE_BOX_WIDTH - titleSize.X) / 2, boxY + (35 - titleSize.Y) / 2);
            _spriteBatch.DrawString(_font, title, titlePosition, Color.Gold, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
        }

        // Draw message text with word wrapping
        if (_font != null)
        {
            float messageScale = 0.5f;
            float maxWidth = MESSAGE_BOX_WIDTH - (MESSAGE_BOX_PADDING * 2);
            Vector2 messagePosition = new Vector2(boxX + MESSAGE_BOX_PADDING, boxY + 45);

            // Simple word wrapping
            string[] words = message.Split(' ');
            string currentLine = "";
            float lineHeight = _font.MeasureString("A").Y * messageScale + 5;
            float currentY = messagePosition.Y;

            foreach (string word in words)
            {
                string testLine = currentLine.Length > 0 ? currentLine + " " + word : word;
                Vector2 testSize = _font.MeasureString(testLine) * messageScale;

                if (testSize.X > maxWidth && currentLine.Length > 0)
                {
                    // Draw current line
                    _spriteBatch.DrawString(_font, currentLine, new Vector2(messagePosition.X, currentY), Color.White, 0f, Vector2.Zero, messageScale, SpriteEffects.None, 0f);
                    currentLine = word;
                    currentY += lineHeight;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            // Draw last line
            if (currentLine.Length > 0)
            {
                _spriteBatch.DrawString(_font, currentLine, new Vector2(messagePosition.X, currentY), Color.White, 0f, Vector2.Zero, messageScale, SpriteEffects.None, 0f);
            }
        }
        else
        {
            // Fallback without font
            DrawSimpleText(_spriteBatch, message, new Vector2(boxX + MESSAGE_BOX_PADDING, boxY + 45), Color.White, 1.0f, (int)(MESSAGE_BOX_WIDTH - MESSAGE_BOX_PADDING * 2));
        }

        // Draw progress bar showing time remaining
        float progressWidth = MESSAGE_BOX_WIDTH - (MESSAGE_BOX_PADDING * 2);
        float progressHeight = 8f;
        float progressX = boxX + MESSAGE_BOX_PADDING;
        float progressY = boxY + MESSAGE_BOX_HEIGHT - MESSAGE_BOX_PADDING - progressHeight;

        // Background
        Rectangle progressBg = new Rectangle((int)progressX, (int)progressY, (int)progressWidth, (int)progressHeight);
        _spriteBatch.Draw(_pixelTexture, progressBg, new Color(30, 30, 40, 255));
        DrawRectangle(_spriteBatch, _pixelTexture, progressBg, Color.Gray, 1f);

        // Progress bar
        float progress = (float)(_messageDisplayTime / MESSAGE_DISPLAY_DURATION);
        float progressBarWidth = progressWidth * (1 - progress); // Decrease as time passes
        if (progressBarWidth > 0)
        {
            Rectangle progressBar = new Rectangle((int)progressX, (int)progressY, (int)progressBarWidth, (int)progressHeight);
            _spriteBatch.Draw(_pixelTexture, progressBar, Color.Gold);
        }
    }

    private void DrawPhaseTracker()
    {
        // Get all turn phases except None
        var phases = System.Enum.GetValues(typeof(TurnPhase)).Cast<TurnPhase>().Where(p => p != TurnPhase.None).ToArray();
        var currentPhase = TurnManager.Instance.CurrentTurnPhase;

        // Calculate vertical tracker position (right side of screen)
        float startY = 200;
        float totalHeight = PHASE_BUTTON_SIZE + PHASE_SPACING + (phases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)) + PHASE_BUTTON_SIZE + 40;
        float trackerX = _graphics.PreferredBackBufferWidth - PHASE_BOX_WIDTH - PHASE_TRACKER_RIGHT_MARGIN;

        // Draw background panel
        Rectangle bgPanel = new Rectangle(
            (int)(trackerX - 5),
            (int)(startY - 30),
            (int)(PHASE_BOX_WIDTH + 10),
            (int)totalHeight);
        _spriteBatch.Draw(_pixelTexture, bgPanel, new Color(40, 80, 40, 220));
        DrawRectangle(_spriteBatch, _pixelTexture, bgPanel, new Color(100, 200, 100), 2f);

        // Draw title above the tracker
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "TURN", new Vector2(trackerX + 45, startY - 25), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);
        }

        // Draw up button
        Rectangle upButton = new Rectangle(
            (int)trackerX,
            (int)startY,
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);
        _spriteBatch.Draw(_pixelTexture, upButton, new Color(60, 100, 60));
        DrawRectangle(_spriteBatch, _pixelTexture, upButton, Color.LightGreen, 2f);

        // Draw "^" symbol
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "^", new Vector2(upButton.X + 60, upButton.Y + 5), Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        }

        // Draw each phase box vertically
        float phaseStartY = startY + PHASE_BUTTON_SIZE + PHASE_SPACING;
        for (int i = 0; i < phases.Length; i++)
        {
            var phase = phases[i];
            bool isCurrent = phase == currentPhase;

            Rectangle phaseBox = new Rectangle(
                (int)trackerX,
                (int)(phaseStartY + i * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
                (int)PHASE_BOX_WIDTH,
                (int)PHASE_BOX_HEIGHT);

            // Draw box background
            Color boxColor = isCurrent ? new Color(100, 200, 100) : new Color(80, 100, 80);
            _spriteBatch.Draw(_pixelTexture, phaseBox, boxColor);

            // Draw box border
            Color borderColor = isCurrent ? Color.Yellow : Color.DarkGray;
            float borderThickness = isCurrent ? 3f : 2f;
            DrawRectangle(_spriteBatch, _pixelTexture, phaseBox, borderColor, borderThickness);

            // Draw phase name
            string phaseName = phase.ToString();

            if (_font != null)
            {
                float scale = 0.45f;
                Vector2 textSize = _font.MeasureString(phaseName) * scale;
                string displayText = phaseName;

                // Truncate if too long
                while (displayText.Length > 0 && _font.MeasureString(displayText).X * scale > PHASE_BOX_WIDTH - 10)
                {
                    displayText = displayText.Substring(0, displayText.Length - 1);
                }

                // Center text in box
                textSize = _font.MeasureString(displayText) * scale;
                Vector2 textPosition = new Vector2(
                    phaseBox.X + (PHASE_BOX_WIDTH - textSize.X) / 2,
                    phaseBox.Y + (PHASE_BOX_HEIGHT - textSize.Y) / 2);

                _spriteBatch.DrawString(_font, displayText, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            else
            {
                DrawSimpleText(_spriteBatch, phaseName, new Vector2(phaseBox.X + 10, phaseBox.Y + 15), Color.White, 0.9f, (int)PHASE_BOX_WIDTH - 20);
            }
        }

        // Draw down button
        Rectangle downButton = new Rectangle(
            (int)trackerX,
            (int)(phaseStartY + phases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);
        _spriteBatch.Draw(_pixelTexture, downButton, new Color(60, 100, 60));
        DrawRectangle(_spriteBatch, _pixelTexture, downButton, Color.LightGreen, 2f);

        // Draw "v" symbol
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "v", new Vector2(downButton.X + 60, downButton.Y + 5), Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        }
    }

    private void DrawGamePhaseTracker()
    {
        // Get all game phases
        var phases = System.Enum.GetValues(typeof(GamePhase)).Cast<GamePhase>().ToArray();
        var currentPhase = TurnManager.Instance.CurrentGamePhase;

        // Calculate vertical tracker position (left side of screen)
        float startY = 200;
        float totalHeight = PHASE_BUTTON_SIZE + PHASE_SPACING + (phases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)) + PHASE_BUTTON_SIZE + 35;

        // Draw background panel
        Rectangle bgPanel = new Rectangle(
            (int)(PHASE_TRACKER_LEFT_X - 5),
            (int)(startY - 25),
            (int)(PHASE_BOX_WIDTH + 10),
            (int)totalHeight);
        _spriteBatch.Draw(_pixelTexture, bgPanel, new Color(50, 50, 100, 220));
        DrawRectangle(_spriteBatch, _pixelTexture, bgPanel, new Color(150, 150, 255), 2f);

        // Draw title above the tracker
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "GAME", new Vector2(PHASE_TRACKER_LEFT_X + 45, startY - 20), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);
        }

        // Draw up button
        Rectangle upButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)startY,
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);
        _spriteBatch.Draw(_pixelTexture, upButton, new Color(70, 70, 120));
        DrawRectangle(_spriteBatch, _pixelTexture, upButton, Color.LightBlue, 2f);

        // Draw "^" symbol
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "^", new Vector2(upButton.X + 60, upButton.Y + 5), Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        }

        // Draw each phase box vertically
        float phaseStartY = startY + PHASE_BUTTON_SIZE + PHASE_SPACING;
        for (int i = 0; i < phases.Length; i++)
        {
            var phase = phases[i];
            bool isCurrent = phase == currentPhase;

            Rectangle phaseBox = new Rectangle(
                (int)PHASE_TRACKER_LEFT_X,
                (int)(phaseStartY + i * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
                (int)PHASE_BOX_WIDTH,
                (int)PHASE_BOX_HEIGHT);

            // Draw box background
            Color boxColor = isCurrent ? new Color(100, 150, 255) : new Color(90, 90, 120);
            _spriteBatch.Draw(_pixelTexture, phaseBox, boxColor);

            // Draw box border
            Color borderColor = isCurrent ? Color.Cyan : Color.DarkGray;
            float borderThickness = isCurrent ? 3f : 2f;
            DrawRectangle(_spriteBatch, _pixelTexture, phaseBox, borderColor, borderThickness);

            // Draw phase name
            string phaseName = phase.ToString();

            if (_font != null)
            {
                float scale = 0.5f;
                Vector2 textSize = _font.MeasureString(phaseName) * scale;

                // Center text in box
                Vector2 textPosition = new Vector2(
                    phaseBox.X + (PHASE_BOX_WIDTH - textSize.X) / 2,
                    phaseBox.Y + (PHASE_BOX_HEIGHT - textSize.Y) / 2);

                _spriteBatch.DrawString(_font, phaseName, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            else
            {
                DrawSimpleText(_spriteBatch, phaseName, new Vector2(phaseBox.X + 10, phaseBox.Y + 15), Color.White, 1.0f, (int)PHASE_BOX_WIDTH - 20);
            }
        }

        // Draw down button
        Rectangle downButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)(phaseStartY + phases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)),
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BUTTON_SIZE);
        _spriteBatch.Draw(_pixelTexture, downButton, new Color(70, 70, 120));
        DrawRectangle(_spriteBatch, _pixelTexture, downButton, Color.LightBlue, 2f);

        // Draw "v" symbol
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "v", new Vector2(downButton.X + 60, downButton.Y + 5), Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        }
    }

    private void DrawCurrentPlayerTracker()
    {
        var currentPlayer = TurnManager.Instance.CurrentPlayer;

        // Get all turn phases to calculate where TurnPhase tracker ends
        var turnPhases = System.Enum.GetValues(typeof(TurnPhase)).Cast<TurnPhase>().Where(p => p != TurnPhase.None).ToArray();
        var gamePhases = System.Enum.GetValues(typeof(GamePhase)).Cast<GamePhase>().ToArray();

        // Calculate vertical position (left side, below TurnPhase tracker)
        float gamePhaseHeight = 200 + PHASE_BUTTON_SIZE + PHASE_SPACING + (gamePhases.Length * (PHASE_BOX_HEIGHT + PHASE_SPACING)) + PHASE_BUTTON_SIZE + 20;
        float gapUI = 50;
        float startY = gamePhaseHeight + gapUI;
        float totalHeight = 2 * PHASE_BOX_HEIGHT + PHASE_SPACING + 40;

        // Draw background panel
        Rectangle bgPanel = new Rectangle(
            (int)(PHASE_TRACKER_LEFT_X - 5),
            (int)(startY - 30),
            (int)(PHASE_BOX_WIDTH + 10),
            (int)totalHeight);
        _spriteBatch.Draw(_pixelTexture, bgPanel, new Color(80, 60, 40, 220));
        DrawRectangle(_spriteBatch, _pixelTexture, bgPanel, new Color(200, 150, 100), 2f);

        // Draw title above the tracker
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, "PLAYER", new Vector2(PHASE_TRACKER_LEFT_X + 40, startY - 20), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);
        }

        // Draw Rome button
        Rectangle romeButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)startY,
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BOX_HEIGHT);

        bool isRome = currentPlayer == PlayerType.Rome;
        Color romeColor = isRome ? new Color(255, 100, 100) : new Color(120, 80, 80);
        _spriteBatch.Draw(_pixelTexture, romeButton, romeColor);
        DrawRectangle(_spriteBatch, _pixelTexture, romeButton, isRome ? Color.Yellow : Color.DarkRed, isRome ? 3f : 2f);

        // Draw Rome text
        if (_font != null)
        {
            float scale = 0.35f; // Adjusted for larger font size (16pt)
            Vector2 textSize = _font.MeasureString("ROME") * scale;
            Vector2 textPosition = new Vector2(
                romeButton.X + (PHASE_BOX_WIDTH - textSize.X) / 2,
                romeButton.Y + (PHASE_BOX_HEIGHT - textSize.Y) / 2);
            _spriteBatch.DrawString(_font, "ROME", textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        else
        {
            DrawSimpleText(_spriteBatch, "ROME", new Vector2(romeButton.X + 30, romeButton.Y + 15), Color.White, 1.0f, (int)PHASE_BOX_WIDTH - 20);
        }

        // Draw Carthage button
        Rectangle carthageButton = new Rectangle(
            (int)PHASE_TRACKER_LEFT_X,
            (int)(startY + PHASE_BOX_HEIGHT + PHASE_SPACING),
            (int)PHASE_BOX_WIDTH,
            (int)PHASE_BOX_HEIGHT);

        bool isCarthage = currentPlayer == PlayerType.Carthage;
        Color carthageColor = isCarthage ? new Color(100, 150, 255) : new Color(80, 100, 120);
        _spriteBatch.Draw(_pixelTexture, carthageButton, carthageColor);
        DrawRectangle(_spriteBatch, _pixelTexture, carthageButton, isCarthage ? Color.Yellow : Color.DarkBlue, isCarthage ? 3f : 2f);

        // Draw Carthage text
        if (_font != null)
        {
            string carthageText = "CARTHAGE";
            float maxWidth = PHASE_BOX_WIDTH - 10;
            float scale = 0.35f; // Adjusted for larger font size (16pt)

            // Measure and adjust scale to fit
            Vector2 textSize = _font.MeasureString(carthageText) * scale;
            if (textSize.X > maxWidth)
            {
                scale = maxWidth / _font.MeasureString(carthageText).X;
            }

            // Recalculate size with final scale
            textSize = _font.MeasureString(carthageText) * scale;
            Vector2 textPosition = new Vector2(
                carthageButton.X + (PHASE_BOX_WIDTH - textSize.X) / 2,
                carthageButton.Y + (PHASE_BOX_HEIGHT - textSize.Y) / 2);
            _spriteBatch.DrawString(_font, carthageText, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        else
        {
            DrawSimpleText(_spriteBatch, "CARTHAGE", new Vector2(carthageButton.X + 10, carthageButton.Y + 15), Color.White, 0.9f, (int)PHASE_BOX_WIDTH - 20);
        }
    }

    private void DrawRollButton(double totalElapsedSeconds)
    {
        var combatManager = CombatManager.Instance;

        // Only show button if there's a combat declared
        if (combatManager.Attacker == null || combatManager.Target == null)
            return;

        // Calculate button position (center bottom of screen, above Rome cards)
        float buttonX = (_graphics.PreferredBackBufferWidth - ROLL_BUTTON_WIDTH) / 2;
        float buttonY = _graphics.PreferredBackBufferHeight - CARD_HEIGHT - CARD_HAND_Y - ROLL_BUTTON_HEIGHT - 20;

        Rectangle rollButton = new Rectangle(
            (int)buttonX,
            (int)buttonY,
            (int)ROLL_BUTTON_WIDTH,
            (int)ROLL_BUTTON_HEIGHT);

        // Draw button background with pulsing effect using GameTime for deterministic animation
        float pulse = (float)Math.Sin(totalElapsedSeconds * Math.PI) * 0.2f + 0.8f;
        Color buttonColor = new Color((int)(200 * pulse), (int)(50 * pulse), (int)(50 * pulse));
        _spriteBatch.Draw(_pixelTexture, rollButton, buttonColor);

        // Draw button border
        DrawRectangle(_spriteBatch, _pixelTexture, rollButton, Color.Yellow, 3f);

        // Draw "ROLL" text
        if (_font != null)
        {
            string buttonText = "ROLL!";
            float scale = 0.8f;
            Vector2 textSize = _font.MeasureString(buttonText) * scale;
            Vector2 textPosition = new Vector2(
                rollButton.X + (ROLL_BUTTON_WIDTH - textSize.X) / 2,
                rollButton.Y + (ROLL_BUTTON_HEIGHT - textSize.Y) / 2);

            _spriteBatch.DrawString(_font, buttonText, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        else
        {
            DrawSimpleText(_spriteBatch, "ROLL!", new Vector2(rollButton.X + 25, rollButton.Y + 20), Color.White, 1.5f, (int)ROLL_BUTTON_WIDTH - 20);
        }
    }

    private Color GetCardColor(PlayerType faction)
    {
        return faction == PlayerType.Rome ? new Color(255, 100, 100) : new Color(100, 150, 255);
    }

    private void DrawRectangle(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color, float thickness)
    {
        // Top
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, (int)thickness), color);
        // Bottom
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - (int)thickness, rect.Width, (int)thickness), color);
        // Left
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, (int)thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - (int)thickness, rect.Y, (int)thickness, rect.Height), color);
    }

    private void DrawSimpleText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale, int maxWidth)
    {
        // Simple bitmap font rendering using 5x7 pixel characters
        int charWidth = (int)(6 * scale);
        int charHeight = (int)(8 * scale);
        int currentX = (int)position.X;
        int currentY = (int)position.Y;

        // Truncate text if too long
        int maxChars = maxWidth / charWidth;
        if (text.Length > maxChars && maxChars > 3)
        {
            text = text.Substring(0, maxChars - 3) + "...";
        }

        foreach (char c in text.ToUpper())
        {
            if (currentX + charWidth > position.X + maxWidth)
                break;

            DrawCharacter(spriteBatch, c, new Vector2(currentX, currentY), color, scale);
            currentX += charWidth;
        }
    }

    private void DrawCharacter(SpriteBatch spriteBatch, char c, Vector2 position, Color color, float scale)
    {
        int x = (int)position.X;
        int y = (int)position.Y;
        int pixelSize = Math.Max(1, (int)(1.5f * scale)); // Ensure at least 1 pixel size

        // Simple 5x7 bitmap font - drawing basic letters and numbers
        bool[,] pixels = GetCharacterBitmap(c);

        if (pixels != null)
        {
            for (int row = 0; row < pixels.GetLength(0); row++)
            {
                for (int col = 0; col < pixels.GetLength(1); col++)
                {
                    if (pixels[row, col])
                    {
                        Rectangle pixelRect = new Rectangle(
                            x + col * pixelSize,
                            y + row * pixelSize,
                            pixelSize,
                            pixelSize);
                        spriteBatch.Draw(_pixelTexture, pixelRect, color);
                    }
                }
            }
        }
    }

    private bool[,] GetCharacterBitmap(char c)
    {
        // 5x7 bitmap font data
        return c switch
        {
            'A' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {true,true,true,true,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true} },
            'B' => new bool[,] { {true,true,true,true,false}, {true,false,false,false,true}, {true,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,true,true,true,false} },
            'C' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,true}, {false,true,true,true,false} },
            'D' => new bool[,] { {true,true,true,false,false}, {true,false,false,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,true,false}, {true,true,true,false,false} },
            'E' => new bool[,] { {true,true,true,true,true}, {true,false,false,false,false}, {true,false,false,false,false}, {true,true,true,true,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,true,true,true,true} },
            'F' => new bool[,] { {true,true,true,true,true}, {true,false,false,false,false}, {true,false,false,false,false}, {true,true,true,true,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,false} },
            'G' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,false}, {true,false,true,true,true}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            'H' => new bool[,] { {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,true,true,true,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true} },
            'I' => new bool[,] { {true,true,true,true,true}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {true,true,true,true,true} },
            'K' => new bool[,] { {true,false,false,false,true}, {true,false,false,true,false}, {true,false,true,false,false}, {true,true,false,false,false}, {true,false,true,false,false}, {true,false,false,true,false}, {true,false,false,false,true} },
            'L' => new bool[,] { {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,true,true,true,true} },
            'M' => new bool[,] { {true,false,false,false,true}, {true,true,false,true,true}, {true,false,true,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true} },
            'N' => new bool[,] { {true,false,false,false,true}, {true,true,false,false,true}, {true,false,true,false,true}, {true,false,false,true,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true} },
            'O' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            'P' => new bool[,] { {true,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {true,true,true,true,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,false,false,false,false} },
            'R' => new bool[,] { {true,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {true,true,true,true,false}, {true,false,true,false,false}, {true,false,false,true,false}, {true,false,false,false,true} },
            'S' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,false}, {false,true,true,true,false}, {false,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            'T' => new bool[,] { {true,true,true,true,true}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false} },
            'U' => new bool[,] { {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            'V' => new bool[,] { {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,false,true,false}, {false,true,false,true,false}, {false,false,true,false,false} },
            'W' => new bool[,] { {true,false,false,false,true}, {true,false,false,false,true}, {true,false,false,false,true}, {true,false,true,false,true}, {true,false,true,false,true}, {true,true,false,true,true}, {true,false,false,false,true} },
            'X' => new bool[,] { {true,false,false,false,true}, {true,false,false,false,true}, {false,true,false,true,false}, {false,false,true,false,false}, {false,true,false,true,false}, {true,false,false,false,true}, {true,false,false,false,true} },
            'Y' => new bool[,] { {true,false,false,false,true}, {true,false,false,false,true}, {false,true,false,true,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false} },
            '0' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,true,true}, {true,false,true,false,true}, {true,true,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            '1' => new bool[,] { {false,false,true,false,false}, {false,true,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,false,true,false,false}, {false,true,true,true,false} },
            '2' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {false,false,false,false,true}, {false,false,false,true,false}, {false,false,true,false,false}, {false,true,false,false,false}, {true,true,true,true,true} },
            '3' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {false,false,false,false,true}, {false,false,true,true,false}, {false,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            '4' => new bool[,] { {false,false,false,true,false}, {false,false,true,true,false}, {false,true,false,true,false}, {true,false,false,true,false}, {true,true,true,true,true}, {false,false,false,true,false}, {false,false,false,true,false} },
            '5' => new bool[,] { {true,true,true,true,true}, {true,false,false,false,false}, {true,true,true,true,false}, {false,false,false,false,true}, {false,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            '6' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,false}, {true,false,false,false,false}, {true,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            '7' => new bool[,] { {true,true,true,true,true}, {false,false,false,false,true}, {false,false,false,true,false}, {false,false,true,false,false}, {false,true,false,false,false}, {false,true,false,false,false}, {false,true,false,false,false} },
            '8' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,false} },
            '9' => new bool[,] { {false,true,true,true,false}, {true,false,false,false,true}, {true,false,false,false,true}, {false,true,true,true,true}, {false,false,false,false,true}, {false,false,false,false,true}, {false,true,true,true,false} },
            ' ' => new bool[,] { {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false} },
            '.' => new bool[,] { {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,true,false,false}, {false,false,true,false,false} },
            _ => new bool[,] { {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false}, {false,false,false,false,false} }
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from event handlers to prevent memory leaks
            MessageController.Instance.Message -= OnMessage;
            SoundController.Instance.Play -= OnPlaySound;
        }

        base.Dispose(disposing);
    }
}
