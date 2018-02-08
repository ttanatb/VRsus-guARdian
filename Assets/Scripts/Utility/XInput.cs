using UnityEngine;

public class XInput : Singleton<XInput>
{
    public InputResult CheckTap(LayerMask layer, params TouchPhase[] touchPhases)
    {
        InputResult info = new InputResult();
        Vector3 position = Vector3.zero;
        bool validInput = false;

#if UNITY_IOS
        if (Input.touchCount < 1) 
        {
            info.result = ResultType.NoTap;
            return info;
        }

        foreach (Touch t in Input.touches) 
        {
            foreach(TouchPhase phase in touchPhases) 
            {
                if (t.phase == phase)
                {
                    position = t.position;
                    validInput = true;
                    break;
                }
            }

            if (validInput) break;
        }
#else
        foreach (TouchPhase phase in touchPhases)
        {
            if ((phase == TouchPhase.Began    && Input.GetMouseButtonDown(0)) ||
                (phase == TouchPhase.Ended    && Input.GetMouseButtonUp(0))   ||
                ((phase == TouchPhase.Moved || phase == TouchPhase.Stationary) && Input.GetMouseButton(0)))
            {
                validInput = true;
                break;
            }
        }

        position = Input.mousePosition;
#endif

        if (!validInput)
        {
            info.result = ResultType.NoTap;
            return info;
        }

        if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out info.hit, float.MaxValue, layer))
            info.result = ResultType.Success;
        else info.result = ResultType.MissTap;

        return info;
    }
}

public struct InputResult
{
    public ResultType result;
    public RaycastHit hit;
}

public enum ResultType
{
    NoTap = 0,
    MissTap = 1,
    Success = 2,
}