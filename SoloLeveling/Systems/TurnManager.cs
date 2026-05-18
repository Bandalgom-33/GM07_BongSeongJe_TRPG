using System;
using System.Collections.Generic;
using System.Linq;
using SoloLeveling.Common;
using SoloLeveling.Entities;
using SoloLeveling.Skills;
using SoloLeveling.Ui;

namespace SoloLeveling.Systems
{
    

    // 전투의 턴 흐름을 관리하는 클래스
    class TurnManager
    {
        private static readonly Random statusRandom = new Random();
        private static readonly Random battleRandom = new Random();
        private BattleUI battleUI;

        private List<Shadow> playerUnits;
        private List<Monster> enemyUnits;
        private Player currentPlayer;
        private List<Shadow> battleParticipants;
        private List<Monster> defeatedEnemies = new List<Monster>();

        public TurnOwner CurrentTurn { get; private set; }

        public TurnManager(List<Shadow> playerUnits, List<Monster> enemyUnits, Player currentPlayer = null)
        {
            this.playerUnits = playerUnits;
            this.enemyUnits = enemyUnits;
            this.currentPlayer = currentPlayer;

            // 전투 시작 시점의 아군 목록을 따로 저장
            // 전투 중 넘어온 아군 유닛 경험치 분리를 위해 사용
            battleParticipants = new List<Shadow>(playerUnits);

            CurrentTurn = TurnOwner.PlayerSide;

            battleUI = new BattleUI();
        }

        // 현재 턴이 아군 턴인지 확인
        public bool IsPlayerTurn()
        {
            return CurrentTurn == TurnOwner.PlayerSide;
        }

        // 현재 턴이 적군 턴인지 확인
        public bool IsEnemyTurn()
        {
            return CurrentTurn == TurnOwner.EnemySide;
        }

        // 턴 넘기기
        public void ChangeTurn()
        {
            if (CurrentTurn == TurnOwner.PlayerSide)
            {
                CurrentTurn = TurnOwner.EnemySide;
            }
            else
            {
                CurrentTurn = TurnOwner.PlayerSide;
            }
        }

        // 현재 턴 정보를 UI에 출력
        public void PrintCurrentTurn()
        {
            battleUI.PrintCurrentTurn(CurrentTurn);
        }
        // 아군 유닛 생존 확인
        public bool HasAlivePlayerUnit()
        {
            foreach (Shadow shadow in playerUnits)
            {
                if (shadow.CurrentHp > 0)
                {
                    return true;
                }
            }

            return false;
        }

        // 적군 유닛 생존 확인
        public bool HasAliveEnemyUnit()
        {
            foreach (Monster monster in enemyUnits)
            {
                if (monster.CurrentHp > 0)
                {
                    return true;
                }
            }

            // 반복문이 끝날 때까지 살아있는 몬스터를 찾지 못했다면 적 전멸입니다.
            return false;
        }


        // 현재 전투 결과를 확인
        public BattleResult CheckBattleResult()
        {

            bool hasPlayer = HasAlivePlayerUnit();
            bool hasEnemy = HasAliveEnemyUnit();

            if (hasPlayer && !hasEnemy)
            {
                return BattleResult.PlayerWin;
            }

            if (!hasPlayer && hasEnemy)
            {
                return BattleResult.EnemyWin;
            }

            return BattleResult.Continue;
        }

        // 현재 전투 결과를 UI에 출력
        public void PrintBattleResult()
        {
            BattleResult result = CheckBattleResult();

            battleUI.PrintBattleResult(result);
        }
        // 체력이 0 이하가 된 적 몬스터를 찾아서 적군 목록에서 제거
        public List<Monster> RemoveDeadEnemies()
        {
            List<Monster> deadEnemies = new List<Monster>();


            foreach (Monster monster in enemyUnits)
            {
                if (monster.CurrentHp <= 0)
                {
                    deadEnemies.Add(monster);
                }
            }

            foreach (Monster deadMonster in deadEnemies)
            {
                enemyUnits.Remove(deadMonster);

                // 전투 보상 계산을 위해 처치한 몬스터 목록에 기록
                defeatedEnemies.Add(deadMonster);
            }

            // 제거된 몬스터 목록을 반환합니다.
            return deadEnemies;
        }
        // 사망한 Shadow 상태를 확인합니다.
        public void RemoveDeadPlayers()
        {
            foreach (Shadow shadow in playerUnits)
            {
                if (!shadow.IsAlive)
                {
                    continue;
                }

                if (shadow.CurrentHp <= 0)
                {
                    battleUI.AddLog($"{shadow.Name}이(가) 전투 불능 상태가 되었습니다.");
                }
            }
        }

