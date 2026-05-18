using SoloLeveling.Common;
using SoloLeveling.Entities;
using SoloLeveling.SaveData;
using SoloLeveling.Systems;
using SoloLeveling.Ui;

namespace SoloLeveling.Core
{
    internal class GameManager
    {
        private BattleUI battleUI = new BattleUI();
        private SaveManager saveManager = new SaveManager();
        private GameScene currentScene = GameScene.MainMenu;
        private GateDifficulty currentGateDifficulty = GateDifficulty.Easy;
        private Player currentPlayer;

        //게임 메인 루프 
        public void StartGame()
        {
            while (true)
            {
                switch (currentScene)
                {
                    case GameScene.MainMenu:
                        ShowMainMenu();
                        break;

                    case GameScene.Town:
                        OpenTown();
                        break;

                    case GameScene.GateMenu:
                        OpenGateMenu();
                        break;

                    case GameScene.UnitManagement:
                        OpenUnitManagement();
                        break;

                    case GameScene.Battle:
                        StartGateBattle();
                        break;
                }
            }
        }
        // 메인 메뉴 출력
        private void ShowMainMenu()
        {
            List<string> menuOptions = new List<string>
            {
                "새 게임",
                "이어하기",
                "종료"
            };

            int selectedIndex = battleUI.SelectFrameMenu("SOLO LEVELING RPG", menuOptions, BuildMainInfo());

            if (selectedIndex == 0)
            {
                StartNewGame();
            }
            else if (selectedIndex == 1)
            {
                ContinueGame();
            }
            else if (selectedIndex == 2)
            {
                ExitGame();
                Environment.Exit(0);
            }
        }
        // 마을 메뉴
        private void OpenTown()
        {
            List<string> menuOptions = new List<string>
            {
                "게이트 입장",
                "그림자 관리",
                "상점",
                "저장하기",
                "메인 메뉴로 돌아가기"
            };

            int selectedIndex = battleUI.SelectFrameMenu("마을", menuOptions, BuildPlayerInfo());

            if (selectedIndex == 0)
            {
                currentScene = GameScene.GateMenu;
            }
            else if (selectedIndex == 1)
            {
                currentScene = GameScene.UnitManagement;
            }
            else if (selectedIndex == 2)
            {
                OpenShopV2();
            }
            else if (selectedIndex == 3)
            {
                SaveGame();
                currentScene = GameScene.Town;
            }
            else if (selectedIndex == 4)
            {
                currentScene = GameScene.MainMenu;
            }
        }
        // 전투 구역 메뉴
        private void OpenGateMenu()
        {
            List<string> menuOptions = new List<string>
            {
                "초급 게이트",
                "중급 게이트",
                "상급 게이트",
                "마을로 돌아가기"
            };

            List<string> infoLines = new List<string>
            {
                "게이트 난이도를 선택하세요.",
                "",
                "초급: 일반 몬스터",
                "중급: 엘리트 몬스터",
                "상급: 보스 몬스터",
                "",
                "전투에는 Shadow만 참가합니다."
            };

            int selectedIndex = battleUI.SelectFrameMenu("게이트 선택", menuOptions, infoLines);

            if (selectedIndex == 0)
            {
                currentGateDifficulty = GateDifficulty.Easy;
                currentScene = GameScene.Battle;
            }
            else if (selectedIndex == 1)
            {
                currentGateDifficulty = GateDifficulty.Normal;
                currentScene = GameScene.Battle;
            }
            else if (selectedIndex == 2)
            {
                currentGateDifficulty = GateDifficulty.Hard;
                currentScene = GameScene.Battle;
            }
            else if (selectedIndex == 3)
            {
                currentScene = GameScene.Town;
            }
        }
        // 메인 메뉴에서 새게임 선택시 진행 매서드
        private void StartNewGame()
        {
            string playerName = battleUI.ReadTextInput("새 게임", "플레이어 이름을 입력하세요.", "성진우");

            currentPlayer = new Player(playerName);
            Shadow starterShadow = new Shadow("기사 그림자", 100, UnitClass.Normal);
            currentPlayer.AddShadow(starterShadow, false);

            battleUI.ShowMessage
                (
                "새 게임",
                new List<string>
                {
                    $"{playerName} 플레이어가 생성되었습니다.",
                    "초기 Shadow가 합류했습니다.",
                    "",
                    "마을로 이동합니다."
                }
                );

            currentScene = GameScene.Town;
        }
        // 메인 메뉴에서 이어하기 선택시 진행 매서드
        private void ContinueGame()
        {
            GameSaveData saveData = saveManager.LoadGame(false);

            if (saveData == null)
            {
                string loadErrorMessage = string.IsNullOrWhiteSpace(saveManager.LastLoadErrorMessage)
                    ? "저장 파일을 불러오지 못했습니다."
                    : saveManager.LastLoadErrorMessage;

                battleUI.ShowMessage
                    (
                    "이어하기",
                    new List<string>
                    {
                        loadErrorMessage,
                        "새 게임을 시작하거나 메인 메뉴로 돌아가세요."
                    }
                    );
                currentScene = GameScene.MainMenu;
                return;
            }

            currentPlayer = new Player(saveData.Player);

            battleUI.ShowMessage
                (
                "이어하기",
                BuildPlayerInfo("게임 데이터를 불러왔습니다.")
                );

            currentScene = GameScene.Town;
        }
        // 상점 메뉴
        private void OpenShopV2()
        {
            if (currentPlayer == null)
            {
                battleUI.ShowMessage
                    (
                    "상점",
                    new List<string>
                    {
                        "먼저 새 게임을 시작해야 합니다."
                    }
                    );
                currentScene = GameScene.MainMenu;
                return;
            }

            while (true)
            {
                List<ItemType> itemTypes = ItemDatabase.GetShopItems();
                List<string> menuOptions = itemTypes
                    .Select(itemType => ItemDatabase.GetItemData(itemType).Name)
                    .ToList();

                menuOptions.Add("뒤로가기");

                int selectedIndex = battleUI.SelectFrameMenu("상점", menuOptions, BuildShopInfo());

                if (selectedIndex >= itemTypes.Count)
                {
                    currentScene = GameScene.Town;
                    return;
                }

                BuyItem(itemTypes[selectedIndex]);
            }
        }
        //아이템 구매
        private void BuyItem(ItemType itemType)
        {
            ItemData itemData = ItemDatabase.GetItemData(itemType);

            if (!currentPlayer.SpendGold(itemData.Price))
            {
                battleUI.ShowMessage
                    (
                    "상점",
                    new List<string>
                    {
                        "골드가 부족합니다.",
                        "",
                        $"{itemData.Name}",
                        $"가격: {itemData.Price}",
                        $"보유 골드: {currentPlayer.Gold}"
                    }
                    );
                return;
            }

            currentPlayer.AddItem(itemType, 1);

            battleUI.ShowMessage
                (
                "상점",
                new List<string>
                {
                    "구매 완료",
                    "",
                    $"{itemData.Name} x1",
                    $"남은 골드: {currentPlayer.Gold}",
                    $"보유 수량: {currentPlayer.GetItemCount(itemType)}"
                }
                );
        }
        // 상점 출력
        private List<string> BuildShopInfo()
        {
            List<string> lines = new List<string>
            {
                $"보유 골드: {currentPlayer.Gold}",
                ""
            };

            foreach (ItemType itemType in ItemDatabase.GetAllItemTypes())
            {
                ItemData itemData = ItemDatabase.GetItemData(itemType);
                lines.Add($"{itemData.Name}");
                lines.Add($"가격: {itemData.Price} / 회복량: {itemData.Amount}");
                lines.Add($"보유 수량: {currentPlayer.GetItemCount(itemType)}");
                lines.Add(itemData.Description);
                lines.Add("");
            }

            return lines;
        }
        //저장
        private void SaveGame()
        {
            if (currentPlayer == null)
            {
                battleUI.ShowMessage
                    (
                    "저장하기",
                    new List<string>
                    {
                        "저장할 플레이어 데이터가 없습니다.",
                        "먼저 새 게임을 시작해야 합니다."
                    }
                    );
                return;
            }

            GameSaveData saveData = new GameSaveData
            {
                Player = currentPlayer.ToSaveData(),
                CurrentScene = "MainMenu"
            };

            saveManager.SaveGame(saveData, false);

            battleUI.ShowMessage
                (
                "저장하기",
                new List<string>
                {
                    "게임이 저장되었습니다.",
                    "",
                    $"플레이어: {currentPlayer.Name}",
                    $"레벨: {currentPlayer.Level}",
                    $"보유 그림자 수: {currentPlayer.OwnedShadows.Count}",
                    "",
                    $"저장 위치: {saveManager.CurrentSavePath}"
                }
                );
        }
        //게임 종료 메세지 
        private void ExitGame()
        {
            battleUI.ShowMessage
                (
                "종료",
                new List<string>
                {
                    "게임을 종료합니다."
                }
                );
        }
        //아군 유닛 관리 메뉴
        private void OpenUnitManagement()
        {
            if (currentPlayer == null)
            {
                battleUI.ShowMessage
                    (
                    "그림자 관리",
                    new List<string>
                    {
                        "먼저 새 게임을 시작해야 합니다.",
                        "관리할 Shadow가 없습니다."
                    }
                    );
                currentScene = GameScene.MainMenu;
                return;
            }

            while (true)
            {
                List<string> menuOptions = new List<string>
                {
                    "보유 그림자 상세 보기",
                    "모든 그림자 회복",
                    "뒤로 가기"
                };

                int selectedIndex = battleUI.SelectFrameMenu("그림자 관리", menuOptions, BuildShadowSummary());

                if (selectedIndex == 0)
                {
                    ShowShadowDetails();
                }
                else if (selectedIndex == 1)
                {
                    currentPlayer.RecoverAllShadows(false);
                    battleUI.ShowMessage
                        (
                        "그림자 관리",
                        new List<string>
                        {
                            "모든 그림자의 HP와 MP가 회복되었습니다."
                        }
                        );
                }
                else if (selectedIndex == 2)
                {
                    currentScene = GameScene.Town;
                    break;
                }
            }
        }
        //아군 유닛 상세 정보 출력
        private void ShowShadowDetails()
        {
            battleUI.ShowShadowDetailsPaged(currentPlayer.OwnedShadows);
        }
        // 전투 시작
        private void StartGateBattle()
        {
            List<Shadow> playerUnits = battleUI.SelectBattleParty(currentPlayer.OwnedShadows, 3);

            if (playerUnits.Count == 0)
            {
                battleUI.ShowMessage
                    (
                    "전투 편성",
                    new List<string>
                    {
                        "전투에 참가할 Shadow가 없어 전투를 시작할 수 없습니다."
                    }
                    );
                currentScene = GameScene.Town;
                return;
            }

            List<Monster> enemyUnits = MonsterDatabase.CreateMonsters(currentGateDifficulty, currentPlayer.Level);

            TurnManager turnManager = new TurnManager(playerUnits, enemyUnits, currentPlayer);
            BattleResult battleResult = turnManager.StartBattleLoop();

            if (battleResult == BattleResult.EnemyWin)
            {
                HandleBattleDefeat();
            }
            else if (battleResult == BattleResult.PlayerWin)
            {
                currentPlayer.AddGold(GetGoldReward(currentGateDifficulty));
            }

            battleUI.ShowMessage("전투 결과", BuildBattleResultLines(battleResult));

            currentScene = GameScene.Town;
        }
        //패배 처리
        private void HandleBattleDefeat()
        {
            currentPlayer.RecoverAllShadows(false);
        }
        //난이도 별 골드 계산
        private int GetGoldReward(GateDifficulty difficulty)
        {
            if (difficulty == GateDifficulty.Easy)
            {
                return 100;
            }

            if (difficulty == GateDifficulty.Normal)
            {
                return 250;
            }

            return 500;
        }
        // 메인 메뉴 정보 출력
        private List<string> BuildMainInfo()
        {
            if (currentPlayer == null)
            {
                return new List<string>
                {
                    "새 게임을 시작하거나 저장된 데이터를 불러오세요.",
                    "",
                    "Player는 전투에 직접 참가하지 않습니다.",
                    "전투에는 Shadow만 아군으로 표시됩니다."
                };
            }

            return BuildPlayerInfo();
        }
        // 플레이어 정보 출력
        private List<string> BuildPlayerInfo(string header = "플레이어 정보")
        {
            if (currentPlayer == null)
            {
                return new List<string>
                {
                    header,
                    "",
                    "플레이어 데이터가 없습니다."
                };
            }

            return new List<string>
            {
                header,
                "",
                $"플레이어: {currentPlayer.Name}",
                $"레벨: {currentPlayer.Level}",
                $"랭크: {currentPlayer.Rank}",
                $"골드: {currentPlayer.Gold}",
                $"보유 그림자 수: {currentPlayer.OwnedShadows.Count}"
            };
        }
        //아군 유닛 목록 출력
        private List<string> BuildShadowSummary()
        {
            List<string> lines = BuildPlayerInfo("보유 Shadow");
            lines.Add("");

            if (currentPlayer.OwnedShadows.Count == 0)
            {
                lines.Add("보유한 그림자가 없습니다.");
                return lines;
            }

            foreach (Shadow shadow in currentPlayer.OwnedShadows)
            {
                string stateText = shadow.IsAlive ? "생존" : "전투불능";
                lines.Add($"{shadow.Name} / Lv.{shadow.Level} / {shadow.Grade} / {stateText}");
            }

            return lines;
        }
        // 전투 결과 출력
        private List<string> BuildBattleResultLines(BattleResult battleResult)
        {
            List<string> lines = new List<string>
            {
                battleResult == BattleResult.PlayerWin ? "전투 결과: 승리" : "전투 결과: 패배"
            };

            if (battleResult == BattleResult.EnemyWin)
            {
                lines.Add("모든 그림자가 회복되고 마을로 복귀합니다.");
            }

            lines.Add("");
            lines.Add($"플레이어: {currentPlayer.Name}");
            lines.Add($"레벨: {currentPlayer.Level}");
            lines.Add($"보유 골드: {currentPlayer.Gold}");
            lines.Add($"보유 그림자 수: {currentPlayer.OwnedShadows.Count}");

            return lines;
        }
    }
}
