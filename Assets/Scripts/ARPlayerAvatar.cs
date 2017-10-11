using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARPlayerAvatar : MonoBehaviour
{
    enum StepPhase
    {
        Stationary,
        Lift,
        Step
    }

    public Transform rightHandObj;
    public Transform leftHandObj;
    public Transform lookObj;
    public Transform leftFootObj;
    public Transform rightFootObj;
    public Transform leftShoulder;
    public Transform rightShoulder;

    private Animator animator;

    private float lerpConst = 20f;
    private float dist = .35f;
    private float yPos;

    private bool isLeft = true;
    private float floorYPos;

    [SerializeField]
    private StepPhase currFootPhase = StepPhase.Stationary;
    private Vector3 footLerpPos;
    private Vector3 stepDir;

    public float footSeperation = 1.27f;
    public float centerOfBalanceThreshold = 0.01f;
    public float maxStepDist = 0.01f;
    public float stepHeight = 1f;
    public float stepSpeed = 2f;

    public float FloorYPos
    {
        get { return floorYPos; }
        set
        {
            floorYPos = value;
            yPos = .809f + value;

            //Debug.Log(floorYPos + " " + yPos);
        }
    }

    // Use this for initialization
    private void Start()
    {
        animator = GetComponent<Animator>();
        yPos = transform.position.y;
        floorYPos = 1.5f;
    }

    private void Update()
    {
        floorYPos = LocalObjectBuilder.Instance.FloorPos;

        Vector3 floorVec = transform.position;
        floorVec.y = floorYPos;
        Debug.DrawLine(transform.position, floorVec);
        //Finds the vector from the character to the controller, but parallel to floor
        Vector3 projectedForward = Vector3.ProjectOnPlane(lookObj.forward, Vector3.up).normalized;

        //updates forward
        transform.forward = Vector3.Lerp(transform.forward, projectedForward, Time.deltaTime * lerpConst);

        //updates position
        Vector3 desiredPos = lookObj.position - projectedForward * dist;
        desiredPos.y = yPos;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * lerpConst);

        //updates foot position
        UpdateFoot(leftFootObj, rightFootObj, transform);

        //Vector3 pos = head.position;
        //pos.y -= characterHeight / 2 - offset;
        //transform.position = pos;

        //used to widen feet a bit more
        
        //Vector3 vecBetweenFeet = (leftFootObj.position - rightFootObj.position).normalized / 2f;
        //float currFootSeperation = footSeperation * transform.localScale.x;

        //bend hip if head's y pos is lowered
        //if (characterHeight > head.localPosition.y)
        //{
        //    forward = Quaternion.Euler((MAX_transform_ROT - MIN_transform_ROT) * (characterHeight - head.localPosition.y) / (characterHeight),
        //        0, 0) * forward;
        //}

        Vector3 footOrientation = transform.forward;
        footOrientation.y = 0;

        leftFootObj.forward = Vector3.Lerp(leftFootObj.forward, footOrientation, Time.deltaTime * lerpConst);
        rightFootObj.forward = Vector3.Lerp(leftFootObj.forward, footOrientation, Time.deltaTime * lerpConst);
    }


    void UpdateFoot(Transform left, Transform right, Transform center)
    {
        //if (isLeft)
        //    Debug.Log("Current foot: Left, Current phase: " + currFootPhase);
        //else Debug.Log("Current foot: Right, Current phase: " + currFootPhase);

        switch (currFootPhase)
        {
            case StepPhase.Stationary:
                //Debug.Log(Mathf.Abs(left.position.y - floorYPos) + " " + Mathf.Abs(right.position.y - floorYPos));

                if (CheckCross(left, right, center) ||
                    Mathf.Abs(left.position.y - floorYPos) > 0.05f ||
                    Mathf.Abs(right.position.y - floorYPos) > 0.05f)//, 0.00013f))
                {
                    if (isLeft)
                    {
                        footLerpPos = GetFootToShoulderPos(leftShoulder);
                        stepDir = (footLerpPos + Vector3.up * stepHeight) - left.position;
                    }
                    else
                    {
                        footLerpPos = GetFootToShoulderPos(rightShoulder);
                        stepDir = (footLerpPos + Vector3.up * stepHeight) - right.position;
                    }


                    currFootPhase = StepPhase.Lift;
                }
                else if (CheckCenterOfBalance(centerOfBalanceThreshold, left, right, center))
                {
                    Step(left, right, center, maxStepDist);
                    currFootPhase = StepPhase.Lift;

                    if (isLeft)
                    {
                        stepDir = (footLerpPos + Vector3.up * stepHeight) - left.position;
                    }
                    else
                    {
                        stepDir = (footLerpPos + Vector3.up * stepHeight) - right.position;
                    }
                }
                break;
            case StepPhase.Lift:
                if (isLeft)
                {
                    left.position = Vector3.Lerp(left.position, left.position + stepDir, Time.deltaTime * stepSpeed);
                    if (left.position.y - floorYPos > stepHeight)
                    {
                        stepDir = footLerpPos - left.position;
                        currFootPhase = StepPhase.Step;
                    }
                }
                else
                {
                    right.position = Vector3.Lerp(right.position, footLerpPos + Vector3.up, Time.deltaTime * stepSpeed);
                    if (right.position.y - floorYPos > stepHeight)
                    {
                        stepDir = footLerpPos - right.position;
                        currFootPhase = StepPhase.Step;
                    }

                }

                Debug.DrawLine(left.position, footLerpPos);
                Debug.DrawLine(right.position, footLerpPos);

                break;
            case StepPhase.Step:
                if (isLeft)
                {
                    left.position = Vector3.Lerp(left.position, left.position + stepDir, Time.deltaTime * stepSpeed);
                    if (left.localPosition.y - floorYPos < 0.03f)
                    {
                        isLeft = !isLeft;
                        currFootPhase = StepPhase.Stationary;
                        if (left.localPosition.y > floorYPos)
                            left.Translate(0, left.localPosition.y - floorYPos, 0, Space.Self);
                        else
                            left.Translate(0, floorYPos - left.localPosition.y, 0, Space.Self);
                    }

                    Debug.DrawLine(left.position, footLerpPos);

                }
                else
                {
                    right.position = Vector3.Lerp(right.position, right.position + stepDir, Time.deltaTime * stepSpeed);
                    if (right.localPosition.y - floorYPos < 0.03f)
                    {
                        isLeft = !isLeft;
                        currFootPhase = StepPhase.Stationary;
                        if (right.localPosition.y > floorYPos)
                            right.Translate(0, right.localPosition.y - floorYPos, 0, Space.Self);
                        else
                            right.Translate(0, floorYPos - right.localPosition.y, 0, Space.Self);
                    }

                    Debug.DrawLine(right.position, footLerpPos);
                }
                break;
        }
    }

    private bool CheckCross(Transform left, Transform right, Transform center)
    {
        if (Vector3.Dot(right.position - left.position, transform.right) < 0)// || (right.position - left.position).sqrMagnitude < threshold)
        {
            return true;
        }
        return false;
    }

    private bool CheckCenterOfBalance(float threshold, Transform left, Transform right, Transform center)
    {
        Vector3 currCen = (left.position + right.position) / 2f;
        currCen.y = center.position.y;

        if ((currCen - center.position).sqrMagnitude > Mathf.Pow(threshold, 2))
            return true;
        else return false;
    }

    private void Step(Transform left, Transform right, Transform center, float maxStepDelta)
    {
        Vector3 footCenter = (left.position + right.position) / 2f;
        Vector3 vecToCenterOfBalance = center.position - footCenter;
        vecToCenterOfBalance.y = 0;

        if (Mathf.Abs(Vector3.Dot(vecToCenterOfBalance, transform.right)) < Mathf.Abs(Vector3.Dot(vecToCenterOfBalance, transform.forward)))
        {
            Vector3 vecBetweenFeet;
            if ((center.position - left.position).sqrMagnitude < (center.position - right.position).sqrMagnitude)
            {
                isLeft = false;
                vecBetweenFeet = center.position - left.position;
            }
            else
            {
                isLeft = true;
                vecBetweenFeet = center.position - right.position;
            }


            vecBetweenFeet.y = 0;
            footLerpPos = center.position + vecBetweenFeet;
            footLerpPos.y = floorYPos;
        }

        else
        {
            if ((GetFootToShoulderPos(leftShoulder) - left.position).sqrMagnitude > (GetFootToShoulderPos(rightShoulder) - right.position).sqrMagnitude)
            {
                isLeft = true;
                footLerpPos = GetFootToShoulderPos(leftShoulder);
            }
            else
            {
                isLeft = false;
                footLerpPos = GetFootToShoulderPos(rightShoulder);
            }
        }
    }

    //puts the foot underneath the shoulder
    Vector3 GetFootToShoulderPos(Transform shoulder)
    {
        //Debug.DrawLine(foot.position, shoulder.position);
        Vector3 pos = shoulder.position;
        pos.y = floorYPos;
        return pos;
    }

    // Update is called once per frame
    void OnAnimatorIK()
    {
        if (animator)
        {
            if (lookObj)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(lookObj.position);
            }
            else animator.SetLookAtWeight(0);

            if (leftHandObj)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }

            if (rightHandObj)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            }

            if (leftFootObj)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootObj.position);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootObj.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            }

            if (rightFootObj)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootObj.position);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootObj.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }
    }
}

