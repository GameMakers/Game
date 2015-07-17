using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

	public Transform player;
	public Transform prefab;
	public Vector3 start_pos;

	private readonly int TOTAL_PLATFORMS = 3000;
	private readonly int TOTAL_INSTANCIATED_PLATFORMS = 30;
	private readonly float VERTICAL_BUFFER = 1;

	//information about the player's mobility used to determine if one platform can be reached from another
    private Character_mobility character_mobility;
    private float platform_horizontal_gap, platform_upwards_gap, platform_downwards_gap;
		
	//stores the minimally necessary information on all platforms in the entire level
    private List<Game_object> all_platforms;
	//each list stores the farthest point of all platforms for a particular extremity in sorted order - this is used to identify when they should become active
	private SortedList<float,int> right_extremities, left_extremities, upper_extremities, lower_extremities;

	
	void Start(){
        //set up feasible platform gaps sizes based on the player's mobility
        character_mobility = new Character_mobility();
        float character_speed = character_mobility.get_top_speed();
        platform_horizontal_gap = character_speed * 9;
        platform_upwards_gap = character_speed * 5;
        platform_downwards_gap = character_speed * 15;

        all_platforms = new List<Game_object>();
		right_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		left_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		upper_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());
		lower_extremities = new SortedList<float,int>(new Duplicate_key_comparer<float>());

		//place the start platform
		Vector3 start_scale = new Vector3(4, 0.36f, 1);
        Game_object start_platform = new Game_object(start_pos, start_scale);
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
            Game_object platform = new Game_object();
			while (is_overlapping && placement_attempts < 3){
				++placement_attempts;

                //determine a width range for the platform
                int platform_width_class = rnd.Next(1, 10);
                int platform_width;
                //20% chance for a short platform
                if (platform_width_class < 2) 
                    platform_width = rnd.Next(2, 10);
                //70% chance for a normal platform
                else if (platform_width_class < 9) 
                    platform_width = rnd.Next(10, 30);
                //10% chance for a long platform
                else
                    platform_width = rnd.Next(30, 50);

                //randomly select a height for the platform
                double platform_height = rnd.NextDouble() + 0.15;

                //set the platform scale
                Vector3 platform_scale = new Vector3(platform_width, (float)platform_height, 1);

				//select a random location on the x-axis for the platform within a reachable proximity of the selected platform
                int platform_x = rnd.Next((int)((all_platforms[index].get_left_extremity() - platform_horizontal_gap) - (platform_width / 2)), (int)((all_platforms[index].get_right_extremity() + platform_horizontal_gap) + (platform_width / 2)));

                int platform_y;
                //if the two platforms do not overlap on the x-axis, select any reachable location on the y-axis
                if (platform_x + (platform_width / 2) < all_platforms[index].get_left_extremity() || platform_x - (platform_width / 2) > all_platforms[index].get_right_extremity())
				    platform_y = rnd.Next((int)(all_platforms[index].get_lower_extremity() - platform_downwards_gap), (int)(all_platforms[index].get_upper_extremity() + platform_upwards_gap));
                //otherwise, ensure that the platforms do not overlap on the y-axis
                else {
                    int y_placement = rnd.Next(1, 2);
                    //50% chance to place the platform above
                    if (y_placement == 1)
                        platform_y = rnd.Next((int)(all_platforms[index].get_upper_extremity() + VERTICAL_BUFFER), (int)(all_platforms[index].get_upper_extremity() + VERTICAL_BUFFER + platform_upwards_gap));
                    //50% chance to place the platform below
                    else
                        platform_y = rnd.Next((int)(all_platforms[index].get_lower_extremity() - VERTICAL_BUFFER - platform_downwards_gap), (int)(all_platforms[index].get_lower_extremity() - VERTICAL_BUFFER));
                }

                //set the platform location
				Vector3 platform_pos = new Vector3(platform_x, platform_y, 1);

                platform = new Game_object(platform_pos, platform_scale);
				float left_extremity = platform.get_left_extremity();
				float right_extremity = platform.get_right_extremity();
				float upper_buffer = platform.get_upper_extremity() + VERTICAL_BUFFER;
				float lower_buffer = platform.get_lower_extremity() - VERTICAL_BUFFER;
				
				//determine the index of the first platform fully to the right of the new platform
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
				
        //set up an object mnanager for the platforms
        GameObject game_object_manager = GameObject.Find("Game Object Manager");
        Object_manager platform_manager = Object_manager.create_component(game_object_manager, all_platforms, player, prefab, TOTAL_PLATFORMS, TOTAL_INSTANCIATED_PLATFORMS);
	}

}