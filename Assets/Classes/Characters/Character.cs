using UnityEngine;
using System.Collections.Generic;

public class Character : MonoBehaviour {

	protected string character_name;

	protected int level;
	protected int total_health;
	protected int current_health;
	protected int power;
	protected int accuracy;
	protected int defense;
	protected int agility;

	protected Dictionary<Status_condition, int> condition_resistances;

	//conditions currently being suffered (bleeding, poision, etc)
	protected List<Status_condition> status_conditions;
	
	
	
	
	public string to_string(){
		return character_name;
	}

	public int get_level(){
		return level;
	}

	public int get_total_health(){
		return total_health;
	}

	public int get_current_health(){
		return current_health;
	}

	public int get_power(){
		return power;
	}

	public int get_accuracy(){
		return accuracy;
	}

	public int get_defense(){
		return defense;
	}

	public int get_agility(){
		return agility;
	}

	public Dictionary<Status_condition, int> get_condition_resistances(){
		return condition_resistances;
	}




	public void take_damage(int base_damage){
		//PREFORM DAMAGE CALCULATION
	}

	public void apply_condition_damages(){
		//CALCULATE DAMAGE TAKEN FROM CONDITIONS BASED ON RESISTANCES
	}

	public void level_up(){
		level++;
	}

}
