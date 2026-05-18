using SoloLeveling.Core;

namespace SoloLeveling
{
    // 프로그램이 처음 실행되는 시작 지점입니다.
    internal class Program
    {
        // GameManager를 생성하고 게임 메인 루프를 시작합니다.
        static void Main(string[] args)
        {
            GameManager gameManager = new GameManager();
            gameManager.StartGame();
        }
    }
}
