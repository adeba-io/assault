using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault.Maneuvers
{
    [CreateAssetMenu(fileName = "New Technique", menuName = "Assault/Maneuvers/Technique")]
    public class Technique : Maneuver
    {
        [SerializeField] Attack[] _attacks;

        [SerializeField] List<InputComboNode> _links;

        FighterDamager _fighterDamager;
        Damageable _fighterDamageable;

        new public FighterController fighterController
        {
            set
            {
                _fighterController = value;
                _fighterPhysics = _fighterController.GetComponent<FighterPhysics>();
                _fighterDamager = _fighterController.GetComponent<FighterDamager>();
                _fighterDamageable = _fighterController.GetComponent<Damageable>();
            }
        }

        public List<InputComboNode> links { get { return _links; } }

        public Technique() { }

        public override void Initialize(FighterController controller)
        {
            base.Initialize(controller);
        }

        public override void Update()
        {
            base.Update();

            for (int i = 0; i < _attacks.Length; i++)
            {
                if (_currentFrame == _attacks[i].enableFrame)
                    _attacks[i].Enable(_fighterDamager);

                else if (_currentFrame == _attacks[i].disableFrame)
                    _attacks[i].Disable();
            }
        }
    }
}
