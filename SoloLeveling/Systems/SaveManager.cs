using System.Text.Json;
using SoloLeveling.SaveData;

namespace SoloLeveling.Systems
{
    // 게임 저장과 불러오기를 담당하는 클래스
    internal class SaveManager
    {
        private const string SaveFileName = "save.json";
        private static readonly string SaveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
        private static readonly string SavePath = Path.Combine(SaveDirectory, SaveFileName);
        public string CurrentSavePath => SavePath;
        public string LastLoadErrorMessage { get; private set; } = "";

        // 현재 게임 데이터를 JSON 파일로 저장
        public void SaveGame(GameSaveData saveData, bool showMessage = true)
        {
            if (saveData == null)
            {
                if (showMessage)
                {
                    Console.WriteLine("저장할 데이터가 없습니다.");
                }
                return;
            }

            try
            {
                Directory.CreateDirectory(SaveDirectory);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(saveData, options);
                File.WriteAllText(SavePath, json);

                if (showMessage)
                {
                    Console.WriteLine("게임이 저장되었습니다.");
                    Console.WriteLine($"저장 위치: {SavePath}");
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    Console.WriteLine("게임 저장에 실패했습니다.");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 저장된 JSON 파일을 읽어서 게임 저장 데이터로 복원.
        public GameSaveData LoadGame(bool showMessage = true)
        {
            if (!File.Exists(SavePath))
            {
                LastLoadErrorMessage = "저장 파일이 존재하지 않습니다.";

                if (showMessage)
                {
                    Console.WriteLine(LastLoadErrorMessage);
                }
                return null;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                GameSaveData saveData = JsonSerializer.Deserialize<GameSaveData>(json);

                if (saveData == null || saveData.Player == null)
                {
                    BackupCorruptedSave();
                    LastLoadErrorMessage = "저장 파일 데이터가 올바르지 않습니다.";

                    if (showMessage)
                    {
                        Console.WriteLine(LastLoadErrorMessage);
                    }

                    return null;
                }

                LastLoadErrorMessage = "";

                if (showMessage)
                {
                    Console.WriteLine("게임 데이터를 불러왔습니다.");
                }

                return saveData;
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                BackupCorruptedSave();
                LastLoadErrorMessage = "저장 파일이 손상되었습니다. 새 게임을 시작하거나 저장 파일을 삭제하세요.";

                if (showMessage)
                {
                    Console.WriteLine("저장 파일이 손상되었습니다.");
                    Console.WriteLine("새 게임을 시작하거나 저장 파일을 삭제하세요.");
                }

                return null;
            }
        }

        // 손상된 저장 파일 문제에서 새 데이터 호출
        private void BackupCorruptedSave()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    return;
                }

                string backupPath = Path.Combine(
                    SaveDirectory,
                    $"save_backup_corrupted_{DateTime.Now:yyyyMMdd_HHmmss}.json");

                File.Move(SavePath, backupPath, true);
            }
            catch
            {
            }
        }
    }
}
