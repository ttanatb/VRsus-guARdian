/// <summary>
/// Singleton skeleton for scripts that do not inherit from MonoBehaviours
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : Singleton<T>
{
    //instance of the singleton
    private static T instance;
    public static T Instance { get { return instance; } }

    //constructor
	public Singleton() 
	{
		if (instance == null)
            instance = (T)this;
	}
}