using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {
    //The reason why this is protected is because other classes are going to inherit from physics object  
    //and we want them to be able to access velocity but we don't want it to be accessible from outside of the class
    protected Vector2 velocity;
    protected Rigidbody2D rb2d;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];   
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected bool grounded;
    protected Vector2 groundNormal;
    protected Vector2 targetVelocity;
    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    public float minGroundNormalY = .65f;
    public float gravityModifier = 1f; //This will allow us to scale the gravity

    private void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }
    // Use this for initialization
    void Start () {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    // Because we are doing physics we want to have a fixed update function - this function is more consistant than the update function
    // Gravity is going to affect the object every frame by pushing it downward 
    private void FixedUpdate()
    {
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime; //Time.deltaTime = the time it took us to render the last frame
        velocity.x = targetVelocity.x;

        grounded = false;
        
        Vector2 deltaPosition = velocity * Time.deltaTime; //the change in position
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
        Vector2 move = moveAlongGround * deltaPosition.x;
        Movement(move, false);
        move = Vector2.up * deltaPosition.y;
        Movement(move, true);
    }
    

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius); //shell radius is padding to make sure we cannot pass inside of another collider.
            hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                if(currentNormal.y > minGroundNormalY)
                {
                    grounded = true;
                    if(yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }

        }
        rb2d.position = rb2d.position + move.normalized * distance;
    }


}