        // 죽은 몬스터 목록을 받아 Shadow로 변환
        public List<Shadow> ExtractShadows(List<Monster> deadEnemies)
        {
            List<Shadow> extractedShadows = new List<Shadow>();

            foreach (Monster deadMonster in deadEnemies)
            {
                Shadow newShadow = new Shadow(deadMonster);
                newShadow.Heal(newShadow.MaxHp);

                newShadow.RecoverMp(newShadow.MaxMp);

                if (currentPlayer != null)
                {
                    currentPlayer.AddShadow(newShadow, false);
                }

                if (playerUnits.Count < 3 && !playerUnits.Contains(newShadow))
                {
                    playerUnits.Add(newShadow);
                    battleUI.AddLog($"{newShadow.Name} 전투 참가");
                }
                else
                {
                    battleUI.AddLog($"{newShadow.Name} 그림자 저장 완료");
                }
                extractedShadows.Add(newShadow);
            }

            return extractedShadows;
        }
        // 공격 가능한 첫 번째 적 몬스터
        private Monster GetFirstAvailableEnemy()
        {
            return enemyUnits.FirstOrDefault(monster => monster.CurrentHp > 0 && !monster.HasStatusEffect(StatusEffectType.Stun));
        }

        // 적 공격 대상으로 사용할 살아 있는 아군 Shadow를 랜덤 선택
        private Shadow SelectRandomAlivePlayerUnit()
        {
            List<Shadow> alivePlayers = playerUnits
                .Where(shadow => shadow.CurrentHp > 0)
                .ToList();

            if (alivePlayers.Count == 0)
            {
                return null;
            }

            int index = battleRandom.Next(alivePlayers.Count);
            return alivePlayers[index];
        }

        // 적군 턴을 자동으로 처리
        public void ProcessEnemyTurn()
        {
            if (!IsEnemyTurn())
            {
                battleUI.AddLog("현재는 적군 턴이 아닙니다.");
                return;
            }

            ProcessSideDamageStatusEffects(enemyUnits);

            if (!HasAliveEnemyUnit())
            {
                battleUI.DrawBattleLayout(playerUnits, enemyUnits);
                return;
            }

            List<Monster> stunnedAtTurnStart = GetStunnedAliveUnits(enemyUnits);

            Monster attacker = GetFirstAvailableEnemy();

            Shadow target = SelectRandomAlivePlayerUnit();

            if (attacker == null)
            {
                ConsumeStuns(stunnedAtTurnStart);
                battleUI.AddLog("행동할 수 있는 적군이 없습니다.");
                battleUI.RefreshBattleLog();
                battleUI.DrawBattleLayout(playerUnits, enemyUnits);
                ChangeTurn();
                return;
            }

            if (target == null)
            {
                battleUI.AddLog("공격할 아군 대상이 없습니다.");
                return;
            }

            battleUI.AddLog($"{attacker.Name} → {target.Name}");

            SkillList selectedSkill = attacker.SelectRandomSkill();

            UseSkillAndLog(attacker, selectedSkill, target);

            ConsumeStuns(stunnedAtTurnStart);
            ChangeTurn();
        }
        // 아군 턴 처리
        public void ProcessPlayerTurn()
        {
            if (!IsPlayerTurn())
            {
                battleUI.AddLog("현재는 아군 턴이 아닙니다.");
                return;
            }

            ProcessSideDamageStatusEffects(playerUnits);

            if (!HasAlivePlayerUnit())
            {
                battleUI.DrawBattleLayout(playerUnits, enemyUnits);
                return;
            }

            List<Shadow> stunnedAtTurnStart = GetStunnedAliveUnits(playerUnits);

            if (!HasAvailablePlayerUnit())
            {
                ConsumeStuns(stunnedAtTurnStart);
                battleUI.AddLog("행동 가능한 그림자가 없습니다.");
                battleUI.RefreshBattleLog();
                battleUI.DrawBattleLayout(playerUnits, enemyUnits);
                ChangeTurn();
                return;
            }

            BattleCommandType selectedCommand = battleUI.SelectBattleCommand();

            if (selectedCommand == BattleCommandType.Item)
            {
                bool itemTurnEnded = ProcessItemCommand();

                if (itemTurnEnded)
                {
                    ConsumeStuns(stunnedAtTurnStart);
                }

                return;
            }

            Shadow attacker = battleUI.SelectPlayerUnit(playerUnits);
            Monster target = battleUI.SelectEnemyTarget(enemyUnits);

            if (attacker == null)
            {
                battleUI.RefreshBattleLog();
                ChangeTurn();
                return;
            }

            if (target == null)
            {
                battleUI.AddLog("공격할 적 대상이 없습니다.");
                battleUI.RefreshBattleLog();
                return;
            }

            SkillList selectedSkill = SkillList.Attack;

            if (selectedCommand == BattleCommandType.Skill)
            {
                selectedSkill = battleUI.SelectSkill(attacker);
            }

            UseSkillAndLog(attacker, selectedSkill, target);
            ConsumeStuns(stunnedAtTurnStart);

            battleUI.DrawBattleLayout(playerUnits, enemyUnits);

            ChangeTurn();
        }

