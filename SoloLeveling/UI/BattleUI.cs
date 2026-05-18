using System;
using System.Collections.Generic;
using System.Linq;
using SoloLeveling.Common;
using SoloLeveling.Entities;
using SoloLeveling.Skills;
using SoloLeveling.Systems;

namespace SoloLeveling.Ui 
{
    // 콘솔 화면 출력과 전투/메뉴 선택 UI를 담당하는 클래스
    class BattleUI
    {
        private const int BattleScreenWidth = 140;
        private const int BattleScreenHeight = 40;

        private const int FrameX = 0;
        private const int FrameY = 0;
        private const int FrameWidth = 140;
        private const int FrameHeight = 40;
        private const int LeftPanelX = 4;
        private const int LeftPanelY = 6;
        private const int LeftPanelWidth = 40;
        private const int LeftPanelHeight = 26;
        private const int RightPanelX = 52;
        private const int RightPanelY = 6;
        private const int RightPanelWidth = 84;
        private const int RightPanelHeight = 26;
        private const int FooterY = 36;

        private const int BattleFieldX = 2;
        private const int BattleFieldY = 1;
        private const int BattleFieldWidth = 136;
        private const int BattleFieldHeight = 22;

        private const int BottomY = 27;
        private const int BottomHeight = 10;

        private const int LogBoxX = 2;
        private const int LogBoxY = BottomY;
        private const int LogBoxWidth = 42;
        private const int LogBoxHeight = BottomHeight;

        private const int CommandBoxX = 46;
        private const int CommandBoxY = BottomY;
        private const int CommandBoxWidth = 40;
        private const int CommandBoxHeight = BottomHeight;

        private const int InfoBoxX = 88;
        private const int InfoBoxY = BottomY;
        private const int InfoBoxWidth = 50;
        private const int InfoBoxHeight = BottomHeight;

        private const int MaxLogCount = 6;
        // 전투로그 저장
        private List<string> battleLogs = new List<string>();

        // 콘솔 좌표를 기준으로 사각형 박스 그리기
        private void DrawBox(int x, int y, int width, int height, string title = "")
        {
            if (width < 2 || height < 2)
            {
                return;
            }

            string topLine = "┌" + new string('─', width - 2) + "┐";
            string middleLine = "│" + new string(' ', width - 2) + "│";
            string bottomLine = "└" + new string('─', width - 2) + "┘";

            WriteAt(x, y, topLine, width);

            for (int i = 1; i < height - 1; i++)
            {
                WriteAt(x, y + i, middleLine, width);
            }

            WriteAt(x, y + height - 1, bottomLine, width);

            if (!string.IsNullOrWhiteSpace(title))
            {
                WriteAt(x + 2, y, $" {title} ", width - 4);
            }
        }
        // 특정 좌표에 글자 출력
        private void WriteAt(int x, int y, string text, int maxLength)
        {
            if (maxLength <= 0)
            {
                return;
            }

            if (x < 0 || y < 0)
            {
                return;
            }

            if (x >= Console.WindowWidth || y >= Console.WindowHeight)
            {
                return;
            }

            int remainWidth = Console.WindowWidth - x;

            if (remainWidth <= 0)
            {
                return;
            }

            int safeLength = Math.Min(maxLength, remainWidth);

            text ??= "";

            if (text.Length > safeLength)
            {
                text = text.Substring(0, safeLength);
            }

            try
            {
                Console.SetCursorPosition(x, y);
                Console.Write(text.PadRight(safeLength));
            }
            catch
            {
                // 콘솔 창 크기가 실행 중 변경될 때 발생하는 예외
            }
        }

        // Shadow를 아군 위치에 출력
        private void DrawShadowUnit(Shadow shadow, int x, int y)
        {
            string stateText = shadow.IsAlive ? "생존" : "전투불능";

            WriteAt(x, y, "   /\\_/\\", 18);
            WriteAt(x, y + 1, "  ( •_•)", 18);
            WriteAt(x, y + 2, "  /|___|\\", 18);

            WriteAt(x, y + 4, $"{shadow.Name}", 18);
            WriteAt(x, y + 5, $"Lv.{shadow.Level} {shadow.Grade}", 18);
            WriteAt(x, y + 6, $"HP {shadow.CurrentHp}/{shadow.MaxHp}", 18);
            WriteAt(x, y + 7, $"MP {shadow.CurrentMp}/{shadow.MaxMp}", 18);
            WriteAt(x, y + 8, $"{stateText}", 18);
            WriteAt(x, y + 9, GetStatusSummary(shadow), 18);
        }

