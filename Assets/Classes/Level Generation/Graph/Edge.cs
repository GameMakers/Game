using UnityEngine;
using System.Collections;

public class Edge {

	private float xDistance;
	private float yDistance;
	private int index;
	
	public Edge(float x, float y, int index){
		xDistance = x;
		yDistance = y;
		this.index = index;
	}
	
	
	public float getXDist(){
		return xDistance;
	}
	
	public float getYDist(){
		return yDistance;
	}
	
	public int getIndex(){
		return index;
	}
	
}
