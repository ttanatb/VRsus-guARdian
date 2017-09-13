using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
	private static T instance;

	public static T Instance
	{
		get
		{
			return instance;
		}
	}

	protected virtual void Awake()
	{
		if (!instance)
		{
			instance = (T)this;
		}

		else
		{
			Debug.LogError("Singleton already created");
		}
	}

	protected virtual void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}
}
