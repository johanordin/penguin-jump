using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class TuxController : MonoBehaviour
{

    //Public variables
    public float jumpPower;
    public float moveSpeed;

    //Internal movement variables
    protected bool jumping;
    protected bool alive;
    protected bool hasFish;

    //Internal markers variables
    protected Transform startCS;
    protected Transform endCS;
    protected Transform obstacleCS;
    protected bool towardsEnd;
    protected bool obstacleTracking;
    protected bool startTracking;
    protected bool endTracking;

    //Fish
    public GameObject static_fish;
    public GameObject fish;

    //Animation
    private Mesh[] walk, flying, dying, dancing, jump;
    public float walkDuration, flyingDuration, dyingDuration, dancingDuration, jumpingDuration;
	private float ixW, ixF, ixD, ixDnc, ixJump;

    public float maxJumpHeight;
    protected float height;
    protected bool shouldRespawn;

    // constant
    protected const float startRadiusCheck = 5.0f;
    protected const float endRadiusCheck = 150.0f;
    protected const float obstacleRadiusCheck = 120.0f;
    protected const float theta = 90.0f;


    //Start is called on the frame when a script is enabled
    //just before any of the Update methods is called the first time.
    protected void Start()
    {
        startTracking = false;
        endTracking = false;
        obstacleTracking = false;

        // load animation
        walk = Resources.LoadAll<Mesh>("tux/tux_walk");
        flying = Resources.LoadAll<Mesh>("tux/tux_fall");
        dying = Resources.LoadAll<Mesh>("tux/tux_die");
        dancing = Resources.LoadAll<Mesh>("tux/tux_dance");
		jump = Resources.LoadAll<Mesh>("tux/tux_jump");

        // restart state
        hasFish = false;
        RestartState();
    }

    protected void RestartState()
    {
        alive = true;
        towardsEnd = true;
        jumping = false;
        height = 0.0f;
        ixW = ixF = ixD = ixDnc = 0.0f;

        if (hasFish) DropFish();
    }

    protected void MoveToTarget()
    {
        float epsilon;
        Transform target, other;
        if (towardsEnd)
        {
            target = endCS;
            other = startCS;
            epsilon = endRadiusCheck;
        }
        else
        {
            target = startCS;
            other = endCS;
            epsilon = startRadiusCheck;
        }

        // orient to target
        transform.LookAt(target,startCS.transform.up);

        // move
        Vector3 currPos = transform.position;
        transform.position += (target.position - currPos).normalized * moveSpeed * Time.deltaTime;

        // check if target has been reached
        if ((transform.position - target.position).magnitude < epsilon)
        { 
            TargetReached();
            transform.LookAt(other, startCS.transform.up); // avoid fish flickering
        }
    }

    protected void TargetReached()
    {
        if (towardsEnd) PickUpFish();
        else DropFish();
        towardsEnd = !towardsEnd;
    }

    protected float NextAminFrame(ref Mesh mesh, ref Mesh[] anim, ref float idx, float duration)
    {
        // add the time to the index
        idx += anim.Length * (Time.deltaTime * 1000.0f / duration);

        // get index (as integer)
        int current_idx = (int)idx;

        // check if animation finished
        if (current_idx > anim.Length - 1)
        {
            // set actual index to 0.0f
            idx = 0.0f;

            // get animation last frame
            mesh = anim[anim.Length - 1];

            // return 1.0f (animation ended)
            return 1.0f;
        }
        else {

            // get animation frame
            mesh = anim[current_idx];

            // return percentage of animation
            return idx / anim.Length;
        }
    }

    protected void NextWalkAnimFrame(ref Mesh mesh)
    {
        NextAminFrame(ref mesh, ref walk, ref ixW, walkDuration);
    }

    protected void NextDanceAnimFrame(ref Mesh mesh)
    {
        NextAminFrame(ref mesh, ref dancing, ref ixDnc, dancingDuration);
        transform.Rotate(transform.up * theta * Time.deltaTime);
    }

	protected void NextJumpAnimFrame(ref Mesh mesh)
	{
        float perc = NextAminFrame(ref mesh, ref jump, ref ixJump, jumpingDuration);

        // check if animation finished
        if (perc >= 1.0f - 1e-5)
        { 
            jumping = false;
            height = 0.0f;
        }

        // get the current height
        else
        {
            // interpolate height using a sine
            height = maxJumpHeight * Mathf.Sin(perc * Mathf.PI);
        }

    }

    protected void NextDieAnimFrame(ref Mesh mesh)
    {
        float perc = NextAminFrame(ref mesh, ref dying, ref ixD, dyingDuration);

        // check if animation finished
        if (perc >= 1.0f - 1e-5) shouldRespawn = true;
    }

    //Update is called every frame.
    protected void Update()
    {
        Mesh mesh = walk[0];

        // little hack to handle jumps (get to actual position)
        transform.position -= new Vector3(0, height, 0);

        //UnityEngine.Debug.Log(obstacleTracking);

        if (obstacleTracking && alive) CheckObstacle();

        if (alive)
        {
            // if alive, move

            if (startTracking && (endTracking || !towardsEnd))
            {
                MoveToTarget();
                if (jumping) NextJumpAnimFrame(ref mesh);
                else NextWalkAnimFrame(ref mesh);
            }
            else if (startTracking && towardsEnd)
            {
                if (jumping) NextJumpAnimFrame(ref mesh);
                else NextDanceAnimFrame(ref mesh);
            }

        }
        else
        {
            // if dead, animate death and check for respawn
            NextDieAnimFrame(ref mesh);
        }

        gameObject.GetComponentInChildren<MeshFilter>().mesh = mesh;

        // little hack to handle jumps (get to the air)
        transform.position += new Vector3(0, height, 0);

        // respawn if needed
        if (!alive && shouldRespawn) Respawn();
    }

    public void StartTrackingFound(Transform pos)
    {
        startTracking = true;
        startCS = pos;
        transform.position = startCS.position;
        transform.rotation = startCS.rotation;

        // restart state
        RestartState();

        // show tux
        gameObject.GetComponentInChildren<Renderer>().enabled = true;
    }

    public void StartTrackingLost()
    {
        startTracking = false;

        // hide tux
        gameObject.GetComponentInChildren<Renderer>().enabled = false;

        // hide fish
        fish.GetComponentInChildren<Renderer>().enabled = false;

        // show static_fish if marker available
        if (endTracking) static_fish.GetComponentInChildren<Renderer>().enabled = true;

    }

    public void StartUpdate(Transform pos)
    {
        startCS = pos;
    }

    public void EndTrackingFound(Transform pos)
    {
        endTracking = true;
        endCS = pos;
        if (!hasFish)
        {
            towardsEnd = true;
            static_fish.GetComponentInChildren<Renderer>().enabled = true;
        }
    }

    public void EndTrackingLost()
    {
        endTracking = false;
        towardsEnd = false;
        static_fish.GetComponentInChildren<Renderer>().enabled = false;
    }

    public void EndUpdate(Transform pos)
    {
        endCS = pos;
    }

    protected void PickUpFish()
    {
        hasFish = true;
        fish.GetComponentInChildren<Renderer>().enabled = true;
        static_fish.GetComponentInChildren<Renderer>().enabled = false;
    }

    protected void DropFish()
    {
        hasFish = false;
        fish.GetComponentInChildren<Renderer>().enabled = false;
        if (endTracking) static_fish.GetComponentInChildren<Renderer>().enabled = true;
    }

	public void MakeAnimJump()
	{
		jumping = true;
	}

	public void ObstacleFound(Transform pos)
	{
        obstacleTracking = true;
        obstacleCS = pos;
	}

	public void ObstacleLost()
	{
        obstacleTracking = false;
    }

	protected void CheckObstacle()
	{
        // check distance to obstacle
        float dist = (obstacleCS.position - transform.position).magnitude;

        //UnityEngine.Debug.Log(dist);

        if (!jumping && dist < obstacleRadiusCheck)
        {
            // die
            alive = false;
            DropFish();
            shouldRespawn = false;

		}
	}

    protected void Respawn()
    {
        RestartState();
        transform.position = startCS.position;
        transform.rotation = startCS.rotation;
    }

}
