using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class ConsumableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private List<ModiflierData> modiflierDatas = new List<ModiflierData>();
        public string ActionName => "Consume";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            foreach (ModiflierData data in modiflierDatas)
            {
                data.statModiflier.AffectCharacter(character,data.value);
            }
            return true;
        }
    }

    public interface IDestroyableItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get;}

        public AudioClip actionSFX { get;}
        bool PerformAction(GameObject character, List<ItemParameter> itemState);
    }

    [Serializable]
    public class ModiflierData
    {
        public CharactorStatModiflierSO statModiflier;
        public float value;
    }
}