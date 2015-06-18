using System.Collections.Generic;

public class Vertex {

	//list of positions of vertices which are connected to this vertex by an edge
	private List<Edge> edges;
	
	public Vertex(){
		edges = new List<Edge>();
	}
	
	
	
	public void addEdge(Edge newEdge){
		edges.Add (newEdge);
	}
	
	public List<Edge> getEdges(){
		return edges;
	}
}
