namespace Game
{
    public static class MessageResponse
    {
        // Connection
        public static readonly Message CONNECTION_FAILED = new("connection_failed", "Failed to connect to the server.");
        public static readonly Message CONNECTION_TIMEOUT = new("connection_timeout", "Connection timed out.");
        public static readonly Message CONNECTION_LOST = new("connection_lost", "Connection to the server was lost.");
        public static readonly Message CONNECTION_REJECTED = new("connection_rejected", "Connection was rejected.");
        public static readonly Message CONNECTION_INVALID = new("connection_invalid", "Invalid connection.");
        
        // Server
        public static readonly Message SERVER_CLOSED = new("server_closed", "The server has been closed.");
        public static readonly Message SERVER_SHUTDOWN = new("server_shutdown", "The server has been shut down.");
        public static readonly Message SERVER_FULL = new("server_full", "The server is full.");
        public static readonly Message SERVER_UNAVAILABLE = new("server_unavailable", "The server is currently unavailable.");

        // Room / Game
        public static readonly Message GAME_FULL = new("game_full", "The game is full.");
        public static readonly Message GAME_NOT_FOUND = new("game_not_found", "The game could not be found.");
        public static readonly Message GAME_STARTED = new("game_started", "The game has already started.");
        public static readonly Message GAME_ENDED = new("game_ended", "The game has ended.");
        public static readonly Message GAME_LOCKED = new("game_locked", "The game is locked.");
        public static readonly Message GAME_CLOSED = new("game_closed", "The game has been closed.");

        // Player
        public static readonly Message PLAYER_KICKED = new("player_kicked", "You have been kicked from the game.");
        public static readonly Message PLAYER_BANNED = new("player_banned", "You have been banned from the game.");
        public static readonly Message PLAYER_NOT_FOUND = new("player_not_found", "Player could not be found.");
        public static readonly Message PLAYER_ALREADY_CONNECTED = new("player_already_connected", "Player is already connected.");
        public static readonly Message PLAYER_OTHER_HAS_PAUSED = new("player_other_has_paused", "Other player has paused the game.");

        // Authentication
        public static readonly Message UNAUTHORIZED = new("unauthorized", "You are not authorized to join the game.");
        public static readonly Message INVALID_TOKEN = new("invalid_token", "Invalid connection token.");
        public static readonly Message TOKEN_EXPIRED = new("token_expired", "Connection token has expired.");

        // Network
        public static readonly Message NETWORK_ERROR = new("network_error", "A network error occurred.");
        public static readonly Message TRANSPORT_ERROR = new("transport_error", "A transport error occurred.");

        // General
        public static readonly Message DISCONNECT = new("disconnect", "Disconnected from the server.");
        public static readonly Message UNKNOWN = new("unknown", "An unknown error occurred.");
        
        public static readonly Message[] All =
        {
            CONNECTION_FAILED, CONNECTION_TIMEOUT, CONNECTION_LOST, CONNECTION_REJECTED, CONNECTION_INVALID,
            SERVER_CLOSED, SERVER_SHUTDOWN, SERVER_FULL, SERVER_UNAVAILABLE,
            GAME_FULL, GAME_NOT_FOUND, GAME_STARTED, GAME_ENDED, GAME_LOCKED, GAME_CLOSED,
            PLAYER_KICKED, PLAYER_BANNED, PLAYER_NOT_FOUND, PLAYER_ALREADY_CONNECTED, PLAYER_OTHER_HAS_PAUSED,
            UNAUTHORIZED, INVALID_TOKEN, TOKEN_EXPIRED,
            NETWORK_ERROR, TRANSPORT_ERROR,
            DISCONNECT, UNKNOWN
        };
    }
}