using Unity.Netcode;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Supply", menuName = "Supply/Supply")]
    public class SupplySO : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [SerializeField] private Sprite icon;
        
        public Sprite Icon => icon;
        public string DisplayName => displayName;
        
        public static implicit operator string(SupplySO self) => self.id;

        public SupplyData Create()
        {
            return new SupplyData(id);
        }
    }
}