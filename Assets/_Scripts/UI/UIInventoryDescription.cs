using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class UIInventoryDescription : MonoBehaviour
    {
        // TODO: make this work with item drop somehow

        [SerializeField]
        private Image itemImage;
        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private TMP_Text description;

        public void Awake()
        {
            ResetDescription();
        }

        public void ResetDescription()
        {
            itemImage.gameObject.SetActive(false);
            title.text = "";
            description.text = "";
        }

        public void SetDescriptrion(Sprite sprite, string itemName, string itemDesctiption)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            title.text = itemName;
            description.text = itemDesctiption;
        }
    }
}