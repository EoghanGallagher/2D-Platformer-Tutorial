using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ RequireComponent ( typeof ( BoxCollider2D ) ) ]
public class Controller2D : MonoBehaviour 
{
	public LayerMask collisionMask;

	float maxClimbAngle = 80.0f;
	const float skinWidth = .015f;
	BoxCollider2D collider;
	// Use this for initialization
	RayCastOrigins rayCastOrigins;

	public CollisionInfo collisionsInfo;

	public int horizRayCount = 4;
	public int vertRayCount = 4;

	float horizRaySpacing, verticalRaySpacing;
	void Start ()
	{
		collider = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();
	}


	struct RayCastOrigins
	{
		public Vector2 topLeft,
			topRight,
			bottomLeft,
			bottomRight;
	}

	public struct CollisionInfo
	{
		public bool above, below, left, right;
		public bool climbingSlope;

		public float slopeAngle ,slopeAngleOld;

		public void Reset()
		{
			above = below = left = right = false;
			climbingSlope = false;
			
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

	void UpdateRaycastOrigins()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand( skinWidth * -2 );

		rayCastOrigins.bottomLeft = new Vector2( bounds.min.x, bounds.min.y );
		rayCastOrigins.bottomRight = new Vector2( bounds.max.x, bounds.min.y );
		rayCastOrigins.topLeft = new Vector2( bounds.min.x, bounds.max.y );
		rayCastOrigins.topRight = new Vector2( bounds.max.x, bounds.max.y );
	}

	void CalculateRaySpacing()
	{
	
		Bounds bounds = collider.bounds;
		bounds.Expand( skinWidth * -2 );

		horizRayCount = Mathf.Clamp( horizRayCount, 2, int.MaxValue );
		vertRayCount = Mathf.Clamp( vertRayCount, 2, int.MaxValue );

		horizRaySpacing = bounds.size.y / ( horizRayCount - 1 );
		verticalRaySpacing = bounds.size.x / ( vertRayCount - 1 );
	}

	void HorizontalCollisions( ref Vector3 velocity )
	{
		float directionX = Mathf.Sign( velocity.x );
		float rayLength = Mathf.Abs( velocity.x ) + skinWidth;
		
		for( int i = 0; i < horizRayCount; i++ )
		{
			Vector2 rayOrigin = ( directionX == -1 ) ? rayCastOrigins.bottomLeft : rayCastOrigins.bottomRight;
			rayOrigin += Vector2.up * ( horizRaySpacing  * i  );

			RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.right * directionX, rayLength, collisionMask );

			if( hit )
			{
				
				//Get slope angle for slope movement....
				
				float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );
				
				if( i == 0 && slopeAngle <= maxClimbAngle )
				{
					float distanceToSlopeStart = 0;
					
					if( slopeAngle != collisionsInfo.slopeAngleOld )
					{
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope( ref velocity, slopeAngle  );
					velocity.x += distanceToSlopeStart * directionX;
					
				}

				if( !collisionsInfo.climbingSlope || slopeAngle > maxClimbAngle )
				{						
					velocity.x = ( hit.distance - skinWidth ) * directionX;
					rayLength = hit.distance;

					if( collisionsInfo.climbingSlope )
					{
						velocity.y = Mathf.Tan( collisionsInfo.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x );
					}
					collisionsInfo.left = directionX == -1;
					collisionsInfo.right = directionX == 1;
				}
			}
			Debug.DrawRay( rayOrigin, Vector2.right *  directionX * rayLength, Color.red );
		}
	}
	
	void VerticalCollisions( ref Vector3 velocity )
	{
		float directionY = Mathf.Sign( velocity.y );
		float rayLength = Mathf.Abs( velocity.y ) + skinWidth;
		
		for( int i = 0; i < vertRayCount; i++ )
		{
			Vector2 rayOrigin = ( directionY == -1 ) ? rayCastOrigins.bottomLeft : rayCastOrigins.topLeft;
			rayOrigin += Vector2.right * ( verticalRaySpacing * i + velocity.x );

			RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.up * directionY, rayLength, collisionMask );

			if( hit )
			{
				velocity.y = ( hit.distance - skinWidth ) * directionY;
				rayLength = hit.distance;

				if( collisionsInfo.climbingSlope )
				{
					velocity.x = velocity.y / Mathf.Tan( collisionsInfo.slopeAngle * Mathf.Deg2Rad ) * Mathf.Sign( velocity.x );
				}

				collisionsInfo.below = directionY == -1;
				collisionsInfo.above = directionY == 1;
			}

			//Debug.DrawRay( rayCastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red );
			Debug.DrawRay( rayOrigin, Vector2.up * directionY * rayLength, Color.red );
		}
	}

	public void Move( Vector3 velocity )
	{
		collisionsInfo.Reset();
		
		UpdateRaycastOrigins();
		
		if( velocity.x != 0 )
			HorizontalCollisions( ref velocity );
		
		if( velocity.y != 0 )
			VerticalCollisions( ref velocity );
		
		transform.Translate( velocity );
	}



	void ClimbSlope( ref Vector3 velocity , float slopeAngle )
	{
		float moveDistance = Mathf.Abs( velocity.x );
		float climbVeclocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;

		if( velocity.y <= climbVeclocityY )
		{
			velocity.y = climbVeclocityY;
			velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
			
			collisionsInfo.below = true;
			collisionsInfo.climbingSlope = true;
			collisionsInfo.slopeAngle = slopeAngle;

		}

	}
	

}
