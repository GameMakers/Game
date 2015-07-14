using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class Object_manager : MonoBehaviour {

    public Transform player;
    public Transform prefab;
    //public Vector3 start_pos;

    private int x_recycle_cutoff, y_recycle_cutoff;
    private int total_objects;
    private int total_instanciated_objects;
    //private readonly float VERTICAL_BUFFER = 1;

    
    //stores the minimally necessary information on all platforms in the entire level
    private List<Game_object> all_objects;
    //each list stores the farthest point of all platforms for a particular extremity in sorted order - this is used to identify when they should become active
    private SortedList<float, int> right_extremities, left_extremities, upper_extremities, lower_extremities;
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

    private Queue<int> inactive_object_indices;
    private List<Transform> active_objects;



    public static Object_manager create_component(GameObject where, List<Game_object> objects, Transform player, Transform prefab, int total_objects = 100, int total_instanciated_objects = 10) {
        Object_manager object_manager = where.AddComponent<Object_manager>();
        object_manager.setup(objects, player, prefab, total_objects, total_instanciated_objects);
        return object_manager;
    }

    public Object_manager(List<Game_object> objects, int total_objects = 100, int total_instanciated_objects = 10){
        all_objects = objects;
        this.total_objects = total_objects;
        this.total_instanciated_objects = total_instanciated_objects;
    }

    public void setup(List<Game_object> objects, Transform player, Transform prefab, int total_objects, int total_instanciated_objects) {
        x_recycle_cutoff = 19;
        y_recycle_cutoff = 10;
        all_objects = objects;
        this.total_objects = total_objects;
        this.player = player;
        this.prefab = prefab;
        this.total_instanciated_objects = total_instanciated_objects;
    }

    void Start() {

        active_objects = new List<Transform>();
        inactive_object_indices = new Queue<int>();
        //all_objects = new List<Game_object>();
        //TEST THE COMPARATOR
        right_extremities = new SortedList<float, int>(new Duplicate_key_comparer<float>());
        left_extremities = new SortedList<float, int>(new Duplicate_key_comparer<float>());
        upper_extremities = new SortedList<float, int>(new Duplicate_key_comparer<float>());
        lower_extremities = new SortedList<float, int>(new Duplicate_key_comparer<float>());

        //populate the extremity lists
        for (int a = 0; a < total_objects; ++a){
            right_extremities.Add(all_objects[a].get_right_extremity(), a);
            left_extremities.Add(all_objects[a].get_left_extremity(), a);
            upper_extremities.Add(all_objects[a].get_upper_extremity(), a);
            lower_extremities.Add(all_objects[a].get_lower_extremity(), a);
        }

        //instantiate up to the number of allowed platforms
        for (int a = 0; a < total_instanciated_objects; ++a) {
            Transform platform = (Transform)Instantiate(prefab);
            active_objects.Add(platform);
            inactive_object_indices.Enqueue(a);
        }

        //compute the current extremity cutoffs
        right_extremity_cutoff = player.localPosition.x - x_recycle_cutoff;
        left_extremity_cutoff = player.localPosition.x + x_recycle_cutoff;
        upper_extremity_cutoff = player.localPosition.y - y_recycle_cutoff;
        lower_extremity_cutoff = player.localPosition.y + y_recycle_cutoff;
        float temp_pos, temp_scale;

        //find the smallest active right extremity
        float prev_extremity = float.MinValue;
        int stage = 1;
        for (int a = 0; a < total_objects; ++a) {
            //when the first right extremity within the active x range is found
            if (stage == 1 && right_extremities.ElementAt(a).Key > right_extremity_cutoff) {
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
            else if (stage == 2) {
                temp_pos = all_objects[right_extremities.ElementAt(a).Value].get_position().y;
                temp_scale = all_objects[right_extremities.ElementAt(a).Value].getScale().y;

                //if the platform is within the allowable y range
                if (((temp_pos + (temp_scale / 2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < lower_extremity_cutoff)) {
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
        for (int a = total_objects - 1; a >= 0; --a) {
            if (stage == 1 && left_extremities.ElementAt(a).Key < left_extremity_cutoff) {
                stage = 2;
                next_right_pos = prev_extremity;
                left_extremity_index = a;
                next_right_pos_rep = left_extremities.ElementAt(a).Key;
            }
            else if (stage == 2) {
                temp_pos = all_objects[left_extremities.ElementAt(a).Value].get_position().y;
                temp_scale = all_objects[left_extremities.ElementAt(a).Value].getScale().y;

                if (((temp_pos + (temp_scale / 2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < lower_extremity_cutoff)) {
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
        for (int a = 0; a < total_objects; ++a) {
            if (stage == 1 && upper_extremities.ElementAt(a).Key > upper_extremity_cutoff) {
                stage = 2;
                next_lower_pos = prev_extremity;
                upper_extremity_index = a;
                next_lower_pos_rep = upper_extremities.ElementAt(a).Key;
            }
            else if (stage == 2) {
                temp_pos = all_objects[upper_extremities.ElementAt(a).Value].get_position().x;
                temp_scale = all_objects[upper_extremities.ElementAt(a).Value].getScale().x;

                if (((temp_pos + (temp_scale / 2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < left_extremity_cutoff)) {
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
        for (int a = total_objects - 1; a >= 0; --a) {
            if (stage == 1 && lower_extremities.ElementAt(a).Key < lower_extremity_cutoff) {
                stage = 2;
                next_upper_pos = prev_extremity;
                lower_extremity_index = a;
                next_upper_pos_rep = lower_extremities.ElementAt(a).Key;
            }
            else if (stage == 2) {
                temp_pos = all_objects[lower_extremities.ElementAt(a).Value].get_position().x;
                temp_scale = all_objects[lower_extremities.ElementAt(a).Value].getScale().x;

                if (((temp_pos + (temp_scale / 2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < left_extremity_cutoff)) {
                    max_y_pos = lower_extremities.ElementAt(a).Key;
                    max_y_index = a;
                    break;
                }
            }

            prev_extremity = lower_extremities.ElementAt(a).Key;
        }

        //check all platforms in order of their right extremity location for allowable activation until the farthest allowable left extremity is reached
        int x_index = right_extremity_index;
        while (right_extremities.ElementAt(x_index).Value != left_extremities.ElementAt(left_extremity_index).Value) {
            //print("checking");
            activate_if_in_y_range(right_extremities.ElementAt(x_index).Value);
            x_index++;
        }

        //check the platform with the farthest allowable left extremity for activation
        activate_if_in_y_range(left_extremities.ElementAt(left_extremity_index).Value);
    }

    private bool activate_if_in_y_range(int index) {
        float temp_pos = all_objects[index].get_position().y;
        float temp_scale = all_objects[index].getScale().y;
        //if the platform is within the allowable y range then activate it
        if (((temp_pos + (temp_scale / 2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < lower_extremity_cutoff)) {
            activate(index);
            return true;
        }

        return false;
    }

    private bool activate_if_in_x_range(int index) {
        float temp_pos = all_objects[index].get_position().x;
        float temp_scale = all_objects[index].getScale().x;
        //if the platform is within the allowable x range then activate it
        if (((temp_pos + (temp_scale / 2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < left_extremity_cutoff)) {
            activate(index);
            return true;
        }

        return false;
    }

    private bool is_in_y_range(int index) {
        float temp_pos = all_objects[index].get_position().y;
        float temp_scale = all_objects[index].getScale().y;
        if (((temp_pos + (temp_scale / 2)) > upper_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < lower_extremity_cutoff))
            return true;

        return false;
    }

    private bool is_in_x_range(int index) {
        float temp_pos = all_objects[index].get_position().x;
        float temp_scale = all_objects[index].getScale().x;
        if (((temp_pos + (temp_scale / 2)) > right_extremity_cutoff) && ((temp_pos - (temp_scale / 2)) < left_extremity_cutoff))
            return true;

        return false;
    }

    //take an inactive platform and give it the characteristics of the platform at the specified index
    private void activate(int index) {
        if (inactive_object_indices.Count == 0)
            Debug.LogError("We've run out of instaciated instances. Too many objects on-screen!");
        else{


            //print("Inactive: ");
            //foreach (int i in inactive_object_indices)
                //print(i+", ");

            int active_index = inactive_object_indices.Dequeue();

            /*if (Math.Abs(player.localPosition.x - active_objects[active_index].localPosition.x) < 18 && Math.Abs(player.localPosition.y - active_objects[active_index].localPosition.y) < 9) { 
                print("Deactivating: " + active_objects[active_index].localPosition.x + ", " + active_objects[active_index].localPosition.y);
                print("x: " + player.localPosition.x);
                print("y: " + player.localPosition.y);

                print("Activating: " + active_index);
            }*/

            all_objects[index].set_index(active_index);
            active_objects[active_index].localPosition = all_objects[index].get_position();
            active_objects[active_index].localScale = all_objects[index].getScale();
        }
    }


    void Update() {
        //print("x: " + player.localPosition.x);
        //print("y: " + player.localPosition.y);
        right_extremity_cutoff = player.localPosition.x - x_recycle_cutoff;
        left_extremity_cutoff = player.localPosition.x + x_recycle_cutoff;
        upper_extremity_cutoff = player.localPosition.y - y_recycle_cutoff;
        lower_extremity_cutoff = player.localPosition.y + y_recycle_cutoff;

        //if the next platform off the left border is reached via border push
        if (next_left_pos >= right_extremity_cutoff) {
            right_extremity_index--;

            //activate the platform if it is within the valid y-range and update the minimum active x position if it is activated
            if (activate_if_in_y_range(right_extremities.ElementAt(right_extremity_index).Value)) {
                min_x_pos = right_extremities.ElementAt(right_extremity_index).Key;
                min_x_index = right_extremity_index;
            }

            //update the next platform positions
            next_left_pos_rep = next_left_pos;
            if (right_extremity_index == 0)
                next_left_pos = float.MinValue;
            else
                next_left_pos = right_extremities.ElementAt(right_extremity_index - 1).Key;
        }

        //if the next platform off the right border is reached via border push
        if (next_right_pos <= left_extremity_cutoff) {
            left_extremity_index++;

            if (activate_if_in_y_range(left_extremities.ElementAt(left_extremity_index).Value)) {
                max_x_pos = left_extremities.ElementAt(left_extremity_index).Key;
                max_x_index = left_extremity_index;
            }

            next_right_pos_rep = next_right_pos;
            if (left_extremity_index == (total_objects - 1))
                next_right_pos = float.MaxValue;
            else
                next_right_pos = left_extremities.ElementAt(left_extremity_index + 1).Key;
        }

        //if the next platform off the upper border is reached via border push
        if (next_upper_pos <= lower_extremity_cutoff) {
            lower_extremity_index++;
            if (activate_if_in_x_range(lower_extremities.ElementAt(lower_extremity_index).Value)) {
                max_y_pos = lower_extremities.ElementAt(lower_extremity_index).Key;
                max_y_index = lower_extremity_index;
            }

            next_upper_pos_rep = next_upper_pos;
            if (lower_extremity_index == (total_objects - 1))
                next_upper_pos = float.MaxValue;
            else
                next_upper_pos = lower_extremities.ElementAt(lower_extremity_index + 1).Key;
        }

        //if the next platform off the bottom border is reached via border push
        if (next_lower_pos >= upper_extremity_cutoff) {
            upper_extremity_index--;

            if (activate_if_in_x_range(upper_extremities.ElementAt(upper_extremity_index).Value)) {
                min_y_pos = upper_extremities.ElementAt(upper_extremity_index).Key;
                min_y_index = upper_extremity_index;
            }

            next_lower_pos_rep = next_lower_pos;
            if (upper_extremity_index == 0)
                next_lower_pos = float.MinValue;
            else
                next_lower_pos = upper_extremities.ElementAt(upper_extremity_index - 1).Key;
        }


        //if the left borders are pulled such that the next platform off the left border must be updated
        if (next_left_pos_rep < right_extremity_cutoff) {
            next_left_pos = next_left_pos_rep;
            right_extremity_index++;
            if (right_extremity_index == (total_objects - 1))
                next_left_pos_rep = float.MaxValue;
            else
                next_left_pos_rep = right_extremities.ElementAt(right_extremity_index).Key;
        }

        //if the borders are pulled such that the next platform off the right border must be updated
        if (next_right_pos_rep > left_extremity_cutoff) {
            next_right_pos = next_right_pos_rep;
            left_extremity_index--;
            if (left_extremity_index == 0)
                next_right_pos_rep = float.MinValue;
            else
                next_right_pos_rep = left_extremities.ElementAt(left_extremity_index).Key;
        }

        //if the borders ares pulled such that the next platform off the upper border must be updated
        if (next_upper_pos_rep > lower_extremity_cutoff) {
            next_upper_pos = next_upper_pos_rep;
            lower_extremity_index--;
            if (lower_extremity_index == 0)
                next_upper_pos_rep = float.MinValue;
            else
                next_upper_pos_rep = lower_extremities.ElementAt(lower_extremity_index).Key;
        }

        //if the borders are pulled such that the next platform off the lower border must be updated
        if (next_lower_pos_rep < upper_extremity_cutoff) {
            next_lower_pos = next_lower_pos_rep;
            upper_extremity_index++;
            if (upper_extremity_index == (total_objects - 1))
                next_lower_pos_rep = float.MaxValue;
            else
                next_lower_pos_rep = upper_extremities.ElementAt(upper_extremity_index).Key;
        }

        //check if a platform should be deactivated off the left border
        if (min_x_pos < right_extremity_cutoff) {
            int index = right_extremities.ElementAt(min_x_index).Value;

            if (Math.Abs(player.localPosition.x - min_x_pos) < 18) {
                print("Deactivating at left " + min_x_pos);
                print("x: " + player.localPosition.x);
                print("y: " + player.localPosition.y);
            }

            //deactivate the platform
            inactive_object_indices.Enqueue(all_objects[index].get_index());

            //find the next active platform farthest to the left
            for (int a = min_x_index + 1; a < total_objects; ++a) {
                if (is_in_y_range(right_extremities.ElementAt(a).Value)) {
                    min_x_pos = right_extremities.ElementAt(a).Key;
                    min_x_index = a;
                    break;
                }
            }

            //if the deactivated platform was also the lowest active platform
            if (index == upper_extremities.ElementAt(min_y_index).Value) {
                //find the next lowest active platform to update the references
                for (int a = min_y_index + 1; a < total_objects; ++a) {
                    if (is_in_x_range(upper_extremities.ElementAt(a).Value)) {
                        min_y_pos = upper_extremities.ElementAt(a).Key;
                        min_y_index = a;
                        break;
                    }
                }
            }

            //if the deactivated platform was also the highest active platform
            if (index == lower_extremities.ElementAt(max_y_index).Value) {
                //find the next highest active platform to update the references
                for (int a = max_y_index - 1; a >= 0; --a) {
                    if (is_in_x_range(lower_extremities.ElementAt(a).Value)) {
                        max_y_pos = lower_extremities.ElementAt(a).Key;
                        max_y_index = a;
                        break;
                    }
                }
            }
        }

        //check if a platform should be deactivated off the right border
        if (max_x_pos > left_extremity_cutoff) {
            int index = left_extremities.ElementAt(max_x_index).Value;

            if (Math.Abs(player.localPosition.x - max_x_pos) < 18) {
                print("Deactivating at right " + max_x_pos);
                print("x: " + player.localPosition.x);
                print("y: " + player.localPosition.y);
            }

            inactive_object_indices.Enqueue(all_objects[index].get_index());

            for (int a = max_x_index - 1; a >= 0; --a) {
                if (is_in_y_range(left_extremities.ElementAt(a).Value)) {
                    max_x_pos = left_extremities.ElementAt(a).Key;
                    max_x_index = a;
                    break;
                }
            }

            if (index == upper_extremities.ElementAt(min_y_index).Value) {
                for (int a = min_y_index + 1; a < total_objects; ++a) {
                    if (is_in_x_range(upper_extremities.ElementAt(a).Value)) {
                        min_y_pos = upper_extremities.ElementAt(a).Key;
                        min_y_index = a;
                        break;
                    }
                }
            }

            if (index == lower_extremities.ElementAt(max_y_index).Value) {
                for (int a = max_y_index - 1; a >= 0; --a) {
                    if (is_in_x_range(lower_extremities.ElementAt(a).Value)) {
                        max_y_pos = lower_extremities.ElementAt(a).Key;
                        max_y_index = a;
                        break;
                    }
                }
            }
        }

        //check if a platform should be deactivated off the bottom border
        if (min_y_pos < upper_extremity_cutoff) {
            int index = upper_extremities.ElementAt(min_y_index).Value;

            if (Math.Abs(player.localPosition.y - min_y_pos) < 9) {
                print("Deactivating at bottom " + min_y_pos);
                print("x: " + player.localPosition.x);
                print("y: " + player.localPosition.y);
            }

            inactive_object_indices.Enqueue(all_objects[index].get_index());

            for (int a = min_y_index + 1; a < total_objects; ++a) {
                if (is_in_x_range(upper_extremities.ElementAt(a).Value)) {
                    min_y_pos = upper_extremities.ElementAt(a).Key;
                    min_y_index = a;
                    break;
                }
            }

            if (index == right_extremities.ElementAt(min_x_index).Value) {
                for (int a = min_x_index + 1; a < total_objects; ++a) {
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

        //check if a platform should be deactivated off the top border
        if (max_y_pos > lower_extremity_cutoff) {
            int index = lower_extremities.ElementAt(max_y_index).Value;

            if (Math.Abs(player.localPosition.y - max_y_pos) < 9) {
                print("Deactivating at top " + max_y_pos);
                print("x: " + player.localPosition.x);
                print("y: " + player.localPosition.y);
            }

            inactive_object_indices.Enqueue(all_objects[index].get_index());

            for (int a = max_y_index - 1; a >= 0; --a) {
                if (is_in_x_range(lower_extremities.ElementAt(a).Value)) {
                    max_y_pos = lower_extremities.ElementAt(a).Key;
                    max_y_index = a;
                    break;
                }
            }

            if (index == right_extremities.ElementAt(min_x_index).Value) {
                for (int a = min_x_index + 1; a < total_objects; ++a) {
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
        if (left_extremity_index == all_objects.Count - 1)
            next_right_pos = float.MaxValue;
        if (right_extremity_index == 0)
            next_left_pos = float.MinValue;
        if (upper_extremity_index == 0)
            next_lower_pos = float.MinValue;
        if (lower_extremity_index == all_objects.Count - 1)
            next_upper_pos = float.MaxValue;

    }

}