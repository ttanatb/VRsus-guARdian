using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }
    }

	public Singleton() 
	{
		if (instance == null)
		{
			instance = (T)this;
		}

		else
		{
			Debug.LogError("Singleton already created");
		}
	}
}