using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Guard : MonoBehaviour
{

    public enum Phase
    {
        Pause,
        Wander,
        Chase,
        Return
    }

    private const float MIN_WALK_TIMER = 1f;
    private const float MAX_WALK_TIMER = 40f;

    [MinMaxSlider(MIN_WALK_TIMER, MAX_WALK_TIMER)]
    public Vector2 walkTimer;

    private const float MIN_PAUSE_TIMER = 3f;
    private const float MAX_PAUSE_TIMER = 10f;

    [MinMaxSlider(MIN_PAUSE_TIMER, MAX_PAUSE_TIMER)]
    public Vector2 pauseTimer;

    private const float SPEED_MIN = .6f;
    private const float SPEED_MAX = 2.3f;

    [MinMaxSlider(SPEED_MIN, SPEED_MAX)]
    public Vector2 speedMinMax;

    public float radius = 4f;
    public Vector3 origin;

    private Vector3 previousPos;
    private Vector3 velocity;

    [SerializeField]
    private float maxSpeedScaled;

    private float randomSeed;

    private Vector3 seekPos;
    private Vector3 nextPos;

    [SerializeField]
    private float timer;

    [SerializeField]
    private Phase currPhase;
    private SkinnedMeshRenderer[] skinnedRendereres;

    private float radiusScaled;
    private float radiusScaledSqr;
    //private Material defMat;

    private Transform playerAvatar;

    void Awake()
    {
        //if (!isServer)
        //{
            //Destroy(this);
        //}
    }

    // Use this for initialization
    void Start()
    {

        currPhase = Phase.Pause;
        timer = Random.Range(pauseTimer.x, pauseTimer.y);

        randomSeed = Random.value * 200;

        skinnedRendereres = GetComponentsInChildren<SkinnedMeshRenderer>();
        maxSpeedScaled = Random.Range(speedMinMax.x, speedMinMax.y) * transform.localScale.z;
        GetComponent<Animator>().speed = (maxSpeedScaled / transform.localScale.z) / ((speedMinMax.x + speedMinMax.y) / 2f);

        previousPos = transform.position;


        radiusScaled = radius * transform.localScale.z;
        radiusScaledSqr = Mathf.Pow(radiusScaled, 2f);

        origin = transform.position;
        //defMat = skinnedR.sharedMaterial;
        //skinnedR.enabled = false;
    }


    // Update is called once per frame
    void Update()
    {
        switch (currPhase)
        {
            case Phase.Pause:
                UpdatePause();
                velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 10f);
                break;

            case Phase.Wander:
                UpdatePause();
                velocity += Wander() * Time.deltaTime;
                velocity += SteerInwards(radiusScaledSqr, origin, 0.4f) * 0.6f * Time.deltaTime;

                velocity = Vector3.ClampMagnitude(velocity, maxSpeedScaled);
                break;

            case Phase.Chase:
                velocity += Seek(playerAvatar.position);


                velocity = Vector3.ClampMagnitude(velocity, maxSpeedScaled * 2);
                break;
            case Phase.Return:
                if ((transform.position - origin).sqrMagnitude < radiusScaledSqr)
                    SwitchToWander();
                else velocity += Seek(origin) * Time.deltaTime;

                velocity = Vector3.ClampMagnitude(velocity, maxSpeedScaled);
                break;
        }

        if (velocity != Vector3.zero)
            transform.forward = velocity.normalized;

        nextPos = transform.position + velocity * 1.5f * transform.localScale.z;
        transform.Translate(velocity * Time.deltaTime, Space.World);

        //Debug.DrawLine(nextPos, transform.position);
    }

    void UpdatePause()
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            if (currPhase == Phase.Wander)
                SwitchToPause();
            else SwitchToWander();
        }
    }

    void SwitchToPause()
    {
        currPhase = Phase.Pause;
        timer = Random.Range(pauseTimer.x, pauseTimer.y);
    }

    void SwitchToWander()
    {
        currPhase = Phase.Wander;
        timer = Random.Range(walkTimer.x, walkTimer.y);
    }

    //public void Respawn()
    //{
    //    StopAllCoroutines();
    //}

    private Vector3 Wander()
    {
        Vector3 wanderCenter = transform.position + transform.forward * 4f;
        float wanderRadius = 3f;
        float angle = Mathf.PerlinNoise(randomSeed + Time.fixedTime, randomSeed) * Mathf.PI * wanderRadius;

        seekPos.x = wanderCenter.x + wanderRadius * Mathf.Cos(angle);
        seekPos.y = transform.position.y;
        seekPos.z = wanderCenter.z + wanderRadius * Mathf.Sin(angle);

        return Seek(seekPos);
    }

    private Vector3 Seek(Vector3 targetPos)
    {
        return ((targetPos - transform.position).normalized * maxSpeedScaled - velocity);
    }

    //private Vector3 SteerInBounds(Vector3 center, float radiusSqr)
    //{
    //    if ((nextPos - center).sqrMagnitude > radiusSqr)
    //    {
    //        return Seek(center);
    //    }
    //    else return Vector3.zero;
    //}

    private Vector3 SteerInwards(float radiusSqr, Vector3 center, float borderRatio)
    {
        float newRadius = radiusSqr * borderRatio;
        float dist = (nextPos - center).sqrMagnitude;
        if (dist > newRadius)
        {
            return Seek(center) * (dist / radiusSqr);
        }
        else return Vector3.zero;
    }

    //private void OnDrawGizmosSelected()
    //{
    //	Gizmos.color = Color.white;
    //	Gizmos.DrawWireSphere(origin, radiusScaled);
    //	Gizmos.DrawLine(transform.position, nextPos);
    //}


}