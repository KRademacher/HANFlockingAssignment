using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour
{
	public Transform agentPrefab;
    public Transform predatorPrefab;

	public int numberOfAgents = 5;
    public int numberOfPredators = 5;

	public List<Agent> agents;
    public List<Predator> predators;

    public float bound;
    public float spawnRadius;

	void Start()
    {
		agents = new List<Agent>();
        predators = new List<Predator>();

        Spawn(predatorPrefab, numberOfPredators);
        Spawn(agentPrefab, numberOfAgents);

		agents.AddRange(FindObjectsOfType<Agent>());
        predators.AddRange(FindObjectsOfType<Predator>());
	}

	private void Spawn(Transform prefab, int numberOfAgents)
    {
		for(int i = 0; i < numberOfAgents; i++){

			var obj = Instantiate(prefab,
			                      new Vector3(Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius)),
			                      Quaternion.identity);
		}
	}

	public List<Agent> GetNeighbours(Agent agent, float radius)
    {
        List<Agent> neighbours = new List<Agent>();
        foreach (Agent otherAgent in agents)
        {
            if (otherAgent != agent && Vector3.Distance(agent.position, otherAgent.position) <= radius)
            {
                neighbours.Add(otherAgent);
            }
        }
        return neighbours;
	}

    public List<Predator> GetEnemies(Agent agent, float radius)
    {
        List<Predator> enemies = new List<Predator>();
        foreach (var predator in predators)
        {
            if (Vector3.Distance(agent.position, predator.position) <= radius)
            {
                enemies.Add(predator);
            }
        }
        return enemies;
    }
}