using UnityEngine;

public class AgentConfig : MonoBehaviour
{
	public float radiusForCohesion = 30;
	public float radiusForSeparation;
	public float radiusForAlignment;
    public float radiusForWandering = 2f;
    public float wanderJitter = 10f;
    public float wanderDistance = 3f;

    public float enemyRadius;

	public float coefficientForCohesion;
	public float coefficientForSeparation;
	public float coefficientForAlignment;
    public float coefficientForWandering = 10f;
    public float coefficientForAvoiding;

    public float maxFieldOfViewAngle;

    public float maxAcceleration;
    public float maxVelocity;
}