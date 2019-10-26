using UnityEngine;

public class Predator : Agent
{
    protected override Vector3 Combine()
    {
        return agentConfig.coefficientForWandering * Wander();
    }
}