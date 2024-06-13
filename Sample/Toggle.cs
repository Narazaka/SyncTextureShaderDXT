using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace net.narazaka.vrchat.sync_texture_shaderdxt.sample
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Toggle : UdonSharpBehaviour
    {
        [SerializeField] GameObject[] Targets;
        [UdonSynced, FieldChangeCallback(nameof(Active))] bool _Active;
        bool Active
        {
            get => _Active;
            set
            {
                _Active = value;
                foreach (var target in Targets)
                {
                    target.SetActive(value);
                }
            }
        }

        private void Start()
        {
            Active = Targets[0].activeSelf;
        }

        public override void Interact() => ToggleActive();

        public void SetActiveON() => SetActive(true, nameof(SetActiveON));
        public void SetActiveOFF() => SetActive(false, nameof(SetActiveOFF));
        public void ToggleActive() => SetActive(!Active, nameof(ToggleActive));

        void SetActive(bool value, string method)
        {
            Active = value;
            if (Networking.IsOwner(gameObject))
            {
                RequestSerialization();
            }
            else
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, method);
            }
        }
    }
}
