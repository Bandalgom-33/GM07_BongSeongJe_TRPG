using SoloLeveling.Common;

namespace SoloLeveling.Systems
{
    // 아이템 하나의 기본 정보를 저장하는 데이터 클래스
    internal class ItemData
    {
        public ItemType Type { get; }
        public string Name { get; }
        public int Price { get; }
        public int Amount { get; }
        public ItemEffectType EffectType { get; }
        public string Description { get; }
        public ItemData(ItemType type, string name, int price, int amount, ItemEffectType effectType, string description)
        {
            Type = type;
            Name = name;
            Price = price;
            Amount = amount;
            EffectType = effectType;
            Description = description;
        }
    }

    //아이템 데이터를 모아두는 데이터베이스 클래스
    internal static class ItemDatabase
    {
        private static readonly Dictionary<ItemType, ItemData> items = new Dictionary<ItemType, ItemData>
        {
            {
                ItemType.HpPotion,
                new ItemData(ItemType.HpPotion, "체력 회복 포션", 100, 300, ItemEffectType.HealHp, "Shadow의 HP를 300 회복합니다.")
            },
            {
                ItemType.MpPotion,
                new ItemData(ItemType.MpPotion, "마나 회복 포션", 120, 100, ItemEffectType.HealMp, "Shadow의 MP를 100 회복합니다.")
            }
        };

        // 아이템 종류를 기준으로 실제 아이템 정보를 가져온다
        public static ItemData GetItemData(ItemType type)
        {
            return items[type];
        }

        // 등록된 모든 아이템 종류를 반환
        public static List<ItemType> GetAllItemTypes()
        {
            return items.Keys.ToList();
        }

        // 상점에서 판매할 아이템 목록을 반환
        public static List<ItemType> GetShopItems()
        {
            return GetAllItemTypes();
        }

    }
}
