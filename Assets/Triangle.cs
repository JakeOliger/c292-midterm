public class Triangle : Enemy
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        bounty = 10;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
}