        // 행동 가능한 아군 확인
        private bool HasAvailablePlayerUnit()
        {
            return playerUnits.Any(shadow => shadow.CurrentHp > 0 && !shadow.HasStatusEffect(StatusEffectType.Stun));
        }

        // 살아 있지만 기절 상태인 유닛 목록
        private List<TUnit> GetStunnedAliveUnits<TUnit>(IEnumerable<TUnit> units)
            where TUnit : Unit
        {
            return units
                .Where(unit => unit.CurrentHp > 0 && unit.HasStatusEffect(StatusEffectType.Stun))
                .ToList();
        }

        // 기절 상태를 이번 턴에 소모
        private void ConsumeStuns<TUnit>(IEnumerable<TUnit> stunnedUnits)
            where TUnit : Unit
        {
            foreach (TUnit unit in stunnedUnits)
            {
                if (unit.CurrentHp > 0 && unit.TryConsumeStun())
                {
                    battleUI.AddLog($"{unit.Name} 기절");
                    battleUI.AddLog("행동 불가");
                }
            }
        }

        // 전투 중 아이템 선택과 사용 처리
        private bool ProcessItemCommand()
        {
            if (currentPlayer == null)
            {
                battleUI.AddLog("아이템 정보를 찾을 수 없습니다.");
                battleUI.RefreshBattleLog();
                return false;
            }

            ItemType? selectedItem = battleUI.SelectBattleItem(currentPlayer);

            if (selectedItem == null)
            {
                battleUI.AddLog("아이템 사용 취소");
                battleUI.RefreshBattleLog();
                return false;
            }

            ItemType itemType = selectedItem.Value;

            if (!currentPlayer.HasItem(itemType))
            {
                battleUI.AddLog("보유 수량 부족");
                battleUI.RefreshBattleLog();
                return false;
            }

            Shadow target = battleUI.SelectItemTarget(playerUnits, itemType);

            if (target == null)
            {
                battleUI.RefreshBattleLog();
                return false;
            }

            ItemData itemData = ItemDatabase.GetItemData(itemType);
            int recovered = 0;

            if (itemData.EffectType == ItemEffectType.HealHp)
            {
                recovered = target.HealHp(itemData.Amount);
                battleUI.AddLog($"{target.Name} HP {recovered} 회복");
            }
            else if (itemData.EffectType == ItemEffectType.HealMp)
            {
                recovered = target.HealMp(itemData.Amount);
                battleUI.AddLog($"{target.Name} MP {recovered} 회복");
            }

            currentPlayer.RemoveItem(itemType, 1);
            battleUI.DrawBattleLayout(playerUnits, enemyUnits);
            ChangeTurn();
            return true;
        }

        // 스킬 사용, 피해 적용, 로그 출력을 한 번에 처리
        private void UseSkillAndLog(Unit attacker, SkillList selectedSkill, Unit target)
        {
            SkillList actualSkill = selectedSkill;
            SkillData selectedSkillData = SkillDatabase.GetSkillData(selectedSkill);
            int beforeHp = target.CurrentHp;
            int beforeMp = attacker.CurrentMp;

            if (selectedSkill != SkillList.Attack && attacker.CurrentMp < selectedSkillData.MpCost)
            {
                battleUI.PrintMpShortage(attacker);
                actualSkill = SkillList.Attack;
            }

            int damage = attacker.UseSkill(actualSkill, target);
            damage = Math.Max(0, beforeHp - target.CurrentHp);

            battleUI.PrintSkillUse(attacker, actualSkill, target, damage);

            if (actualSkill == SkillList.Attack && attacker.CurrentMp > beforeMp)
            {
                battleUI.PrintMpRecovery(attacker, attacker.CurrentMp - beforeMp);
            }

            if (beforeHp > 0 && target.CurrentHp <= 0)
            {
                battleUI.AddLog($"{target.Name} 쓰러짐");
            }

            if (target.IsAlive)
            {
                ApplyStatusEffect(actualSkill, target);
            }
        }

