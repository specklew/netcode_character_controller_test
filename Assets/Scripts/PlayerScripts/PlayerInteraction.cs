using System;
using Interaction;
using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    [RequireComponent(typeof(PlayerUIController)), RequireComponent(typeof(Player))]
    public class PlayerInteraction : NetworkBehaviour
    {
        private Player _player;
        private PlayerUIController _uiController;

        private string _interactionKey;

        private RaycastHit _hit;
        private Collider _lastCollider;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _uiController = GetComponent<PlayerUIController>();

            PlayerInputActions map = new();

            _interactionKey = map.FindAction("Player/Interaction").bindings[0].path;
            
            int index = _interactionKey.LastIndexOf(">/", StringComparison.Ordinal) + 2;
            
            _interactionKey = _interactionKey.Substring(index, _interactionKey.Length - index);
            _interactionKey = _interactionKey.ToUpper();
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (Physics.Raycast(_player.camera.transform.position, _player.camera.transform.forward, out var hit, _player.interactionRaycastDistance))
            {
                if (_player.input.interaction)
                {
                    InteractableObject interactableObject = hit.collider.GetComponentInParent(typeof(InteractableObject)) as InteractableObject;
                    if (interactableObject != null)
                    {
                        PlayerInteractWithObject(interactableObject);
                    }
                }
            
                if (hit.collider == _lastCollider) return; //If raycast hits the same object return.
            
                //Disable highlight on the last collider.
                if (_lastCollider != null)
                {
                    InteractableObject interact1 = _lastCollider.GetComponentInParent(typeof(InteractableObject)) as InteractableObject;
                    if (interact1 != null)
                    {
                        HideInteractionEffects(interact1);
                    }
                }
            
                InteractableObject interact2 = hit.collider.GetComponentInParent(typeof(InteractableObject)) as InteractableObject;
                if (interact2 != null)
                {
                    ShowInteractionEffects(interact2);
                }
            
                _lastCollider = hit.collider;
            }
            else
            {
                if (_lastCollider == null) return;
            
                //If the raycast leaves the collider disable highlight.
                InteractableObject interact = _lastCollider.GetComponentInParent(typeof(InteractableObject)) as InteractableObject;
                if (interact != null)
                {
                    HideInteractionEffects(interact);
                }

                _lastCollider = null;
            }
        }

        private void PlayerInteractWithObject(InteractableObject interactableObject)
        {
            interactableObject.Interact();
        }

        private void ShowInteractionEffects(InteractableObject interactableObject)
        {
            interactableObject.EnableHighlight();
            _uiController.ShowInteractionText("[ Press " + _interactionKey + " ]");
        }

        public void HideInteractionEffects(InteractableObject interactableObject)
        {
            interactableObject.DisableHighlight();
            _uiController.HideInteractionText();
        }
    }
}
