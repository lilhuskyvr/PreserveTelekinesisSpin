using System;
using ThunderRoad;
using UnityEngine;

namespace PreserveTelekinesisSpin
{
    public class CustomJointItem : MonoBehaviour
    {
        private Item _item;
        public ConfigurableJoint joint;

        private void Awake()
        {
            _item = GetComponent<Item>();
        }

        private void OnDestroy()
        {
            Destroy(joint);
        }
    }
}