        // 한 진영의 독, 출혈 같은 지속 피해 상태 이상을 처리
        private void ProcessSideDamageStatusEffects<TUnit>(IEnumerable<TUnit> units)
            where TUnit : Unit
        {
            foreach (TUnit unit in units.Where(unit => unit.CurrentHp > 0).ToList())
            {
                List<StatusEffectResult> results = unit.ProcessStatusEffects();

                foreach (StatusEffectResult result in results)
                {
                    if (result.Type == StatusEffectType.Poison)
                    {
                        battleUI.AddLog($"{unit.Name} 독 피해 {result.Damage}");
                    }
                    else if (result.Type == StatusEffectType.Bleed)
                    {
                        battleUI.AddLog($"{unit.Name} 출혈 피해 {result.Damage}");
                    }
                }

                if (!unit.IsAlive)
                {
                    battleUI.AddLog($"{unit.Name} 쓰러짐");
                }
            }

            battleUI.RefreshBattleLog();
        }

        // 스킬에 설정된 상태 이상 효과를 확률에 따라 대상에게 적용
        private void ApplyStatusEffect(SkillList skill, Unit target)
        {
            SkillData skillData = SkillDatabase.GetSkillData(skill);

            if (skillData.StatusEffectType == StatusEffectType.None || skillData.StatusChance <= 0)
            {
                return;
            }

            if (statusRandom.Next(100) >= skillData.StatusChance)
            {
                battleUI.AddLog($"{target.Name} 상태이상 저항");
                return;
            }

            target.AddStatusEffect
            (new StatusEffect
            {
                Type = skillData.StatusEffectType,
                RemainingTurns = skillData.StatusDuration,
                DamagePerTurn = skillData.StatusDamagePerTurn
            }
            );

            battleUI.AddLog(GetStatusEffectLog(target, skillData.StatusEffectType));
        }

        // 적용된 상태 이상 종류에 맞는 전투 로그 문구
        private string GetStatusEffectLog(Unit target, StatusEffectType type)
        {
            return type switch
            {
                StatusEffectType.Stun => $"{target.Name} 기절",
                StatusEffectType.Poison => $"{target.Name} 독 상태",
                StatusEffectType.Bleed => $"{target.Name} 출혈 상태",
                _ => $"{target.Name} 상태 변화"
            };
        }

        // 전투 승리 후 보상을 지급
        private void GiveVictoryRewards()
        {
            if (defeatedEnemies.Count == 0)
            {
                battleUI.AddLog("처치한 적이 없어 보상이 없습니다.");
                return;
            }

            int playerExpReward = 0;
            int shadowExpReward = 0;

            foreach (Monster monster in defeatedEnemies)
            {
                playerExpReward += monster.Level * 30;
                shadowExpReward += monster.Level * 20;
            }

            battleUI.AddLog("===== 전투 보상 =====");

            if (currentPlayer != null)
            {
                currentPlayer.AddExp(playerExpReward, false);
            }
            else
            {
                battleUI.AddLog("Player 정보가 연결되지 않아 Player 경험치는 지급되지 않았습니다.");
            }
            foreach (Shadow shadow in battleParticipants)
            {
                if (shadow.IsAlive)
                {
                    shadow.AddExp(shadowExpReward);
                }
            }

            battleUI.AddLog($"Player 경험치 보상: {playerExpReward}");
            battleUI.AddLog($"Shadow 경험치 보상: {shadowExpReward}");
            battleUI.AddLog("=====================");
        }
        // 전투 루프
        public BattleResult StartBattleLoop()
        {
            battleUI.AddLog("전투 시작");
            battleUI.DrawBattleLayout(playerUnits, enemyUnits);

            while (true)
            {
                BattleResult result = CheckBattleResult();

                if (result != BattleResult.Continue)
                {
                    break;
                }

                PrintCurrentTurn();
                battleUI.RefreshBattleLog();

                if (IsPlayerTurn())
                {
                    ProcessPlayerTurn();
                }
                else if (IsEnemyTurn())
                {
                    ProcessEnemyTurn();
                }

                RemoveDeadPlayers();


                List<Monster> deadEnemies = RemoveDeadEnemies();

                if (deadEnemies.Count > 0)
                {
                    List<Shadow> extractedShadows = ExtractShadows(deadEnemies);

                    battleUI.PrintExtractedShadows(extractedShadows);
                }
                battleUI.DrawBattleLayout(playerUnits, enemyUnits);


            }

            BattleResult finalResult = CheckBattleResult();

            PrintBattleResult();

            if (finalResult == BattleResult.PlayerWin)
            {
                GiveVictoryRewards();
            }
            battleUI.DrawBattleLayout(playerUnits, enemyUnits);
            Console.ReadKey(true);
            ClearBattleStatusEffects();
            return finalResult;
        }

        // 전투 종료 후 상태 이상을 모두 제거
        private void ClearBattleStatusEffects()
        {
            foreach (Shadow shadow in playerUnits)
            {
                shadow.ClearStatusEffects();
            }

            foreach (Monster monster in enemyUnits)
            {
                monster.ClearStatusEffects();
            }
        }

    }
}
