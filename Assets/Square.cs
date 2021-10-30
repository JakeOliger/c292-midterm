public class Square : Enemy
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        bounty = 35;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
}
