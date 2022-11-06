/*******************************************************************************
 * AddNGonPolygonCollider.cs
 * 
 * When attached to a game object this script will add an N-sided hollow
 * PolygonCollider2D to the object.  Its primary use in Spacebomb is to generate
 * the collider for the edge of the world.
 ******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AddNGonPolygonCollider : MonoBehaviour 
{
    /***************************************************************************
     * CONSTANTS
     **************************************************************************/

    /// <summary>
    /// The distance between the inner and outer rings.  The collider should
    /// have some width in order to avoid fast objects moving through it.
    /// </summary>
    private const float width = 5f;

    /***************************************************************************
     * INSPECTOR VARIABLES
     **************************************************************************/
    public bool isTrigger = false;
    public bool usedByEffector = false;

    /// <summary>
    /// The number of sides for the generated polygon.
    /// </summary>
    public int numSides;

    /// <summary>
    /// How far from the center should the inner points be.
    /// </summary>
    public float radius;

    /// <summary>
    /// The physics material to attach to the collider.
    /// </summary>
    public PhysicsMaterial2D physicsMaterial2D;

    /***************************************************************************
     * START
     **************************************************************************/

	// Use this for initialization
	void Start () 
    {
	    if (numSides < 3) 
        {
            throw new UnityException("N-sided collider must have 3+ sides.");
        }

        PolygonCollider2D newCollider = 
                        this.gameObject.AddComponent<PolygonCollider2D> ();

        //set physics material of new collider
        newCollider.sharedMaterial = physicsMaterial2D;
        newCollider.isTrigger = isTrigger;
        newCollider.usedByEffector = usedByEffector;

        List<Vector2> allPoints = new List<Vector2> ((numSides+1)*2);

        //inner points
        allPoints.AddRange(getPolygonPoints (radius));

        //outer points - 2 rings makes the collider hollow
        allPoints.AddRange(getPolygonPoints (radius + width));

        //make the new collider use the points created
        newCollider.points = allPoints.ToArray();

	}

    /***************************************************************************
     * PRIVATE FUNCTIONS
     **************************************************************************/

    /// <summary>
    /// Gets the points that make up a polygon of radius r.
    /// </summary>
    /// <returns>Array of Vector2 points describing the polygon.</returns>
    /// <param name="r">The radius of the polygon..</param>
    private Vector2[] getPolygonPoints(float r)
    {
        //create array to hold points - the +1 allows the polygon to close
        Vector2 [] points = new Vector2[numSides + 1];
        
        for(int i=0; i<(numSides+1); i++)
        {
            //maths from stack overflow!
            float x = r * Mathf.Cos((2*Mathf.PI*i)/numSides);
            float y = r * Mathf.Sin((2*Mathf.PI*i)/numSides);
            
            points[i] = new Vector2(x,y);
        }

        return points;
    }
}