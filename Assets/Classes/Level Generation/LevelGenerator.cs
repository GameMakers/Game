using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

	public Transform player;
	public Transform prefab;
	public Vector3 start_pos;
	private Vector3 next_pos;

	private readonly int TOTAL_PLATFORMS = 1000;
	private readonly int X_RECYLE_CUTOFF = 20, Y_RECYCLE_CUTOFF = 10;
	private readonly int TOTAL_INSTANCIATED_PLATFORMS = 100;
	private readonly float VERTICAL_BUFFER = 1;

	//REFERENCE THE PLAYER MOVEMENT SCRIPT FOR THESE VALUES
	//information about the player's mobility used to determine if one platform can be reached from another
	//private readonly float playerSpeed = 10;
	//private readonly float playerJump = 5;
	private int platform_horizontal_gap = 40;
	private int platform_upwards_gap = 40;
	private int platform_downwards_gap = 70;
	
	
	//stores the minimally necessary information on all platforms in the entire level
	private List<Platform> all_platforms;
	//each list stores the farthest point of all platforms for a particular extremity in sorted order - this is used to identify when they should become active
	private SortedList<float,int> right_extremities, left_extremities, upper_extremities, lower_extremities;
	//used to keep track of index locations in the extremity lists
	private int right_extremity_index, left_extremity_index, upper_extremity_index, lower_extremity_index;
	//used to keep track of the farthest active platform with respect to each border
	private float min_x_pos, max_x_pos, min_y_pos, max_y_pos;
	private int min_x_index, max_x_index, min_y_index, max_y_index;
	//used to keep track of the nearest inactive platform in each direction
	private float next_left_pos, next_right_pos, next_upper_pos, next_lower_pos;
	//used to keep track of replacement platforms when the borders are pulled by player movement
	private float next_left_pos_rep, next_right_pos_rep, next_upper_pos_rep, next_lower_pos_rep;
	//used to keep track of the cutoffs for the player's current position
	float right_extremity_cutoff, left_extremity_cutoff, upper_extremity_cutoff, lower_extremity_cutoff;
	
	//a directed graph with platforms as nodes and edges to represent reachable platforms
	private Graph platform_graph;
	
	private Queue<Transform> inactive_platforms;
	private List<Transform> active_platforms;
	
	
	//FIX GENERATION BUGS
	//VERIFY THAT GRAPH CAN BE REMOVED
	
	//ADD CODE TO MARK THE START AND END PLATFORMS
	//IMPROVE LEVEL GENERATION RULES
	//VERIFY CASES WHEN LIST ENDS ARE REACHED
	//VERIFY THE CORECTNESS FOR ALL EVENTS
	
	
	void Start(){
		active_platforms = new List<Transform>();
		inactive_platforms = new Queue<Transform>();
		all_platforms = new List<Platform>();
		//TEST THE COMPARATOR
		right_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		left_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		upper_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		lower_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		platform_graph = new Graph();

		//instantiate up to the number of allowed platforms
		for (int a=0; a<TOTAL_INSTANCIATED_PLATFORMS; a++) {
			Transform platform = (Transform)Instantiate(prefab);
			inactive_platforms.Enqueue(platform);
		}

		//place the start platform
		Vector3 start_scale = new Vector3(4, 0.36f, 1);
		Platform start_platform = new Platform(start_pos, start_scale);
		all_platforms.Add(start_platform);
		right_extremities.Add(start_platform.get_right_extremity(), 0);
		left_extremities.Add(start_platform.get_left_extremity(), 0);
		upper_extremities.Add(start_platform.get_upper_extremity(), 0);
		lower_extremities.Add(start_platform.get_lower_extremity(), 0);

		System.Random rnd = new System.Random();
		//for each remaining platform to be created
		for (int a=1; a<TOTAL_PLATFORMS; ++a){		
			bool is_overlapping = true;
		
			//randomly select an existing platform from which the new platform should be reachable
			int index = rnd.Next(all_platforms.Count);
			int placement_attempts = 0;
			
			//try up to 3 times to create and place the new platform
			Platform platform = new Platform();
			while (is_overlapping && placement_attempts < 3){
				++placement_attempts;
			
				//select a random width for the new platform
				int platform_width = rnd.Next(2, 10);
				Vector3 platform_scale = new Vector3(platform_width, 0.36f, 1);

				//select a random location for the platform within a reachable proximity of the selected platform
				int platform_x = rnd.Next((int)(all_platforms[index].get_left_extremity() - platform_horizontal_gap), (int)(all_platforms[index].get_right_extremity() + platform_horizontal_gap));
				int platform_y = rnd.Next((int)(all_platforms[index].get_lower_extremity() - platform_downwards_gap), (int)(all_platforms[index].get_upper_extremity() + platform_upwards_gap));
				Vector3 platform_pos = new Vector3(platform_x, platform_y, 1);
				
				platform = new Platform(platform_pos, platform_scale);
				float left_extremity = platform.get_left_extremity();
				float right_extremity = platform.get_right_extremity();
				float upper_buffer = platform.get_upper_extremity() + VERTICAL_BUFFER;
				float lower_buffer = platform.get_lower_extremity() - VERTICAL_BUFFER;
				
				//determine the index of the first platform fully to the right of hte new platform
				int stop_index = 0;
				foreach (KeyValuePair<float, int> extremity in left_extremities){
					if (extremity.Key >= right_extremity){
						stop_index = extremity.Value;
						break;
					}
				}

				bool is_potential_overlap = false;
				is_overlapping = false;
				
				//walk through the existing platforms in order of their right extremities
				foreach (KeyValuePair<float, int> extremity in right_extremities){
					//start checking for overlapping platforms once the first platform with its right extremity to the left of the left extremity of the new platform is reached
					if (extremity.Key >= left_extremity)
						is_potential_overlap = true;
				
					//if the stop index has been reached, do not check any more platforms
					if (extremity.Value == stop_index)
						break;
						
					//if the current platform overlaps on the x-axis with the new platform
					if (is_potential_overlap){
						//if the current platform falls within the vertical buffer of the new platform, an overlap has occured
						if (!(all_platforms[extremity.Value].get_lower_extremity() > upper_buffer  || all_platforms[extremity.Value].get_upper_extremity() < lower_buffer)){
							is_overlapping = true;
							break;
						}
					}
				}			
			}
			
			//MARK FIRST PLATFORM AS FULL AND EXLCUDE FROM FUTURE ADDITIONS?
			//if the new platform is still overlapping, do not place it - decrement the iteration number to try again from a new platform to reach from
			if (is_overlapping)
				--a;
			//otherwise, place the platform
			else{
				all_platforms.Add(platform);
				right_extremities.Add(platform.get_right_extremity(), a);
				left_extremities.Add(platform.get_left_extremity(), a);
				upper_extremities.Add(platform.get_upper_extremity(), a);
				lower_extremities.Add(platform.get_lower_extremity(), a);
			}
		}

		//construct a directed graph with edges between plaforms that are sufficiently reachable from each other
		//build_graph_edges();
		
		//check if the graph represents a level which can be traversed based on the mobility of the player
		//UPDATE THIS TO CORRECT THE GRAPH WHEN IT IS NOT VALID
		//ADD THE CORRECT INDICATION OF THE START AND END NODES
		//if (!platform_graph.isValid(0, 1))
			//print ("LEVEL CANNOT BE COMPLETED BY CURRENT PLAYER");
				
		right_extremity_cutoff = player.localPosition.x - X_RECYLE_CUTOFF;
		left_extremity_cutoff = player.localPosition.x + X_RECYLE_CUTOFF;
		upper_extremity_cutoff = player.localPosition.y - Y_RECYCLE_CUTOFF;
		lower_extremity_cutoff = player.localPosition.y + Y_RECYCLE_CUTOFF;
		float temp_pos, temp_scale;
		
		//find the smallest active right extremity
		float prev_extremity = float.MinValue;
		int stage = 1;
		for (int a = 0; a < all_platforms.Count; ++a){
			//when the first right extremity within the active x range is found
			if (stage == 1 && right_extremities.ElementAt(a).Key > right_extremity_cutoff){
				stage = 2;
				right_extremities.ElementAt(a);
				//mark the position of the next right extremity that would appear when pushing the borders left
				next_left_pos = prev_extremity;			
				//mark the extremity index
				//right_extremity_index = right_extremities.ElementAt(a).Value;
				right_extremity_index = a;
				
				//SHOULD THIS SAY DISAPPEAR?
				//mark the position of the next right extremity that would appear when pulling the borders to the right
				next_left_pos_rep = right_extremities.ElementAt(a).Key;
			}
			//continue checking platforms until the first which is also within the active y range is found
			else if (stage == 2){
				temp_pos = all_platforms[right_extremities.ElementAt(a).Value].get_position().y;
				temp_scale = all_platforms[right_extremities.ElementAt(a).Value].getScale().y;
				
				//if the platform is within the allowable y range
				if (((temp_pos + (temp_scale/2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < lower_extremity_cutoff)){
					//mark the extremity position and index
					min_x_pos = right_extremities.ElementAt(a).Key;
					min_x_index = right_extremities.ElementAt(a).Value;			
					break;
				}
			}
			
			prev_extremity = right_extremities.ElementAt(a).Key;
		}

		//find the largest active left extremity
		prev_extremity = float.MaxValue;
		stage = 1;
		for (int a = all_platforms.Count-1; a >= 0; --a){
			if (stage == 1 && left_extremities.ElementAt(a).Key < left_extremity_cutoff){
				stage = 2;
				next_right_pos = prev_extremity;
				//left_extremity_index = left_extremities.ElementAt(0).Value;
				left_extremity_index = a;
				next_right_pos_rep = left_extremities.ElementAt(a).Key;
			}
			else if (stage == 2){
				temp_pos = all_platforms[left_extremities.ElementAt(a).Value].get_position().y;
				temp_scale = all_platforms[left_extremities.ElementAt(a).Value].getScale().y;
				
				if (((temp_pos + (temp_scale/2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < lower_extremity_cutoff)){
					max_x_pos = left_extremities.ElementAt(a).Key;
					max_x_index = left_extremities.ElementAt(a).Value;
					break;
				}
			}
			
			prev_extremity = left_extremities.ElementAt(a).Key;
		}
		
		//find the smallest active upper extremity
		prev_extremity = float.MinValue;
		stage = 1;
		for (int a = 0; a < all_platforms.Count; ++a){
			if (stage == 1 && upper_extremities.ElementAt(a).Key > upper_extremity_cutoff){
				stage = 2;
				next_lower_pos = prev_extremity;
				//upper_extremity_index = upper_extremities.ElementAt(0).Value;
				upper_extremity_index = a;
				next_lower_pos_rep = upper_extremities.ElementAt(a).Key;
			}
			else if (stage == 2){
				temp_pos = all_platforms[upper_extremities.ElementAt(a).Value].get_position().x;
				temp_scale = all_platforms[upper_extremities.ElementAt(a).Value].getScale().x;
				
				if (((temp_pos + (temp_scale/2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < left_extremity_cutoff)){
					min_y_pos = upper_extremities.ElementAt(a).Key;
					min_y_index = upper_extremities.ElementAt(a).Value;
					break;
				}
			}
			
			prev_extremity = upper_extremities.ElementAt(a).Key;
		}
		
		//find the largest active lower extremity
		prev_extremity = float.MaxValue;
		stage = 1;
		for (int a = all_platforms.Count-1; a >= 0; --a){
		//if (stage==1)
				//print (lower_extremities.ElementAt(0).Key);
			if (stage == 1 && lower_extremities.ElementAt(a).Key < lower_extremity_cutoff){
				stage = 2;
				next_upper_pos = prev_extremity;
				//lower_extremity_index = lower_extremities.ElementAt(0).Value;
				lower_extremity_index = a;
				next_upper_pos_rep = lower_extremities.ElementAt(a).Key;
			}
			else if (stage == 2){
				temp_pos = all_platforms[lower_extremities.ElementAt(a).Value].get_position().x;
				temp_scale = all_platforms[lower_extremities.ElementAt(a).Value].getScale().x;
				
				if (((temp_pos + (temp_scale/2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < left_extremity_cutoff)){
					max_y_pos = lower_extremities.ElementAt(a).Key;
					max_y_index = lower_extremities.ElementAt(a).Value;
					break;
				}
			}
			
			prev_extremity = lower_extremities.ElementAt(a).Key;
		}

		//check all platforms in order of their right extremity location for allowable activation until the farthest allowable left extremity is reached
		int x_index = right_extremity_index;
		while (right_extremities.ElementAt(x_index).Value != left_extremities.ElementAt(left_extremity_index).Value){
			activate_if_in_y_range(right_extremities.ElementAt(x_index).Value);
			x_index++;
		}
		
		//check the platform with the farthest allowable left extremity for activation
		activate_if_in_y_range(left_extremities.ElementAt(left_extremity_index).Value);
	}
	
	private bool activate_if_in_y_range(int index){
		float temp_pos = all_platforms[index].get_position().y;
		float temp_scale = all_platforms[index].getScale().y;
		//if the platform is within the allowable y range then activate it
		if (((temp_pos + (temp_scale/2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < lower_extremity_cutoff)){
			activate(index);
			return true;
		}
		
		return false;
	}
	
	private bool activate_if_in_x_range(int index){
		float temp_pos = all_platforms[index].get_position().x;
		float temp_scale = all_platforms[index].getScale().x;
		//if the platform is within the allowable y range then activate it
		if (((temp_pos + (temp_scale/2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < left_extremity_cutoff)){
			activate(index);
			return true;
		}
		
		return false;
	}
	
	//take an inactive platform and activate it with the characteristics of the platform at the specified index
	private void activate(int index){
		prefab = inactive_platforms.Dequeue();
		prefab.localPosition = all_platforms[index].get_position();
		prefab.localScale = all_platforms[index].getScale();
		active_platforms.Add(prefab);
	}


	void Update(){
		right_extremity_cutoff = player.localPosition.x - X_RECYLE_CUTOFF;
		left_extremity_cutoff = player.localPosition.x + X_RECYLE_CUTOFF;
		upper_extremity_cutoff = player.localPosition.y - Y_RECYCLE_CUTOFF;
		lower_extremity_cutoff = player.localPosition.y + Y_RECYCLE_CUTOFF;
		float temp_position;
		int index;

		//if the next platform off the left border is reached via border push
		if (next_left_pos >= right_extremity_cutoff){
			right_extremity_index--;
			
			//activate the platform if it is within the valid y-range and update the minimum active x position if it is activated
			if (activate_if_in_y_range(right_extremities.ElementAt(right_extremity_index).Value)){
				min_x_pos = right_extremities.ElementAt(right_extremity_index).Key;
				min_x_index = right_extremities.ElementAt(right_extremity_index).Value;
			}
			
			//update the next platform positions
			next_left_pos_rep = next_left_pos;			
			if (right_extremity_index == 0)
				next_left_pos = float.MinValue;
			else
				next_left_pos = right_extremities.ElementAt(right_extremity_index-1).Key;
		}
		
		//if the next platform off the right border is reached via border push
		if (next_right_pos <= left_extremity_cutoff){
			left_extremity_index++;
			
			if (activate_if_in_y_range(left_extremities.ElementAt(left_extremity_index).Value)){
				max_x_pos = left_extremities.ElementAt(left_extremity_index).Key;
				max_x_index = left_extremities.ElementAt(left_extremity_index).Value;
			}
			
			next_right_pos_rep = next_right_pos;
			if (left_extremity_index == (left_extremities.Count-1))
				next_right_pos = float.MaxValue;
			else
				next_right_pos = left_extremities.ElementAt(left_extremity_index+1).Key;
		}

		//if the next platform off the upper border is reached via border push
		if (next_upper_pos <= lower_extremity_cutoff){
			lower_extremity_index++;
			if (activate_if_in_x_range(lower_extremities.ElementAt(lower_extremity_index).Value)){
				max_y_pos = lower_extremities.ElementAt(lower_extremity_index).Key;
				max_y_index = lower_extremities.ElementAt(lower_extremity_index).Value;
			}
			
			next_upper_pos_rep = next_upper_pos;
			if (lower_extremity_index == (lower_extremities.Count-1))
				next_upper_pos = float.MaxValue;
			else
				next_upper_pos = lower_extremities.ElementAt(lower_extremity_index+1).Key;
		}

		//if the next platform off the bottom border is reached via border push
		if (next_lower_pos >= upper_extremity_cutoff){
			upper_extremity_index--;
			
			if (activate_if_in_x_range(upper_extremities.ElementAt(upper_extremity_index).Value)){
				min_y_pos = upper_extremities.ElementAt(upper_extremity_index).Key;
				min_y_index = upper_extremities.ElementAt(upper_extremity_index).Value;
			}
			
			next_lower_pos_rep = next_lower_pos;
			if (upper_extremity_index == 0)
				next_lower_pos = float.MinValue;
			else
				next_lower_pos = upper_extremities.ElementAt(upper_extremity_index-1).Key;
		}
		
		
		//if the left borders are pulled such that the next platform off the left border must be updated
		if (next_left_pos_rep <= right_extremity_cutoff){
			next_left_pos = next_left_pos_rep;
			right_extremity_index++;
			if (right_extremity_index == (all_platforms.Count-1))
				next_left_pos_rep = float.MaxValue;
			else
				next_left_pos_rep = right_extremities.ElementAt(right_extremity_index).Key;
		}
		
		//if the borders are pulled such that the next platform off the right border must be updated
		if (next_right_pos_rep >= left_extremity_cutoff){
			next_right_pos = next_right_pos_rep;
			left_extremity_index--;
			if (upper_extremity_index == 0)
				next_right_pos_rep = float.MinValue;
			else
				next_right_pos_rep = left_extremities.ElementAt(left_extremity_index).Key;
		}
		
		//if the borders ares pulled such that the next platform off the upper border must be updated
		if (next_upper_pos_rep >= lower_extremity_cutoff){
			next_upper_pos = next_upper_pos_rep;
			lower_extremity_index--;
			if (lower_extremity_index == 0)
				next_upper_pos_rep = float.MinValue;
			else
				next_upper_pos_rep = lower_extremities.ElementAt(lower_extremity_index).Key;
		}
		
		//if the borders are pulled such that the next platform off the lower border must be updated
		if (next_lower_pos_rep <= upper_extremity_cutoff){
			next_lower_pos = next_lower_pos_rep;
			upper_extremity_index++;
			if (upper_extremity_index == (all_platforms.Count-1))
				next_lower_pos_rep = float.MaxValue;
			else
				next_lower_pos_rep = upper_extremities.ElementAt(upper_extremity_index).Key;
		}


		//index set to max value in order to throw an exception if the indicated platform cannot be found
		index = int.MaxValue;

		//check if a platform should be deactivated off the left border
		if (min_x_pos < right_extremity_cutoff){
			float new_pos = float.MaxValue;
			float new_y_min = float.MaxValue, newYMax = float.MinValue;
			
			//loop through all active platforms
			for (int a=0; a<active_platforms.Count; a++){
				//store the position of the right extremity current platform
				temp_position = (active_platforms[a].localPosition.x + (active_platforms[a].localScale.x / 2));
				//if the platform to deactivate has been found then store its index
				if (temp_position == min_x_pos)
					index = a;
				//update the new plaform farthest to the left border
				else if (temp_position < new_pos)
					new_pos = temp_position;
					
				//store the position of the upper extremity current platform
				temp_position = (active_platforms[a].localPosition.y + (active_platforms[a].localScale.y / 2));
				if (temp_position > min_y_pos && temp_position < new_y_min)
					new_y_min = temp_position;
				//store the position of the lower extremity current platform
				temp_position -= active_platforms[a].localScale.y;
				if (temp_position < max_y_pos && temp_position > newYMax)
					newYMax = temp_position;
			}
			
			//update the global min active
			min_x_pos = new_pos;
			
			//check if the platform to deactive was also the farthest platform for the other borders
			if ((active_platforms[index].localPosition.y + (active_platforms[index].localScale.y / 2)) == min_y_pos)
				min_y_pos = new_y_min;
			if ((active_platforms[index].localPosition.y - (active_platforms[index].localScale.y / 2)) == max_y_pos)
				max_y_pos = newYMax;

			//move the platform to be deactivated
			inactive_platforms.Enqueue(active_platforms[index]);
			active_platforms.RemoveAt(index);

			index = int.MaxValue;
		}

		//check if a platform should be deactivated off the right border
		if (max_x_pos > left_extremity_cutoff){
			float new_pos = float.MinValue;
			float new_y_min = float.MaxValue, newYMax = float.MinValue;
			
			for (int a=0; a<active_platforms.Count; a++){
				temp_position = (active_platforms[a].localPosition.x - (active_platforms[a].localScale.x / 2));
				if (temp_position == max_x_pos)
					index = a;
				else if (temp_position > new_pos)
					new_pos = temp_position;
					
				temp_position = (active_platforms[a].localPosition.y + (active_platforms[a].localScale.y / 2));
				if (temp_position > min_y_pos && temp_position < new_y_min)
					new_y_min = temp_position;
				temp_position -= active_platforms[a].localScale.y;
				if (temp_position < max_y_pos && temp_position > newYMax)
					newYMax = temp_position;
			}
			
			max_x_pos = new_pos;
			
			if ((active_platforms[index].localPosition.y + (active_platforms[index].localScale.y / 2)) == min_y_pos)
				min_y_pos = new_y_min;
			if ((active_platforms[index].localPosition.y - (active_platforms[index].localScale.y / 2)) == max_y_pos)
				max_y_pos = newYMax;
			
			inactive_platforms.Enqueue(active_platforms[index]);
			active_platforms.RemoveAt(index);

			index = int.MaxValue;
		}

		//check if a platform should be deactivated off the bottom border
		if (min_y_pos < upper_extremity_cutoff){
			float new_pos = float.MaxValue;
			float new_x_min = float.MaxValue, newXMax = float.MinValue;
			
			for (int a=0; a<active_platforms.Count; a++){
				temp_position = (active_platforms[a].localPosition.y + (active_platforms[a].localScale.y / 2));
				if (temp_position == min_y_pos)
					index = a;
				else if (temp_position < new_pos)
					new_pos = temp_position;
					
				temp_position = (active_platforms[a].localPosition.x + (active_platforms[a].localScale.x / 2));
				if (temp_position > min_x_pos && temp_position < new_x_min)
					new_x_min = temp_position;
				temp_position -= active_platforms[a].localScale.x;
				if (temp_position < max_x_pos && temp_position > newXMax)
					newXMax = temp_position;
			}
			
			min_y_pos = new_pos;
			
			if ((active_platforms[index].localPosition.x + (active_platforms[index].localScale.x / 2)) == min_x_pos)
				min_x_pos = new_x_min;
			if ((active_platforms[index].localPosition.x - (active_platforms[index].localScale.x / 2)) == max_x_pos)
				max_x_pos = newXMax;
			
			inactive_platforms.Enqueue(active_platforms[index]);
			active_platforms.RemoveAt(index);

			index = int.MaxValue;
		}

		//check if a platform should be deactivated off the top border
		if (max_y_pos > lower_extremity_cutoff){
			float new_pos = float.MinValue;
			float new_x_min = float.MaxValue, newXMax = float.MinValue;
			
			for (int a=0; a<active_platforms.Count; a++){
				temp_position = (active_platforms[a].localPosition.y - (active_platforms[a].localScale.y / 2));
				if (temp_position == max_y_pos)
					index = a;
				else if (temp_position > new_pos)
					new_pos = temp_position;
					
				temp_position = (active_platforms[a].localPosition.x + (active_platforms[a].localScale.x / 2));
				if (temp_position > min_x_pos && temp_position < new_x_min)
					new_x_min = temp_position;
				temp_position -= active_platforms[a].localScale.x;
				if (temp_position < max_x_pos && temp_position > newXMax)
					newXMax = temp_position;
			}
			
			max_y_pos = new_pos;
			
			if ((active_platforms[index].localPosition.x + (active_platforms[index].localScale.x / 2)) == min_x_pos)
				min_x_pos = new_x_min;
			if ((active_platforms[index].localPosition.x - (active_platforms[index].localScale.x / 2)) == max_x_pos)
				max_x_pos = newXMax;
			
			inactive_platforms.Enqueue(active_platforms[index]);
			active_platforms.RemoveAt(index);
		}
		
		
		//check if any extremity lists have reached their end
		if (left_extremity_index == all_platforms.Count-1)
			next_right_pos = float.MaxValue;
		if (right_extremity_index == 0)
			next_left_pos = float.MinValue;
		if (upper_extremity_index == 0)
			next_lower_pos = float.MinValue;
		if (lower_extremity_index == all_platforms.Count-1)
			next_upper_pos = float.MaxValue;

	}
	
	
	/*private void build_graph_edges(){
		float right_extremity_pos, left_extremity_pos, upper_extremity_pos, other_upper_extremity_pos;
		int index, other_index = 0, other_index_walk, left_extremity_index_lookup;
		float x_gap, y_gap;
		
		//walk through platforms in order of their right extremities
		for (int a=0; a<all_platforms.Count; a++){
			//get the right extremity platform info
			index = right_extremities[a].Value;
			right_extremity_pos = all_platforms[index].get_position().x + (all_platforms[index].getScale().x / 2);
			upper_extremity_pos = all_platforms[index].get_position().y + (all_platforms[index].getScale().y / 2);
			
			//walk through platforms in order of their left extremities starting from the last platform selected in this way
			//walk until the left extremity is to the right of the chosen right extremity
			left_extremity_index_lookup = left_extremities[other_index].Value;
			left_extremity_pos = all_platforms[left_extremity_index_lookup].get_position().x - (all_platforms[left_extremity_index_lookup].getScale().x / 2);
			while ((left_extremity_pos < right_extremity_pos) && other_index < (all_platforms.Count-1)){
				//get the left extremity platform info
				other_index++;
				left_extremity_index_lookup = left_extremities[other_index].Value;
				left_extremity_pos = all_platforms[left_extremity_index_lookup].get_position().x - (all_platforms[left_extremity_index_lookup].getScale().x / 2);
			}
			
			//walk forward from the chosen left extremity until the gap on the x axe between the current left extremity platorm and the right extremity platform becomes too great
			other_index_walk = other_index;
			do{
				//get the left extremity platform info
				left_extremity_index_lookup = left_extremities[other_index_walk].Value;
				left_extremity_pos = all_platforms[other_index_walk].get_position().x - (all_platforms[other_index_walk].getScale().x / 2);
				other_upper_extremity_pos = all_platforms[other_index_walk].get_position().y + (all_platforms[other_index_walk].getScale().y / 2);
				x_gap = left_extremity_pos - right_extremity_pos;
				
				//if the left extremity platform is above the right extremity platform
				if (other_upper_extremity_pos > upper_extremity_pos){
					y_gap = other_upper_extremity_pos - upper_extremity_pos;
					//check if the left extremity platform can be reached from the right extremity platform
					if (y_gap < platform_upwards_gap)
						//add an edge to the graph to indicate that the platform can be reached
						platform_graph.addEdge(index, new Edge(x_gap, y_gap, left_extremity_index_lookup));
					//check if the right extremity platform can be reached from the left extremity platform
					if (y_gap < platform_downwards_gap)
						//add an edge to the graph to indicate that the platform can be reached
						platform_graph.addEdge(left_extremity_index_lookup, new Edge(x_gap, y_gap, index));
				}
				//if the left extremity platform is below the right extremity platform
				else{
					y_gap = upper_extremity_pos - other_upper_extremity_pos;
					//check if the left extremity platform can be reached from the right extremity platform
					if (y_gap < platform_downwards_gap)
						//add an edge to the graph to indicate that the platform can be reached
						platform_graph.addEdge(index, new Edge(x_gap, y_gap, left_extremity_index_lookup));
					//check if the right extremity platform can be reached from the left extremity platform
					if (y_gap < platform_upwards_gap)
						//add an edge to the graph to indicate that the platform can be reached
						platform_graph.addEdge(left_extremity_index_lookup, new Edge(x_gap, y_gap, index));
				}
				
				other_index_walk++;
			} while (((left_extremity_pos - right_extremity_pos) < platform_horizontal_gap) && other_index_walk < all_platforms.Count);
		}
	}*/
	

}