using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour {

	public enum Phase
	{
		Spawn,
		Pause,
		Wander,
		Death
	}

    

    private const float MIN_WALK_TIMER = 1f;
    private const float MAX_WALK_TIMER = 40f;

    [MinMaxSlider(MIN_WALK_TIMER, MAX_WALK_TIMER)]
    public Vector2 walkTimer;

	private const float MIN_PAUSE_TIMER = 3f;
	private const float MAX_PAUSE_TIMER = 10f;

	[MinMaxSlider(MIN_PAUSE_TIMER, MAX_PAUSE_TIMER)]
	public Vector2 pauseTimer;

	private Animator anim;
	private Vector3 velocity;
	private float maxSpeed;
	private bool isWandering = true;
	private Vector3 previousPos;
	private float randomSeed;
	private Vector3 seekPos;
	private Vector3 nextPos;
	private Vector3 steerVec;

	private float timer;
	private bool isPausing = false;

	[SerializeField]
	private Phase currPhase;
	private SkinnedMeshRenderer skinnedR;

	private Material defMat;


	// Use this for initialization
	void Start()
	{
		currPhase = Phase.Spawn;
		randomSeed = Random.value * 200;
		skinnedR = GetComponentInChildren<SkinnedMeshRenderer>();
		defMat = skinnedR.sharedMaterial;
		anim = GetComponent<Animator>();
		anim.SetBool("IsPausing", false);
		maxSpeed = Random.Range(1.2f, 2.3f) * transform.localScale.z;
		anim.speed = .2f + maxSpeed / (2.3f * transform.localScale.z);
		previousPos = transform.position;

		timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * anim.speed + .2f;
		skinnedR.enabled = false;
	}


    // Update is called once per frame
    void Update()
	{
		//gameObject.SetActive (false);
		if (Input.GetKeyDown(KeyCode.F))
		{
			Kill();
		}
		switch (currPhase)
		{
			case Phase.Spawn:
				velocity = Vector3.zero;
				UpdatePause();
				break;
			case Phase.Pause:
				UpdatePause();
				velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 10f);

				break;
			case Phase.Wander:
				UpdatePause();
				velocity += Wander() * Time.deltaTime;
				velocity += steerVec;
				//velocity += SteerInwards (EnemyManager.Instance.MinBorder, EnemyManager.Instance.MaxBorder,
				//  EnemyManager.Instance.Center, 0.6f) * Time.deltaTime * 2f;
				//velocity += SteerInBounds (EnemyManager.Instance.MinBorder, EnemyManager.Instance.MaxBorder, 
				//  EnemyManager.Instance.Center) * Time.deltaTime * 5f;
				if (velocity != Vector3.zero)
				{
					transform.forward = velocity.normalized;
				}
				break;
			case Phase.Death:
				//velocity = Vector3.Lerp(velocity, (transform.position - EnemyManager.Instance.PlayerPos) * 10f, Time.deltaTime * 10f);// - transform.position
				break;
		}

		velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
		nextPos = transform.position + velocity * 4f;

		transform.Translate(velocity * Time.deltaTime, Space.World);
	}

	void UpdatePause()
	{
		timer -= Time.deltaTime;
		if (timer < 0f)
		{
			if (currPhase == Phase.Wander)
			{
				currPhase = Phase.Pause;
                timer = Random.Range(pauseTimer.x, pauseTimer.y);
				anim.SetBool("IsPausing", true);
			}
			else
			{
				currPhase = Phase.Wander;
                timer = Random.Range(walkTimer.x, walkTimer.y);
				anim.SetBool("IsPausing", false);
			}
		}
	}

	private void RandomizePosition()
	{
		Vector3 pos = Vector3.one;
		pos.x = Random.Range(-.3f, .3f);
		pos.z = Random.Range(-.3f, .3f);
		pos.y = 0;
		transform.localPosition = pos;
	}

	public void Kill()
	{
		StartCoroutine(StartDisappearing());
		currPhase = Phase.Death;
		maxSpeed *= 10f;
		anim.SetBool("IsDead", true);
		skinnedR.enabled = true;

	}

	public void Respawn()
	{
		StopAllCoroutines();
		RandomizePosition();
		anim.SetBool("IsDead", false);
		skinnedR.enabled = true;
		if (currPhase == Phase.Death)
		{
			skinnedR.sharedMaterial = defMat;
		}
		timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * anim.speed + .2f;

		currPhase = Phase.Spawn;
		maxSpeed = Random.Range(1.2f, 2.3f) * transform.localScale.z;
	}

	private Vector3 Wander()
	{
		Vector3 wanderCenter = transform.position + transform.forward * 4f;
		float wanderRadius = 3f;
		float angle = Mathf.PerlinNoise(randomSeed + Time.fixedTime, randomSeed) * Mathf.PI * wanderRadius;

		seekPos.x = wanderCenter.x + wanderRadius * Mathf.Cos(angle);
		seekPos.y = transform.position.y;
		seekPos.z = wanderCenter.z + wanderRadius * Mathf.Sin(angle);

		//Debug.DrawLine (seekPos, transform.position);
		return Seek(seekPos);
	}

	private Vector3 Seek(Vector3 targetPos)
	{
		return ((targetPos - transform.position).normalized * maxSpeed - velocity);
	}

	/*
    private Vector3 SteerInBounds(Vector2 min, Vector2 max, Vector3 center)
    {
        if (nextPos.x > max.x || nextPos.x < min.x
            || nextPos.z > max.y || nextPos.z < min.y)
            return Seek(center);
        else return Vector3.zero;
    }
    private Vector3 SteerInwards(Vector2 min, Vector2 max, Vector3 center, float borderRatio)
    {
        //Debug.DrawLine (new Vector3 (min.x, center.y, min.y), new Vector3 (max.x, center.y, max.y));
        Vector2 innerMax, innerMin;
        innerMax = innerMin = (max - min) / 2f * borderRatio;
        innerMax.x += center.x;
        innerMax.y += center.z;
        innerMin.x = center.x - innerMin.x;
        innerMin.y = center.z - innerMin.y;
        if (nextPos.x > innerMax.x || nextPos.x < innerMin.x
            || nextPos.z > innerMax.y || nextPos.z < innerMin.y) {
            float factor = 1f;
            if (nextPos.x > innerMax.x) {
                factor *= (nextPos.x - innerMax.x) / (max.x - innerMax.x);
            } else if (nextPos.x < innerMin.x) {
                factor *= (innerMin.x - nextPos.x) / (innerMin.x - min.x);
            }
            if (nextPos.z > innerMax.y) {
                factor *= (nextPos.z - innerMax.y) / (max.y - innerMax.y);
            } else if (nextPos.z < innerMin.y) {
                factor *= (innerMin.y - nextPos.z) / (innerMin.y - min.y);
            }
            //Debug.Log (factor);
            return Seek (center) * factor;
        }
        else return Vector3.zero;
    }
    */
	//private Vector3 AvoidOthers(

	IEnumerator StartDisappearing()
	{
		//skinnedR.material = EnemyManager.Instance.transparentMat;

		for (;;)
		{
			Color c = skinnedR.material.color;
			c.a *= 0.91f;
			skinnedR.material.color = c;
			if (c.a < 0.01f)
			{
				skinnedR.enabled = false;
				yield return null;
			}
			else yield return new WaitForSeconds(0.01f);
		}
	}

	void OnTriggerStay(Collider c)
	{
		if (c.tag == "Boundary")
		{
			steerVec = Vector3.Lerp(steerVec, Seek(transform.parent.TransformPoint(Vector3.zero)) * Time.deltaTime * 5f, Time.deltaTime * 10f);
		}
	}
	void OnTriggerExit(Collider c)
	{
		if (c.tag == "Boundary")
		{
			steerVec = Vector3.zero;
		}
	}
}