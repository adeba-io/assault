using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Managers;

namespace Assault.Boxes
{
    public class Hurtbox : InteractionBox
    {
        protected override void AssignBoxColor()
        {
            for (int i = 0; i < _renderShapes.Length; i++) _renderShapes[i].GetComponent<SpriteRenderer>().color = FighterManager.FM.hurtboxColor;
        }
    }
}
