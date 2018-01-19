using UnityEngine;

/// <summary>
/// Base class for MonoBehaviour scripts that are Singletons
/// </summary>
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    //the instance of the object
	private static T instance;
	public static T Instance { get { return instance; } }

    /// <summary>
    /// Sets the reference of the instance
    /// </summary>
	protected virtual void Awake()
	{
		if (!instance)
			instance = (T)this;
		else Debug.LogError("Singleton already created");
	}

    /// <summary>
    /// Destroys reference to instance
    /// </summary>
	protected virtual void OnDestroy()
	{
		if (instance == this)
			instance = null;
	}
}