        // Monster를 적 위치에 출력
        private void DrawMonsterUnit(Monster monster, int x, int y)
        {
            string stateText = monster.IsAlive ? "생존" : "처치됨";

            WriteAt(x, y, "   /^^^^\\", 18);
            WriteAt(x, y + 1, "  ( o_o )", 18);
            WriteAt(x, y + 2, "  /|###|\\", 18);

            WriteAt(x, y + 4, $"{monster.Name}", 18);
            WriteAt(x, y + 5, $"Lv.{monster.Level} {monster.Grade}", 18);
            WriteAt(x, y + 6, $"HP {monster.CurrentHp}/{monster.MaxHp}", 18);
            WriteAt(x, y + 7, $"MP {monster.CurrentMp}/{monster.MaxMp}", 18);
            WriteAt(x, y + 8, $"{stateText}", 18);
            WriteAt(x, y + 9, GetStatusSummary(monster), 18);
        }

        // 유닛에게 걸린 상태 이상 표시
        private string GetStatusSummary(Unit unit)
        {
            if (unit.StatusEffects.Count == 0)
            {
                return "";
            }

            List<string> parts = new List<string>();

            foreach (StatusEffect effect in unit.StatusEffects)
            {
                string name = effect.Type switch
                {
                    StatusEffectType.Stun => "기절",
                    StatusEffectType.Poison => "독",
                    StatusEffectType.Bleed => "출혈",
                    _ => ""
                };

                if (!string.IsNullOrWhiteSpace(name))
                {
                    parts.Add($"{name}{effect.RemainingTurns}");
                }
            }

            return string.Join(" ", parts);
        }

