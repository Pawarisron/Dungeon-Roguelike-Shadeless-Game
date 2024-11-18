using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharactorStatHealthModiflierSO : CharactorStatModiflierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {

        HealthManager health = character.GetComponent<HealthManager>();
        if (health != null) 
            health.Heal(val);

    }
}
