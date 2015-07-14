using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

	public Transform player;
	public Transform prefab;
	public Vector3 start_pos;

	private readonly int TOTAL_PLATFORMS = 3000;
	/*private readonly int X_RECYLE_CUTOFF = 18, Y_RECYCLE_CUTOFF = 9;
	private readonly int TOTAL_INSTANCIATED_PLATFORMS = 70;*/
	private readonly float VERTICAL_BUFFER = 1;

	//information about the player's mobility used to determine if one platform can be reached from another
    private Character_mobility character_mobility;
    private float platform_horizontal_gap, platform_upwards_gap, platform_downwards_gap;
	
	
	//stores the minimally necessary information on all platforms in the entire level
    private List<Game_object> all_platforms;
	//each list stores the farthest point of all platforms for a particular extremity in sorted order - this is used to identify when they should become active
	private SortedList<float,int> right_extremities, left_extremities, upper_extremities, lower_extremities;
	//used to keep track of index locations in the extremity lists
	/*private int right_extremity_index, left_extremity_index, upper_extremity_index, lower_extremity_index;
	//used to keep track of the farthest active platform with respect to each border
	private float min_x_pos, max_x_pos, min_y_pos, max_y_pos;
	private int min_x_index, max_x_index, min_y_index, max_y_index;
	//used to keep track of the nearest inactive platform in each direction
	private float next_left_pos, next_right_pos, next_upper_pos, next_lower_pos;
	//used to keep track of replacement platforms when the borders are pulled by player movement
	private float next_left_pos_rep, next_right_pos_rep, next_upper_pos_rep, next_lower_pos_rep;
	//used to keep track of the cutoffs for the player's current position
	float right_extremity_cutoff, left_extremity_cutoff, upper_extremity_cutoff, lower_extremity_cutoff;
		
	private Queue<int> inactive_platform_indices;
	private List<Transform> active_platforms;*/
	
	
	//VERIFY THAT GRAPH CAN BE REMOVED
	//ADD CODE TO MARK THE START AND END PLATFORMS
	//IMPROVE LEVEL GENERATION RULES
	//VERIFY CASES WHEN LIST ENDS ARE REACHED
	
	
	void Start(){
        //set up feasible platform gaps sizes based on the player's mobility
        character_mobility = new Character_mobility();
        float character_speed = character_mobility.get_top_speed();
        platform_horizontal_gap = character_speed * 6;
        platform_upwards_gap = character_speed * 4;
        platform_downwards_gap = character_speed * 10;

		/*active_platforms = new List<Transform>();
		inactive_platform_indices = new Queue<int>();*/
        all_platforms = new List<Game_object>();
		//TEST THE COMPARATOR
		right_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		left_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		upper_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		lower_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());

		//instantiate up to the number of allowed platforms
		/*for (int a = 0; a < TOTAL_INSTANCIATED_PLATFORMS; ++a) {
			Transform platform = (Transform)Instantiate(prefab);
            active_platforms.Add(platform);
            inactive_platform_indices.Enqueue(a);
		}*/

		//place the start platform
		Vector3 start_scale = new Vector3(4, 0.36f, 1);
        Game_object start_platform = new Game_object(start_pos, start_scale);
		all_platforms.Add(start_platform);
		/*right_extremities.Add(start_platform.get_right_extremity(), 0);
		left_extremities.Add(start_platform.get_left_extremity(), 0);
		upper_extremities.Add(start_platform.get_upper_extremity(), 0);
		lower_extremities.Add(start_platform.get_lower_extremity(), 0);*/

		System.Random rnd = new System.Random();
		//for each remaining platform to be created
		for (int a=1; a<TOTAL_PLATFORMS; ++a){		
			bool is_overlapping = true;
		
			//randomly select an existing platform from which the new platform should be reachable
			int index = rnd.Next(all_platforms.Count);
			int placement_attempts = 0;
			
			//try up to 3 times to create and place the new platform
            Game_object platform = new Game_object();
			while (is_overlapping && placement_attempts < 3){
				++placement_attempts;
			
				//select a random width for the new platform
				int platform_width = rnd.Next(2, 10);
				Vector3 platform_scale = new Vector3(platform_width, 0.36f, 1);

				//select a random location for the platform within a reachable proximity of the selected platform
				int platform_x = rnd.Next((int)(all_platforms[index].get_left_extremity() - platform_horizontal_gap), (int)(all_platforms[index].get_right_extremity() + platform_horizontal_gap));
				int platform_y = rnd.Next((int)(all_platforms[index].get_lower_extremity() - platform_downwards_gap), (int)(all_platforms[index].get_upper_extremity() + platform_upwards_gap));
				Vector3 platform_pos = new Vector3(platform_x, platform_y, 1);

                platform = new Game_object(platform_pos, platform_scale);
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
				
        //compute the current extremity cutoffs
		/*right_extremity_cutoff = player.localPosition.x - X_RECYLE_CUTOFF;
		left_extremity_cutoff = player.localPosition.x + X_RECYLE_CUTOFF;
		upper_extremity_cutoff = player.localPosition.y - Y_RECYCLE_CUTOFF;
		lower_extremity_cutoff = player.localPosition.y + Y_RECYCLE_CUTOFF;
		float temp_pos, temp_scale;
		
		//find the smallest active right extremity
		float prev_extremity = float.MinValue;
		int stage = 1;
        for (int a = 0; a < TOTAL_PLATFORMS; ++a){
			//when the first right extremity within the active x range is found
			if (stage == 1 && right_extremities.ElementAt(a).Key > right_extremity_cutoff){
				stage = 2;
				right_extremities.ElementAt(a);
				//mark the position of the next right extremity that would appear when pushing the borders left
				next_left_pos = prev_extremity;			
				//mark the extremity index
				right_extremity_index = a;
				
				//mark the position of the next right extremity that would disappear when pulling the borders to the right
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
					min_x_index = a;			
					break;
				}
			}
			
			prev_extremity = right_extremities.ElementAt(a).Key;
		}

		//find the largest active left extremity
		prev_extremity = float.MaxValue;
		stage = 1;
        for (int a = TOTAL_PLATFORMS - 1; a >= 0; --a){
			if (stage == 1 && left_extremities.ElementAt(a).Key < left_extremity_cutoff){
				stage = 2;
				next_right_pos = prev_extremity;
				left_extremity_index = a;
				next_right_pos_rep = left_extremities.ElementAt(a).Key;
			}
			else if (stage == 2){
				temp_pos = all_platforms[left_extremities.ElementAt(a).Value].get_position().y;
				temp_scale = all_platforms[left_extremities.ElementAt(a).Value].getScale().y;
				
				if (((temp_pos + (temp_scale/2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < lower_extremity_cutoff)){
					max_x_pos = left_extremities.ElementAt(a).Key;
					max_x_index = a;
					break;
				}
			}
			
			prev_extremity = left_extremities.ElementAt(a).Key;
		}
		
		//find the smallest active upper extremity
		prev_extremity = float.MinValue;
		stage = 1;
        for (int a = 0; a < TOTAL_PLATFORMS; ++a){
			if (stage == 1 && upper_extremities.ElementAt(a).Key > upper_extremity_cutoff){
				stage = 2;
				next_lower_pos = prev_extremity;
				upper_extremity_index = a;
				next_lower_pos_rep = upper_extremities.ElementAt(a).Key;
			}
			else if (stage == 2){
				temp_pos = all_platforms[upper_extremities.ElementAt(a).Value].get_position().x;
				temp_scale = all_platforms[upper_extremities.ElementAt(a).Value].getScale().x;
				
				if (((temp_pos + (temp_scale/2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < left_extremity_cutoff)){
					min_y_pos = upper_extremities.ElementAt(a).Key;
					min_y_index = a;
					break;
				}
			}
			
			prev_extremity = upper_extremities.ElementAt(a).Key;
		}
		
		//find the largest active lower extremity
		prev_extremity = float.MaxValue;
		stage = 1;
        for (int a = TOTAL_PLATFORMS - 1; a >= 0; --a){
			if (stage == 1 && lower_extremities.ElementAt(a).Key < lower_extremity_cutoff){
				stage = 2;
				next_upper_pos = prev_extremity;
				lower_extremity_index = a;
				next_upper_pos_rep = lower_extremities.ElementAt(a).Key;
			}
			else if (stage == 2){
				temp_pos = all_platforms[lower_extremities.ElementAt(a).Value].get_position().x;
				temp_scale = all_platforms[lower_extremities.ElementAt(a).Value].getScale().x;
				
				if (((temp_pos + (temp_scale/2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < left_extremity_cutoff)){
					max_y_pos = lower_extremities.ElementAt(a).Key;
					max_y_index = a;
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
		activate_if_in_y_range(left_extremities.ElementAt(left_extremity_index).Value);*/

        GameObject game_object_manager = GameObject.Find("Game Object Manager");
        Object_manager platform_manager = Object_manager.create_component(game_object_manager, all_platforms, player, prefab, TOTAL_PLATFORMS, 30);
	}
	
	/*private bool activate_if_in_y_range(int index){
		float temp_pos = all_platforms[index].get_position().y;
		float temp_scale = all_platforms[index].getScale().y;
		//if the platform is within the allowable y range then activate it
		if (((temp_pos + (temp_scale/2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < lower_extremity_cutoff)){
			activate(index);
			return true;
		}
		
		return false;
	}*/
	
	/*private bool activate_if_in_x_range(int index){
		float temp_pos = all_platforms[index].get_position().x;
		float temp_scale = all_platforms[index].getScale().x;
		//if the platform is within the allowable x range then activate it
		if (((temp_pos + (temp_scale/2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale/2)) < left_extremity_cutoff)){
			activate(index);
			return true;
		}
		
		return false;
	}*/

    /*private bool is_in_y_range(int index) {
        float temp_pos = all_platforms[index].get_position().y;
        float temp_scale = all_platforms[index].getScale().y;
        if (((temp_pos + (temp_scale / 2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < lower_extremity_cutoff))
            return true;

        return false;
    }*/

    /*private bool is_in_x_range(int index) {
        float temp_pos = all_platforms[index].get_position().x;
        float temp_scale = all_platforms[index].getScale().x;
        if (((temp_pos + (temp_scale / 2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < left_extremity_cutoff))
            return true;

        return false;
    }*/
	
	//take an inactive platform and give it the characteristics of the platform at the specified index
	/*private void activate(int index){
        int active_index = inactive_platform_indices.Dequeue();
        all_platforms[index].set_index(active_index);
		active_platforms[active_index].localPosition = all_platforms[index].get_position();
        active_platforms[active_index].localScale = all_platforms[index].getScale();
	}*/


	/*void Update(){
		right_extremity_cutoff = player.localPosition.x - X_RECYLE_CUTOFF;
		left_extremity_cutoff = player.localPosition.x + X_RECYLE_CUTOFF;
		upper_extremity_cutoff = player.localPosition.y - Y_RECYCLE_CUTOFF;
		lower_extremity_cutoff = player.localPosition.y + Y_RECYCLE_CUTOFF;

		//if the next platform off the left border is reached via border push
		if (next_left_pos >= right_extremity_cutoff){
			right_extremity_index--;
			
			//activate the platform if it is within the valid y-range and update the minimum active x position if it is activated
			if (activate_if_in_y_range(right_extremities.ElementAt(right_extremity_index).Value)){
				min_x_pos = right_extremities.ElementAt(right_extremity_index).Key;
				min_x_index = right_extremity_index;
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
				max_x_index = left_extremity_index;
			}
			
			next_right_pos_rep = next_right_pos;
			if (left_extremity_index == (TOTAL_PLATFORMS-1))
				next_right_pos = float.MaxValue;
			else
				next_right_pos = left_extremities.ElementAt(left_extremity_index+1).Key;
		}

		//if the next platform off the upper border is reached via border push
		if (next_upper_pos <= lower_extremity_cutoff){
			lower_extremity_index++;
			if (activate_if_in_x_range(lower_extremities.ElementAt(lower_extremity_index).Value)){
				max_y_pos = lower_extremities.ElementAt(lower_extremity_index).Key;
				max_y_index = lower_extremity_index;
			}
			
			next_upper_pos_rep = next_upper_pos;
			if (lower_extremity_index == (TOTAL_PLATFORMS-1))
				next_upper_pos = float.MaxValue;
			else
				next_upper_pos = lower_extremities.ElementAt(lower_extremity_index+1).Key;
		}

		//if the next platform off the bottom border is reached via border push
		if (next_lower_pos >= upper_extremity_cutoff){
			upper_extremity_index--;
			
			if (activate_if_in_x_range(upper_extremities.ElementAt(upper_extremity_index).Value)){
				min_y_pos = upper_extremities.ElementAt(upper_extremity_index).Key;
				min_y_index = upper_extremity_index;
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
			if (right_extremity_index == (TOTAL_PLATFORMS-1))
				next_left_pos_rep = float.MaxValue;
			else
				next_left_pos_rep = right_extremities.ElementAt(right_extremity_index).Key;
		}
		
		//if the borders are pulled such that the next platform off the right border must be updated
		if (next_right_pos_rep >= left_extremity_cutoff){
			next_right_pos = next_right_pos_rep;
			left_extremity_index--;
            if (left_extremity_index == 0)
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
			if (upper_extremity_index == (TOTAL_PLATFORMS-1))
				next_lower_pos_rep = float.MaxValue;
			else
				next_lower_pos_rep = upper_extremities.ElementAt(upper_extremity_index).Key;
		}

		//check if a platform should be deactivated off the left border
		if (min_x_pos < right_extremity_cutoff){
            int index = right_extremities.ElementAt(min_x_index).Value;

            //deactivate the platform
            inactive_platform_indices.Enqueue(all_platforms[index].get_index());

            //find the next active platform farthest to the left
            for (int a = min_x_index + 1; a < TOTAL_PLATFORMS; ++a){
                if (is_in_y_range(right_extremities.ElementAt(a).Value)) {
                    min_x_pos = right_extremities.ElementAt(a).Key;
                    min_x_index = a;
                    break;
                }
            }

            //if the deactivated platform was also the lowest active platform
            if (index == upper_extremities.ElementAt(min_y_index).Value){
                //find the next lowest active platform to update the references
                for (int a = min_y_index+1; a < TOTAL_PLATFORMS; ++a) {
                    if (is_in_x_range(upper_extremities.ElementAt(a).Value)) {
                        min_y_pos = upper_extremities.ElementAt(a).Key;
                        min_y_index = a;
                        break;
                    }
                }
            }

            //if the deactivated platform was also the highest active platform
            if (index == lower_extremities.ElementAt(max_y_index).Value){
                //find the next highest active platform to update the references
                for (int a = max_y_index-1; a >= 0; --a) {
                    if (is_in_x_range(lower_extremities.ElementAt(a).Value)){
                        max_y_pos = lower_extremities.ElementAt(a).Key;
                        max_y_index = a;
                        break;
                    }
                }
            }
		}

		//check if a platform should be deactivated off the right border
		if (max_x_pos > left_extremity_cutoff){
            int index = left_extremities.ElementAt(max_x_index).Value;

            inactive_platform_indices.Enqueue(all_platforms[index].get_index());

            for (int a = max_x_index-1; a >= 0; --a) {
                if (is_in_y_range(left_extremities.ElementAt(a).Value)) {
                    max_x_pos = left_extremities.ElementAt(a).Key;
                    max_x_index = a;
                    break;
                }
            }

            if (index == upper_extremities.ElementAt(min_y_index).Value) {
                for (int a = min_y_index+1; a < TOTAL_PLATFORMS; ++a) {
                    if (is_in_x_range(upper_extremities.ElementAt(a).Value)) {
                        min_y_pos = upper_extremities.ElementAt(a).Key;
                        min_y_index = a;
                        break;
                    }
                }
            }

            if (index == lower_extremities.ElementAt(max_y_index).Value) {
                for (int a = max_y_index-1; a >= 0; --a) {
                    if (is_in_x_range(lower_extremities.ElementAt(a).Value)) {
                        max_y_pos = lower_extremities.ElementAt(a).Key;
                        max_y_index = a;
                        break;
                    }
                }
            }
		}

		//check if a platform should be deactivated off the bottom border
		if (min_y_pos < upper_extremity_cutoff){
            int index = upper_extremities.ElementAt(min_y_index).Value;

            inactive_platform_indices.Enqueue(all_platforms[index].get_index());

            for (int a = min_y_index+1; a < TOTAL_PLATFORMS; ++a){
                if (is_in_x_range(upper_extremities.ElementAt(a).Value)) {
                    min_y_pos = upper_extremities.ElementAt(a).Key;
                    min_y_index = a;
                    break;
                }
            }

            if (index == right_extremities.ElementAt(min_x_index).Value){
                for (int a = min_x_index+1; a < TOTAL_PLATFORMS; ++a) {
                    if (is_in_y_range(right_extremities.ElementAt(a).Value)){
                        min_x_pos = right_extremities.ElementAt(a).Key;
                        min_x_index = a;
                        break;
                    }
                }
            }

            if (index == left_extremities.ElementAt(max_x_index).Value){
                for (int a = max_x_index-1; a >= 0; --a){
                    if (is_in_y_range(right_extremities.ElementAt(a).Value)){
                        max_x_pos = left_extremities.ElementAt(a).Key;
                        max_x_index = a;
                        break;
                    }
                }
            }
		}

		//check if a platform should be deactivated off the top border
		if (max_y_pos > lower_extremity_cutoff){
            int index = lower_extremities.ElementAt(max_y_index).Value;

            inactive_platform_indices.Enqueue(all_platforms[index].get_index());

            for (int a = max_y_index-1; a >= 0; --a) {
                if (is_in_x_range(lower_extremities.ElementAt(a).Value)) {
                    max_y_pos = lower_extremities.ElementAt(a).Key;
                    max_y_index = a;
                    break;
                }
            }

            if (index == right_extremities.ElementAt(min_x_index).Value) {
                for (int a = min_x_index + 1; a < TOTAL_PLATFORMS; ++a) {
                    if (is_in_y_range(right_extremities.ElementAt(a).Value)) {
                        min_x_pos = right_extremities.ElementAt(a).Key;
                        min_x_index = a;
                        break;
                    }
                }
            }

            if (index == left_extremities.ElementAt(max_x_index).Value) {
                for (int a = max_x_index - 1; a >= 0; --a) {
                    if (is_in_y_range(right_extremities.ElementAt(a).Value)) {
                        max_x_pos = left_extremities.ElementAt(a).Key;
                        max_x_index = a;
                        break;
                    }
                }
            }
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

	}*/	

}