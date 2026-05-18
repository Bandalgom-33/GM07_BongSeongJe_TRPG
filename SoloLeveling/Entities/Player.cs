using SoloLeveling.Common;
using SoloLeveling.SaveData;

namespace SoloLeveling.Entities
{
    // 플레이어 정보를 관리
    internal class Player
    {
        private PlayerBase player = new PlayerBase();
        private BaseInfo baseInfo = new BaseInfo();
        private List<Shadow> ownedShadows = new List<Shadow>();
        private Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

        private int currentExp;
        private int maxExp;

        public string Name => baseInfo.Name;
        public int Level => baseInfo.Level;
        public int CurrentExp => currentExp;
        public int MaxExp => maxExp;
        public Rank Rank => player.rank;
        public int Gold => player.Gold;
        public List<Shadow> OwnedShadows => ownedShadows;


        // 새 게임을 시작할 때 플레이어 기본 정보를 생성
        public Player(string name)
        {
            baseInfo.Name = name;
            baseInfo.Level = 1;

            currentExp = 0;
            maxExp = CalculateMaxExp();

            player.rank = Rank.F;
            player.Gold = 1000;

            UpdateRank();
        }

        // 플레이어에게 경험치를 추가하고 레벨업을 처리
        public void AddExp(int amount, bool showMessage = true)
        {
            if (amount <= 0)
            {
                return;
            }

            currentExp += amount;

            if (showMessage)
            {
                Console.WriteLine($"{Name}이(가) 경험치 {amount}를 획득했습니다.");
            }

            while (currentExp >= maxExp)
            {
                currentExp -= maxExp;
                LevelUp(showMessage);
            }
        }

        // 전투 보상이나 이벤트로 골드를 추가
        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            player.Gold += amount;
        }

        // 상점 구매 등에 필요한 골드를 차감
        public bool SpendGold(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (player.Gold < amount)
            {
                return false;
            }

            player.Gold -= amount;
            return true;
        }

        // 아이템을 인벤토리에 추가
        public void AddItem(ItemType type, int count)
        {
            if (count <= 0)
            {
                return;
            }

            items[type] = GetItemCount(type) + count;
        }

        // 아이템 사용 시 보유 개수를 차감
        public bool RemoveItem(ItemType type, int count)
        {
            if (count <= 0)
            {
                return true;
            }

            int currentCount = GetItemCount(type);

            if (currentCount < count)
            {
                return false;
            }

            items[type] = currentCount - count;
            return true;
        }

        // 아이템 갯수 카운팅
        public int GetItemCount(ItemType type)
        {
            if (!items.ContainsKey(type))
            {
                return 0;
            }

            return items[type];
        }

        // 아이템 소지 확인
        public bool HasItem(ItemType type)
        {
            return GetItemCount(type) > 0;
        }

        // 플레이어 레벨을 올리고 다음 레벨에 필요한 경험치와 랭크를 갱신
        private void LevelUp(bool showMessage)
        {
            baseInfo.Level++;
            maxExp = CalculateMaxExp();
            UpdateRank();

            if (showMessage)
            {
                Console.WriteLine($"{Name}의 레벨이 {Level}이 되었습니다.");
                Console.WriteLine($"현재 랭크: {Rank}");
            }
        }

        // 현재 레벨 기준으로 다음 레벨업에 필요한 경험치를 계산
        private int CalculateMaxExp()
        {
            return 100 + (baseInfo.Level - 1) * 50;
        }

        // 플레이어 레벨에 맞춰 랭크를 갱신
        private void UpdateRank()
        {
            if (Level >= 90)
            {
                player.rank = Rank.EX;
            }
            else if (Level >= 80)
            {
                player.rank = Rank.SSS;
            }
            else if (Level >= 70)
            {
                player.rank = Rank.SS;
            }
            else if (Level >= 60)
            {
                player.rank = Rank.S;
            }
            else if (Level >= 50)
            {
                player.rank = Rank.A;
            }
            else if (Level >= 40)
            {
                player.rank = Rank.B;
            }
            else if (Level >= 30)
            {
                player.rank = Rank.C;
            }
            else if (Level >= 20)
            {
                player.rank = Rank.D;
            }
            else if (Level >= 10)
            {
                player.rank = Rank.E;
            }
            else
            {
                player.rank = Rank.F;
            }
        }

        // 새로 획득한 Shadow를 보유 목록에 추가
        public void AddShadow(Shadow shadow, bool showMessage = true)
        {
            if (shadow == null)
            {
                return;
            }

            ownedShadows.Add(shadow);

            if (showMessage)
            {
                Console.WriteLine($"{shadow.Name}이(가) 그림자 군단에 합류했습니다.");
            }
        }

        // 보유 중인 모든 Shadow의 HP와 MP를 전부 회복
        public void RecoverAllShadows(bool showMessage = true)
        {
            if (ownedShadows.Count == 0)
            {
                if (showMessage)
                {
                    Console.WriteLine("회복할 그림자가 없습니다.");
                }
                return;
            }

            foreach (Shadow shadow in ownedShadows)
            {
                shadow.FullRecover();
            }

            if (showMessage)
            {
                Console.WriteLine("모든 그림자의 HP와 MP가 회복되었습니다.");
            }
        }

        // 현재 플레이어 정보를 저장용 데이터로 변환
        public PlayerSaveData ToSaveData()
        {
            PlayerSaveData saveData = new PlayerSaveData
            {
                Name = Name,
                Level = Level,
                CurrentExp = CurrentExp,
                MaxExp = MaxExp,
                Rank = Rank,
                Gold = Gold
            };

            foreach (Shadow shadow in ownedShadows)
            {
                saveData.OwnedShadows.Add(shadow.ToSaveData());
            }

            foreach (KeyValuePair<ItemType, int> item in items)
            {
                if (item.Value > 0)
                {
                    saveData.Items.Add(new ItemSaveData
                    {
                        Type = item.Key,
                        Count = item.Value
                    });
                }
            }

            return saveData;
        }

        // 저장 파일에서 불러온 데이터로 플레이어를 복원
        public Player(PlayerSaveData saveData)
        {
            baseInfo.Name = saveData.Name;
            baseInfo.Level = saveData.Level;

            currentExp = saveData.CurrentExp;
            maxExp = saveData.MaxExp;

            player.rank = saveData.Rank;
            player.Gold = saveData.Gold;

            foreach (ShadowSaveData shadowSaveData in saveData.OwnedShadows)
            {
                Shadow restoredShadow = new Shadow(shadowSaveData);

                ownedShadows.Add(restoredShadow);
            }

            if (saveData.Items != null)
            {
                foreach (ItemSaveData itemSaveData in saveData.Items)
                {
                    if (itemSaveData.Count > 0)
                    {
                        items[itemSaveData.Type] = itemSaveData.Count;
                    }
                }
            }
        }

    }
}
