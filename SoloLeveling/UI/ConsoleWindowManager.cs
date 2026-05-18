using System.Runtime.InteropServices;

namespace SoloLeveling.Ui
{
    // 콘솔 창 크기와 리사이즈 제한을 설정 클래스
    internal static class ConsoleWindowManager
    {
        public const int GameWidth = 122;
        public const int GameHeight = 42;

        private const int GwlStyle = -16;
        private const int WsMaximizeBox = 0x00010000;
        private const int WsSizeBox = 0x00040000;

        private const int MfByCommand = 0x00000000;
        private const int ScSize = 0xF000;
        private const int ScMaximize = 0xF030;

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool DeleteMenu(IntPtr hMenu, int uPosition, int uFlags);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool DrawMenuBar(IntPtr hWnd);

        // 콘솔 기본 설정
        public static void SetupConsoleWindow()
        {
            try
            {
                Console.Title = "Solo Leveling Console RPG";
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.CursorVisible = false;

                SetFixedConsoleSize(GameWidth, GameHeight);
                DisableResizeControls();
            }
            catch
            {
                
            }
        }

        // 콘솔 크기 재지정
        public static void EnsureConsoleSize()
        {
            try
            {
                Console.CursorVisible = false;

                if (Console.WindowWidth != GameWidth ||
                    Console.WindowHeight != GameHeight ||
                    Console.BufferWidth != GameWidth ||
                    Console.BufferHeight != GameHeight)
                {
                    SetFixedConsoleSize(GameWidth, GameHeight);
                }
            }
            catch
            {
               
            }
        }

        // 콘솔 창과 버퍼 크기 설정
        private static void SetFixedConsoleSize(int width, int height)
        {
            try
            {
                int targetWidth = Math.Min(width, Console.LargestWindowWidth);
                int targetHeight = Math.Min(height, Console.LargestWindowHeight);

                if (targetWidth <= 0 || targetHeight <= 0)
                {
                    return;
                }

                if (Console.WindowWidth > targetWidth || Console.WindowHeight > targetHeight)
                {
                    Console.SetWindowSize(targetWidth, targetHeight);
                }

                if (Console.BufferWidth < targetWidth || Console.BufferHeight < targetHeight)
                {
                    Console.SetBufferSize(
                        Math.Max(Console.BufferWidth, targetWidth),
                        Math.Max(Console.BufferHeight, targetHeight)
                    );
                }

                Console.SetWindowSize(targetWidth, targetHeight);
                Console.SetBufferSize(targetWidth, targetHeight);
            }
            catch
            {
                // Some terminals and sandboxed hosts do not allow fixed console sizes.
            }
        }

        // 콘솔 창 크기 변경 버튼 비활성화
        private static void DisableResizeControls()
        {
            try
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return;
                }

                IntPtr handle = GetConsoleWindow();

                if (handle == IntPtr.Zero)
                {
                    return;
                }

                RemoveResizeStyles(handle);
                RemoveResizeSystemMenuItems(handle);
                DrawMenuBar(handle);
            }
            catch
            {
                
            }
        }

        // 크기 조절과 최대화 옵션을 제거
        private static void RemoveResizeStyles(IntPtr handle)
        {
            try
            {
                int style = GetWindowLong(handle, GwlStyle);
                style &= ~WsSizeBox;
                style &= ~WsMaximizeBox;
                SetWindowLong(handle, GwlStyle, style);
            }
            catch
            {
            }
        }

        // 크기 조절과 최대화 항목을 제거
        private static void RemoveResizeSystemMenuItems(IntPtr handle)
        {
            try
            {
                IntPtr systemMenu = GetSystemMenu(handle, false);

                if (systemMenu == IntPtr.Zero)
                {
                    return;
                }

                DeleteMenu(systemMenu, ScSize, MfByCommand);
                DeleteMenu(systemMenu, ScMaximize, MfByCommand);
            }
            catch
            {
            }
        }
    }
}
