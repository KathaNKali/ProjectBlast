using UnityEngine;
using ProjectBlast.Grid;

namespace ProjectBlast.Heroes
{
    /// <summary>
    /// Base Hero class - represents a deployable combat unit.
    /// Heroes are placed on grids and auto-fire at enemies.
    /// 
    /// This is a minimal stub for GridManager integration.
    /// Full implementation will include TDE Weapon, Health, and combat logic.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hero : MonoBehaviour
    {
        [Header("Grid Integration")]
        [Tooltip("Current slot this hero occupies (managed by GridManager)")]
        public GridSlot CurrentGridSlot;
        
        [Header("Hero Identity")]
        public string HeroName = "Hero";
        
        [Header("Visual Feedback")]
        [Tooltip("Material to apply when hero is selected/highlighted")]
        public Material HighlightMaterial;
        
        private Material _originalMaterial;
        private Renderer _renderer;
        
        /// <summary>
        /// Whether this hero is currently in the Firing zone and can attack
        /// </summary>
        public bool IsInFiringZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Firing;
        
        /// <summary>
        /// Whether this hero is in the Active zone and ready for deployment
        /// </summary>
        public bool IsInActiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Active;
        
        /// <summary>
        /// Whether this hero is in the Passive zone and waiting in queue
        /// </summary>
        public bool IsInPassiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Passive;
        
        void Start()
        {
            if (string.IsNullOrEmpty(HeroName))
            {
                HeroName = gameObject.name;
            }
            
            // Cache renderer for visual feedback
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
            }
        }
        
        /// <summary>
        /// Called when hero is clicked (requires Collider component)
        /// </summary>
        void OnMouseDown()
        {
            // Block input during animations
            if (HeroQueueManager.Instance != null && HeroQueueManager.Instance.IsAnimating)
            {
                return;
            }
            
            // Only heroes in Active zone can be deployed
            if (IsInActiveZone)
            {
                HeroQueueManager.Instance?.OnHeroClicked(this);
            }
        }
        
        /// <summary>
        /// Highlight this hero (visual feedback)
        /// </summary>
        public void Highlight()
        {
            if (_renderer != null && HighlightMaterial != null)
            {
                _renderer.material = HighlightMaterial;
            }
        }
        
        /// <summary>
        /// Remove highlight from this hero
        /// </summary>
        public void Unhighlight()
        {
            if (_renderer != null && _originalMaterial != null)
            {
                _renderer.material = _originalMaterial;
            }
        }
    }
}
