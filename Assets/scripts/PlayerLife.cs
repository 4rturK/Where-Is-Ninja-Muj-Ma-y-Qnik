public class PlayerLife : LifeSystem
{
    private void Awake()
    {
        hp = 150;
    }

    void Update()
    {
        if (!isAlive())
        {
            Destroy(gameObject);
        }
    }
}

