using UnityEngine;

public interface IMoveable
{
    public float MaxSpeed       { get;  set; }
    public float Acceleration   { get;  set; }
    public float Deacceleration { get;  set; }
    public float RollSpeed      { get;  set; }
}
