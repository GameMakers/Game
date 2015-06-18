using System.Collections.Generic;

public class Graph {


	//ADD PARAMETERS AND A CHECK FOR PLAYER MOBILITY ON EDGE CROSSINGS

	private List<Vertex> vertices;
	
	public Graph(){
		vertices = new List<Vertex>();
	}
	
	
	
	public void addVertex(Vertex newVertex){
		vertices.Add (newVertex);
	}
	
	public List<Vertex> getVertices(){
		return vertices;
	}
	
	public void addEdge(int fromVertex, Edge newEdge){
		vertices[fromVertex].addEdge(newEdge);
	}
	
	//run a BFS to check if the goal can be reached from the start point
	public bool isValid(int startPos, int goal){
		List<int> usedVertices = new List<int>();
		Queue<int> pendingVertices = new Queue<int>();
		List<Edge> currentEdges;
		int tempIndex;
		
		usedVertices.Add(startPos);
		pendingVertices.Enqueue(startPos);
		
		//loop while vertices remain in the queue
		while (pendingVertices.Count > 0){
			//get the list of edges for the current vertex
			currentEdges = vertices[pendingVertices.Dequeue()].getEdges();
			//loop through the edges of the current vertex
			for (int a=0; a<currentEdges.Count; a++){
				//if the current player has the ability to traverse this edge
				//IMPLEMENT THIS
				if(true){
					//get the vertex which the current edge leads to
					tempIndex = currentEdges[a].getIndex();
					//if the other vertex has no yet been visited
					if (!usedVertices.Contains(tempIndex)){
						//if the goal is found then indicate that the graph is valid
						if(tempIndex == goal)
							return true;
						//add the other vertex to the list of visited vertices and to the queue
						usedVertices.Add(tempIndex);
						pendingVertices.Enqueue(tempIndex);
					}
				}
			}
		}
		
		//if the goal was not reached then indicate that the graph is not valid
		return false;
	}
}
