namespace SoloLeveling.SaveData
{
    // 게임 전체 저장 데이터
    internal class GameSaveData
    {
        // 저장할 플레이어 데이터
        public PlayerSaveData Player { get; set; }

        // 현재 저장 데이터 버전
        public int SaveVersion { get; set; } = 1;

        // 마지막으로 저장한 위치
        public string CurrentScene { get; set; } = "MainMenu";
    }
}
