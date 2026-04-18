using Microsoft.Xna.Framework.Input;

namespace DragonGlare.Managers
{
    public static class InputManager
    {
        private static KeyboardState _currentKeyState;
        private static KeyboardState _previousKeyState;

        public static void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();
        }

        public static bool IsKeyDown(Keys key) => _currentKeyState.IsKeyDown(key);
        
        public static bool IsKeyPressed(Keys key) => 
            _currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key);
    }
}
