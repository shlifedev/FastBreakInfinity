using System.Collections.Generic;

namespace LD.Numeric.ProbabilityTable
{
    
    public class ProbabilityTable<T> where T : IProbaiblityItem
    {
        public ProbabilityTable(IRandomizer randomizer)
        {
            _randomizer = randomizer;
        }
        
        public ProbabilityTable()
        {
            _randomizer = new DefaultRandomizer();
        }

        private List<T> Items { get; } = new();
        
        public IReadOnlyList<T> GetItems() => Items;

        private IRandomizer _randomizer;
        
        private HashSet<T> UniqueItems { get; } = new();

        private long TotalProbability { get; set; }
        
        
        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if(UniqueItems.Contains(item))
                return;
            Items.Add(item);
            UniqueItems.Add(item);
            TotalProbability += item.Probability;
        }


        /// <summary>
        /// 특정 아이템의 확률이 변경 되었을 때 호출
        /// </summary>
        /// <param name="item"></param>
        public void Update(T item)
        {
            TotalProbability = 0;
            foreach (var i in Items)
            {
                TotalProbability += i.Probability;
            }
        }
        /// <summary>
        /// 아이템 삭제
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        { 
            if(UniqueItems.Contains(item) == false)
                return;
            Items.Remove(item);
            UniqueItems.Remove(item);
            TotalProbability -= item.Probability;
        }

        
  
        
        
        
        /// <summary>
        /// 아이템의 확률을 반환합니다.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public double GetItemProbability(T item)
        {
            return (double)item.Probability / TotalProbability;
        }
        
        
        /// <summary>
        /// 아이템을 랜덤으로 반환합니다.
        /// </summary>
        /// <returns></returns>
        public T GetRandomItem()
        {
            long randomValue = _randomizer.GetRandomValue(0, TotalProbability);
            long currentProbability = 0;
            foreach (var item in Items)
            {
                currentProbability += item.Probability;
                if (randomValue < currentProbability)
                    return item;
            }
            return default;
        }
         
        
    }
}