/*

//fields
private const float MIN_transform_ROT = 0f;
private const float MAX_transform_ROT = 30f;

private const float MIN_STEP_THRESH_SQR = 0.1f;
//component
private Animator animator;

//targets for IK
public Transform rightHandTarget = null;
public Transform leftHandTarget = null;
public Transform lookTarget = null;
public Transform rightFootTarget = null;
public Transform leftFootTarget = null;
public PlaceTargetParent targets;

//head target
public Transform head;
private Vector3 prevHeadPos;

//transforms from the player (and bones)
public Transform player;


public Transform hip;

private StepPhase rightFootPhase = StepPhase.Stationary;

private Vector3 rFootLerpPos;


public float desiredHeight = 0.07f;

//height of the character

[SerializeField]
private float characterHeight = .15f;

//how separated the feet are
public float footSeperation = 1.27f;

private float offset = 0;

private SkinnedMeshRenderer[] skinnedRenderers;
private MeshRenderer[] meshRenderers;

public Material defaultMat;
public Material animatingMat;

void Start()
{
    animator = GetComponent<Animator>();
    AdjustModelToHeight(desiredHeight);
    characterHeight = GetModelHeight();
    Vector3 headPos = head.localPosition;
    headPos.y = characterHeight - .2f;
    head.localPosition = headPos;
    transform.position = targets.transform.position;
    offset = (transform.position.y - targets.transform.position.y + characterHeight / 2) * transform.localScale.y;

    skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    meshRenderers = GetComponentsInChildren<MeshRenderer>();

    //		foreach (Renderer r in skinnedRenderers) {
    //			Debug.Log (r.gameObject);
    //		}
    //		foreach (Renderer r in meshRenderers) {
    //			Debug.Log (r.gameObject);
    //		}
}


// Update is called once per frame
void Update()
{
    

private float GetModelHeight()
{
    float height = 0;
    int index = 0;
    SkinnedMeshRenderer[] r = GetComponentsInChildren<SkinnedMeshRenderer>();
    for (int i = 0; i < r.Length; i++)
    {
        if (r[i].bounds.size.y > height)
        {
            height = r[i].bounds.size.y;
            index = i;
        }
    }

    for (int i = 0; i < r.Length; i++)
    {
        if (i != index)
        {
            if (r[i].bounds.min.y < r[index].bounds.max.y)
            {
                height += r[i].bounds.size.y;
                height -= r[index].bounds.max.y - r[i].bounds.min.y;
            }
            else if (r[i].bounds.max.x > r[index].bounds.min.y)
            {
                height += r[i].bounds.size.y;
                height -= r[i].bounds.max.x - r[index].bounds.min.y;
            }
        }
    }

    return height - .01f;
}

private void AdjustModelToHeight(float desiredHeight)
{
    transform.localScale *= desiredHeight / GetModelHeight();
}

float currAlpha = 0f;
public void HideCharacter()
{
    StopAllCoroutines();
    currAlpha = 1f;
    foreach (SkinnedMeshRenderer r in skinnedRenderers)
    {
        r.sharedMaterial = animatingMat;
    }
    foreach (MeshRenderer r in meshRenderers)
    {
        r.sharedMaterial = animatingMat;
    }
    StartCoroutine(FadeOut());
}

IEnumerator FadeIn()
{
    for (; ; )
    {
        currAlpha = Mathf.Lerp(currAlpha, 1f, Time.deltaTime * 5f);
        Color c = meshRenderers[0].sharedMaterial.color;
        c.a = currAlpha;
        meshRenderers[0].sharedMaterial.color = c;
        if (currAlpha > 0.995f)
        {
            foreach (SkinnedMeshRenderer r in skinnedRenderers)
            {
                r.sharedMaterial = defaultMat;
            }
            foreach (MeshRenderer r in meshRenderers)
            {
                r.sharedMaterial = defaultMat;
            }
            break;
        }
        else
        {
            //Debug.Log (currAlpha);

            yield return new WaitForSeconds(.01f);
        }

    }
    yield return null;
}

IEnumerator FadeOut()
{
    //Debug.Log("Hiding character");

    for (; ; )
    {
        currAlpha = Mathf.Lerp(currAlpha, 0f, Time.deltaTime * 5f);
        Color c = meshRenderers[0].sharedMaterial.color;
        c.a = currAlpha;
        meshRenderers[0].sharedMaterial.color = c;
        if (currAlpha < 0.005f)
        {
            break;
        }
        else
        {

            yield return new WaitForSeconds(.01f);
        }
    }
    yield return null;
}

public void ShowCharacter()
{
    StopAllCoroutines();
    //StopCoroutine ("FadeIn");
    //StopCoroutine ("FadeOut");
    currAlpha = 0f;

    StartCoroutine(FadeIn());

}
*/
