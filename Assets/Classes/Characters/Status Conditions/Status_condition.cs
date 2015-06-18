using UnityEngine;
using System.Collections;

public class Status_condition : MonoBehaviour {

	protected string condition_type;

	protected int total_duration;
	protected int remaining_duration;
	protected int damage_per_second;
	//percentages by which the stats of the afflicted character are reduced while the condition is in effect
	protected int accuracy_reduction;
	protected int damage_reduction;
	protected int agility_reduction;
	protected int defense_reduction;
	protected bool reverses_controls;



	public string to_string(){
		return condition_type;
	}

	public int get_total_duration(){
		return total_duration;
	}

	public int get_remaining_duration(){
		return remaining_duration;
	}

	public int get_damage_per_second(){
		return damage_per_second;
	}

	public int get_accuracy_reduction(){
		return accuracy_reduction;
	}

	public int get_damage_reduction(){
		return damage_reduction;
	}

	public int get_agility_reduction(){
		return agility_reduction;
	}

	public int get_defense_reduction(){
		return defense_reduction;
	}

	public bool are_controls_reversed(){
		return reverses_controls;
	}
	
}
