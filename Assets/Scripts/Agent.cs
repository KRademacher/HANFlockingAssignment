using System;
using UnityEngine;

public class Agent: MonoBehaviour
{
	public Vector3 position;
	public Vector3 velocity;
	public Vector3 acceleration;
    public Vector3 wanderTarget;
	public World world;
    public AgentConfig agentConfig;

    private float time;

	void Start()
    {
		world = FindObjectOfType<World>();
        agentConfig = FindObjectOfType<AgentConfig>();
        position = transform.position;

        //Add an initial velocity for test purposes
        velocity = new Vector3(UnityEngine.Random.Range(-3, 3), 0, UnityEngine.Random.Range(-3, 3));
	}
	
	void Update()
    {
        time = Time.deltaTime;

        acceleration = Combine();
        acceleration = Vector3.ClampMagnitude(acceleration, agentConfig.maxAcceleration);

        //Euler forward integration
        //modify the velocity with acceleration and time factor
        velocity = velocity + acceleration * time;

        //set a max velocity to avoid unrealistic values;
        velocity = Vector3.ClampMagnitude(velocity, agentConfig.maxVelocity);

        //Modify the position with velocity and time factor
        position = position + velocity * time;

        WrapAround(ref position, -world.bound, world.bound);

        transform.position = position;

        //Look at the way it's going.
        if (velocity.magnitude > 0)
        {
            transform.LookAt(position + velocity);
        }
    }

    /// <summary>
    /// Return a vector that will steer our current velocity towards the centre of mass of all nearby neighbours
    /// </summary>
    /// <returns></returns>
	private Vector3 Cohesion()
    {
        Vector3 centreOfFlock = new Vector3();
        int countOfAgents = 0;

        var neighbours = world.GetNeighbours(this, agentConfig.radiusForCohesion);
        //print(neighbours.Count);

        //return empty Vector3 if no neighbours
        if (neighbours.Count == 0)
        {
            return centreOfFlock;
        }

        //find the centre of mass of al neighbours
        foreach (var agent in neighbours)
        {
            if (IsInFieldOfView(agent.position))
            {
                centreOfFlock += agent.position;
                countOfAgents++;
            }
        }
        if (countOfAgents == 0)
        {
            return centreOfFlock;
        }

        centreOfFlock /= countOfAgents;

        //a vector from our position x towards the centre of mass r
        centreOfFlock = centreOfFlock - this.position;

		return centreOfFlock.normalized;
	}

    /// <summary>
    /// Steer in the opposite direction from each of our nearby neighbours
    /// </summary>
    /// <returns></returns>
	private Vector3 Separation()
    {
        Vector3 avoidDirection = new Vector3();

        var neighbours = world.GetNeighbours(this, agentConfig.radiusForSeparation);
        if (neighbours.Count == 0)
        {
            return avoidDirection;
        }

        //add the contribution of each neighbour towards this agent
        foreach (var agent in neighbours)
        {
            if (IsInFieldOfView(agent.position))
            {
                Vector3 towardsMe = this.position - agent.position;

                //force contribution will vary inversly proportional to distance
                if (towardsMe.magnitude > 0)
                {
                    avoidDirection += towardsMe.normalized / towardsMe.magnitude;
                }
            }
        }
		return avoidDirection.normalized;
	}

	private Vector3 Alignment()
    {
        Vector3 rotationAlignment = new Vector3();

        var neighbours = world.GetNeighbours(this, agentConfig.radiusForAlignment);
        if (neighbours.Count == 0)
        {
            return rotationAlignment;
        }

        //match direction and speed == match velocity
        foreach (var agent in neighbours)
        {
            if (IsInFieldOfView(agent.position))
            {
                rotationAlignment += agent.velocity;
            }
        }

		return rotationAlignment.normalized;
	}

    protected Vector3 Wander()
    {
        float jitter = agentConfig.wanderJitter * time;
        // add a small random vector to the target's position so it jitters on the circle
        float randomJitter = UnityEngine.Random.value * jitter;
        wanderTarget += new Vector3(randomJitter, 0, randomJitter);
        
        // reproject the vector back to unit circle
        wanderTarget = wanderTarget.normalized;
        
        // increase length to be the same as the radius of the wander circle
        wanderTarget *= agentConfig.radiusForWandering;

        // position the target (circle) in front of the agent
        Vector3 target = wanderTarget + new Vector3(0, 0, agentConfig.wanderDistance);

        // project the target from local space to world space ???
        Vector3 targetInWorld = transform.TransformVector(target);

        // steer towards it
        targetInWorld -= position;

        targetInWorld = targetInWorld.normalized;

        return targetInWorld;
    }

    protected virtual Vector3 Combine()
    {
        return agentConfig.coefficientForCohesion * Cohesion() +
            agentConfig.coefficientForSeparation * Separation() +
            agentConfig.coefficientForAlignment * Alignment() +
            agentConfig.coefficientForWandering * Wander() +
            agentConfig.coefficientForAvoiding * AvoidEnemies();
	}

    private Vector3 AvoidEnemies()
    {
        Vector3 avoidVector = new Vector3();
        var enemies = world.GetEnemies(this, agentConfig.enemyRadius);
        if (enemies.Count > 0)
        {
            foreach (var enemy in enemies)
            {
                avoidVector += Flee(enemy.position);
            }
        }
        return avoidVector.normalized;
    }

    private Vector3 Flee(Vector3 target)
    {
        Vector3 fleeVector = position - target;
        fleeVector = fleeVector.normalized * agentConfig.maxVelocity;
        return fleeVector - velocity;
    }

    private void WrapAround(ref Vector3 vector, float min, float max)
    {
        vector.x = WrapAroundFloat(vector.x, min, max);
        vector.y = WrapAroundFloat(vector.y, min, max);
        vector.z = WrapAroundFloat(vector.z, min, max);
    }

    private float WrapAroundFloat(float value, float min, float max)
    {
        if (value > max)
        {
            value = min;
        }
        else if (value < min)
        {
            value = max;
        }
        return value;
    }

    private bool IsInFieldOfView(Vector3 vector) => Vector3.Angle(vector, position) <= agentConfig.maxFieldOfViewAngle;
}