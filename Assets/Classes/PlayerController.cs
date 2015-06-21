using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
    Character_mobility character_mobility;
	bool isGrounded;
	public bool isPunching;
	SpriteRenderer SR;
    private float topspeed;
    private float speedDifference;
    public Sprite Idle;
	public Sprite Punch1;
	public Sprite Punch2;
	public Sprite Punch3;
	Vector3 Direction;
    Rigidbody2D Rb2d;

	// Use this for initialization
	void Start () 
	{
        character_mobility = new Character_mobility();
        topspeed = character_mobility.get_top_speed();
        Rb2d = this.GetComponent<Rigidbody2D>();
		SR = this.GetComponent<SpriteRenderer> ();
		isGrounded = true;
		isPunching = false;
		SR.sprite = Idle;
		Direction = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () 
	{

        if (Input.GetKey (KeyCode.LeftArrow))
		{
            if (Rb2d.velocity.magnitude >= topspeed)
            {
                speedDifference = (topspeed - Rb2d.velocity.magnitude);
                this.GetComponent<Rigidbody2D>().AddForce(Vector2.right * speedDifference * Time.deltaTime);
            }
            else
            {
                this.GetComponent<Rigidbody2D>().AddForce(Vector2.right * -600 * Time.deltaTime);
            }

            SR.sprite = Idle;
            Direction = new Vector3(-1, 1, 1);
            transform.localScale = Direction;
		}

		if (Input.GetKey (KeyCode.RightArrow))
		{
            if (Rb2d.velocity.magnitude >= topspeed)
            {
                speedDifference = (topspeed - Rb2d.velocity.magnitude);
                this.GetComponent<Rigidbody2D>().AddForce(Vector2.right * (speedDifference*-1) * Time.deltaTime);
            }
            else
            {
			    this.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.right * 600 * Time.deltaTime);
			}

            SR.sprite = Idle;
            Direction = new Vector3(1, 1, 1);
            transform.localScale = Direction;
		}

		if (Input.GetKeyDown (KeyCode.Space) && isGrounded) 
		{
			this.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * 25000 * Time.deltaTime);
			isGrounded = false;
			SR.sprite = Punch1;
		}

		if (Input.GetKeyDown (KeyCode.LeftControl) && !isPunching) 
		{
			Debug.Log("Punching!");
			Debug.Log (SR.sprite.ToString());
			Sprite t_sprite = SR.sprite;
			Vector3 t_direction = Direction;
			isPunching = true;
			for(int i =0; i < 120; i++)
			{
				if(i > 0 && i <39)
				{
					SR.sprite = Punch1;
					transform.localScale = t_direction;
				}
				if(i>39 && i < 79)
				{
					SR.sprite = Punch2;
					transform.localScale = t_direction;
				}
				if(i>79 && i < 119)
					SR.sprite = Punch3;
				if(i >= 119)
					SR.sprite = t_sprite;
					transform.localScale = t_direction;
					isPunching = false;
			}
			Debug.Log ("Done Punching");
			Debug.Log (SR.sprite.ToString());
		}

	}



	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.tag == "ground") 
		{
			isGrounded = true;
			SR.sprite = Idle;
		}
	}
}
