using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	/*

		How to calculate Gravity given a Jump Height and time to reach jump height
		
		Known : jump height and time to jump apex

		Equation : deltaMovement = velocity * time + acceleration * ( (time)squared ) / 2
		
		So : jumpHeight = gravity * ( ( timeToJumpApex )squared ) / 2

		Solve for gravity

		Multiply each side by 2

		2 * JumpHeight = gravity * ( ( timeToJumpApex )squared )

		Divide both sides by gravity

		( 2 * JumpHeight ) / gravity = ( timeToJumpApex )squared

		Divide 1 by whole equation to get reciprical

		Result: gravity / ( 2 * JumpHeight )  = 1 / ( timeToJumpApex )squared

		Multiply boths side by ( 2 * jumpHeight ) 

		gravity = 2 * JumpHeight / ( timeToJumpApex )squared

	
	*/


	/*
		How to calculate jump velocity given gravity and time to jump apex

		acceleration = gravity

		finalVelocity = initialVelocity + ( acceleration * time )

		jumpVelocity = gravity * timeToJumpApex; 

	 */

[ RequireComponent ( typeof( Controller2D ) ) ]
public class Player : MonoBehaviour 
{

	public float jumpHeight = 4;
	public float timeToJumpApex = .4f;
	float gravity;

	float jumpVelocity;

	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	float moveSpeed = 6;
	float veclocityXSmoothing;

	Vector3 velocity;

	Controller2D controller;


	// Use this for initialization
	void Start () 
	{
		controller = GetComponent<Controller2D>();

		gravity = -( 2 * jumpHeight ) / Mathf.Pow( timeToJumpApex , 2 ); 
		jumpVelocity = Mathf.Abs( gravity ) * timeToJumpApex;

		print( "Gravity: " + gravity + " Jump velocity: " + jumpVelocity );
	}

	void Update()
	{
		if( controller.collisionsInfo.above || controller.collisionsInfo.below )
		{
			velocity.y = 0;
		}

		Vector2 input = new Vector3 ( Input.GetAxisRaw( "Horizontal" ), Input.GetAxisRaw( "Vertical" ) );

		if( Input.GetKeyDown( KeyCode.Space ) && controller.collisionsInfo.below )
		{
			velocity.y = jumpVelocity;
		}

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp( velocity.x , targetVelocityX , ref veclocityXSmoothing, ( controller.collisionsInfo.below ) ? accelerationTimeGrounded : accelerationTimeAirborne );
		velocity.y += gravity * Time.deltaTime;
		controller.Move( velocity * Time.deltaTime );
	}
	
	
}