        // 전투 UI에 필요한 콘솔 창 크기를 확보
        private void EnsureConsoleSize()
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.CursorVisible = false;
            }
            catch
            {
                // 실패 처리
            }
        }


        // 전투화면 전체 출력
        public void DrawBattleLayout(List<Shadow> playerUnits, List<Monster> enemyUnits)
        {
            EnsureConsoleSize();

            Console.Clear();



            DrawBox(0, 0, BattleScreenWidth, BattleScreenHeight, "전투");
            DrawBox(BattleFieldX, BattleFieldY, BattleFieldWidth, BattleFieldHeight, "전장");

            DrawBox(LogBoxX, LogBoxY, LogBoxWidth, LogBoxHeight, "전투 로그");
            DrawBox(CommandBoxX, CommandBoxY, CommandBoxWidth, CommandBoxHeight, "명령 선택");
            DrawBox(InfoBoxX, InfoBoxY, InfoBoxWidth, InfoBoxHeight, "대상 / 정보");

            DrawBattleLog();
            DrawCommandMenu(0);

            WriteAt(InfoBoxX + 2, InfoBoxY + 2, "대상을 선택하세요.", InfoBoxWidth - 4);

            int shadowStartX = 7;
            int shadowStartY = 4;

            for (int i = 0; i < playerUnits.Count && i < 3; i++)
            {
                DrawShadowUnit(playerUnits[i], shadowStartX + (i * 24), shadowStartY);
            }

 
            int monsterStartX = 80;
            int monsterStartY = 4;

            for (int i = 0; i < enemyUnits.Count && i < 3; i++)
            {
                DrawMonsterUnit(enemyUnits[i], monsterStartX + (i * 18), monsterStartY);
            }
        }
        // 명령 메뉴 출력
        private void DrawCommandMenu(int selectedIndex)
        {
            for (int i = 0; i < GetBattleCommandNames().Length; i++)
            {
                DrawCommandMenuLine(i, i == selectedIndex);
            }
        }

        // 명령 메뉴 깜빡임 개선
        private void DrawCommandMenuLine(int index, bool selected)
        {
            string[] commands = GetBattleCommandNames();

            if (index < 0 || index >= commands.Length)
            {
                return;
            }

            int startX = CommandBoxX + 2;
            int startY = CommandBoxY + 2;
            int width = CommandBoxWidth - 4;
            string cursor = selected ? "▶" : " ";

            WriteAt(startX, startY + index, $"{cursor} {commands[index]}", width);
        }

        // 전투 명령에 표시할 이름 목록을 반환
        private string[] GetBattleCommandNames()
        {
            return new string[]
            {
                "공격",
                "스킬",
                "아이템"
            };
        }
        // 현재 턴 출력
        public void PrintCurrentTurn(TurnOwner currentTurn)
        {
            if (currentTurn == TurnOwner.PlayerSide)
            {
                AddLog("아군 턴");
            }
            else if (currentTurn == TurnOwner.EnemySide)
            {
                AddLog("적군 턴");
            }
        }

        // 전투 결과를 출력.
        public void PrintBattleResult(BattleResult result)
        {
            if (result == BattleResult.Continue)
            {
                AddLog("전투 계속");
            }
            else if (result == BattleResult.PlayerWin)
            {
                AddLog("전투 승리");
            }
            else if (result == BattleResult.EnemyWin)
            {
                AddLog("전투 패배");
            }
        }
        // 아군 유닛 스킬을 선택
        // 방향키로 스킬 선택, Enter로 확정
        public SkillList SelectSkill(Shadow shadow)
        {
            List<SkillList> skills = shadow.skills
                .Where(skill => !SkillDatabase.GetSkillData(skill).IsBasicAttack)
                .ToList();

            if (skills.Count == 0)
            {
                AddLog("사용 가능한 스킬 없음");
                return SkillList.Attack;
            }

            List<string> menuOptions = new List<string>();

            foreach (SkillList skill in skills)
            {
                SkillData skillData = SkillDatabase.GetSkillData(skill);
                menuOptions.Add($"{skillData.Name} MP {skillData.MpCost}");
            }

            int selectedIndex = SelectBoxMenu("스킬 선택", menuOptions, InfoBoxX, InfoBoxY, InfoBoxWidth, InfoBoxHeight);

            if (selectedIndex < 0)
            {
                return SkillList.Attack;
            }

            return skills[selectedIndex];
        }

        // 공격할 적 몬스터 선택
        public Monster SelectEnemyTarget(List<Monster> enemyUnits)
        {
            List<Monster> aliveEnemies = enemyUnits
                .Where(monster => monster.CurrentHp > 0)
                .ToList();

            if (aliveEnemies.Count == 0)
            {
                AddLog("공격할 수 있는 적 없음");
                return null;
            }
            List<string> menuOptions = new List<string>();

            foreach (Monster monster in aliveEnemies)
            {
                menuOptions.Add(monster.Name);
            }

            int selectedIndex = SelectBoxMenu("대상 선택", menuOptions, InfoBoxX, InfoBoxY, InfoBoxWidth, InfoBoxHeight);

            if (selectedIndex < 0)
            {
                return aliveEnemies[0];
            }

            return aliveEnemies[selectedIndex];
        }
        // 행동할 아군 유닛 선택
        public Shadow SelectPlayerUnit(List<Shadow> playerUnits)
        {
            List<Shadow> availablePlayers = playerUnits
                .Where(shadow => shadow.CurrentHp > 0 && !shadow.HasStatusEffect(StatusEffectType.Stun))
                .ToList();

            if (availablePlayers.Count == 0)
            {
                AddLog("행동 가능한 그림자 없음");
                return null;
            }

            List<string> menuOptions = new List<string>();

            foreach (Shadow shadow in availablePlayers)
            {
                menuOptions.Add(shadow.Name);
            }

            int selectedIndex = SelectBoxMenu("아군 선택", menuOptions, InfoBoxX, InfoBoxY, InfoBoxWidth, InfoBoxHeight);

            if (selectedIndex < 0)
            {
                return availablePlayers[0];
            }

            return availablePlayers[selectedIndex];
        }
        // 스킬 사용 결과 출력
        public void PrintSkillUse(Unit attacker, SkillList skill, Unit target, int damage)
        {
            AddLog($"{attacker.Name} {skill}");
            AddLog($"{target.Name} {damage} 피해");
        }

        // MP 부족 상황
        public void PrintMpShortage(Unit unit)
        {
            AddLog($"{unit.Name} MP 부족");
            AddLog("기본 공격 사용");
        }

        // 기본 공격으로 MP를 회복한 상황 출력
        public void PrintMpRecovery(Unit unit, int recoveryAmount)
        {
            AddLog($"{unit.Name} MP {recoveryAmount} 회복");
        }

        // 그림자 추출 결과 출력
        public void PrintExtractedShadows(List<Shadow> extractedShadows)
        {
            AddLog($"그림자 추출 {extractedShadows.Count}개");

            foreach (Shadow shadow in extractedShadows)
            {
                AddLog($"{shadow.Name} 합류");
            }
        }

        // 일반 메뉴 화면 출력
        public void DrawFrame(string title, string footer = "방향키 이동 / Enter 선택")
        {
            EnsureConsoleSize();
            Console.Clear();

            DrawBox(FrameX, FrameY, FrameWidth, FrameHeight, title);
            WriteAt(2, 3, new string('─', FrameWidth - 4), FrameWidth - 4);
            WriteAt(2, FooterY - 1, new string('─', FrameWidth - 4), FrameWidth - 4);
            WriteAt(4, FooterY + 1, footer, FrameWidth - 8);
        }

        // 공통 프레임 메뉴 출력
        public int SelectFrameMenu(string title, List<string> options, List<string>? infoLines = null, string footer = "방향키 이동 / Enter 선택")
        {
            if (options == null || options.Count == 0)
            {
                return -1;
            }

            int selectedIndex = 0;

            DrawFrame(title, footer);
            DrawBox(LeftPanelX, LeftPanelY, LeftPanelWidth, LeftPanelHeight, "메뉴");
            DrawBox(RightPanelX, RightPanelY, RightPanelWidth, RightPanelHeight, "정보");
            DrawMenuOptions(options, selectedIndex, LeftPanelX + 2, LeftPanelY + 2, LeftPanelWidth - 4, LeftPanelHeight - 4);
            DrawInfoLines(infoLines, RightPanelX + 2, RightPanelY + 2, RightPanelWidth - 4, RightPanelHeight - 4);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                int previousIndex = selectedIndex;

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;

                    if (selectedIndex < 0)
                    {
                        selectedIndex = options.Count - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;

                    if (selectedIndex >= options.Count)
                    {
                        selectedIndex = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return selectedIndex;
                }

                if (previousIndex != selectedIndex)
                {
                    DrawMenuLine(LeftPanelX + 2, LeftPanelY + 2 + previousIndex, LeftPanelWidth - 4, options[previousIndex], false);
                    DrawMenuLine(LeftPanelX + 2, LeftPanelY + 2 + selectedIndex, LeftPanelWidth - 4, options[selectedIndex], true);
                }
            }
        }

        // 안내 메세지 출력
        public void ShowMessage(string title, List<string> lines, string footer = "아무 키나 누르면 계속합니다.")
        {
            DrawFrame(title, footer);
            DrawBox(LeftPanelX, LeftPanelY, FrameWidth - 8, LeftPanelHeight, "내용");
            DrawInfoLines(lines, LeftPanelX + 2, LeftPanelY + 2, FrameWidth - 12, LeftPanelHeight - 4);
            Console.ReadKey(true);
        }

        // 입력 화면 출력, 반환
        public string ReadTextInput(string title, string prompt, string defaultValue)
        {
            DrawFrame(title, "이름 입력 후 Enter");
            DrawBox(LeftPanelX, LeftPanelY, FrameWidth - 8, LeftPanelHeight, "입력");
            WriteAt(LeftPanelX + 2, LeftPanelY + 2, prompt, FrameWidth - 12);
            WriteAt(LeftPanelX + 2, LeftPanelY + 4, "> ", 2);

            try
            {
                Console.CursorVisible = true;
                Console.SetCursorPosition(LeftPanelX + 4, LeftPanelY + 4);
            }
            catch
            {
            }

            string? input = Console.ReadLine();
            Console.CursorVisible = false;

            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            return input;
        }

        // 메뉴 선택지 출력
        private void DrawMenuOptions(List<string> options, int selectedIndex, int x, int y, int width, int height)
        {
            for (int i = 0; i < height; i++)
            {
                string text = "";

                if (i < options.Count)
                {
                    DrawMenuLine(x, y + i, width, options[i], i == selectedIndex);
                    continue;
                }

                WriteAt(x, y + i, text, width);
            }
        }

        // 메뉴 라인 수정
        private void DrawMenuLine(int x, int y, int width, string text, bool selected)
        {
            string cursor = selected ? "▶" : " ";
            WriteAt(x, y, $"{cursor} {text}", width);
        }

        // 설명 문구 출력
        private void DrawInfoLines(List<string>? lines, int x, int y, int width, int height)
        {
            for (int i = 0; i < height; i++)
            {
                string text = "";

                if (lines != null && i < lines.Count)
                {
                    text = lines[i];
                }

                WriteAt(x, y + i, text, width);
            }
        }

        // 전투에 참가할 멤버 선택
        public List<Shadow> SelectBattleParty(List<Shadow> ownedShadows, int maxCount)
        {
            const int unitsPerPage = 3;
            List<Shadow> shadows = ownedShadows ?? new List<Shadow>();

            if (!shadows.Any(shadow => shadow.IsAlive))
            {
                ShowMessage(
                    "전투 편성",
                    new List<string>
                    {
                        "전투에 참가할 수 있는 Shadow가 없습니다."
                    });
                return new List<Shadow>();
            }

            HashSet<Shadow> selectedShadows = new HashSet<Shadow>();
            int selectedIndex = 0;
            int currentPage = 0;
            int totalPages = Math.Max(1, (shadows.Count + unitsPerPage - 1) / unitsPerPage);

            DrawBattlePartySelection(shadows, selectedShadows, selectedIndex, currentPage, unitsPerPage, maxCount, "");

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                int previousIndex = selectedIndex;
                int previousPage = currentPage;
                string message = "";
                bool selectionChanged = false;
                bool infoChanged = false;
                List<Shadow> pageShadows = GetPagedShadows(shadows, currentPage, unitsPerPage);
                int startIndex = pageShadows.Count;
                int menuCount = pageShadows.Count + 1;

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;

                    if (selectedIndex < 0)
                    {
                        selectedIndex = menuCount - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;

                    if (selectedIndex >= menuCount)
                    {
                        selectedIndex = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow || keyInfo.Key == ConsoleKey.A)
                {
                    currentPage--;

                    if (currentPage < 0)
                    {
                        currentPage = totalPages - 1;
                    }

                    selectedIndex = 0;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow || keyInfo.Key == ConsoleKey.D)
                {
                    currentPage++;

                    if (currentPage >= totalPages)
                    {
                        currentPage = 0;
                    }

                    selectedIndex = 0;
                }
                else if (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.Spacebar)
                {
                    if (selectedIndex == startIndex)
                    {
                        if (selectedShadows.Count == 0)
                        {
                            message = "최소 1명을 선택해야 합니다.";
                            infoChanged = true;
                        }
                        else
                        {
                            return selectedShadows.Take(maxCount).ToList();
                        }
                    }
                    else
                    {
                        Shadow shadow = pageShadows[selectedIndex];

                        if (!shadow.IsAlive)
                        {
                            message = "전투불능 Shadow는 선택할 수 없습니다.";
                            infoChanged = true;
                        }
                        else if (selectedShadows.Contains(shadow))
                        {
                            selectedShadows.Remove(shadow);
                            selectionChanged = true;
                            infoChanged = true;
                        }
                        else if (selectedShadows.Count < maxCount)
                        {
                            selectedShadows.Add(shadow);
                            selectionChanged = true;
                            infoChanged = true;
                        }
                        else
                        {
                            message = $"최대 {maxCount}명까지 선택할 수 있습니다.";
                            infoChanged = true;
                        }
                    }
                }
                else if (keyInfo.Key == ConsoleKey.F)
                {
                    if (selectedShadows.Count == 0)
                    {
                        message = "최소 1명을 선택해야 합니다.";
                        infoChanged = true;
                    }
                    else
                    {
                        return selectedShadows.Take(maxCount).ToList();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return new List<Shadow>();
                }

                if (previousPage != currentPage || previousIndex != selectedIndex || !string.IsNullOrWhiteSpace(message))
                {
                    DrawBattlePartySelection(shadows, selectedShadows, selectedIndex, currentPage, unitsPerPage, maxCount, message);
                    continue;
                }

                if (selectionChanged)
                {
                    DrawBattlePartyLine(pageShadows, selectedShadows, selectedIndex, true);
                }

                if (infoChanged)
                {
                    DrawBattlePartyInfo(selectedShadows.Count, maxCount, currentPage, totalPages, message);
                }
            }
        }

        // 아군 유닛 목록 페이지화
        private List<Shadow> GetPagedShadows(List<Shadow> shadows, int currentPage, int unitsPerPage)
        {
            return shadows
                .Skip(currentPage * unitsPerPage)
                .Take(unitsPerPage)
                .ToList();
        }
        // 전투 참가 선택화면 출력
        private void DrawBattlePartySelection
            (
            List<Shadow> shadows,
            HashSet<Shadow> selectedShadows,
            int selectedIndex,
            int currentPage,
            int unitsPerPage,
            int maxCount,
            string message
            )
        {
            int totalPages = Math.Max(1, (shadows.Count + unitsPerPage - 1) / unitsPerPage);
            List<Shadow> pageShadows = GetPagedShadows(shadows, currentPage, unitsPerPage);
            List<string> options = new List<string>();

            foreach (Shadow shadow in pageShadows)
            {
                string selectedMark = selectedShadows.Contains(shadow) ? "[선택]" : "";
                string disabledMark = shadow.IsAlive ? "" : "[전투불능]";
                options.Add($"{shadow.Name} {selectedMark} {disabledMark}".Trim());
            }

            options.Add("전투 시작");

            DrawFrame("전투 참가 선택", "↑↓ 이동 / ←→ 페이지 / Enter 선택 / F 시작 / ESC 취소");
            DrawBox(LeftPanelX, LeftPanelY, LeftPanelWidth, LeftPanelHeight, "Shadow");
            DrawBox(RightPanelX, RightPanelY, RightPanelWidth, RightPanelHeight, "선택 정보");
            DrawMenuOptions(options, selectedIndex, LeftPanelX + 2, LeftPanelY + 2, LeftPanelWidth - 4, LeftPanelHeight - 4);
            DrawBattlePartyInfo(selectedShadows.Count, maxCount, currentPage, totalPages, message);
        }

        // 전투 참가 유닛 갱신
        private void DrawBattlePartyLine(List<Shadow> pageShadows, HashSet<Shadow> selectedShadows, int index, bool selected)
        {
            int x = LeftPanelX + 2;
            int y = LeftPanelY + 2 + index;
            int width = LeftPanelWidth - 4;

            if (index == pageShadows.Count)
            {
                DrawMenuLine(x, y, width, "전투 시작", selected);
                return;
            }

            if (index < 0 || index >= pageShadows.Count)
            {
                return;
            }

            Shadow shadow = pageShadows[index];
            string selectedMark = selectedShadows.Contains(shadow) ? "[선택]" : "";
            string disabledMark = shadow.IsAlive ? "" : "[전투불능]";
            DrawMenuLine(x, y, width, $"{shadow.Name} {selectedMark} {disabledMark}".Trim(), selected);
        }

        // 전투 참가 안내 문구
        private void DrawBattlePartyInfo(int selectedCount, int maxCount, int currentPage, int totalPages, string message)
        {
            List<string> infoLines = new List<string>
            {
                $"선택: {selectedCount} / {maxCount}",
                $"Page {currentPage + 1} / {totalPages}",
                "",
                "Enter 또는 Space: 선택/해제",
                "F: 전투 시작",
                "ESC: 취소",
                "",
                message
            };

            DrawInfoLines(infoLines, RightPanelX + 2, RightPanelY + 2, RightPanelWidth - 4, RightPanelHeight - 4);
        }

        // 아군 유닛 상세 페이지 보유 유닛 목록  
        public void ShowShadowDetailsPaged(List<Shadow> ownedShadows)
        {
            const int unitsPerPage = 3;
            List<Shadow> shadows = ownedShadows ?? new List<Shadow>();

            if (shadows.Count == 0)
            {
                ShowMessage(
                    "그림자 상세",
                    new List<string>
                    {
                        "보유한 그림자가 없습니다."
                    });
                return;
            }

            int currentPage = 0;
            int totalPages = Math.Max(1, (shadows.Count + unitsPerPage - 1) / unitsPerPage);

            DrawShadowDetailPage(shadows, currentPage, unitsPerPage);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                int previousPage = currentPage;

                if (keyInfo.Key == ConsoleKey.LeftArrow || keyInfo.Key == ConsoleKey.A)
                {
                    currentPage--;

                    if (currentPage < 0)
                    {
                        currentPage = totalPages - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow || keyInfo.Key == ConsoleKey.D)
                {
                    currentPage++;

                    if (currentPage >= totalPages)
                    {
                        currentPage = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.Enter)
                {
                    return;
                }

                if (previousPage != currentPage)
                {
                    DrawShadowDetailPage(shadows, currentPage, unitsPerPage);
                }
            }
        }

        // 아군 유닛 상세 페이지 출력
        private void DrawShadowDetailPage(List<Shadow> shadows, int currentPage, int unitsPerPage)
        {
            int totalPages = Math.Max(1, (shadows.Count + unitsPerPage - 1) / unitsPerPage);
            List<Shadow> pageShadows = shadows
                .Skip(currentPage * unitsPerPage)
                .Take(unitsPerPage)
                .ToList();
            List<string> lines = new List<string>
            {
                $"Page {currentPage + 1} / {totalPages}",
                ""
            };

            for (int i = 0; i < pageShadows.Count; i++)
            {
                Shadow shadow = pageShadows[i];
                SkillData[] skillData = shadow.GetSkills()
                    .Select(skill => SkillDatabase.GetSkillData(skill))
                    .ToArray();
                string skills = string.Join(", ", skillData.Select(skill => skill.Name));
                string stateText = shadow.IsAlive ? "생존" : "전투불능";

                lines.Add($"{currentPage * unitsPerPage + i + 1}. {shadow.Name}");
                lines.Add($"   Lv.{shadow.Level} / Grade: {shadow.Grade} / {stateText}");
                lines.Add($"   HP {shadow.CurrentHp}/{shadow.MaxHp}");
                lines.Add($"   MP {shadow.CurrentMp}/{shadow.MaxMp}");
                lines.Add($"   스킬: {skills}");
                lines.Add("");
            }

            lines.Add("←/A 이전 페이지  →/D 다음 페이지");
            lines.Add("Enter 또는 ESC 뒤로가기");

            DrawFrame("그림자 상세", "←→ 페이지 이동 / Enter 또는 ESC 뒤로가기");
            DrawBox(LeftPanelX, LeftPanelY, FrameWidth - 8, LeftPanelHeight, "보유 Shadow");
            DrawInfoLines(lines, LeftPanelX + 2, LeftPanelY + 2, FrameWidth - 12, LeftPanelHeight - 4);
        }

        // 아이템 선택
        public ItemType? SelectBattleItem(Player player)
        {
            List<ItemType> itemTypes = ItemDatabase.GetAllItemTypes();
            List<string> options = new List<string>();

            foreach (ItemType itemType in itemTypes)
            {
                ItemData itemData = ItemDatabase.GetItemData(itemType);
                options.Add($"{itemData.Name} x{player.GetItemCount(itemType)}");
            }

            options.Add("뒤로가기");

            int selectedIndex = SelectBoxMenu("아이템 선택", options, InfoBoxX, InfoBoxY, InfoBoxWidth, InfoBoxHeight);

            if (selectedIndex < 0 || selectedIndex >= itemTypes.Count)
            {
                return null;
            }

            return itemTypes[selectedIndex];
        }

        // 아이템 대상 선택
        public Shadow SelectItemTarget(List<Shadow> playerUnits, ItemType itemType)
        {
            IEnumerable<Shadow> candidates = playerUnits;

            ItemData itemData = ItemDatabase.GetItemData(itemType);

            if (itemData.EffectType == ItemEffectType.HealMp)
            {
                candidates = candidates.Where(shadow => shadow.IsAlive);
            }

            List<Shadow> targetShadows = candidates.ToList();

            if (targetShadows.Count == 0)
            {
                AddLog("사용할 대상 없음");
                return null;
            }

            List<string> options = targetShadows
                .Select(shadow => shadow.Name)
                .ToList();

            int selectedIndex = SelectBoxMenu("아이템 대상", options, InfoBoxX, InfoBoxY, InfoBoxWidth, InfoBoxHeight);

            if (selectedIndex < 0)
            {
                return null;
            }

            return targetShadows[selectedIndex];
        }

        // 전투 명령 선택
        public BattleCommandType SelectBattleCommand()
        {
            int selectedIndex = 0;
            int previousIndex = selectedIndex;
            int commandCount = GetBattleCommandNames().Length;

            DrawCommandMenu(selectedIndex);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                previousIndex = selectedIndex;

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;

                    if (selectedIndex < 0)
                    {
                        selectedIndex = commandCount - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;

                    if (selectedIndex >= commandCount)
                    {
                        selectedIndex = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return (BattleCommandType)selectedIndex;
                }

                if (previousIndex != selectedIndex)
                {
                    DrawCommandMenuLine(previousIndex, false);
                    DrawCommandMenuLine(selectedIndex, true);
                }
            }
        }

        // 선택 매뉴 처리
        private int SelectBoxMenu(string title, List<string> options, int boxX, int boxY, int boxWidth, int boxHeight)
        {
            if (options == null || options.Count == 0)
            {
                return -1;
            }

            int selectedIndex = 0;
            int visibleCount = Math.Max(1, boxHeight - 4);
            int contentX = boxX + 2;
            int contentY = boxY + 2;
            int contentWidth = boxWidth - 4;
            int scrollStart = 0;

            DrawBox(boxX, boxY, boxWidth, boxHeight, title);
            DrawBoxMenuLines(options, selectedIndex, scrollStart, visibleCount, contentX, contentY, contentWidth);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                int previousIndex = selectedIndex;
                int previousScrollStart = scrollStart;

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;

                    if (selectedIndex < 0)
                    {
                        selectedIndex = options.Count - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;

                    if (selectedIndex >= options.Count)
                    {
                        selectedIndex = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return selectedIndex;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return -1;
                }

                if (selectedIndex < scrollStart)
                {
                    scrollStart = selectedIndex;
                }
                else if (selectedIndex >= scrollStart + visibleCount)
                {
                    scrollStart = selectedIndex - visibleCount + 1;
                }

                if (previousScrollStart != scrollStart)
                {
                    DrawBox(boxX, boxY, boxWidth, boxHeight, title);
                    DrawBoxMenuLines(options, selectedIndex, scrollStart, visibleCount, contentX, contentY, contentWidth);
                }
                else if (previousIndex != selectedIndex)
                {
                    DrawMenuLine(contentX, contentY + previousIndex - scrollStart, contentWidth, options[previousIndex], false);
                    DrawMenuLine(contentX, contentY + selectedIndex - scrollStart, contentWidth, options[selectedIndex], true);
                }
            }
        }

        // 박스 메뉴 갱신 처리
        private void DrawBoxMenuLines(List<string> options, int selectedIndex, int scrollStart, int visibleCount, int x, int y, int width)
        {
            for (int i = 0; i < visibleCount; i++)
            {
                int optionIndex = scrollStart + i;

                if (optionIndex < options.Count)
                {
                    DrawMenuLine(x, y + i, width, options[optionIndex], optionIndex == selectedIndex);
                }
                else
                {
                    WriteAt(x, y + i, "", width);
                }
            }
        }

        // 전투 로그를 추
        public void AddLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            battleLogs.Add(message);

            if (battleLogs.Count > MaxLogCount)
            {
                battleLogs.RemoveAt(0);
            }
        }

        // 전투 로그 갱신
        public void RefreshBattleLog()
        {
            DrawBattleLog();
        }
        // 최근 전투 로그
        private void DrawBattleLog()
        {
            int startX = LogBoxX + 2;
            int startY = LogBoxY + 2;
            int width = LogBoxWidth - 4;

            for (int i = 0; i < MaxLogCount; i++)
            {
                string text = i < battleLogs.Count ? battleLogs[i] : "";
                WriteAt(startX, startY + i, text, width);
            }
        }

    }
}
