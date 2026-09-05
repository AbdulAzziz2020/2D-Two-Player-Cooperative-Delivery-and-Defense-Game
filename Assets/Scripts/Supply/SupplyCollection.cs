using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class SupplyCollection : MonoBehaviour
    {
        public static SupplyCollection Singleton { get; private set; }
        
        [SerializeField] private SupplySO[] supplyList;

        private Dictionary<string, SupplySO> lookup;

        private void Awake()
        {
            Singleton = this;    
        }

        public SupplySO GetSupply(string id) => lookup.GetFromLookup(supplyList, x => x, x => x, id);

        public SupplySO GetRandomSupply() => supplyList.GetRandom();
    }
}