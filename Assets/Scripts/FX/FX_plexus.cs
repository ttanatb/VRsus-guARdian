using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FX_plexus : MonoBehaviour {

    public float maxDistance = 1.0f;
    public int maxConnections = 5;
    public int maxLineRendereres = 100;

    new ParticleSystem particleSystem;
    ParticleSystem.Particle[] particles;

    ParticleSystem.MainModule particleSystemMainModule;

    public LineRenderer lineRendererTemplate;

    List<LineRenderer> lineRenderers = new List<LineRenderer>();

    Transform _transform;
	// Use this for initialization
	void Start () {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystemMainModule = particleSystem.main;

#if UNITY_IOS
        Destroy(gameObject);
#endif
    }
	
	// Update is called once per frame
	void LateUpdate () {
        int maxParticles = particleSystemMainModule.maxParticles;

        if (particles == null || particles.Length < maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        int lrIndex = 0;
        int lineRendererCount = lineRenderers.Count;

        if (lineRendererCount > maxLineRendereres)
        {
            for (int i = maxLineRendereres; i < lineRendererCount; i++)
            {
                Destroy(lineRenderers[i].gameObject);     
            }

            int removedCount = lineRendererCount - maxLineRendereres;
            lineRenderers.RemoveRange(maxLineRendereres, removedCount);
            lineRendererCount -= removedCount;
        }
       

        if (maxConnections > 0 && maxLineRendereres > 0)
        {


            particleSystem.GetParticles(particles);
            int particleCount = particleSystem.particleCount;

            float maxDistanceSqr = maxDistance * maxDistance;

        

            Vector3 p1_position, p2_position;

            ParticleSystemSimulationSpace simulationSpace = particleSystemMainModule.simulationSpace;

            switch (simulationSpace)
            {
                case ParticleSystemSimulationSpace.Local:
                    {
                        _transform = transform;

                        break;
                    }
                case ParticleSystemSimulationSpace.Custom:
                    {
                        _transform = particleSystemMainModule.customSimulationSpace;

                        break;
                    }
                case ParticleSystemSimulationSpace.World:
                    {
                        _transform = transform;

                        break;
                    }
                default:
                    {
                        throw new System.NotSupportedException(
                            string.Format("Unsupported Simulation Space '{0}'.", System.Enum.GetName(typeof(ParticleSystemSimulationSpace), particleSystemMainModule.simulationSpace)));
                    }
            }
            for (int i = 0; i < particleCount; i++)
            {
                if (lrIndex >= maxLineRendereres)
                    break;
                p1_position = particles[i].position;

                int connections = 0;
                for (int j = i + 1; j < particleCount; j++)
                {
                    p2_position = particles[j].position;
                    float distanceSqr = Vector3.SqrMagnitude(p1_position - p2_position);

                    if (distanceSqr <= maxDistanceSqr)
                    {
                        LineRenderer lr;

                        if (lrIndex == lineRendererCount)
                        {
                            lr = Instantiate(lineRendererTemplate, _transform, false);
                            lineRenderers.Add(lr);

                            lineRendererCount++;

                        }

                        lr = lineRenderers[lrIndex];

                        lr.enabled = true;
                        lr.useWorldSpace = simulationSpace == ParticleSystemSimulationSpace.World ? true : false;
                        
                        lr.SetPosition(0, p1_position);
                        lr.SetPosition(1, p2_position);

                        lr.startColor = particles[i].color;
                        lr.endColor = particles[j].color;


                        lrIndex++;
                        connections++;

                        if (connections >= maxConnections || lrIndex >= maxLineRendereres)
                            break;

                    }

                }
            }
        }

        for (int i = lrIndex; i < lineRendererCount; i++)
        {
            lineRenderers[i].enabled = false;
        }

	}
}
